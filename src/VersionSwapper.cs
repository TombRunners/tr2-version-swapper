using System;

using Utils;

namespace TR2_Version_Swapper
{
    public static class VersionSwapper
    {
        /// <summary>
        ///     Asks the user if they want Patch 1, then acts appropriately.
        /// </summary>
        public static void HandlePatch()
        {
            Console.WriteLine("Would you like me to install CORE's Patch 1 on top of your selected version?");
            Console.WriteLine("Please note that you are not required to install this optional patch.");
            bool installPatch = ConsoleIO.UserPromptYesNo("Install CORE's Patch 1 onto your selected version? [y/n]: ");
            if (installPatch)
            {
                Program.NLogger.Debug("User wants Patch 1 installed...");
                FileIO.CopyDirectory(Program.Directories.Patch, Program.Directories.Game, true);
                Program.NLogger.Info("Installed Patch 1 successfully.");
                ConsoleIO.PrintHeader("Patch 1 successfully installed!", foregroundColor: ConsoleColor.DarkGreen);
            }
            else
            {
                Program.NLogger.Debug("User declined Patch 1 installation.");
                ConsoleIO.PrintHeader("Skipping Patch 1 installation.", foregroundColor: ConsoleColor.White);
            }
        }

        /// <summary>
        ///     Checks if the music fix is already installed, then takes the appropriate action.
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
                Console.Write("needs to be installed once. "); // Omit '\n' and leave space for a clean same-line prompt.
                bool installFix = ConsoleIO.UserPromptYesNo("Install the music fix? [y/n]: ");
                if (installFix)
                {
                    Program.NLogger.Debug("User wants the music fix installed...");
                    FileIO.CopyDirectory(Program.Directories.MusicFix, Program.Directories.Game, true);
                    Program.NLogger.Info("Installed music fix successfully.");
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
        ///     Checks if the music fix is installed in the game directory.
        /// </summary>
        /// <returns>True if music fix is installed, false otherwise</returns>
        private static bool IsMusicFixInstalled()
        {
            string firstMissingFile = FileIO.FindMissingFile(FileAudit.MusicFilesAudit.Keys, Program.Directories.Game);
            return string.IsNullOrEmpty(firstMissingFile);
        }
    }
}