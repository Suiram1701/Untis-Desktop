using Data.Static;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using UntisDesktop.Extensions;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Models;

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

            // Subjects
            for (int i = 0; i < period.SubjectsIds.Length; i++)
            {
                ObjectId subject = period.SubjectsIds[i];
                string? subjectStr = SubjectFile.s_DefaultInstance[subject.Id]?.LongName;
                string? orgSubjectStr = SubjectFile.s_DefaultInstance[subject.OriginalId ?? -1]?.LongName;

                if ((subjectStr is null && subject.Id != 0) || (subject.OriginalId is not null && orgSubjectStr is null))     // One of the subjects was not found
                    Logger.LogWarning($"Period load: period = {period.Id}{(subjectStr is null ? $", subject not found = {subject.Id}" : string.Empty)}{((orgSubjectStr is null && subject.OriginalId is not null) ? $", original subject not found = {subject.OriginalId}" : string.Empty)}");

                Code code;
                string subjectString;

                if (subject.Id != 0 && subject.OriginalId is null)     // Normal
                {
                    code = Code.None;
                    subjectString = subjectStr ?? "Err";
                }
                else if (subject.Id != 0 && subject.OriginalId is not null)     // Irregular
                {
                    code = Code.Irregular;
                    subjectString = (subjectStr ?? "Err") + $" ({orgSubjectStr ?? "Err"})";
                }
                else if (subject.Id == 0 && subject.OriginalId is not null)      // Cancelled
                {
                    code = Code.Cancelled;
                    subjectString = orgSubjectStr ?? "Err";
                }
                else
                {
                    code = Code.Irregular;
                    subjectString = "Err";
                }

                Subjects.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                int objId = Subjects.Children.Add(new PeriodInformation(code, subjectString));
                Grid.SetColumn(Subjects.Children[objId], Subjects.ColumnDefinitions.Count - 1);

                if (i < period.SubjectsIds.Length - 1)
                {
                    Subjects.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                    int placeHolderId = Subjects.Children.Add(new PeriodInformation(Code.None, ", "));
                    Grid.SetColumn(Subjects.Children[placeHolderId], Subjects.ColumnDefinitions.Count - 1);
                }
            }

            // Teachers
            for (int i = 0; i < period.TeacherIds.Length; i++)
            {
                ObjectId teachers = period.TeacherIds[i];
                Teacher? teacher = TeacherFile.s_DefaultInstance[teachers.Id];
                Teacher? orgTeacher = TeacherFile.s_DefaultInstance[teachers.OriginalId ?? -1];
                string? teacherStr = teacher?.Title + teacher?.LongName;
                string? orgTeacherStr = orgTeacher?.Title + orgTeacher?.LongName;

                if ((teacherStr is null && teachers.Id != 0) || (teachers.OriginalId is not null && orgTeacherStr is null))     // One of the subjects was not found
                    Logger.LogWarning($"Period load: period = {period.Id}{(teacherStr is null ? $", teacher not found = {teachers.Id}" : string.Empty)}{((orgTeacherStr is null && teachers.OriginalId is not null) ? $", original teacher not found = {teachers.OriginalId}" : string.Empty)}");

                Code code;
                string teacherString;

                if (teachers.Id != 0 && teachers.OriginalId is null)     // Normal
                {
                    code = Code.None;
                    teacherString = teacherStr ?? "Err";
                }
                else if (teachers.Id != 0 && teachers.OriginalId is not null)     // Irregular
                {
                    code = Code.Irregular;
                    teacherString = (teacherStr ?? "Err") + $" ({orgTeacherStr ?? "Err"})";
                }
                else if (teachers.Id == 0 && teachers.OriginalId is not null)      // Cancelled
                {
                    code = Code.Cancelled;
                    teacherString = orgTeacherStr ?? "Err";
                }
                else
                {
                    code = Code.Irregular;
                    teacherString = "Err";
                }

                Teachers.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                int objId = Teachers.Children.Add(new PeriodInformation(code, teacherString));
                Grid.SetColumn(Teachers.Children[objId], Teachers.ColumnDefinitions.Count - 1);

                if (i < period.TeacherIds.Length - 1)
                {
                    Teachers.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                    int placeHolderId = Teachers.Children.Add(new PeriodInformation(Code.None, ", "));
                    Grid.SetColumn(Teachers.Children[placeHolderId], Teachers.ColumnDefinitions.Count - 1);
                }
            }

            // Rooms
            for (int i = 0; i < period.RoomIds.Length; i++)
            {
                ObjectId room = period.RoomIds[i];
                string? roomStr = RoomFile.s_DefaultInstance[room.Id]?.LongName;
                string? orgRoomStr = RoomFile.s_DefaultInstance[room.OriginalId ?? -1]?.LongName;

                if ((roomStr is null && room.Id != 0) || (room.OriginalId is not null && orgRoomStr is null))     // One of the subjects was not found
                    Logger.LogWarning($"Period load: period = {period.Id}{(roomStr is null ? $", room not found = {room.Id}" : string.Empty)}{((orgRoomStr is null && room.OriginalId is not null) ? $", original room not found = {room.OriginalId}" : string.Empty)}");

                Code code;
                string roomString;

                if (room.Id != 0 && room.OriginalId is null)     // Normal
                {
                    code = Code.None;
                    roomString = roomStr ?? "Err";
                }
                else if (room.Id != 0 && room.OriginalId is not null)     // Irregular
                {
                    code = Code.Irregular;
                    roomString = (roomStr ?? "Err") + $" ({orgRoomStr ?? "Err"})";
                }
                else if (room.Id == 0 && room.OriginalId is not null)      // Cancelled
                {
                    code = Code.Cancelled;
                    roomString = orgRoomStr ?? "Err";
                }
                else
                {
                    code = Code.Irregular;
                    roomString = "Err";
                }

                Rooms.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                int objId = Rooms.Children.Add(new PeriodInformation(code, roomString));
                Grid.SetColumn(Rooms.Children[objId], Rooms.ColumnDefinitions.Count - 1);

                if (i < period.RoomIds.Length - 1)
                {
                    Rooms.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                    int placeHolderId = Rooms.Children.Add(new PeriodInformation(Code.None, ", "));
                    Grid.SetColumn(Rooms.Children[placeHolderId], Rooms.ColumnDefinitions.Count - 1);
                }
            }

            // Classes
            for (int i = 0; i < period.ClassIds.Length; i++)
            {
                ObjectId @class = period.ClassIds[i];
                string? classStr = ClassFile.s_DefaultInstance[@class.Id]?.LongName;
                string? orgClassStr = ClassFile.s_DefaultInstance[@class.OriginalId ?? -1]?.LongName;

                if ((classStr is null && @class.Id != 0) || (@class.OriginalId is not null && orgClassStr is null))     // One of the subjects was not found
                    Logger.LogWarning($"Period load: period = {period.Id}{(classStr is null ? $", class not found = {@class.Id}" : string.Empty)}{((orgClassStr is null && @class.OriginalId is not null) ? $", original class not found = {@class.OriginalId}" : string.Empty)}");

                Code code;
                string classString;

                if (@class.Id != 0 && @class.OriginalId is null)     // Normal
                {
                    code = Code.None;
                    classString = classStr ?? "Err";
                }
                else if (@class.Id != 0 && @class.OriginalId is not null)     // Irregular
                {
                    code = Code.Irregular;
                    classString = (classStr ?? "Err") + $" ({orgClassStr ?? "Err"})";
                }
                else if (@class.Id == 0 && @class.OriginalId is not null)      // Cancelled
                {
                    code = Code.Cancelled;
                    classString = orgClassStr ?? "Err";
                }
                else
                {
                    code = Code.Irregular;
                    classString = "Err";
                }

                Classes.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                int objId = Classes.Children.Add(new PeriodInformation(code, classString));
                Grid.SetColumn(Classes.Children[objId], Classes.ColumnDefinitions.Count - 1);

                if (i < period.ClassIds.Length - 1)
                {
                    Classes.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                    int placeHolderId = Classes.Children.Add(new PeriodInformation(Code.None, ", "));
                    Grid.SetColumn(Classes.Children[placeHolderId], Classes.ColumnDefinitions.Count - 1);
                }
            }
        }
    }
}
