using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MessengerClient
{
    /// <summary>
    ///     The main sending client and UI thread
    /// </summary>
    class Client
    {
        /// <summary>
        ///     Used for converting between strings and byte arrays
        /// </summary>
        private static ASCIIEncoding encoder = new ASCIIEncoding();

        /// <summary>
        ///     The server-assigned client ID
        /// </summary>
        private static int clientId = 0;

        /// <summary>
        ///     Gets the ID for this client
        /// </summary>
        /// <returns>A 32-bit integer representing the client ID.</returns>
        public static int GetClientId()
        {
            return clientId;
        }

        /// <summary>
        ///     The TcpClient to connect to the server
        /// </summary>
        public static TcpClient client = new TcpClient();

        /// <summary>
        ///     The server address
        /// </summary>
        private static IPEndPoint serverEndPoint;

        /// <summary>
        ///     Runs the client
        /// </summary>
        /// <param name="ip">The server IP address</param>
        /// <param name="port">The server port</param>
        public static void Start(string ip, int port)
        {
            serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            try
            {
                client.Connect(serverEndPoint);
            }
            catch (Exception e)
            {
                throw new Exception("No connection was made: " + e.Message);
            }

            while (true)
            {
                Output.Write(ConsoleColor.DarkBlue, "Me: ");
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                string message = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Black;

                if (Commands.IsCommand(message))
                {
                    Commands.HandleCommand(client, message);
                    continue;
                }

                SendMessage(message);
            }
        }

        /// <summary>
        ///     Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public static void SendMessage(string message)
        {
            NetworkStream clientStream = client.GetStream();
            byte[] buffer;
            if (message.StartsWith("[Disconnect]") || message.StartsWith("[Command]"))
            {
                buffer = encoder.GetBytes(message);
            }
            else
            {
                buffer = encoder.GetBytes("[Send]" + message);
            }

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }

        /// <summary>
        ///     Displays output based on a server response code.
        /// </summary>
        /// <param name="code">The ResponseCode returned by the server</param>
        public static void HandleResponse(ResponseCode code)
        {
            switch (code)
            {
                case ResponseCode.Success:
                    return;
                case ResponseCode.ServerError:
                    Output.Message(ConsoleColor.DarkRed, "The server could not process your message. (100)");
                    break;
                case ResponseCode.NoDateFound:
                    Output.Message(ConsoleColor.DarkRed, "Could not retrieve messages from the server. (200)");
                    break;
                case ResponseCode.BadDateFormat:
                    Output.Message(ConsoleColor.DarkRed, "Could not retrieve messages from the server. (201)");
                    break;
                case ResponseCode.NoMessageFound:
                    Output.Message(ConsoleColor.DarkRed, "The server could not process your message. (300)");
                    break;
                case ResponseCode.NoHandlingProtocol:
                    Output.Message(ConsoleColor.DarkRed, "The server could not process your message. (400)");
                    break;
                case ResponseCode.NoCode:
                    Output.Message(ConsoleColor.DarkRed, "Could not process the server's response. (NoCode)");
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        ///     Tries to parse an ID code from a string.
        /// </summary>
        /// <param name="id"></param>
        public static void ParseClientId(string id)
        {
            clientId = Int32.Parse(id);
        }

        /// <summary>
        ///     Disconnects the client from the server and signals other threads to do the same.
        /// </summary>
        public static void Disconnect()
        {
            SendMessage("[Disconnect]");
            Commands.EndRcvThread = true;
            Output.Debug("Requested receive thread termination.");
            Output.Message(ConsoleColor.DarkGreen, "Shutting down...");
        }
    }
}
