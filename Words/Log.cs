using System;

namespace Words.Service
{
    public static class Log
    {
        public static void Write(string message)
        {
            Write(message, ConsoleColor.DarkGreen, ConsoleColor.Green);
        }

        public static void WriteError(string message)
        {
            Write(message, ConsoleColor.DarkRed, ConsoleColor.Red);
        }

        private static void Write(string message, ConsoleColor timeColor, ConsoleColor messageColor)
        {
            ConsoleColor clr = Console.ForegroundColor;
            Console.ForegroundColor = timeColor;
            Console.Write($"{DateTime.Now}  ");
            Console.ForegroundColor = messageColor;
            Console.WriteLine(message);
            Console.ForegroundColor = clr;
        }
    }
}
