using Data.Profiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Exceptions;
using WebUntisAPI.Client.Models;

namespace Data.Static;

[File(@"\Untis Desktop\Static\{UserId}\")]
public class TeacherFile : FileBase<TeacherFile>
{
    [XmlIgnore]
    public static TeacherFile s_DefaultInstance = new();

    [XmlElement("teacher")]
    public List<Teacher> Teachers = new();

    public Teacher? this[int id] { get => Teachers.FirstOrDefault(t => t.Id == id); }

    static TeacherFile()
    {
        SetProfile(ProfileCollection.GetActiveProfile());
    }

    public static void SetProfile(ProfileFile profile)
    {
        int currentProfileId = profile.User?.Id ?? throw new InvalidDataException($"Static {nameof(TeacherFile)} constructor: {nameof(ProfileFile.User)} is null");
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\Untis Desktop\Static\{currentProfileId}\Teachers.xml";

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
            s_DefaultInstance.Teachers = (await client.GetTeachersAsync("reloadTeachers", CancellationToken.None)).ToList();
            s_DefaultInstance.Update();
        }
        catch (WebUntisException ex)
        {
            Logger.LogWarning($"Teacher file: Reloading teacher failed with a {nameof(WebUntisException)}: {ex.Message}, Code: {ex.Code}");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Teacher file: Reloading teachers failed: {ex.Source}, Message: {ex.Message}");
            throw;
        }
    }
}
