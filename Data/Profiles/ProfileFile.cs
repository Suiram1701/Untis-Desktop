using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Models;

namespace Data.Profiles;

[File(@"\Untis Desktop\Profiles\")]
[XmlRoot(Namespace = "https://github.com/Suiram1701/Untis-Desktop/raw/develop/Data/Schemas/ProfileSchema.xsd")]
public class ProfileFile : FileBase<ProfileFile>
{
    [XmlAttribute("isActive")]
    public bool IsActive { get; set; } = false;

    [XmlElement("school")]
    public School? School { get; set; } = null;

    [XmlIgnore]
    public string Password
    {
        get => Encoding.UTF8.GetString(Convert.FromBase64String(EncodedPassword));
        set => EncodedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }
    [XmlElement("encodedPassword")]
    public string EncodedPassword = string.Empty;

    [XmlIgnore]
    public IUser? User { get => Student as IUser ?? Teacher; }

    [XmlElement("studentUser")]
    public Student? Student = null;
    public bool ShouldSerialize_student() => Student != null;

    [XmlElement("teacherUser")]
    public Teacher? Teacher = null;
    public bool ShouldSerialize_teacher() => Teacher != null;

    [XmlElement("options")]
    public ProfileOptions Options { get; set; } = new();

    /// <summary>
    /// Login into the profile
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The logged in client</returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="HttpRequestException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<WebUntisClient> LoginAsync(CancellationToken ct)
    {
        WebUntisClient client = new("UntisDesktop", TimeSpan.FromSeconds(5));
        try
        {
            bool loggedIn = await client.LoginAsync(School?.Server, School?.LoginName, User?.Name, Password, "UntisDesktop_Login", ct);

            if (!loggedIn)
            {
                Logger.LogError($"Login to profile {User?.Id} failed in cause of bad credentials");
                throw new UnauthorizedAccessException();
            }

            if (User != client.User)
            {
                Student = client.User as Student;
                Teacher = client.User as Teacher;
            }

            School school = await SchoolSearch.GetSchoolByNameAsync(School?.LoginName, "UpdateSchoolData", ct);
            if (School != school)
                School = school;

            Update();
        }
        catch (HttpRequestException ex)
        {
            if (ex.Source == "System.Net.Http")
                Logger.LogError($"Http request to login to profile {User?.Id} failed.");
            else
                Logger.LogError($"Login to profile {User?.Id} failed in cause of an occurred http error: {ex.Source}; Message: {ex.Message}; Code: {ex.StatusCode}");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Login to profile {User?.Id} failed in cause an occurred exception: {ex.Source}; Message: {ex.Message}");
            throw;
        }

        return client;
    }
}
