using System.IO;
using UnityEngine;

namespace CMS
{
    internal interface IContentFileReader
    {
        byte[] GetBody();
    }

    public class ResourcesContainerFileReader : IContentFileReader
    {
        public byte[] GetBody()
        {
            return LoadBody(CmsConstants.ContentFileName);
        }

        private static byte[] LoadBody(string fileName)
        {
            var resourcePath = Path.GetFileNameWithoutExtension(fileName);
            var asset = Resources.Load<TextAsset>(resourcePath);

            if (asset == null)
                throw new FileNotFoundException(
                    $"CMS content resource '{resourcePath}' was not found. " +
                    $"Rebuild it using Cms/Build CompositionRoot and ensure " +
                    $"'{CmsConstants.CompileRoot}/{fileName}' is imported by Unity.",
                    fileName);

            using var stream = new MemoryStream(asset.bytes);
            using var reader = new BinaryReader(stream);

            var magic = reader.ReadBytes(ContentFileConstants.Magic.Length);

            if (!System.Linq.Enumerable.SequenceEqual(magic, ContentFileConstants.Magic))
                throw new InvalidDataException($"Invalid container file format: {System.BitConverter.ToString(magic)}");

            var version = reader.ReadInt32();

            if (version != ContentFileConstants.Version)
                throw new InvalidDataException($"Invalid container file version: {version}");

            var bodySize = reader.ReadInt32();

            return reader.ReadBytes(bodySize);
        }
    }
}