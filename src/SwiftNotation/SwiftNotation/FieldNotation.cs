using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;

namespace SwiftNotation
{
    public class FieldNotation
    {
        public const string FIXED_LENGTH_SIGN = "!";
        public const string RANGE_LENGTH_SIGN = "-";
        public const string MULTILINE_LENGTH_SIGN = "*";


        public bool optional { get; set; }
        public string prefix { get; set; }
        public string charSet { get; set; }
        public int length0 { get; set; }
        public int? length1 { get; set; }
        public string lengthSign { get; set; }

        public FieldNotation(bool optional,
                             string prefix,
                             string charSet, 
                             int length0,
                             int? length1,
                             string lengthSign)
        {
            //if (optional == null) throw new Exception("Error:CanNotBeNull");
            //if (charSet == null) throw new Exception("Error:CanNotBeNull");
            //if (length0 == null) throw new Exception("Error:CanNotBeNull");

            this.optional = optional;
            this.prefix = prefix;
            this.charSet = charSet;
            this.length0 = length0;
            this.length1 = length1;
            this.lengthSign = lengthSign;
            
            if (!String.IsNullOrEmpty(this.lengthSign))
            {
                Contract.Requires(!this.length1.HasValue, String.Format("Missing field length sign between field lengths: {0}", this));
            }
            else switch (this.lengthSign)
                 {
                    case FIXED_LENGTH_SIGN:
                        Contract.Requires(!this.length1.HasValue, String.Format("Unexpected field length after fixed length sign {0} : {1} ", FIXED_LENGTH_SIGN, this));
                        break;
                    case RANGE_LENGTH_SIGN:
                        Contract.Requires(!this.length1.HasValue, String.Format("Missing field length after range length sign {0} : {1} ", RANGE_LENGTH_SIGN, this));

                        break;
                    case MULTILINE_LENGTH_SIGN:
                        Contract.Requires(!this.length1.HasValue, String.Format("Missing field length after multiline length sign {0} : {1} ", MULTILINE_LENGTH_SIGN, this));
                        break;
                    default:
                        Contract.Requires(!this.length1.HasValue, String.Format("Unknown length sign :  {0} ", this.ToString()));
                        break;
                 }
        }

        public override string ToString()
        {
            String fieldNotation = "";
            if (!String.IsNullOrEmpty(prefix))
            {
                fieldNotation += prefix;
            }

            fieldNotation += length0;
            if (!String.IsNullOrEmpty(lengthSign))
            {
                fieldNotation += lengthSign;
                if (lengthSign.Equals(RANGE_LENGTH_SIGN) || lengthSign.Equals(MULTILINE_LENGTH_SIGN))
                {
                    fieldNotation += length1;
                }
            }
            fieldNotation += charSet;
            if (optional)
            {
                fieldNotation = "[" + fieldNotation + "]";
            }
            return fieldNotation;
        }
    }
}
