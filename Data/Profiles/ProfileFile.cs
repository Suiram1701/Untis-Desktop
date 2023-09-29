using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public IUser User { get => Student as IUser ?? Teacher!; }

    [XmlElement("studentUser")]
    public Student? Student = null;
    public bool ShouldSerialize_student() => Student != null;

    [XmlElement("teacherUser")]
    public Teacher? Teacher = null;
    public bool ShouldSerialize_teacher() => Teacher != null;

    [XmlElement("accountConfig")]
    public AccountConfig AccountConfig { get; set; } = new();

    [XmlElement("accountGenerally")]
    public GeneralAccount GeneralAccount { get; set; } = new();

    [XmlElement("accountContactDetails")]
    public ContactDetails ContactDetails { get; set; } = new();

    [XmlIgnore]
    public Image? ProfileImage
    {
        get
        {
            if (string.IsNullOrEmpty(ProfileImageEncoded))
                return null;

            return Image.Load(Convert.FromBase64String(ProfileImageEncoded));
        }
        set
        {
            if (value is null)
            {
                ProfileImageEncoded = string.Empty;
                return;
            }

            using MemoryStream imageStream = new();
            value.SaveAsPng(imageStream);
            ProfileImageEncoded = Convert.ToBase64String(imageStream.ToArray());
        }
    }

    [XmlElement("profileImageEncoded")]
    public string ProfileImageEncoded = string.Empty;
    public bool ShouldSerialize_ProfileImageEncoded() => !string.IsNullOrEmpty(ProfileImageEncoded);

    [XmlElement("options")]
    public ProfileOptions Options { get; set; } = new();

    static ProfileFile()
    {
        s_Overrides.Add(typeof(Gender), new(new CustomAttributeProvider(new XmlTypeAttribute("Gender"))));
        s_Overrides.Add(typeof(GeneralAccount.Gender), new(new CustomAttributeProvider(new XmlTypeAttribute("UserGender"))));
    }

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

            // Update user information
            if (User != client.User)
            {
                Student = client.User as Student;
                Teacher = client.User as Teacher;
            }

            // Update school information
            School school = await SchoolSearch.GetSchoolByNameAsync(School?.LoginName, "UpdateSchoolData", ct);

            // Update account information
            AccountConfig = await client.GetAccountConfigAsync(ct);
            GeneralAccount = await client.GetGenerallyAccountInformationAsync(ct);

            (ContactDetails contactDetails, bool canRead, _) = await client.GetContactDetailsAsync(ct);
            if (canRead)
                ContactDetails = contactDetails;

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

    private static XmlAttributeOverrides s_Overrides = new();

    protected override void Serialize(Stream stream, FileBase<ProfileFile> file)
    {
        XmlSerializerNamespaces namespaces = new();
        string? xmlns = GetType().GetCustomAttribute<XmlRootAttribute>()?.Namespace;
        if (xmlns is string ns)
            namespaces.Add("", ns);

        new XmlSerializer(typeof(ProfileFile), s_Overrides).Serialize(stream, file, namespaces);
    }

    protected override ProfileFile? Deserialize(Stream stream)
    {
        return new XmlSerializer(typeof(ProfileFile), s_Overrides).Deserialize(stream) as ProfileFile;
    }
}
