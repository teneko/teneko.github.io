using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using Nuke.Common.Git;
using Teronis.Diagnostics;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.ProjectModel.ProjectModelTasks;

partial class Build : NukeBuild
{
    const string GitExecutableName = "git";
    const string DotNetExecutableName = "dotnet";
    const string DocFxArguments = "";

    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    readonly GitRepository TeronisDotNetRepository;

    AbsolutePath RootObjectDirectory => RootDirectory / "obj";
    AbsolutePath TeronisDotNetDirectory => RootObjectDirectory / TeronisDotNetRepository.Identifier;
    AbsolutePath TeronisDotNetSolutionFile => TeronisDotNetDirectory / "Teronis.DotNet~Publish.sln";

    readonly Lazy<Solution> LazyTeronisDotNetSolution;

    public Build() {
        TeronisDotNetRepository = GitRepository.FromUrl("https://github.com/teroneko/Teronis.DotNet.git");
        LazyTeronisDotNetSolution = new Lazy<Solution>(() => ParseSolution(TeronisDotNetSolutionFile));
    }

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DeleteDirectory(RootObjectDirectory);
        });

    Target Restore => _ => _
        .Before(Compile)
        .Executes(() =>
        {
            DotNetRestore();
            RunSimpleProcess(GitExecutableName, $"clone --depth 1 {TeronisDotNetRepository.HttpsUrl} {TeronisDotNetDirectory}", echoCommand: true);
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            LazyTeronisDotNetSolution.Value.AllProjects.ForEach(project => {
                //RunDocFx(
            });
        });

}
