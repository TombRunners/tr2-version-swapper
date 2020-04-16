using System.Collections.Generic;
using System.IO;

namespace TR2_Version_Swapper.Utils
{
    /// <summary>
    ///     Provides utility filesystem functions.
    /// </summary>
    public static class FileIo
    {
        /// <summary>
        ///     Recursively copies from source to destination, overwriting if applicable.
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        public static void CopyRecursively(string sourceDirName, string destDirName)
        {
            // Get the directory's files and subdirectories.
            var dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] subDirs = dir.GetDirectories();
            FileInfo[] files = dir.GetFiles();
            
            // Copy the files in the directory.
            foreach (FileInfo file in files)
            {
                string destPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(destPath, true);
            }

            // Recursively call this function to copy subdirectories.
            foreach (DirectoryInfo subDir in subDirs)
            {
                string destPath = Path.Combine(destDirName, subDir.Name);
                CopyRecursively(subDir.FullName, destPath);
            }
        }

        /// <summary>
        ///     Deletes the oldest log file to prevent unnecessary bloat.
        /// </summary>
        /// <returns>True if a file was deleted.</returns>
        public static bool DeleteExcessLogFiles()
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            var files = new List<string>(Directory.GetFiles(dir));
            if (files.Count > 10)
            {
                files.Sort();
                File.Delete(files[0]);
                return true;
            }
            return false;
        }
    }
}