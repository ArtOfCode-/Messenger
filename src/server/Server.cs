using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace MessengerServer
{
    /// <summary>
    ///     The server controller: deals with client connections and task delegation.
    /// </summary>
    class Server
    {
        /// <summary>
        ///     The TCP listener used to accept clients.
        /// </summary>
        private TcpListener tcpListener;

        /// <summary>
        ///     The thread to accept clients. Spawns client communication threads.
        /// </summary>
        private Thread listenThread;

        /// <summary>
        ///     Used for converting between strings and byte arrays.
        /// </summary>
        private ASCIIEncoding encoder = new ASCIIEncoding();

        /// <summary>
        ///     DEPRECATED. Messages that have been received.
        /// </summary>
        [Obsolete]
        public Dictionary<DateTime, string> Messages = new Dictionary<DateTime, string>();

        /// <summary>
        ///     DEPRECATED. Gets messages after a certain time.
        /// </summary>
        /// <param name="time">The time to retrieve messages after. A DateTime in string format.</param>
        /// <returns>A List of strings representing the messages received after <i>time</i>.</returns>
        [Obsolete]
        private List<string> MessagesAfter(string time)
        {
            List<string> matches = new List<string>();
            DateTime sinceTime = DateTime.Parse(time);
            foreach (KeyValuePair<DateTime, string> pair in Messages)
            {
                if (pair.Key.CompareTo(sinceTime) > 0)
                {
                    matches.Add(pair.Value);
                }
            }
            return matches;
        }

        /// <summary>
        ///     A list of all clients with an active connection to the server.
        /// </summary>
        private List<TcpClient> connectedClients = new List<TcpClient>();

        /// <summary>
        ///     Starts a new Server instance and runs the listener thread.
        /// </summary>
        /// <param name="port">The port to run the server on.</param>
        public Server(int port)
        {
            Output.Log("Starting server: all network interfaces, port " + port, LogType.Info);
            this.tcpListener = new TcpListener(IPAddress.Any, port);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        /// <summary>
        ///     The method for the client listener thread.
        /// </summary>
        private void ListenForClients()
        {
            Output.Log("Listener thread spawned", LogType.Info);

            this.tcpListener.Start();
            Output.Log("TCP listener started, ready to accept clients.", LogType.Info);

            while (true)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();
                Output.Log("Client connected, starting thread...", LogType.Info);

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComms));
                clientThread.Start(client);
                connectedClients.Add((TcpClient) client);
            }
        }

        /// <summary>
        ///     The method for the client communication handler thread.
        /// </summary>
        /// <param name="client">An (object)TcpClient representing the connected client.</param>
        private void HandleClientComms(object client)
        {
            int clientId = new Random().Next(0, int.MaxValue - 1);
            TcpClient tcpClient = (TcpClient) client;
            NetworkStream clientStream = tcpClient.GetStream();
            Output.Log("Communication thread started with client at " + tcpClient.Client.RemoteEndPoint.ToString(), LogType.Info);
            Output.Log("Client identifier is " + clientId, LogType.Info);

            SendToClient(tcpClient, clientId.ToString());

            byte[] message = new byte[4096];
            int bytesRead;

            string data = "";

            while (true)
            {
                bytesRead = 0;
                data = "";

                try
                {
                    bytesRead = clientStream.Read(message, 0, 4096);
                    Output.Log("Read " + bytesRead + " bytes from " + clientId, LogType.Info);
                }
                catch(Exception e)
                {
                    Output.Log("Could not read from client: " + e.Message, LogType.Error);
                    if (e.GetType() == Type.GetType("System.IO.IOException"))
                    {
                        connectedClients.Remove(tcpClient);
                        Thread.CurrentThread.Abort();
                    }
                    break;
                }

                if (bytesRead == 0)
                {
                    Output.Log("Client " + clientId + " disconnected", LogType.Info);
                    connectedClients.Remove(tcpClient);
                    Thread.CurrentThread.Abort();
                    break;
                }

                string received = encoder.GetString(message, 0, bytesRead);
                data += received;
                Output.Log("Message: " + received, LogType.Info);

                try
                {
                    HandleMessage(tcpClient, clientId, data);
                }
                catch (ThreadAbortException tae)
                {
                    Output.Log("Client thread disconnect complete: " + tae.Message, LogType.Info);
                }
                catch (Exception e)
                {
                    Output.Log("Could not handle message: " + e.Message, LogType.Warn);
                }
            }
        }

        /// <summary>
        ///     Sends a message to the specified client.
        /// </summary>
        /// <param name="client">The client to send the message to.</param>
        /// <param name="message">The message to send.</param>
        public void SendToClient(TcpClient client, string message)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] msg = new byte[message.Length];
                msg = encoder.GetBytes(message);
                stream.Write(msg, 0, message.Length);
                stream.Flush();
                Output.Log("Sent to client: " + message, LogType.Info);
            }
            catch (Exception e)
            {
                Output.Log("Unexpected exception sending: " + e.Message, LogType.Error);
            }
        }

        /// <summary>
        ///     Handles a message received from the client.
        /// </summary>
        /// <param name="client">The client who sent the message.</param>
        /// <param name="clientId">The integer identifier for the client.</param>
        /// <param name="message">The message to handle.</param>
        private void HandleMessage(TcpClient client, int clientId, string message)
        {
            Output.Log("Attempting to handle message " + message, LogType.Info);
            if (message.StartsWith("[AllSince]"))
            {
                try
                {
                    string date = message.Split(']')[1];
                    List<string> messages = MessagesAfter(date);
                    string data = "";
                    foreach (string msg in messages)
                    {
                        data += msg + "|&|";
                    }
                    SendToClient(client, data);
                }
                catch (IndexOutOfRangeException e)
                {
                    SendToClient(client, "[200]");
                    throw new Exception("No date was found for [AllSince]: " + e.Message);
                }
                catch (FormatException e)
                {
                    SendToClient(client, "[201]");
                    throw new Exception("Date was not formatted correctly: " + e.Message);
                }
                catch (Exception e)
                {
                    SendToClient(client, "[100]");
                    throw new Exception("Unexpected exception: " + e.Message);
                }
            }
            else if (message.StartsWith("[Send]"))
            {
                try
                {
                    string text = message.Split(']')[1];
                    Messages.Add(DateTime.Now, "<" + clientId + ">" + text);
                    Output.Log("Added to message list: " + text, LogType.Info);
                    NotifyAllClients("<" + clientId + ">" + text);
                    SendToClient(client, "[600]");
                }
                catch (Exception e)
                {
                    SendToClient(client, "[300]");
                    throw new Exception("No message could be found for [Send]: " + e.Message);
                }
            }
            else if (message.StartsWith("[Disconnect]"))
            {
                Output.Log("Client " + clientId + "'s client thread disconnected", LogType.Warn);
                connectedClients.Remove(client);
                Thread.CurrentThread.Abort();
            }
            else if (message.StartsWith("[Command]"))
            {
                string command = message.Substring(9);
                Commands.HandleCommand(client, command);
            }
            else
            {
                SendToClient(client, "[400]");
                throw new Exception("No handling protocol specified.");
            }
        }

        /// <summary>
        ///     Notifies all clients of a new chat message. Wraps SendToClient.
        /// </summary>
        /// <param name="message">The chat message to notify clients of.</param>
        public void NotifyAllClients(string message)
        {
            Output.Log("Notifying all clients of message " + message, LogType.Info);
            foreach (TcpClient client in connectedClients)
            {
                Output.Log("Notify: client " + client.Client.RemoteEndPoint.ToString(), LogType.Info);
                SendToClient(client, "[Message]" + message);
            }
        }
    }
}
