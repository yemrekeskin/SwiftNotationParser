using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SwiftNotationParser.Tests
{
    [TestClass]
    public class TagTests
    {
        [TestMethod]
        public void WHEN_valid_Tag()
        {
            string notation = "3!a15d";
            string value = "EUR50000,00";

            SwiftParser parser = new SwiftParser(notation);
            var result = parser.parse(value);

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void WHEN_valid_Tag2()
        {
            string notation = "1!a6!n3!a15d";
            string value = "A123456ABC1234,";

            SwiftParser parser = new SwiftParser(notation);
            var result = parser.parse(value);

            Assert.AreEqual(4, result.Count);
        }
    }
}
