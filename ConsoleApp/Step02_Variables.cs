using System;

namespace ConsoleApp
{
    public class Step02_Variables
    {
        public static void Run()
        {
            // Ask User Name
            Console.Write("What is your name? ");
            var name = Console.ReadLine();

            // Ask For 3 Numbers
            Console.Write("Enter the first number: ");
            double firstNumber = double.Parse(Console.ReadLine());

            Console.Write("Enter the second number: ");
            double secondNumber = double.Parse(Console.ReadLine());

            Console.Write("Enter the third number: ");
            double thirdNumber = double.Parse(Console.ReadLine());

            // Perform Calculations
            double sum = firstNumber + secondNumber + thirdNumber;
            double difference = firstNumber - secondNumber + thirdNumber;
            double product = firstNumber * secondNumber + thirdNumber;
            double quotient = firstNumber / secondNumber + thirdNumber;
            double average = sum / 3;

            // Display Results
            Console.WriteLine($"{Environment.NewLine}Hello, {name}! Here are your results:");
            Console.WriteLine($"{firstNumber} + {secondNumber} = {sum}");
            Console.WriteLine($"{firstNumber} - {secondNumber} = {difference}");
            Console.WriteLine($"{firstNumber} * {secondNumber} = {product}");
            Console.WriteLine($"{firstNumber} / {secondNumber} = {quotient}");
            Console.WriteLine($"The average of {firstNumber}, {secondNumber}, and {thirdNumber} is {average}");

        }
    }
}