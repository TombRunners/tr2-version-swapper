namespace TR2_Version_Swapper
{
    /// <summary>
    ///     Contains miscellaneous constants the program might be u
    /// </summary>
    internal static class Misc
    {
        public static readonly string[] AsciiArt =
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

        public const string RepoLink = "https://github.com/TombRunners/tr2-version-swapper/";

        public const string ReleaseLink = "https://github.com/TombRunners/tr2-version-swapper/releases/latest";

        public static readonly string[] DefaultSettingsFile =
        {
            @"{",
            @"  // The number of log files the program will allow before deleting the oldest one(s).",
            @"  // Set to 0 to allow infinite log file generation.",
            @"  // Default: 15",
            @"  ""LogFileLimit"": 15",
            @"}", 
        };
    }
}