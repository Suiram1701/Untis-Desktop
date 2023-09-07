using Data.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UntisDesktop.ViewModels;
using WebUntisAPI.Client.Models.Messages;
using static Data.Messages.MessagePermissionsFile;
using UntisDesktop.UserControls;
using Newtonsoft.Json.Linq;
using UntisDesktop.Localization;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using WebUntisAPI.Client.Models;
using UntisDesktop.Extensions;
using System.Windows.Threading;

namespace UntisDesktop.Views;

public partial class RecipientDialog : Window
{
    public List<MessagePerson> SelectedRecipients = new();

    private RecipientDialogViewModel ViewModel { get => (RecipientDialogViewModel)DataContext; }

    public RecipientDialog(List<MessagePerson> currentSelectedPersons)
    {
        SelectedRecipients = currentSelectedPersons;

        if (!s_DefaultInstance.Permissions.RecipientOptions.Any())
            DialogResult = false;

        InitializeComponent();

        ViewModel.PropertyChanged += (_, e) =>
        {
            Dispatcher.Invoke(() =>
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
                else if (e.PropertyName == nameof(RecipientDialogViewModel.AvailablePeople))
                    RenderRecipients();
            }, DispatcherPriority.Render);
        };

        // Display the different recipient options when more than one option
        if (s_DefaultInstance.Permissions.RecipientOptions.Length > 1)
        {
            foreach (string optionName in s_DefaultInstance.Permissions.RecipientOptions)
            {
                // Add the option button
                Button optionBtn = new()
                {
                    Name = optionName,
                    Content = new Label
                    {
                        Content = LangHelper.GetString("RecipientDialog.RO." + optionName),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new(-5)
                    },
                    Margin = new(5, 10, 5, 0),
                    Template = (ControlTemplate)Application.Current.FindResource("MenuBtn"),
                    Style = new(typeof(Button))
                    {
                        Triggers =
                        {
                            new DataTrigger
                            {
                                Binding = new Binding("CurrentRecipientOption") { Mode = BindingMode.OneWay },
                                Value = optionName,
                                Setters = { new Setter(BackgroundProperty, Application.Current.FindResource("PressedDarkBtnColor")) }
                            }
                        }
                    }
                };
                optionBtn.Click += SelectRecipientOption_ClickAsync;

                int id = RecipientOptions.Children.Add(optionBtn);
                RecipientOptions.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                Grid.SetColumn(optionBtn, RecipientOptions.ColumnDefinitions.Count - 1);
            }
        }

        // Display the available filters
        Task.Run(async () =>
        {
            try
            {
                Dictionary<string, FilterItem[]> filters = await App.Client!.GetStaffSearchFiltersAsync();

                Dispatcher.Invoke(() =>
                {
                    foreach ((string type, FilterItem[] items) in filters)
                    {
                        FilterBox filterBox = new(LangHelper.GetString("RecipientDialog.FT." + type), items) { VerticalAlignment = VerticalAlignment.Top };

                        // The handler when a filter is selected
                        filterBox.SelectionChangedEvent += (_, e) =>
                        {
                            if (ViewModel.Filters.ContainsKey(type))
                            {
                                if (ViewModel.Filters[type].Any(i => i.ReferenceId == e.UpdateId))
                                {
                                    ViewModel.Filters[type].Remove(ViewModel.Filters[type].FirstOrDefault(i => i.ReferenceId == e.UpdateId));

                                    // Remove full type when nothing is in there
                                    if (!ViewModel.Filters[type].Any())
                                        ViewModel.Filters.Remove(type);
                                }
                                else
                                    ViewModel.Filters[type].Add(items.First(i => i.ReferenceId == e.UpdateId));
                            }
                            else
                                ViewModel.Filters.Add(type, new() { items.FirstOrDefault(i => i.ReferenceId == e.UpdateId) });

                            Task.Run(ViewModel.ApplyFiltersAsync);
                            e.Handled = true;
                        };

                        Filters.Children.Add(filterBox);
                    }
                });
            }
            catch (Exception ex)
            {
                ex.HandleWithDefaultHandler(ViewModel, "Get Staff filters");
            }
        });

        Task.Run(ViewModel.ApplyFiltersAsync);
    }

    private void RenderRecipients()
    {
        VisibleRecipients.Children.Clear();
        VisibleRecipients.RowDefinitions.Clear();

        foreach ((string type, MessagePerson[] people) in ViewModel.AvailablePeople)
        {
            if (!people.Any())     // When empty skip
                continue;

            if (ViewModel.ViewSelectedRecipients)     // When selection required skip the non selected people
                if (!SelectedRecipients.Any(r => people.Any(p => p.Id == r.Id)))
                    continue;

            if (!string.IsNullOrEmpty(type))
            {
                int labelId = VisibleRecipients.Children.Add(new TextBlock
                {
                    Text = LangHelper.GetString("RecipientDialog.RT." + type),
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new(20, ViewModel.AvailablePeople.First().Key == type
                    ? 0
                    : 25, 0, 0)
                });
                VisibleRecipients.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) });
                Grid.SetRow(VisibleRecipients.Children[labelId], VisibleRecipients.RowDefinitions.Count - 1);
            }

            foreach (MessagePerson person in people)
            {
                if (ViewModel.ViewSelectedRecipients)     // When selection required skip the non selected people
                    if (!SelectedRecipients.Any(r => r.DisplayName == person.DisplayName))
                        continue;

                Recipient recipient = new(person, SelectedRecipients.Any(r => r.DisplayName == person.DisplayName)) { VerticalAlignment = VerticalAlignment.Top };
                recipient.ToggleSelectEventHandler += (_, _) =>
                {
                    // Handler for add / remove of the person
                    if (recipient.IsSelected)
                        SelectedRecipients.Add(person);
                    else
                        SelectedRecipients.Remove(SelectedRecipients.FirstOrDefault(r => r.DisplayName == person.DisplayName) ?? new());
                };

                // Only one person can selected
                if (ViewModel.CurrentRecipientOption == "TEACHER")
                {
                    recipient.ToggleSelectEventHandler += (_, _) =>
                    {
                        foreach (Recipient recipient in VisibleRecipients.Children.OfType<Recipient>())
                        {
                            if (recipient.MessagePerson.Id != person.Id)
                            {
                                recipient.SetValue(Recipient.IsSelectedProperty, false);
                                SelectedRecipients.Remove(recipient.MessagePerson);
                            }
                        }

                        foreach (MessagePerson p in SelectedRecipients.ToArray())
                        {
                            if (p.Id != person.Id)
                                SelectedRecipients.Remove(p);
                        }
                    };
                }

                int id = VisibleRecipients.Children.Add(recipient);
                VisibleRecipients.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) });
                Grid.SetRow(VisibleRecipients.Children[id], VisibleRecipients.RowDefinitions.Count - 1);
            }
        }
    }

    private async void SelectRecipientOption_ClickAsync(object sender, RoutedEventArgs e)
    {
        string name = ((FrameworkElement)sender).Name;

        if (s_DefaultInstance.Permissions.RecipientOptions.Contains(name))
        {
            ViewModel.CurrentRecipientOption = name;
            ViewModel.SearchText = string.Empty;
            ViewModel.ViewSelectedRecipients = false;
            ViewModel.Filters.Clear();

            await ViewModel.ApplyFiltersAsync();
        }

        e.Handled = true;
    }

    private void Abort_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        e.Handled = true;
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        e.Handled = true;
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

    private void ViewSelectedPeople_Click(object sender, RoutedEventArgs e)
    {
        RenderRecipients();
    }
}
