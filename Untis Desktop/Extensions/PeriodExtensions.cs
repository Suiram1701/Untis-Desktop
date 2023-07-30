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
