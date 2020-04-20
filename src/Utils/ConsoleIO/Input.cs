using System;

namespace TR2_Version_Swapper.Utils.ConsoleIO
{
    /// <summary>
    ///     Provides some reusable console input reading functionality.
    /// </summary>
    public static class Input
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
    }
}