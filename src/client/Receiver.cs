using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MessengerClient
{
    /// <summary>
    ///     Main class for receiving messages from the server
    /// </summary>
    class Receiver
    {
        /// <summary>
        ///     The TcpClient to connect to the server
        /// </summary>
        private static TcpClient client = new TcpClient();

        /// <summary>
        ///     The server address
        /// </summary>
        private static IPEndPoint serverEndPoint;

        /// <summary>
        ///     Starts the reception using the specified address
        /// </summary>
        /// <param name="address">An IPEndPoint representing the server address</param>
        public static void Start(object address)
        {
            string[] parts = ((string) address).Split(':');

            try
            {
                serverEndPoint = new IPEndPoint(IPAddress.Parse(parts[0]), Int32.Parse(parts[1]));
            }
            catch (Exception e)
            {
                Output.Message(ConsoleColor.DarkRed, "Could not connect: " + e.Message);
                return;
            }

            try
            {
                client.Connect(serverEndPoint);
                client.ReceiveTimeout = 500;
            }
            catch (Exception e)
            {
                Output.Message(ConsoleColor.DarkRed, "Could not connect: " + e.Message);
                return;
            }

            NetworkStream stream = client.GetStream();
            string data = "";
            byte[] received = new byte[4096];

            while (true)
            {
                if (Commands.EndRcvThread)
                {
                    Output.Debug("Ending receiver thread");
                    client.Close();
                    Output.Debug("Cleaned up receive client");
                    Commands.RcvThreadEnded = true;
                    Commands.HandleResponse("[DisconnectAcknowledge]");
                    Output.Debug("Notified Commands handler of thread abortion");
                    Thread.CurrentThread.Abort();
                    return;
                }

                data = "";
                received = new byte[4096];

                int bytesRead = 0;
                try
                {
                    bytesRead = stream.Read(received, 0, 4096);
                }
                catch (Exception e)
                {
                    continue;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                int endIndex = received.Length - 1;
                while (endIndex >= 0 && received[endIndex] == 0)
                {
                    endIndex--;
                }

                byte[] finalMessage = new byte[endIndex + 1];
                Array.Copy(received, 0, finalMessage, 0, endIndex + 1);

                data = Encoding.ASCII.GetString(finalMessage);
                Output.Debug("Server message: " + data);

                try
                {
                    ProcessMessage(data);
                }
                catch (Exception e)
                {
                    Output.Message(ConsoleColor.DarkRed, "Could not process the server's response (" + data + "): " + e.Message);
                }
            }
        }

        /// <summary>
        ///     Attempts to execute the appropriate response to a server message
        /// </summary>
        /// <param name="response">The message from the server</param>
        public static void ProcessMessage(string response)
        {
            Output.Debug("Processing message: " + response);
            response = response.Trim();

            if (response.StartsWith("[Message]"))
            {
                Output.Debug("Starts with [Message], trying to find ID");
                response = response.Substring(9);

                int openIndex = response.IndexOf("<");
                int closeIndex = response.IndexOf(">");

                if (openIndex < 0 || closeIndex < 0 || closeIndex < openIndex)
                {
                    Output.Debug("No ID tag? ( <ID-#-HERE> )");
                    throw new FormatException("Could not find ID tag in message");
                }

                int diff = closeIndex - openIndex;
                int id = Int32.Parse(response.Substring(openIndex + 1, diff - 1));
                if (id != Client.GetClientId())
                {
                    string message = response.Substring(closeIndex + 1);
                    Console.WriteLine();
                    Output.Message(ConsoleColor.DarkYellow, "<Stranger> " + message);
                    Output.Write(ConsoleColor.DarkBlue, "Me: ");
                }
                else
                {
                    Output.Debug("ID is client ID, not displaying.");
                }
            }
            else if (response == "[DisconnectAcknowledge]" || response == "[CommandInvalid]")
            {
                Output.Debug("Sending response to Commands handler: " + response);
                Commands.HandleResponse(response);
            }
            else if (response.Length == 5 && response.StartsWith("[") && response.EndsWith("]"))
            {
                Client.HandleResponse(ResponseCodes.GetResponse(response));
            }
            else
            {
                Output.Debug("Figuring out what to do with server message: " + response);
                try
                {
                    Int32.Parse(response);
                    Output.Debug("Int32.Parse has not failed, assume client ID sent.");
                    Client.ParseClientId(response);
                    return;
                }
                catch (Exception e) {
                    Output.Debug("Could not process client ID: " + e.Message);
                }
                Output.Debug("Could not identify what to do with message.");
                Output.Message(ConsoleColor.DarkCyan, "<Server> " + response);
            }
        }
    }
}
