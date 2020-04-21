using System;

using TR2_Version_Swapper.Utils;

namespace TR2_Version_Swapper
{
    public static class VersionSwapper
    {
        /// <summary>
        ///     Checks if the music fix is already installed then takes the appropriate action.
        ///     If the fix is detected, simply lets the user know it is installed. Otherwise,
        ///     asks if the user would like the music fix installed and acts accordingly.
        /// </summary>
        public static void HandleMusicFix()
        {
            if (IsMusicFixInstalled())
            {
                Program.NLogger.Debug("Music fix is already installed. Reminding the user they have it installed.");
                ConsoleIO.PrintHeader("You already have the music fix installed.", "Skipping music fix installation...");
                Console.WriteLine();
            }
            else
            {
                Program.NLogger.Debug("Music fix is not installed. Asking the user if they want it to be installed.");
                Console.WriteLine("You switched to a non-Multipatch version. In-game music might not work and/or");
                Console.WriteLine("the game might freeze or lag when it tries to load music. I can install a music");
                Console.WriteLine("fix which should resolve most music-related issues.");
                Console.WriteLine("Please note that you are not required to install this optional fix. Any time you");
                Console.WriteLine("run this program and select a version, I will check for the fix and ask again");
                Console.WriteLine("if you want to install it. The fix applies to all versions the same, so it only");
                Console.Write("needs to be installed once. "); // Omit '\n' and leave space for clean same-line prompt.
                bool installFix = ConsoleIO.UserPromptYesNo();
                if (installFix)
                {
                    Program.NLogger.Debug("User wants the music fix installed.");
                    FileIO.CopyDirectory(Program.Directories.MusicFix, Program.Directories.Game, true);
                    Program.NLogger.Info("Installed music fix.");
                    ConsoleIO.PrintHeader("Music fix successfully installed!", foregroundColor: ConsoleColor.DarkGreen);
                }
                else
                {
                    Program.NLogger.Debug("User declined the music fix installation.");
                    ConsoleIO.PrintHeader("Skipping music fix.", "I'll ask again next time.", ConsoleColor.White);
                }
            }
        }

        /// <summary>
        ///     Checks the game directory to see if the music fix files are already present.
        /// </summary>
        /// <returns>True if music fix is installed, false otherwise</returns>
        private static bool IsMusicFixInstalled()
        {
            try
            {
                FileIO.FindMissingFile(FileAudit.MusicFilesAudit.Keys, Program.Directories.Game);
            }
            catch (RequiredFileMissingException)
            {
                return false;
            }

            return true;
        }
    }
}