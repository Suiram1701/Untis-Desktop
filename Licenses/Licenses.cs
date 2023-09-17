using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Licenses;

public static partial class Licenses
{
    [GeneratedRegex(@".v((?:(\d+)\.){2,3}(\d+))", RegexOptions.Singleline)]
    private static partial Regex VersionRegex();

    public static IEnumerable<LicenceInformation> GetLicenses()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        foreach (string path in assembly.GetManifestResourceNames())
        {
            string resourceName = path[(path.IndexOf('.') + 1)..path.LastIndexOf('.')];
            using Stream stream = assembly.GetManifestResourceStream(path)!;

            string name;
            Version? version;

            if (VersionRegex().Match(resourceName) is Match versionMatch)
            {
                name = resourceName[..versionMatch.Index];
                version = Version.Parse(versionMatch.Groups[1].Value);
            }
            else
            {
                name = resourceName;
                version = null;
            }

            int currentByte = -1;
            StringBuilder sb = new();
            while ((currentByte = stream.ReadByte()) != -1)
            {
                sb.Append((char)currentByte);
            }

            yield return new(name, version, sb.ToString());
        }
    }
}
