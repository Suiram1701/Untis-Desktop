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
using Data.Extensions;
using System.Xml;
using System.Drawing;
using System.Reflection;

namespace Data.Static;

[File(@"\Untis Desktop\Static\{UserId}\")]
[XmlRoot(Namespace = "https://github.com/Suiram1701/Untis-Desktop/raw/develop/Data/Schemas/StatusDataSchema.xsd")]
public class StatusDataFile : FileBase<StatusDataFile>
{
    [XmlIgnore]
    public static StatusDataFile s_DefaultInstance = new();

    [XmlElement("statusData")]
    public StatusData StatusData = new();

    static StatusDataFile()
    {
        SetProfile(ProfileCollection.GetActiveProfile());
    }

    protected override void Serialize(Stream stream, FileBase<StatusDataFile> file)
    {
        using XmlWriter xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true });

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("SubjectFile", GetType().GetCustomAttribute<XmlRootAttribute>()?.Namespace);

        StatusData.LsColors.WriteToXml("lsColors", xmlWriter);
        StatusData.OhColors.WriteToXml("ohColors", xmlWriter);
        StatusData.SbColors.WriteToXml("sbColors", xmlWriter);
        StatusData.BsColors.WriteToXml("bsColors", xmlWriter);
        StatusData.ExColors.WriteToXml("exColors", xmlWriter);
        StatusData.CancelledLessonColors.WriteToXml("cancelledLsColors", xmlWriter);
        StatusData.IrregularLessonColors.WriteToXml("irregularLsColors", xmlWriter);

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
    }

    protected override StatusDataFile? Deserialize(Stream stream)
    {
        StatusData statusData = new();

        using XmlReader xmlReader = XmlReader.Create(stream);

        statusData.LsColors = statusData.LsColors.ReadFromXml("lsColors", xmlReader);
        statusData.OhColors = statusData.OhColors.ReadFromXml("ohColors", xmlReader);
        statusData.SbColors = statusData.SbColors.ReadFromXml("sbColors", xmlReader);
        statusData.BsColors = statusData.BsColors.ReadFromXml("bsColors", xmlReader);
        statusData.ExColors = statusData.ExColors.ReadFromXml("exColors", xmlReader);
        statusData.CancelledLessonColors = statusData.CancelledLessonColors.ReadFromXml("cancelledLsColors", xmlReader);
        statusData.IrregularLessonColors = statusData.IrregularLessonColors.ReadFromXml("irregularLsColors", xmlReader);

        return new StatusDataFile() { StatusData = statusData };
    }

    public static void SetProfile(ProfileFile profile)
    {
        int currentProfileId = profile.User?.Id ?? throw new InvalidDataException($"Static {nameof(StatusDataFile)} constructor: {nameof(ProfileFile.User)} is null");
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Static\{currentProfileId}\StatusData.xml";

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
            s_DefaultInstance.StatusData = await client.GetStatusDataAsync("reloadStatusData", CancellationToken.None);
            s_DefaultInstance.Update();
        }
        catch (WebUntisException ex)
        {
            Logger.LogWarning($"StatusData file: Reloading statusData failed with a {nameof(WebUntisException)}: {ex.Message}, Code: {ex.Code}");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError($"StatusData file: Reloading statusData failed: {ex.Source}, Message: {ex.Message}");
            throw;
        }
    }
}
