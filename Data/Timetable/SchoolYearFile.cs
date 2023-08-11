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

namespace Data.Timetable;

[File(@"\Untis Desktop\Timetable\{UserId}\")]
[XmlRoot(Namespace = "https://github.com/Suiram1701/Untis-Desktop/raw/develop/Data/Schemas/SchoolYearsSchema.xsd")]
public class SchoolYearFile : FileBase<SchoolYearFile>
{
    [XmlIgnore]
    public static SchoolYearFile s_DefaultInstance = new();

    [XmlElement("schoolYear")]
    public List<SchoolYear> SchoolYears = new();

    public SchoolYear? this[int id] { get => SchoolYears.FirstOrDefault(s => s.Id == id); }

    public SchoolYear? this[DateTime dateTime] { get => SchoolYears.FirstOrDefault(s => s.StartDate <= dateTime && s.EndDate >= dateTime); }

    static SchoolYearFile()
    {
        SetProfile(ProfileCollection.GetActiveProfile());
    }

    public static void SetProfile(ProfileFile profile)
    {
        int currentProfileId = profile.User?.Id ?? throw new InvalidDataException($"Static {nameof(SchoolYearFile)} constructor: {nameof(ProfileFile.User)} is null");
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Timetable\{currentProfileId}\SchoolYears.xml";

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
            // TODO: java.lang.NullPointerException fix
            /*
             * The server kept throwing a java.lang.NullPointerException and I suspect it's because the school year is over, but I'm not sure.
             * In order to be able to continue my work on this project, I use some data from the school years in the past.
             */

            s_DefaultInstance.SchoolYears = (await client.GetSchoolYearsAsync("reloadSchoolYears", CancellationToken.None)).ToList();
            s_DefaultInstance.Update();
        }
        catch (WebUntisException ex)
        {
            Logger.LogWarning($"SchoolYears file: Reloading schoolYears failed with a {nameof(WebUntisException)}: {ex.Message}, Code: {ex.Code}");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError($"SchoolYears file: Reloading schoolYears failed: {ex.Source}, Message: {ex.Message}");
            throw;
        }
    }
}
