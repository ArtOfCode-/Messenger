using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessengerServer
{
    /// <summary>
    ///     Controls server execution, including start and stop.
    /// </summary>
    class Program
    {
        /// <summary>
        ///     The default port number for the server to run on.
        /// </summary>
        private static int DefaultPort = 1100;

        /// <summary>
        ///     The command-line argument to specify the port number.
        /// </summary>
        private static string PortArgument = "--port";

        /// <summary>
        ///     The main entry point for the program.
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        static void Main(string[] args)
        {
            int userPort = 0;

            if (args.Length == 2 && args[0] == PortArgument)
            {
                if (!Int32.TryParse(args[1], out userPort))
                {
                    Output.Log("Switch --port: Invalid port number specified (" + args[1] + ").", LogType.Error);
                    Output.Log("Switch --port: Defaulting to port " + DefaultPort, LogType.Warn);
                }
            }
            else
            {
                if (args.Length != 0)
                {
                    Output.Log("Could not parse arguments: wrong number of arguments or invalid argument.", LogType.Error);
                }
            }

            Output.Log("Starting server on port " + (userPort > 0 ? userPort : DefaultPort), LogType.Info);

            new Server(userPort > 0 ? userPort : DefaultPort);
        }
    }
}
