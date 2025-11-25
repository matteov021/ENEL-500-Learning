using System;
using System.ComponentModel;

namespace ConsoleApp
{
    public class PersonViewModel : INotifyPropertyChanged
    {
        private string _name;
        private int _age;
        private int _height;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name)); // Notify UI
                }
            }
        }

        public int Age
        {
            get => _age;
            set
            {
                if (_age != value)
                {
                    _age = value;
                    OnPropertyChanged(nameof(Age)); // Notify UI
                }
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Height)); // Notify UI
                }
            }
        }

        public void CelebrateBirthday()
        {
            Age += 1;
            Console.WriteLine($"Happy Birthday, {Name}! You are now {Age} years old!");
        }

        // --- INotifyPropertyChanged Implementation ---
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Step12_INotifyPropertyChanged
    {
        public static void Run()
        {
            PersonViewModel person = new PersonViewModel();
            person.PropertyChanged += Person_PropertyChanged;

            person.Name = "Alice";
            person.Age = 30;
            person.Height = 150;
            person.CelebrateBirthday();

            person.Name = "Bob";  // Triggers PropertyChanged
            person.Age = 35;
            person.Height = 180;
            person.CelebrateBirthday();
        }

        private static void Person_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var person = sender as PersonViewModel;
            Console.WriteLine($"Property '{e.PropertyName}' changed! New values: Name = {person.Name}, Age = {person.Age}, Height = {person.Height}");
        }
    }
}