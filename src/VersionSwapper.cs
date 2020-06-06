using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Utils;

namespace TR2_Version_Swapper
{
    public static class VersionSwapper
    {
        /// <summary>
        ///     The user's Version prompt options.
        /// </summary>
        private enum Version
        {
            Multipatch = 1,
            EPC = 2,
            UKB = 3
        }

        /// <summary>
        ///     Mapping of Versions to directory strings.
        /// </summary>
        private static readonly Dictionary<Version, string> SelectionDictionary = new Dictionary<Version, string>
        {
            {Version.Multipatch, "Multipatch"},
            {Version.EPC, "Eidos Premier Collection"},
            {Version.UKB, "Eidos UK Box"}
        };        
        
        /// <summary>
        ///     Ensures any TR2 process from the target directory is killed.
        /// </summary>
        private static void EnsureNoTr2RunningFromGameDir()
        {
            try
            {
                Process tr2Process = FindTr2RunningFromGameDir();
                if (tr2Process == null)
                {
                    Program.NLogger.Debug("No TR2 process of concern found; looks safe to copy files.");
                }
                else
                {
                    Program.NLogger.Info("Found TR2 process of concern.");
                    KillRunningTr2Game(tr2Process);
                    Program.NLogger.Info("Handled TR2 process of concern.");
                }
            }
            catch (Exception e)
            {
                Program.NLogger.Error($"An unexpected error occurred while trying to find running TR2 processes. {e.Message}\n{e.StackTrace}");
                ConsoleIO.PrintWithColor("I was unable to finish searching for running TR2 processes.", ConsoleColor.Yellow);
                Console.WriteLine("Please note that a TR2 game or background task running from the target folder");
                Console.WriteLine("could cause the program to crash due to errors.");
                Console.WriteLine("Double-check and make sure no TR2 game or background task is running.");
            }
        }

        /// <summary>
        ///     Finds a TR2 process from the target directory if it exists.
        /// </summary>
        /// <returns>The running Process or null.</returns>
        private static Process FindTr2RunningFromGameDir()
        {
            Program.NLogger.Debug("Checking for a TR2 process running in the target folder...");
            Process[] processes = Process.GetProcesses();
            return processes.FirstOrDefault(p =>
                p.ProcessName.ToLower() == "tomb2" &&
                p.MainModule != null &&
                Directory.GetParent(p.MainModule.FileName).FullName == Program.Directories.Game
            );
        }

        /// <summary>
        ///     Asks the user how to kill the process, then acts accordingly.
        /// </summary>
        /// <param name="p">TR2 Process of concern</param>
        private static void KillRunningTr2Game(Process p)
        {
            string processInfo = $"Name: {p.ProcessName} | ID: {p.Id} | Start time: {p.StartTime.TimeOfDay}";
            Program.NLogger.Debug($"Found a TR2 process running from target folder. {processInfo}");
            ConsoleIO.PrintWithColor("TR2 is running from the target folder.", ConsoleColor.Yellow);
            ConsoleIO.PrintWithColor(processInfo, ConsoleColor.Yellow);
            Console.WriteLine("Would you like me to end the task for you? If not, I will give a message");
            Console.Write("describing how to find and close it. ");
            if (ConsoleIO.UserPromptYesNo())
            {
                Program.NLogger.Debug("User wants the program to kill the running TR2 task.");
                try
                {
                    p.Kill();
                }
                catch (Exception e)
                {
                    Program.NLogger.Error(e, "An unexpected error occurred while trying to kill the TR2 process.");
                    ConsoleIO.PrintWithColor("I was unable to kill the TR2 process. You will have to do it yourself.", ConsoleColor.Yellow);
                    Program.NLogger.Debug("Going into the user prompt loop due to a failure in killing the process.");
                    LetUserKillTask(p);
                }
            }
            else
            {
                Program.NLogger.Debug("User opted to kill the running TR2 process on their own.");
                LetUserKillTask(p);
            }

            // During testing, it was found that the program went from process killing to file copying too fast,
            // and the files had not yet been freed for access, causing exceptions. Briefly pausing seems to fix this.
            Thread.Sleep(100);
        }

