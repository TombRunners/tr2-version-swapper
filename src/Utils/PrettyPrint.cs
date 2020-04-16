using System;

namespace TR2_Version_Swapper.Utils
{
    /// <summary>
    ///     Provides some pretty-printing functionality.
    /// </summary>
    /// <remarks>
    ///     Credits, particularly for the Header function: https://stackoverflow.com/a/48755366/10466817
    /// </remarks>
    public static class PrettyPrint
    {
        /// <summary>
        ///     Prints an application header, and sets the console title
        /// </summary>
        /// <param name="title">Header title</param>
        /// <param name="subtitle">Header subtitle</param>
        /// <param name="foregroundColor">Foreground color</param>
        /// <param name="printWidth">Set the width of the console output.</param>
        public static void Header(string title,
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
            if (!string.IsNullOrEmpty(subtitle))
                Console.WriteLine(subtitleContent);
            Console.WriteLine($"╚{borderLine}╝");
            Console.ResetColor();
        }

        /// <summary>
        ///     Returns text aligned to the center of printWidth, with edgeString on the left and right edges.
        ///     If it is too wide, it is returned with only a space between text and edgeString.
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
            return string.Format(
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
        public static void Center(string text, ConsoleColor foregroundColor = ConsoleColor.Gray, int printWidth = 80)
        {
            WithColor(text.Length > printWidth ? text : HeaderHelper(text), foregroundColor);
        }

        /// <summary>
        ///     Performs Console.Write with a given color, then resets the console color.
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="foregroundColor"></param>
        public static void WithColor(string text, ConsoleColor foregroundColor = ConsoleColor.Gray)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}