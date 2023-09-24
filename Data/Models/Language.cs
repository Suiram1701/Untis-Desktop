using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Data.Models;

[DebuggerDisplay("{FullName,nq}")]
public class Language
{
    /// <summary>
    /// The WU internal name
    /// </summary>
    [XmlElement("internalName")]
    public string InternalName { get; set; }

    /// <summary>
    /// The localized full name of the language
    /// </summary>
    [XmlElement("fullName")]
    public string FullName { get; set; }

    public Language()
    {
        InternalName = string.Empty;
        FullName = string.Empty;
    } 

    public Language(string internalName, string fullName)
    {
        InternalName = internalName;
        FullName = fullName;
    }
}
