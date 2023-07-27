using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebUntisAPI.Client.Models;

namespace UntisDesktop.Extensions;

internal static class PeriodExtensions
{
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
