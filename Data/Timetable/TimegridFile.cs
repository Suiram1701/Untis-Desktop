using Data.Extensions;
using Data.Profiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Exceptions;
using WebUntisAPI.Client.Models;

namespace Data.Timetable;

[File(@"\Untis Desktop\Timetable\{UserId}\")]
[XmlRoot(Namespace = "https://github.com/Suiram1701/Untis-Desktop/raw/develop/Data/Schemas/TimegridSchema.xsd")]
public class TimegridFile : FileBase<TimegridFile>
{
    [XmlIgnore]
    public static TimegridFile s_DefaultInstance = new();

    [XmlElement("timegrid")]
    public Timegrid Timegrid = new();

    static TimegridFile()
    {
        SetProfile(ProfileCollection.GetActiveProfile());
    }

    protected override void Serialize(Stream stream, FileBase<TimegridFile> file)
    {
        using XmlWriter xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true });

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("TimegridFile", GetType().GetCustomAttribute<XmlRootAttribute>()?.Namespace);

        foreach (KeyValuePair<Day, SchoolHour[]> schoolDay in Timegrid)
        {
            xmlWriter.WriteStartElement("schoolDay");
            xmlWriter.WriteAttributeString("day", schoolDay.Key.ToString());

            foreach (SchoolHour hour in schoolDay.Value)
                hour.WriteToXml("schoolHour", xmlWriter);

            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
    }

    protected override TimegridFile? Deserialize(Stream stream)
    {
        Dictionary<Day, SchoolHour[]> schoolDays = new();

        using XmlReader xmlReader = XmlReader.Create(stream);

        while (xmlReader.Read())
        {
            if (xmlReader.IsStartElement("schoolDay"))
            {
                string? dayStr = xmlReader.GetAttribute("day");
                Day day = dayStr switch
                {
                    "Sunday" => Day.Sunday,
                    "Monday" => Day.Monday,
                    "Tuesday" => Day.Tuesday,
                    "Wednesday" => Day.Wednesday,
                    "Thursday" => Day.Thursday,
                    "Friday" => Day.Friday,
                    "Saturday" => Day.Saturday,
                    _ => throw new InvalidDataException($"The value '{dayStr}' can't be converted into {nameof(Day)}.")
                };

                List<SchoolHour> schoolHours = new();
                while (xmlReader.Read())
                {
                    if (xmlReader.IsStartElement("schoolHour"))
                    {
                        SchoolHour hour = new();
                        hour.ReadFromXml(xmlReader);
                        schoolHours.Add(hour);
                    }
                    else
                        break;
                }

                schoolDays.Add(day, schoolHours.ToArray());
            }
        }

        return new TimegridFile() { Timegrid = new() { SchoolDays = schoolDays } };
    }

    public static void SetProfile(ProfileFile profile)
    {
        int currentProfileId = profile.User?.Id ?? throw new InvalidDataException($"Static {nameof(TimegridFile)} constructor: {nameof(ProfileFile.User)} is null");
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Timetable\{currentProfileId}\Timegrid.xml";

        string fileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Timetable\{currentProfileId}\";
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        s_DefaultInstance = Load(filePath) ?? Create(filePath);
        s_DefaultInstance.Update();
    }

    public static async Task UpdateFromClientAsync(WebUntisClient client)
    {
        try
        {
            s_DefaultInstance.Timegrid = await client.GetTimegridAsync("reloadTimegrid");
            s_DefaultInstance.Update();
        }
        catch (WebUntisException ex)
        {
            Logger.LogWarning($"Timegrid file: Reloading timegrid failed with a {nameof(WebUntisException)}: {ex.Message}, Code: {ex.Code}");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Timegrid file: Reloading timegrid failed: {ex.Source}, Message: {ex.Message}");
            throw;
        }
    }
}
