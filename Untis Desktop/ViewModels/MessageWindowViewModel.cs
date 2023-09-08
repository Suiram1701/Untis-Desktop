using Data.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UntisDesktop.Localization;
using WebUntisAPI.Client.Models.Messages;

namespace UntisDesktop.ViewModels;

internal class MessageWindowViewModel : ViewModelBase, IWindowViewModel
{
    // Commands
    public DelegateCommand ReloadOfflineCommand { get; }

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

    public string PersonType
    {
        get => _personType;
        set
        {
            if (_personType != value)
            {
                _personType = value;
                RaisePropertyChanged();
            }
        }
    }
    private string _personType = LangHelper.GetString("MessageWindow.R");

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
    }
}
