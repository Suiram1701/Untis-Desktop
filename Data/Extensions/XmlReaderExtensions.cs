using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Data.Extensions;

internal static class XmlReaderExtensions
{
    public static void GoToNextElement(this XmlReader reader, string tagName)
    {
        if (reader.LocalName != tagName)
            reader.ReadToFollowing(tagName);
    }
}
