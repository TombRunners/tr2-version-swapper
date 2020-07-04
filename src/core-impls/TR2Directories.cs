using System.IO;

using TRVS.Core;

namespace TR2_Version_Swapper
{
    /// <inheritdoc cref="IDirectories"/>
    internal class TR2Directories : IDirectories
    {
        /// <summary>
        ///     Folder containing each packaged version.
        /// </summary>
        public readonly string Versions;

        /// <summary>
        ///     Music fix utility's folder.
        /// </summary>
        public readonly string MusicFix;

        /// <summary>
        ///     Patch 1 utility's folder.
        /// </summary>
        public readonly string Patch;

        /// <inheritdoc/>
        public string Game { get; }

        internal TR2Directories()
        {
            string root = Path.GetFullPath(Directory.GetCurrentDirectory());
            Game = Directory.GetParent(root).FullName;
            Versions = Path.Combine(root, "versions");
            MusicFix = Path.Combine(root, "utilities/music_fix");
            Patch = Path.Combine(root, "utilities/patch");
        }
    }
}
