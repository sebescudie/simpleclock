using System;
using System.Linq;
using System.Xml.Linq;
using Nuke.Common;
using Serilog;
using System.IO;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.PathConstruction;
using _build;

class Build : NukeBuild
{
    // ---------------------------------------------------------------- Parameters
    [Parameter("Compiler Path")]
    readonly string CompilerPath;

    [Parameter("Chocolatey API Key")]
    [Secret]
    readonly string ApiKey;

    [Parameter("Chocolatey Feed URL")]
    readonly string Feed;

    // ---------------------------------------------------------------- Paths and magic strings

    readonly string winX64Rid = "win-x64";
    readonly string packId = "simpleclock";
    readonly string repoUrl = "https://github.com/sebescudie/simpleclock";

    string Version = String.Empty;

    AbsolutePath VersionFilePath = RootDirectory / "version.props";
    AbsolutePath VvvvSourcePath = RootDirectory / "simpleclock.vl";
    AbsolutePath VvvvOutputDir = RootDirectory / $"output/simpleclock";
    AbsolutePath ReleasesFolder = RootDirectory / "releases";

    // ---------------------------------------------------------------- Secrets

    string githubToken = Environment.GetEnvironmentVariable("SIMPLECLOCK_GITHUB_TOKEN", EnvironmentVariableTarget.User);

    public static int Main() => Execute<Build>(x => x.Compile);

    Target Clean => _ => _
        .Before(RetrieveVersion)
        .Executes(() =>
        {
            Log.Information("Ensure vvvv output dir and Velopack releases dir are empty");
            // Clear vvvv output folder
            Utils.EnsureCleanDirectory(VvvvOutputDir);

            // Clear Velopack reealases directory
            Utils.EnsureCleanDirectory(ReleasesFolder);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            Console.WriteLine("Nothing to restore");
        });

    Target RetrieveVersion => _ => _
        .Executes(() =>
        {
            try
            {
                Version = XDocument.Load(VersionFilePath).Descendants("Version").FirstOrDefault()?.Value ?? "0.0.0";
                Log.Information($"Found version {Version}");

            }
            catch
            {
                Log.Error("Could not read version file");
                throw;
            }
        });

    // Compiles the vvvv app
    Target Compile => _ => _
        .DependsOn(RetrieveVersion)
        .Executes(() =>
        {
            Log.Information("Compiling vvvv app");
            var build = ProcessTasks.StartProcess(CompilerPath, $"{VvvvSourcePath} --output-type WinExe --rid {winX64Rid}");
            build.WaitForExit();
        });

    // Generates a Velopack release
    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            // Download the previous release
            Log.Information("Downloading previous release");
            var downloadPreviousRelease = ProcessTasks.StartProcess("vpk", $"download github --repoUrl {repoUrl} --outputDir {ReleasesFolder} --token {githubToken}");
            downloadPreviousRelease.WaitForExit();

            // Pack the current one
            Log.Information("Packing new release");
            var pack = ProcessTasks.StartProcess("vpk", $"pack --packId {packId} --packTitle simpleclock --packVersion {Version} --packDir {VvvvOutputDir} --outputDir {ReleasesFolder} --mainExe simpleclock.exe --framework net8.0-x64-desktop");
            pack.AssertWaitForExit();
        });

    // Distributes the release
    Target Distribute => _ => _
    .DependsOn(Pack)
    .Executes(() =>
    {
        Log.Information("Uploading new release");
        var uploadRealease = ProcessTasks.StartProcess("vpk", $"upload github --repoUrl {repoUrl} --token {githubToken} --tag {Version}");
        uploadRealease.WaitForExit();
    });

}
