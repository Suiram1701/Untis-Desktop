using Data.Messages;
using Data.Profiles;
using Data.Timetable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using UntisDesktop.Extensions;
using UntisDesktop.Localization;
using UntisDesktop.UserControls;
using UntisDesktop.Views;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Exceptions;
using WebUntisAPI.Client.Models;
using WebUntisAPI.Client.Models.Messages;

namespace UntisDesktop.ViewModels;

internal class MainWindowViewModel : ViewModelBase, IWindowViewModel
{
    // Commands
    public DelegateCommand ReloadOfflineCommand { get; }

    public DelegateCommand ReloadTabCommand { get; }

    public DelegateCommand OtherWeekBtnCommand { get; }

    public DelegateCommand ReloadMailsCommand { get; }

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
                RaisePropertyChanged(nameof(ViewTimetableReloadBtn));
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

        await (App.Client!.RunWithDefaultExceptionHandler(async client =>
        {
            IsUnreadNewsAvailable = (await client.GetUnreadNewsCountAsync()) > 0;
            TodayNews = await client.GetNewsFeedAsync(DateTime.Now);
        }, this, "Today tab loading") ?? Task.CompletedTask);
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
    public string CurrentSchoolYearString
    {
        get
        {
            string? schoolYearStr = SchoolYearFile.s_DefaultInstance[SelectedWeek]?.Name;
            schoolYearStr ??= SchoolYearFile.s_DefaultInstance[NextWeek]?.Name;

            return schoolYearStr ?? LangHelper.GetString("MainWindow.Timetable.NASY");
        }
    }

