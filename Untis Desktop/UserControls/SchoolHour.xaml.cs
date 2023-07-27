using Data.Static;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UntisDesktop.ViewModels;
using WebUntisAPI.Client.Models;

namespace UntisDesktop.UserControls
{
    /// <summary>
    /// Interaktionslogik für SchoolHour.xaml
    /// </summary>
    public partial class SchoolHour : UserControl
    {
        public Period Lesson { get; }

        public Color BackgroundColor
        {
            get
            {
                return StatusDataFile.s_DefaultInstance.StatusData.GetLessonTypeColor(Lesson.LessonType).BackColor;
            }
        }

        public SchoolHour(Period period)
        {
            Lesson = period;
            InitializeComponent();
        }
    }
}
