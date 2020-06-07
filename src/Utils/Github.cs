using System;
using System.Threading.Tasks;

using Octokit;

namespace Utils
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
        public static async Task<Version> GetLatestVersion()
        {
            var github = new GitHubClient(new ProductHeaderValue("tr2-version-swapper"));
            Release latest = await github.Repository.Release.GetLatest("TombRunners", "tr2-version-swapper");
            return latest.TagName[0] == 'v'
                ? new Version(latest.TagName.Substring(1))
                : new Version(latest.TagName);
        }
    }
}