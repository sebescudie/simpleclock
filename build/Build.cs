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
    
    string Version = String.Empty;

    AbsolutePath VersionFilePath = RootDirectory / "version.props";
    AbsolutePath VvvvSourcePath = RootDirectory / "simpleclock.vl";
    AbsolutePath VvvvOutputDir = RootDirectory / $"output/simpleclock";
    AbsolutePath ReleasesFolder = RootDirectory / "releases";
    
    public static int Main () => Execute<Build>(x => x.Compile);

    Target Clean => _ => _
        .Before(RetrieveVersion)
        .Executes(() =>
        {
            
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

    Target Compile => _ => _
        .DependsOn(RetrieveVersion)
        .Executes(() =>
        {
            var build = ProcessTasks.StartProcess(CompilerPath, $"{VvvvSourcePath} --output-type WinExe --rid {winX64Rid}");
            build.WaitForExit();
        });

    Target Velopack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var velopack = ProcessTasks.StartProcess("vpk", $"pack --packId {packId} --packTitle simpleclock --packVersion {Version} --packDir {VvvvOutputDir} --outputDir {ReleasesFolder} --mainExe simpleclock.exe --framework net8.0-x64-desktop");
            velopack.AssertWaitForExit();
        });
    


}
