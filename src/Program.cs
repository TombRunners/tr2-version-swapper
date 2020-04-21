using System;

using NLog;
using TR2_Version_Swapper.Utils;


namespace TR2_Version_Swapper
{
    internal class Program
    {
        internal static readonly Logger NLogger = LogManager.GetCurrentClassLogger();

        internal static InstallDirectories Directories;

        internal static int LogFileLimit;

        public static int Main(string[] args)
        {
            if (!ProgramManager.InitializeProgram(args))
                return 1;

            ProgramManager.DeleteExcessLogFiles();
            ProgramManager.VersionCheck();
            ProgramManager.EnsureNoTr2RunningFromGameDir();

            try
            {
                ProgramManager.ValidateInstallation();
            }
            catch (Exception e)
            {
                if (e is BadInstallationLocationException ||
                    e is RequiredFileMissingException ||
                    e is InvalidGameFileException)
                {
                    NLogger.Fatal($"Installation failed to validate. {e.Message}\n{e.StackTrace}");
                    ConsoleIO.PrintWithColor(e.Message, ConsoleColor.Red);
                    Console.WriteLine("You are advised to re-install the latest release to fix the issue:");
                    Console.WriteLine(Info.ReleaseLink);
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey(true);
                    return 2;
                }

                NLogger.Fatal($"An unhandled exception occurred while validating the installation. {e.Message}\n{e.StackTrace}");
                ConsoleIO.PrintWithColor("An unhandled exception occurred while validating your installation.", ConsoleColor.Red);
                Console.WriteLine("I've put some information about it in the log file.");
                return -1;
            }
            
            // TODO: VersionSwapper.HandleVersionInstallation();
            VersionSwapper.HandleMusicFix();
            // TODO: VersionSwapper.HandlePatchInstallation();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
            return 0;
        }
    }
}
