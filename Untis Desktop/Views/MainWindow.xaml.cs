using Data.Profiles;
using Data.Timetable;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using UntisDesktop.Extensions;
using UntisDesktop.Localization;
using UntisDesktop.UserControls;
using UntisDesktop.ViewModels;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Exceptions;
using WebUntisAPI.Client.Models;
using WebUntisAPI.Client.Models.Messages;
using WebUntis = WebUntisAPI.Client.Models;
using Data;
using Licenses;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Data;
using UntisDesktop.Converter;

namespace UntisDesktop.Views;

public partial class MainWindow : Window
{
    public static DateTime SelectedWeek
    {
        get => s_SelectedWeek;
        set
        {
            int offset = DayOfWeek.Sunday - value.DayOfWeek;
            s_SelectedWeek = value.AddDays(offset);

            ProfileFile activeProfile = ProfileCollection.GetActiveProfile();
            activeProfile.Options.SelectedWeek = s_SelectedWeek;
            activeProfile.Update();
        }
    }
    private static DateTime s_SelectedWeek;

    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    public MainWindow(bool isOffline)
    {
        ProfileOptions options = ProfileCollection.GetActiveProfile().Options;
        s_SelectedWeek = options.SelectedWeek;

        InitializeComponent();
        ViewModel.IsOffline = isOffline;

        ViewModel.PropertyChanged += async (_, e) =>
        {
            // Error box update
            if (e.PropertyName == nameof(MainWindowViewModel.ErrorBoxContent))
            {
                if (ViewModel.ErrorBoxContent != string.Empty)
                {
                    Storyboard popupAnimation = (Storyboard)ErrorBox.FindResource("PopUpAnimation");
                    popupAnimation.AutoReverse = false;
                    popupAnimation.Begin();
                }
            }

            // Timetable reload
            else if (e.PropertyName == nameof(MainWindowViewModel.SelectedWeek))
            {
                foreach (TimegridDay day in Timegrid.Children.OfType<TimegridDay>())
                    day.ViewModel.Update();
                await UpdateTimetableAsync();

                return;
            }

            // Mails reload animation
            else if (e.PropertyName == nameof(MainWindowViewModel.IsMailsLoading))
                ToggleMailsLoadingAnimation(ViewModel.IsMailsLoading);

            // Message inbox
            else if (e.PropertyName == nameof(MainWindowViewModel.MsgInbox))
                UpdateMailInbox();

            // Confirmation message inbox
            else if (e.PropertyName == nameof(MainWindowViewModel.ConfirmationMsgInbox))
                UpdateConfirmationMailInbox();

            // Sent messages
            else if (e.PropertyName == nameof(MainWindowViewModel.SentMsg))
                UpdateMailsSent();

            // Draft messages
            else if (e.PropertyName == nameof(MainWindowViewModel.DraftMessages))
                UpdateMailDrafts();
        };

        SetupTimegrid();
        _ = UpdateTimetableAsync();

        RenderLicenses();

        DisplayLastMenuItem(options.SelectedMenuItem);
        DisplayLastOptionsMenuItem(options.SelectedOptionsMenuItems);
    }

    [GeneratedRegex(@"^Hour(\d{2}){2}_(?:\d{2}){2}$")]
    private static partial Regex HourColumnRegex();

