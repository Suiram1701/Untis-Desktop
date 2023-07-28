using Data.Static;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using UntisDesktop.Extensions;
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

    public static IEnumerable<string> GetSubjectStrings(this Period period)
    {
        IEnumerable<Subject> allSubjects = SubjectFile.s_DefaultInstance.Subjects;
        return period.SubjectsIds.Select(subject =>
        {
            string? subjectStr = allSubjects.FirstOrDefault(s => s.Id == subject.Id)?.LongName;
            string? orgSubjectStr = allSubjects.FirstOrDefault(s => s.Id == subject.OriginalId)?.LongName;

            if ((subjectStr is null && subject.Id != 0) || (subject.OriginalId is not null && orgSubjectStr is null))
                Logger.LogWarning($"Period load: period = {period.Id}{(subjectStr is null ? $", subject not found = {subject.Id}" : string.Empty)}{((orgSubjectStr is null && subject.OriginalId is not null) ? $", original subject not found = {subject.OriginalId}" : string.Empty)}");

            // Id = 0 means the subject is cancelled
            if (subject.Id == 0)
                return (orgSubjectStr ?? "Err").StrikeThrough();

            string subjectString = subjectStr ?? "Err";
            if (subject.OriginalId is not null)
                subjectString += $" ({orgSubjectStr ?? "Err"})";
            return subjectString;
        });
    }

    public static IEnumerable<string> GetRoomStrings(this Period period)
    {
        IEnumerable<Room> allRooms = RoomFile.s_DefaultInstance.Rooms;
        return period.RoomIds.Select(room =>
        {
            string? roomStr = allRooms.FirstOrDefault(s => s.Id == room.Id)?.LongName;
            string? orgRoomsStr = allRooms.FirstOrDefault(s => s.Id == room.OriginalId)?.LongName;

            if ((roomStr is null && room.Id != 0) || (room.OriginalId is not null && orgRoomsStr is null))
                Logger.LogWarning($"Period load: period = {period.Id}{(roomStr is null ? $", room not found = {room.Id}" : string.Empty)}{((orgRoomsStr is null && room.OriginalId is not null) ? $", original room not found = {room.OriginalId}" : string.Empty)}");

            // Id = 0 means the room is cancelled
            if (room.Id == 0)
                return (orgRoomsStr ?? "Err").StrikeThrough();

            string roomString = roomStr ?? "Err";
            if (room.OriginalId is not null)
                roomString += $" ({orgRoomsStr ?? "Err"})";
            return roomString;
        });
    }

    public static IEnumerable<string> GetClassesString(this Period period)
    {
        IEnumerable<Class> allClasses = ClassFile.s_DefaultInstance.Classes;
        return period.ClassIds.Select(@class =>
        {
            string? classStr = allClasses.FirstOrDefault(s => s.Id == @class.Id)?.LongName;
            string? orgClassStr = allClasses.FirstOrDefault(s => s.Id == @class.OriginalId)?.LongName;

            if ((classStr is null && @class.Id != 0) || (@class.OriginalId is not null && orgClassStr is null))
                Logger.LogWarning($"Period load: period = {period.Id}{(classStr is null ? $", class not found = {@class.Id}" : string.Empty)}{((orgClassStr is null && @class.OriginalId is not null) ? $"original class not found = {@class.OriginalId}" : string.Empty)}");

            // Id = 0 means the class is cancelled
            if (@class.Id == 0)
                return (orgClassStr ?? "Err").StrikeThrough();

            string classString = classStr ?? "Err";
            if (@class.OriginalId is not null)
                classString += $" ({orgClassStr ?? "Err"})";
            return classString;
        });
    }

    public static IEnumerable<string> GetTeacherStrings(this Period period)
    {
        IEnumerable<Teacher> allTeachers = TeacherFile.s_DefaultInstance.Teachers;
        return period.TeacherIds.Select(teachers =>
        {
            Teacher? teacher = allTeachers.FirstOrDefault(s => s.Id == teachers.Id);
            Teacher? orgTeacher = allTeachers.FirstOrDefault(s => s.Id == teachers.OriginalId);
            string? teacherStr = teacher?.Title + teacher?.LongName;
            string? orgTeacherStr = orgTeacher?.Title + orgTeacher?.LongName;

            if ((teacherStr is null && teachers.Id != 0) || (teachers.OriginalId is not null && orgTeacherStr is null))
                Logger.LogWarning($"Period load: period = {period.Id}{(teacherStr is null ? $", teacher not found = {teachers.Id}" : string.Empty)}{((orgTeacherStr is null && teachers.OriginalId is not null) ? $", original teacher not found = {teachers.OriginalId}" : string.Empty)}");

            // Id = 0 means the teacher is cancelled
            if (teachers.Id == 0)
                return (orgTeacherStr ?? "Err").StrikeThrough();

            string teacherString = teacherStr == string.Empty || teacherStr is null ? "Err" : teacherStr;
            if (teachers.OriginalId is not null)
                teacherString += $" ({orgTeacherStr ?? "Err"})";

            return teacherString;
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
