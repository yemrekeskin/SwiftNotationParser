using System;
using System.Collections.Generic;
using System.Text;

namespace SwiftNotationParser
{
    public class Bic
    {
        public string InstitutionCode { get; set; }
        public string CountryCode { get; set; }
        public string LocationCode { get; set; }
        public string BranchCode { get; set; }
        
        private static SwiftParser SwiftParser = new SwiftParser("4!a2!a2!c[3!c]");

        public Bic(String InstitutionCode, String CountryCode, String LocationCode, String BranchCode)
        {
            if (String.IsNullOrEmpty(InstitutionCode)) throw new Exception("ERROR: InstitutionCode not empty");
            if (String.IsNullOrEmpty(CountryCode))     throw new Exception("ERROR: CountryCode not empty");
            if (String.IsNullOrEmpty(LocationCode))    throw new Exception("ERROR: LocationCode not empty");
            
            this.InstitutionCode = InstitutionCode;
            this.CountryCode = CountryCode;
            this.LocationCode = LocationCode;
            this.BranchCode = BranchCode;

            BranchCode = String.IsNullOrEmpty(this.BranchCode) ? "" : this.BranchCode;

            String bicText = this.InstitutionCode + this.CountryCode + this.LocationCode + this.BranchCode;
            EnsureValid(bicText);
        }

        public static Bic GetBic(String value)
        {
            List<string> subfieldList = SwiftParser.parse(value);
            string institutionCode = subfieldList[0];
            string countryCode = subfieldList[1];
            string locationCode = subfieldList[2];
            string branchCode = subfieldList[3];
            
            return new Bic(institutionCode, countryCode, locationCode, branchCode);
        }

        private void EnsureValid(string bicText)
        {
            SwiftParser.parse(bicText);
        }
    }
}
