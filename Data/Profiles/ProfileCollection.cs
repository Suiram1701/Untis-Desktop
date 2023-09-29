using Data.Extensions;
using Data.Messages;
using Data.Static;
using Data.Timetable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebUntisAPI.Client;
using TypeExtensions = Data.Extensions.TypeExtensions;

namespace Data.Profiles;

public class ProfileCollection : FileCollectionBase<ProfileCollection, ProfileFile>
{
    public readonly static ProfileCollection s_DefaultInstance = LoadCollection();

    public static ProfileFile GetActiveProfile()
    {
        if (s_DefaultInstance.FirstOrDefault(profile => profile.IsActive == true) is ProfileFile profile)
            return profile;
        else
        {
            ProfileFile file = s_DefaultInstance.First();
            file.IsActive = true;
            file.Update();

            return file;
        }
    }

    public async static Task SetActiveProfileAsync(ProfileFile profile, WebUntisClient client)
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

            List<Task> updateTasks = new();
            foreach (Type type in TypeExtensions.GetProfileDependedTypes())
            {
                type.ExecuteSetProfile(profile);
                updateTasks.Add(type.ExecuteUpdateFromClientAsync(client));
            }

            await Task.WhenAll(updateTasks);
        }
    }
}
