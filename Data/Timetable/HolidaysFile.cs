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
[XmlRoot(Namespace = "https://github.com/Suiram1701/Untis-Desktop/raw/develop/Data/Schemas/HolidaysSchema.xsd")]
public class HolidaysFile : FileBase<HolidaysFile>
{
    [XmlIgnore]
    public static HolidaysFile s_DefaultInstance = new();

    [XmlElement("holidays")]
    public List<Holidays> Holidays = new();

    public Holidays? this[int id] { get => Holidays.FirstOrDefault(r => r.Id == id); }

    static HolidaysFile()
    {
        SetProfile(ProfileCollection.GetActiveProfile());
    }

    public static void SetProfile(ProfileFile profile)
    {
        int currentProfileId = profile.User?.Id ?? throw new InvalidDataException($"Static {nameof(HolidaysFile)} constructor: {nameof(ProfileFile.User)} is null");
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Timetable\{currentProfileId}\Holidays.xml";

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
             * In order to be able to continue my work on this project, I use some holidays from Germany.
             */

            //s_DefaultInstance.Classes = (await client.GetHolidaysAsync("reloadHolidays", CancellationToken.None)).ToList();

            s_DefaultInstance.Holidays = new()
            {
                new() {Id = 0, Name = "WF", LongName = "Winterferien", StartDate = new(2023, 1, 30), EndDate = new(2023, 2, 3)},
                new() {Id = 1, Name = "OSF", LongName = "Osterferien", StartDate = new(2023, 4, 3), EndDate = new(2023, 4, 14)},
                new() {Id = 2, Name = "PF", LongName = "Pfingsten", StartDate = new(2023, 5, 19), EndDate = new(2023, 5, 19)},
                new() {Id = 3, Name = "SF", LongName = "Sommerferien", StartDate = new(2023, 7, 13), EndDate = new(2023, 8, 26)},
                new() {Id = 4, Name = "HF", LongName = "Herbstferien", StartDate = new(2023, 10, 23), EndDate = new(2023, 11, 4)},
                new() {Id = 5, Name = "W", LongName = "Weinachten", StartDate = new(2023, 12, 23), EndDate = new(2024, 1, 5)}
            };

            s_DefaultInstance.Update();
        }
        catch (WebUntisException ex)
        {
            Logger.LogWarning($"Holidays file: Reloading holidays failed with a {nameof(WebUntisException)}: {ex.Message}, Code: {ex.Code}");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Holidays file: Reloading holidays failed: {ex.Source}, Message: {ex.Message}");
            throw;
        }
    }
}
