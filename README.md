### Static content IDs

`MetadataGenerationPipeline` generates `Assets/_Generated/Content.cs` after metadata
generation, on initialization and each `IRootDirectoryChangeObserver.Changed` event.
Access IDs through `CMS.Generated.CmsData.FILE_NAME`. Unchanged source is not rewritten.

Register exclusions through an editor extension installer:

```csharp
using CMS;
using CMS.Editor;

public sealed class StaticSourceExclusions : EditorExtensionInstaller<StaticSourceIgnore>
{
    public override void Install(Locator locator, IRegistrator<StaticSourceIgnore> registrator)
    {
        registrator.Register(StaticSourceIgnore.ForExtension(".txt"));
        // Path relative to CmsConstants.PreCompilePath (or an absolute path).
        registrator.Register(StaticSourceIgnore.ForPath("Internal/settings.json"));
        registrator.Register(StaticSourceIgnore.ForId(
            ContentId.FromGuid("11111111-1111-1111-1111-111111111111")));
    }
}
```

Exclusions affect only the generated source. Metadata and content compilation still run.
Names are uppercased, unsupported characters become underscores, and leading digits
are prefixed with an underscore. Name collisions receive a content ID suffix.
