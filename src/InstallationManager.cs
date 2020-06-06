using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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
            catch (Exception e)
            {
                if (e is ApiException || e is HttpRequestException)
                    Program.NLogger.Error($"Github request failed due to API/HTTP failure. {e.Message}\n{e.StackTrace}");
                else
                    Program.NLogger.Error($"Version check failed with an unforeseen error. {e.Message}\n{e.StackTrace}");

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
            foreach (KeyValuePair<string, string> item in fileAudit)
            {
                try
                {
                    string hash = FileIO.ComputeMd5Hash(Path.Combine(dir, item.Key));
                    if (hash != item.Value)
                        throw new InvalidGameFileException($"File {item.Key} was modified.\nGot {hash}, expected {item.Value}");
                }
                catch (FileNotFoundException e)
                {
                    throw new RequiredFileMissingException(e.Message);
                }
            }
        }
    }
}
