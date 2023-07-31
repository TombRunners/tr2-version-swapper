using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Win32;
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
        
        /// <summary>
        ///     The installed games music track file extensions.
        /// </summary>
        /// <remarks>
        ///     Steam ships with MP3 and GOG versions ship with OGG.
        /// </remarks>
        private enum MusicFileType
        {
            Mp3 = 1,
            Ogg = 2,
            OtherOrUnknown = 3
        }

        private static readonly List<string> InstalledMusicFixFiles = new List<string> {"fmodex.dll", "winmm.dll"};

        private const string FullscreenBorderFixName = "Tomb Raider series fullscreen border fix";

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
            string choice = string.Empty;
            int selectionNumber = 0;
            while (selectionNumber < 1 || selectionNumber > 3)
            {
                Console.Write("Enter your desired version number, or enter nothing for the default [3]: ");
                choice = Console.ReadLine().Trim();
                if (string.IsNullOrEmpty(choice))
                    selectionNumber = 3;
                else
                    int.TryParse(choice, out selectionNumber);
            }

            var selectedVersion = (Version)selectionNumber;
            ProgramData.NLogger.Debug($"User input `{choice}`, interpreting as {selectedVersion}");
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
            bool installPatch = ConsoleIO.UserPromptYesNo("Install CORE's Patch 1 onto your selected version? [y/N]: ", ConsoleIO.DefaultOption.No);
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
            MusicFileType ext = DetermineMusicFileType();
            if (ext == MusicFileType.OtherOrUnknown)
            {
                ProgramData.NLogger.Debug("Could not determine which music file types are installed. Alerting user.");
                Console.WriteLine("I could not find MP3 or OGG files in your game installation's music folder.");
                Console.WriteLine("Therefore, I cannot determine whether you have a correct music fix installed.");
                Console.WriteLine("I recommend you validate or reinstall from Steam/GOG to acquire music files");
                Console.WriteLine("that my supplied music fix utility can fix.");
                return;
            }
                                
            bool correctMusicFixIsInstalled = IsCorrectMusicFixInstalled(ext);
            if (correctMusicFixIsInstalled)
            {
                ProgramData.NLogger.Debug("Music fix is already installed. Reminding the user they have it installed.");
                ConsoleIO.PrintHeader("You already have the music fix installed.", "Skipping music fix installation...", ConsoleColor.White);
                Console.WriteLine();
            }
            else
            {
                ProgramData.NLogger.Debug("Music fix is not installed. Asking the user if they want it to be installed.");
                Console.WriteLine("After switching game versions, in-game music will likely fail and/or the game");
                Console.WriteLine("could freeze or lag when it tries to load music files. I can install a music");
                Console.WriteLine("fix to resolve these issues.");
                Console.WriteLine("Please note that you are not required to install this optional fix. Any time you");
                Console.WriteLine("run this program and select a version, I will check for the fix and ask again");
                Console.WriteLine("if you need to install it. The fix works for all affected versions, so it only");
                Console.Write("needs to be installed once. "); // Omit '\n' and leave space for a clean, same-line prompt.
                bool installFix = ConsoleIO.UserPromptYesNo("Install the music fix? [Y/n]: ", ConsoleIO.DefaultOption.Yes);
                if (installFix)
                {
                    ProgramData.NLogger.Debug("User wants the music fix installed...");
                    FileInfo fmodex = new FileInfo(Path.Combine(Directories.MusicFix, "fmodex.dll"));
                    FileInfo winmm = ext switch
                    {
                        MusicFileType.Mp3 => new FileInfo(Path.Combine(Directories.MusicFix, "winmm-mp3.dll")),
                        MusicFileType.Ogg => new FileInfo(Path.Combine(Directories.MusicFix, "winmm-ogg.dll")),
                        _ => throw new ArgumentException()
                    };
                    try
                    {
                        fmodex.CopyTo(Path.Combine(Directories.Game, "fmodex.dll"), overwrite: true);
                        winmm.CopyTo(Path.Combine(Directories.Game, "winmm.dll"), overwrite: true);
                    }
                    catch (Exception e)
                    {
                        ProgramManager.GiveErrorMessageAndExit("Failed to copy files!", e, 3);
                    }
                    correctMusicFixIsInstalled = true;
                    ProgramData.NLogger.Info("Installed music fix successfully.");
                    ConsoleIO.PrintHeader("Music fix successfully installed!", foregroundColor: ConsoleColor.Green);
                }
                else
                {
                    ProgramData.NLogger.Debug("User declined the music fix installation.");
                    ConsoleIO.PrintHeader("Skipping music fix.", "I'll ask again next time.", ConsoleColor.White);
                }
            }

            if (!correctMusicFixIsInstalled) 
                return;

            if (IsFullscreenBorderFixInstalled(out RegistryKey compatibilityPatchKey))
                HandleFullscreenBorderFix(compatibilityPatchKey);
            else
                ProgramData.NLogger.Debug("Fullscreen Border Fix compatibility patch not found. Music fix should work fine.");
        }

        private MusicFileType DetermineMusicFileType()
        {
            var dir = new DirectoryInfo(Path.Combine(Directories.Game, "music"));
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                ProgramData.NLogger.Debug($"Checking {file}");
                if (file.Extension.ToLower() == ".mp3")
                    return MusicFileType.Mp3;
                else if(file.Extension.ToLower() == ".ogg")
                    return MusicFileType.Ogg;
            }
            return MusicFileType.OtherOrUnknown;
        }

        /// <summary>
        ///     Checks if the music fix is installed in the game directory.
        /// </summary>
        /// <returns>
        ///     <see langword="true"/> if music fix is installed, <see langword="false"/> otherwise
        /// </returns>
        private bool IsCorrectMusicFixInstalled(MusicFileType ext)
        {
            string firstMissingFile = FileIO.FindMissingFile(InstalledMusicFixFiles, Directories.Game);
            if (!string.IsNullOrEmpty(firstMissingFile))
                return false;

            var hash = FileIO.ComputeMd5Hash(Path.Combine(Directories.Game, "winmm.dll"));
            return ext switch
            {
                MusicFileType.Mp3 => hash == TR2FileAudit.MusicFilesAudit["winmm-mp3.dll"],
                MusicFileType.Ogg => hash == TR2FileAudit.MusicFilesAudit["winmm-ogg.dll"],
                _ => throw new ArgumentException(),
            };
        }

        /// <summary>
        ///     Checks the registry to see if "Tomb Raider series fullscreen border fix" is installed.
        /// </summary>
        /// <param name="compatibilityPatchKey"><see cref="RegistryKey"/> of the installation if it exists, otherwise <see langword="null"/></param>
        /// <returns>
        ///     <see langword="true"/> if the fix is installed, <see langword="false"/> otherwise
        /// </returns>
        private static bool IsFullscreenBorderFixInstalled(out RegistryKey compatibilityPatchKey)
        {
            const string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using RegistryKey installedProgramsKey = Registry.LocalMachine.OpenSubKey(registryKey);
            if (installedProgramsKey != null)
            {
                // Since it is a Windows Compatibility Solution Database file installed, the key we want ends with `.sdb`.
                IEnumerable<string> installedCompatibilitySolutionDatabases = installedProgramsKey.GetSubKeyNames().Where(name => name.EndsWith(".sdb"));
                foreach (string database in installedCompatibilitySolutionDatabases)
                {
                    RegistryKey patchKey = installedProgramsKey.OpenSubKey(database);
                    if (patchKey is null)
                        continue;

                    // Here, an `.sdb` key has two values inside: `DisplayName` and `UninstallString`.
                    string displayName = (string)patchKey.GetValue("DisplayName");
                    if (displayName != FullscreenBorderFixName)
                        continue;

                    compatibilityPatchKey = patchKey;
                    return true;
                }
            }

            compatibilityPatchKey = null;
            return false;
        }

        /// <summary>
        ///     Asks the user if they want to uninstall the fix, then acts appropriately.
        /// </summary>
        /// <param name="compatibilityPatchKey"><see cref="RegistryKey"/> of the fix</param>
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private void HandleFullscreenBorderFix(RegistryKey compatibilityPatchKey)
        {
            ProgramData.NLogger.Debug($"{FullscreenBorderFixName} is installed. Alerting the user that it breaks the music fix.");
            Console.WriteLine($"You currently have a program called \"{FullscreenBorderFixName}\"");
            Console.WriteLine("installed on your PC. It is a compatibility patch which breaks the music");
            Console.WriteLine("fix by blocking required DLLs from loading.");
            Console.WriteLine("The problem can be fixed by uninstalling the compatibility patch (recommended).");
            Console.WriteLine("Please note that you are not required to uninstall the program. Any time you");
            Console.WriteLine("run this program and you have the music fix installed, I will check for this");
            Console.WriteLine("compatibility patch and ask again if you want to uninstall it.");
            bool uninstallPatch = ConsoleIO.UserPromptYesNo($"Allow me to uninstall \"{FullscreenBorderFixName}\"? [Y/n]: ", ConsoleIO.DefaultOption.Yes);
            if (uninstallPatch)
            {
                ProgramData.NLogger.Debug("User wants the fullscreen border patch uninstalled...");
                try
                {
                    var uninstallProcess = new Process()
                    {
                        StartInfo =
                        {
                            FileName = "cmd.exe",
                            Arguments = "/c " + compatibilityPatchKey.GetValue("UninstallString"),
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        }
                    };

                    ProgramData.NLogger.Debug($"Attempting to run `{uninstallProcess.StartInfo.FileName}` with arguments `{uninstallProcess.StartInfo.Arguments}`.");
                    uninstallProcess.Start();
                    uninstallProcess.WaitForExit();

                    // If uninstallation was successful, we should not be able to re-open the key.
                    string compatibilityPatchKeyName = compatibilityPatchKey.Name.Replace(@"HKEY_LOCAL_MACHINE\", string.Empty);
                    compatibilityPatchKey.Close();
                    if (Registry.LocalMachine.OpenSubKey(compatibilityPatchKeyName) is null)
                    {
                        const string successMessage = "Fullscreen border patch successfully uninstalled!";
                        ProgramData.NLogger.Debug(successMessage);
                        ConsoleIO.PrintHeader(successMessage, foregroundColor: ConsoleColor.Green);
                    }
                    else
                    {
                        throw new ApplicationException("sdbinst failed to uninstall the patch.");
                    }
                }
                catch (Exception e)
                {
                    const string failureMessage = "Fullscreen border patch uninstallation failed!";
                    ProgramData.NLogger.Error(e, failureMessage);
                    ConsoleIO.PrintHeader(failureMessage, "You'll have to uninstall it yourself, sorry!", ConsoleColor.Red);
                    Console.WriteLine("You can do this from \"Apps & Features\" like any other program.");
                }
            }
            else
            {
                ProgramData.NLogger.Debug("User declined the fullscreen border patch installation.");
                ConsoleIO.PrintHeader("Skipping fullscreen border patch uninstallation.", "I'll ask again next time.", ConsoleColor.White);
            }

        }
    }
}