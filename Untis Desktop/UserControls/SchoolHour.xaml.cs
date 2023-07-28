using Data.Static;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Windows;
using System.Windows.Controls;
using UntisDesktop.Extensions;
using WebUntisAPI.Client.Models;

namespace UntisDesktop.UserControls
{
    /// <summary>
    /// Interaktionslogik für SchoolHour.xaml
    /// </summary>
    public partial class SchoolHour : UserControl
    {
        public Period Lesson { get; }

        public Color Border { get => Lesson.Code == WebUntisAPI.Client.Code.Cancelled ? Color.Red : Color.Transparent; }

        public Color ForegroundColor { get => Lesson.GetForeColor(); }

        public Color BackgroundColor { get => Lesson.GetBackColor(); }

        public string Subjects { get => string.Join(", ", Lesson.GetSubjectStrings()); }

        public string Rooms { get => string.Join(", ", Lesson.GetRoomStrings()); }

        public string Teachers { get => string.Join(", ", Lesson.GetTeacherStrings()); }

        public string Classes { get => string.Join(", ", Lesson.GetClassesString()); }

        public SchoolHour(Period period)
        {
            Lesson = period;
            InitializeComponent();
        }
    }
}
