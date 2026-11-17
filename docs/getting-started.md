# Build a file-based MauiReactor app

This experiment builds and runs a native .NET MAUI app from one C# source file.
It uses [MauiReactor](https://github.com/adospace/reactorui-maui), the community
library by Adolfo Marinucci. It does not use Microsoft's WinUI Reactor.

The generated app contains `app.cs`, with no project file, XAML, platform startup
files, or manifest. Shared build and native startup code lives in the
`MauiReactor.FileBased.Sdk` package.

**Status:** experimental, Mac Catalyst runtime checks only. SDK previews are
distributed through GitHub Packages, not NuGet.org. Install the template from
this checkout.

## Install the prerequisites

Use macOS with:

- The .NET 10 SDK. This sample's `global.json` selects the latest installed
  .NET 10 feature band, starting at `10.0.100`.
- The MAUI workload for that SDK.
- Xcode compatible with the installed Apple workload.
- Network access to nuget.org and to GitHub Packages (including its Azure blob download host) for first restore.
- A GitHub personal access token (classic) with `read:packages`, and access to
  the package. GitHub requires authentication for public NuGet packages too.

For the workload set used in the runtime checks:

```console
dotnet workload install maui --version 10.0.102
```

Run this command from the checkout root so that it selects .NET 10.
Follow the [Apple workload/Xcode compatibility guidance](https://aka.ms/xcode-requirement).
Installing the workload and installing Xcode are separate steps.

### Exact runtime check environment

| Item | Version or result |
|---|---|
| Host | Apple Silicon, macOS 27.2.1 |
| .NET SDK | 10.0.101 |
| Workload set | 10.0.102 |
| Mac Catalyst SDK | 26.2.10191 |
| Xcode installed / required by workload | 27.2 / 26.2 |
| Reactor.Maui | 4.0.18 |
| Microsoft.Maui.Controls | 10.0.41 |
| DevFlow CLI and agent | 0.1.0-preview.13.26512.5 |
| Original local SDK package | 0.1.0-local.3 |
| Sample and template output | Native launch and counter state change passed |

The host did not have Xcode 26.2. The runtime checks therefore added
`-p:ValidateXcodeVersion=false` to build/run commands. This local override bypasses
the workload's version check; it does not make the combination supported. The
normal commands below assume a matched Xcode and workload. Do not copy the
override into a release build configuration.

## Authenticate before SDK resolution

The checkout's `NuGet.config` contains the feed URL, not credentials.
It maps `MauiReactor.FileBased.Sdk` to GitHub Packages and other packages to
nuget.org. Keep this configuration beside your app or in a parent
directory. The custom SDK resolves before normal package restore, so adding a
`#:package` or a late `RestoreSources` property cannot bootstrap it.

Create a personal access token **(classic)** with `read:packages`. The token
must belong to a user who can read the package. Fine-grained tokens are not
supported by this registry. Follow
[GitHub's NuGet authentication guidance](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry).

Set the NuGet credential environment variable in the shell that runs `dotnet`.
For Zsh:

```console
read -rs "GITHUB_PACKAGES_TOKEN?GitHub Packages token: "; echo
export NuGetPackageSourceCredentials_github="Username=YOUR_GITHUB_USERNAME;Password=$GITHUB_PACKAGES_TOKEN;ValidAuthenticationTypes=Basic"
unset GITHUB_PACKAGES_TOKEN
```

For Bash, replace the first command with
`read -rsp "GitHub Packages token: " GITHUB_PACKAGES_TOKEN; echo`.
Replace `YOUR_GITHUB_USERNAME` with your GitHub login. The suffix `github` must
match the source key in `NuGet.config`, including case. Do not commit the token
or paste it into a command argument. Unset the credential environment variable
when finished: `unset NuGetPackageSourceCredentials_github`.

An HTTP 401/403 means authentication or package access failed. Check the token
type, scope, owner access, and source name. An SDK already in the global package
cache can hide this problem; a first-time user must be able to download it.
Published package visibility is controlled separately from repository visibility.

## Run the sample

```console
dotnet run samples/Counter/app.cs
```

Click **Increment**. The counter changes from `Count: 0` to `Count: 1`.
The value is in-memory component state. It resets when the process exits.

Use the file name explicitly. Bare `dotnet run` reported that no project existed
on the tested SDK. The `.csproj` name in build messages identifies the SDK's
virtual project, not a file that you must create.

## Create an app

From the checkout root:

```console
dotnet new install ./samples/Counter
dotnet new maui-reactor-file -n HelloReactor -o artifacts/HelloReactor
cd artifacts/HelloReactor
dotnet run app.cs
```

The output is one `app.cs` file. It inherits the feed and .NET 10 selection
from the sample's parent directories. The app title defaults to `HelloReactor`.
The template does not run restore or change a global NuGet configuration.

To generate somewhere else, put a `NuGet.config` in that location or a parent
directory. Copy this checkout's `NuGet.config` and `global.json`, and set the
same credential environment variable in your shell. You can then install this
template by its checkout path and generate the same single C# file.

If an older checkout has already installed `maui-reactor-file`, the template
engine can report conflicting templates. `dotnet new uninstall` lists the
registered package paths. Remove the older template registration with
`dotnet new uninstall <old-template-path>`, then install this checkout's
`samples/Counter` template. This removes its template registration, not its
source files. Do not remove another project's registration without checking it.

## Read the source

The complete app is:

```csharp
#:sdk MauiReactor.FileBased.Sdk@0.1.0-preview.1

public class CounterState
{
    public int Count { get; set; }
}

public class App : Component<CounterState>
{
    public override VisualNode Render()
        => ContentPage(
            VStack(
                Label("Hello, MauiReactor!").FontSize(28),
                Label("One C# source file. Native .NET MAUI controls."),
                Label($"Count: {State.Count}")
                    .FontSize(24)
                    .AutomationId("count"),
                Button("Increment", () => SetState(s => s.Count++))
                    .AutomationId("increment")
            )
            .Spacing(16)
            .Padding(32)
            .VCenter()
        );
}
```

`App` must be a public component in the global namespace, with a public
parameterless constructor. The SDK's startup calls
`UseMauiReactorApp<global::App>()`.

`Component<CounterState>` supplies `State` and `SetState`. The button callback
changes state, and MauiReactor updates the native MAUI controls.
`ContentPage`, `VStack`, `Label`, and `Button` are inherited factory methods.
They keep the markup free of constructor syntax.

The SDK adds `global using MauiReactor`. It disables implicit MAUI namespace
imports so that `Label` and `Button` are not ambiguous between MAUI and Reactor
types. Add explicit namespace imports for other libraries when needed.

## Use directives and overrides

[.NET file-based apps](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/file-based-programs)
support SDK, package, and property directives:

```csharp
#:sdk MauiReactor.FileBased.Sdk@0.1.0-preview.1
#:property ApplicationTitle=MyCounter
#:property ApplicationId=com.example.mycounter
```

You can add `#:package Package.Name@version` for another dependency. Pin the
version that you tested. This app uses native .NET SDK directives, rather than a
custom source parser.

| Setting | Default / behavior |
|---|---|
| `TargetFramework` | `net10.0-maccatalyst` |
| `ApplicationTitle` | Source directory name |
| `AssemblyName` | Aligned with `ApplicationTitle` for the Apple run target |
| `ApplicationId` | `com.mauilabs.reactor.` plus sanitized directory name |
| `ApplicationDisplayVersion` / `ApplicationVersion` | `1.0` / `1` |
| `SupportedOSPlatformVersion` | `15.0` |
| `PublishAot` | `false` |
| `MauiReactorEnableDevFlow` | `true` in Debug; no agent reference in Release |
| Package version properties | See the [SDK contract](../src/MauiReactor.Sdk/README.md) |

The SDK owns package versions and disables Central Package Management for its
consumer. This keeps generated apps independent of a parent's package settings.

### Override the Apple manifest

```csharp
#:property AppBundleManifest=MyInfo.plist
```

Use this when you need permission descriptions, custom scene setup, or other
Apple metadata. `AppBundleManifest` is the Apple workload's property.
The SDK also accepts `ApplicationManifest` as an alias when
`AppBundleManifest` is empty.

The packaged target runs after the Apple SDK detects a consumer manifest.
A detected or explicitly selected `AppBundleManifest` takes priority. The
packaged default is only the fallback. It enables one MAUI scene and names the
packaged `SceneDelegate` class.

Apple SDK 27 requires scene-based lifecycle adoption. Omitting that metadata
caused a native launch failure during development. The packaged manifest keeps
this requirement out of the user's C# source.

## Inspect the app with DevFlow

The SDK includes and registers the DevFlow agent only for Debug builds.
Use the matching CLI version:

```console
dotnet tool install Microsoft.Maui.Cli \
    --version 0.1.0-preview.13.26512.5 \
    --tool-path artifacts/tools
```

From the checkout root, after launching the app:

```console
artifacts/tools/maui devflow wait
artifacts/tools/maui devflow list
artifacts/tools/maui devflow ui query --automationId count --agent-port <port>
artifacts/tools/maui devflow ui tap --automationId increment --agent-port <port>
artifacts/tools/maui devflow ui assert Text "Count: 1" --automationId count --agent-port <port>
artifacts/tools/maui devflow ui screenshot --agent-port <port> --output counter.png
```

Replace `<port>` with the port for this app in `devflow list`. When several apps
run, use app identity and an explicit port. A generic wait result alone does not
prove that it selected this app.

A stale broker or mismatched CLI can reject mutations with a lease-authority
error. Use a matching CLI, agent, and broker. Do not restart a shared broker
while another session uses it.

To omit DevFlow even in Debug:

```csharp
#:property MauiReactorEnableDevFlow=false
```

Release evaluation excludes the package reference and startup registration.
This check is not a store-distribution or security audit.

## Edit and rerun

Quit the app, edit `app.cs`, and run `dotnet run app.cs` again. The .NET SDK
rebuilds changed source. A second `open` of a running Mac Catalyst app can reuse
the existing process, so quit before checking a source edit.

Reactive updates through `SetState` are runtime state changes. They are separate
from code hot reload.

This sample does not promise code hot reload. MauiReactor's existing
`Reactor.Maui.HotReloadConsole` tool opens a `.csproj` through an MSBuild workspace.
Its file-based virtual-project path has not been integrated or tested here.
`dotnet watch` is also untested for this sample. Use the normal project-based
MauiReactor template when you need its established hot reload workflow.

## Choose a platform

| Platform | This sample |
|---|---|
| Mac Catalyst on Apple Silicon | Launch, factory markup, button state update, and template output checked |
| iOS simulator/device | Apple startup code included; build, deployment, and UI untested |
| Intel Mac | Untested |
| Android | No startup implementation in this SDK |
| Windows | No startup implementation in this SDK |
| Linux GTK / macOS AppKit | Not included |

An iOS experiment can select `#:property TargetFramework=net10.0-ios`, but it
still needs a compatible workload, simulator or device, and platform-specific
deployment settings. This guide does not claim that changing one directive
proves that platform.

MauiReactor itself supports more targets than this small sample SDK. Use the
upstream template for those platforms.

## Understand the boundary

This is a **standalone native app**, not a companion-hosted script. The .NET SDK
builds the source into an app bundle in its file-based build cache. There is no
Go server, QR pairing step, or network code loader.

The prototype has no app icon, custom fonts, splash assets, privacy permission
strings, signing setup, entitlements file, or store packaging workflow.
It does not enable Reactor's code hot reload or provide multi-file authoring.
Counter state is not persistent storage.

For a larger or distributable app, start with
[`Reactor.Maui.TemplatePack`](https://github.com/adospace/reactorui-maui#quick-start)
and move the component into a regular MAUI project. Keep its explicit native
metadata, resources, signing, and platform setup. Automatic conversion of this
custom SDK app to that project format has not been checked.

## Publish a preview

The package ID is `MauiReactor.FileBased.Sdk`. The package project sets
`RepositoryUrl` to this repository so that GitHub attaches the package here.
Publishing uses GitHub Packages, not NuGet.org.

Versions use `0.1.0-preview.N`, starting with `0.1.0-preview.1`.
Increase `N` in the SDK project and update the sample directive and documentation
before publishing new contents. Versions are immutable; the workflow deliberately
does not skip duplicate versions or overwrite packages.

Run **Actions > Publish preview > Run workflow** on `main`, or push a tag such as
`v0.1.0-preview.1` whose version exactly matches the SDK project. The workflow
accepts only preview versions. A manual run also uses the checked-in version.
It uses `contents: read` and `packages: write` with the short-lived `GITHUB_TOKEN`;
no personal publishing token is needed in repository secrets.

After publishing, a separate step resolves `Sdk.props` from an empty package
cache using the authenticated GitHub feed. This checks registry consumption and
early SDK resolution, not native app execution. Native Mac Catalyst checks still
require macOS, the MAUI workload, and Xcode.

GitHub initially gives new packages private visibility. Check the package's
settings after the first publish and set the intended visibility and access.
The repository being public does not by itself make a new package public.
Even a public GitHub NuGet package requires authenticated downloads.

### Develop the SDK locally

Local development does not need a GitHub publishing token:

```console
dotnet pack src/MauiReactor.Sdk/MauiReactor.Sdk.csproj -o artifacts/feed
mkdir -p artifacts/LocalCounter
cp samples/Counter/app.cs artifacts/LocalCounter/app.cs
```

Create `artifacts/NuGet.config` with the following content. Its paths are relative
to `artifacts/`, and its mapping overrides the parent configuration:

```xml
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <clear />
    <packageSource key="local">
      <package pattern="MauiReactor.FileBased.Sdk" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

Then run `dotnet run artifacts/LocalCounter/app.cs`. For changed SDK contents,
use a new local version such as `0.1.0-local.4` and change the copied directive
to match. NuGet caches packages by ID and version; a cached published version
can otherwise hide local changes. Keep local-only versions out of publishing
commits.

## Prior work and source references

This experiment combines existing work:

- The [.NET SDK file-based implementation](https://github.com/dotnet/sdk/blob/v10.0.101/src/Cli/dotnet/Commands/Run/VirtualProjectBuildingCommand.cs)
  creates a virtual project and writes console defaults before importing a
  custom SDK. This allows `#:sdk` to provide the MAUI target before restore.
- [MauiReactor](https://github.com/adospace/reactorui-maui) already supplies
  `UseMauiReactorApp`, stateful components, fluent control factories, a
  project template pack, and a project-based hot reload tool. The inspected
  upstream snapshot was `b728a69ac30da98e24ec84382d978b0853a35475`.
- [Comet Go](https://github.com/dotnet/maui-labs/blob/main/src/Go/README.md) supplies a related authoring experience,
  but its server and companion load Comet code. Its
  [delta compiler](https://github.com/dotnet/maui-labs/blob/main/src/Go/Server/DeltaCompiler.cs) strips directives
  before compilation. This SDK does not reuse that loader or change Comet.
- MAUI's [application delegate](https://github.com/dotnet/maui/blob/10.0.41/src/Core/src/Platform/iOS/MauiUIApplicationDelegate.cs)
  selects scene startup from manifest metadata, and its
  [scene delegate](https://github.com/dotnet/maui/blob/10.0.41/src/Core/src/Platform/iOS/MauiUISceneDelegate.cs)
  creates the MAUI window for the default scene configuration.
- The [Apple workload](https://github.com/dotnet/macios/blob/main/msbuild/Xamarin.MacDev.Tasks/Tasks/CompileAppManifest.cs)
  validates the scene manifest for SDK 27 and newer.

The local source review covered `reactorui-maui`, `mauireactor-samples`,
`SimpleReactor`, and the maui-labs Go path. It did not identify an existing
file-based MauiReactor implementation in those sources. This is a bounded
search result, not a claim that no such work exists elsewhere.
