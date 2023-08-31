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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UntisDesktop.UserControls;
using UntisDesktop.ViewModels;
using WebUntisAPI.Client.Models.Messages;

namespace UntisDesktop.Views;

public partial class MessageWindow : Window
{
    private MessageWindowViewModel ViewModel { get => (MessageWindowViewModel)DataContext; }

    public MessageWindow()
    {
        InitializeComponent();

        ViewModel.PropertyChanged += (_, e) =>
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
        };
    }

    public MessageWindow(MessagePreview preview) : this()
    {
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

    private void AddRecipient_Click(object sender, RoutedEventArgs e)
    {
        RecipientDialog recipientDialog = new(new(ViewModel.Recipients));
        if (recipientDialog.ShowDialog() == true)
        {
            ViewModel.Recipients.Clear();
            Recipients.Children.Clear();

            foreach (MessagePerson person in recipientDialog.SelectedRecipients)
            {
                ViewModel.Recipients.Add(person);
                RecipientControl recipient = new(person);
                recipient.DeleteEventHandler += (_, _) =>
                {
                    ViewModel.Recipients.Remove(recipient.MessagePerson);
                    Recipients.Children.Remove(recipient);
                };

                Recipients.Children.Add(recipient);
            }
        }
    }
}
