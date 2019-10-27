using SwiftNotationParser;
using System;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            string notation = "3!a15d";
            string value = "EUR50000,00";

            notation = "1!a6!n3!a15d";
            value = "A" + "123456" + "ABC" + "1234,";

            notation = "3*5a"; // ?
            value = "A\nAA\nAAA";

            notation = "5n[/5n]";
            value = "123/11";

            notation = "5!a";
            value = "ABCDF";

            SwiftParser parser = new SwiftParser(notation);
            var result = parser.parse(value);

            Console.ReadLine();
        }
    }
}
