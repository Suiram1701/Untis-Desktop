using Data.Profiles;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using UntisDesktop.ViewModels;

namespace UntisDesktop.Extensions;

internal static class ProfileFileExtensions
{
    /// <summary>
    /// Get the profile image of the account
    /// </summary>
    /// <param name="profileFile"></param>
    /// <returns>The image, if this is <see langword="null"/> the image should be the default image</returns>
    public static async Task<Image?> GetProfileImageAsync(this ProfileFile profileFile)
    {
        bool isOffline = Application.Current.Dispatcher.Invoke(IsOffline);

        if (isOffline || App.Client is null)
            return profileFile.ProfileImage;

        (Image image, bool canRead, _) = await App.Client.GetOwnProfileImageAsync();

        // Delete the saved image when it was from WU depleted or when you don't have a read permission
        if (!canRead || image is null)
        {
            if (profileFile.ProfileImage != null)
            {
                profileFile.ProfileImage = null;
                profileFile.Update();
            }
            return null;
        }

        // Save the image when it isn't saved
        if (!profileFile.ShouldSerialize_ProfileImageEncoded())
        {
            profileFile.ProfileImage = image;
            profileFile.Update();
        }

        return image;
    }

    private static bool IsOffline()
    {
        return Application.Current.Windows
            .Cast<Window>()
            .Select(w => w.DataContext)
            .OfType<WindowViewModelBase>()
            .Any(d => d.IsOffline);
    }
}
