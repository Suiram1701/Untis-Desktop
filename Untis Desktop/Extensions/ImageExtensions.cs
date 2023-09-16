using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace UntisDesktop.Extensions;

internal static class ImageExtensions
{
    public static void LoadImage(this Image img, Uri source)
    {
        BitmapImage image = new();
        image.BeginInit();
        image.UriSource = source;
        image.EndInit();

        img.Source = image;
    }
}
