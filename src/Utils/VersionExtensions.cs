using System;

namespace TR2_Version_Swapper.Utils
{
    public static class VersionExtensions
    {
        /// <summary>
        ///     Provides an alternative to the built-in Version compare, which sometimes yields undesirable
        ///     results when comparing a version with no revision number to a version with a revision number, etc.
        ///     With this, you specify up-front how many of the version's numbers to compare.
        /// </summary>
        /// <returns>-1 if `this` is less, 0 if equal, 1 if `this` is greater</returns>
        /// <remarks>
        ///     Credits: https://stackoverflow.com/a/28695949/10466817
        /// </remarks>
        public static int CompareTo(this Version version, Version otherVersion, int significantParts)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));
            if (otherVersion == null)
                return 1;

            if (version.Major != otherVersion.Major && significantParts >= 1)
            {
                if (version.Major > otherVersion.Major)
                    return 1;
                return -1;
            }

            if (version.Minor != otherVersion.Minor && significantParts >= 2)
            {
                if (version.Minor > otherVersion.Minor)
                    return 1;
                return -1;
            }

            if (version.Build != otherVersion.Build && significantParts >= 3)
            {
                if (version.Build > otherVersion.Build)
                    return 1;
                return -1;
            }

            if (version.Revision != otherVersion.Revision && significantParts >= 4)
            {
                if (version.Revision > otherVersion.Revision)
                    return 1;
                return -1;
            }

            return 0;
        }
    }
}