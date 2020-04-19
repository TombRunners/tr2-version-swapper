namespace TR2_Version_Swapper
{
    internal static class FileAudit
    {
        internal static readonly string[] GameFiles =
{
            "tomb2.exe",
            "data/floating.tr2",
            "data/title.pcx",
            "data/tombpc.dat"
        };

        internal static readonly string[] VersionNames =
        {
            "Multipatch",
            "Eidos Premier Collection",
            "Eidos UK Box"
        };

        internal static readonly string[] VersionFiles =
        {
            "Multipatch/tomb2.exe",
            "Multipatch/data/floating.tr2",
            "Multipatch/data/title.pcx",
            "Multipatch/data/tombpc.dat",
            "Eidos Premier Collection/tomb2.exe",
            "Eidos Premier Collection/data/floating.tr2",
            "Eidos Premier Collection/data/title.pcx",
            "Eidos Premier Collection/data/tombpc.dat",
            "Eidos UK Box/tomb2.exe",
            "Eidos UK Box/data/floating.tr2",
            "Eidos UK Box/data/title.pcx",
            "Eidos UK Box/data/tombpc.dat"
        };

        internal static readonly string[] VersionFileHashes =
        {
            // Multipatch
            "964f0c4e08ff44a905e8fc9a78f605dc", // tomb2.exe
            "1e7d0d88ff9d569e22982af761bb006b", // data/floating.tr2
            "a5dad5ff5cb275825ff1895ca76fa908", // data/title.pcx
            "d48757da01f8642f1a3d82fae0fc99e4", // data/tombpc.dat
            // EPC
            "793c67c79a50984d9bd17ad391f03c57", // tomb2.exe (With No-CD crack)
            "1e7d0d88ff9d569e22982af761bb006b", // data/floating.tr2
            "cdf5c232f71fe1d45b184c45252b6fb0", // data/title.pcx
            "d48757da01f8642f1a3d82fae0fc99e4", // data/tombpc.dat
            // UKB
            "12d56521ce038b55efba97463357a3d7", // tomb2.exe (With No-CD crack)
            "b8fc5d8444b15527cec447bc0387c41a", // data/floating.tr2
            "cdf5c232f71fe1d45b184c45252b6fb0", // data/title.pcx
            "d48757da01f8642f1a3d82fae0fc99e4"  // data/tombpc.dat
        };

        internal static readonly string[] MusicFiles =
        {
            "fmodex.dll",
            "winmm.dll",
            "music/01.wma",
            "music/02.wma",
            "music/03.wma",
            "music/04.wma",
            "music/05.wma",
            "music/06.wma",
            "music/07.wma",
            "music/08.wma",
            "music/09.wma",
            "music/10.wma",
            "music/11.wma",
            "music/12.wma",
            "music/13.wma",
            "music/14.wma",
            "music/15.wma",
            "music/16.wma",
            "music/17.wma",
            "music/18.wma",
            "music/19.wma",
            "music/20.wma",
            "music/21.wma",
            "music/22.wma",
            "music/23.wma",
            "music/24.wma",
            "music/25.wma",
            "music/26.wma",
            "music/27.wma",
            "music/28.wma",
            "music/29.wma",
            "music/30.wma",
            "music/31.wma",
            "music/32.wma",
            "music/33.wma",
            "music/34.wma",
            "music/35.wma",
            "music/36.wma",
            "music/37.wma",
            "music/38.wma",
            "music/39.wma",
            "music/40.wma",
            "music/41.wma",
            "music/42.wma",
            "music/43.wma",
            "music/44.wma",
            "music/45.wma",
            "music/46.wma",
            "music/47.wma",
            "music/48.wma",
            "music/49.wma",
            "music/50.wma",
            "music/51.wma",
            "music/52.wma",
            "music/53.wma",
            "music/54.wma",
            "music/55.wma",
            "music/56.wma",
            "music/57.wma",
            "music/58.wma",
            "music/59.wma",
            "music/60.wma",
            "music/61.wma"
        };

        internal static readonly string[] MusicFileHashes =
        {
            "a5106cf9d7371f842f500976692dd29e", // fmodex.dll
            "f683a8f1a309798ff75d11d65092315a", // winmm.dll
            "590e72b218f5e8b48034e3541fe97c6d", // music/01.wma
            "fed38f566071be693513e36c0d15f170", // music/02.wma
            "2cfe7fc25d8390b7cc2788acd2733935", // music/03.wma
            "07f25bd38711809eca329859d3ece473", // music/04.wma
            "0c86fe58a90120394de561f5ef67ae62", // music/05.wma
            "88c88f1b7a8300aef1cd7971d9fd5396", // music/06.wma
            "c30ccab7bee446de0bbef9d28b94cb73", // music/07.wma
            "f76c275ca2e77907f33cd9d63bfef5c3", // music/08.wma
            "b19911abe212db1c7d1787fb7f6c98dd", // music/09.wma
            "c00f87741608ce8c92f0a053ca047cc4", // music/10.wma
            "e13422953a580a18c4d94159dd5a9282", // music/11.wma
            "b9c67962733f2917114cb5cb3cd92097", // music/12.wma
            "38679d0da6201cb29c8124014d93a369", // music/13.wma
            "11bf3b9fb0a9f4cf779a16d4299bbf2f", // music/14.wma
            "00d5b4fadcf1ea7c3456ef2e9d3ff3d3", // music/15.wma
            "c01b434f7a610fd7a0fd0adf2696202f", // music/16.wma
            "d07d4278616285168395df2e05540296", // music/17.wma
            "f07d25bb1b50759f0b1065ab6ebb8a59", // music/18.wma
            "181a79084797441042322df1520fc6c0", // music/19.wma
            "66ebcee8a802a81c4d300b17b5357984", // music/20.wma
            "90100947566f9619e654951cae5b3e58", // music/21.wma
            "9b094269a4a0c2b728ef4baa93bb0372", // music/22.wma
            "878d095cd491655e81da16cb5b2ff774", // music/23.wma
            "5feb203ad19debb568f643545ad25455", // music/24.wma
            "636bf312aa7ed095dc489e93210f7382", // music/25.wma
            "53ce29c5f56e5f43ffed1be875ee851e", // music/26.wma
            "f426977bfd9df13b08e1f12082e87f43", // music/27.wma
            "70bdc8fb7af407f97d07a5e6f8f5f54c", // music/28.wma
            "917022ad714b1a4a2867090d53f3bf4a", // music/29.wma
            "5df7a28cb734489c967a2158d800766f", // music/30.wma
            "b717746ac2c6ed7039630181fd05f9fd", // music/31.wma
            "4768ae196c2d6caa47cd954a1c0939b1", // music/32.wma
            "19adf60895768114cf5500dec64bb762", // music/33.wma
            "c5476d65cac1f9a29d5b75158817da09", // music/34.wma
            "b379eb720b7345502d10147f8ccd1de9", // music/35.wma
            "cef39b8db6ecfdfce890f7bd076dd6b0", // music/36.wma
            "69ba4ffdc99a8619b78f5d287c0c46a3", // music/37.wma
            "dce3c21a53de5c4f96c89d5fad100900", // music/38.wma
            "a4c261f6cc5f1c7e467ac8a0d5c3dfc0", // music/39.wma
            "707f525c291579bf349ef51315eed037", // music/40.wma
            "a5b3a83adf8795d9fe8201428da4a273", // music/41.wma
            "b6c52dd302e465e0a43c1d63da3acf55", // music/42.wma
            "a6539ee3d324d4e46045a20fc0ca12a0", // music/43.wma
            "3d3a651eb10d0c0a209af5ab518a6c2e", // music/44.wma
            "31c855f32da3b383ca2c8e8f4e12239c", // music/45.wma
            "d5df518485aae2665dffde896f43b301", // music/46.wma
            "d340d61f5834048ce276937c5a536d64", // music/47.wma
            "a7adf895643b9c95408c29f1d780bb90", // music/48.wma
            "918d36e8aab1cc03aad7664fd9d08c7a", // music/49.wma
            "82c24fcdee7757a404f36663c7770d26", // music/50.wma
            "2a15e167c318aa480276b123d6556e1e", // music/51.wma
            "7f08f59f8671f0f32ed58e71eb377ac8", // music/52.wma
            "081886e5fd3fb11552c6f730a99051cf", // music/53.wma
            "5782adfe3f2bf28fc32d8bd318f60398", // music/54.wma
            "c74435d8f79df88cb2ceffde9d2af169", // music/55.wma
            "bbf328f0a3995f5fd6fed7ebba66c10d", // music/56.wma
            "ad7b069e9ab62bdc5d431125666ba182", // music/57.wma
            "8ab107201a117caf1da9b572d0eb5836", // music/58.wma
            "b4af2d28449131ff6ba2f96f1c802958", // music/59.wma
            "b801e9250f7e73526ee08024063c8727", // music/60.wma
            "590e72b218f5e8b48034e3541fe97c6d"  // music/61.wma
        };

        internal static readonly string[] PatchFiles =
        {
            "tr2p1readme.rtf",
            "tomb2.exe"
        };

        internal static readonly string[] PatchFileHashes =
        {
            "100439b46ecad0a318d757bb814ae890", // tr2p1readme.rtf
            "39cab6b4ae3c761b67ae308a0ab22e44"  // tomb2.exe (With No-CD crack)
        };
    }
}