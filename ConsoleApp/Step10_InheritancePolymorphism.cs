using System;

namespace ConsoleApp
{

    public abstract class Animal
    {
        // Property
        public string Name { get; set; }

        // Constructor
        public Animal(string name)
        {
            Name = name;
        }

        // Virtual = can be overridden by subclasses
        public abstract void Speak();

        // Another virtual method
        public abstract void Move();
    }

    public class Dawg : Animal
    {
        // Constructor
        public Dawg(string name) : base(name) { }

        // Override = provides new implementation of base class method
        public override void Speak()
        {
            Console.WriteLine($"{Name} says: Woof!");
        }

        // Override another method
        public override void Move()
        {
            Console.WriteLine($"{Name} runs.");
        }
    }

    public class Cat : Animal
    {
        // Constructor
        public Cat(string name) : base(name) { }

        // Override
        public override void Speak()
        {
            Console.WriteLine($"{Name} says: Meow!");
        }

        // Override another method
        public override void Move()
        {
            Console.WriteLine($"{Name} sneaks.");
        }
    }

    public class Bird : Animal
    {
        // Constructor
        public Bird(string name) : base(name) { }

        // Override
        public override void Speak()
        {
            Console.WriteLine($"{Name} says: Tweet!");
        }

        // Override another method
        public override void Move()
        {
            Console.WriteLine($"{Name} flies.");
        }
    }

    public class Cow : Animal
    {
        // Constructor
        public Cow(string name) : base(name) { }

        // Override
        public override void Speak()
        {
            Console.WriteLine($"{Name} says: Moo!");
        }

        public override void Move()
        {
            Console.WriteLine($"{Name} walks slowly.");
        }
    }

    public class Step10_InheritancePolymorphism
    {
        public static void Run()
        {
            Console.WriteLine("=== INHERITANCE & POLYMORPHISM ===");

            Animal[] animals = new Animal[]
            {
                new Dawg("Rex"),
                new Cat("Whiskers"),
                new Bird("Sky"),
                new Cow("Bessie")
            };

            foreach (Animal a in animals)
            {
                a.Speak(); // Polymorphism here!
            }

            Console.WriteLine();

            foreach (Animal a in animals)
            {
                a.Move();  // Polymorphism here!
            }
        }
    }
}