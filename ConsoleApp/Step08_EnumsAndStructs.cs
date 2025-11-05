using System;

namespace ConsoleApp
{

    // Define an enum for days of the week
    public enum DayOfWeek
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday
    }

    // Define an enum for directions
    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    // Define an enum for traffic light states
    public enum TrafficLight
    {
        Red,
        Yellow,
        Green
    }   

    // Define a struct for a 2D point
    public struct Point 
    {
        public int X;
        public int Y;

        public Point(int x, int y) 
        {
            X = x;
            Y = y;
        }

        public void Move(int dx, int dy) 
        { 
            X += dx;
            Y += dy;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

    }

    public class Step08_EnumsAndStructs
    {
        public static void Run()
        {

            Console.WriteLine($"{Environment.NewLine}=== ENUMS ===");

            DayOfWeek today = DayOfWeek.Friday;
            Console.WriteLine($"Today is: {today}");

            Direction playerDirection = Direction.North;
            Console.WriteLine($"Player is facing: {playerDirection}");

            int directionValue = (int)playerDirection;
            Console.WriteLine($"Direction as number: {directionValue}");

            playerDirection = (Direction)2;
            Console.WriteLine($"New player direction: {playerDirection}");

            // -----------------------------------------------------------------

            Console.WriteLine($"{Environment.NewLine}=== STRUCTS ===");

            Point p1 = new Point(3, 4);
            Console.WriteLine($"Initial Point: {p1}");

            p1.Move(2, -1);
            Console.WriteLine($"After moving (2, -1): {p1}");

            Point p2 = new Point(0, 0);
            Console.WriteLine($"Another Point: {p2}");

            double distance = CalculateDistance(p1, p2);
            Console.WriteLine($"Distance between {p1} and {p2} is {distance:F2}");

            // -----------------------------------------------------------------

            Console.WriteLine($"{Environment.NewLine}=== TRAFFIC LIGHT ===");

            Console.Write($"Enter Traffic Light (Red, Yellow, Green): ");
            string input = Console.ReadLine();
            if (Enum.TryParse(input, true, out TrafficLight light))
            {
                string action = TrafficLightAction(light);
                Console.WriteLine($"Traffic Light is {light}: {action}");
            }
            else
            {
                Console.WriteLine("Invalid traffic light color.");
            }

        }

        // Method to calculate distance between two points
        static double CalculateDistance(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Method to get action based on traffic light
        static string TrafficLightAction(TrafficLight light)
        {
            switch (light)
            {
                case TrafficLight.Red:
                    return "Stop";
                case TrafficLight.Yellow:
                    return "Caution";
                case TrafficLight.Green:
                    return "Go";
                default:
                    return "Unknown signal";
            }
        }
    }
}