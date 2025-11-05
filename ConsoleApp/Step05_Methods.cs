using System;

namespace ConsoleApp
{
    public class Step05_Methods
    {
        public static void Run()
        {
            // Methods Example
            Console.WriteLine("=== Methods Example ===");

            // Ask For 2 Numbers
            Console.Write("Enter First Number: ");
            double firstNumber = double.Parse(Console.ReadLine());
            
            Console.Write("Enter Second Number: ");
            double secondNumber = double.Parse(Console.ReadLine());

            // Perform Calculations Using Methods
            double sum = Add(firstNumber, secondNumber);
            double difference = Subtract(firstNumber, secondNumber);
            double product = Multiply(firstNumber, secondNumber);
            double quotient = Divide(firstNumber, secondNumber);
            double max = Max(firstNumber, secondNumber);  

            // Display Results
            Console.WriteLine($"{Environment.NewLine}Here are your results:");
            Console.WriteLine($"{firstNumber} + {secondNumber} = {sum}");
            Console.WriteLine($"{firstNumber} - {secondNumber} = {difference}");
            Console.WriteLine($"{firstNumber} * {secondNumber} = {product}");
            Console.WriteLine($"{firstNumber} / {secondNumber} = {quotient}");
            Console.WriteLine($"The larger number is {max}");
            Console.WriteLine($"Is the number 4 even? {isEven(4)}");
            Console.WriteLine($"Is the number 5 even? {isEven(5)}");

        }

        // Add Method
        static double Add(double firstNumber, double secondNumber) 
        { 
            return firstNumber + secondNumber;   
        }

        // Subtract Method
        static double Subtract(double firstNumber, double secondNumber)
        {
            return firstNumber - secondNumber;
        }
        
        // Multiply Method
        static double Multiply(double firstNumber, double secondNumber)
        {
            return firstNumber * secondNumber;
        }

        // Divide Method With Zero Check
        static double Divide(double firstNumber, double secondNumber)
        {
            if (secondNumber == 0)
            {
                Console.WriteLine($"{Environment.NewLine}Cannot Divide By Zero!");
                return 0;
            }
            
            return firstNumber / secondNumber;
        }

        // Max Method
        static double Max(double firstNumber, double secondNumber) 
        {
            if (firstNumber > secondNumber) 
            {
                return firstNumber;
            }

            return secondNumber;
        }

        // IsEven Method
        static bool isEven(int n) 
        { 
            return n % 2 == 0;
        }

    }
}