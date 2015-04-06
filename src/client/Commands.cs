using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace MessengerClient
{
    /// <summary>
    ///     Deals with control commands and sending server commands
    /// </summary>
    class Commands
    {
        /// <summary>
        ///     For /exit command. Tells the Receiver thread to shutdown.
        /// </summary>
        public static volatile bool EndRcvThread = false;

        /// <summary>
        ///     For /exit command. Is true when the Receiver thread has shut down.
        /// </summary>
        public static volatile bool RcvThreadEnded = false;

        /// <summary>
        ///     For /exit command. Tells the Program loop that exit handling has finished and the program can be shut down.
        /// </summary>
        public static bool ExitHandlingFinished = false;

        /// <summary>
        ///     Checks if a string could be a command.
        /// </summary>
        /// <param name="command">The string to check</param>
        /// <returns>True if the string could be a command; false otherwise</returns>
        public static bool IsCommand(string command)
        {
            if (command.StartsWith("/"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///     Takes a command and executes the appropriate response.
        /// </summary>
        /// <param name="client">The client connection to the server</param>
        /// <param name="command">The command run</param>
        public static void HandleCommand(TcpClient client, string command)
        {
            string[] args = command.Split(' ');
            switch (args[0].ToLower())
            {
                case "/server":
                    if (args.Length >= 2)
                    {
                        int startIndex = args[0].Length;
                        string commandArgs = command.Substring(startIndex + 1);
                        Client.SendMessage("[Command]" + commandArgs);
                    }
                    else
                    {
                        Output.Message(ConsoleColor.DarkRed, "Not enough arguments");
                        return;
                    }
                    break;
                case "/exit":
                    Client.Disconnect();
                    break;
                default:
                    Output.Message(ConsoleColor.DarkRed, "Unknown command.");
                    return;
            }
        }

        /// <summary>
        ///     Handles the server's response to a sent server command.
        /// </summary>
        /// <param name="response">The response from the server.</param>
        public static void HandleResponse(string response)
        {
            // Command was sent; server did not recognise
            if (response == "[CommandInvalid]")
            {
                Output.Message(ConsoleColor.DarkRed, "The command was not recognised by the server.");
                return;
            }

            // Disconnect was sent; server acknowledges
            if (response == "[DisconnectAcknowledge]")
            {
                EndRcvThread = true;
                Output.Debug("Waiting for thread termination");
                while (!RcvThreadEnded)
                {
                    Thread.Sleep(100);
                }
                Output.Debug("Thread terminated, cleaning send client");
                Client.SendMessage("");
                Client.client.Close();
                Output.Debug("Cleaned up send client");
                if (Output.DebugMode)
                {
                    Console.WriteLine();
                    Output.Debug("Press any key to exit");
                    Console.ReadKey();
                }
                Environment.Exit(0);
            }

            // Fallback for neither case: pass it off to the client
            ResponseCode code = ResponseCodes.GetResponse(response);
            Client.HandleResponse(code);
        }
    }
}
