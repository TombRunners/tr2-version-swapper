using System.Collections.Generic;

using TRVS.Core;

namespace TR2_Version_Swapper
{
    /// <inheritdoc/>
    internal class TR2MiscInfo : MiscInfoBase
    {
        /// <inheritdoc/>
        public override IEnumerable<string> AsciiArt => new[]
        {
            @"    _______ _____  ___                     ",
            @"   |__   __|  __ \|__ \                    ",
            @"      | |  | |__) |  ) |                   ",
            @"      | |  |  _  /  / /                    ",
            @"      | |  | | \ \ / /_                    ",
            @"__    |_|_ |_|  \_\____|                   ",
            @"\ \    / /          (_)                    ",
            @" \ \  / /__ _ __ ___ _  ___  _ __          ",
            @"  \ \/ / _ \ '__/ __| |/ _ \| '_ \         ",
            @"   \  /  __/ |  \__ \ | (_) | | | |        ",
            @"  __\/_\___|_|  |___/_|\___/|_| |_|        ",
            @" / ____|                                   ",
            @"| (_____      ____ _ _ __  _ __   ___ _ __ ",
            @" \___ \ \ /\ / / _` | '_ \| '_ \ / _ \ '__|",
            @" ____) \ V  V / (_| | |_) | |_) |  __/ |   ",
            @"|_____/ \_/\_/ \__,_| .__/| .__/ \___|_|   ",
            @"                    | |   | |              ",
            @"                    |_|   |_|              "
        };

        /// <inheritdoc/>
        public override string RepoLink => "https://github.com/TombRunners/tr2-version-swapper/";

        /// <inheritdoc/>
        public override string LatestReleaseLink => "https://github.com/TombRunners/tr2-version-swapper/releases/latest";
    }
}
