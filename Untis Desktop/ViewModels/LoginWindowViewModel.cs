using Data.Profiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using UntisDesktop.Localization;
using UntisDesktop.Views;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Exceptions;
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

                _ = Application.Current.Dispatcher.Invoke(async () =>
                {
                    School[]? search = null;
                    try
                    {
                        search = await WebUntisAPI.Client.SchoolSearch.SearchAsync(value, "UntisDesktop_Search");
                    }
                    catch (Exception ex)
                    {
                        if (ex.Source == "System.Net.Http")
                            ErrorBoxContent = LangHelper.GetString("LoginWindow.Search.NNC");
                        else
                            ErrorBoxContent = LangHelper.GetString("App.Err.OEX", ex.Source ?? "System.Exception", ex.Message);
                        Logger.LogError($"School search: {ex.Source ?? "System.Exception"}; {ex.Message}");

                        SearchResults.Clear();
                        Search_TooManyResults = false;
                        goto Reload;
                    }

                    SearchResults.Clear();
                    if (search is School[] schools)
                    {
                        Search_TooManyResults = false;
                        foreach (School school in schools)
                            SearchResults.Add(school);
                    }
                    else
                        Search_TooManyResults = true;

                    Reload:
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
                Match match = Regex.Match(_serverUrl, @"^(?<=https?:[/\\]{2})?[a-zA-Z]+\.webuntis\.com$");
                if (!match.Success)
                    AddError(LangHelper.GetString("LoginWindow.Err.NWU"));

                _ = Application.Current.Dispatcher.Invoke(async () =>
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
                }, DispatcherPriority.Input);
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
        ExtendedOptionsCommand.RaiseCanExecuteChanged();

        PasswordVisibilityCommand = new DelegateCommand(_ => IsPasswordVisible = !IsPasswordVisible);

        LoginCommand = new DelegateCommand(_ => !HasErrors && !IsServerUrlEmpty && !IsSchoolNameEmpty && !IsUserNameEmpty && !IsPasswordEmpty, async _ =>
        {
            using WebUntisClient client = new("UntisDesktop", TimeSpan.FromSeconds(5));
            try
            {
                if (await client.LoginAsync(ServerUrl, SchoolName, UserName, Password, "LoginCheck"))
                {
                    if (ProfileCollection.s_DefaultInstance.Any(profile => profile.User?.Name == UserName))
                    {
                        ErrorBoxContent = LangHelper.GetString("LoginWindow.Inf.PAL");
                        return;
                    }

                    // Create new profile
                    ProfileFile profile = ProfileCollection.s_DefaultInstance.Add(client.User.Id.ToString());
                    profile.School = await WebUntisAPI.Client.SchoolSearch.GetSchoolByNameAsync(SchoolName);
                    profile.Password = Password;
                    if (client.UserType == UserType.Student)
                        profile.Student = client.User as Student;
                    else
                        profile.Teacher = client.User as Teacher;

                    profile.Update();
                    ProfileCollection.s_DefaultInstance.ReloadCollection();
                    _ = ProfileCollection.SetActiveProfileAsync(profile);

                    Application.Current.MainWindow.Close();
                    new MainWindow().Show();
                }
                else
                    ErrorBoxContent = LangHelper.GetString("LoginWindow.Login.InvC");
            }
            catch (WebUntisException ex)
            {
                if (ex.Code == -8500)
                    ErrorBoxContent = LangHelper.GetString("LoginWindow.D.InvSN");
                else
                {
                    ErrorBoxContent = LangHelper.GetString("App.Err.OEX", ex.Message, ex.Code.ToString());
                    Logger.LogError($"WebUntis exception: {ex.Message}; Code {ex.Code}");
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.Source == "System.Net.Http")
                    ErrorBoxContent = LangHelper.GetString("LoginWindow.Err.NIC");
                else
                {
                    ErrorBoxContent = LangHelper.GetString("App.Err.OEX", ex.Message, ((int)(ex.StatusCode ?? 0)).ToString());
                    Logger.LogError($"Unexpected HttpRequestException was thrown: {ex.Message}; Code: {ex.StatusCode}");
                }
            }
            catch (TaskCanceledException)
            {
                ErrorBoxContent = LangHelper.GetString("LoginWindow.Err.RTL");
                Logger.LogWarning($"The answer from the WebUntis server took too long. Server: {ServerUrl}");
            }
            catch (Exception ex)
            {
                ErrorBoxContent = LangHelper.GetString("App.Err.OEX", ex.Source ?? "System.Exception", ex.Message);
                Logger.LogError($"An occurred {ex.Source} was thrown; Message: {ex.Message}");
            }
            finally
            {
                client.Dispose();
            }
        });
        LoginCommand.RaiseCanExecuteChanged();
    }
}
