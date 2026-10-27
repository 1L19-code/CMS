using System.IO;
using UnityEngine;

namespace CMS.Editor
{
    public interface IContainerFileReader
    {
        byte[] GetBody();
    }

    public class ContainerFileReader : IContainerFileReader
    {
        private readonly byte[] _body;

        public ContainerFileReader(string filePath)
        {
            _body = LoadBody(filePath);
        }

        public byte[] GetBody()
        {
            return _body;
        }

        private static byte[] LoadBody(string filePath)
        {
            var path = Path.GetFullPath(filePath);

            using var stream = new FileStream(path, FileMode.Open);
            using var reader = new BinaryReader(stream);

            var magic = reader.ReadBytes(ContainerFileConstants.Magic.Length);

            if (!System.Linq.Enumerable.SequenceEqual(magic, ContainerFileConstants.Magic))
                throw new InvalidDataException($"Invalid container file format: {System.BitConverter.ToString(magic)}");

            var version = reader.ReadInt32();

            if (version != ContainerFileConstants.Version)
                throw new InvalidDataException($"Invalid container file version: {version}");

            var bodySize = reader.ReadInt32();

            return reader.ReadBytes(bodySize);
        }
    }

    public class ResourcesContainerFileReader : IContainerFileReader
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

            var magic = reader.ReadBytes(ContainerFileConstants.Magic.Length);

            if (!System.Linq.Enumerable.SequenceEqual(magic, ContainerFileConstants.Magic))
                throw new InvalidDataException($"Invalid container file format: {System.BitConverter.ToString(magic)}");

            var version = reader.ReadInt32();

            if (version != ContainerFileConstants.Version)
                throw new InvalidDataException($"Invalid container file version: {version}");

            var bodySize = reader.ReadInt32();

            return reader.ReadBytes(bodySize);
        }
    }
}
