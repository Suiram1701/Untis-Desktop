using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Profiles;

public class ProfileCollection : FileCollectionBase<ProfileCollection, ProfileFile>
{
    public readonly static ProfileCollection s_DefaultInstance = LoadCollection();

    public static ProfileFile GetActiveProfile()
    {
        return s_DefaultInstance.FirstOrDefault(profile => profile.IsActive == true) ?? s_DefaultInstance.First();
    }

    public static void SetActiveProfile(ProfileFile profile)
    {
        if (s_DefaultInstance.Any(p => p.Name == profile.Name))
        {
            foreach (ProfileFile p in s_DefaultInstance)
            {
                if (p.Name == profile.Name)
                    p.IsActive = true;
                else
                    p.IsActive = false;
            }

            foreach (ProfileFile p in s_DefaultInstance)
                p.Update();
        }
    }
}
