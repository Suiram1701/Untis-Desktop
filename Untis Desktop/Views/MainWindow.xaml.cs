using Data.Profiles;
using Data.Static;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UntisDesktop.Extensions;
using UntisDesktop.UserControls;
using UntisDesktop.ViewModels;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Models;
using WebUntis = WebUntisAPI.Client.Models;

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
        }
    }
    private static DateTime s_SelectedWeek;

    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    public MainWindow()
    {
        SelectedWeek = DateTime.Now;

        InitializeComponent();
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

            // Timetable update
            if (e.PropertyName == nameof(MainWindowViewModel.SelectedWeek))
            {
                foreach (TimegridDay day in Timegrid.Children.OfType<TimegridDay>())
                    day.ViewModel.Update();
                await UpdateTimetableAsync();
            }
        };

        SetupTimegrid();
    }

    [GeneratedRegex(@"^Hour(\d{2}){2}_(\d{2}){2}$")]
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
        using WebUntisClient client = await ProfileCollection.GetActiveProfile().LoginAsync(CancellationToken.None).ConfigureAwait(true);
        List<Period> periods = (await client.GetOwnTimetableAsync(new DateTime(2023, 6, 12), new DateTime(2023, 6, 16)).ConfigureAwait(true)).ToList();

        // Remove old
        foreach (UserControls.SchoolHour schoolHour in Timegrid.Children.OfType<UserControls.SchoolHour>().ToArray())
            Timegrid.Children.Remove(schoolHour);

        List<int> addedHours = new();

        // Set nomal lessons (hours that contains only one lesson)
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
            IEnumerable<Period> sameLessons = normalLessons.Where(p => p.Id != period.Id && periods.Any(ls => ls.StartTime >= p.StartTime || ls.EndTime <= p.EndTime) && p.IsSameLesson(period));
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

            int id = Timegrid.Children.Add(new UserControls.SchoolHour(period));
            Grid.SetRow(Timegrid.Children[id], targetRow);
            Grid.SetColumn(Timegrid.Children[id], targetColumn);
            Grid.SetColumnSpan(Timegrid.Children[id], targetColumnSpan);

            foreach (Period p in sameLessons)
                addedHours.Add(p.Id);
            addedHours.Add(period.Id);
        }

        // Set multi lessons (hours that contains more than one lesson)
        Period[] mutliLessons = periods.Where(p => !normalLessons.Contains(p)).ToArray();
        foreach (Period period in mutliLessons)
        {
            if (addedHours.Contains(period.Id))
                continue;

            Period[] sameHourLessons = mutliLessons.Where(p => p.Date == period.Date && p.StartTime <= period.EndTime && p.EndTime >= period.StartTime).ToArray();
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
                int columnSpan = sameHourLessons.Count(p => p.IsSameLesson(lesson));
                
                int id = grid.Children.Add(new UserControls.SchoolHour(lesson));
                Grid.SetRow(grid.Children[id], currentRow);
                Grid.SetColumn(grid.Children[id], currentColumn);
                Grid.SetColumnSpan(grid.Children[id], columnSpan);

                currentColumn += columnSpan;
                if (currentColumn >= columnCount)
                {
                    currentColumn = 0;
                    currentRow++;
                }
            }

            Period firstPeriod = sameHourLessons.MinBy(p => p.StartTime)!;
            int targetRow = Timegrid.RowDefinitions.IndexOf(Timegrid.RowDefinitions.FirstOrDefault(r => r.Name == firstPeriod.Date.DayOfWeek.ToString()));
            int targetColumn = Timegrid.ColumnDefinitions.IndexOf(Timegrid.ColumnDefinitions.MinBy(c =>
            {
                Match nameMatch = HourColumnRegex().Match(c.Name);
                if (!nameMatch.Success)
                    return new TimeSpan(9999, 9, 9);

                DateTime hourStartTime = new(2020, 1, 1, int.Parse(nameMatch.Groups[1].Captures[0].Value), int.Parse(nameMatch.Groups[1].Captures[1].Value), 0);
                return hourStartTime >= firstPeriod.StartTime ? hourStartTime - firstPeriod.StartTime : firstPeriod.StartTime - hourStartTime;
            }));
            int targetColumnSpan = (columnCount - 1) * 2 + 1;

            _ = Timegrid.Children.Add(grid);
            Grid.SetRow(grid, targetRow);
            Grid.SetColumn(grid, targetColumn);
            Grid.SetColumnSpan(grid, targetColumnSpan);

            foreach (Period p in sameHourLessons)
                addedHours.Add(p.Id);
            addedHours.Add(period.Id);
        }
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
        string targetName = ((FrameworkElement)sender).Name;
        switch (targetName)
        {
            case "TodayBtn":
                TodayItem.IsSelected = true;
                break;
            case "TimetableBtn":
                TimetableItem.IsSelected = true;
                break;
            case "MailBtn":
                MailItem.IsSelected = true;
                break;
            case "SettingsBtn":
                SettingsItem.IsSelected = true;
                break;
            case "ProfileBtn":
                ProfileItem.IsSelected = true;
                break;
        }
    }

    private void ListViewScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ScrollViewer scrollViewer = (ScrollViewer)sender;
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        e.Handled = true;
    }
}