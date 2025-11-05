using System;
using System.Globalization;

namespace ConsoleApp
{
    public class Step07_Strings
    {
        public static void Run()
        {
            Console.WriteLine("=== STRING BASICS ===");

            string firstName = "Alice";
            string lastName = "Smith";
            int age = 30;

            // Concatenation (joining strings)
            string message = "Hello, my name is " + firstName + " " + lastName + ".";
            Console.WriteLine(message);

            // String interpolation (easier & preferred)
            Console.WriteLine($"I am {age} years old.");

            // String length
            Console.WriteLine($"Your name has {firstName.Length + lastName.Length} letters.");

            // Access individual characters
            Console.WriteLine($"The first letter of your name is {firstName[0]}.");

            // -----------------------------------------------------------------

            Console.WriteLine($"{Environment.NewLine}=== STRING METHODS ===");

            string sample = "   C# Programming is FUN!   ";

            Console.WriteLine($"Original: '{sample}'");
            Console.WriteLine($"Trimmed: '{sample.Trim()}'");
            Console.WriteLine($"Uppercase: {sample.ToUpper()}");
            Console.WriteLine($"Lowercase: {sample.ToLower()}");
            Console.WriteLine($"Replace: {sample.Replace("FUN", "awesome")}");
            Console.WriteLine($"Contains 'C#'? {sample.Contains("C#")}");
            Console.WriteLine($"Index of 'P': {sample.IndexOf('P')}");

            // -----------------------------------------------------------------

            Console.WriteLine($"{Environment.NewLine}=== SPLIT AND JOIN ===");

            string csv = "apple,banana,cherry,mango";
            string[] fruits = csv.Split(','); // split into array

            Console.WriteLine("Fruits from CSV:");
            foreach (string fruit in fruits)
            {
                Console.WriteLine(fruit);
            }

            string joined = string.Join(" | ", fruits);
            Console.WriteLine($"Joined back: {joined}");

            // -----------------------------------------------------------------

            Console.WriteLine($"{Environment.NewLine}=== FORMATTING ===");

            double price = 19.99;
            DateTime today = DateTime.Now;

            Console.WriteLine($"Price: {price:C}"); // currency format
            Console.WriteLine($"Date: {today:dddd, MMMM dd yyyy}"); // formatted date

            // -----------------------------------------------------------------

            Console.WriteLine($"{Environment.NewLine}=== WORD COUNTER ===");

            Console.Write($"{Environment.NewLine}Please enter a sentence: ");
            string sentence = Console.ReadLine();
            string[] words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);    
            Console.WriteLine($"You entered {words.Length} words.");

            // -----------------------------------------------------------------

            Console.WriteLine($"{Environment.NewLine}=== WORD REVERSER ===");

            Console.Write($"{Environment.NewLine}Please enter a word: ");
            string input = Console.ReadLine();
            char[] chars = input.ToCharArray();
            Array.Reverse(chars);
            string reversed = new string(chars);
            Console.WriteLine($"Reversed: {reversed}");

        }
    }
}