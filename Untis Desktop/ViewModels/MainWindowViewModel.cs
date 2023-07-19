using Data.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Models;

namespace UntisDesktop.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{
    // Commands
    public DelegateCommand ReloadTabCommand { get; }

    // views
    public string ErrorBoxContent
    {
        get => _errorBoxContent;
        set
        {
            _errorBoxContent = value;
            RaisePropertyChanged();
        }
    }
    private string _errorBoxContent = string.Empty;

    // General
    public DateTime CurrentDate { get => DateTime.Now.ToLocalTime(); }

    public ProfileFile CurrentProfile { get => ProfileCollection.GetActiveProfile(); }

    #region Today
    public async Task LoadTodayTabAsync()
    {
        try
        {
            using WebUntisClient client = await CurrentProfile.LoginAsync(CancellationToken.None);

            Task<int> unreadCountTask = client.GetUnreadNewsCountAsync();
            Task<News> newsTask = client.GetNewsFeedAsync(DateTime.Now);

            IsUnreadNewsAvailable = await unreadCountTask.ConfigureAwait(true) > 0;
            TodayNews = await newsTask.ConfigureAwait(true);
        }
        catch (Exception)
        {

        }

    }

    public bool IsUnreadNewsAvailable
    {
        get => _isUnreadNewsAvailable;
        set
        {
            if (value != _isUnreadNewsAvailable)
            {
                _isUnreadNewsAvailable = value;
                RaisePropertyChanged();
            }
        }
    }
    private bool _isUnreadNewsAvailable = false;

    public News TodayNews
    {
        get => _todayNews;
        set
        {
            if (TodayNews != _todayNews)
            {
                _todayNews = TodayNews;
                RaisePropertyChanged();
                RaiseErrorsChanged(nameof(IsSysNewsAvailable));
                RaisePropertyChanged(nameof(IsNewsAvailable));
                RaisePropertyChanged(nameof(IsAnyNewsAvailable));
            }
        }
    }
    private News _todayNews = new();

    public bool IsSysNewsAvailable { get => TodayNews.SystemMessage is not null; }
    public bool IsNewsAvailable { get => TodayNews.Messages?.Any() ?? false; }
    public bool IsAnyNewsAvailable { get => TodayNews.Messages is not null && IsNewsAvailable; }
    #endregion

    public MainWindowViewModel()
    {
        ReloadTabCommand = new DelegateCommand(async parameter =>
        {
            string targetName = (string)parameter;
            switch (targetName)
            {
                case "TodayBtn":
                    await LoadTodayTabAsync().ConfigureAwait(true);
                    break;
                case "TimetableBtn":
                    break;
                case "MailBtn":
                    break;
                case "SettingsBtn":
                    break;
                case "ProfileBtn":
                    break;
            }
        });

        _ = Application.Current.Dispatcher.Invoke(async () =>
        {
            await LoadTodayTabAsync().ConfigureAwait(true);
        }, DispatcherPriority.Loaded);
    }
}
