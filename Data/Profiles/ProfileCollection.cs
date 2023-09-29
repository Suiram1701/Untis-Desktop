using Data.Messages;
using Data.Static;
using Data.Timetable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebUntisAPI.Client;

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

            // Statics
            TeacherFile.SetProfile(profile);
            Task teacherTask = TeacherFile.UpdateFromClientAsync(client);

            RoomFile.SetProfile(profile);
            Task roomTask = RoomFile.UpdateFromClientAsync(client);

            SubjectFile.SetProfile(profile);
            Task subjectTask = SubjectFile.UpdateFromClientAsync(client);

            ClassFile.SetProfile(profile);
            Task classesTask = ClassFile.UpdateFromClientAsync(client);

            StatusDataFile.SetProfile(profile);
            Task statusDataTask = StatusDataFile.UpdateFromClientAsync(client);

            LanguageFile.SetProfile(profile);
            Task languageTask = LanguageFile.UpdateFromClientAsync(client);

            // Timetable
            SchoolYearFile.SetProfile(profile);
            Task schoolYearsTask = SchoolYearFile.UpdateFromClientAsync(client);

            TimegridFile.SetProfile(profile);
            Task timegridTask = TimegridFile.UpdateFromClientAsync(client);

            PeriodFile.SetProfile(profile);
            Task periodsTask = PeriodFile.UpdateFromClientAsync(client);

            HolidaysFile.SetProfile(profile);
            Task holidaysTask = HolidaysFile.UpdateFromClientAsync(client);

            // Messages
            MessagePermissionsFile.SetProfile(profile);
            Task messagePermissionTask = MessagePermissionsFile.UpdateFromClientAsync(client);

            await Task.WhenAll(teacherTask, roomTask, subjectTask, classesTask, statusDataTask, languageTask, schoolYearsTask, timegridTask, periodsTask, holidaysTask, messagePermissionTask);
        }
    }
}
