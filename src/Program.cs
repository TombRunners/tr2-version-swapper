using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using CommandLine;
using CommandLine.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using Octokit;

using TR2_Version_Swapper.Utils;


namespace TR2_Version_Swapper
{
    internal class Program
    {
        private static readonly string[] AsciiArt =
        {
            @"    _______ _____  ___                     ",
            @"   |__   __|  __ \|__ \                    ",
            @"      | |  | |__) |  ) |                   ",
            @"      | |  |  _  /  / /                    ",
            @"      | |  | | \ \ / /_                    ",
            @"__    |_|_ |_|  \_\____|                   ",
            @"\ \    / /          (_)                    ",
            @" \ \  / /__ _ __ ___ _  ___  _ __          ",
            @"  \ \/ / _ \ '__/ __| |/ _ \| '_ \         ",
            @"   \  /  __/ |  \__ \ | (_) | | | |        ",
            @"  __\/_\___|_|  |___/_|\___/|_| |_|        ",
            @" / ____|                                   ",
            @"| (_____      ____ _ _ __  _ __   ___ _ __ ",
            @" \___ \ \ /\ / / _` | '_ \| '_ \ / _ \ '__|",
            @" ____) \ V  V / (_| | |_) | |_) |  __/ |   ",
            @"|_____/ \_/\_/ \__,_| .__/| .__/ \___|_|   ",
            @"                    | |   | |              ",
            @"                    |_|   |_|              "
        };

        private const string RepoLink = "https://github.com/TombRunners/tr2-version-swapper/";
        
        private const string ReleaseLink = "https://github.com/TombRunners/tr2-version-swapper/releases/latest";

        private static readonly Logger NLogger = LogManager.GetCurrentClassLogger();

        private class Options
        {
            [Option('v', Default = false, HelpText = "Enable console logging.")]
            public bool Verbose { get; set; }
        }

        public static int Main(string[] args)
        {
            if (!InitializeProgram(args))
                return 1;

            bool fileDeleted = FileIo.DeleteExcessLogFiles();
            if (fileDeleted)
                NLogger.Info("Excess log file deleted.");

            VersionCheck();

            var io = new InstallHelper(NLogger);
            NLogger.Debug("Checking required files and install location...");
            try
            {
                io.ValidateInstallation();
            }
            catch (Exception e)
            {
                if (e is BadInstallationLocationException ||
                    e is RequiredFileMissingException ||
                    e is InvalidGameFileException)
                {
                    NLogger.Fatal(e, "Installation failed to validate.");
                    NLogger.Fatal(e.StackTrace);
                    PrintInstallationInstructions(e);
                    Console.ReadKey();
                    return 2;
                }

                NLogger.Fatal(e, "An unexpected error occurred while validating the installation.");
                return -1;
            }
            NLogger.Info("Required files found and install locations looks good.");

            NLogger.Debug("Checking for a TR2 process running in the target folder...");
            try
            {
                Process tr2Process = io.FindTr2RunningFromGameDir();
                if (tr2Process == null)
                {
                    NLogger.Info("No TR2 processes running from the target folder.");
                }
                else
                {
                    NLogger.Debug("Found running TR2 process of concern.");
                    io.HandleRunningTr2Game(tr2Process);
                    NLogger.Info("Handled running TR2 processes.");
                }
            }
            catch (Exception e)
            {
                NLogger.Error(e, "An unexpected error occurred while trying to find running TR2 processes.");
                PrettyPrint.WithColor("I was unable to finish searching for running TR2 process.", ConsoleColor.Yellow);
                Console.WriteLine("Please note that a TR2 game or background task running from the target folder,");
                Console.WriteLine("could cause issues with the program, such as preventing overwrites.");
                Console.WriteLine("Double-check and be sure no such TR2 game or background task is running.");
            }



            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
            return 0;
        }

        /// <summary>
        ///     Prints the splash screen and handles program arguments.
        /// </summary>
        /// <param name="args">Program arguments</param>
        /// <returns>True if the program can continue. False if args generated errors or if help/version was passed.</returns>
        private static bool InitializeProgram(IEnumerable<string> args)
        {
            // Ensure the program is readable and print the splash.
            if (Console.WindowWidth < 81)
                Console.WindowWidth = 81;
            PrintIntroSplash();

            // Parse arguments and propagate arguments..
            var parser = new Parser(with => with.HelpWriter = null);
            ParserResult<Options> parserResult = parser.ParseArguments<Options>(args);
            parserResult
                .WithParsed(ConfigLogger)
                .WithNotParsed(errs => DisplayHelp(parserResult, errs));
            return parserResult.Tag == ParserResultType.Parsed;
        }

        private static void ConfigLogger(Options opts)
        {
            LogLevel consoleLogLevel = opts.Verbose ? LogLevel.Info : LogLevel.Off;

            var config = new LoggingConfiguration();

            // Configure the targets and rules.
            var consoleTarget = new ConsoleTarget
            {
                Layout = "${uppercase:${level}}: ${message} ${exception}"
            };
            config.AddRule(consoleLogLevel, LogLevel.Error, consoleTarget);
            var fileTarget = new FileTarget
            {
                FileName = Path.Combine("logs", typeof(Program).FullName + "." + DateTime.Now.ToString("s") + ".log"),
                Layout = "${longdate} | ${stacktrace} | ${uppercase:${level}} | ${message} ${exception}"
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

            // Set and load the configuration.
            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
            NLogger.Info("Verbose mode activated.");

            // Create a hook to handle SIGINT (CTRL + C).
            Console.CancelKeyPress += delegate {
                NLogger.Debug("User gave SIGINT. Ending Program.");
                PrettyPrint.WithColor(
                    "Received SIGINT. It's up to you to know the current state of your game!", ConsoleColor.Yellow);
            };
        }

        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            HelpText helpText = null;
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

        private static void PrintIntroSplash()
        {
            foreach (string s in AsciiArt) PrettyPrint.Center(s, ConsoleColor.DarkCyan);
            PrettyPrint.Center("Made with love by Midge", ConsoleColor.DarkCyan);
            PrettyPrint.Center($"Source code: {RepoLink}");
        }

        /// <summary>
        ///     Notifies the user if their program is outdated.
        /// </summary>
        private static void VersionCheck()
        {
            NLogger.Debug("Running Github Version checks...");
            Version current = typeof(Program).Assembly.GetName().Version;
            try
            {
                Version latest = Github.GetLatestRelease().GetAwaiter().GetResult();
                int result = current.CompareTo(latest, 3);

                if (result == -1)
                {
                    NLogger.Debug($"Latest Github release ({latest}) is newer than the running version ({result}).");
                    PrettyPrint.Header("A new release is available!", ReleaseLink, ConsoleColor.Green);
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
                Console.WriteLine("Unable to check for the latest version. Consider manually checking:");
                Console.WriteLine(ReleaseLink);
            }
            finally
            {
                Console.WriteLine();
            }
        }

        private static void PrintInstallationInstructions(Exception e)
        {
            PrettyPrint.WithColor(e.Message, ConsoleColor.Red);
            Console.WriteLine("You are advised to re-install the latest release to fix the issue:");
            Console.WriteLine(ReleaseLink);
        }
    }
}
