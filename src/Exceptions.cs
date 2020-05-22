// ReSharper disable UnusedMember.Global
using System;

namespace TR2_Version_Swapper
{
    /// <summary>
    ///     Thrown if the program is not located in a TR2 game installation.
    /// </summary>
    public class BadInstallationLocationException : Exception
    {
        public BadInstallationLocationException()
        {
        }

        public BadInstallationLocationException(string message)
            : base(message)
        {
        }

        public BadInstallationLocationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    ///     Thrown if a packaged game file has been tampered.
    /// </summary>
    public class InvalidGameFileException : Exception
    {
        public InvalidGameFileException()
        {
        }

        public InvalidGameFileException(string message)
            : base(message)
        {
        }

        public InvalidGameFileException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    ///     Thrown if a packaged game file is missing.
    /// </summary>
    public class RequiredFileMissingException : Exception
    {
        public RequiredFileMissingException()
        {
        }

        public RequiredFileMissingException(string message)
            : base(message)
        {
        }

        public RequiredFileMissingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
