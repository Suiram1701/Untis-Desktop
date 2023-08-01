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
using System.Drawing;
using System.Xml.Schema;
using System.Xml;
using Data.Extensions;
using System.Reflection;

namespace Data.Static;

[File(@"\Untis Desktop\Static\{UserId}\")]
[XmlRoot(Namespace = "https://github.com/Suiram1701/Untis-Desktop/raw/develop/Data/Schemas/SubjectsSchema.xsd")]
public class SubjectFile : FileBase<SubjectFile>
{
    [XmlIgnore]
    public static SubjectFile s_DefaultInstance = new();

    [XmlElement("subject")]
    public List<Subject> Subjects = new();

    public Subject? this[int id] { get => Subjects.FirstOrDefault(s => s.Id == id); }

    static SubjectFile()
    {
        SetProfile(ProfileCollection.GetActiveProfile());
    }

    protected override void Serialize(Stream stream, FileBase<SubjectFile> file)
    {
        using XmlWriter xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true });

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("SubjectFile", GetType().GetCustomAttribute<XmlRootAttribute>()?.Namespace);

        foreach (Subject subject in Subjects)
        {
            xmlWriter.WriteStartElement("subject");
            xmlWriter.WriteAttributeString("id", subject.Id.ToString());
            xmlWriter.WriteAttributeString("Active", subject.Active ? "true" : "false");

            xmlWriter.WriteElementString("name", subject.Name);
            xmlWriter.WriteElementString("longName", subject.LongName);
            xmlWriter.WriteElementString("alternateName", subject.AlternateName);

            subject.ForeColor.WriteToXml("foreColor", xmlWriter);
            subject.BackColor.WriteToXml("backColor", xmlWriter);

            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
    }

    protected override SubjectFile? Deserialize(Stream stream)
    {
        List<Subject> subjects = new();

        using XmlReader xmlReader = XmlReader.Create(stream);

        while (xmlReader.Read())
        {
            if (xmlReader.IsStartElement("subject"))
            {
                Subject subject = new()
                {
                    Id = int.Parse(xmlReader.GetAttribute("id") ?? "0"),
                    Active = bool.Parse(xmlReader.GetAttribute("Active") ?? string.Empty)
                };

                xmlReader.GoToNextElement("name");
                subject.Name = xmlReader.ReadElementContentAsString();

                xmlReader.GoToNextElement("longName");
                subject.LongName = xmlReader.ReadElementContentAsString();

                subject.BackColor = new Color().ReadFromXml("foreColor", xmlReader);
                subject.BackColor = new Color().ReadFromXml("backColor", xmlReader);

                subjects.Add(subject);
            }
        }

        return new SubjectFile() { Subjects = subjects };
    }

    public static void SetProfile(ProfileFile profile)
    {
        int currentProfileId = profile.User?.Id ?? throw new InvalidDataException($"Static {nameof(SubjectFile)} constructor: {nameof(ProfileFile.User)} is null");
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Static\{currentProfileId}\Subjects.xml";

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
            s_DefaultInstance.Subjects = (await client.GetSubjectsAsync("reloadSubjects", CancellationToken.None)).ToList();
            s_DefaultInstance.Update();
            Color c = s_DefaultInstance.Subjects[0].ForeColor;
        }
        catch (WebUntisException ex)
        {
            Logger.LogWarning($"Subject file: Reloading subjects failed with a {nameof(WebUntisException)}: {ex.Message}, Code: {ex.Code}");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Subject file: Reloading subjects failed: {ex.Source}, Message: {ex.Message}");
            throw;
        }
    }
}
