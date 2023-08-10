using Data.Static;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using UntisDesktop.Extensions;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Models;
using System.Windows.Input;
using UntisDesktop.Views;
using System.Linq;

namespace UntisDesktop.UserControls
{
    /// <summary>
    /// Interaktionslogik für SchoolHour.xaml
    /// </summary>
    public partial class SchoolHour : UserControl
    {
        public Period Lesson { get; }

        public Color BorderColor { get => Lesson.Code == Code.Cancelled ? Color.Red : BackgroundColor; }

        public Color CrossLineColor { get => Lesson.Code == Code.Cancelled ? Color.Red : Color.Transparent; }

        public Color ForegroundColor { get => Lesson.GetForeColor(); }

        public Color BackgroundColor { get => Lesson.GetBackColor(); }

        public SchoolHour(Period period)
        {
            Lesson = period;
            InitializeComponent();

            int counter = 0;

            // Subjects
            foreach ((string subjectString, Code code) in period.GetSubjects())
            {
                Subjects.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                int objId = Subjects.Children.Add(new PeriodInformation(code, subjectString));
                Grid.SetColumn(Subjects.Children[objId], Subjects.ColumnDefinitions.Count - 1);

                if (counter< period.SubjectsIds.Length - 1)
                {
                    Subjects.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                    int placeHolderId = Subjects.Children.Add(new PeriodInformation(Code.None, ", "));
                    Grid.SetColumn(Subjects.Children[placeHolderId], Subjects.ColumnDefinitions.Count - 1);
                }

                counter++;
            }

            // Teachers
            counter = 0;
            foreach ((string teacherString, Code code) in period.GetTeachers())
            {
                Teachers.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                int objId = Teachers.Children.Add(new PeriodInformation(code, teacherString));
                Grid.SetColumn(Teachers.Children[objId], Teachers.ColumnDefinitions.Count - 1);

                if (counter< period.TeacherIds.Length - 1)
                {
                    Teachers.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                    int placeHolderId = Teachers.Children.Add(new PeriodInformation(Code.None, ", "));
                    Grid.SetColumn(Teachers.Children[placeHolderId], Teachers.ColumnDefinitions.Count - 1);
                }

                counter++;
            }

            // Rooms
            counter = 0;
            foreach ((string roomString, Code code) in period.GetRooms())
            {
                Rooms.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                int objId = Rooms.Children.Add(new PeriodInformation(code, roomString));
                Grid.SetColumn(Rooms.Children[objId], Rooms.ColumnDefinitions.Count - 1);

                if (counter< period.RoomIds.Length - 1)
                {
                    Rooms.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                    int placeHolderId = Rooms.Children.Add(new PeriodInformation(Code.None, ", "));
                    Grid.SetColumn(Rooms.Children[placeHolderId], Rooms.ColumnDefinitions.Count - 1);
                }

                counter++;
            }

            // Classes
            counter = 0;
            foreach ((string classString, Code code) in period.GetClasses())
            {
                Classes.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                int objId = Classes.Children.Add(new PeriodInformation(code, classString));
                Grid.SetColumn(Classes.Children[objId], Classes.ColumnDefinitions.Count - 1);

                if (counter< period.ClassIds.Length - 1)
                {
                    Classes.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                    int placeHolderId = Classes.Children.Add(new PeriodInformation(Code.None, ", "));
                    Grid.SetColumn(Classes.Children[placeHolderId], Classes.ColumnDefinitions.Count - 1);
                }

                counter++;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (Application.Current.Windows.OfType<PeriodWindow>().FirstOrDefault(w => w.Period.Id == Lesson.Id) is Window window)
            {
                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;
                window.Topmost = true;
            }
            else
            {
                new PeriodWindow(Lesson)
                {
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = Application.Current.MainWindow.Left,
                    Top = Application.Current.MainWindow.Top
                }.Show();
            }

            base.OnMouseDown(e);
        }
    }
}
