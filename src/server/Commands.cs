using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MessengerServer
{
    /// <summary>
    ///     Deals with server commands and communications about them.
    /// </summary>
    class Commands
    {
        /// <summary>
        ///     Handles a command.
        /// </summary>
        /// <param name="client">The client who sent the command.</param>
        /// <param name="command">The command to handle.</param>
        public static void HandleCommand(TcpClient client, string command)
        {
            string[] args = command.Split(' ');
            switch (args[0].ToLower().Trim())
            {
                case "force":
                    if (args.Length == 2)
                    {
                        SendToClient(client, "[" + args[1] + "]");
                    }
                    else
                    {
                        SendToClient(client, "[CommandInvalid]");
                    }
                    break;
                default:
                    SendToClient(client, "[CommandInvalid]");
                    break;
            }
        }

        /// <summary>
        ///     Sends a message to a client.
        /// </summary>
        /// <param name="client">The client to send to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendToClient(TcpClient client, string message)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] msg = new byte[message.Length];
                msg = Encoding.ASCII.GetBytes(message);
                stream.Write(msg, 0, message.Length);
                stream.Flush();
                Output.Log("Sent to client: " + message, LogType.Info);
            }
            catch (Exception e)
            {
                Output.Log("Unexpected exception sending: " + e.Message, LogType.Error);
            }
        }
    }
}
