using TRVS.Core;

namespace TR2_Version_Swapper
{
    internal class TR2Program
        : ProgramBase<TR2Directories, TR2FileAudit, TR2InstallationManager, TR2VersionSwapper>
    {
        protected override TRVSProgramData ProgramData => new TRVSProgramData
        {
            GameAbbreviation = "TR2",
            GameExe = "tomb2",
            NLogger = NLog.LogManager.GetCurrentClassLogger(),
            MiscInfo = new TR2MiscInfo(),
            Settings = new TRVSUserSettings(),
            Version = typeof(Program).Assembly.GetName().Version
        };

        protected override TR2Directories Directories => new TR2Directories();

        protected override TR2FileAudit FileAudit => new TR2FileAudit();
    }
}
