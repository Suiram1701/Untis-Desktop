using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WebUntisAPI.Client.Models;

namespace Data.Extensions;

internal static class ColorExtensions
{
    /// <summary>
    /// Serialize a <see cref="Color"/> to xml
    /// </summary>
    /// <param name="color">The color</param>
    /// <param name="elementName">Name of the element</param>
    /// <param name="writer">The xml writer</param>
    public static void WriteToXml(this Color color, string elementName, XmlWriter writer)
    {
        writer.WriteStartElement(elementName);

        writer.WriteAttributeString("A", color.A.ToString());
        writer.WriteAttributeString("R", color.R.ToString());
        writer.WriteAttributeString("G", color.G.ToString());
        writer.WriteAttributeString("B", color.B.ToString());

        writer.WriteEndElement();
    }

    /// <summary>
    /// Deserialize a <see cref="Color"/> from xml
    /// </summary>
    /// <param name="_"></param>
    /// <param name="elementName">The element name of the color</param>
    /// <param name="xmlReader">The reader</param>
    /// <returns>The color</returns>
    public static Color ReadFromXml(this Color _, string elementName, XmlReader xmlReader)
    {
        xmlReader.ReadToFollowing(elementName);
        return Color.FromArgb(
            int.Parse(xmlReader.GetAttribute("A") ?? "0"),
            int.Parse(xmlReader.GetAttribute("R") ?? "0"),
            int.Parse(xmlReader.GetAttribute("G") ?? "0"),
            int.Parse(xmlReader.GetAttribute("B") ?? "0")
        );
    }
}
