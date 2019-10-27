using System;
using System.Collections.Generic;

namespace SwiftNotationParser
{
    /**
     * 
     * a = alphabetic, A through Z, upper case only
     * n = numeric digits, 0 through 9 only
     * c = alpha-numeric capital letters and numeric digits only ( a &amp; n above )
     * d = decimals, including decimal comma ',' preceding the fractional part. The fractional part may be missing, but the decimal comma must always be present
     * e = space
     * s = sign ( + or _ )
     * h = hex ( 0 to 9, A to F)
     * 
     * x = SWIFT X character set : SPACE, A to Z, a to z, 0 to 9, and  + - / ? . : , ( ) '                   and CrLF
     * y = SWIFT Y character set : SPACE, A to Z, a to z, 0 to 9, and  + - / ? . : , ( ) ' = ! &quot; % &amp; * &lt; &gt; ;
     * z = SWIFT Z character set : SPACE, A to Z, a to z, 0 to 9, and  + - / ? . : , ( ) ' = ! &quot; % &amp; * &lt; &gt; ; and CrLf
     * 
     * A = alphabetic, A through Z, upper and lower case
     * B = alphanumeric upper case or lower case, and numeric digits
     * 
     * length specification:
     * nn = maximum length ( minimum is 1 )
     * nn-nn = minimum and maximum length
     * nn! = fixed length
     * nn*nn = maximum number of lines time maximum line length - Will always be the last field
     * 
     * separators
     * LSep = left separator (&quot;/&quot;, &quot;//&quot;, &quot;BR&quot; for CrLf, &quot;ISIN &quot;, etc.), field starts with the character specified
     * RSep = right separator (&quot;/&quot;, &quot;//&quot;, &quot;BR&quot; for CrLf, &quot;ISIN &quot;, etc.), field ends with the character specified
     * 
     * examples
     * 6!n = 6 numeric, fixed length
     * 6n = numeric up to 6 characters
     * 1!e = one blank space
     * 6*50x = up to 6 lines of up to 50 characters
     * 
     */
    public class SwiftNotations
    {
        public const string DECIMAL_NUMBER_PATTERN = "[0-9]+,[0-9]*";

        public readonly Dictionary<string, string> SEPARATOR_MAP = new Dictionary<string, string>();
        public readonly Dictionary<string, string> CHARSET_REGEX_MAP = new Dictionary<string, string>();

        /**
        * Group 1: Field Prefix
        * Group 2: Field length0
        * Group 3: Field length sign ! - *
        * Group 4: Field length1
        * Group 5: Field charset
        */
        public readonly string FIELD_NOTATION_PATTERN;
        public readonly string FULL_FIELD_NOTATION_PATTERN;

        public SwiftNotations()
        {
            // see class description for separator details
            SEPARATOR_MAP.Add("/", "/");
            SEPARATOR_MAP.Add("//", "//");
            SEPARATOR_MAP.Add("BR", "\n");

            // see class description for charset details
            CHARSET_REGEX_MAP.Add("a", "[A-Z]");
            CHARSET_REGEX_MAP.Add("n", "[0-9]");
            CHARSET_REGEX_MAP.Add("c", "[0-9A-Z]");
            CHARSET_REGEX_MAP.Add("d", "[0-9,]");
            CHARSET_REGEX_MAP.Add("e", " ");
            CHARSET_REGEX_MAP.Add("s", "[+_]");
            CHARSET_REGEX_MAP.Add("h", "[0-9A-F]");
            CHARSET_REGEX_MAP.Add("x", "[ 0-9A-Za-z+-/?.:,()'\\n]");
            CHARSET_REGEX_MAP.Add("y", "[ 0-9A-Za-z+-/?.:,()'=!\"%&*<>;]");
            CHARSET_REGEX_MAP.Add("z", "[ 0-9A-Za-z+-/?.:,()'=!\"%&*<>;\\n]");
            CHARSET_REGEX_MAP.Add("A", "[A-Za-z]");
            CHARSET_REGEX_MAP.Add("B", "[0-9A-Za-z]");

            FIELD_NOTATION_PATTERN = "(" + String.Join("|", SEPARATOR_MAP.Keys) + ")?([0-9]{1,2})([!\\-*])?([0-9]{1,2})?([" + String.Join("", CHARSET_REGEX_MAP.Keys) + "])";
            FULL_FIELD_NOTATION_PATTERN = "\\[" + FIELD_NOTATION_PATTERN + "\\]" + "|" + FIELD_NOTATION_PATTERN;
        }
    }
}
