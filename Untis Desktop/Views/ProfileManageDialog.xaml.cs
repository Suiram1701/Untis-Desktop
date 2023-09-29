using Data.Profiles;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Data.Timetable;
using UntisDesktop.Extensions;
using UntisDesktop.Localization;

namespace UntisDesktop.Views;

public partial class ProfileManageDialog : Window
{
    private ProfileManageDialogViewModel ViewModel { get => (ProfileManageDialogViewModel)DataContext; }

    public ProfileManageDialog()
    {
        InitializeComponent();

        foreach (ProfileFile profile in ProfileCollection.s_DefaultInstance)
        {
            ProfileControl control = new(profile);
            control.Switch += ProfileSwitchBtn_ClickAsync;
            control.Delete += ProfileDeleteBtn_ClickAsync;

            Profiles.Children.Add(control);
        }
    }

    private void ProfileAddBtn_Click(object sender, RoutedEventArgs e)
    {
        new LoginWindow().ShowDialog();
        DialogResult = true;
        e.Handled = true;
    }

    private async void ProfileSwitchBtn_ClickAsync(object sender, RoutedEventArgs e)
    {
        ProfileControl profileControl = (ProfileControl)sender;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;

            await App.Client!.LogoutAsync();
            App.Client.Dispose();

            // Login to the profile
            App.Client = await profileControl.ProfileFile.LoginAsync(CancellationToken.None);
            await ProfileCollection.SetActiveProfileAsync(profileControl.ProfileFile, App.Client);
        }
        catch (Exception ex)
        {
            ex.HandleWithDefaultHandler(ViewModel, "Change profile");
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        DialogResult = true;
        e.Handled = true;
    }

    private async void ProfileDeleteBtn_ClickAsync(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(LangHelper.GetString("ProfileManageDialog.D.C"), LangHelper.GetString("ProfileManageDialog.D.T"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            e.Handled = true;
            return;
        }

        ProfileControl profileControl = (ProfileControl)sender;
        try
        {
            profileControl.ProfileFile.Delete();
            ProfileCollection.s_DefaultInstance.ReloadCollection();

            // Close the app when it was the last profile
            if (!ProfileCollection.s_DefaultInstance.Any())
            {
                Application.Current.Shutdown(0);
                return;
            }

            // When the active profile was deleted select another profile
            if (ProfileCollection.s_DefaultInstance.All(p => !p.IsActive))
            {
                ProfileFile activeProfile = ProfileCollection.s_DefaultInstance.First();

                Mouse.OverrideCursor = Cursors.Wait;

                await App.Client!.LogoutAsync();
                App.Client.Dispose();

                // Login to the profile
                App.Client = await activeProfile.LoginAsync(CancellationToken.None);
                await ProfileCollection.SetActiveProfileAsync(activeProfile, App.Client);
            }
        }
        catch (Exception ex)
        {
            ex.HandleWithDefaultHandler(ViewModel, "Delete profile");
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

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

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        Close();
        e.Handled = true;
    }
}