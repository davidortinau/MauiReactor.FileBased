# MauiReactor file-based SDK (experimental)

`MauiReactor.FileBased.Sdk` is an experimental preview MSBuild SDK. It supplies
MAUI build defaults, package references, C# startup source, and an Apple
`Info.plist` with a `UIApplicationSceneManifest`. It has no compiled runtime
library of its own. Previews are distributed through
[GitHub Packages](https://github.com/davidortinau/MauiReactor.FileBased/packages),
not NuGet.org. This is a personal experiment, not an official Microsoft release.

The sample uses `#:sdk MauiReactor.FileBased.Sdk@0.1.0-preview.2`.
Configure the GitHub Packages feed and authenticate with a personal access token
(classic) with `read:packages`, as described in the
[repository README](https://github.com/davidortinau/MauiReactor.FileBased#run-the-counter).
GitHub requires authentication even for public NuGet packages.
SDK resolution happens before normal package restore, so the feed must be
available in `NuGet.config`; a later `#:package` cannot bootstrap this SDK.

## App contract

Define a public `App` component in the global namespace, with a public
parameterless constructor. The packaged startup calls
`UseMauiReactorApp<global::App>()`. Inherited component factory methods are
available through a generated `global using MauiReactor`.

The default target is `net10.0-maccatalyst`. iOS startup is included but untested.
Android and Windows startup are not included.

Defaults include `UseMaui=true`, `SingleProject=true`, `PublishAot=false`,
minimum Apple platform version `15.0`, app version `1.0` / build `1`, and
the source folder name as the app title. The default application identifier is
`com.mauilabs.reactor.` plus the folder name with non-alphanumeric characters
removed and letters lowercased. Set your own identifier for real apps.
The SDK aligns `AssemblyName` with `ApplicationTitle` because the Apple
`dotnet run` target opens `$(AssemblyName).app`.

Package versions are self-contained SDK defaults rather than references to the
repository's central package file. This lets a generated app run outside the
repository. Override them with `#:property` directives when needed:

| Property | Default |
|---|---|
| `MauiReactorPackageVersion` | `4.0.18` |
| `MauiControlsPackageVersion` | `10.0.41` |
| `MauiDevFlowPackageVersion` | `0.1.0-preview.13.26512.5` |
| `MauiReactorEnableDevFlow` | `true` (Debug only) |
| `MauiReactorEnableDefaultResourceItems` | `true`: include `Resources/Images`, `Resources/Fonts`, and `Resources/Raw` next to `app.cs`, and register fonts by file name |

Other version combinations are untested. The SDK disables Central Package
Management for its consumer project. Extra dependencies can use `#:package`.
It disables implicit namespace imports to prevent MAUI/Reactor type-name
conflicts, then imports `MauiReactor` explicitly.

## Override the manifest

The target runs after the Apple SDK's `_DetectAppManifest` target, for MAUI iOS
and Mac Catalyst builds only. An existing `AppBundleManifest`, including a
manifest detected by the Apple SDK, takes priority. If `ApplicationManifest`
is set instead, the package uses that path. Otherwise it selects the packaged
default.

For example, add this directive to use your own Apple manifest:

```csharp
#:property AppBundleManifest=MyInfo.plist
```

The SDK supplies a C# scene delegate registered as `SceneDelegate` and derived
from `Microsoft.Maui.MauiUISceneDelegate`. The default manifest uses MAUI's
`__MAUI_DEFAULT_SCENE_CONFIGURATION__` configuration name and opens one window.
It supplies no app icon, privacy permission strings, signing configuration, or
store metadata. Use your own manifest and a regular MAUI project for distribution.
