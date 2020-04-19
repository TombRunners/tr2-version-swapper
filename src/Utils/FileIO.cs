using System;
using System.IO;
using System.Security.Cryptography;

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
        ///     Computes a given file's MD5 hash.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>The file's MD5 hash.</returns>
        public static string ComputeMd5Hash(string file)
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
    }
}