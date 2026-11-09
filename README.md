# CMS for Unity

A file-based content management package for Unity. Author content in a dedicated editor window, reference it through stable GUID-based IDs, and compile it into a binary container for runtime lookup.

The package includes plugins for serializable definitions, Unity prefab references, and entities that group related content.

## Authoring workflow

1. Open **CMS > Window**. The package creates a `Content` directory at the Unity project root if necessary.
2. Right-click a directory in the content tree to create a folder, definition, prefab reference, or entity.
3. Select a file to edit it in the inspector pane and save your changes using its controls.
4. Run **CMS > Compile** to refresh metadata and generate runtime assets.
5. Recompile after changing content and before building your player. The package does not register an automatic player-build compilation hook.

Keep `.cid` sidecar files with their content in version control: they hold the IDs used by references. Move content through the CMS file manager to preserve its associated metadata.

## Runtime API

Create a content service with `ContentFactory.Create()`, then call and await `IndexAsync()` on Unity's main thread before retrieving content. Initialize once during application startup and reuse the instance for lookups.

For example, after creating and compiling a prefab reference named `Player` in the CMS window:

```csharp
using CMS;
using CMS.Generated;
using UnityEngine;

public sealed class ContentExample : MonoBehaviour
{
    private IContent _content;

    private async void Start()
    {
        try
        {
            _content = ContentFactory.Create();
            await _content.IndexAsync();

            GameObject prefab = _content.Get<GameObject>(CmsData.PLAYER);
            Instantiate(prefab);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }
}
```

`CmsData.PLAYER` is generated from your content filename; replace it with the appropriate generated field. Use `ContentId.FromGuid("...")` if you already know an existing content ID. If your scripts use assembly definitions, reference `ilya.cms.Runtime` and ensure the generated IDs are accessible to your game assembly.

The factory returns a new, independent `IContent` instance on each call. Creation does not load Resources or initialize plugins. `IContent` exposes:

```csharp
ValueTask IndexAsync();
T Get<T>(ContentId contentId);
```

`IndexAsync()` loads the compiled Resources container, discovers runtime plugin installers, and builds the lookup index. Missing IDs, incompatible requested types, and unregistered readers throw exceptions. Calling `Get<T>()` before initialization also throws.

For definitions, request your definition type. For entities, request `Entity` and then resolve its attachments as shown below.

## CmsData: accessing content IDs

`CMS.Generated.CmsData` is a generated static class containing named `ContentId` fields for your content. It lives in `Assets/_Generated/Content.cs` and is refreshed by the editor metadata pipeline, including when you compile content.

For example, a file named `Player.pfb` produces `CmsData.PLAYER`. Use that field with `content.Get<GameObject>(CmsData.PLAYER)` instead of copying GUID strings into game code. The field identifies the content; it does not load or cache the object.

Field names follow filenames, while the underlying IDs are stored in `.cid` metadata. A rename can change the generated field name, so update affected code references. Do not edit the generated file manually. Some files, including entity attachments, are intentionally excluded from this class while remaining available to the content system.

## Standard plugins

Each standard content plugin has both **Editor** and **Runtime** parts. In this package they are split between `Editor/BasicPlugins/<Plugin>` and `Runtime/BasicPlugins/<Plugin>`, within the package's corresponding assemblies. The Editor part provides authoring tools and compilation integration; the Runtime part reads compiled content in the player.

This layout was chosen for user convenience: the standard plugins are included in the CMS assemblies, so users only need to reference the appropriate CMS assembly (`ilya.cms.Runtime` or `ilya.cms.Editor`) rather than add a separate assembly reference for each standard plugin.

### Definitions (`.def`)

Definitions store typed data using Unity's `JsonUtility`. Create a serializable subclass of `DefinitionBase` in a runtime assembly:

```csharp
using System;
using Cms.DefinitionPlugin.Common;

[Serializable]
public sealed class EnemyDefinition : DefinitionBase
{
    public int Health = 100;
    public float Speed = 3f;
}
```

