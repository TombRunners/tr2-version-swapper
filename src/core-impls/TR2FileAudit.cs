using System.Collections.Generic;
using TRVS.Core;

namespace TR2_Version_Swapper
{
    /// <inheritdoc cref="IFileAudit"/>
    internal class TR2FileAudit : IFileAudit
    {
        /// <inheritdoc/>
        public IEnumerable<string> GameFiles {
            get
            {
                yield return "tomb2.exe";
                yield return "data/floating.tr2";
                yield return "data/title.pcx";
                yield return "data/tombpc.dat";
            }
        }

        /// <summary>
        ///     Maps packaged version files to their MD5 hashes.
        /// </summary>
        internal static readonly Dictionary<string, string> VersionFilesAudit = new Dictionary<string, string>
        {
            {"Multipatch/tomb2.exe", "964f0c4e08ff44a905e8fc9a78f605dc"},
            {"Multipatch/data/floating.tr2", "1e7d0d88ff9d569e22982af761bb006b"},
            {"Multipatch/data/title.pcx", "a5dad5ff5cb275825ff1895ca76fa908"},
            {"Multipatch/data/tombpc.dat", "d48757da01f8642f1a3d82fae0fc99e4"},
            {"Eidos Premier Collection/tomb2.exe", "793c67c79a50984d9bd17ad391f03c57"},
            {"Eidos Premier Collection/data/floating.tr2", "1e7d0d88ff9d569e22982af761bb006b"},
            {"Eidos Premier Collection/data/title.pcx", "cdf5c232f71fe1d45b184c45252b6fb0"}, 
            {"Eidos Premier Collection/data/tombpc.dat", "d48757da01f8642f1a3d82fae0fc99e4"},
            {"Eidos UK Box/tomb2.exe", "12d56521ce038b55efba97463357a3d7"},                  
            {"Eidos UK Box/data/floating.tr2", "b8fc5d8444b15527cec447bc0387c41a"},          
            {"Eidos UK Box/data/title.pcx", "cdf5c232f71fe1d45b184c45252b6fb0"},             
            {"Eidos UK Box/data/tombpc.dat", "d48757da01f8642f1a3d82fae0fc99e4"}
        };

        /// <summary>
        ///     Maps packaged music fix files to their MD5 hashes.
        /// </summary>
        internal static readonly Dictionary<string, string> MusicFilesAudit = new Dictionary<string, string>
        {
            {"fmodex.dll", "a5106cf9d7371f842f500976692dd29e"},
            {"winmm-mp3.dll", "69f37deb6bba2e7621f794c9dbaf301c"},
            {"winmm-ogg.dll", "134a5dd8d713e7898dd269b7b6aeb969"}
        };

        /// <summary>
        ///     Maps packaged Patch 1 files to their MD5 hashes.
        /// </summary>
        internal static readonly Dictionary<string, string> PatchFilesAudit = new Dictionary<string, string>
        {
            {"tr2p1readme.rtf", "100439b46ecad0a318d757bb814ae890"},
            {"tomb2.exe", "39cab6b4ae3c761b67ae308a0ab22e44"}  // With no-CD crack
        };
    }
}