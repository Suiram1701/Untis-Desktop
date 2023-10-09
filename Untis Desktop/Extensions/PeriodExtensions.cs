using Data.Static;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Models;

namespace UntisDesktop.Extensions;

internal static class PeriodExtensions
{
    public static Color GetForeColor(this Period period)
    {
        if (period.Code != WebUntisAPI.Client.Code.None)
            return StatusDataFile.s_DefaultInstance.StatusData.GetCodeColor(period.Code)?.ForeColor ?? Color.Black;

        Color? color = SubjectFile.s_DefaultInstance.Subjects.FirstOrDefault(s => s.Id == period.SubjectsIds.FirstOrDefault(new ObjectId()).Id)?.ForeColor;
        if (color?.IsEmpty ?? true)
            color = StatusDataFile.s_DefaultInstance.StatusData.GetLessonTypeColor(period.LessonType).ForeColor;

        return color.Value;
    }

    public static Color GetBackColor(this Period period)
    {
        if (period.Code != WebUntisAPI.Client.Code.None)
            return StatusDataFile.s_DefaultInstance.StatusData.GetCodeColor(period.Code)?.BackColor ?? Color.Orange;

        Color? color = SubjectFile.s_DefaultInstance.Subjects.FirstOrDefault(s => s.Id == period.SubjectsIds.FirstOrDefault(new ObjectId()).Id)?.BackColor;
        if (color?.IsEmpty ?? true)
            color = StatusDataFile.s_DefaultInstance.StatusData.GetLessonTypeColor(period.LessonType).BackColor;

        return color.Value;
    }

    public static IEnumerable<(string subjectString, Code code, Color? color)> GetSubjects(this Period period) => GetSubjects(period, true);

