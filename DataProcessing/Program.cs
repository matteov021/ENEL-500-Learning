using System;
using System.IO.Ports;

namespace main
{
    internal class Program
    {
        static SerialPort myPort;

        static string systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        static string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        static void Main(string[] args)
        {
            myPort = new SerialPort("COM8", 115200, Parity.None, 8, StopBits.One);

            // COM10 will be the send the message and also event sender (?)
            // Attach the Event Handler for receiving data
            myPort.DataReceived += new SerialDataReceivedEventHandler(DataHandler);

            myPort.Open();
            Console.WriteLine("Listening for STM data on COM8... Press any key to exit.");
            while (true)
            {
                // Send the message
                Console.WriteLine("Sent hello");
                myPort.Write("hello\n");
                Thread.Sleep(1000);
            }
        }

        // Handler is static therefore it is active the entire execution of program.
        // Can change to dynamic if we add conditional logic to activate the handler only in certain conditions.
        private static void DataHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string incomingData = sp.ReadExisting();
            Console.WriteLine($"Received from STM: {incomingData}");

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(systemPath, "TextPractice.txt"), append:true))
            {
                    outputFile.WriteLine(incomingData + ',');
            }

        }
    }
}
