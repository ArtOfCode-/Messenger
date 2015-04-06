using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MessengerClient
{
    /// <summary>
    ///     The main class for this program.
    /// </summary>
    class Program
    {
        /// <summary>
        ///     The Thread representation of the Receiver class
        /// </summary>
        private static Thread receiverThread;

        /// <summary>
        ///     True if the program is running the entry point for the first time.
        /// </summary>
        private static bool FirstRun = true;

        /// <summary>
        ///     The main entry point for the program.
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        static void Main(string[] args)
        {
            if (FirstRun)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Clear();
                Application.ApplicationExit += new EventHandler(QuitClient);
                FirstRun = false;
            }

            if (args.Length == 1 && args[0] == "--debug")
            {
                Console.WriteLine("<DEBUG> Setting debug mode ON...");
                Output.DebugMode = true;
            }

            Console.WriteLine("Enter the IP to connect to, including the port:");
            string address = Console.ReadLine();
            try
            {
                string[] parts = address.Split(':');
                receiverThread = new Thread(new ParameterizedThreadStart(Receiver.Start));
                receiverThread.Start(address);
                Client.Start(parts[0], Int32.Parse(parts[1]));
            }
            catch (Exception e)
            {
                Console.Clear();
                Output.Message(ConsoleColor.DarkRed, "Could not connect: " + e.Message);
                Main(new string[1]);
            }
        }

        /// <summary>
        ///     Handles a user-close-program event (i.e. not /exit)
        /// </summary>
        /// <param name="sender">Event handler sender</param>
        /// <param name="e">Details of the event</param>
        private static void QuitClient(object sender, EventArgs e)
        {
            Client.Disconnect();
            while (!Commands.ExitHandlingFinished)
            {
                Thread.Sleep(100);
            }
        }
    }
}
