using System;
using System.Collections.Generic;
using System.Text;

namespace SwiftNotationParser
{
    public class FieldNotation
    {
        public const string FIXED_LENGTH_SIGN = "!";
        public const string RANGE_LENGTH_SIGN = "-";
        public const string MULTILINE_LENGTH_SIGN = "*";

        public bool Optional { get; set; }
        public string Prefix { get; set; }
        public string CharSet { get; set; }
        public int Length0 { get; set; }
        public int? Length1 { get; set; }
        public string LengthSign { get; set; }

        public FieldNotation(bool optional,
                             string prefix,
                             string charSet,
                             int length0,
                             int? length1,
                             string lengthSign)
        {
            this.Optional = optional;
            this.Prefix = prefix;
            this.CharSet = charSet;
            this.Length0 = length0;
            this.Length1 = length1;
            this.LengthSign = lengthSign;
        }
    }
}
