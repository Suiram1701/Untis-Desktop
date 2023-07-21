using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WebUntisAPI.Client.Models;

namespace Data.Extensions;

internal static class ForeBackColorExtensions
{
    /// <summary>
    /// Serialize a <see cref="ForeBackColors"/> to xml
    /// </summary>
    /// <param name="colors"></param>
    /// <param name="elementName">The serialized element name</param>
    /// <param name="writer">The writer</param>
    public static void WriteToXml(this ForeBackColors colors, string elementName, XmlWriter writer)
    {
        writer.WriteStartElement(elementName);

        colors.ForeColor.WriteToXml("foreColor", writer);
        colors.BackColor.WriteToXml("backColor", writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Deserialize a <see cref="ForeBackColors"/> from xml
    /// </summary>
    /// <param name="_"></param>
    /// <param name="elementName">The serialized element name</param>
    /// <param name="reader">The reader</param>
    /// <returns>The <see cref="ForeBackColors"/></returns>
    public static ForeBackColors ReadFromXml(this ForeBackColors _, string elementName, XmlReader reader)
    {
        reader.ReadToFollowing(elementName);
        Color foreColor = new Color().ReadFromXml("foreColor", reader);
        Color backColor = new Color().ReadFromXml("backColor", reader);

        return new ForeBackColors()
        {
            ForeColor = foreColor,
            BackColor = backColor
        };
    }
}
