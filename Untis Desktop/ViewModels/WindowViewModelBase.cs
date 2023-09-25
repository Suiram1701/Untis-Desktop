using Data.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UntisDesktop.ViewModels;

internal abstract class WindowViewModelBase : ViewModelBase
{
    // Commands
    public DelegateCommand ReloadOfflineCommand { get; }

    // Views
    public virtual string ErrorBoxContent
    {
        get => _errorBoxContent;
        set
        {
            _errorBoxContent = value;
            RaisePropertyChanged();
        }
    }
    private string _errorBoxContent = string.Empty;

    public virtual bool IsOffline
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

    protected WindowViewModelBase()
    {
        ReloadOfflineCommand = new(async _ =>
        {
            try
            {
                App.Client = await ProfileCollection.GetActiveProfile().LoginAsync(CancellationToken.None);
                SetOffline(false);
            }
            catch
            {
                SetOffline(true);
            }
        });
    }
}