    public static IEnumerable<(string subjectString, Code code, Color? color)> GetSubjects(this Period period, bool useLongString)
    {
        return period.SubjectsIds.Select(subject =>
        {
            Subject? nSubject = SubjectFile.s_DefaultInstance[subject.Id];
            Subject? orgSubject = SubjectFile.s_DefaultInstance[subject.OriginalId ?? -1];

            // Read the strings
            string? subjectStr = useLongString
                ? nSubject?.LongName
                : nSubject?.Name;
            string? orgSubjectStr = useLongString
                ? orgSubject?.LongName
                : orgSubject?.Name;

            Color? subjectColor = nSubject?.BackColor.IsEmpty ?? true
                ? null
                : nSubject.BackColor;

            if ((subjectStr is null && subject.Id != 0) || (subject.OriginalId is not null && orgSubjectStr is null))     // One of the subjects was not found
                Logger.LogWarning($"Period load: period = {period.Id}{(subjectStr is null ? $", subject not found = {subject.Id}" : string.Empty)}{((orgSubjectStr is null && subject.OriginalId is not null) ? $", original subject not found = {subject.OriginalId}" : string.Empty)}");

            Code code;
            string subjectString;

            // Modify the strings in the different cases
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
                subjectColor = orgSubject?.BackColor.IsEmpty ?? true
                ? null
                : orgSubject.BackColor;
                subjectString = orgSubjectStr ?? "Err";
            }
            else
            {
                code = Code.Irregular;
                subjectString = "Err";
            }

            return (subjectString, code, subjectColor);
        });
    }

    public static IEnumerable<(string teacherString, Code code)> GetTeachers(this Period period) => GetTeachers(period, true);

    public static IEnumerable<(string teacherString, Code code)> GetTeachers(this Period period, bool useLongString)
    {
        return period.TeacherIds.Select(teachers =>
        {
            Teacher? teacher = TeacherFile.s_DefaultInstance[teachers.Id];
            Teacher? orgTeacher = TeacherFile.s_DefaultInstance[teachers.OriginalId ?? - 1];

            // Read the strings
            string? teacherStr = useLongString
                ? teacher?.LongName
                : teacher?.Name;
            string? orgTeacherStr = useLongString
                ? orgTeacher?.LongName
                : orgTeacher?.Name;

            if ((teacherStr is null && teachers.Id != 0) || (teachers.OriginalId is not null && orgTeacherStr is null))
                Logger.LogWarning($"Period load: period = {period.Id}{(teacherStr is null ? $", teacher not found = {teachers.Id}" : string.Empty)}{((orgTeacherStr is null && teachers.OriginalId is not null) ? $", original teacher not found = {teachers.OriginalId}" : string.Empty)}");

            Code code;
            string teacherString;

            // Modify the strings in the different cases
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

            return (teacherString, code);
        });
    }

    public static IEnumerable<(string roomString, Code code)> GetRooms(this Period period) => GetRooms(period, true);

    public static IEnumerable<(string roomString, Code code)> GetRooms(this Period period, bool useLongString)
    {
        return period.RoomIds.Select(rooms =>
        {
            Room? room = RoomFile.s_DefaultInstance[rooms.Id];
            Room? orgRoom = RoomFile.s_DefaultInstance[rooms.OriginalId ?? -1];

            // Read the strings
            string? roomStr = useLongString
                ? room?.LongName
                : room?.Name;
            string? orgRoomStr = useLongString
                ? orgRoom?.LongName
                : orgRoom?.Name;

            if ((roomStr is null && rooms.Id != 0) || (rooms.OriginalId is not null && orgRoomStr is null))     // One of the subjects was not found
                Logger.LogWarning($"Period load: period = {period.Id}{(roomStr is null ? $", room not found = {rooms.Id}" : string.Empty)}{((orgRoomStr is null && rooms.OriginalId is not null) ? $", original room not found = {rooms.OriginalId}" : string.Empty)}");

            Code code;
            string roomString;

            // Modify the strings in the different cases
            if (rooms.Id != 0 && rooms.OriginalId is null)     // Normal
            {
                code = Code.None;
                roomString = roomStr ?? "Err";
            }
            else if (rooms.Id != 0 && rooms.OriginalId is not null)     // Irregular
            {
                code = Code.Irregular;
                roomString = (roomStr ?? "Err") + $" ({orgRoomStr ?? "Err"})";
            }
            else if (rooms.Id == 0 && rooms.OriginalId is not null)      // Cancelled
            {
                code = Code.Cancelled;
                roomString = orgRoomStr ?? "Err";
            }
            else
            {
                code = Code.Irregular;
                roomString = "Err";
            }

            return (roomString, code);
        });
    }

    public static IEnumerable<(string classString, Code code)> GetClasses(this Period period) => GetClasses(period, true);

    public static IEnumerable<(string classString, Code code)> GetClasses(this Period period, bool useLongString)
    {
        return period.ClassIds.Select(classes =>
        {
            Class? @class = ClassFile.s_DefaultInstance[classes.Id];
            Class? orgClass = ClassFile.s_DefaultInstance[classes.OriginalId ?? -1];

            // Read the strings
            string? classStr = useLongString
                ? @class?.LongName
                : @class?.Name;
            string? orgClassStr = useLongString
                ? orgClass?.LongName
                : orgClass?.Name;

            if ((classStr is null && classes.Id != 0) || (classes.OriginalId is not null && orgClassStr is null))     // One of the subjects was not found
                Logger.LogWarning($"Period load: period = {period.Id}{(classStr is null ? $", class not found = {classes.Id}" : string.Empty)}{((orgClassStr is null && classes.OriginalId is not null) ? $", original class not found = {classes.OriginalId}" : string.Empty)}");

            Code code;
            string classString;

            // Modify the strings in the different cases
            if (classes.Id != 0 && classes.OriginalId is null)     // Normal
            {
                code = Code.None;
                classString = classStr ?? "Err";
            }
            else if (classes.Id != 0 && classes.OriginalId is not null)     // Irregular
            {
                code = Code.Irregular;
                classString = (classStr ?? "Err") + $" ({orgClassStr ?? "Err"})";
            }
            else if (classes.Id == 0 && classes.OriginalId is not null)      // Cancelled
            {
                code = Code.Cancelled;
                classString = orgClassStr ?? "Err";
            }
            else
            {
                code = Code.Irregular;
                classString = "Err";
            }

            return (classString, code);
        });
    }

    public static bool IsSameLesson(this Period period, Period other)
    {
        return period.LessonType == other.LessonType && period.Code == other.Code && period.RoomIds.SequenceEqual(other.RoomIds) && period.ClassIds.SequenceEqual(other.ClassIds)
            && period.SubjectsIds.SequenceEqual(other.SubjectsIds) && period.TeacherIds.SequenceEqual(other.TeacherIds) && period.StudentGroup == other.StudentGroup
            && period.SubstitutionText == other.SubstitutionText && period.Info == other.Info && period.Date == other.Date && period.LessonNumber == other.LessonNumber && period.LessonText == other.LessonText;
    }

    public class PeriodEqualityComparer : IEqualityComparer<Period>
    {
        bool IEqualityComparer<Period>.Equals(Period? x, Period? y)
        {
            if (x is null || y is null)
                return false;

            return x.IsSameLesson(y);
        }

        int IEqualityComparer<Period>.GetHashCode([DisallowNull] Period obj)
        {
            return obj.LessonType.GetHashCode() + obj.Code.GetHashCode() + obj.RoomIds.Sum(n => n.Id + n.OriginalId ?? 0) + obj.ClassIds.Sum(n => n.Id + n.OriginalId ?? 0) + obj.SubjectsIds.Sum(n => n.Id + n.OriginalId ?? 0) + obj.TeacherIds.Sum(n => n.Id + n.OriginalId ?? 0)
                + obj.StudentGroup.GetHashCode() + obj.SubstitutionText.GetHashCode() + obj.Info.GetHashCode() + obj.Date.GetHashCode() + obj.LessonNumber + obj.LessonText.GetHashCode();
        }
    }
}
