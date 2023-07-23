using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using WebUntisAPI.Client.Models;

namespace Data.Extensions;

internal static partial class SchoolHourExtensions
{
    /// <summary>
    /// Serialize a <see cref="SchoolHour"/> to XML
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="elementName">The name of the <see cref="SchoolHour"/> element</param>
    /// <param name="xmlWriter">The writer</param>
    public static void WriteToXml(this SchoolHour hour, string elementName, XmlWriter xmlWriter)
    {
        xmlWriter.WriteStartElement(elementName);

        xmlWriter.WriteAttributeString("name", hour.Name);
        xmlWriter.WriteAttributeString("startTime", hour.StartTime.ToString("t"));
        xmlWriter.WriteAttributeString("endTime", hour.EndTime.ToString("t"));

        xmlWriter.WriteEndElement();
    }

    /// <summary>
    /// Read a <see cref="SchoolHour"/> from XML
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="elementName">The element to read</param>
    /// <param name="xmlReader">The reader</param>
    public static void ReadFromXml(this SchoolHour hour,  XmlReader xmlReader)
    {
        hour.Name = xmlReader.GetAttribute("name");

        Match startTimeMatch = TimeRegex().Match(xmlReader.GetAttribute("startTime") ?? "00:00");
        hour.StartTime = new DateTime(2020, 1, 1, int.Parse(startTimeMatch.Groups[1].Value), int.Parse(startTimeMatch.Groups[2].Value), 0);

        Match endTimeMatch = TimeRegex().Match(xmlReader.GetAttribute("endTime") ?? "00:00");
        hour.EndTime = new DateTime(2020, 1, 1, int.Parse(endTimeMatch.Groups[1].Value), int.Parse(endTimeMatch.Groups[2].Value), 0);
    }

    [GeneratedRegex(@"^(\d{1,2}):(\d{1,2})$")]
    private static partial Regex TimeRegex();
}
