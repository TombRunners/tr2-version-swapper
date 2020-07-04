using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using TRVS.Core;

namespace TR2_Version_Swapper
{
    /// <inheritdoc/>
    // ReSharper doesn't detect the Activator.CreateInstance{T} usage
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    internal class TR2VersionSwapper : VersionSwapperBase<TR2Directories>
    {
        /// <summary>
        ///     The user's <see cref="VersionPrompt"/> options.
        /// </summary>
        private enum Version
        {
            Multipatch = 1,
            EPC = 2,
            UKB = 3
        }

        /// <summary>
        ///     Mapping of <see cref="Version"/>s to directory <see langword="string"/>s.
        /// </summary>
        private static readonly Dictionary<Version, string> SelectionDictionary = new Dictionary<Version, string>
        {
            {Version.Multipatch, "Multipatch"},
            {Version.EPC, "Eidos Premier Collection"},
            {Version.UKB, "Eidos UK Box"}
        };

        protected override TRVSProgramData ProgramData { get; }
        protected override TRVSProgramManager ProgramManager { get; }
        protected override TR2Directories Directories { get; }
        
        public TR2VersionSwapper(TRVSProgramData programData, TRVSProgramManager programManager, TR2Directories directories)
        {
            ProgramData = programData;
            ProgramManager = programManager;
            Directories = directories;
        }

        /// <inheritdoc/>
        public override void SwapVersions()
        {
            HandleVersions();
            HandlePatch();
            HandleMusicFix();
        }

        /// <summary>
        ///     Prompts, then returns the directory of the user's chosen version.
        /// </summary>
        /// <returns>
        ///     The folder name of the chosen version
        /// </returns>
        private string VersionPrompt()
        {
            PrintVersionList();
            int selectionNumber = 0;
            while (selectionNumber < 1 || selectionNumber > 3)
            {
                Console.Write("Enter the number of your desired version: ");
                int.TryParse(Console.ReadLine(), out selectionNumber);
            }

            var selectedVersion = (Version)selectionNumber;
            ProgramData.NLogger.Debug($"User input `{selectionNumber}`, interpreting as {selectedVersion}");
            return SelectionDictionary[selectedVersion];
        }

        /// <summary>
        ///     Asks the user which version they want, then acts appropriately.
        /// </summary>
        private void HandleVersions()
        {
            string selectedVersion = VersionPrompt();
            string versionDir = Path.Combine(Directories.Versions, selectedVersion);

            TryCopyingDirectory(versionDir, Directories.Game);
            ProgramData.NLogger.Info($"Installed {selectedVersion} successfully.");
            ConsoleIO.PrintHeader($"{selectedVersion} successfully installed!", foregroundColor: ConsoleColor.Green);
            Console.WriteLine();
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
        private void HandlePatch()
        {
            Console.WriteLine("Would you like me to install CORE's Patch 1 on top of your selected version?");
            Console.WriteLine("Please note that you are not required to install this optional patch.");
            bool installPatch = ConsoleIO.UserPromptYesNo("Install CORE's Patch 1 onto your selected version? [y/n]: ");
            if (installPatch)
            {
                ProgramData.NLogger.Debug("User wants Patch 1 installed...");
                TryCopyingDirectory(Directories.Patch, Directories.Game);
                ProgramData.NLogger.Info("Installed Patch 1 successfully.");
                ConsoleIO.PrintHeader("Patch 1 successfully installed!", foregroundColor: ConsoleColor.Green);
            }
            else
            {
                ProgramData.NLogger.Debug("User declined Patch 1 installation.");
                ConsoleIO.PrintHeader("Skipping Patch 1 installation.", foregroundColor: ConsoleColor.White);
            }
            Console.WriteLine();
        }

        /// <summary>
        ///     Asks the user if they want the music fix, then acts appropriately.
        /// </summary>
        /// <remarks>
        ///     Checks if the music fix is already installed and takes the appropriate action.
        ///     If the fix is detected, simply reminds the user it is installed.
        /// </remarks>
        private void HandleMusicFix()
        {
            if (IsMusicFixInstalled())
            {
                ProgramData.NLogger.Debug("Music fix is already installed. Reminding the user they have it installed.");
                ConsoleIO.PrintHeader("You already have the music fix installed.", "Skipping music fix installation...", ConsoleColor.White);
                Console.WriteLine();
            }
            else
            {
                ProgramData.NLogger.Debug("Music fix is not installed. Asking the user if they want it to be installed.");
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
                    ProgramData.NLogger.Debug("User wants the music fix installed...");
                    TryCopyingDirectory(Directories.MusicFix, Directories.Game);
                    ProgramData.NLogger.Info("Installed music fix successfully.");
                    ConsoleIO.PrintHeader("Music fix successfully installed!", foregroundColor: ConsoleColor.Green);
                }
                else
                {
                    ProgramData.NLogger.Debug("User declined the music fix installation.");
                    ConsoleIO.PrintHeader("Skipping music fix.", "I'll ask again next time.", ConsoleColor.White);
                }
            }
        }

        /// <summary>
        ///     Checks if the music fix is installed in the game directory.
        /// </summary>
        /// <returns>
        ///     <see langword="true"/> if music fix is installed, <see langword="false"/> otherwise
        /// </returns>
        private bool IsMusicFixInstalled()
        {
            string firstMissingFile = FileIO.FindMissingFile(TR2FileAudit.MusicFilesAudit.Keys, Directories.Game);
            return string.IsNullOrEmpty(firstMissingFile);
        }
    }
}