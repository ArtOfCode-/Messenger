using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessengerClient
{
    /// <summary>
    ///     Details types of log messages.
    /// </summary>
    enum LogType
    {
        Info,
        Warn,
        Error
    }

    /// <summary>
    ///     Deals with displaying server proceedings on the console.
    /// </summary>
    class Output
    {
        /// <summary>
        ///     Determines whether Debug messages are displayed or not
        /// </summary>
        public static bool DebugMode = false;

        /// <summary>
        ///     Writes a message to the console in a log format.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="type">The type of log (info/warning/error).</param>
        public static void Log(string message, LogType type)
        {
            DateTime now = DateTime.Now;
            string date = now.ToShortDateString();
            string time = now.ToShortTimeString();
            switch (type)
            {
                case LogType.Info:
                    Console.WriteLine("[" + time + " INFO] " + message);
                    break;
                case LogType.Warn:
                    Console.WriteLine("[" + time + " WARN] " + message);
                    break;
                case LogType.Error:
                    Console.WriteLine("[" + time + " ERROR] " + message);
                    break;
            }
        }

        /// <summary>
        ///     Console.WriteLine equivalent with color.
        /// </summary>
        /// <param name="color">The color to write in.</param>
        /// <param name="message">The message to write.</param>
        public static void Message(ConsoleColor color, string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }
        /// <summary>
        ///     Console.WriteLine equivalent, using the Output-default color.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void Message(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }
        /// <summary>
        ///     Console.WriteLine equivalent, using the Output-default color.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="lineBreak">Include a line break at the end?</param>
        public static void Message(string message, bool lineBreak)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            if (lineBreak)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        ///     Console.Write equivalent with color
        /// </summary>
        /// <param name="color">The color to write in</param>
        /// <param name="message">The message to write</param>
        public static void Write(ConsoleColor color, string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        ///     Clears the previous line.
        /// </summary>
        public static void ClearLine()
        {
            int currentLineCursor = Console.CursorTop - 1;
            Console.SetCursorPosition(0, currentLineCursor);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        /// <summary>
        ///     Writes a debug information message to console if DebugMode is on
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void Debug(string message)
        {
            if (DebugMode)
            {
                Console.WriteLine("<DEBUG> " + message);
            }
        }
    }
}
