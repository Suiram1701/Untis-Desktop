using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
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
using UntisDesktop.ViewModels;

namespace UntisDesktop.Views;
/// <summary>
/// Interaktionslogik für LoginWindow.xaml
/// </summary>
public partial class LoginWindow : Window
{
    private LoginWindowViewModel ViewModel => (LoginWindowViewModel)DataContext;

    public LoginWindow()
    {
        try
        {
            using Ping ping = new();
            ping.Send("https://www.google.com");
        }
        catch (PingException)
        {
            Logger.LogWarning("No internet connection available");
            MessageBox.Show(LangHelper.GetString("LoginWindow.Err.NIC"), LangHelper.GetString("LoginWindow.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
            Application.Current.Shutdown(0);
        }

        InitializeComponent();

        ViewModel.PropertyChanged += ErrorBoxContent_PropertyChanged;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        PasswordBox pwdBox = (PasswordBox)sender;
        ViewModel.Password = pwdBox.Password;
        
        e.Handled = true;
    }

    private void PasswordBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            PasswordBox pwdBox = (PasswordBox)sender;
            pwdBox.Password = ViewModel.Password;
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

    private void ErrorBoxContent_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LoginWindowViewModel.ErrorBoxContent))
        {
            if (ViewModel.ErrorBoxContent != string.Empty)
            {
                Storyboard popupAnimation = (Storyboard)ErrorBox.FindResource("PopUpAnimation");
                popupAnimation.AutoReverse = false;
                popupAnimation.Begin();
            }
        }
    }
}
