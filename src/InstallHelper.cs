using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

using NLog;
using TR2_Version_Swapper.Utils;

namespace TR2_Version_Swapper
{
    public class InstallHelper
    {
        private readonly string _rootDir;

        private readonly string _gameDir;

        private readonly Logger NLogger;

        public InstallHelper(Logger l)
        {
            _rootDir = Path.GetFullPath(Directory.GetCurrentDirectory());
            _gameDir = Directory.GetParent(_rootDir).FullName;
            NLogger = l;
        }

        /// <summary>
        ///     Ensures that all required version swapping files are present and that the program is placed in a TR2 installation.
        /// </summary>
        public void ValidateInstallation()
        {
            ValidatePackagedFiles(_rootDir);
            CheckDirectoryLooksLikeGame(_gameDir);
        }

        private static void ValidatePackagedFiles(string dir)
        {
            // Check that each version's game files are present.
            string versionsDir = Path.Combine(dir, "versions");
            ValidateMd5Hashes(FileAudit.VersionFiles, FileAudit.VersionFileHashes, versionsDir);

            // Check that the utility files are present.
            string utilitiesDir = Path.Combine(dir, "utilities");
            ValidateMd5Hashes(FileAudit.MusicFiles, FileAudit.MusicFileHashes, utilitiesDir);
            ValidateMd5Hashes(FileAudit.PatchFiles, FileAudit.PatchFileHashes, utilitiesDir);
        }

        private static void CheckDirectoryLooksLikeGame(string gameDir)
        {
            try
            {
                AllFilesInDirectory(FileAudit.GameFiles, gameDir);
            }
            catch (RequiredFileMissingException e)
            {
                throw new BadInstallationLocationException("Parent folder does not look like a TR2 game installation.", e);
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
                string fileHash = ComputeMd5Hash(path);
                if (requiredHash != fileHash)
                    throw new InvalidGameFileException(
                        $"File {path} was modified.\nGot {fileHash}, expected {requiredHash}"
                    );
            }
        }

        private static string ComputeMd5Hash(string file)
        {
            FileStream fs = null;
            byte[] hash = null;
            try
            {
                fs = new FileStream(file, FileMode.Open);
                using var md5 = MD5.Create();
                hash = md5.ComputeHash(fs);
            }
            catch (IOException e)
            {
                if (e is DirectoryNotFoundException || e is FileNotFoundException)
                    throw new RequiredFileMissingException($"File \"{file}\" not found!", e);
            }
            finally
            {
                fs?.Close();
            }

            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        ///     Finds a TR2 process running form the targeted game directory if it exists.
        /// </summary>
        /// <returns>The running Process or null.</returns>
        public Process FindTr2RunningFromGameDir()
        {
            
            Process[] processes = Process.GetProcesses();
            return processes.FirstOrDefault(p => 
                p.ProcessName.ToLower() == "tomb2" &&
                p.MainModule != null &&
                Directory.GetParent(p.MainModule.FileName).FullName == Path.GetFullPath(_gameDir)
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
            char c = Console.ReadKey().KeyChar;
            Console.WriteLine();
            if (c == 'y' || c == 'Y')
            {
                NLogger.Debug($"Interpreted {c} as user allowing program to kill the running TR2 task.");
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
                NLogger.Debug($"Interpreted {c} as user opting to kill the running TR2 task on their own.");
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

        public bool GameDirHasMusicFixInstalled()
        {
            try
            {
                AllFilesInDirectory(FileAudit.MusicFiles, _gameDir);
            }
            catch (RequiredFileMissingException)
            {
                return false;
            }
            return true;
        }
    }
}