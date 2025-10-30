using System;

namespace ConsoleApp
{
    public class Step04_Loops
    {
        public static void Run()
        {

            // For Loop Example
            Console.WriteLine("=== FOR LOOP EXAMPLE ===");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Number: {i}");
            }

            // While Loop Example
            Console.WriteLine("=== WHILE LOOP EXAMPLE ===");
            string input = "";
            while (input != "exit")
            {
                Console.Write("Type something (or 'exit' to stop): ");
                input = Console.ReadLine();
                Console.WriteLine($"You typed: {input}");
            }

            // For Each Loop Example
            Console.WriteLine("=== FOR EACH LOOP EXAMPLE ===");
            string[] fruits = { "Apple", "Banana", "Cherry" };
            foreach (string fruit in fruits)
            {
                Console.WriteLine($"Fruits: {fruit}");
            }

            // Extra Example: Multiplication
            Console.WriteLine("=== EXTRA EXAMPLE ===");
            Console.Write("Enter a number: ");
            int number = int.Parse(Console.ReadLine());
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine($"{number} x {i} = {number * i}");
            }

        }
    }
}