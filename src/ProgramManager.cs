using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

using CommandLine;
using CommandLine.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using Octokit;
using Utils;

namespace TR2_Version_Swapper
{
    /// <summary>
    ///     Class used by CommandLine.
    /// </summary>
    /// <remarks>
    ///     The lack of explicit set usage (CommandLine's Bind not detected as such)
    ///     causes some unnecessary ReSharper warnings which have been suppressed.
    /// </remarks>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    internal class Args
    {
        [Option('v', Default = false, HelpText = "Enable console logging.")]
        public bool Verbose { get; set; }
    }

    /// <summary>
    ///     Contains the program settings that can be changed by the user.
    /// </summary>
    internal struct UserSettings
    {
        public int LogFileLimit;
    }

    /// <summary>
    ///     Organizes working and target directories.
    /// </summary>
    internal struct InstallDirectories
    {
        public string Game;
        public string Versions;
        public string MusicFix;
        public string Patch;
    }

    /// <summary>
    ///     Provides functionality for program initialization and maintenance.
    /// </summary>
    public static class ProgramManager
    {
        /// <summary>
        ///     Sets title, prints intro, handles args and settings, hooks SIGINT.
        /// </summary>
        /// <param name="args">Program arguments</param>
        /// <returns>False if args/settings had errors or help/version was requested, true otherwise</returns>
        public static bool InitializeProgram(IEnumerable<string> args)
        {
            Console.Title = "TR2 Version Swapper";
            PrintSplash();
            bool argsParsedAndNotHelpOrVersion = HandleProgramArgs(args);
            SetSigIntHook();
            bool settingsParsed = HandleUserSettings();
            SetDirectories();

            return argsParsedAndNotHelpOrVersion && settingsParsed;
        }

        /// <summary>
        ///     Ensures console is sufficiently wide then prints the intro splash.
        /// </summary>
        private static void PrintSplash()
        {
            if (Console.WindowWidth < 81)
                Console.WindowWidth = 81;

            foreach (string s in Misc.AsciiArt)
                ConsoleIO.PrintCentered(s, ConsoleColor.DarkCyan);

            ConsoleIO.PrintCentered("Made with love by Midge", ConsoleColor.DarkCyan);
            ConsoleIO.PrintCentered($"Source code: {Misc.RepoLink}");
        }

        /// <summary>
        ///     Parses and propagates command-line arguments.
        /// </summary>
        /// <param name="args">Program arguments</param>
        /// <returns>False if args were incorrect or help/version was requested, true otherwise</returns>
        private static bool HandleProgramArgs(IEnumerable<string> args)
        {
            var parser = new Parser(with => with.HelpWriter = null);
            ParserResult<Args> parserResult = parser.ParseArguments<Args>(args);
            parserResult
                .WithParsed(ConfigLogger)
                .WithNotParsed(errs => DisplayHelp(parserResult, errs));
            return parserResult.Tag == ParserResultType.Parsed;
        }

