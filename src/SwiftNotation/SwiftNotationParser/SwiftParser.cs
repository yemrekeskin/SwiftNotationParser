using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SwiftNotationParser
{
    public class SwiftParser
    {
        public string Notation { get; set; }
        private List<FieldNotation> FieldNotations;
        private List<string> SubFieldNotationPatterns;

        public SwiftNotations SwiftNotations { get; set; }
        
        public SwiftParser(string notation)
        {
            this.Notation = notation;
            this.SwiftNotations = new SwiftNotations();

            this.FieldNotations = ParseFieldNotations(notation);
            this.SubFieldNotationPatterns = GenerateSubFieldNotationPatterns(this.FieldNotations);
        }

        public List<FieldNotation> ParseFieldNotations(string swiftNotation)
        {
            List<FieldNotation> result = new List<FieldNotation>();

            string fieldNotationPattern = SwiftNotations.FULL_FIELD_NOTATION_PATTERN;
            MatchCollection collection = Regex.Matches(swiftNotation, fieldNotationPattern);

            for (int i = 0; i < collection.Count; i++)
            {
                //parseIndex = collection[i].Index;

                GroupCollection fieldNotations = collection[i].Groups;
                string fieldNotation = fieldNotations[0].Value;

                // trim optional indicator
                String trimmedFieldNotation = fieldNotation.Replace("^\\[(.*)\\]$", "$1");
                Match fieldPropertiesMatcher = Regex.Match(trimmedFieldNotation, SwiftNotations.FIELD_NOTATION_PATTERN);

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
        public List<string> GenerateSubFieldNotationPatterns(List<FieldNotation> fieldNotations)
        {
            List<string> result = new List<string>(fieldNotations.Count);

            int fieldIndex = -1;
            for (int i = 0; i < fieldNotations.Count; i++)
            {
                FieldNotation fieldNotation = fieldNotations[i];
                fieldIndex++;

                // select charset
                String charSetRegex = SwiftNotations.CHARSET_REGEX_MAP.GetValueOrDefault(fieldNotation.CharSet);
                if (charSetRegex == null)
                {
                    throw new Exception("Unknown charset: " + fieldNotation.CharSet);
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
                    if (upcomingFieldNotation.Prefix.HasValue())
                    {
                        fieldDelimiterList.Add(SwiftNotations.SEPARATOR_MAP.GetValueOrDefault(upcomingFieldNotation.Prefix));
                    }
                    if (!upcomingFieldNotation.Optional)
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
                string lengthSign = fieldNotation.LengthSign;
                if (!lengthSign.HasValue())
                {
                    int maxCharacters = fieldNotation.Length0;
                    subFieldRegex = "(:?" + delimiterLookaheadRegex + charSetRegex + ")" + "{1," + maxCharacters + "}";
                }
                else
                {
                    switch (lengthSign)
                    {
                        case FieldNotation.FIXED_LENGTH_SIGN:
                            {
                                int fixedCharacters = fieldNotation.Length0;
                                subFieldRegex = "(:?" + delimiterLookaheadRegex + charSetRegex + ")" + "{" + fixedCharacters + "}";
                                break;
                            }
                        case FieldNotation.RANGE_LENGTH_SIGN:
                            {
                                int minCharacters = fieldNotation.Length0;
                                int maxCharacters = fieldNotation.Length1.Value;
                                subFieldRegex = "(:?" + delimiterLookaheadRegex + charSetRegex + ")" + "{" + minCharacters + "," + maxCharacters + "}";
                                break;
                            }
                        case FieldNotation.MULTILINE_LENGTH_SIGN:
                            {
                                int maxLines = fieldNotation.Length0;
                                int maxLineCharacters = fieldNotation.Length1.Value;
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
                string prefix = fieldNotation.Prefix;
                if (prefix.HasValue())
                {
                    subFieldRegex = SwiftNotations.SEPARATOR_MAP.GetValueOrDefault(prefix) + subFieldRegex;
                }

                // make field optional if so
                if (fieldNotation.Optional)
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
            foreach (FieldNotation fieldNotation in this.FieldNotations)
            {
                fieldIndex++;
                string fieldPattern = this.SubFieldNotationPatterns[fieldIndex];
                
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
                    MatchCollection matches = Regex.Matches(fieldValue, SwiftNotations.DECIMAL_NUMBER_PATTERN);
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
