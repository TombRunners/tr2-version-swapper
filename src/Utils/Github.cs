using System;
using System.Threading.Tasks;
using Octokit;

namespace TR2_Version_Swapper.Utils
{
    /// <summary>
    ///     Provides interaction with Github's REST API.
    /// </summary>
    public static class Github
    {
        /// <summary>
        ///     Get the latest release information from Github using Octokit.
        /// </summary>
        /// <returns>The latest Github release's version (based on TagName).</returns>
        public static async Task<Version> GetLatestRelease()
        {
            var github = new GitHubClient(new Octokit.ProductHeaderValue("tr2-version-swapper"));
            Release latest = await github.Repository.Release.GetLatest("TombRunners", "tr2-version-swapper");
            return new Version(latest.TagName.Substring(1));
        }
    }
}