Create a definition in the CMS window, select its type, edit its fields, and save it. Follow Unity serialization rules for fields. Concrete definition classes must have unique simple type names because the editor uses those names as selector keys.

The **Editor** part provides the creation action, type selector, field inspector, and icon. The **Runtime** part deserializes the stored type and JSON data into your definition class. Retrieve it with `content.Get<EnemyDefinition>(definitionId)`.

Keep definition types available in player builds. Types resolved only through reflection may require preservation when managed code stripping is enabled.

### Prefab references (`.pfb`)

A `.pfb` file stores the GUID of a Unity asset. Assign an asset in its inspector and click **Save**. Saving a scene `GameObject` creates a prefab asset under `Assets/Prefabs/`.

Compilation collects referenced assets in `prefabs.asset`. At runtime, request the asset's actual type, such as `GameObject`. Retrieval returns the asset reference; instantiate it explicitly when you need a scene instance.

The **Editor** part provides the asset picker, save action, icon, and custom compiler that builds the reference container. The **Runtime** part loads that container and resolves the GUID stored in each content segment to its Unity asset.

### Entities (`.ent`)

Entities group content references into the predefined `Prefab`, `Model`, and `UI` slots, plus arbitrary named attachments:

```csharp
Entity entity = content.Get<Entity>(entityId);
GameObject prefab = entity.Get<GameObject>(EntitySlot.Prefab);
EnemyDefinition settings = entity.Get<EnemyDefinition>("settings");
```

This example uses the `CMS` and `UnityEngine` namespaces. It assumes an initialized `IContent` instance, an authored entity ID, and both attachments assigned. Named keys are case-sensitive. Missing attachments throw exceptions.

Attached files are hidden from the main content tree and excluded from generated static IDs by the entity editor plugin. They still receive metadata and are included in compilation.

The **Editor** part provides entity creation, an icon, and an attachment inspector, and manages visibility of attached content. The **Runtime** part decodes the attachment index and returns an `Entity` whose `Get<T>()` methods resolve attachments through the same `IContent` service.

### Folder support

`Editor/BasicPlugins/Directory` adds the **Create/Folder** action. It is an editor utility rather than a runtime content type, so it has no matching runtime reader.

## Installation

The package manifest declares Unity **2022.2** as the minimum version.

Install via Git using `https://github.com/1L19-code/CMS.git`, place the repository at `Packages/com.ilya.cms` in your Unity project, or use **Window > Package Manager > Add package from disk** and select its `package.json`.

The current source references Unity Visual Scripting, Plastic SCM's `Codice.CM.Common`, and `JetBrains.Annotations` without declaring package dependencies. These assemblies must be available, or the corresponding unused imports must be removed before compilation. The editor assembly definition also contains an additional GUID-based assembly reference that must resolve in the host project.

## Files and generated assets

| Path or extension | Purpose |
| --- | --- |
| `Content/` | Authoring files, relative to the Unity project root |
| `.cid` | Content ID sidecar metadata |
| `.rid` | Runtime reader ID sidecar metadata |
| `Assets/_Generated/Content.cs` | Generated `CMS.Generated.CmsData` fields |
| `Assets/Resources/content.bytes` | Compiled binary container |
| `Assets/Resources/prefabs.asset` | Generated Unity asset reference container |
| `Assets/Prefabs/` | Default destination for prefabs created from scene objects |

`CmsConstants` exposes the source directory, compilation directory, metadata extensions, and container filename. The prefab compiler uses the fixed path `Assets/Resources/prefabs.asset`. Runtime loading uses `Resources.Load`, so keep the generated assets in a Resources directory.

## Advanced: generated content IDs

Metadata generation writes `Assets/_Generated/Content.cs`. Access IDs through `CMS.Generated.CmsData.FILE_NAME`, or construct a known ID with `ContentId.FromGuid("...")`.

Generation runs when editor services initialize and when `IRootDirectoryChangeObserver.Changed` fires. Fields are sorted by name. The source is rewritten when names or IDs change, including removals and exclusions; comparison ignores field order, formatting, and comments. Missing or malformed generated files are regenerated.

