using Data.Static;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using UntisDesktop.Extensions;
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

    public static IEnumerable<(string subjectString, Code code)> GetSubjects(this Period period)
    {
        return period.SubjectsIds.Select(subject =>
        {
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

            return (subjectString, code);
        });
    }

    public static IEnumerable<(string teacherString, Code code)> GetTeachers(this Period period)
    {
        return period.TeacherIds.Select(teachers =>
        {
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

            return (teacherString, code);
        });
    }

    public static IEnumerable<(string roomString, Code code)> GetRooms(this Period period)
    {
        return period.RoomIds.Select(room =>
        {
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

            return (roomString, code);
        });
    }

    public static IEnumerable<(string classString, Code code)> GetClasses(this Period period)
    {
        return period.ClassIds.Select(@class =>
        {
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
