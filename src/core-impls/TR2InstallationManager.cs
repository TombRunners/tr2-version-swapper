using System.Diagnostics.CodeAnalysis;
using TRVS.Core;

namespace TR2_Version_Swapper
{
    /// <inheritdoc/>
    // ReSharper doesn't detect the Activator.CreateInstance{T} usage
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    internal class TR2InstallationManager : InstallationManagerBase<TR2Directories, TR2FileAudit>
    { 
        protected override TRVSProgramData ProgramData { get; }
        protected override TRVSProgramManager ProgramManager { get; }
        protected override TR2FileAudit FileAudit { get; }
        protected override TR2Directories Directories { get; }

        public TR2InstallationManager(TRVSProgramData programData, TRVSProgramManager programManager, TR2FileAudit fileAudit, TR2Directories directories)
        {
            ProgramData = programData;
            ProgramManager = programManager;
            FileAudit = fileAudit;
            Directories = directories;
        }
        
        /// <inheritdoc/>
        protected override void ValidatePackagedFiles()
        {
            // Check that each version's game files are present and unmodified.
            ValidateMd5Hashes(TR2FileAudit.VersionFilesAudit, Directories.Versions);
            // Check that the utility files are present and unmodified.
            ValidateMd5Hashes(TR2FileAudit.MusicFilesAudit, Directories.MusicFix);
            ValidateMd5Hashes(TR2FileAudit.PatchFilesAudit, Directories.Patch);
        }
    }
}
