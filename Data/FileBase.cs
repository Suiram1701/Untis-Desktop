using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Data;

/// <summary>
/// The base for a xml file
/// </summary>
/// <typeparam name="TFile">The type of the file to serialize</typeparam>
public abstract class FileBase<TFile>
    where TFile : FileBase<TFile>, new()
{
    [XmlIgnore]
    private readonly XmlSerializer _serializer = new(typeof(TFile));

    internal readonly static FileAttribute s_Attribute = FileAttribute.GetAttribute<TFile>();

    /// <summary>
    /// The raw name of the file (without extension)
    /// </summary>
    /// <remarks>
    /// Real name will also be update by change
    /// </remarks>
    [XmlIgnore]
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                try
                {
                    string newPath = Path[..(Path.LastIndexOf('\\') + 1)] + $"{value}.{s_Attribute.Extension}";

                    if (File.Exists(newPath))
                        return;

                    File.Copy(Path, newPath);
                    File.Delete(Path);

                    _name = value;
                    _path = newPath;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"An occurred error happened while change the name of a file at {Path}; Error: {ex.Source}; Message: {ex.Message}");
                    throw;
                }
            }
        }
    }
    [XmlIgnore]
    private string _name = string.Empty;

    /// <summary>
    /// The full path of the file
    /// </summary>
    [XmlIgnore]
    public string Path { get => _path; }
    [XmlIgnore]
    private string _path = string.Empty;

    public void Update()
    {
        try
        {
            using FileStream updateStream = new(Path, FileMode.OpenOrCreate, FileAccess.Write);
            updateStream.SetLength(0);
            _serializer.Serialize(updateStream, this);
        }
        catch (Exception ex)
        {
            Logger.LogError($"An occurred error happened while update a file at {Path}; Error: {ex.Source}; Message: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Delete the file
    /// </summary>
    public void Delete()
    {
        try
        {
            File.Delete(Path);
        }
        catch (FileNotFoundException)
        {
            Logger.LogWarning($"File {Path} could not deleted");
        }
        catch (Exception ex)
        {
            Logger.LogError($"An occurred error happened while delete a file at {Path}; Error: {ex.Source}; Message: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Create a new file
    /// </summary>
    /// <param name="path">The path of the new file (includes the name and without extension)</param>
    /// <returns></returns>
    public static TFile Create(string path)
    {
        try
        {
            TFile file = new();
            string savePath = Regex.Replace(path, @"\.[^(?:/|\\)]+$", string.Empty);
            file._name = savePath[(savePath.LastIndexOf('\\') + 1)..(savePath.Length)];
            savePath += $".{s_Attribute.Extension}";
            file._path = savePath;

            string saveFolder = savePath[..savePath.LastIndexOf('\\')];
            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            using FileStream createStream = new(savePath, FileMode.OpenOrCreate, FileAccess.Write);
            createStream.SetLength(0);
            file._serializer.Serialize(createStream, file);

            return file;
        }
        catch (Exception ex)
        {
            Logger.LogError($"$An occurred error happened while creating file at {path}; Error: {ex.Source}; Message: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Load a file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <returns>The <see cref="TFile"/> instance</returns>
    /// <exception cref="Exception">An exception that happened while loading a file</exception>
    public static TFile? Load(string path)
    {
        TFile? file = null;
        try
        {
            using FileStream loadStream = new(path, FileMode.Open, FileAccess.Read);

            file = new XmlSerializer(typeof(TFile)).Deserialize(loadStream) as TFile ?? throw new InvalidDataException("Unexpected XML file format");
            file._name = Regex.Match(path, @"(?<=(?:/|\\))[^(?:/|\\)]+(?=\..+)").Value;
            file._path = path;
        }
        catch (InvalidDataException)
        {
            Logger.LogWarning($"Unexpected XML file format at \"{path}\"");
        }
        catch (Exception ex)
        {
            Logger.LogError($"An occurred error happened while reading file at {path}; Error: {ex.Source}; Message: {ex.Message}");
        }

        return file;
    }
}
