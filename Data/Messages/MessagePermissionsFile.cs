using Data.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Exceptions;
using WebUntisAPI.Client.Models.Messages;

namespace Data.Messages;

[File(@"\Untis Desktop\Messages\{UserId}\")]
[XmlRoot(Namespace = "https://github.com/Suiram1701/Untis-Desktop/raw/develop/Data/Schemas/MessagePermissionsSchema.xsd")]
public class MessagePermissionsFile : FileBase<MessagePermissionsFile>
{
    [XmlIgnore]
    public static MessagePermissionsFile s_DefaultInstance = new();

    [XmlElement("permissions")]
    public MessagePermissions Permissions = new();

    static MessagePermissionsFile()
    {
        SetProfile(ProfileCollection.GetActiveProfile());
    }

    public static void SetProfile(ProfileFile profile)
    {
        int currentProfileId = profile.User?.Id ?? throw new InvalidDataException($"Static {nameof(MessagePermissionsFile)} constructor: {nameof(ProfileFile.User)} is null");
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Messages\{currentProfileId}\MessagePermissions.xml";

        string fileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Messages\{currentProfileId}\";
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        s_DefaultInstance = Load(filePath) ?? Create(filePath);
        s_DefaultInstance.Update();
    }

    public static async Task UpdateFromClientAsync(WebUntisClient client)
    {
        try
        {
            s_DefaultInstance.Permissions = await client.GetMessagePermissionsAsync(CancellationToken.None);
            s_DefaultInstance.Update();
        }
        catch (WebUntisException ex)
        {
            Logger.LogWarning($"MessagePermissions file: Reloading MessagePermissions failed with a {nameof(WebUntisException)}: {ex.Message}, Code: {ex.Code}");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError($"MessagePermissions file: Reloading MessagePermissions failed: {ex.Source}, Message: {ex.Message}");
            throw;
        }
    }
}
