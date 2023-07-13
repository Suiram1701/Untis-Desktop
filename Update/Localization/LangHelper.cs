using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Update.Localization;

/// <summary>
/// Lang helper to get strings in current selected lang.
/// </summary>
internal static class LangHelper
{
    public static readonly CultureInfo[] s_SuportedCultures = new CultureInfo[6]
    {
        new("de"),
        new("en"),
        new("es"),
        new("fr"),
        new("ru"),
        new("sv")
    };

    /// <summary>
    /// Current Selected culture
    /// </summary>
    /// <remarks>
    /// If culture to set isnt available the culture doenst change
    /// </remarks>
    public static CultureInfo Culture
    {
        get => s_Culture;
        set
        {
            if (s_SuportedCultures.Any(lang => value.Name.Contains(lang.Name)))
                s_Culture = value;
        }
    }
    private static CultureInfo s_Culture = new("en");

    /// <summary>
    /// Resource manager that contains lang strings
    /// </summary>
    private static readonly ResourceManager s_ResManager = new("Update.Localization.Strings", Assembly.GetExecutingAssembly());

    /// <summary>
    /// Setup lang helper
    /// </summary>
    static LangHelper()
    {
        Culture = CultureInfo.InstalledUICulture;
    }

    /// <summary>
    /// Get a string in the selected lang by the given lang key.
    /// </summary>
    /// <param name="langKey">Lang key for string</param>
    /// <returns>The string in the selected lang. If lang key not found the return value is <see langword="null"/>.</returns>
    public static string GetString(string langKey)
    {
        try
        {
            string value = s_ResManager.GetString(langKey, s_Culture) ?? langKey;
            value = value.Replace("\\n", "\n");
            return value;
        }
        catch     // Thrown when resource key not found
        {
            return langKey;
        }
    }

    /// <summary>
    /// Get a string in the selected lang by the given lang key and insert given strings.
    /// <para>
    /// Replace symbol in lang string is '{i}'.
    /// </para>
    /// <para>
    /// If too many insert strings the remain strings will ignore and if not enough insert strings given the rest insert places will be ignored.
    /// </para>
    /// </summary>
    /// <param name="langKey">Lang key for string</param>
    /// <param name="insert">Strings to insert</param>
    /// <returns>The string in the selected lang. If lang key not found the return value is <see langword="null"/>.</returns>
    public static string GetString(string langKey, params string[] insert)
    {
        // Get base lang string and check it
        string baseString = GetString(langKey);

        // Replace symbols with given strings to insert
        string readyString = baseString;
        for (int i = 0; i < insert.Length; i++)
            readyString = readyString.Replace($"{{{i}}}", insert[i]);

        return readyString;
    }
}