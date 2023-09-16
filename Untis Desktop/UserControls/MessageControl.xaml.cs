using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UntisDesktop.Extensions;
using UntisDesktop.Localization;
using UntisDesktop.ViewModels;
using UntisDesktop.Views;
using WebUntisAPI.Client.Exceptions;
using WebUntisAPI.Client.Models.Messages;

namespace UntisDesktop.UserControls;

public partial class MessageControl : UserControl
{
    public static DateTime PlaceholderTime { get => new(2023, 12, 14); }

    public MessagePreview Message { get; set; }

    public string DisplayedName { get => Message.Sender?.DisplayName ?? Message.Recipients.Select(r => r.DisplayName).FirstOrDefault("Err"); }

    public string DisplayedProfileImg
    {
        get
        {
            StringBuilder sb = new();
            foreach (char c in DisplayedName.Split(' ').Select(s => s[0]).Take(2))
                sb.Append(c);
            return sb.ToString();
        }
    }

    public bool IsSentMessage { get; set; }

    public MessageControl(MessagePreview message, bool isSentMessage = false)
    {
        Message = message;
        IsSentMessage = isSentMessage;
        InitializeComponent();
    }

    private async void Delete_ClickAsync(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(LangHelper.GetString("MainWindow.Mail.Del.Text"), LangHelper.GetString("MainWindow.Mail.Del.Title"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            MainWindowViewModel viewModel = (MainWindowViewModel)Application.Current.Windows.Cast<Window>().OfType<MainWindow>().First().DataContext;
            if (viewModel.IsOffline)
                return;

            try
            {
                await App.Client!.DeleteMessageAsync(Message);
            }
            catch (WebUntisException ex)
            {
                switch (ex.Code)
                {
                    case (int)WebUntisException.Codes.NoRightForMethod:
                        viewModel.ErrorBoxContent = LangHelper.GetString("App.Err.WU.NRFM");
                        Logger.LogWarning($"Message deletion: {nameof(WebUntisException)} {nameof(WebUntisException.Codes.NoRightForMethod)}");
                        break;
                    case (int)WebUntisException.Codes.NotAuthticated:
                        viewModel.ErrorBoxContent = LangHelper.GetString("App.Err.WU.NA");
                        Logger.LogWarning($"Message deletion: {nameof(WebUntisException)} {nameof(WebUntisException.Codes.NotAuthticated)}");
                        break;
                    default:
                        viewModel.ErrorBoxContent = LangHelper.GetString("App.Err.WU", ex.Message);
                        Logger.LogError($"Message deletion: Unexpected {nameof(WebUntisException)} Message: {ex.Message}, Code: {ex.Code}");
                        break;
                }
                return;
            }
            catch (HttpRequestException ex)
            {
                if (ex.Source == "System.Net.Http" && ex.StatusCode is null)
                    viewModel.IsOffline = true;
                else
                    viewModel.ErrorBoxContent = LangHelper.GetString("App.Err.NERR", ex.Message, ((int?)ex.StatusCode)?.ToString() ?? "0");
                Logger.LogWarning($"Message deletion: {nameof(HttpRequestException)} Code: {ex.StatusCode}, Message: {ex.Message}");
                return;
            }
            catch (Exception ex) when (ex.Source == "System.Net.Http")
            {
                viewModel.IsOffline = true;
                return;
            }
            catch (Exception ex)
            {
                viewModel.ErrorBoxContent = LangHelper.GetString("App.Err.OEX", ex.Source ?? "System.Exception", ex.Message);
                Logger.LogError($"Message deletion: {ex.Source ?? "System.Exception"}; {ex.Message}");
                return;
            }

            await viewModel.LoadMailTabAsync();
        }
    }

    private async void Revoke_ClickAsync(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(LangHelper.GetString("MainWindow.Mail.Revoke.Text"), LangHelper.GetString("MainWindow.Mail.Revoke.Title"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            MainWindowViewModel mainWindowViewModel = (MainWindowViewModel)Window.GetWindow(this).DataContext;

            if (mainWindowViewModel.IsOffline)
                return;

            try
            {
                await App.Client!.RevokeMessageAsync(Message);
            }
            catch (Exception ex)
            {
                ex.HandleWithDefaultHandler(mainWindowViewModel, "Message revoke");
            }

            await mainWindowViewModel.LoadMailTabAsync();

            e.Handled = true;
        }
    }

    private async void Reply_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            Message replyMessage = await App.Client!.GetReplyFormAsync(Message);
            new MessageWindow(replyMessage).Show();
        }
        catch (Exception ex)
        {
            IWindowViewModel viewModel = (IWindowViewModel)Window.GetWindow(this).DataContext;
            ex.HandleWithDefaultHandler(viewModel, "Reply Message");
        }

        e.Handled = true;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            new MessageWindow(Message).Show();
            e.Handled = true;
        }
    }
}