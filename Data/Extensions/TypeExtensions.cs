using Data.Profiles;
using Data.Timetable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebUntisAPI.Client;

namespace Data.Extensions;

internal static class TypeExtensions
{
    /// <summary>
    /// Get all types that contains the methods SetProfile and UpdateFromClientAsync
    /// </summary>
    /// <returns>The types</returns>
    internal static IEnumerable<Type> GetProfileDependedTypes()
    {
        Assembly assembly = typeof(ProfileFile).Assembly;

        foreach (Type t in assembly.GetTypes()
            .Where(type => type.GetMethod(nameof(PeriodFile.SetProfile), BindingFlags.Static | BindingFlags.Public) is not null)
            .Where(type => type.GetMethod(nameof(PeriodFile.UpdateFromClientAsync), BindingFlags.Static | BindingFlags.Public) is not null))
        {
            yield return t;
        }
    }

    /// <summary>
    /// Run the SetProfile method
    /// </summary>
    /// <remarks>
    /// Use this only for types that are contained in the result of <see cref="GetProfileDependedTypes"/>
    /// </remarks>
    /// <param name="type"></param>
    /// <param name="profile">the profile parameter</param>
    public static void ExecuteSetProfile(this Type type, ProfileFile profile)
    {
        MethodInfo method = type.GetMethod(nameof(PeriodFile.SetProfile), BindingFlags.Static | BindingFlags.Public, new[] { typeof(ProfileFile) })!;
        method.Invoke(null, new[] { profile });
    }

    /// <summary>
    /// Run the UpdateFromClientAsync method
    /// </summary>
    /// <remarks>
    /// Use this only for types that are contained in the result of <see cref="GetProfileDependedTypes"/>
    /// </remarks>
    /// <param name="type"></param>
    /// <param name="client">The client parameter</param>
    /// <returns>The async task</returns>
    public static Task ExecuteUpdateFromClientAsync(this Type type, WebUntisClient client)
    {
        MethodInfo method = type.GetMethod(nameof(PeriodFile.UpdateFromClientAsync), BindingFlags.Static | BindingFlags.Public, new[] { typeof(WebUntisClient) })!;
        return (Task)method.Invoke(null, new[] { client })!;
    }
}
