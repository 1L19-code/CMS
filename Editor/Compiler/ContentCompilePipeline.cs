#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

namespace CMS.Editor
{
    internal class ContentCompilePipeline : IContentCompilePipeline
    {
        private readonly IRegistrationResolver<ICustomContentCompiler> _compilerResolver;

        private readonly UnregisteredFileCompiler _unregisteredFileCompiler;
        private readonly MetadataGenerationPipeline _metadataGenerationPipeline;

        public ContentCompilePipeline(UnregisteredFileCompiler unregisteredFileCompiler,
            IRegistrationResolver<ICustomContentCompiler> compilerResolver,
            MetadataGenerationPipeline metadataGenerationPipeline)
        {
            _unregisteredFileCompiler = unregisteredFileCompiler;
            _compilerResolver = compilerResolver;
            _metadataGenerationPipeline = metadataGenerationPipeline;
        }

        public void Build()
        {
            List<Segment> segments = new();
            List<string> ignoredExtensions = new();
            var compilers = _compilerResolver.GetInstances();

            foreach (var compiler in compilers)
                compiler.Compile();

            _metadataGenerationPipeline.Refresh();

            foreach (var compiler in compilers)
            {
                segments.AddRange(compiler.GetCustomContentSegments());
                ignoredExtensions.AddRange(compiler.FileExtensions);
            }              
            
            _unregisteredFileCompiler.Build(segments, ignoredExtensions.ToHashSet());
        }
    }
}
#endif
