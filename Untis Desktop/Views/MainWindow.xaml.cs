using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using UntisDesktop.ViewModels;

namespace UntisDesktop.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();

        ViewModel.PropertyChanged += ErrorBoxContent_PropertyChanged;
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
        if (e.PropertyName == nameof(MainWindowViewModel.ErrorBoxContent))
        {
            if (ViewModel.ErrorBoxContent != string.Empty)
            {
                Storyboard popupAnimation = (Storyboard)ErrorBox.FindResource("PopUpAnimation");
                popupAnimation.AutoReverse = false;
                popupAnimation.Begin();
            }
        }
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