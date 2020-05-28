using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Octokit;
using Utils;

namespace TR2_Version_Swapper
{
    internal static class InstallationManager
    {
        /// <summary>
        ///     Notifies the user if their program is outdated.
        /// </summary>
        public static void VersionCheck()
        {
            Program.NLogger.Debug("Running Github Version checks...");
            Version current = typeof(Program).Assembly.GetName().Version;
            try
            {
                Version latest = Github.GetLatestVersion().GetAwaiter().GetResult();
                int result = current.CompareTo(latest, 3);

                if (result == -1)
                {
                    Program.NLogger.Debug($"Latest Github release ({latest}) is newer than the running version ({result}).");
                    ConsoleIO.PrintHeader("A new release is available!", Misc.ReleaseLink, ConsoleColor.Yellow);
                    Console.WriteLine("You are strongly advised to update to ensure leaderboard compatibility:");
                }
                else if (result == 0)
                {
                    Program.NLogger.Debug($"Version is up-to-date ({latest}).");
                }
                else // result == 1
                {
                    Program.NLogger.Debug($"Running version ({current}) has not yet been released on Github ({latest}).");
                    Console.WriteLine("You seem to be running a pre-release version.");
                    Console.WriteLine("Let me know how testing goes! :D");
                }
            }
            catch (ApiException e)
            {
                Program.NLogger.Error(e, "Github request failed.");
                ConsoleIO.PrintWithColor("Unable to check for the latest version. Consider manually checking:", ConsoleColor.Yellow);
                Console.WriteLine(Misc.ReleaseLink);
            }
            finally
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        ///     Validates packaged files, ensures target directory looks like TR2.
        /// </summary>
        public static void ValidateInstallation()
        {
            try
            {
                ValidatePackagedFiles();
                Program.NLogger.Info("Successfully validated packaged files using MD5 hashes.");
                CheckGameDirLooksLikeATr2Install();
                Program.NLogger.Info("Parent directory seems like a TR2 game installation.");
            }
            catch (Exception e)
            {
                if (e is BadInstallationLocationException ||
                    e is RequiredFileMissingException ||
                    e is InvalidGameFileException)
                {
                    Program.NLogger.Fatal($"Installation failed to validate. {e.Message}\n{e.StackTrace}");
                    ConsoleIO.PrintWithColor(e.Message, ConsoleColor.Red);
                    Console.WriteLine("You are advised to re-install the latest release to fix the issue:");
                    Console.WriteLine(Misc.ReleaseLink);
                    ProgramManager.EarlyPauseAndExit(2);
                }
                
                const string statement = "An unhandled exception occurred while validating your installation.";
                ProgramManager.GiveErrorMessageAndExit(statement, e, 1);
            }
        }

        /// <summary>
        ///     Ensures files packaged in releases are untampered.
        /// </summary>
        private static void ValidatePackagedFiles()
        {
            // Check that each version's game files are present and unmodified.
            ValidateMd5Hashes(FileAudit.VersionFilesAudit, Program.Directories.Versions);
            // Check that the utility files are present and unmodified.
            ValidateMd5Hashes(FileAudit.MusicFilesAudit, Program.Directories.MusicFix);
            ValidateMd5Hashes(FileAudit.PatchFilesAudit, Program.Directories.Patch);
        }

        /// <summary>
        ///     Ensures target directory contains affected game files and folders.
        /// </summary>
        /// <exception cref="BadInstallationLocationException">Targeted directory is missing a file or folder</exception>
        private static void CheckGameDirLooksLikeATr2Install()
        {
            string missingFile = FileIO.FindMissingFile(FileAudit.GameFiles, Program.Directories.Game);
            if (!string.IsNullOrEmpty(missingFile))
                throw new BadInstallationLocationException($"Parent folder is missing game file {missingFile}, cannot be a TR2 installation.");
            if (!Directory.Exists(Path.Combine(Program.Directories.Game, "music")))
                throw new BadInstallationLocationException("Parent folder does not contain a music folder, cannot be a TR2 installation.");
        }

        /// <summary>
        ///     Checks that files in dir match their required MD5 hashes.
        /// </summary>
        /// <param name="fileAudit">Mapping of file names to readable, lowercased MD5 hashes</param>
        /// <param name="dir">Directory to operate within</param>
        /// <exception cref="RequiredFileMissingException">A file was not found</exception>
        /// <exception cref="InvalidGameFileException">A file's MD5 hash did not match expected value</exception>
        private static void ValidateMd5Hashes(Dictionary<string, string> fileAudit, string dir)
        {
            foreach ((string file, string requiredHash) in fileAudit)
            {
                try
                {
                    string hash = FileIO.ComputeMd5Hash(Path.Combine(dir, file));
                    if (hash != requiredHash)
                        throw new InvalidGameFileException($"File {file} was modified.\nGot {hash}, expected {requiredHash}");
                }
                catch (FileNotFoundException e)
                {
                    throw new RequiredFileMissingException(e.Message);
                }
            }
        }

        /// <summary>
        ///     Ensures any TR2 process from the target directory is killed.
        /// </summary>
        public static void EnsureNoTr2RunningFromGameDir()
        {
            try
            {
                Process tr2Process = FindTr2RunningFromGameDir();
                if (tr2Process == null)
                {
                    Program.NLogger.Info("No TR2 processes running from the target folder.");
                }
                else
                {
                    Program.NLogger.Debug("Found running TR2 process of concern.");
                    KillRunningTr2Game(tr2Process);
                    Program.NLogger.Info("Handled running TR2 process of concern.");
                }
            }
            catch (Exception e)
            {
                Program.NLogger.Error(e, "An unexpected error occurred while trying to find running TR2 processes.");
                ConsoleIO.PrintWithColor("I was unable to finish searching for running TR2 processes.", ConsoleColor.Yellow);
                Console.WriteLine("Please note that a TR2 game or background task running from the target folder");
                Console.WriteLine("could cause issues with the program, such as preventing overwrites.");
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
        ///     Asks the user if they want the program to kill the running TR2 process. If user declines,
        ///     a loop will begin: first it gives a message and waits for user input to continues,
        ///     then exits if the process has ended.
        /// </summary>
        /// <param name="p">TR2 Process of concern</param>
        private static void KillRunningTr2Game(Process p)
        {
            string processInfo = $"Name: {p.ProcessName} | ID: {p.Id} | Start time: { p.StartTime.TimeOfDay}";
            Program.NLogger.Debug($"Found a TR2 process running from target folder. {processInfo}");
            ConsoleIO.PrintWithColor("TR2 is running from the target folder.", ConsoleColor.Yellow);
            ConsoleIO.PrintWithColor(processInfo, ConsoleColor.Yellow);
            Console.WriteLine("Would you like me to end the task for you? If not, I will give a message");
            Console.Write("describing how to find and close it. Type \"y\" to have me kill the task: ");
            if (ConsoleIO.UserPromptYesNo())
            {
                Program.NLogger.Debug("User is allowing the program to kill the running TR2 task.");
                try
                {
                    p.Kill();
                }
                catch (Exception e)
                {
                    Program.NLogger.Error(e, "An unexpected error occurred while trying to kill the TR2 process.");
                    ConsoleIO.PrintWithColor("I was unable to kill the TR2 process. You will have to do it yourself.", ConsoleColor.Yellow);
                }
            }
            else
            {
                Program.NLogger.Debug("User is opting to kill the running TR2 task on their own.");
                if (p.HasExited)
                {
                    Program.NLogger.Debug("Process ended before the user prompt loop started.");
                    Console.WriteLine("Process ended by external actor. Skipping message prompt and wait loop.");
                    Console.WriteLine();
                }
                bool stillRunning = true;
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
        }
    }
}
