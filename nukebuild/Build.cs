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

    public static int Main() => Execute<Build>(x => x.Generate);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    readonly GitRepository TeronisDotNetRepository;

    AbsolutePath DocsDirectory => RootDirectory / "docs";

    //AbsolutePath ObjDirectory => RootDirectory / "obj";
    AbsolutePath DocsMetaDirectory => RootDirectory / "docs-meta";

    AbsolutePath DocsMetaObjDirectory => DocsMetaDirectory / "obj";
    AbsolutePath TeronisDotNetGitHubCloneDirectory => DocsMetaObjDirectory / (TeronisDotNetRepository.Identifier.Split("/").Last() + "~GitHubClone");
    AbsolutePath TeronisDotNetGitHubCloneSolutionFile => TeronisDotNetGitHubCloneDirectory / "Teronis.DotNet~Publish.sln";

    [PackageExecutable("docfx.console", "docfx.exe")]
    readonly Tool DocFx;

    readonly Lazy<Solution> LazyTeronisDotNetSolution;

    public Build()
    {
        TeronisDotNetRepository = GitRepository.FromUrl("https://github.com/teroneko/Teronis.DotNet.git");
        LazyTeronisDotNetSolution = new Lazy<Solution>(() => ParseSolution(TeronisDotNetGitHubCloneSolutionFile));
    }

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DeleteDirectory(DocsMetaObjDirectory);
            DeleteDirectory(DocsDirectory);
        });

    Target Restore => _ => _
        .Before(GenerateMetadata)
        .Before(GenerateBuild)
        .Executes(() =>
        {
            DotNetRestore();
            RunSimpleProcess(GitExecutableName, $"clone --depth 1 {TeronisDotNetRepository.HttpsUrl} {TeronisDotNetGitHubCloneDirectory}", echoCommand: true);
        });

    Target GenerateMetadata => _ => _
        .DependsOn(Restore)
        .Before(GenerateBuild)
        .Executes(() =>
        {
            var metadataArgs = new Arguments()
                 .Add("metadata ./docfx.json");

            DocFx.Invoke(
                arguments: metadataArgs.RenderForExecution(),
                workingDirectory: DocsMetaDirectory);
        });

    Target GenerateBuild => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var buildArgs = new Arguments()
                .Add("build ./docfx.json")
                .Add("--maxParallelism 0", condition: IsLocalBuild);

            DocFx.Invoke(
                arguments: buildArgs.RenderForExecution(),
                workingDirectory: DocsMetaDirectory);
        });

    Target Generate => _ => _
        .DependsOn(Restore)
        .DependsOn(GenerateMetadata)
        .DependsOn(GenerateBuild);

}