        /// <summary>
        ///     Configures the logger according to program arguments. 
        /// </summary>
        /// <param name="opts"></param>
        private static void ConfigLogger(Args opts)
        {
            LogLevel consoleLogLevel = opts.Verbose ? LogLevel.Info : LogLevel.Off;
            var config = new LoggingConfiguration();

            // Configure the targets and rules.
            var consoleTarget = new ConsoleTarget
            {
                Layout = "${uppercase:${level}}: ${message} ${exception}"
            };
            config.AddRule(consoleLogLevel, LogLevel.Error, consoleTarget);

            Directory.CreateDirectory("logs");
            var fileTarget = new FileTarget
            {
                FileName = Path.Combine("logs", typeof(Program).FullName + "." + DateTime.Now.ToString("s") + ".log"),
                Layout = "${longdate} | ${stacktrace} | ${uppercase:${level}} | ${message} ${exception}"
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

            // Set and load the configuration.
            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
            Program.NLogger.Info("Verbose mode activated.");
        }

        /// <summary>
        ///     Generates and prints customized messages for help and version.
        /// </summary>
        /// <param name="result">Results from commandline library's parser</param>
        /// <param name="errs">Errors from commandline library's parser</param>
        private static void DisplayHelp(ParserResult<Args> result, IEnumerable<Error> errs)
        {
            HelpText helpText;
            if (errs.IsVersion())
            {
                helpText = HelpText.AutoBuild(result);
            }
            else
            {
                helpText = HelpText.AutoBuild(result, h =>
                {
                    h.AddNewLineBetweenHelpSections = false;
                    h.AdditionalNewLineAfterOption = false;
                    h.MaximumDisplayWidth = 80;
                    return HelpText.DefaultParsingErrorsHandler(result, h);
                }, e => e);
            }
            Console.WriteLine(helpText);
        }

        /// <summary>
        ///     Creates a hook to handle SIGINT (CTRL + C).
        /// </summary>
        private static void SetSigIntHook()
        {
            Console.CancelKeyPress += delegate {
                Program.NLogger.Debug("User gave SIGINT. Ending Program.");
                ConsoleIO.PrintWithColor(
                    "Received SIGINT. It's up to you to know the current state of your game!", ConsoleColor.Yellow);
            };
        }

        /// <summary>
        ///     Tries to get user settings; creates default if file doesn't exist.
        /// </summary>
        /// <returns>True if process was completed successfully, false if not</returns>
        private static bool HandleUserSettings()
        {
            const string fileName = "appsettings.json";
            
            if (!File.Exists(fileName))
            {
                CreateDefaultUserSettingsFile();
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                Program.NLogger.Debug($"Created a default user settings file at {filePath}.");
                Console.WriteLine("I created a default user settings files at");
                Console.WriteLine(filePath);
                Console.WriteLine("You can edit the settings in this file to your desired amounts.");
                Console.WriteLine();
            }
            
            return ApplyUserSettingsFromFile(fileName);
        }
        
        /// <summary>
        ///     Writes a default user settings file.
        /// </summary>
        private static void CreateDefaultUserSettingsFile()
        {
            using StreamWriter stream = File.CreateText("appsettings.json");
            foreach (string line in Misc.DefaultSettingsFile)
                stream.WriteLine(line);
        }

        /// <summary>
        ///     Serializes user settings into the UserSettings object.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static bool ApplyUserSettingsFromFile(string fileName)
        {
            // TODO: Allow user to see failures by displaying message and "Press any key to exit..." prompt.
            try
            {
                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(fileName, optional: false)
                    .Build();
                
                // Try to parse and give specific error messages for each setting.
                if (!int.TryParse(config["LogFileLimit"], out Program.Settings.LogFileLimit))
                {
                    ConsoleIO.PrintWithColor($"Unable to read the numeric value \"LogFileLimit\" from {fileName}", ConsoleColor.Yellow);
                    Console.WriteLine("To fix this, you need to edit the number to be an integer without quotes.");
                    return false;
                }
            }
            catch (FormatException e)
            {
                Program.NLogger.Fatal($"An error occurred while parsing the user settings file! {e.InnerException?.Message}");
                ConsoleIO.PrintWithColor("A critical error occurred while trying to read settings.");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Sets the program's working and target directories.
        /// </summary>
        private static void SetDirectories()
        {
            string root = Path.GetFullPath(Directory.GetCurrentDirectory());
            Program.Directories = new InstallDirectories
            {
                Game = Directory.GetParent(root).FullName,
                Versions = Path.Combine(root, "versions"),
                MusicFix = Path.Combine(root, "utilities/music_fix"),
                Patch = Path.Combine(root, "utilities/patch"),
            };
        }
        
        /// <summary>
        ///     Deletes the oldest log file(s) according to user's set limit.
        /// </summary>
        /// <returns>True if a file was deleted, false otherwise.</returns>
        public static void DeleteExcessLogFiles()
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            var files = new List<string>(Directory.GetFiles(dir));
            files.Sort();
            
            int limit = Program.Settings.LogFileLimit;
            if (limit == 0)
                return;
            
            if (files.Count > limit)
            {
                Program.NLogger.Debug($"Excessive log file count: {files.Count} vs {limit}");
                ConsoleIO.PrintWithColor($"Log file limit of {limit} exceeded (total: {files.Count})", ConsoleColor.Yellow);
                Console.WriteLine("Files will be deleted accordingly.");
                Console.WriteLine();
            }
            else if (files.Count + 3 > limit)
            {
                Program.NLogger.Debug($"Log file count approaching excessive: {files.Count} vs {limit}");
                ConsoleIO.PrintWithColor($"You are approaching your set log file limit ({files.Count} of {limit})", ConsoleColor.Yellow);
                Console.WriteLine("Be sure to edit appsettings.json to adjust the limit to your tastes.");
                Console.WriteLine();
            }

            while (files.Count > limit)
            {
                try
                {
                    File.Delete(files[0]);
                    Program.NLogger.Info($"Deleted excess log file {files[0]}.");
                }
                catch (Exception e)
                {
                    Program.NLogger.Error(e, "Could not delete at least one excess log file.");
                    ConsoleIO.PrintWithColor($"You have more than your setting of {limit} log files in the logs folder.", ConsoleColor.Yellow);
                    Console.WriteLine("Normally I'd take care of this for you but I had an unexpected error.");
                    Console.WriteLine("I've put some information about it in the log file.");
                    Console.WriteLine();
                    break;
                }
                
                files.RemoveAt(0);
            }
        }

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
            ValidatePackagedFiles();
            Program.NLogger.Info("Packaged files validated using MD5 hashes.");
            CheckGameDirLooksLikeATr2Install();
            Program.NLogger.Info("Parent directory seems like a TR2 game installation.");
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
                    {
                        throw new InvalidGameFileException(
                            $"File {file} was modified.\nGot {hash}, expected {requiredHash}"
                        );
                    }
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
            Program.NLogger.Debug($"Found a TR2 process running from target folder. Name: { p.ProcessName} | ID: { p.Id} | Start time: { p.StartTime.TimeOfDay}");
            ConsoleIO.PrintWithColor("TR2 is running from the target folder.", ConsoleColor.Yellow);
            ConsoleIO.PrintWithColor($"Name: {p.ProcessName} | ID: {p.Id} | Start time: {p.StartTime.TimeOfDay}");
            Console.WriteLine("Would you like me to end the task for you? If not, I will give a message");
            Console.Write("describing how to find and close it. Type \"y\" to have me kill the task: ");
            bool userResponse = ConsoleIO.UserPromptYesNo();
            if (userResponse)
            {
                Program.NLogger.Debug("User is allowing the program to kill the running TR2 task.");
                try
                {
                    p.Kill();
                }
                catch (Exception e)
                {
                    Program.NLogger.Error(e, "An unexpected error occurred while trying to kill a running TR2 process.");
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
                        Program.NLogger.Debug("User continued the program but the TR2 process is still running, looping.");
                        Console.WriteLine("Process still running, prompting again.");
                    }
                    Program.NLogger.Debug("User continued the program after TR2 process has ended.");
                    Console.WriteLine();
                }
            }
        }
    }
}