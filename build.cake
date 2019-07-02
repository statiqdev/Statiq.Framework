// The following environment variables need to be set for Build target:
// SIGNTOOL (to sign the executable)

// The following environment variables need to be set for Publish target:
// STATIQ_NUGET_API_KEY
// STATIQ_GITHUB_TOKEN

// Publishing workflow:
// - Update ReleaseNotes.md and RELEASE in develop branch
// - Run a normal build with Cake to set SolutionInfo.cs in the repo and run through unit tests (`build.cmd`)
// - Push to develop and fast-forward merge to master
// - Switch to master
// - Wait for CI to complete build and publish to GitHub Package Repository
// - Run a Publish build with Cake (`build -target Publish`)
// - No need to add a version tag to the repo - added by GitHub on publish
// - Switch back to develop branch
// - Add a blog post about the release

#addin "Cake.FileHelpers"
#addin "Octokit"
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.7.0"
#tool "nuget:?package=NuGet.CommandLine&version=4.9.2"
#tool "AzurePipelines.TestLogger&version=1.0.2"

using Octokit;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var isLocal = BuildSystem.IsLocalBuild;
var isRunningOnUnix = IsRunningOnUnix();
var isRunningOnWindows = IsRunningOnWindows();
var isRunningOnBuildServer = !string.IsNullOrEmpty(EnvironmentVariable("AGENT_NAME")); // See https://github.com/cake-build/cake/issues/1684#issuecomment-397682686
var isPullRequest = !string.IsNullOrWhiteSpace(EnvironmentVariable("SYSTEM_PULLREQUEST_PULLREQUESTID"));  // See https://github.com/cake-build/cake/issues/2149
var buildNumber = TFBuild.Environment.Build.Number.Replace('.', '-');
var branch = TFBuild.Environment.Repository.Branch;

var releaseNotes = ParseReleaseNotes("./ReleaseNotes.md");

var version = releaseNotes.Version.ToString();
var semVersion = version + (isLocal ? string.Empty : string.Concat("-build-", buildNumber));

var msBuildSettings = new DotNetCoreMSBuildSettings()
    .WithProperty("Version", semVersion)
    .WithProperty("AssemblyVersion", version)
    .WithProperty("FileVersion", version);

var buildDir = Directory("./build");
var nugetRoot = buildDir + Directory("nuget");
var binDir = buildDir + Directory("bin");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    Information("Building version {0} of Statiq.", semVersion);
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectories(new DirectoryPath[] { buildDir, binDir, nugetRoot });
    });

Task("Patch-Assembly-Info")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        var file = "./SolutionInfo.cs";
        CreateAssemblyInfo(file, new AssemblyInfoSettings {
            Product = "Statiq",
            Copyright = "Copyright \xa9 Statiq Contributors",
            Version = version,
            FileVersion = version,
            InformationalVersion = semVersion
        });
    });

Task("Restore-Packages")
    .IsDependentOn("Patch-Assembly-Info")
    .Does(() =>
    {
        DotNetCoreRestore("./StatiqFramework.sln", new DotNetCoreRestoreSettings
        {
            MSBuildSettings = msBuildSettings
        });
    });

Task("Build")
    .IsDependentOn("Restore-Packages")
    .Does(() =>
    {
        DotNetCoreBuild("./StatiqFramework.sln", new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            NoRestore = true,
            MSBuildSettings = msBuildSettings
        });
    });

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .DoesForEach(GetFiles("./tests/**/*.csproj"), project =>
    {
        DotNetCoreTestSettings testSettings = new DotNetCoreTestSettings()
        {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration
        };
        if (isRunningOnBuildServer)
        {
            testSettings.Filter = "TestCategory!=ExcludeFromBuildServer";
            testSettings.Logger = "AzurePipelines";
            testSettings.TestAdapterPath = GetDirectories($"./tools/AzurePipelines.TestLogger.*/contentFiles/any/any").First();
        }

        Information($"Running tests in {project}");
        DotNetCoreTest(MakeAbsolute(project).ToString(), testSettings);
    })
    .DeferOnError();

