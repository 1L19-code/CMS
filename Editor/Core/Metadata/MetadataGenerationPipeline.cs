using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Editor
{
    internal sealed class MetadataGenerationPipeline : IDisposable
    {
        private readonly IRegistrationResolver<IMetadataGenerator> _generators;
        private readonly IRegistrationResolver<FileExtension> _customMetadataResolver;
        private readonly IRegistrator<FileExtension> _customMetadataRegistrator;
        private readonly IRootDirectoryChangeObserver _observer;
        private readonly StaticSourceService _staticSource;

        public MetadataGenerationPipeline(IRegistrationResolver<IMetadataGenerator> generators,
            IRegistrationResolver<FileExtension> customMetadataResolver,
            IRegistrator<FileExtension> customMetadataRegistrator,
            IRootDirectoryChangeObserver observer, StaticSourceService staticSource)
        {
            _generators = generators;
            _customMetadataResolver = customMetadataResolver;
            _customMetadataRegistrator = customMetadataRegistrator;
            _observer = observer;
            _staticSource = staticSource;

            Refresh();
            _observer.Changed += Refresh;
        }

        public void Refresh()
        {
            var generators = _generators.GetInstances().OrderBy(generator => generator.Order).ToArray();
            var generatedExtensions = new HashSet<FileExtension>();

            foreach (var generator in generators)
            {
                var extension = generator.MetadataExtension;

                if (string.IsNullOrEmpty(extension.Value))
                    throw new InvalidOperationException("Metadata generator must declare a file extension");

                if (!generatedExtensions.Add(extension))
                    throw new InvalidOperationException(
                        $"Multiple metadata generators registered for '{extension}'");

                _customMetadataRegistrator.Register(extension);
            }

            var customMetadataExtensions = _customMetadataResolver.GetInstances().ToHashSet();

            var contentEntries = RawFileManager.GetAllEntries()
                .Where(entry => !entry.IsDirectory &&
                                !customMetadataExtensions.Contains(FileExtension.FromPath(entry.Path)))
                .ToArray();

            foreach (var entry in contentEntries)
            {
                foreach (var generator in generators)
                {
                    var existingData =
                        MetadataFileUtility.LoadOrDefault(entry.Path, generator.MetadataExtension);
                    var data = generator.Generate(entry, existingData?.ToArray());

                    if (data == null || existingData != null && data.SequenceEqual(existingData))
                        continue;

                    RawFileManager.Save(data,
                        MetadataFileUtility.GetPath(entry.Path, generator.MetadataExtension));
                }
            }

            _staticSource.Build(contentEntries);
        }

        public void Dispose()
        {
            _observer.Changed -= Refresh;
        }
    }
}