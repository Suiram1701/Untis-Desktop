using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licenses;

[DebuggerDisplay("{LibraryName,nq} v{Version.ToString(),nq}")]
public class LicenceInformation
{
    public string LibraryName { get; set; }

    public Version? Version { get; set; }

    public string Content { get; set; }

    internal LicenceInformation(string name, Version? version, string content)
    {
        LibraryName = name;
        Version = version;
        Content = content;
    }
}
