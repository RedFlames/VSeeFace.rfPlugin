
using System;

namespace rfPlugin;

public static class StringExt
{
    // because uhh yea why can't I easily to string.Contains but case-insensitive
    public static bool IContains(this string str, string search) => str.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
}