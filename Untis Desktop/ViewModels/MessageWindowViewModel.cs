﻿using Data.Messages;
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

internal class MessageWindowViewModel : WindowViewModelBase
{
    // Commands

    public DelegateCommand ToggleReplyCommand { get; }

    public DelegateCommand ToggleRequestReadConfirmationCommand { get; }

    public bool IsConfirmationMessage
    {
        get => _isConfirmationMessage;
        set
        {
            if (_isConfirmationMessage != value)
            {
                _isConfirmationMessage = value;
                RaisePropertyChanged();
            }
        }
    }
    private bool _isConfirmationMessage = false;

    public string ConfirmationDateString
    {
        get => _confirmationDateString;
        set
        {
            if (_confirmationDateString != value)
            {
                _confirmationDateString = value;
                RaisePropertyChanged();
            }
        }
    }
    private string _confirmationDateString = string.Empty;

    public bool CanSendRequestConfirmation
    {
        get => _canSendRequestConfirmation;
        set
        {
            if (_canSendRequestConfirmation != value)
            {
                _canSendRequestConfirmation = value;
                RaisePropertyChanged();
            }
        }
    }
    private bool _canSendRequestConfirmation = false;

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            if (value != _isReadOnly)
            {
                _isReadOnly = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ShowRecipientAdd));
                RaisePropertyChanged(nameof(CanSaveAsDraft));
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
                RaisePropertyChanged(nameof(ShowRecipientAdd));
                RaisePropertyChanged(nameof(CanSaveAsDraft));
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

    public bool ShowRecipientAdd { get => !IsReplyForm && !IsReadOnly; }

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

    public bool CanSaveAsDraft { get => !IsReadOnly && !IsReplyForm; }

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

    public MessageWindowViewModel() : base()
    {
        ToggleReplyCommand = new(_ => ForbidReply = !ForbidReply);

        ToggleRequestReadConfirmationCommand = new(_ => RequestReadConfirmation = !RequestReadConfirmation);
    }
}
