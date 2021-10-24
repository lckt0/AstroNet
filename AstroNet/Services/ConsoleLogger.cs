using System;

namespace Astro_Renewed.Services
{
    class ConsoleLogger
    {

        private static void PrintTime()
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            string time = DateTime.Now.ToString("HH:mm:ss");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(time);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("]");
            Console.ForegroundColor = oldColor;
        }

        public enum PrintMode
        {
            Info,
            Protection,
            Success,
            Warning,
            Error
        }

        private static void PrintType(PrintMode mode)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[");
            switch (mode)
            {
                case PrintMode.Info:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write("i");
                        break;
                    }
                case PrintMode.Protection:
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write("*");
                        break;
                    }
                case PrintMode.Success:
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("+");
                        break;
                    }
                case PrintMode.Warning:
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("!");
                        break;
                    }
                case PrintMode.Error:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("x");
                        break;
                    }
                default: break;
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] ");
            Console.ForegroundColor = oldColor;
        }

        public static void Log(string msg, PrintMode mode = PrintMode.Protection)
        {
            PrintTime();
            PrintType(mode);
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;
            if (mode == PrintMode.Protection)
            {
                ConsoleColor oldColor2 = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                string[] splitt = msg.Split('"');
                Console.Write(splitt[0]);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("\"" + splitt[1] + "\"");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write(splitt[2] + "\n");
                Console.ForegroundColor = oldColor2;
                return;
            }
            Console.Write(msg + "\n");
            Console.ForegroundColor = oldColor;
        }
    }
}
