using Data.Profiles;
using Data.Static;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
using System.Windows.Shapes;
using UntisDesktop.Extensions;
using UntisDesktop.Localization;
using UntisDesktop.UserControls;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Models;

namespace UntisDesktop.Views;

public partial class PeriodWindow : Window
{
    public static Color IrregularColor { get => StatusDataFile.s_DefaultInstance.StatusData.IrregularLessonColors.BackColor; }

    public static Color CancelledColor { get => StatusDataFile.s_DefaultInstance.StatusData.CancelledLessonColors.BackColor; }

    public Period Period { get; }

    public string WindowTitle { get => $"{SubjectsString} - {Period.StartTime:t}-{Period.EndTime:t} - {Period.Date:D}"; }

    public string SubjectsLabel
    {
        get => LangHelper.GetString(Period.SubjectsIds.Length > 1
            ? "PeriodWindow.Subjects"
            : "PeriodWindow.Subject");
    }
    public string SubjectsString
    {
        get => string.Join(", ", Period.GetSubjects().Select(subject => subject.code == Code.Cancelled
            ? subject.subjectString.StrikeThrough()
            : subject.subjectString));
    }

    public string TeachersString
    {
        get => string.Join(", ", Period.GetTeachers().Select(teacher => teacher.code == Code.Cancelled
        ? teacher.teacherString.StrikeThrough()
        : teacher.teacherString));
    }

    public string RoomsLabel
    {
        get => LangHelper.GetString(Period.RoomIds.Length > 1
            ? "PeriodWindow.Rooms"
            : "PeriodWindow.Room");
    }
    public string RoomsString
    {
        get => string.Join(", ", Period.GetRooms().Select(room => room.code == Code.Cancelled
        ? room.roomString.StrikeThrough()
        : room.roomString));
    }

    public string ClassesLabel
    {
        get => LangHelper.GetString(Period.ClassIds.Length > 1
            ? "PeriodWindow.Classes"
            : "PeriodWindow.Class");
    }
    public string ClassesString
    {
        get => string.Join(", ", Period.GetClasses().Select(@class => @class.code == Code.Cancelled
        ? @class.classString.StrikeThrough()
        : @class.classString)) + (string.IsNullOrEmpty(Period.StudentGroup) ? string.Empty : (" | " + Period.StudentGroup));
    }

    // Visibilities
    public bool HasSubjects { get => Period.SubjectsIds.Any(); }

    public bool HasTeachers { get => Period.TeacherIds.Any(); }

    public bool HasRooms { get => Period.RoomIds.Any(); }

    public bool HasClasses { get => Period.ClassIds.Any(); }

    public bool IsIrregular { get => Period.Code == Code.Irregular; }

    public bool IsCancelled { get => Period.Code == Code.Cancelled; }

    public bool HasInfo { get => !string.IsNullOrEmpty(Period.Info); }

    public bool HasSubstitutionInfo { get => !string.IsNullOrEmpty(Period.SubstitutionText); }

    public bool HasLessonText { get => !string.IsNullOrEmpty(Period.LessonText); }

    public bool HasActivityType { get => !string.IsNullOrEmpty(Period.ActivityType); }

    public PeriodWindow(Period period)
    {
        Period = period;
        InitializeComponent();

        // Apply saved size
        ProfileOptions options = ProfileCollection.GetActiveProfile().Options;
        Height = options.PeriodWindowSize.Height;
        Width = options.PeriodWindowSize.Width;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        // Save window size
        ProfileFile profile = ProfileCollection.GetActiveProfile();
        int height = (int)Math.Round(sizeInfo.NewSize.Height, 0);
        int width = (int)Math.Round(sizeInfo.NewSize.Width, 0);
        profile.Options.PeriodWindowSize = new(width, height);
        profile.Update();
    }
}
