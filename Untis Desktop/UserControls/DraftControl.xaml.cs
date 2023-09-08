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
            catch (Exception ex)
            {
                Window window = Window.GetWindow(this);
                ex.HandleWithDefaultHandler((IWindowViewModel)window.DataContext, "Draft deletion");
            }

            await viewModel.LoadMailTabAsync();
        }
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            new MessageWindow(Draft).Show();
            e.Handled = true;
        }
    }
}