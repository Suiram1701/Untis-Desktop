using Data.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UntisDesktop.Localization;
using WebUntisAPI.Client.Models.Messages;

namespace UntisDesktop.ViewModels;

internal class RecipientDialogViewModel : ViewModelBase, IWindowViewModel
{
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

    public Dictionary<string, MessagePerson[]> AvailablePeople = new();

    public string SearchTextPlaceHolder { get => LangHelper.GetString("RecipientDialog.S." + CurrentRecipientOption); }

    public bool ViewSearchTextPlaceHolder { get => string.IsNullOrEmpty(SearchText); }

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
            }
        }
    }
    private string _currentRecipientOption = MessagePermissionsFile.s_DefaultInstance.Permissions.RecipientOptions.First();

    public async Task ApplyFiltersAsync()
    {
        AvailablePeople.Clear();
        RaisePropertyChanged(nameof(AvailablePeople));

        switch (CurrentRecipientOption)
        {
            case "TEACHER":
                AvailablePeople = (await App.Client!.GetMessagePeopleAsync());

                // Filter
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
                break;
            default:
                break;
        }

        RaisePropertyChanged(nameof(AvailablePeople));
    }

    private bool FilterBySearchText(MessagePerson person)
    {
        string normalizedSearchText = SearchText.ToLower();
        return person.DisplayName.ToLower().Contains(normalizedSearchText)
            || (person.ClassName?.ToLower().Contains(normalizedSearchText) ?? false)
            || person.Tags.Any(t => t.ToLower().Contains(normalizedSearchText));
    }
}