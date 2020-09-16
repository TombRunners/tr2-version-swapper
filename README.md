# TR2 Version Swapper

### Table of Contents
* [Overview](#overview)
  * [Supported Versions](#supported-versions)
  * [Included Utilities](#included-utilities)
* [Report Issues / Get Help](#report-issues-get-help)
* [Development](#development)

### Overview
This solution compiles a program that automates swapping and patching Tomb Raider II PC game versions.

The program does the following:
* Checks its surroundings before proceeding, giving guidance if things seem wrong
* Prompts you to select a version to switch to, then performs copy operations to switch to it
* Prompts to give you the options to use the included utilities, then acts accordingly

> If you can successfully copy-paste all required files to and from the correct folders when switching versions and applying the utilities, 
> then you don't *need* the executable, but it will save clicks/keystrokes/time.

Releases contain installation and usage instructions.

#### Supported Versions
 * Multipatch
 * Eidos Premier Collection [EPC]
 * Eidos UK Box [UKB]

#### Included Utilities
 * Patch 1
 * Music Fix
 * Fullscreen Border Fix Uninstaller

### Report Issues / Get Help
For program bugs or feature requests, use the [issues](https://github.com/TombRunners/tr2-version-swapper/issues) page.
For general help or questions, join the [Tomb Runner Discord](https://discord.gg/011hZixyZfK5g61NL) and ask in the appropriate channel.

### Development
In order to build or develop this project, you must use [TRVS Core's](https://github.com/MidgeOnGithub/trvs-core) NuGet package.
Its package (and symbols) are hosted [here](https://www.nuget.org/packages/TRVS.Core/), and can easily be obtained with Visual Studio's built-on NuGet package manager.