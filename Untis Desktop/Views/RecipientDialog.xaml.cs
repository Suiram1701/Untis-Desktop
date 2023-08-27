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

        // TODO: Only for development
        if (!s_DefaultInstance.Permissions.RecipientOptions.Contains("STAFF"))
            s_DefaultInstance.Permissions.RecipientOptions = s_DefaultInstance.Permissions.RecipientOptions.Append("STAFF").ToArray();

        InitializeComponent();

        // Display the different recipient option when more than one
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

        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(RecipientDialogViewModel.AvailablePeople))
                RenderRecipients();
        };

        _ = ViewModel.ApplyFiltersAsync();
    }

    private void RenderRecipients()
    {
        VisibleRecipients.Children.Clear();
        VisibleRecipients.RowDefinitions.Clear();

        foreach ((string type, MessagePerson[] people) in ViewModel.AvailablePeople)
        {
            if (!people.Any())
                continue;

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

            foreach (MessagePerson person in people)
            {
                Recipient recipient = new(person, SelectedRecipients.Any(r => r.Id == person.Id)) { VerticalAlignment = VerticalAlignment.Top };
                recipient.ToggleSelectEventHandler += (_, _) =>
                {
                    // Handler for add / remove of the person
                    if (recipient.IsSelected && SelectedRecipients.All(r => r.Id != person.Id))
                        SelectedRecipients.Add(person);
                    else
                        SelectedRecipients.Remove(SelectedRecipients.FirstOrDefault(r => r.Id == person.Id) ?? new());
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
}
