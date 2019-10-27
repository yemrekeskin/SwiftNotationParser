using System;
using System.Collections.Generic;
using System.Text;

namespace SwiftNotationParser
{
    public static class StringExtention
    {
        public static bool HasValue(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }
    }
}
