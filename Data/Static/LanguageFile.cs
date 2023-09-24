using Data.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WebUntisAPI.Client.Exceptions;
using WebUntisAPI.Client.Models;
using WebUntisAPI.Client;
using Data.Models;

namespace Data.Static;

[File(@"\Untis Desktop\Static\{UserId}\")]
[XmlRoot(Namespace = "https://github.com/Suiram1701/Untis-Desktop/raw/develop/Data/Schemas/LanguagesSchema.xsd")]
public class LanguageFile : FileBase<LanguageFile>
{
    [XmlIgnore]
    public static LanguageFile s_DefaultInstance = new();

    [XmlElement("language")]
    public List<Language> Languages = new();

    public Language? this[string internalName] { get => Languages.FirstOrDefault(l => l.InternalName == internalName); }

    static LanguageFile()
    {
        SetProfile(ProfileCollection.GetActiveProfile());
    }

    public static void SetProfile(ProfileFile profile)
    {
        int currentProfileId = profile.User?.Id ?? throw new InvalidDataException($"Static {nameof(LanguageFile)} constructor: {nameof(ProfileFile.User)} is null");
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Static\{currentProfileId}\Languages.xml";

        string fileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Static\{currentProfileId}\";
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        s_DefaultInstance = Load(filePath) ?? Create(filePath);
        s_DefaultInstance.Update();
    }

    public static async Task UpdateFromClientAsync(WebUntisClient client)
    {
        try
        {
            Dictionary<string, string> languages = await client.GetAvailableLanguagesAsync();

            s_DefaultInstance.Languages.Clear();
            foreach ((string key, string value) in languages)
                s_DefaultInstance.Languages.Add(new(key, value));

            s_DefaultInstance.Update();
        }
        catch (WebUntisException ex)
        {
            Logger.LogWarning($"Class file: Reloading classes failed with a {nameof(WebUntisException)}: {ex.Message}, Code: {ex.Code}");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Class file: Reloading classes failed: {ex.Source}, Message: {ex.Message}");
            throw;
        }
    }
}
