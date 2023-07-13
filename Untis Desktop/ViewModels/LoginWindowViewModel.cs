using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UntisDesktop.Views;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Models;

namespace UntisDesktop.ViewModels;

internal class LoginWindowViewModel : ViewModelBase
{
    // views
    public bool IsSchoolSearchPage { get; set; } = true;
    public bool IsLoginPage { get; set; } = false;
    public bool IsExtendedOptions { get; set; } = false;

    // School search
    public string SchoolSearch
    {
        get => _schoolSearch;
        set
        {
            if (_schoolSearch != value)
            {
                _schoolSearch = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsSchoolSearchEmpty));

                Application.Current.Dispatcher.Invoke(async () =>
                {
                    School[]? search = await WebUntisAPI.Client.SchoolSearch.SearchAsync(value, "UntisDesktop_Search");

                    SearchResults.Clear();
                    if (search is School[] schools)
                    {
                        Search_TooManyResults = false;
                        foreach (School school in schools)
                            SearchResults.Add(school);
                    }
                    else
                        Search_TooManyResults = true;

                    RaisePropertyChanged(nameof(SearchResults));
                    RaisePropertyChanged(nameof(Search_TooManyResults));
                    RaisePropertyChanged(nameof(Search_Results));
                    RaisePropertyChanged(nameof(Search_NoResult));
                });
            }
        }
    }
    private string _schoolSearch = string.Empty;

    public bool IsSchoolSearchEmpty => _schoolSearch == string.Empty;

    public ObservableCollection<School> SearchResults { get; set; } = new ObservableCollection<School>();
    public bool Search_TooManyResults { get; set; } = false;
    public bool Search_Results => SearchResults.Count > 0;
    public bool Search_NoResult => SearchResults.Count == 0 && !Search_TooManyResults;

    public LoginWindowViewModel()
    {
    }
}
