using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using NLog;
using Octokit;
using TR2_Version_Swapper.Utils;

namespace TR2_Version_Swapper
{
    internal struct InstallDirectories
    {
        public string Game;
        public string Versions;
        public string MusicFix;
        public string Patch;
    }

    public class VersionSwapper
    {
        private static InstallDirectories _dirs;

        private readonly Logger NLogger;

        public VersionSwapper(Logger l)
        {
            string root = Path.GetFullPath(Directory.GetCurrentDirectory());
            _dirs = new InstallDirectories
            {
                Game = Directory.GetParent(root).FullName,
                Versions = Path.Combine(root, "versions"),
                MusicFix = Path.Combine(root, "utilities/music_fix"),
                Patch = Path.Combine(root, "utilities/patch"),
            };
            NLogger = l;
        }

        private static bool UserPromptYesNo()
        {
            bool value = false;
            bool validInput = false;
            while (!validInput)
            {
                Console.Write("Yes or no? [y/n]: ");
                string inputString = Console.ReadLine();
                Console.WriteLine();
                if (!string.IsNullOrEmpty(inputString))
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
        ///     Deletes the oldest log file to prevent unnecessary bloat.
        /// </summary>
        /// <returns>True if a file was deleted.</returns>
        public void DeleteExcessLogFiles(int logFileLimit)
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            var files = new List<string>(Directory.GetFiles(dir));
            files.Sort();
            
            var fileCount = files.Count;
            if (files.Count > logFileLimit)
            {
                NLogger.Debug($"Excessive log file count: {fileCount} vs {logFileLimit}");
                PrettyPrint.WithColor($"Log file limit of {fileCount} exceeded (total: {logFileLimit})", ConsoleColor.Yellow);
                Console.WriteLine("Files will be deleted accordingly.");
                Console.WriteLine();
            }
            else if (files.Count + 3 > logFileLimit)
            {
                NLogger.Debug($"Log file count approaching excessive: {fileCount} vs {logFileLimit}");
                PrettyPrint.WithColor($"You are approaching your set log file limit ({fileCount} of {logFileLimit})", ConsoleColor.Yellow);
                Console.WriteLine("Be sure to edit appsettings.json to adjust the limit to your tastes.");
                Console.WriteLine();
            }

            while (files.Count > logFileLimit)
            {
                try
                {
                    File.Delete(files[0]);
                    NLogger.Info($"Deleted excess log file {files[0]}.");
                }
                catch (Exception e)
                {
                    NLogger.Error(e, "Could not delete at least one excess log file.");
                    PrettyPrint.WithColor($"You have more than your setting of {logFileLimit} log files in the logs folder.", ConsoleColor.Yellow);
                    Console.WriteLine("Normally I'd take care of this for you but I had an unexpected error.");
                    Console.WriteLine("I've put some information about it in the log file.");
                    Console.WriteLine();
                    break;
                }
                
                files.RemoveAt(0);
            }
        }

        /// <summary>
        ///     Ensures that all required version swapping files are present and that the program is placed in a TR2 installation.
        /// </summary>
        public void ValidateInstallation()
        {
            ValidatePackagedFiles();
            NLogger.Info("Packaged files validated using MD5 hashes.");
            CheckGameDirLooksLikeATr2Install();
            NLogger.Info("Parent directory seems like a TR2 game installation.");
        }

        /// <summary>
        ///     Notifies the user if their program is outdated.
        /// </summary>
        public void VersionCheck()
        {
            NLogger.Debug("Running Github Version checks...");
            Version current = typeof(Program).Assembly.GetName().Version;
            try
            {
                Version latest = Github.GetLatestVersion().GetAwaiter().GetResult();
                int result = current.CompareTo(latest, 3);

                if (result == -1)
                {
                    NLogger.Debug($"Latest Github release ({latest}) is newer than the running version ({result}).");
                    PrettyPrint.Header("A new release is available!", Info.ReleaseLink, ConsoleColor.Green);
                    Console.WriteLine("You are strongly advised to update to ensure leaderboard compatibility:");
                }
                else if (result == 0)
                {
                    NLogger.Debug($"Version is up-to-date ({latest}).");
                }
                else // result == 1
                {
                    NLogger.Debug($"Running version ({current}) has not yet been released on Github ({latest}).");
                    Console.WriteLine("You seem to be running a pre-release version.");
                    Console.WriteLine("Let me know how testing goes! :D");
                }
            }
            catch (ApiException e)
            {
                NLogger.Error(e, "Github request failed.");
                PrettyPrint.WithColor("Unable to check for the latest version. Consider manually checking:", ConsoleColor.Yellow);
                Console.WriteLine(Info.ReleaseLink);
            }
            finally
            {
                Console.WriteLine();
            }
        }

        private static void ValidatePackagedFiles()
        {
            // Check that each version's game files are present and unmodified.
            ValidateMd5Hashes(FileAudit.VersionFiles, FileAudit.VersionFileHashes, _dirs.Versions);
            // Check that the utility files are present and unmodified.
            ValidateMd5Hashes(FileAudit.MusicFiles, FileAudit.MusicFileHashes, _dirs.MusicFix);
            ValidateMd5Hashes(FileAudit.PatchFiles, FileAudit.PatchFileHashes, _dirs.Patch);
        }

        private static void CheckGameDirLooksLikeATr2Install()
        {
            try
            {
                AllFilesInDirectory(FileAudit.GameFiles, _dirs.Game);
            }
            catch (RequiredFileMissingException e)
            {
                throw new BadInstallationLocationException("Parent folder is missing a critical game file, cannot be a TR2 installation.", e);
            }

            if (!Directory.Exists(Path.Combine(_dirs.Game, "music")))
            {
                throw new BadInstallationLocationException("Parent folder does not contain a music folder, cannot be a TR2 installation.");
            }
        }

        private static void AllFilesInDirectory(string[] fileNames, string dir)
        {
            foreach (string file in fileNames)
            {
                string path = Path.Combine(dir, file);
                if (!File.Exists(path))
                    throw new RequiredFileMissingException($"File \"{file}\" not found in\n\"{dir}\"");
            }
        }

        private static void ValidateMd5Hashes(string[] fileNames, string[] fileHashes, string dir)
        {
            for (int i = 0; i < fileNames.Length; ++i)
            {
                string path = Path.Combine(dir, fileNames[i]);
                string requiredHash = fileHashes[i];
                string fileHash = FileIo.ComputeMd5Hash(path);
                if (requiredHash != fileHash)
                    throw new InvalidGameFileException(
                        $"File {path} was modified.\nGot {fileHash}, expected {requiredHash}"
                    );
            }
        }

        /// <summary>
        ///     Finds a TR2 process running form the targeted game directory if it exists.
        /// </summary>
        /// <returns>The running Process or null.</returns>
        public Process FindTr2RunningFromGameDir()
        {
            NLogger.Debug("Checking for a TR2 process running in the target folder...");
            Process[] processes = Process.GetProcesses();
            return processes.FirstOrDefault(p => 
                p.ProcessName.ToLower() == "tomb2" &&
                p.MainModule != null &&
                Directory.GetParent(p.MainModule.FileName).FullName == _dirs.Game
            );
        }

        /// <summary>
        ///     Asks the user if they want the program to kill the running TR2 process. If user declines,
        ///     a loop will begin: first it gives a message and waits for user input to continues,
        ///     then exits if the process has ended.
        /// </summary>
        /// <param name="p">TR2 Process of concern</param>
        public void HandleRunningTr2Game(Process p)
        {
            NLogger.Debug($"Found a TR2 process running from target folder. Name: { p.ProcessName} | ID: { p.Id} | Start time: { p.StartTime.TimeOfDay}");
            PrettyPrint.WithColor("TR2 is running from the target folder.", ConsoleColor.Yellow);
            PrettyPrint.WithColor($"Name: {p.ProcessName} | ID: {p.Id} | Start time: {p.StartTime.TimeOfDay}");
            Console.WriteLine("Would you like me to end the task for you? If not, I will give a message");
            Console.Write("describing how to find and close it. Type \"y\" to have me kill the task: ");
            bool userResponse = UserPromptYesNo();
            if (userResponse)
            {
                NLogger.Debug($"User is allowing the program to kill the running TR2 task.");
                try
                {
                    p.Kill();
                }
                catch (Exception e)
                {
                    NLogger.Error(e, "An unexpected error occurred while trying to kill a running TR2 process.");
                    PrettyPrint.WithColor("I was unable to kill the TR2 process. You will have to do it yourself.", ConsoleColor.Yellow);
                }
            }
            else
            {
                NLogger.Debug($"User is opting to kill the running TR2 task on their own.");
                if (p.HasExited)
                {
                    NLogger.Debug("Process ended before the user prompt loop started.");
                    Console.WriteLine("Process ended by external actor. Skipping message prompt and wait loop.");
                    Console.WriteLine();
                }
                bool stillRunning = true;
                while (stillRunning)
                {
                    Console.WriteLine("Be sure that all TR2 game windows are closed. Then, if you are still");
                    Console.WriteLine("getting this message, check Task Manager for any phantom processes.");
                    Console.WriteLine("Press a key to continue. Or press CTRL + C to exit this program.");
                    NLogger.Debug("Waiting for user to close the running task, running ReadKey.");
                    Console.ReadKey(true);
                    stillRunning = !p.HasExited;
                    if (stillRunning)
                    { 
                        NLogger.Debug("User continued the program but the TR2 process is still running, looping.");
                        Console.WriteLine("Process still running, prompting again.");
                    }
                    NLogger.Debug("User continued the program after TR2 process has ended.");
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        ///     Checks if the music fix is already installed then takes the appropriate action.
        ///     If the fix is detected, simply lets the user know it is installed. Otherwise,
        ///     asks if the user would like the music fix installed and acts accordingly.
        /// </summary>
        public void HandleMusicFix()
        {
            if (IsMusicFixInstalled())
            {
                NLogger.Debug("Detected that the music fix is already installed. Reminding the user they have it installed.");
                PrettyPrint.Header("You already have the music fix installed.","Skipping music fix installation...");
                Console.WriteLine();
            }
            else
            {
                NLogger.Debug("Music fix is not installed. Asking the user if they want it to be installed.");
                Console.WriteLine("You switched to a non-Multipatch version. In-game music might not work and/or");
                Console.WriteLine("the game might freeze or lag when it tries to load music. I can install a music");
                Console.WriteLine("fix which should resolve most music-related issues.");
                Console.WriteLine("Please note that you are not required to install this optional fix. Any time you");
                Console.WriteLine("run this program and select a version, I will check for the fix and ask again");
                Console.WriteLine("if you want to install it. The fix applies to all versions the same, so it only");
                Console.Write("needs to be installed once. ");
                // We omit '\n' and leave a space so the response function can cleanly prompt on the same line.
                bool installFix = UserPromptYesNo();
                if (installFix)
                {
                    NLogger.Debug("User wants the music fix installed.");
                    FileIo.CopyRecursively(_dirs.MusicFix, _dirs.Game);
                    NLogger.Info("Installed music fix.");
                    PrettyPrint.Header("Music fix successfully installed!", foregroundColor: ConsoleColor.DarkGreen);
                }
                else
                {
                    NLogger.Debug("User declined the music fix installation.");
                    PrettyPrint.Header("Skipping music fix installation", "I will ask again next time.", ConsoleColor.White);
                }
            }
        }

        private static bool IsMusicFixInstalled()
        {
            try
            {
                AllFilesInDirectory(FileAudit.MusicFiles, _dirs.Game);
            }
            catch (RequiredFileMissingException)
            {
                return false;
            }
            return true;
        }
    }
}