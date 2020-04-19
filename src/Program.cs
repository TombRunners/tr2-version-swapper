using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using NLog;
using NLog.Config;
using NLog.Targets;

using TR2_Version_Swapper.Utils;


namespace TR2_Version_Swapper
{
    internal class Program
    {
        private static readonly Logger NLogger = LogManager.GetCurrentClassLogger();

        private class Args
        {
            [Option('v', Default = false, HelpText = "Enable console logging.")]
            public bool Verbose { get; set; }
        }

        private static int LogFileLimit;

        public static int Main(string[] args)
        {
            if (!InitializeProgram(args))
                return 1;

            var versionSwapper = new VersionSwapper(NLogger);
            versionSwapper.DeleteExcessLogFiles(LogFileLimit);
            versionSwapper.VersionCheck();

            try
            {
                versionSwapper.ValidateInstallation();
            }
            catch (Exception e)
            {
                if (e is BadInstallationLocationException ||
                    e is RequiredFileMissingException ||
                    e is InvalidGameFileException)
                {
                    NLogger.Fatal(e, "Installation failed to validate.");
                    NLogger.Fatal(e.StackTrace);
                    PrettyPrint.WithColor(e.Message, ConsoleColor.Red);
                    Console.WriteLine("You are advised to re-install the latest release to fix the issue:");
                    Console.WriteLine(Info.ReleaseLink);
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey(true);
                    return 2;
                }

                NLogger.Fatal(e, "An unhandled error occurred while validating the installation.");
                PrettyPrint.WithColor("An unhandled exception occurred while validating your installation.", ConsoleColor.Red);
                Console.WriteLine("I've put some information about it in the log file.");
                return -1;
            }

            try
            {
                Process tr2Process = versionSwapper.FindTr2RunningFromGameDir();
                if (tr2Process == null)
                {
                    NLogger.Info("No TR2 processes running from the target folder.");
                }
                else
                {
                    NLogger.Debug("Found running TR2 process of concern.");
                    versionSwapper.HandleRunningTr2Game(tr2Process);
                    NLogger.Info("Handled running TR2 processes.");
                }
            }
            catch (Exception e)
            {
                NLogger.Error(e, "An unexpected error occurred while trying to find running TR2 processes.");
                PrettyPrint.WithColor("I was unable to finish searching for running TR2 processes.", ConsoleColor.Yellow);
                Console.WriteLine("Please note that a TR2 game or background task running from the target folder");
                Console.WriteLine("could cause issues with the program, such as preventing overwrites.");
                Console.WriteLine("Double-check and make sure no TR2 game or background task is running.");
            }

            versionSwapper.HandleMusicFix();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
            return 0;
        }

        /// <summary>
        ///     Prints the splash screen, handles program arguments, creates a SIGINT hook.
        /// </summary>
        /// <param name="args">Program arguments</param>
        /// <returns>true if the program can continue, false if args generated errors or if help/version was requested.</returns>
        private static bool InitializeProgram(IEnumerable<string> args)
        {
            // Ensure the program is readable and print the intro splash.
            if (Console.WindowWidth < 81)
                Console.WindowWidth = 81;
            foreach (string s in Info.AsciiArt) PrettyPrint.Center(s, ConsoleColor.DarkCyan);
            PrettyPrint.Center("Made with love by Midge", ConsoleColor.DarkCyan);
            PrettyPrint.Center($"Source code: {Info.RepoLink}");

            // Parse and propagate arguments.
            var parser = new Parser(with => with.HelpWriter = null);
            ParserResult<Args> parserResult = parser.ParseArguments<Args>(args);
            parserResult
                .WithParsed(ConfigLogger)
                .WithNotParsed(errs => DisplayHelp(parserResult, errs));

            // Create a hook to handle SIGINT (CTRL + C).
            Console.CancelKeyPress += delegate {
                NLogger.Debug("User gave SIGINT. Ending Program.");
                PrettyPrint.WithColor(
                    "Received SIGINT. It's up to you to know the current state of your game!", ConsoleColor.Yellow);
            };

            // Apply settings from appsettings.json.
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            if (!int.TryParse(config["LogFileLimit"], out LogFileLimit))
            {
                LogFileLimit = 10;
                PrettyPrint.WithColor("Unable to read the numeric value \"LogFileLimit\" from appsettings.json", ConsoleColor.Yellow);
                Console.WriteLine("To fix this, you need to edit the number to be an integer without quotes.");
                return false;
            }

            return parserResult.Tag == ParserResultType.Parsed;
        }

        private static void ConfigLogger(Args opts)
        {
            LogLevel consoleLogLevel = opts.Verbose ? LogLevel.Info : LogLevel.Off;
            var config = new LoggingConfiguration();

            // Configure the targets and rules.
            var consoleTarget = new ConsoleTarget();
            consoleTarget.Layout = "${uppercase:${level}}: ${message} ${exception}";
            config.AddRule(consoleLogLevel, LogLevel.Error, consoleTarget);

            var fileTarget = new FileTarget();
            fileTarget.FileName = Path.Combine("logs", typeof(Program).FullName + "." + DateTime.Now.ToString("s") + ".log");
            fileTarget.Layout = "${longdate} | ${stacktrace} | ${uppercase:${level}} | ${message} ${exception}";
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

            // Set and load the configuration.
            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
            NLogger.Info("Verbose mode activated.");
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

    }
}