Names come from filenames without extensions. Spaces and camelCase boundaries split words (`playerHealth` and `Player Health` become `PLAYER_HEALTH`; `HTTPServer` becomes `HTTP_SERVER`). Unsupported characters become underscores, leading digits receive an underscore prefix, and collisions receive a content ID suffix.

Register exclusions through an editor extension installer:

```csharp
using CMS;
using CMS.Editor;

public sealed class StaticSourceExclusions : EditorExtensionInstaller<StaticSourceIgnore>
{
    public override void Install(Locator locator, IRegistrator<StaticSourceIgnore> registrator)
    {
        registrator.Register(StaticSourceIgnore.ForExtension(".txt"));
        // Relative to CmsConstants.PreCompilePath, or use an absolute path.
        registrator.Register(StaticSourceIgnore.ForPath("Internal/settings.json"));
        registrator.Register(StaticSourceIgnore.ForId(
            ContentId.FromGuid("11111111-1111-1111-1111-111111111111")));
    }
}
```

Place this installer in an editor assembly. Exclusions affect generated source only; metadata generation and content compilation still run.

## Creating custom plugins

For a custom plugin, keep its code in a dedicated directory with separate **Editor** and **Runtime** assemblies:

```text
MyContentPlugin/
  Runtime/
    MyContentPlugin.Runtime.asmdef
    MyPlugin.cs
    MyRuntimeInstaller.cs
  Editor/
    MyContentPlugin.Editor.asmdef
    MyInspector.cs
    MyEditorInstaller.cs
    MyCompiler.cs              # Optional custom compilation
```

Reference `ilya.cms.Runtime` from the runtime assembly. Restrict the editor assembly to the Editor platform and reference `ilya.cms.Editor`, `ilya.cms.Runtime`, and your plugin's runtime assembly. Keep shared types and format identifiers in Runtime so the player never depends on editor code.

Start by choosing a file extension, a data format, and a shared `ReaderId`. Implement a runtime reader for that format, then add an editor inspector and register both through installers. The inspector's reader ID must match the runtime registration. Add a custom compiler only if the source bytes need conversion or additional Unity assets must be generated.

### API overview

Editor and runtime extensions are discovered through reflection. Use concrete installer classes with parameterless constructors.

- **Runtime readers:** implement `ICmsPlugin.Get<T>(ReadOnlyMemory<byte>)` and register it by `ReaderId` through `RuntimeExtensionInstaller<ReaderId, ICmsPlugin>`.
- **Inspectors:** implement `IInspectorWindow` and register it by `FileExtension` through `EditorExtensionInstaller<FileExtension, IInspectorWindow>`. Its `ReaderId` determines the generated reader metadata.
- **Custom compilation:** implement `ICustomContentCompiler`, declare the handled `FileExtensions`, and return `Segment` records from `GetCustomContentSegments()`. Register it through `EditorExtensionInstaller<ICustomContentCompiler>`.
- **Editor behavior:** installers can also register context menu actions, icons, metadata generators, file tree filters, and static source exclusions.

Files without a custom compiler are packed as raw bytes, excluding registered metadata files. Packing a file does not provide runtime decoding: its reader ID must match a registered runtime plugin. The default runtime plugin throws for unsupported content.

Installers receive a `Locator` for accessing services and an `IRegistrator` for registering extensions. `ContentId` identifies a content entry, `ReaderId` selects its runtime decoder, and `Segment` carries the content ID, reader ID, and compiled bytes. Use the standard plugins as examples of how the Editor and Runtime parts fit together.

This is a high-level API overview. The documentation will be expanded in the future with detailed API descriptions and complete custom plugin examples.

## Source layout

```text
Runtime/
  Core/           Runtime interface, indexing, and plugin contracts
  Common/         IDs, constants, service locator, and registration
  BasicPlugins/   Definition, prefab, and entity readers
Editor/
  Core/           CMS window, file management, metadata, and drawers
  Compiler/       Binary container generation and compiler contracts
  BasicPlugins/   Inspectors, authoring actions, and prefab compilation
```
