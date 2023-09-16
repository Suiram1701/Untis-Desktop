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
using UntisDesktop.Extensions;
using UntisDesktop.Localization;
using UntisDesktop.UserControls;
using UntisDesktop.ViewModels;
using WebUntisAPI.Client.Models.Messages;
using System.Windows.Threading;
using System.ComponentModel;

namespace UntisDesktop.Views;

public partial class MessageWindow : Window
{
    private MessageWindowViewModel ViewModel { get => (MessageWindowViewModel)DataContext; }

    private Message? _originalMessage = null;

    private readonly bool _isDraft = false;
    private Draft? _originalDraft = null;

    private bool _isClosing = false;

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

    /// <summary>
    /// Reply a message
    /// </summary>
    /// <param name="message">the reply form</param>
    public MessageWindow(Message message) : this()
    {
        ViewModel.IsReplyForm = true;
        ViewModel.Subject = message.Subject;
        ViewModel.Content = message.Content;

        Recipients.Children.Add(new RecipientControl(message.Recipients[0], false));

        RenderReplyHistory(message.ReplyHistory);
    }

    /// <summary>
    /// Display a sent or a incoming message
    /// </summary>
    /// <param name="preview">The preview</param>
    public MessageWindow(MessagePreview preview) : this()
    {
        // Preload the preview
        ViewModel.IsReadOnly = true;
        ViewModel.Subject = preview.Subject;
        ViewModel.HasAttachments = preview.HasAttachments;
        ViewModel.Content = preview.ContentPreview;
        ViewModel.SentDate = preview.SentTime;

        // Display recipients or sender
        if (preview.Sender is null)
        {
            foreach (MessagePerson person in preview.Recipients)
                Recipients.Children.Add(new RecipientControl(person, false));
        }
        else
        {
            ViewModel.RecipientType = LangHelper.GetString("MessageWindow.S");
            Recipients.Children.Add(new RecipientControl(preview.Sender, false));
        }

        _ = Dispatcher.Invoke(async () =>
        {
            try
            {
                // Load the full message
                Message message = await preview.GetFullMessageAsync(App.Client);
                _originalMessage = message;

                ViewModel.Content = message.Content;
                foreach (Attachment attachment in message.Attachments)
                    Attachments.Children.Add(new AttachmentControl(attachment));

                RenderConfirmationBox(message.ConfirmationInformations);

                RenderReplyHistory(message.ReplyHistory);
            }
            catch (Exception ex)
            {
                ex.HandleWithDefaultHandler(ViewModel, "Load complete message");
            }
        }, DispatcherPriority.DataBind);
    }

