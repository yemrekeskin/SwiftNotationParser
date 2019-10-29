using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SwiftNotationParser.Tests
{
    [TestClass]
    public class BicTests
    {
        [TestMethod]
        public void WHEN_invalid_bic_RETURN_bic()
        {
            String bicText = "HASP11HH";

            Bic bic = Bic.GetBic(bicText);

            Assert.IsNull(bic);
        }

        [TestMethod]
        public void WHEN_valid_bic_RETURN_bic()
        {
            String bicText = "HASPDEHHXXX";

            Bic bic = Bic.GetBic(bicText);

            Assert.IsNotNull(bic);
            Assert.AreEqual("HASP", bic.InstitutionCode);
            Assert.AreEqual("DE", bic.CountryCode);
            Assert.AreEqual("HH", bic.LocationCode);
            Assert.AreEqual("XXX", bic.BranchCode);            
        }

        [TestMethod]
        public void WHEN_valid_bic_without_branche_code_RETURN_bic()
        {
            String bicText = "HASPDEHH";

            Bic bic = Bic.GetBic(bicText);

            Assert.IsNotNull(bic);
            Assert.AreEqual("HASP", bic.InstitutionCode);
            Assert.AreEqual("DE", bic.CountryCode);
            Assert.AreEqual("HH", bic.LocationCode);
            Assert.AreEqual("", bic.BranchCode);
        }
    }
}
