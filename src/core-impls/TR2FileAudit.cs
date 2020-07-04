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
            {"Eidos UK Box/data/tombpc.dat", "d48757da01f8642f1a3d82fae0fc99e4"},            
        };

        /// <summary>
        ///     Maps packaged music fix files to their MD5 hashes.
        /// </summary>
        internal static readonly Dictionary<string, string> MusicFilesAudit = new Dictionary<string, string>
        {
            {"fmodex.dll", "a5106cf9d7371f842f500976692dd29e"},
            {"winmm.dll", "f683a8f1a309798ff75d11d65092315a"}, 
            {"music/01.wma", "590e72b218f5e8b48034e3541fe97c6d"},
            {"music/02.wma", "fed38f566071be693513e36c0d15f170"},
            {"music/03.wma", "2cfe7fc25d8390b7cc2788acd2733935"},
            {"music/04.wma", "07f25bd38711809eca329859d3ece473"},
            {"music/05.wma", "0c86fe58a90120394de561f5ef67ae62"},
            {"music/06.wma", "88c88f1b7a8300aef1cd7971d9fd5396"},
            {"music/07.wma", "c30ccab7bee446de0bbef9d28b94cb73"},
            {"music/08.wma", "f76c275ca2e77907f33cd9d63bfef5c3"},
            {"music/09.wma", "b19911abe212db1c7d1787fb7f6c98dd"},
            {"music/10.wma", "c00f87741608ce8c92f0a053ca047cc4"},
            {"music/11.wma", "e13422953a580a18c4d94159dd5a9282"},
            {"music/12.wma", "b9c67962733f2917114cb5cb3cd92097"},
            {"music/13.wma", "38679d0da6201cb29c8124014d93a369"},
            {"music/14.wma", "11bf3b9fb0a9f4cf779a16d4299bbf2f"},
            {"music/15.wma", "00d5b4fadcf1ea7c3456ef2e9d3ff3d3"},
            {"music/16.wma", "c01b434f7a610fd7a0fd0adf2696202f"},
            {"music/17.wma", "d07d4278616285168395df2e05540296"},
            {"music/18.wma", "f07d25bb1b50759f0b1065ab6ebb8a59"},
            {"music/19.wma", "181a79084797441042322df1520fc6c0"},
            {"music/20.wma", "66ebcee8a802a81c4d300b17b5357984"},
            {"music/21.wma", "90100947566f9619e654951cae5b3e58"},
            {"music/22.wma", "9b094269a4a0c2b728ef4baa93bb0372"},
            {"music/23.wma", "878d095cd491655e81da16cb5b2ff774"},
            {"music/24.wma", "5feb203ad19debb568f643545ad25455"},
            {"music/25.wma", "636bf312aa7ed095dc489e93210f7382"},
            {"music/26.wma", "53ce29c5f56e5f43ffed1be875ee851e"},
            {"music/27.wma", "f426977bfd9df13b08e1f12082e87f43"},
            {"music/28.wma", "70bdc8fb7af407f97d07a5e6f8f5f54c"},
            {"music/29.wma", "917022ad714b1a4a2867090d53f3bf4a"},
            {"music/30.wma", "5df7a28cb734489c967a2158d800766f"},
            {"music/31.wma", "b717746ac2c6ed7039630181fd05f9fd"},
            {"music/32.wma", "4768ae196c2d6caa47cd954a1c0939b1"},
            {"music/33.wma", "19adf60895768114cf5500dec64bb762"},
            {"music/34.wma", "c5476d65cac1f9a29d5b75158817da09"},
            {"music/35.wma", "b379eb720b7345502d10147f8ccd1de9"},
            {"music/36.wma", "cef39b8db6ecfdfce890f7bd076dd6b0"},
            {"music/37.wma", "69ba4ffdc99a8619b78f5d287c0c46a3"},
            {"music/38.wma", "dce3c21a53de5c4f96c89d5fad100900"},
            {"music/39.wma", "a4c261f6cc5f1c7e467ac8a0d5c3dfc0"},
            {"music/40.wma", "707f525c291579bf349ef51315eed037"},
            {"music/41.wma", "a5b3a83adf8795d9fe8201428da4a273"},
            {"music/42.wma", "b6c52dd302e465e0a43c1d63da3acf55"},
            {"music/43.wma", "a6539ee3d324d4e46045a20fc0ca12a0"},
            {"music/44.wma", "3d3a651eb10d0c0a209af5ab518a6c2e"},
            {"music/45.wma", "31c855f32da3b383ca2c8e8f4e12239c"},
            {"music/46.wma", "d5df518485aae2665dffde896f43b301"},
            {"music/47.wma", "d340d61f5834048ce276937c5a536d64"},
            {"music/48.wma", "a7adf895643b9c95408c29f1d780bb90"},
            {"music/49.wma", "918d36e8aab1cc03aad7664fd9d08c7a"},
            {"music/50.wma", "82c24fcdee7757a404f36663c7770d26"},
            {"music/51.wma", "2a15e167c318aa480276b123d6556e1e"},
            {"music/52.wma", "7f08f59f8671f0f32ed58e71eb377ac8"},
            {"music/53.wma", "081886e5fd3fb11552c6f730a99051cf"},
            {"music/54.wma", "5782adfe3f2bf28fc32d8bd318f60398"},
            {"music/55.wma", "c74435d8f79df88cb2ceffde9d2af169"},
            {"music/56.wma", "bbf328f0a3995f5fd6fed7ebba66c10d"},
            {"music/57.wma", "ad7b069e9ab62bdc5d431125666ba182"},
            {"music/58.wma", "8ab107201a117caf1da9b572d0eb5836"},
            {"music/59.wma", "b4af2d28449131ff6ba2f96f1c802958"},
            {"music/60.wma", "b801e9250f7e73526ee08024063c8727"},
            {"music/61.wma", "590e72b218f5e8b48034e3541fe97c6d"}
        };

        /// <summary>
        ///     Maps packaged Patch 1 files to their MD5 hashes.
        /// </summary>
        internal static readonly Dictionary<string, string> PatchFilesAudit = new Dictionary<string, string>
        {
            {"tr2p1readme.rtf", "100439b46ecad0a318d757bb814ae890"},
            {"tomb2.exe", "39cab6b4ae3c761b67ae308a0ab22e44"},  // With no-CD crack
        };
    }
}