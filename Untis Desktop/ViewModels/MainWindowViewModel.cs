using Data.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using UntisDesktop.Localization;
using UntisDesktop.Views;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Exceptions;
using WebUntisAPI.Client.Models;

namespace UntisDesktop.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{
    // Commands
    public DelegateCommand ReloadOfflineCommand { get; }

    public DelegateCommand ReloadTabCommand { get; }

    public DelegateCommand OtherWeekBtnCommand { get; }

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

    public bool IsOffline
    {
        get => _isOffline;
        set
        {
            if (_isOffline != value)
            {
                _isOffline = value;
                RaisePropertyChanged();
            }
        }
    }
    private bool _isOffline = false;

    // General
    public DateTime CurrentDate { get => DateTime.Now.ToLocalTime(); }

    public ProfileFile CurrentProfile { get => ProfileCollection.GetActiveProfile(); }

    #region Today
    public async Task LoadTodayTabAsync()
    {
        if (IsOffline)
            return;

        try
        {
            using WebUntisClient client = await CurrentProfile.LoginAsync(CancellationToken.None);

            Task<int> unreadCountTask = client.GetUnreadNewsCountAsync();
            Task<News> newsTask = client.GetNewsFeedAsync(DateTime.Now);

            IsUnreadNewsAvailable = await unreadCountTask.ConfigureAwait(true) > 0;
            TodayNews = await newsTask.ConfigureAwait(true);
        }
        catch (WebUntisException ex)
        {
            switch (ex.Code)
            {
                case (int)WebUntisException.Codes.NoRightForMethod:
                    ErrorBoxContent = LangHelper.GetString("App.Err.WU.NRFM");
                    Logger.LogWarning($"Today tab loading: {nameof(WebUntisException)} {nameof(WebUntisException.Codes.NoRightForMethod)}");
                    break;
                case (int)WebUntisException.Codes.NotAuthticated:
                    ErrorBoxContent = LangHelper.GetString("App.Err.WU.NA");
                    Logger.LogWarning($"Today tab loading: {nameof(WebUntisException)} {nameof(WebUntisException.Codes.NotAuthticated)}");
                    break;
                default:
                    ErrorBoxContent = LangHelper.GetString("App.Err.WU", ex.Message);
                    Logger.LogError($"Today tab loading: Unexpected {nameof(WebUntisException)} Message: {ex.Message}, Code: {ex.Code}");
                    break;
            }
        }
        catch (HttpRequestException ex)
        {
            if (ex.Source == "System.Net.Http" && ex.StatusCode is null)
            {
                ErrorBoxContent = LangHelper.GetString("App.Err.NNC");
                IsOffline = true;
            }
            else
                ErrorBoxContent = LangHelper.GetString("App.Err.NERR", ex.Message, ((int?)ex.StatusCode)?.ToString() ?? "0");
            Logger.LogWarning($"Today tab loading: {nameof(HttpRequestException)} Code: {ex.StatusCode}, Message: {ex.Message}");
        }
        catch (Exception ex)
        {
            ErrorBoxContent = LangHelper.GetString("App.Err.OEX", ex.Source ?? "System.Exception", ex.Message);
            Logger.LogError($"Today tab loading: {ex.Source ?? "System.Exception"}; {ex.Message}");
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

    #region Timetable
    public DateTime SelectedWeek
    {
        get => MainWindow.SelectedWeek;
        set
        {
            MainWindow.SelectedWeek = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(NextWeek));
        }

    }
    public DateTime NextWeek { get => SelectedWeek.AddDays(6); }
    #endregion

    public MainWindowViewModel()
    {
        ReloadOfflineCommand = new(async _ =>
        {
            using Ping ping = new();
            try
            {
                await ping.SendPingAsync("google.de");
                IsOffline = false;
            }
            catch
            {
                IsOffline = true;
                return;
            }
        });

        ReloadTabCommand = new(async parameter =>
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

        OtherWeekBtnCommand = new(parameter =>
        {
            if (int.TryParse(parameter as string, out int result))
            {
                SelectedWeek = SelectedWeek.AddDays(result);
            }
        });

        _ = Task.Run(async () =>
        {
            using Ping ping = new();
            try
            {
                await ping.SendPingAsync("google.de");
            }
            catch
            {
                IsOffline = true;
                return;
            }

            Task loadTodayTask = LoadTodayTabAsync();
        });
    }
}
