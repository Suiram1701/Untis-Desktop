using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UntisDesktop.Extensions;

internal static class SizeExtensions
{
    public static Size ConvertToWinSize(this System.Drawing.Size size)
    {
        return new(size.Width, size.Height);
    }
}
