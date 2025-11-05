using System;

namespace ConsoleApp
{

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

        }
    }
}