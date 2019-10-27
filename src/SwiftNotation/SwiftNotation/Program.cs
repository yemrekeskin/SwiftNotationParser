using System;

namespace SwiftNotation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string notation = "3!a15d";
            string value = "EUR50000,00";

            notation = "1!a6!n3!a15d";
            value = "A" + "123456" + "ABC" + "1234,";

            SwiftNotation swift = new SwiftNotation(notation);
            var result = swift.parse(value);

            Console.ReadLine();
        }
    }
}
