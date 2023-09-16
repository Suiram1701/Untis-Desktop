using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Data.Profiles;

public class ProfileOptions
{
    /// <summary>
    /// Last selected week for this profile
    /// </summary>
    [XmlElement("selectedWeek")]
    public DateTime SelectedWeek { get; set; } = DateTime.Now.Date;

    /// <summary>
    /// Weeks in the past that are saved
    /// </summary>
    [XmlElement("pastWeeks")]
    public int BeforeWeeks { get; set; } = 1;

    /// <summary>
    /// Weeks in the future that are saved
    /// </summary>
    [XmlElement("preloadWeeks")]
    public int PreloadWeeks { get; set; } = 4;
}
