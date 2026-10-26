#if UNITY_EDITOR
using CMS;
using CMS.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Cms.Build
{
    public static class CompilerEditorCommand
    {
        [MenuItem("Cms/Build CompositionRoot")]
        public static void Build()
        {
            CompilerBootstrap.Build();
        }
    }

    public class CompilerBootstrap : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Build();
        }

        public static void Build()
        {
            var locator = EditorBootstrap.InitializeServices();
            using var metadataPipeline = locator.Get<MetadataGenerationPipeline>();

            var unregisteredFileCompiler = new UnregisteredFileCompiler(
                CmsConstants.CompileRoot,
                locator.Get<IContentIdResolver>(),
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
