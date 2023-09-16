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
            s_DefaultInstance.Holidays = (await client.GetHolidaysAsync("reloadHolidays")).ToList();
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
