using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UntisDesktop.Extensions;

internal static class StringExtensions
{
    public static string StrikeThrough(this string input)
    {
        StringBuilder result = new();
        foreach (char c in input)
            result.Append(c + "\u0336");
        return result.ToString();
    }
}
