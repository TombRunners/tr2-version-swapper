using System;

using NLog;
using Utils;


namespace TR2_Version_Swapper
{
    internal class Program
    {
        internal static readonly Logger NLogger = LogManager.GetCurrentClassLogger();

        internal static InstallDirectories Directories;

        internal static UserSettings Settings;

        internal const string Game = "TR2";

        public static int Main(string[] args)
        {
            ProgramManager.InitializeProgram(args);
            ProgramManager.DeleteExcessLogFiles();
            
            InstallationManager.VersionCheck();
            InstallationManager.ValidateInstallation();
            
            VersionSwapper.HandleVersions();
            VersionSwapper.HandlePatch();
            VersionSwapper.HandleMusicFix();

            ConsoleIO.PrintHeader("Version swap complete!","Press any key to exit...", ConsoleColor.White);
            Console.ReadKey(true);
            return 0;
        }
    }
}
