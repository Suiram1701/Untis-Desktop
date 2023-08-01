using Data.Extensions;
using Data.Profiles;
using Microsoft.VisualBasic;
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
[XmlRoot(Namespace = "https://github.com/Suiram1701/Untis-Desktop/raw/develop/Data/Schemas/PeriodsSchema.xsd")]
public class PeriodFile : FileBase<PeriodFile>
{
    [XmlIgnore]
    public static PeriodFile s_DefaultInstance = new();

    [XmlElement("period")]
    public List<Period> Periods = new();

    public Period? this[int id] => Periods.FirstOrDefault(p => p.Id == id);

    public IEnumerable<Period> this[DateTime week] => Periods.Where(p => p.Date >= week && p.Date <= week.AddDays(6));

    static PeriodFile()
    {
        SetProfile(ProfileCollection.GetActiveProfile());
    }

    protected override void Serialize(Stream stream, FileBase<PeriodFile> file)
    {
        using XmlWriter xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true });

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("PeriodFile", GetType().GetCustomAttribute<XmlRootAttribute>()?.Namespace);

        foreach (Period period in Periods)
        {
            xmlWriter.WriteStartElement("period");
            xmlWriter.WriteAttributeString("id", period.Id.ToString());

            xmlWriter.WriteElementString("date", period.Date.ToString("s"));
            xmlWriter.WriteElementString("startTime", period.StartTime.ToString("s"));
            xmlWriter.WriteElementString("endTime", period.EndTime.ToString("s"));

            foreach (ObjectId id in period.ClassIds)
                id.WriteToXml(xmlWriter, "classId");

            foreach (ObjectId id in period.TeacherIds)
                id.WriteToXml(xmlWriter, "teacherId");

            foreach (ObjectId id in period.SubjectsIds)
                id.WriteToXml(xmlWriter, "subjectId");

            foreach (ObjectId id in period.RoomIds)
                id.WriteToXml(xmlWriter, "roomId");

            xmlWriter.WriteElementString("lessonType", period.LessonType.ToString());
            xmlWriter.WriteElementString("code", period.Code.ToString());

            xmlWriter.WriteElementString("lessonNumber", period.LessonNumber.ToString());
            xmlWriter.WriteElementString("lessonText", period.LessonText);
            xmlWriter.WriteElementString("statFlags", period.StatisticalFlags);
            xmlWriter.WriteElementString("studentGroup", period.StudentGroup);
            xmlWriter.WriteElementString("substitutionText", period.SubstitutionText);
            xmlWriter.WriteElementString("info", period.Info);
            xmlWriter.WriteElementString("activityType", period.ActivityType);

            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
    }

    protected override PeriodFile? Deserialize(Stream stream)
    {
        List<Period> periods = new();

        using XmlReader xmlReader = XmlReader.Create(stream);

        while (xmlReader.Read())
        {
            if (xmlReader.IsStartElement("period"))
            {
                Period period = new() { Id = int.Parse(xmlReader.GetAttribute("id") ?? "0") };

                xmlReader.GoToNextElement("date");
                period.Date = xmlReader.ReadElementContentAsDateTime();

                xmlReader.GoToNextElement("startTime");
                period.StartTime = xmlReader.ReadElementContentAsDateTime();

                xmlReader.GoToNextElement("endTime");
                period.EndTime = xmlReader.ReadElementContentAsDateTime();

                xmlReader.Read();

                // Classes
                List<ObjectId> classIds = new();
                while (xmlReader.IsStartElement("classId"))
                {
                    classIds.Add(new ObjectId().ReadFromXml(xmlReader, "classId"));
                    xmlReader.Read();
                }
                period.ClassIds = classIds.ToArray();

                // Teachers
                List<ObjectId> teacherIds = new();
                while (xmlReader.IsStartElement("teacherId"))
                {
                    teacherIds.Add(new ObjectId().ReadFromXml(xmlReader, "teacherId"));
                    xmlReader.Read();
                }
                period.TeacherIds = teacherIds.ToArray();

                // Subjects
                List<ObjectId> subjectIds = new();
                while (xmlReader.IsStartElement("subjectId"))
                {
                    subjectIds.Add(new ObjectId().ReadFromXml(xmlReader, "subjectId"));
                    xmlReader.Read();
                }
                period.SubjectsIds = subjectIds.ToArray();

                // Rooms
                List<ObjectId> roomIds = new();
                while (xmlReader.IsStartElement("roomId"))
                {
                    roomIds.Add(new ObjectId().ReadFromXml(xmlReader, "roomId"));
                    xmlReader.Read();
                }
                period.RoomIds = roomIds.ToArray();

                xmlReader.GoToNextElement("lessonType");
                string lessonTypeString = xmlReader.ReadElementContentAsString();
                period.LessonType = lessonTypeString switch
                {
                    nameof(LessonType.Ls) => LessonType.Ls,
                    nameof(LessonType.Oh) => LessonType.Oh,
                    nameof(LessonType.Sb) => LessonType.Sb,
                    nameof(LessonType.Bs) => LessonType.Bs,
                    nameof(LessonType.Ex) => LessonType.Ex,
                    _ => new Func<LessonType>(() =>
                    {
                        Logger.LogWarning($"Period file: period = {period.Id}, unexpected lesson type = {lessonTypeString}");
                        return LessonType.Ls;
                    }).Invoke()
                };

                xmlReader.GoToNextElement("code");
                string codeString = xmlReader.ReadElementContentAsString();
                period.Code = codeString switch
                {
                    nameof(Code.None) => Code.None,
                    nameof(Code.Irregular) => Code.Irregular,
                    nameof(Code.Cancelled) => Code.Cancelled,
                    _ => new Func<Code>(() =>
                    {
                        Logger.LogWarning($"Period file: period = {period.Id}, unexpected code = {codeString}");
                        return Code.None;
                    }).Invoke()
                };

                xmlReader.GoToNextElement("lessonNumber");
                period.LessonNumber = xmlReader.ReadElementContentAsInt();

                xmlReader.GoToNextElement("lessonText");
                period.LessonText = xmlReader.ReadElementContentAsString();

                xmlReader.GoToNextElement("statFlags");
                period.StatisticalFlags = xmlReader.ReadElementContentAsString();

                xmlReader.GoToNextElement("studentGroup");
                period.StudentGroup = xmlReader.ReadElementContentAsString();

                xmlReader.GoToNextElement("substitutionText");
                period.SubstitutionText = xmlReader.ReadElementContentAsString();

                xmlReader.GoToNextElement("info");
                period.Info = xmlReader.ReadElementContentAsString();

                xmlReader.GoToNextElement("activityType");
                period.ActivityType = xmlReader.ReadElementContentAsString();

                periods.Add(period);
            }
        }

        return new() { Periods = periods };
    }

    public static void SetProfile(ProfileFile profile)
    {
        int currentProfileId = profile.User?.Id ?? throw new InvalidDataException($"Static {nameof(PeriodFile)} constructor: {nameof(ProfileFile.User)} is null");
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Timetable\{currentProfileId}\Periods.xml";

        string fileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Timetable\{currentProfileId}\";
        if (!Directory.Exists(fileDirectory))
            Directory.CreateDirectory(fileDirectory);

        s_DefaultInstance = Load(filePath) ?? Create(filePath);
        s_DefaultInstance.Update();
    }

    public static async Task<Period[]> LoadWeekAsync(DateTime updateWeek, bool reload = false)
    {
        if (!reload && s_DefaultInstance[updateWeek].Any())
            return s_DefaultInstance[updateWeek].ToArray();

        using WebUntisClient client = await ProfileCollection.GetActiveProfile().LoginAsync(CancellationToken.None).ConfigureAwait(true);

        IEnumerable<Period> periods = (await client.GetOwnTimetableAsync(updateWeek, updateWeek.AddDays(6)).ConfigureAwait(true)).DistinctBy(p => p.Id);
        foreach (Period period in periods)
        {
            if (s_DefaultInstance[period.Id] is not null)
                s_DefaultInstance.Periods.Remove(s_DefaultInstance[period.Id]!);
            s_DefaultInstance.Periods.Add(period);
        }

        return periods.ToArray();
    }

    public static async Task UpdateFromClientAsync(WebUntisClient client)
    {
        ProfileOptions options = ProfileCollection.GetActiveProfile().Options;

        DateTime currentWeek = DateTime.Now;
        int offset = DayOfWeek.Sunday - currentWeek.DayOfWeek;
        currentWeek = currentWeek.AddDays(offset);

        IEnumerable<DateTime> weeksToLoad = Enumerable.Range(-options.BeforeWeeks, options.BeforeWeeks + options.PreloadWeeks + 1).Select(i => currentWeek.AddDays(7 * i));
        foreach (DateTime week in weeksToLoad)
        { 
            try
            {
                foreach (Period period in await client.GetOwnTimetableAsync(week, week.AddDays(6), "updateTimetable").ConfigureAwait(true))
                {
                    if (s_DefaultInstance[period.Id] is not null)
                        s_DefaultInstance.Periods.Remove(s_DefaultInstance[period.Id]!);
                    s_DefaultInstance.Periods.Add(period);
                }
            }
            catch (WebUntisException ex)
            {
                Logger.LogWarning($"Periods file: Loading periods failed with a {nameof(WebUntisException)}: {ex.Message}, Code: {ex.Code}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Periods file: Loading periods failed: {ex.Source}, Message: {ex.Message}");
                throw;
            }
        }

        s_DefaultInstance.Update();
    }
}
