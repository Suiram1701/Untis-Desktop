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

namespace UntisDesktop.UserControls
{
    /// <summary>
    /// Interaktionslogik für DraftControl.xaml
    /// </summary>
    public partial class DraftControl : UserControl
    {
        public DraftPreview Draft { get; set; }

        public DraftControl(DraftPreview draft)
        {
            Draft = draft;
            InitializeComponent();
        }

        private async void Delete_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(LangHelper.GetString("MainWindow.Mail.Del.D.Text"), LangHelper.GetString("MainWindow.Mail.Del.D.Title"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                MainWindowViewModel viewModel = (MainWindowViewModel)Application.Current.Windows.Cast<Window>().OfType<MainWindow>().First().DataContext;
                if (viewModel.IsOffline)
                    return;

                try
                {
                    await App.Client!.DeleteDraftAsync(Draft);
                }
                catch (WebUntisException ex)
                {
                    switch (ex.Code)
                    {
                        case (int)WebUntisException.Codes.NoRightForMethod:
                            viewModel.ErrorBoxContent = LangHelper.GetString("App.Err.WU.NRFM");
                            Logger.LogWarning($"Draft deletion: {nameof(WebUntisException)} {nameof(WebUntisException.Codes.NoRightForMethod)}");
                            break;
                        case (int)WebUntisException.Codes.NotAuthticated:
                            viewModel.ErrorBoxContent = LangHelper.GetString("App.Err.WU.NA");
                            Logger.LogWarning($"Draft deletion: {nameof(WebUntisException)} {nameof(WebUntisException.Codes.NotAuthticated)}");
                            break;
                        default:
                            viewModel.ErrorBoxContent = LangHelper.GetString("App.Err.WU", ex.Message);
                            Logger.LogError($"Draft deletion: Unexpected {nameof(WebUntisException)} Message: {ex.Message}, Code: {ex.Code}");
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
                    Logger.LogWarning($"Draft deletion: {nameof(HttpRequestException)} Code: {ex.StatusCode}, Message: {ex.Message}");
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
                    Logger.LogError($"Draft deletion: {ex.Source ?? "System.Exception"}; {ex.Message}");
                    return;
                }

                await viewModel.LoadMailTabAsync();
            }
        }
    }
}
