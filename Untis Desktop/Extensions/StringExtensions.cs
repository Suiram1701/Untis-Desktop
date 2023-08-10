using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UntisDesktop.Extensions;

internal static class StringExtensions
{
    public static string StrikeThrough(this string @string)
    {
        StringBuilder builder = new();
        foreach (char c in @string)
        {
            builder.Append(c);
            builder.Append('\u0335');
        }
        return builder.ToString();
    }
}