    public void SetupTimegrid()
    {
        Timegrid.RowDefinitions.Clear();
        Timegrid.RowDefinitions.Add(new() { Height = GridLength.Auto });
        Timegrid.ColumnDefinitions.Clear();
        Timegrid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });

        KeyValuePair<Day, WebUntis.SchoolHour[]>[] sortedTimegrid = TimegridFile.s_DefaultInstance.Timegrid.OrderBy(keyValue => (int)keyValue.Key).ToArray();
        if (sortedTimegrid.Length == 0)
            return;

        int currentRow = 1;
        foreach (KeyValuePair<Day, WebUntis.SchoolHour[]> keyValue in sortedTimegrid)
        {
            Timegrid.RowDefinitions.Add(new RowDefinition()
            {
                Name = keyValue.Key.ToString(),
                Height =  new GridLength(1, GridUnitType.Star)
            });
            int id = Timegrid.Children.Add(new TimegridDay(keyValue.Key));
            Grid.SetRow(Timegrid.Children[id], currentRow++);
            Grid.SetColumn(Timegrid.Children[id], 0);
        }

        DateTime firstTime = sortedTimegrid[0].Value.MinBy(hour => hour.StartTime)?.StartTime ?? new DateTime(2020, 1, 1, 0, 0, 0);
        DateTime lastTime = sortedTimegrid[0].Value.MaxBy(hour => hour.EndTime)?.EndTime ?? new DateTime(2020, 1, 1, 0, 0, 0);
        TimeSpan totalSchoolTime = lastTime - firstTime;

        int currentColumn = 1;
        WebUntis.SchoolHour[] schoolHours = sortedTimegrid[0].Value;
        for (int i = 0; i <= schoolHours.Length - 1; i++)
        {
            Timegrid.ColumnDefinitions.Add(new()
            {
                Name = $"Hour{schoolHours[i].StartTime:HHmm}_{schoolHours[i].EndTime:HHmm}",
                Width = new GridLength((schoolHours[i].EndTime - schoolHours[i].StartTime) / totalSchoolTime, GridUnitType.Star)
            });
            int id = Timegrid.Children.Add(new TimegridHour(schoolHours[i]));
            Grid.SetRow(Timegrid.Children[id], 0);
            Grid.SetColumn(Timegrid.Children[id], currentColumn++);

            if (schoolHours.Length - 1 < i + 1)
                break;
            Timegrid.ColumnDefinitions.Add(new()
            {
                Name = $"Break{schoolHours[i].EndTime:HHmm}_{schoolHours[i + 1].StartTime:HHmm}",
                Width = new GridLength((schoolHours[i + 1].StartTime - schoolHours[i].EndTime) / totalSchoolTime, GridUnitType.Star)
            });
            currentColumn++;
        }
    }

    public async Task UpdateTimetableAsync()
    {
        ViewModel.IsTimetableLoading = true;
        ToggleTimetableLoadingAnimation(true);

        // Remove old elements
        foreach (UIElement element in Timegrid.Children.Cast<UIElement>()
            .Where(e => e.GetType() == typeof(UserControls.SchoolHour) || e.GetType() == typeof(Grid) || e.GetType() == typeof(UserControls.Holidays))
            .ToArray())
            Timegrid.Children.Remove(element);

        WebUntis.Holidays[] holidays = HolidaysFile.s_DefaultInstance.Holidays.ToArray();
        Period[] periods = Array.Empty<Period>();

        try
        {
            if (!ViewModel.IsOffline)
                periods = await PeriodFile.LoadWeekAsync(SelectedWeek, ViewModel.ReloadTimetable).ConfigureAwait(true);
            else
                periods = PeriodFile.s_DefaultInstance[SelectedWeek].ToArray();
        }
        catch (Exception ex)
        {
            ex.HandleWithDefaultHandler(ViewModel, "Timetable loading");
        }

        // Set holidays
        for (DateTime date = SelectedWeek; date < SelectedWeek.AddDays(6); date = date.AddDays(1))
        {
            if (holidays.FirstOrDefault(h => h.StartDate <= date && h.EndDate >= date) is WebUntis.Holidays holiday)
            {
                int targetRow = Timegrid.RowDefinitions.IndexOf(Timegrid.RowDefinitions.FirstOrDefault(r => r.Name == date.DayOfWeek.ToString()));
                if (targetRow == -1)
                    continue;

                int id = Timegrid.Children.Add(new UserControls.Holidays(holiday.LongName));
                Grid.SetRow(Timegrid.Children[id], targetRow);
                Grid.SetColumn(Timegrid.Children[id], 1);
                Grid.SetColumnSpan(Timegrid.Children[id], Timegrid.ColumnDefinitions.Count - 1);

                // Remove overlayed periods
                periods = periods.Where(p => date != p.Date).ToArray();
            }
        }

        List<int> addedHours = new();

        if (!periods.Any())
            goto endTimetableUpdate;

        // Set normal lessons (hours that contains only one lesson)
        Period[] normalLessons = periods.Where(p => !periods.Any(ls =>
        {
            if (ls.Id == p.Id)
                return false;
            return ls.Date == p.Date && (ls.StartTime == p.StartTime || ls.EndTime == p.EndTime);
        })).ToArray();
        foreach (Period period in normalLessons)
        {
            if (addedHours.Contains(period.Id))
                continue;

            // Same lessons that follow to this lesson
            IEnumerable<Period> sameLessons = normalLessons.Where(p => p.Id != period.Id && p.Date == period.Date && periods.Any(ls => ls.StartTime >= p.StartTime || ls.EndTime <= p.EndTime) && p.IsSameLesson(period));
            int targetRow = Timegrid.RowDefinitions.IndexOf(Timegrid.RowDefinitions.FirstOrDefault(r => r.Name == period.Date.DayOfWeek.ToString()));
            int targetColumn = Timegrid.ColumnDefinitions.IndexOf(Timegrid.ColumnDefinitions.MinBy(c =>
            {
                Match nameMatch = HourColumnRegex().Match(c.Name);
                if (!nameMatch.Success)
                    return new TimeSpan(9999, 9, 9);

                DateTime hourStartTime = new(2020, 1, 1, int.Parse(nameMatch.Groups[1].Captures[0].Value), int.Parse(nameMatch.Groups[1].Captures[1].Value), 0);
                return hourStartTime >= period.StartTime ? hourStartTime - period.StartTime : period.StartTime - hourStartTime;
            }));
            int targetColumnSpan = (sameLessons.Count() * 2) + 1;

            if (targetRow == -1 || targetColumn == -1)
                continue;

            period.StartTime = sameLessons.Append(period).Min(p => p.StartTime);
            period.EndTime = sameLessons.Append(period).Max(p => p.EndTime);

            int id = Timegrid.Children.Add(new UserControls.SchoolHour(period));
            Grid.SetRow(Timegrid.Children[id], targetRow);
            Grid.SetColumn(Timegrid.Children[id], targetColumn);
            Grid.SetColumnSpan(Timegrid.Children[id], targetColumnSpan);

            foreach (Period p in sameLessons)
                addedHours.Add(p.Id);
            addedHours.Add(period.Id);
        }

        // Set multi lessons (hours that contains more than one lesson)
        Period[] multipleLessons = periods.Where(p => !normalLessons.Contains(p)).ToArray();
        foreach (Period period in multipleLessons)
        {
            if (addedHours.Contains(period.Id))
                continue;

            Period[] sameHourLessons = multipleLessons.Where(p => p.Date == period.Date && p.StartTime <= period.EndTime && p.EndTime >= period.StartTime).ToArray();
            Period[] uniqueLessons = sameHourLessons.Distinct(new PeriodExtensions.PeriodEqualityComparer()).ToArray();

            // Calc the needed rows and columns
            IEnumerable<IGrouping<DateTime, Period>> groups = uniqueLessons.GroupBy(p => p.StartTime);
            int rowCount = groups.Max(g => g.Count());
            int columnCount = groups.Count();

            Grid grid = new();
            for (int i = 0; i < rowCount; i++)
                grid.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) });
            for (int i = 0; i < columnCount; i++)
                grid.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });

            int currentRow = 0;
            int currentColumn = 0;
            foreach (Period lesson in uniqueLessons.OrderByDescending(p => sameHourLessons.Count(ls => ls.IsSameLesson(p))))
            {
                Period[] sameLessons = sameHourLessons.Where(p => p.IsSameLesson(lesson)).ToArray();
                lesson.StartTime = sameLessons.Min(p => p.StartTime);
                lesson.EndTime = sameLessons.Max(p => p.EndTime);

                int id = grid.Children.Add(new UserControls.SchoolHour(lesson));
                Grid.SetRow(grid.Children[id], currentRow);
                Grid.SetColumn(grid.Children[id], currentColumn);
                Grid.SetColumnSpan(grid.Children[id], sameLessons.Length);

                currentColumn += sameLessons.Length;
                if (currentColumn >= columnCount)
                {
                    currentColumn = 0;
                    currentRow++;
                }
            }

            Period firstPeriod = sameHourLessons.MinBy(p => p.StartTime)!;
            Period lastPeriod = sameHourLessons.MaxBy(p => p.StartTime)!;
            int targetRow = Timegrid.RowDefinitions.IndexOf(Timegrid.RowDefinitions.FirstOrDefault(r => r.Name == firstPeriod.Date.DayOfWeek.ToString()));
            int targetColumn = Timegrid.ColumnDefinitions.IndexOf(Timegrid.ColumnDefinitions.MinBy(c =>
            {
                Match nameMatch = HourColumnRegex().Match(c.Name);
                if (!nameMatch.Success)
                    return new TimeSpan(9999, 9, 9);

                DateTime hourStartTime = new(2020, 1, 1, int.Parse(nameMatch.Groups[1].Captures[0].Value), int.Parse(nameMatch.Groups[1].Captures[1].Value), 0);
                return hourStartTime >= firstPeriod.StartTime ? hourStartTime - firstPeriod.StartTime : firstPeriod.StartTime - hourStartTime;
            }));
            int targetColumnSpan = Timegrid.ColumnDefinitions.IndexOf(Timegrid.ColumnDefinitions.MinBy(c =>
            {
                Match nameMatch = HourColumnRegex().Match(c.Name);
                if (!nameMatch.Success)
                    return new TimeSpan(9999, 9, 9);

                DateTime hourStartTime = new(2020, 1, 1, int.Parse(nameMatch.Groups[1].Captures[0].Value), int.Parse(nameMatch.Groups[1].Captures[1].Value), 0);
                return hourStartTime >= lastPeriod.StartTime ? hourStartTime - lastPeriod.StartTime : lastPeriod.StartTime - hourStartTime;
            })) - targetColumn + 1;

            if (targetRow == -1 || targetColumn == -1)
                continue;

            _ = Timegrid.Children.Add(grid);
            Grid.SetRow(grid, targetRow);
            Grid.SetColumn(grid, targetColumn);
            Grid.SetColumnSpan(grid, targetColumnSpan);

            foreach (Period p in sameHourLessons)
                addedHours.Add(p.Id);
            addedHours.Add(period.Id);
        }

        endTimetableUpdate:
        ViewModel.ReloadTimetable = false;
        ToggleTimetableLoadingAnimation(false);
    }

    private void ErrorBoxClose_Click(object sender, RoutedEventArgs e)
    {
        Storyboard popupAnimation = (Storyboard)ErrorBox.FindResource("PopUpAnimation");
        void OnCompletion(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => ViewModel.ErrorBoxContent = string.Empty);
            popupAnimation.Completed -= OnCompletion;
        }

        popupAnimation.Completed += OnCompletion;
        popupAnimation.AutoReverse = true;
        popupAnimation.Begin();
        popupAnimation.Pause();
        popupAnimation.Seek(TimeSpan.FromSeconds(1));
        popupAnimation.Resume();
    }

    private void MenuBtn_Click(object sender, RoutedEventArgs e)
    {
        ProfileFile profile = ProfileCollection.GetActiveProfile();

        string targetName = ((FrameworkElement)sender).Name;
        switch (targetName)
        {
            case nameof(TodayBtn):
                TodayItem.IsSelected = true;
                profile.Options.SelectedMenuItem = MenuItems.TodayItem;
                break;
            case nameof(TimetableBtn):
                TimetableItem.IsSelected = true;
                profile.Options.SelectedMenuItem = MenuItems.TimetableItem;
                break;
            case nameof(MailBtn):
                MailItem.IsSelected = true;
                profile.Options.SelectedMenuItem = MenuItems.MailItem;
                break;
            case nameof(SettingsBtn):
                SettingsItem.IsSelected = true;
                profile.Options.SelectedMenuItem = MenuItems.SettingsItem;
                break;
            case nameof(ProfileBtn):
                ProfileItem.IsSelected = true;
                profile.Options.SelectedMenuItem = MenuItems.ProfileItem;
                break;
            default:
                return;
        }
        profile.Update();

        e.Handled = true;
    }

    private void OptionsMenuBtn_Click(object sender, RoutedEventArgs e)
    {
        ProfileFile profile = ProfileCollection.GetActiveProfile();

        string targetName = ((FrameworkElement)sender).Name;
        switch (targetName)
        {
            case nameof(NotificationOptBtn):
                NotificationOptItem.IsSelected = true;
                profile.Options.SelectedOptionsMenuItems = OptionsMenuItems.NotifyOptItem;
                break;
            case nameof(ThirdPartyBtn):
                ThirdPartyItem.IsSelected = true;
                profile.Options.SelectedOptionsMenuItems = OptionsMenuItems.ThirdPartyItem;
                break;
            default:
                return;
        }
        profile.Update();

        e.Handled = true;
    }

    private void MailMenuBtn_Click(object sender, RoutedEventArgs e)
    {
        string targetName = ((FrameworkElement)sender).Name;
        switch (targetName)
        {
            case nameof(InboxBtn):
                InboxItem.IsSelected = true;
                break;
            case nameof(SentBtn):
                SentItem.IsSelected = true;
                break;
            case nameof(DraftsBtn):
                DraftsItem.IsSelected = true;
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private void ProfileMenuBtn_Click(object sender, RoutedEventArgs e)
    {
        string targetName = ((FrameworkElement)sender).Name;
        switch (targetName)
        {
            case nameof(GenerallyBtn):
                GenerallyItem.IsSelected = true;
                break;
            case nameof(ContactDetailsBtn):
                ContactDetailsItem.IsSelected = true;
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private void ListViewScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ScrollViewer scrollViewer = (ScrollViewer)sender;
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        e.Handled = true;
    }

    private void NotifyOptBtn_Click(object sender, RoutedEventArgs e)
    {
        string targetName = (((FrameworkElement)sender).Name);
        switch (targetName)
        {
            case nameof(NotifyNews):
                ViewModel.CurrentProfile.Options.NotifyOnNews = !ViewModel.CurrentProfile.Options.NotifyOnNews;
                break;
            case nameof(NotifyMail):
                ViewModel.CurrentProfile.Options.NotifyOnMessages = !ViewModel.CurrentProfile.Options.NotifyOnMessages;
                break;
        }

        ViewModel.CurrentProfile.Update();

        e.Handled = true;
    }

    private void ManageProfilesBtn_Click(object sender, RoutedEventArgs e)
    {
        new ProfileManageDialog().ShowDialog(); 
        e.Handled = true;
    }

    private void ToggleTimetableLoadingAnimation(bool turnOn)
    {
        Storyboard animation = (Storyboard)TimetableLoadingProgressImg.FindResource("TimetableLoadingAnimation");

        if (turnOn)
        {
            ViewModel.IsTimetableLoading = true;
            animation.Begin();
        }
        else
        {
            ViewModel.IsTimetableLoading = false;
            animation.Stop();
        }
    }

    private void ToggleMailsLoadingAnimation(bool turnOn)
    {
        Storyboard animation = (Storyboard)MailLoadingProgressImg.FindResource("MailsLoadingAnimation");

        if (turnOn)
            animation.Begin();
        else
            animation.Stop();
    }

    private void UpdateMailInbox()
    {
        MailInbox.Children.Clear();
        MailInbox.RowDefinitions.Clear();
        foreach (MessagePreview preview in ViewModel.MsgInbox)
        {
            MailInbox.RowDefinitions.Add(new() { Height = GridLength.Auto });
            int id = MailInbox.Children.Add(new MessageControl(preview));
            Grid.SetRow(MailInbox.Children[id], MailInbox.RowDefinitions.Count - 1);
        }
    }

    private void UpdateConfirmationMailInbox()
    {
        ConfirmationMailInbox.Children.Clear();
        ConfirmationMailInbox.RowDefinitions.Clear();
        foreach (MessagePreview preview in ViewModel.ConfirmationMsgInbox)
        {
            ConfirmationMailInbox.RowDefinitions.Add(new() { Height = GridLength.Auto });
            int id = ConfirmationMailInbox.Children.Add(new MessageControl(preview));
            Grid.SetRow(ConfirmationMailInbox.Children[id], ConfirmationMailInbox.RowDefinitions.Count - 1);
        }
    }

    private void UpdateMailsSent()
    {
        SentMails.Children.Clear();
        SentMails.RowDefinitions.Clear();
        foreach (MessagePreview preview in ViewModel.SentMsg)
        {
            preview.IsMessageRead = true;
            SentMails.RowDefinitions.Add(new() { Height = GridLength.Auto });
            int id = SentMails.Children.Add(new MessageControl(preview, isSentMessage: true));
            Grid.SetRow(SentMails.Children[id], SentMails.RowDefinitions.Count - 1);
        }
    }

    private void UpdateMailDrafts()
    {
        DraftMails.Children.Clear();
        DraftMails.RowDefinitions.Clear();
        foreach (DraftPreview preview in ViewModel.DraftMessages)
        {
            DraftMails.RowDefinitions.Add(new() { Height = GridLength.Auto });
            int id = DraftMails.Children.Add(new DraftControl(preview));
            Grid.SetRow(DraftMails.Children[id], DraftMails.RowDefinitions.Count - 1);
        }
    }

    private void DisplayLastMenuItem(MenuItems item)
    {
        if (item != MenuItems.ProfileItem)
            _ = ViewModel.LoadProfileTabAsync();

        switch (item)
        {
            case MenuItems.TodayItem:
                _ = ViewModel.LoadTodayTabAsync();
                TodayItem.IsSelected = true;
                break;
            case MenuItems.TimetableItem:
                TimetableItem.IsSelected = true;
                break;
            case MenuItems.MailItem:
                _ = ViewModel.LoadMailTabAsync();
                MailItem.IsSelected = true;
                break;
            case MenuItems.SettingsItem:
                SettingsItem.IsSelected = true;
                break;
            case MenuItems.ProfileItem:
                _ = ViewModel.LoadProfileTabAsync();
                ProfileItem.IsSelected = true;
                break;
        }
    }

    private void DisplayLastOptionsMenuItem(OptionsMenuItems item)
    {
        switch (item)
        {
            case OptionsMenuItems.NotifyOptItem:
                NotificationOptItem.IsSelected = true;
                break;
            case OptionsMenuItems.ThirdPartyItem:
                ThirdPartyItem.IsSelected = true;
                break;
        }
    }

    private void RenderLicenses()
    {
        List<LicenceInformation> licenses = Licenses.Licenses.GetLicenses().ToList();

        for (int i = 0; i < licenses.Count - 1; i++)
        {
            licenses[i].AddToPanel(ThirdParty);

            // Render the border line
            Line line = new()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new(0, 10, 0, 10)
            };
            line.SetBinding(Line.X2Property, new Binding
            {
                Source = ThirdParty,
                Path = new("ActualWidth"),
                Converter = new MathConverter(),
                ConverterParameter = "-20"
            });
            ThirdParty.Children.Add(line);
        }

        licenses.Last().AddToPanel(ThirdParty);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        foreach (Window window in Application.Current.Windows
            .Cast<Window>()
            .Where(w => w.GetType() != typeof(LoginWindow) && !ReferenceEquals(w, this))
            .ToArray())
            window.Close();
    }
}