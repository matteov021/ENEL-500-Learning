using System;

namespace ConsoleApp
{
    public class Step01_HelloWorld
    {
        public static void Run()
        {
            // Ask User Name
            Console.WriteLine("Hello, World!");
            Console.WriteLine("What Is Your Name?");
            var name = Console.ReadLine();

            // Ask User Age
            Console.WriteLine("What Is Your Age?");
            var age = Convert.ToInt32(Console.ReadLine());

            // Get Current Date and Time
            var currentDate = DateTime.Now;

            // Display Greeting With Name, Age, Date, and Time
            Console.WriteLine($"{Environment.NewLine}Hello, {name}, who is age {age}, on {currentDate:d}, at {currentDate:t}!");

        }
    }
}