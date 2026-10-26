namespace CMS.Editor
{
    public interface IMetadataGenerator
    {
        int Order { get; }
        FileExtension MetadataExtension { get; }

        /// <summary>
        /// Returns metadata bytes for a content file, or null to skip writing.
        /// existingData is null when the metadata file does not exist.
        /// </summary>
        byte[] Generate(FileEntry entry, byte[] existingData);
    }
}
