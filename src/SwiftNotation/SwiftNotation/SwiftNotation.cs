using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SwiftNotation
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
    public class SwiftNotation
    {
        private string DECIMAL_NUMBER_PATTERN = "[0-9]+,[0-9]*";

        private static Dictionary<string, string> SEPARATOR_MAP = new Dictionary<string, string>();
        private static Dictionary<string, string> CHARSET_REGEX_MAP = new Dictionary<string, string>();

        /**
         * Group 1: Field Prefix
         * Group 2: Field length0
         * Group 3: Field length sign ! - *
         * Group 4: Field length1
         * Group 5: Field charset
         */
        //private string FIELD_NOTATION_PATTERN = "(" + String.Join("|", SEPARATOR_MAP.Keys) + ")?([0-9]{1,2})([!\\-*])?([0-9]{1,2})?([" + String.Join("", CHARSET_REGEX_MAP.Keys) + "])";

        private String notation;
        private List<FieldNotation> swiftFieldNotations;
        private List<string> swiftFieldNotationPatterns;

        public SwiftNotation(string notation)
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

            this.notation = notation;
            this.swiftFieldNotations = parseSwiftNotation(notation);
            this.swiftFieldNotationPatterns = generateSubfieldPatterns(this.swiftFieldNotations);
        }

        public List<FieldNotation> parseSwiftNotation(string swiftNotation)
        {
            List<FieldNotation> result = new List<FieldNotation>();

            string FIELD_NOTATION_PATTERN = "(" + String.Join("|", SEPARATOR_MAP.Keys) + ")?([0-9]{1,2})([!\\-*])?([0-9]{1,2})?([" + String.Join("", CHARSET_REGEX_MAP.Keys) + "])";

            string fieldNotationPattern = "\\[" + FIELD_NOTATION_PATTERN + "\\]" + "|" + FIELD_NOTATION_PATTERN;
            MatchCollection collection = Regex.Matches(swiftNotation, fieldNotationPattern);

            //int parseIndex = 0;
            for (int i = 0; i < collection.Count; i++)
            {
                //if (collection[i].Index != parseIndex)
                //{
                //    throw new Exception("Parse error: Unexpected sign(s) near index " + parseIndex + " '" + swiftNotation + "'");
                //}
                //parseIndex = collection[i].Index;

                GroupCollection fieldNotations = collection[i].Groups;
                string fieldNotation = fieldNotations[0].Value;

                // trim optional indicator
                String trimmedFieldNotation = fieldNotation.Replace("^\\[(.*)\\]$", "$1");
                Match fieldPropertiesMatcher = Regex.Match(trimmedFieldNotation, FIELD_NOTATION_PATTERN);

                //if (!fieldPropertiesMatcher.Success)
                //{
                //    throw new Exception("Parse error: Unexpected sign(s) near index " + parseIndex + " '" + swiftNotation + "'");
                //}

                Boolean fieldOptional = fieldNotation.StartsWith("[");
                String fieldPrefix = fieldPropertiesMatcher.Groups[1].Value;
                int fieldLength0 = int.Parse(fieldPropertiesMatcher.Groups[2].Value);
                int fieldLength1 = String.IsNullOrEmpty(fieldPropertiesMatcher.Groups[4].Value) ? 0 : int.Parse(fieldPropertiesMatcher.Groups[4].Value);
                String fieldLengthSign = fieldPropertiesMatcher.Groups[3].Value;
                String fieldCharset = fieldPropertiesMatcher.Groups[5].Value;


                FieldNotation fieldNotationModel = new FieldNotation(
                        fieldOptional,
                        fieldPrefix,
                        fieldCharset,
                        fieldLength0,
                        fieldLength1,
                        fieldLengthSign);

                // add field
                result.Add(fieldNotationModel);
            }

            //if (parseIndex != swiftNotation.Length)
            //{
            //    throw new Exception("Parse error: Unexpected sign(s) near index " + parseIndex + " '" + swiftNotation + "'");
            //}
            return result;
        }

        /**
        * select charset
        * handle delimiter
        * handle length
        * group field value
        * handle prefix
        * set optional if so
        *
        * @param fieldNotationList
        * @return patterns for continuous field matching
        */
        public List<string> generateSubfieldPatterns(List<FieldNotation> fieldNotations)
        {
            Contract.Requires(fieldNotations != null, "fieldNotationList can't be null");

            List<string> result = new List<string>(fieldNotations.Count);

            int fieldIndex = -1;
            for (int i = 0; i < fieldNotations.Count; i++)
            {
                FieldNotation fieldNotation = fieldNotations[i];
                fieldIndex++;

                // select charset
                String charSetRegex = CHARSET_REGEX_MAP.GetValueOrDefault(fieldNotation.charSet);
                if (charSetRegex == null)
                {
                    throw new Exception("Unknown charset: " + fieldNotation.charSet);
                }

                // handle delimiter if any
                string delimiterLookaheadRegex = "";

                // collect possible delimiters
                List<string> fieldDelimiterList = new List<string>();
                var firstIndex = fieldIndex + 1;
                var lastIndex = fieldNotations.Count;
                List<FieldNotation> upcomingFieldNotations = fieldNotations.GetRange(firstIndex, lastIndex - firstIndex);

                foreach (FieldNotation upcomingFieldNotation in upcomingFieldNotations)
                {
                    if (upcomingFieldNotation.prefix.HasValue())
                    {
                        fieldDelimiterList.Add(SEPARATOR_MAP.GetValueOrDefault(upcomingFieldNotation.prefix));
                    }
                    if (!upcomingFieldNotation.optional)
                    {
                        break;
                    }
                }
                if (fieldDelimiterList.Count > 0)
                {
                    delimiterLookaheadRegex = "(?!" + String.Join("|", fieldDelimiterList) + ")";
                }


                // create field regex
                String subFieldRegex;

                // handle length
                string lengthSign = fieldNotation.lengthSign;
                if (!lengthSign.HasValue())
                {
                    int maxCharacters = fieldNotation.length0;
                    subFieldRegex = "(:?" + delimiterLookaheadRegex + charSetRegex + ")" + "{1," + maxCharacters + "}";
                }
                else
                {
                    switch (lengthSign)
                    {
                        case FieldNotation.FIXED_LENGTH_SIGN:
                            {
                                int fixedCharacters = fieldNotation.length0;
                                subFieldRegex = "(:?" + delimiterLookaheadRegex + charSetRegex + ")" + "{" + fixedCharacters + "}";
                                break;
                            }
                        case FieldNotation.RANGE_LENGTH_SIGN:
                            {
                                int minCharacters = fieldNotation.length0;
                                int maxCharacters = fieldNotation.length1.Value;
                                subFieldRegex = "(:?" + delimiterLookaheadRegex + charSetRegex + ")" + "{" + minCharacters + "," + maxCharacters + "}";
                                break;
                            }
                        case FieldNotation.MULTILINE_LENGTH_SIGN:
                            {
                                int maxLines = fieldNotation.length0;
                                int maxLineCharacters = fieldNotation.length1.Value;
                                String lineCharactersRegexRange = "{1," + maxLineCharacters + "}";
                                String lineRegex = "[^\\n]" + lineCharactersRegexRange;
                                subFieldRegex = "(?=" + lineRegex + "(\\n" + lineRegex + ")" + "{0," + (maxLines - 1) + "}" + "$)" // lookahead for maxLines
                                        + "(:?" + delimiterLookaheadRegex + "(:?" + charSetRegex + "|\\n)" + ")" // add new line character to charset
                                        + "{1," + (maxLines * maxLineCharacters + (maxLines - 1)) + "}$";  // calculate max length including newline signs
                                break;
                            }
                        default:
                            throw new Exception("Unsupported length sign '" + lengthSign + "'");
                    }
                }

                // group field value
                subFieldRegex = "(" + subFieldRegex + ")";

                // handle prefix
                string prefix = fieldNotation.prefix;
                if (prefix.HasValue())
                {
                    subFieldRegex = SEPARATOR_MAP.GetValueOrDefault(prefix) + subFieldRegex;
                }

                // make field optional if so
                if (fieldNotation.optional)
                {
                    subFieldRegex = "(?:" + subFieldRegex + ")?";
                }

                string pattern = "^" + subFieldRegex;
                result.Add(pattern);
            }
            return result;
        }

        public List<string> parse(String fieldText)
        {
            int parseIndex = 0;

            List<string> result = new List<string>();

            int fieldIndex = -1;
            foreach (FieldNotation fieldNotation in this.swiftFieldNotations)
            {
                fieldIndex++;
                string fieldPattern = swiftFieldNotationPatterns[fieldIndex];

                //Regex reg = new Regex(fieldPattern);
                //Match match = reg.Match(fieldText);

                string fieldNewText = fieldText.Substring(parseIndex, fieldText.Length - parseIndex);

                MatchCollection collection = Regex.Matches(fieldNewText, fieldPattern);
                if (collection.Count == 0)
                {
                    throw new Exception(
                       String.Format("Field does not match notation  {0} " + fieldNotation + ". " + "'" + fieldText.Substring(parseIndex) + "'", parseIndex));
                }
                String fieldValue = collection[0].Value;
                parseIndex = parseIndex + fieldValue.Length;

                // special handling for d charset due to only on comma constraint
                if (fieldPattern.Equals("d"))
                {
                    MatchCollection matches = Regex.Matches(fieldValue, DECIMAL_NUMBER_PATTERN);
                    if (matches.Count == 0)
                    {
                        //throw new Exception("Field does not match notation " + fieldNotation + ". "
                        //        + "'" + fieldText.Substring(parseIndex) + "'", parseIndex);
                    }
                }

                // add field value
                result.Add(fieldValue);
            }
            
            return result;
        }
    }
}
