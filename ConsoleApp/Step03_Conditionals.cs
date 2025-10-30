using System;

namespace ConsoleApp
{
    public class Step03_Conditionals
    {
        public static void Run()
        {

            // Ask User For A Number
            Console.Write("Enter a number: ");
            int number = int.Parse(Console.ReadLine());

            // Check If Number Is Positive, Negative, Or Zero
            if (number > 0)
            {
                Console.WriteLine("That number is positive");
            }
            else if (number < 0)
            {
                Console.WriteLine("That number is negative");
            }
            else
            {
                Console.WriteLine("That number is zero");
            }

            // Check If Number Is Even Or Odd
            if (number % 2 == 0)
            {
                Console.WriteLine("That number is even");
            }
            else 
            {
                Console.WriteLine("That number is odd");
            }

        }
    }
}