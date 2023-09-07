using Data.Messages;
using Data.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UntisDesktop.Extensions;
using UntisDesktop.Localization;
using WebUntisAPI.Client.Models.Messages;

namespace UntisDesktop.ViewModels;

internal class RecipientDialogViewModel : ViewModelBase, IWindowViewModel
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

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (value != _searchText)
            {
                _searchText = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ViewSearchTextPlaceHolder));
                _ = ApplyFiltersAsync();
            }
        }
    }
    private string _searchText = string.Empty;

    public bool ViewSelectedRecipients
    {
        get => _viewSelectedRecipients;
        set
        {
            if (_viewSelectedRecipients != value)
            {
                _viewSelectedRecipients = value;
                RaisePropertyChanged();
            }
        }
    }
    private bool _viewSelectedRecipients = false;

    public bool ViewNotFound { get => !AvailablePeople.Any(); }

    public Dictionary<string, MessagePerson[]> AvailablePeople = new();

    public Dictionary<string, List<FilterItem>> Filters = new();

    public string SearchTextPlaceHolder { get => LangHelper.GetString("RecipientDialog.S." + CurrentRecipientOption); }

    public bool ViewSearchTextPlaceHolder { get => string.IsNullOrEmpty(SearchText); }

    public bool ViewExtendedFilterOptions { get => CurrentRecipientOption == "STAFF"; }

    public string CurrentRecipientOption
    {
        get => _currentRecipientOption;
        set
        {
            if (value != _currentRecipientOption)
            {
                _currentRecipientOption = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SearchTextPlaceHolder));
                RaisePropertyChanged(nameof(ViewExtendedFilterOptions));
            }
        }
    }
    private string _currentRecipientOption = MessagePermissionsFile.s_DefaultInstance.Permissions.RecipientOptions.First();

    public RecipientDialogViewModel()
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

    public async Task ApplyFiltersAsync()
    {
        AvailablePeople.Clear();
        RaisePropertyChanged(nameof(AvailablePeople));

        switch (CurrentRecipientOption)
        {
            case "TEACHER":
                try
                {
                    AvailablePeople = await App.Client!.GetMessagePeopleAsync();
                }
                catch (Exception ex)
                {
                    ex.HandleWithDefaultHandler(this, "Apply Recipient filter (TEACHER)");
                }

                // search filter
                if (!string.IsNullOrEmpty(SearchText))
                {
                    Dictionary<string, MessagePerson[]> sorted = new();
                    foreach ((string type, MessagePerson[] people) in AvailablePeople)
                    {
                        IEnumerable<MessagePerson> filtertPeople = people.Where(FilterBySearchText);
                        if (filtertPeople.Any())
                            sorted.Add(type, filtertPeople.ToArray());
                    }
                    AvailablePeople = sorted;
                }
                break;
            case "STAFF":
                try
                {
                    Dictionary<string, FilterItem[]> sorted = new();
                    foreach ((string type, List<FilterItem> items) in Filters)
                        sorted.Add(type, items.ToArray());

                    AvailablePeople = new()
                    {
                        {
                            string.Empty,
                            await App.Client!.GetStaffFilterSearchResultAsync(SearchText, sorted)
                        }
                    };
                }
                catch (Exception ex)
                {
                    ex.HandleWithDefaultHandler(this, "Apply Recipient filter (STAFF)");
                }
                break;
            default:
                break;
        }

        RaisePropertyChanged(nameof(AvailablePeople));
        RaisePropertyChanged(nameof(ViewNotFound));
    }

    private bool FilterBySearchText(MessagePerson person)
    {
        string normalizedSearchText = SearchText.ToLower();
        return person.DisplayName.ToLower().Contains(normalizedSearchText)
            || (person.ClassName?.ToLower().Contains(normalizedSearchText) ?? false)
            || (person.Role?.ToLower().Contains(normalizedSearchText) ?? false)
            || person.Tags.Any(t => t.ToLower().Contains(normalizedSearchText));
    }
}