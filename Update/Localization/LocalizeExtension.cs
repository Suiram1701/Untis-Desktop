using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Markup;

namespace Update.Localization;

/// <summary>
/// Localize a key and return the string (Replacable values aren't supportet)
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
internal class LocalizeExtension : MarkupExtension
{
    /// <summary>
    /// The key to localize
    /// </summary>
    [Required]
    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return LangHelper.GetString(Key);
    }
}