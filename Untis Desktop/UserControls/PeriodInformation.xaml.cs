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
using WebUntisAPI.Client;
using WebUntisAPI.Client.Models;

namespace UntisDesktop.UserControls;

public partial class PeriodInformation : UserControl
{
    public Color TargetColor { get; }

    public bool IsCancelled { get; }

    public string InformationString { get; }

    public PeriodInformation(Code code, string infString, Color? normalColor = default)
    {

        TargetColor = code switch
        {
            Code.None => normalColor ?? Color.Transparent,
            Code.Irregular => StatusDataFile.s_DefaultInstance.StatusData.IrregularLessonColors.BackColor,
            Code.Cancelled => StatusDataFile.s_DefaultInstance.StatusData.CancelledLessonColors.BackColor,
            _ => Color.Transparent,
        };
        IsCancelled = code == Code.Cancelled;
        InformationString = infString;

        InitializeComponent();
    }
}