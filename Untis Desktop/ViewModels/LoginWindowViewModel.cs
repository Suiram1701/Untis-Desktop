using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UntisDesktop.Localization;
using UntisDesktop.Views;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Models;

namespace UntisDesktop.ViewModels;

internal class LoginWindowViewModel : ViewModelBase
{
    // Commands
    public DelegateCommand BackCommand { get; }

    public DelegateCommand ExtendedOptionsCommand { get; }

    public DelegateCommand PasswordVisibilityCommand { get; }

    public DelegateCommand LoginCommand { get; }

    // views
    public bool IsLoginPage
    {
        get => _isLoginPage;
        set
        {
            if (_isLoginPage != value)
            {
                _isLoginPage = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsSchoolSearchPage));
            }
        }
    }
    public bool IsSchoolSearchPage { get => !IsLoginPage; }
    private bool _isLoginPage = false;

    public bool IsExtendedOptions
    {
        get => _isExtendedOptions;
        set
        {
            if (_isExtendedOptions != value)
            {
                _isExtendedOptions = value;
                RaisePropertyChanged();
            }
        }
    }
    private bool _isExtendedOptions = false;

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
    public bool IsSchoolSearchEmpty { get => _schoolSearch == string.Empty; }
    private string _schoolSearch = string.Empty;

    public ObservableCollection<School> SearchResults { get; set; } = new ObservableCollection<School>();
    public int SelectedIndex
    {
        set
        {
            if (value != -1)
            {
                School school = SearchResults[value];
                DisplaySchoolName = school.DisplayName;
                ServerUrl = school.Server;
                SchoolName = school.LoginName;

                IsLoginPage = true;
            }
        }
    }
    public bool Search_TooManyResults { get; set; } = false;
    public bool Search_Results { get => SearchResults.Count > 0; }
    public bool Search_NoResult { get => SearchResults.Count == 0 && !Search_TooManyResults && !IsSchoolSearchEmpty; }

    // Login data
    public string DisplaySchoolName
    {
        get => _displaySchoolName;
        set
        {
            if (_displaySchoolName != value)
            {
                _displaySchoolName = value;
                RaisePropertyChanged();
            }
        }
    }
    private string _displaySchoolName = string.Empty;

    public string ServerUrl
    {
        get => _serverUrl;
        set
        {
            if (value != _serverUrl)
            {
                _serverUrl = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsServerUrlEmpty));

                // Validation
                ClearErrors();
                Match match = Regex.Match(_serverUrl, @"(?<=https?:[/\\]{2})?[a-zA-Z]+\.webuntis\.com");
                if (!match.Success)
                    AddError(LangHelper.GetString("LoginWindow.Err.NWU"));
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    try
                    {
                        await Dns.GetHostAddressesAsync(match.Value);
                    }
                    catch
                    {
                        AddError(LangHelper.GetString("LoginWindow.Err.WU404"));
                    }
                    RaiseErrorsChanged();
                    LoginCommand.RaiseCanExecuteChanged();
                });
            }
        }
    }
    public bool IsServerUrlEmpty { get => ServerUrl == string.Empty; }
    private string _serverUrl = string.Empty;

    public string SchoolName
    {
        get => _schoolName;
        set
        {
            if (value != _schoolName)
            {
                _schoolName = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsSchoolNameEmpty));
                LoginCommand.RaiseCanExecuteChanged();
            }
        }
    }
    public bool IsSchoolNameEmpty { get => SchoolName == string.Empty; }
    private string _schoolName = string.Empty;

    public string UserName
    {
        get => _userName;
        set
        {
            if (value != _userName)
            {
                _userName = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsUserNameEmpty));
                LoginCommand.RaiseCanExecuteChanged();
            }
        }
    }
    public bool IsUserNameEmpty { get => UserName == string.Empty; }
    private string _userName = string.Empty;

    public string Password
    {
        get => _password;
        set
        {
            if (value != _password)
            {
                _password = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsPasswordEmpty));
                LoginCommand.RaiseCanExecuteChanged();
            }
        }
    }
    public bool IsPasswordEmpty { get => Password == string.Empty; }
    private string _password = string.Empty;

    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set
        {
            if (_isPasswordVisible != value)
            {
                _isPasswordVisible = value;
                RaisePropertyChanged();
            }
        }
    }
    private bool _isPasswordVisible = false;

    public LoginWindowViewModel()
    {
        BackCommand = new DelegateCommand(_ =>
        {
            ServerUrl = string.Empty;
            SchoolName = string.Empty;
            UserName = string.Empty;
            Password = string.Empty;

            IsExtendedOptions = false;
            IsLoginPage = false;
        });

        ExtendedOptionsCommand = new DelegateCommand(_ => !IsLoginPage, _ =>
        {
            IsExtendedOptions = true;
            IsLoginPage = true;
        });

        PasswordVisibilityCommand = new DelegateCommand(_ => IsPasswordVisible = !IsPasswordVisible);

        LoginCommand = new DelegateCommand(_ => !HasErrors && !IsSchoolNameEmpty && !IsUserNameEmpty && !IsPasswordEmpty, _ =>
        {
        });
    }
}
