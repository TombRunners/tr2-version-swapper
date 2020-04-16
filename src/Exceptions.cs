using System;

namespace TR2_Version_Swapper
{
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