        /// <summary>
        ///     Puts the user in a prompt loop until they kill the process.
        /// </summary>
        /// <param name="p">TR2 process of concern</param>
        private static void LetUserKillTask(Process p)
        {
            bool stillRunning = !p.HasExited;
            if (!stillRunning)
            {
                Program.NLogger.Debug("Process ended before the user prompt loop started.");
                Console.WriteLine("Process ended before I could prompt you. Skipping prompt loop.");
                Console.WriteLine();
            }

            while (stillRunning)
            {
                Console.WriteLine("Be sure that all TR2 game windows are closed. Then, if you are still");
                Console.WriteLine("getting this message, check Task Manager for any phantom processes.");
                Console.WriteLine("Press a key to continue. Or press CTRL + C to exit this program.");
                Program.NLogger.Debug("Waiting for user to close the running task, running ReadKey.");
                Console.ReadKey(true);
                stillRunning = !p.HasExited;
                if (stillRunning)
                {
                    Program.NLogger.Debug("User tried to continue but the TR2 process is still running, looping.");
                    Console.WriteLine("Process still running, prompting again.");
                }
                else
                {
                    Program.NLogger.Debug("User continued the program after the TR2 process had exited.");
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        ///     Asks the user which version they want, then acts appropriately.
        /// </summary>
        public static void HandleVersions()
        {
            string selectedVersion = VersionPrompt();
            string versionDir = Path.Combine(Program.Directories.Versions, selectedVersion);

            TryCopyingDirectory(versionDir, Program.Directories.Game);
            Program.NLogger.Info($"Installed {selectedVersion} successfully.");
            ConsoleIO.PrintHeader($"{selectedVersion} successfully installed!", foregroundColor: ConsoleColor.Green);
            Console.WriteLine();
        }

        /// <summary>
        ///     Prompts, then returns the directory of the user's chosen version.
        /// </summary>
        private static string VersionPrompt()
        {
            PrintVersionList();
            int selectionNumber = 0;
            while (!(selectionNumber >= 1 && selectionNumber <= 3))
            {
                Console.Write("Enter the number of your desired version: ");
                int.TryParse(Console.ReadLine(), out selectionNumber);
            }
            
            var selectedVersion = (Version) selectionNumber;
            Program.NLogger.Debug($"User input `{selectionNumber}`, interpreting as {selectedVersion}");
            return SelectionDictionary[selectedVersion];
        }

        /// <summary>
        ///     Pretty-prints a numbered list of versions the user can choose.
        /// </summary>
        private static void PrintVersionList()
        {
            Console.WriteLine("Version List:");
            for (int i = 1; i <= SelectionDictionary.Values.Count; ++i)
            {
                string name = SelectionDictionary[(Version) i];
                Console.WriteLine($"\t{i}: {name}");
            }
        }

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
                TryCopyingDirectory(Program.Directories.Patch, Program.Directories.Game);
                Program.NLogger.Info("Installed Patch 1 successfully.");
                ConsoleIO.PrintHeader("Patch 1 successfully installed!", foregroundColor: ConsoleColor.Green);
            }
            else
            {
                Program.NLogger.Debug("User declined Patch 1 installation.");
                ConsoleIO.PrintHeader("Skipping Patch 1 installation.", foregroundColor: ConsoleColor.White);
            }
            Console.WriteLine();
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
                ConsoleIO.PrintHeader("You already have the music fix installed.", "Skipping music fix installation...", ConsoleColor.White);
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
                    TryCopyingDirectory(Program.Directories.MusicFix, Program.Directories.Game);
                    Program.NLogger.Info("Installed music fix successfully.");
                    ConsoleIO.PrintHeader("Music fix successfully installed!", foregroundColor: ConsoleColor.Green);
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

        /// <summary>
        ///     Attempts to copy files, preventing and closing program if any errors occur.
        /// </summary>
        /// <param name="srcDir">The directory to copy from</param>
        /// <param name="destDir">The directory to copy to</param>
        private static void TryCopyingDirectory(string srcDir, string destDir)
        {
            EnsureNoTr2RunningFromGameDir();
            try
            {
                Program.NLogger.Debug($"Attempting a copy from \"{srcDir}\" to \"{destDir}\"");
                FileIO.CopyDirectory(srcDir, destDir, true);
            }
            catch (Exception e)
            {
                ProgramManager.GiveErrorMessageAndExit("Failed to copy files!", e, 3);
            }
        }
    }
}