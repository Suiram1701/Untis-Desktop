using Data.Messages;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using UntisDesktop.Localization;
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

        e.Handled = true;
    }

    private void AddAttachment_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Title = LangHelper.GetString("MessageWindow.A.T"),
            Filter = LangHelper.GetString("MessageWindow.A.F", "(*.*)|*.*"),
            CheckFileExists = true,
            Multiselect = true,
        };
        if (dialog.ShowDialog() ?? false)
        {
            int maxFileCount = MessagePermissionsFile.s_DefaultInstance.Permissions.MaxFileCount;
            int currentFileCount = Attachments.Children
                .OfType<AttachmentControl>()
                .Count();

            if (dialog.FileNames.Length + currentFileCount > maxFileCount)
                MessageBox.Show(LangHelper.GetString("MessageWindow.A.TMF", maxFileCount.ToString()), LangHelper.GetString("MessageWindow.A.TMF.T"), MessageBoxButton.OK, MessageBoxImage.Exclamation);

            foreach (string file in dialog.FileNames.Take(maxFileCount - currentFileCount))
            {
                using FileStream stream = new(file, FileMode.Open, FileAccess.Read);
                string fileName = System.IO.Path.GetFileName(file);

                AttachmentControl attachment = new(fileName, stream);
                attachment.DeleteEventHandler += (sender, _) =>
                {
                    ((AttachmentControl)sender!).Stream.Dispose();
                    Attachments.Children.Remove(attachment);
                };
                Attachments.Children.Add(attachment);
            }
        }

        e.Handled = true;
    }

    private void TextContent_KeyDown(object sender, KeyEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        int currentIndex = textBox.CaretIndex;

        switch (e.Key)
        {
            case Key.Enter:
                textBox.Text = textBox.Text.Insert(currentIndex, Environment.NewLine);
                textBox.SelectionStart = ++currentIndex;
                break;
            case Key.Tab:
                textBox.Text = textBox.Text.Insert(currentIndex, "\t");
                textBox.SelectionStart = ++currentIndex;
                break;

        }
    }
}
