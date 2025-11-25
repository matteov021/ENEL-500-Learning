using System;

namespace ConsoleApp
{

    public interface IMovable
    {
        void Move();
    }

    public interface ISpeakable
    {
        void Speak();
    }

    public interface IEatable 
    {
        void Eat();
    }

    public class Robot : IMovable, ISpeakable
    {
        public string Name { get; private set; }

        public Robot(string name)
        {
            Name = name;
        }

        public void Move()
        {
            Console.WriteLine($"{Name} moves on wheels.");
        }

        public void Speak()
        {
            Console.WriteLine($"{Name} says: 'Beep boop.'");
        }
    }

    public class Human : IMovable, ISpeakable, IEatable
    {
        public string Name { get; private set; }

        public Human(string name)
        {
            Name = name;
        }

        public void Move()
        {
            Console.WriteLine($"{Name} walks on two legs.");
        }

        public void Speak()
        {
            Console.WriteLine($"{Name} says: 'Hello!'");
        }

        public void Eat()
        {
            Console.WriteLine($"{Name} is eating food.");
        }
    }

    public class Vehicle : IMovable
    {
        public string Model { get; private set; }

        public Vehicle(string model)
        {
            Model = model;
        }

        public void Move()
        {
            Console.WriteLine($"{Model} drives forward.");
        }
    }

    public class Step11_Interfaces
    {
        public static void Run()
        {

            Console.WriteLine("=== INTERFACES DEMO ===");

            IMovable[] movables = new IMovable[]
            {
                new Robot("R2-D2"),
                new Human("Alice"),
                new Vehicle("Honda Civic")
            };

            ISpeakable[] speakingThings = new ISpeakable[]
            {
                new Robot("C-3PO"),
                new Human("Bob")
            };

            IEatable[] eatingThings = new IEatable[]
            {           
                new Human("Bob"),
                new Human("Alice")
            };

            Console.WriteLine($"{Environment.NewLine}--- Movable Things ---");
            foreach (var m in movables)
            {
                m.Move();
            }

            Console.WriteLine($"{Environment.NewLine}--- Speaking Things ---");
            foreach (var s in speakingThings)
            {
                s.Speak();
            }

            Console.WriteLine($"{Environment.NewLine}--- Eating Things ---");
            foreach (var s in eatingThings)
            {
                s.Eat();
            }
        }
    }
}