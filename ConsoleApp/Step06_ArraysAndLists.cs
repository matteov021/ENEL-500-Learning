using System;
using System.Globalization;

namespace ConsoleApp
{
    public class Step06_ArraysAndLists
    {
        public static void Run()
        {

            Console.WriteLine("=== ARRAYS ===");

            int[] numbers = { 10, 20, 30, 40, 50 };

            // Accessing Elements
            Console.WriteLine($"First Number: {numbers[0]}");
            Console.WriteLine($"Last Number: {numbers[numbers.Length - 1]}");

            // Modifying Elements
            Console.WriteLine($"{Environment.NewLine}All Numbers:");
            for (int i = 0; i < numbers.Length; i++)
            {
                Console.WriteLine($"Index {i}: {numbers[i]}");
            }

            // Calculating Sum
            int sum = 0;
            foreach (int number in numbers)
            {
                sum += number;
            }
            Console.WriteLine($"Sum of numbers = {sum}");

            Console.WriteLine("\n=== LISTS ===");

            // Using Lists
            List<string> fruits = new List<string>();
            fruits.Add("Apple");
            fruits.Add("Banana");
            fruits.Add("Cherry");

            // Displaying List Elements
            Console.WriteLine("Fruits list:");
            foreach (string fruit in fruits)
            {
                Console.WriteLine(fruit);
            }

            // Modifying List
            fruits.Add("Mango");
            fruits.Remove("Banana");

            // Displaying Updated List
            Console.WriteLine($"{Environment.NewLine}Updated Fruits list:");
            foreach (string fruit in fruits)
            {
                Console.WriteLine(fruit);
            }

            // Checking for Existence
            if (fruits.Contains("Apple"))
            {
                Console.WriteLine($"{Environment.NewLine}We Have Apples");
            }

            // Displaying Count
            Console.WriteLine($"{Environment.NewLine}We now have {fruits.Count} fruits in the list.");

            // Extra Example: Average of User-Entered Numbers
            int[] nums = new int[5];
            Console.Write($"{Environment.NewLine}Enter numbers to add to the array: {Environment.NewLine}");
            for (int i = 0; i < 5; i++)
            {
                Console.Write($"Enter Number {i + 1}: ");
                nums[i] = int.Parse(Console.ReadLine());
            }
            double average = nums.Average();
            Console.WriteLine($"The Average is {average}");

            // Extra Example: Dynamic List Input
            List<string> items = new List<string>();
            string input = "";
            while (input != "done") 
            { 
                Console.Write($"{Environment.NewLine}Enter an item to add to the list (or type 'done' to finish): ");
                input = Console.ReadLine();
                if (input != "done")
                {
                    items.Add(input);
                }
            }
            foreach (string item in items)
            {
                Console.WriteLine($"Item: {item}");
            }
        }
    }
}