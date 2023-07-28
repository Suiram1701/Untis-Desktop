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

namespace Data.Static;

[File(@"\Untis Desktop\Static\{UserId}\")]
[XmlRoot(Namespace = "https://github.com/Suiram1701/Untis-Desktop/raw/develop/Data/Schemas/ClassesSchema.xsd")]
public class ClassFile : FileBase<ClassFile>
{
    [XmlIgnore]
    public static ClassFile s_DefaultInstance = new();

    [XmlElement("class")]
    public List<Class> Classes = new();

    public Class? this[int id] { get => Classes.FirstOrDefault(r => r.Id == id); }

    static ClassFile()
    {
        SetProfile(ProfileCollection.GetActiveProfile());
    }

    public static void SetProfile(ProfileFile profile)
    {
        int currentProfileId = profile.User?.Id ?? throw new InvalidDataException($"Static {nameof(ClassFile)} constructor: {nameof(ProfileFile.User)} is null");
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Static\{currentProfileId}\Classes.xml";

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
            s_DefaultInstance.Classes = (await client.GetClassesAsync("reloadClasses", CancellationToken.None)).ToList();
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
