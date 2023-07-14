using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UntisDesktop.ViewModels;

/// <summary>
/// The base of every view model
/// </summary>
internal abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
{
    public bool HasErrors =>
        Errors.Count > 0;

    /// <summary>
    /// Get current View model base
    /// </summary>
    public ViewModelBase Error =>
        this;

    private IDictionary<string, List<string>> Errors { get; } = new Dictionary<string, List<string>>();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// Update a property 
    /// </summary>
    /// <param name="propertyName">Property to update</param>
    protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (!string.IsNullOrEmpty(propertyName))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool HasError([CallerMemberName] string propertyName = "") =>
        Errors.ContainsKey(propertyName);

    public IEnumerable GetErrors([CallerMemberName] string? propertyName = "")
    {
        if (Errors.TryGetValue(propertyName ?? string.Empty, out _))
            return Errors[propertyName ?? string.Empty];
        else
            return Array.Empty<string>();
    }

    /// <summary>
    /// Get the next error for the property
    /// </summary>
    /// <param name="propertyName">Property</param>
    /// <returns>Error message</returns>
    public string this[string propertyName]
    {
        get => GetErrors(propertyName).Cast<string>().FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// Add an error to the error list for the property
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="propertyName">Property</param>
    protected virtual void AddError(string message, [CallerMemberName] string propertyName = "")
    {
        if (!Errors.ContainsKey(propertyName))
            Errors.Add(propertyName, new List<string>());

        Errors[propertyName].Add(message);
    }

    /// <summary>
    /// Remove all errors for the property
    /// </summary>
    /// <param name="propertyName">Property</param>
    protected virtual void ClearErrors([CallerMemberName] string propertyName = "")
    {
        Errors.Remove(propertyName);
        RaiseErrorsChanged(propertyName);
    }

    /// <summary>
    /// Notify that the errors has been changed
    /// </summary>
    /// <param name="propertyName">Property</param>
    protected virtual void RaiseErrorsChanged([CallerMemberName] string propertyName = "")
    {
        if (!string.IsNullOrEmpty(propertyName))
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

        RaisePropertyChanged(nameof(Error));
    }
}