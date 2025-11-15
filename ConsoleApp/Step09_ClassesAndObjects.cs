using System;

namespace ConsoleApp
{

    public class Dog
    {
        private string _name;
        private string _breed;
        private int _age;

        // Constructor
        public Dog(string name, int age, string breed)
        {
            _name = name;
            _age = age;
            _breed = breed;
        }

        // Method to make the dog bark
        public void Bark()
        {
            Console.WriteLine($"{_name} says: Woof! Woof!");
        }
        // Method to display dog's info
        public void DisplayInfo()
        {
            Console.WriteLine($"Name: {_name}, Age: {_age} years old, Breed: {_breed}");
        }
    }

    public class Person
    {
        private string _name;
        private int _age;
        
        // Constructor
        public Person(string name, int age)
        {
            _name = name;
            _age = age;
        }
        
        // Method to display person's info
        public void Introduce()
        {
            Console.WriteLine($"Hi, my name is {_name} and I'm {_age} years old.");
        }
    }

    public class BankAccount 
    { 
        private string _owner;
        private double _balance;

        // Constructor
        public BankAccount (string accountOwner , double startingBalance)
        {
            _owner = accountOwner;
            _balance = startingBalance;
        }

        // Read-only property for owner
        public string Owner
        {
            get { return _owner; }
        }

        // Read-only property for balance
        public double Balance
        {
            get { return _balance; }
        }

        // Method to deposit money
        public void Deposit (double amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Deposit must be positive.");
                return;
            }
            _balance += amount;
            Console.WriteLine($"Deposited ${amount:F2}. New balance: ${_balance:F2}");
        }

        // Method to withdraw money
        public void Withdraw(double amount)
        {
            if (amount > _balance)
            {
                Console.WriteLine("Insufficient funds.");
                return;
            }
            _balance -= amount;
            Console.WriteLine($"Withdrew ${amount:F2}. New balance: ${_balance:F2}");
        }

        // Method to display account info
        public void DisplayAccountInfo()
        {
            Console.WriteLine($"Account Owner: {_owner}, Balance: ${_balance:F2}");
        }

    }

    public class Step09_ClassesAndObjects
    {
        public static void Run()
        {

            Console.WriteLine($"{Environment.NewLine}=== BANK ACCOUNT DEMO ===");

            BankAccount account1 = new BankAccount("Alice", 1000);
            BankAccount account2 = new BankAccount("Bob", 500);

            account1.DisplayAccountInfo();
            account2.DisplayAccountInfo();

            Console.WriteLine();
            account1.Deposit(250);
            account1.Withdraw(100);
            account2.Withdraw(700);
            account2.Deposit(400);

            Console.WriteLine();
            account1.DisplayAccountInfo();
            account2.DisplayAccountInfo();

            Console.WriteLine($"{Environment.NewLine}=== PERSON DEMO ===");

            Person person1 = new Person("Charlie", 30);
            Person person2 = new Person("Diana", 25);
            person1.Introduce();
            person2.Introduce();

            Console.WriteLine($"{Environment.NewLine}=== DOG DEMO ===");

            Dog dog1 = new Dog("Buddy", 3, "Golden Retriever");
            Dog dog2 = new Dog("Max", 5, "Beagle");
            dog1.Bark();
            dog2.Bark();
            dog1.DisplayInfo();
            dog2.DisplayInfo();

        }
    }
}