    public DateTime SelectedWeek
    {
        get => MainWindow.SelectedWeek;
        set
        {
            MainWindow.SelectedWeek = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(NextWeek));
            RaisePropertyChanged(nameof(CurrentSchoolYearString));
        }

    }
    public DateTime NextWeek { get => SelectedWeek.AddDays(6); }

    public bool ReloadTimetable { get; set; }

    public bool IsTimetableLoading
    {
        get => _isTimetableLoading;
        set
        {
            _isTimetableLoading = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(ViewTimetableReloadBtn));
        }
    }
    private bool _isTimetableLoading = false;

    public bool ViewTimetableReloadBtn { get => !IsOffline && !IsTimetableLoading; }
    #endregion

    #region Messages
    public async Task LoadMailTabAsync()
    {
        if (IsOffline)
            return;

        IsMailsLoading = true;

        MsgInbox.Clear();
        RaisePropertyChanged(nameof(MsgInbox));
        ConfirmationMsgInbox.Clear();
        RaisePropertyChanged(nameof(ConfirmationMsgInbox));
        SentMsg.Clear();
        RaisePropertyChanged(nameof(SentMsg));
        DraftMessages.Clear();
        RaisePropertyChanged(nameof(DraftMessages));

        Task<(MessagePreview[] inbox, MessagePreview[] confirmationMessages)> inboxTask = App.Client!.GetMessageInboxAsync();

        Task<MessagePreview[]> sentTask = Task.FromResult(Array.Empty<MessagePreview>());
        if (ShowSentTab)
            sentTask = App.Client.GetSentMessagesAsync();

        Task<DraftPreview[]> draftsTask = Task.FromResult(Array.Empty<DraftPreview>());
        if (ShowDraftsTab)
            draftsTask = App.Client.GetSavedDraftsAsync();

        await Task.WhenAll(inboxTask, sentTask, draftsTask).ConfigureAwait(true);

        foreach (MessagePreview preview in inboxTask.Result.inbox)
            MsgInbox.Add(preview);
        RaisePropertyChanged(nameof(MsgInbox));

        foreach (MessagePreview preview in inboxTask.Result.confirmationMessages)
            ConfirmationMsgInbox.Add(preview);
        RaisePropertyChanged(nameof(ConfirmationMsgInbox));

        foreach (MessagePreview preview in sentTask.Result)
            SentMsg.Add(preview);
        RaisePropertyChanged(nameof(SentMsg));

        foreach (DraftPreview preview in draftsTask.Result)
            DraftMessages.Add(preview);
        RaisePropertyChanged(nameof(DraftMessages));

        IsMailsLoading = false;
    }

    // Visibility
    public bool ShowSentTab { get => MessagePermissionsFile.s_DefaultInstance.Permissions.ShowSentTab; }

    public bool ShowDraftsTab { get => MessagePermissionsFile.s_DefaultInstance.Permissions.ShowDraftsTab; }

    // Messages
    public List<MessagePreview> MsgInbox { get; set; } = new();

    public List<MessagePreview> ConfirmationMsgInbox { get; set; } = new();

    public bool IsConfirmationMessagesAvailable { get => ConfirmationMsgInbox.Any(); }

    public List<MessagePreview> SentMsg { get; set; } = new();

    public List<DraftPreview> DraftMessages { get; set; } = new();

    // Loading
    public bool ReloadMail { get; set; }

    public bool IsMailsLoading
    {
        get => _isMailsLoading;
        set
        {
            _isMailsLoading = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(ViewMailsReloadBtn));
        }
    }
    private bool _isMailsLoading = false;

    public bool ViewMailsReloadBtn { get => !IsOffline && !IsMailsLoading; }
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
                case "MailBtn":
                    await LoadMailTabAsync().ConfigureAwait(true);
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
                if (result == 0)
                    ReloadTimetable = true;
                
                SelectedWeek = SelectedWeek.AddDays(result);
            }
        });

        ReloadMailsCommand = new(async parameter =>
        {
            if (IsOffline)
                return;

            IsMailsLoading = true;

            switch (parameter)
            {
                case 0:     // Inbox tab
                    MsgInbox.Clear();
                    RaisePropertyChanged(nameof(MsgInbox));
                    ConfirmationMsgInbox.Clear();
                    RaisePropertyChanged(nameof(ConfirmationMsgInbox));
                    break;
                case 1:     // Sent tab
                    SentMsg.Clear();
                    RaisePropertyChanged(nameof(SentMsg));
                    break;
                case 2:     // Drafts tab
                    DraftMessages.Clear();
                    RaisePropertyChanged(nameof(DraftMessages));
                    break;
                default:
                    return;
            }

            switch (parameter)
            {
                case 0:     // Inbox tab
                    (MessagePreview[] inbox, MessagePreview[] confirmationPreview) = await App.Client!.GetMessageInboxAsync(CancellationToken.None);
                    foreach (MessagePreview inboxMsg in inbox)
                        MsgInbox.Add(inboxMsg);
                    RaisePropertyChanged(nameof(MsgInbox));

                    foreach (MessagePreview confirmMsg in confirmationPreview)
                        ConfirmationMsgInbox.Add(confirmMsg);
                    RaisePropertyChanged(nameof(ConfirmationMsgInbox));
                    break;
                case 1:     // Sent tab
                    MessagePreview[] sentMessages = await App.Client!.GetSentMessagesAsync(CancellationToken.None);
                    foreach (MessagePreview sentMsg in sentMessages)
                        SentMsg.Add(sentMsg);
                    RaisePropertyChanged(nameof(SentMsg));
                    break;
                case 2:     // Drafts tab
                    DraftPreview[] draftMessages = await App.Client!.GetSavedDraftsAsync(CancellationToken.None);
                    foreach (DraftPreview draftMsg in draftMessages)
                        DraftMessages.Add(draftMsg);
                     RaisePropertyChanged(nameof(DraftMessages));
                    break;
            }

            IsMailsLoading = false;
        });

        _ = Task.Run(async () =>
        {
            using Ping ping = new();
            try
            {
                App.Client = await ProfileCollection.GetActiveProfile().LoginAsync(CancellationToken.None);
            }
            catch
            {
                IsOffline = true;
            }
        });
    }
}