Task("Create-Packages")
    .IsDependentOn("Build")
    .Does(() =>
    {        
        // Get the set of projects to package
        List<FilePath> projects = new List<FilePath>(GetFiles("./src/**/*.csproj"));
        
        // Package all nuspecs
        foreach (var project in projects)
        {
            DotNetCorePack(
                MakeAbsolute(project).ToString(),
                new DotNetCorePackSettings
                {
                    Configuration = configuration,
                    NoBuild = true,
                    NoRestore = true,
                    OutputDirectory = nugetRoot,
                    MSBuildSettings = msBuildSettings
                });
        }
    });

Task("Ensure-NuGet-Source")
    .WithCriteria(() => isRunningOnBuildServer)
    .Does(() =>
    {
        var githubToken = EnvironmentVariable("STATIQ_GITHUB_TOKEN");
        if (string.IsNullOrEmpty(githubToken))
        {
            throw new InvalidOperationException("Could not resolve GitHub token.");
        }

        // Add the authenticated feed source (remove any existing ones first to reset the access token)
        if(NuGetHasSource("https://nuget.pkg.github.com/statiqdev/index.json"))
        {
            NuGetRemoveSource(
                "GitHubStatiq",
                "https://nuget.pkg.github.com/statiqdev/index.json");
        }
        NuGetAddSource(
            "GitHubStatiq",
            "https://nuget.pkg.github.com/statiqdev/index.json",
            new NuGetSourcesSettings
            {
                UserName = "daveaglick",
                Password = githubToken
            });
    });
    
Task("Publish-GitHub-Packages")
    .IsDependentOn("Create-Packages")
    .IsDependentOn("Ensure-NuGet-Source")
    .WithCriteria(() => !isLocal)
    .WithCriteria(() => !isPullRequest)
    .WithCriteria(() => isRunningOnWindows)
    .WithCriteria(() => branch == "develop")
    .Does(() =>
    {
        var githubToken = EnvironmentVariable("STATIQ_GITHUB_TOKEN");
        if (string.IsNullOrEmpty(githubToken))
        {
            throw new InvalidOperationException("Could not resolve GitHub token.");
        }

        foreach (var nupkg in GetFiles(nugetRoot.Path.FullPath + "/*.nupkg"))
        {
            NuGetPush(nupkg, new NuGetPushSettings 
            {
                ApiKey = githubToken,
                Source = "https://nuget.pkg.github.com/statiqdev/index.json"
            });
        }
    });
    
Task("Publish-Packages")
    .IsDependentOn("Create-Packages")
    .WithCriteria(() => isLocal)
    .WithCriteria(() => isRunningOnWindows)
    // TODO: Add criteria that makes sure this is the master branch
    .Does(() =>
    {
        var apiKey = EnvironmentVariable("STATIQ_NUGET_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Could not resolve NuGet API key.");
        }

        foreach (var nupkg in GetFiles(nugetRoot.Path.FullPath + "/*.nupkg"))
        {
            NuGetPush(nupkg, new NuGetPushSettings 
            {
                ApiKey = apiKey,
                Source = "https://api.nuget.org/v3/index.json"
            });
        }
    });

Task("Publish-Release")
    .IsDependentOn("Build")
    .WithCriteria(() => isLocal)
    .WithCriteria(() => isRunningOnWindows)
    // TODO: Add criteria that makes sure this is the master branch
    .Does(() =>
    {
        var githubToken = EnvironmentVariable("STATIQ_GITHUB_TOKEN");
        if (string.IsNullOrEmpty(githubToken))
        {
            throw new InvalidOperationException("Could not resolve GitHub token.");
        }
        
        var github = new GitHubClient(new ProductHeaderValue("Cake"))
        {
            Credentials = new Credentials(githubToken)
        };
        var release = github.Repository.Release.Create("statiqdev", "Framework", new NewRelease("v" + semVersion) 
        {
            Name = semVersion,
            Body = string.Join(Environment.NewLine, releaseNotes.Notes),
            TargetCommitish = "master"
        }).Result;
    });
    
//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////
 
Task("Package")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Create-Packages");

Task("Default")
    .IsDependentOn("Package");    

Task("Publish")
    .IsDependentOn("Publish-Packages")
    .IsDependentOn("Publish-Release");
    
Task("BuildServer")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Publish-GitHub-Packages");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
