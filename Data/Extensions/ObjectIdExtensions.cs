using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebUntisAPI.Client.Models;
using System.Threading.Tasks;
using System.Xml;

namespace Data.Extensions;

internal static class ObjectIdExtensions
{
    public static void WriteToXml(this ObjectId objId, XmlWriter xmlWriter, string elementName)
    {
        xmlWriter.WriteStartElement(elementName);
        xmlWriter.WriteAttributeString("id", objId.Id.ToString());
        xmlWriter.WriteAttributeString("orgId", objId.OriginalId?.ToString());

        xmlWriter.WriteEndElement();
    }

    public static ObjectId ReadFromXml(this ObjectId _, XmlReader xmlReader, string elementName)
    {
        xmlReader.GoToNextElement(elementName);
        return new()
        {
            Id = int.Parse(xmlReader.GetAttribute("id") ?? "0"),
            OriginalId = int.TryParse(xmlReader.GetAttribute("orgId"), out int result) ? result : null
        };
    }
}
