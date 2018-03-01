using System;

namespace Utilities
{
    public class ConsoleWriter : IWriter
    {
        public void WriteLine(string v)
        {
            Console.WriteLine(v);
        }

        public void WriteLine(string v, State color)
        {
            var foreground = Console.ForegroundColor;
            var background = Console.BackgroundColor;
            switch (color)
            {
                case State.Red:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Red;
                    break;
                case State.Yellow:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    break;
                case State.Green:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Green;
                    break;
            }
            Console.WriteLine(v);
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
        }
    }
}