#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CMS.Editor
{
    public static class CompilerEditorCommand
    {
        [MenuItem("CMS/Compile")]
        public static void Build()
        {
            var locator = EditorBootstrap.Initialize();
            CompilerBootstrap.Build(locator);
            AssetDatabase.Refresh();
        }
    }

    public static class CompilerBootstrap
    {
        public static void Build(Locator locator)
        {
            using var metadataPipeline = locator.Get<MetadataGenerationPipeline>();

            var unregisteredFileCompiler = new UnregisteredFileCompiler(
                CmsConstants.CompileRoot,
                locator.Get<IMetadataResolver<ContentId>>(),
                locator.Get<IReaderIdService>(),
                locator.Get<IRegistrationResolver<FileExtension>>());

            var pipeline = new ContentCompilePipeline(
                unregisteredFileCompiler,
                locator.Get<IRegistrationResolver<ICustomContentCompiler>>(),
                metadataPipeline);

            locator.Register(pipeline);
            pipeline.Build();

            AssetDatabase.Refresh();
            Debug.Log("Successfully compiled CMS content");
        }
    }
}
#endif