    /// <summary>
    /// Display a draft
    /// </summary>
    /// <param name="preview">The preview</param>
    public MessageWindow(DraftPreview preview) : this()
    {
        _isDraft = true;

        // Preload the preview
        ViewModel.Subject = preview.Subject;

        ViewModel.HasAttachments = preview.HasAttachments;
        ViewModel.Content = preview.ContentPreview;

        _ = Dispatcher.Invoke(async () =>
        {
            try
            {
                // Load the full message
                Draft draft = await preview.GetFullMessageAsync(App.Client);
                ViewModel.Content = draft.Content;
                foreach (Attachment attachment in draft.Attachments)
                {
                    AttachmentControl control = new(attachment, true);
                    control.DeleteEventHandler += (sender, _) => Attachments.Children.Remove((UIElement)sender);

                    Attachments.Children.Add(control);
                }

                _originalDraft = draft;
            }
            catch (Exception ex)
            {
                ex.HandleWithDefaultHandler(ViewModel, "Load complete message");
            }
        }, DispatcherPriority.DataBind);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isClosing && !ViewModel.IsReadOnly)
        {
            switch (MessageBox.Show(
                owner: this,
                messageBoxText: LangHelper.GetString("MessageWindow.Save.W"),
                caption: LangHelper.GetString("MessageWindow.Save.W.T"),
                button: MessageBoxButton.YesNoCancel,
                icon: MessageBoxImage.Question,
                defaultResult: MessageBoxResult.Cancel))
            {
                case MessageBoxResult.Yes:
                    ReleaseAllAttachments();
                    break;
                case MessageBoxResult.No:
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }

        base.OnClosing(e);
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
            long maxFileSize = MessagePermissionsFile.s_DefaultInstance.Permissions.MaxFileSize;
            int maxFileCount = MessagePermissionsFile.s_DefaultInstance.Permissions.MaxFileCount;
            int currentFileCount = Attachments.Children
                .OfType<AttachmentControl>()
                .Count();

            if (dialog.FileNames.Length + currentFileCount > maxFileCount)
                MessageBox.Show(LangHelper.GetString("MessageWindow.A.TMF", maxFileCount.ToString()), LangHelper.GetString("MessageWindow.A.TMF.T"), MessageBoxButton.OK, MessageBoxImage.Exclamation);

            List<string> tooLargeFiles = new();
            foreach (string file in dialog.FileNames.Take(maxFileCount - currentFileCount))
            {
                using FileStream stream = new(file, FileMode.Open, FileAccess.Read);
                string fileName = Path.GetFileName(file);

                if (stream.Length > maxFileSize)
                {
                    tooLargeFiles.Add(fileName);
                    continue;
                }

                AttachmentControl attachment = new(fileName, stream);
                attachment.DeleteEventHandler += (sender, _) => Attachments.Children.Remove(attachment);
                Attachments.Children.Add(attachment);
            }

            if (tooLargeFiles.Any())
            {
                string fileSizeString = ConvertToSavingUnit(maxFileSize);

                if (tooLargeFiles.Count > 1)
                {
                    MessageBox.Show(
                        messageBoxText: LangHelper.GetString("MessageWindow.A.TLFs", string.Join(", ", tooLargeFiles.Take(tooLargeFiles.Count - 1).Select(f => '"' + f + '"')), '"' + tooLargeFiles.Last() + '"', fileSizeString),
                        caption: LangHelper.GetString("MessageWindow.A.TLFs.T"),
                        button: MessageBoxButton.OK,
                        icon: MessageBoxImage.Exclamation);
                }
                else
                {
                    MessageBox.Show(
                        messageBoxText: LangHelper.GetString("MessageWindow.A.TLF", '"' + tooLargeFiles[0] + '"', fileSizeString),
                        caption: LangHelper.GetString("MessageWindow.A.TLF.T"),
                        button: MessageBoxButton.OK,
                        icon: MessageBoxImage.Exclamation);
                }
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

    private async void Save_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            // Update exist draft
            if (_isDraft)
            {
                if (_originalDraft is null)
                    throw new InvalidOperationException("The object isn't loaded yet.");

                _originalDraft.Subject = ViewModel.Subject;
                _originalDraft.Content = ViewModel.Content;
                _originalDraft.ForbidReply = ViewModel.ForbidReply;

                Tuple<string, Stream>[] newAttachments = Attachments.Children
                    .OfType<AttachmentControl>()
                    .Where(a => a.Stream is not null)
                    .Select(a => new Tuple<string, Stream>(a.FileName, a.Stream!))
                    .ToArray();

                Attachment[] deletedAttachments = _originalDraft.Attachments
                    .Where(a => Attachments.Children.OfType<AttachmentControl>().All(att => a.Id != att.Attachment!.Value.Id))
                    .ToArray();

                await App.Client!.UpdateDraftAsync(_originalDraft, newAttachments, deletedAttachments);
            }
            else
            {
                // Create draft
                Tuple<string, Stream>[] attachments = Attachments.Children
                    .OfType<AttachmentControl>()
                    .Select(a => new Tuple<string, Stream>(a.FileName, a.Stream!))
                    .ToArray();

                await App.Client!.CreateDraftAsync(
                    subject: ViewModel.Subject,
                    content: ViewModel.Content,
                    recipientOption: MessagePermissionsFile.s_DefaultInstance.Permissions.RecipientOptions[0],
                    forbidReply: ViewModel.ForbidReply,
                    copyToStudent: false,
                    attachments: attachments);
            }
        }
        catch (Exception ex)
        {
            ex.HandleWithDefaultHandler(ViewModel, "Draft Saving");
        }
        finally
        {
            ReleaseAllAttachments();
        }

        // Reload messages
        MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().First();
        await ((MainWindowViewModel)mainWindow.DataContext).LoadMailTabAsync();

        _isClosing = true;
        mainWindow.Focus();
        Close();

    }

    private async void Send_ClickAsync(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsReplyForm)
        {
            await SendAsReplyAsync();

            // Reload messages
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().First();
            await ((MainWindowViewModel)mainWindow.DataContext).LoadMailTabAsync();

            _isClosing = true;
            mainWindow.Focus();
            Close();
            return;
        }

