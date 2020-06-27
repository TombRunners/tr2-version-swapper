using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

using CommandLine;
using CommandLine.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using Utils;

namespace TR2_Version_Swapper
{
    /// <summary>
    ///     Class used by <see cref="Parser"/>.
    /// </summary>
    /// <remarks>
    ///     The lack of explicit set usage (CommandLine's Bind not detected as such)
    ///     causes some unnecessary ReSharper warnings which have been suppressed.
    /// </remarks>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    internal class ProgramArguments
    {
        [Option('v', Default = false, HelpText = "Enable console logging.")]
        public bool Verbose { get; set; }
    }

    /// <summary>
    ///     A more custom, thorough version of <see cref="ParserResult{T}.Tag"/>.
    /// </summary>
    internal enum ArgumentParseResult
    {
        ParsedAndShouldContinue,
        HelpOrVersionArgGiven,
        FailedToParse
    }

    /// <summary>
    ///     Contains the program settings that can be changed by the user.
    /// </summary>
    /// <remarks>
    ///     ReSharper doesn't recognize the public set is used by and required for
    ///     full JSON deserialization, thus the warning is suppressed.
    /// </remarks>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    internal struct UserSettings
    {
        public int LogFileLimit { get; set; }
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
    internal static class ProgramManager
    {
        /// <summary>
        ///     Readies console, handles args and settings, hooks SIGINT.
        /// </summary>
        /// <param name="args">Program arguments</param>
        public static void InitializeProgram(IEnumerable<string> args)
        {
            
            SetStageAndPrintSplash();

            switch (HandleProgramArgs(args))
            {
                case ArgumentParseResult.ParsedAndShouldContinue:
                    break;
                case ArgumentParseResult.HelpOrVersionArgGiven:
                    Environment.Exit(0); // No need to pause since they definitely used CMD/PS.
                    break;
                case ArgumentParseResult.FailedToParse:
                    EarlyPauseAndExit(1);
                    break;
                default:
                    var e = new ArgumentOutOfRangeException();
                    GiveErrorMessageAndExit("An unexpected error occurred after parsing arguments.", e, -1);
                    break;
            }

            SetSigIntHook();
            SetDirectories();
            HandleUserSettings();
        }

        /// <summary>
        ///     Sets up console and prints the intro splash.
        /// </summary>
        private static void SetStageAndPrintSplash()
        {
            Console.Title = $"{Program.Game} Version Swapper";
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
        /// <returns>The appropriate ArgumentParseResult value</returns>
        private static ArgumentParseResult HandleProgramArgs(IEnumerable<string> args)
        {
            var result = ArgumentParseResult.ParsedAndShouldContinue;
            var parser = new Parser(with => with.HelpWriter = null);
            ParserResult<ProgramArguments> parserResult = parser.ParseArguments<ProgramArguments>(args);
            parserResult
                .WithParsed(ConfigLogger)
                .WithNotParsed(errs =>
                {
                    if (errs.IsHelp() || errs.IsVersion())
                        result = ArgumentParseResult.HelpOrVersionArgGiven;
                    else
                        result = ArgumentParseResult.FailedToParse;

                    DisplayHelp(parserResult, errs);
                });
            
            return result;
        }

        /// <summary>
        ///     Configures the logger according to program arguments. 
        /// </summary>
        /// <param name="args">Program arguments</param>
        private static void ConfigLogger(ProgramArguments args)
        {
            LogLevel consoleLogLevel = args.Verbose ? LogLevel.Info : LogLevel.Off;
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
        /// <param name="result">Results from CommandLine's parser</param>
        /// <param name="errs">Errors from CommandLine's parser</param>
        private static void DisplayHelp(ParserResult<ProgramArguments> result, IEnumerable<Error> errs)
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
                ConsoleIO.PrintWithColor("Received SIGINT. It's up to you to know the current state of your game!", ConsoleColor.Yellow);
            };
        }

        /// <summary>
        ///     Tries to get user settings; creates default if file doesn't exist.
        /// </summary>
        private static void HandleUserSettings() 
        {
            const string fileName = "appsettings.json";
            if (!File.Exists(fileName))
                CreateDefaultUserSettingsFile(fileName);

            try
            {
                ApplyUserSettingsFromFile(fileName);
            }
            catch (JsonException e)
            {
                const string statement = "An error was encountered while reading the user settings file.";
                GiveErrorMessageAndExit(statement, e, 1);
            }
        }
        
        /// <summary>
        ///     Writes, then alerts the user of a default user settings file.
        /// </summary>
        /// <param name="fileName">User settings file name</param>
        private static void CreateDefaultUserSettingsFile(string fileName)
        {
            using StreamWriter stream = File.CreateText(fileName);
            foreach (string line in Misc.DefaultSettingsFile)
                stream.WriteLine(line);

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            Program.NLogger.Debug($"Created a default user settings file at {filePath}.");
            Console.WriteLine("I created a default user settings files at");
            Console.WriteLine(filePath);
            Console.WriteLine("You can edit the settings in this file to your desired amounts.");
            Console.WriteLine();
        }

        /// <summary>
        ///     Parses and deserializes user settings into a <see cref="UserSettings"/> object.
        /// </summary>
        /// <param name="fileName">User settings file name</param>
        private static void ApplyUserSettingsFromFile(string fileName)
        {
            var jsonOptions = new JsonSerializerOptions()
            {
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            var parsedSettings = JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(fileName), jsonOptions);
            Program.Settings = parsedSettings;
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
                    Console.WriteLine("I've put some additional information in this session's log file.");
                    Console.WriteLine();
                    break;
                }
                
                files.RemoveAt(0);
            }
        }

        /// <summary>
        ///     Provides a standardized format to display an error and exit. 
        /// </summary>
        /// <param name="statement">The string to log and also print to console</param>
        /// <param name="e">The exception inspiring the early program exit</param>
        /// <param name="exitCode">The return code to return to the OS</param>
        public static void GiveErrorMessageAndExit(string statement, Exception e, int exitCode)
        {
            Program.NLogger.Fatal($"{statement} {e.Message}\n{e.StackTrace}");
            ConsoleIO.PrintWithColor(statement, ConsoleColor.Red);
            Console.WriteLine("I've put some additional information in this session's log file.");
            EarlyPauseAndExit(exitCode);
        }

        /// <summary>
        ///     Ends program after pausing to prevent immediate CMD window exits.
        /// </summary>
        /// <param name="exitCode">The return code to return to the OS</param>
        public static void EarlyPauseAndExit(int exitCode)
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
            Environment.Exit(exitCode);
        }
    }
}