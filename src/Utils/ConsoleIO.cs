using System;

namespace TR2_Version_Swapper.Utils
{
    public static class ConsoleIO
    {
        /// <summary>
        ///     Prompts and evaluates user response to yes/no.
        /// </summary>
        /// <returns>True if yes/y, false if no/n</returns>
        public static bool UserPromptYesNo()
        {
            bool value = false;
            bool validInput = false;
            while (!validInput)
            {
                Console.Write("Yes or no? [y/n]: ");
                string inputString = Console.ReadLine();
                Console.WriteLine();
                if (!String.IsNullOrEmpty(inputString))
                {
                    inputString = inputString.ToLower();
                    if (inputString == "yes" || inputString == "y")
                    {
                        value = true;
                        validInput = true;
                    }
                    else if (inputString == "no" || inputString == "n")
                    {
                        validInput = true;
                    }
                }
            }

            return value;
        }

        /// <summary>
        ///     Prints an application header, and sets the console title
        /// </summary>
        /// <param name="title">Header title</param>
        /// <param name="subtitle">Header subtitle</param>
        /// <param name="foregroundColor">Foreground color</param>
        /// <param name="printWidth">Set the width of the console output.</param>
        /// <remarks>
        ///     Credits: https://stackoverflow.com/a/48755366/10466817
        /// </remarks>
        public static void PrintHeader(string title,
            string subtitle = "",
            ConsoleColor foregroundColor = ConsoleColor.Gray,
            int printWidth = 80)
        {
            Console.Title = title + (subtitle != "" ? $" ({subtitle})" : "");
            string titleContent = HeaderHelper(title, "║");
            string subtitleContent = HeaderHelper(subtitle, "║");
            string borderLine = new string('═', printWidth - 2);

            Console.ForegroundColor = foregroundColor;
            Console.WriteLine($"╔{borderLine}╗");
            Console.WriteLine(titleContent);
            if (!String.IsNullOrEmpty(subtitle))
                Console.WriteLine(subtitleContent);
            Console.WriteLine($"╚{borderLine}╝");
            Console.ResetColor();
        }

        /// <summary>
        ///     Returns text centered over printWidth, with edgeString on the left and right edges.
        ///     If text is too wide, it is returned with only a space between text and edgeString.
        /// </summary>
        /// <param name="text">Text to center</param>
        /// <param name="edgeString">Left and right edge text, default is none</param>
        /// <param name="printWidth">Width of the console output</param>
        /// <returns>Center-aligned text with a maximum gap between text and edgeString.</returns>
        private static string HeaderHelper(string text, string edgeString = "", int printWidth = 80)
        {
            int width = printWidth - 2 * edgeString.Length;
            if (width < text.Length)
                return $"{edgeString} {text} {edgeString}";
            return String.Format(
                $"{edgeString}{{0,{width / 2 + text.Length / 2}}}{{1,{width - width / 2 - text.Length / 2 + edgeString.Length}}}",
                text,
                edgeString
            );
        }

        /// <summary>
        ///     Print a one-line banner from the given text. If it is too wide, it is printed normally.
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="foregroundColor">Foreground color</param>
        /// <param name="printWidth">Width of the output</param>
        public static void PrintCentered(string text, ConsoleColor foregroundColor = ConsoleColor.Gray, int printWidth = 80)
        {
            PrintWithColor(text.Length > printWidth ? text : HeaderHelper(text), foregroundColor);
        }

        /// <summary>
        ///     Performs Console.Write with a given color, then resets the console color.
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="foregroundColor"></param>
        public static void PrintWithColor(string text, ConsoleColor foregroundColor = ConsoleColor.Gray)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}