        // Show warning when not enough recipients are selected
        if (!ViewModel.Recipients.Any())
        {
            MessageBox.Show(this, LangHelper.GetString("MessageWindow.Send.NR"), LangHelper.GetString("MessageWindow.Send.NR.T"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;
        }

        try
        {
            if (_isDraft)
            {
                if (_originalDraft is null)
                    throw new InvalidOperationException("The object isn't loaded yet.");

                _originalDraft.Subject = ViewModel.Subject;
                _originalDraft.Content = ViewModel.Content;

                // Add new attachments or remove deleted attachments
                Tuple<string, Stream>[] newAttachments = Attachments.Children
                    .OfType<AttachmentControl>()
                    .Where(a => a.Stream is not null)
                    .Select(a => new Tuple<string, Stream>(a.FileName, a.Stream!))
                    .ToArray();

                Attachment[] deletedAttachments = _originalDraft.Attachments
                    .Where(a => Attachments.Children.OfType<AttachmentControl>().All(att => a.Id != att.Attachment!.Value.Id))
                    .ToArray();
                
                if (newAttachments.Any() || deletedAttachments.Any())
                    await App.Client!.UpdateDraftAsync(_originalDraft, newAttachments, deletedAttachments);


                await App.Client!.SendMessageAsync(_originalDraft, ViewModel.Recipients.ToArray(), ViewModel.RequestReadConfirmation, TimeSpan.FromSeconds(10));

                // Delete the send draft
                await App.Client!.DeleteDraftAsync(_originalDraft);
            }
            else
            {
                Tuple<string, Stream>[] attachments = Attachments.Children
                    .OfType<AttachmentControl>()
                    .Select(a => new Tuple<string, Stream>(a.FileName, a.Stream!))
                    .ToArray();

                await App.Client!.SendMessageAsync(
                    subject: ViewModel.Subject,
                    content: ViewModel.Content.Replace(Environment.NewLine, "<br>"),
                    recipients: ViewModel.Recipients.ToArray(),
                    requestConfirmation: ViewModel.RequestReadConfirmation,
                    forbidReply: ViewModel.ForbidReply,
                    attachments: attachments);
            }

            // Reload messages
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().First();
            await ((MainWindowViewModel)mainWindow.DataContext).LoadMailTabAsync();

            _isClosing = true;
            mainWindow.Focus();
            Close();
        }
        catch (Exception ex)
        {
            ex.HandleWithDefaultHandler(ViewModel, "Send Message");
        }
        finally
        {
            ReleaseAllAttachments();
        }
    }

    private async void SendReadConfirmation_ClickAsync(object sender, RoutedEventArgs e)
    {
        if (_originalMessage is null)
            throw new InvalidOperationException("The object isn't loaded yet.");

        await App.Client!.ConfirmMessageAsync(_originalMessage);

        e.Handled = true;
    }

    public async Task SendAsReplyAsync()
    {
        try
        {
            Tuple<string, Stream>[] attachments = Attachments.Children
                    .OfType<AttachmentControl>()
                    .Select(a => new Tuple<string, Stream>(a.FileName, a.Stream!))
                    .ToArray();

            await App.Client!.ReplyMessageAsync(_originalMessage, ViewModel.Subject, ViewModel.Content.Replace(Environment.NewLine, "<br>"), attachments);
        }
        catch (Exception ex)
        {
            ex.HandleWithDefaultHandler(ViewModel, "Send reply");
        }
        finally
        {
            ReleaseAllAttachments();
        }
    }

    private void ReleaseAllAttachments()
    {
        foreach (Stream stream in Attachments.Children
            .OfType<AttachmentControl>()
            .Where(a => a.Stream is not null)
            .Select(a => a.Stream!))
            stream.Dispose();
    }

    private void RenderReplyHistory(List<Message> history)
    {
        foreach (Message message in history)
            ReplyHistory.Children.Add(new ReplyMessage(message));
    }

    private void RenderConfirmationBox(ConfirmationInformations? confirmationInformation)
    {
        if (confirmationInformation is not null)
        {
            if (confirmationInformation.AllowSendRequestConfirmation)
                ViewModel.CanSendRequestConfirmation = true;
            else
            {
                ViewModel.ConfirmationDateString = LangHelper.GetString("MessageWindow.CM", confirmationInformation.ConfirmationDate.ToString("d"));
                ViewModel.IsConfirmationMessage = true;
            }
        }
    }

    private static string ConvertToSavingUnit(long size)
    {
        string[] sizes = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        int order = 0;
        double fileSize = size;

        while (fileSize >= 1024 && order < sizes.Length - 1)
        {
            order++;
            fileSize /= 1024;
        }

        return string.Format("{0:0.##} {1}", fileSize, sizes[order]);
    }
}
