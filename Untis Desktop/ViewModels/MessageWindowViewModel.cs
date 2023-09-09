using Data.Messages;
using Data.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using UntisDesktop.Localization;
using WebUntisAPI.Client.Models.Messages;

namespace UntisDesktop.ViewModels;

internal class MessageWindowViewModel : ViewModelBase, IWindowViewModel
{
    // Commands
    public DelegateCommand ReloadOfflineCommand { get; }

    public DelegateCommand ToggleReplyCommand { get; }

    public DelegateCommand ToggleRequestReadConfirmationCommand { get; }

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

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            if (value != _isReadOnly)
            {
                _isReadOnly = value;
                RaisePropertyChanged();
            }
        }
    }
    private bool _isReadOnly = false;

    public DateTime SentDate
    {
        get => _sentDate;
        set
        {
            if (_sentDate != value)
            {
                _sentDate = value;
                RaisePropertyChanged();
            }
        }
    }
    private DateTime _sentDate = DateTime.Now;

    public string Subject
    {
        get => _subject;
        set
        {
            if (value != _subject)
            {
                _subject = value;
                RaisePropertyChanged();
            }
        }
    }
    private string _subject = LangHelper.GetString("MessageWindow.DTitle");

    public bool HasAttachments
    {
        get => _hasAttachments;
        set
        {
            if (value != _hasAttachments)
            {
                _hasAttachments = value;
                RaisePropertyChanged();
            }
        }
    }
    private bool _hasAttachments = true;

    public bool IsReplyForm
    {
        get => _isReplyForm;
        set
        {
            if (value != _isReplyForm)
            {
                _isReplyForm = value;
                RaisePropertyChanged();
            }
        }
    }
    private bool _isReplyForm = false;

    public string RecipientType
    {
        get => _recipientType;
        set
        {
            if (_recipientType != value)
            {
                _recipientType = value;
                RaisePropertyChanged();
            }
        }
    }
    private string _recipientType = LangHelper.GetString("MessageWindow.R");

    public string Content
    {
        get => _content;
        set
        {
            if (value != _content)
            {
                _content = value;
                RaisePropertyChanged();
            }
        }
    }
    private string _content = string.Empty;

    public static bool CanForbidReply { get => MessagePermissionsFile.s_DefaultInstance.Permissions.CanForbidReplies; }

    public static bool CanRequestReadConfirmation { get => MessagePermissionsFile.s_DefaultInstance.Permissions.AllowRequestReadConfirmation; }

    public bool ForbidReply
    {
        get => _forbidReply;
        set
        {
            if (_forbidReply != value)
            {
                _forbidReply = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ForbidReplyColor));
            }
        }
    }
    private bool _forbidReply = false;

    public bool RequestReadConfirmation
    {
        get => _requestReadConfirmation;
        set
        {
            if (_requestReadConfirmation != value)
            {
                _requestReadConfirmation = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(RequestReadConfirmationColor));
            }
        }
    }
    private bool _requestReadConfirmation = false;

    public Brush ForbidReplyColor
    {
        get => ForbidReply
            ? (Brush)Application.Current.FindResource("PressedDarkBtnColor")
            : Brushes.White;
    }

    public Brush RequestReadConfirmationColor
    {
        get => RequestReadConfirmation
            ? (Brush)Application.Current.FindResource("PressedDarkBtnColor")
            : Brushes.White;
    }

    public List<MessagePerson> Recipients = new();

    public MessageWindowViewModel()
    {
        ReloadOfflineCommand = new(async _ =>
        {
            try
            {
                App.Client = await ProfileCollection.GetActiveProfile().LoginAsync(CancellationToken.None);
                IsOffline = false;
            }
            catch
            {
                IsOffline = true;
            }
        });

        ToggleReplyCommand = new(_ => ForbidReply = !ForbidReply);

        ToggleRequestReadConfirmationCommand = new(_ => RequestReadConfirmation = !RequestReadConfirmation);
    }
}
