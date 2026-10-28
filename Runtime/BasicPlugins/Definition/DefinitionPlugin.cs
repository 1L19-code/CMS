using System;
using System.IO;
using System.Text;
using Cms.DefinitionPlugin.Common;
using CMS.Editor;
using UnityEngine;

namespace CMS
{
    public sealed class DefinitionPlugin : ICmsPlugin
    {
        public ReaderId ReaderId => DefinitionConstants.ReaderId;

        public T Get<T>(ReadOnlyMemory<byte> data)
        {
            var json = Encoding.UTF8.GetString(data.Span);
            var meta = JsonUtility.FromJson<DefinitionMeta>(json);

            if (meta == null || string.IsNullOrWhiteSpace(meta.DefinitionType))
                throw new InvalidDataException("Definition metadata must specify a definition type.");

            var type = Type.GetType(meta.DefinitionType);

            if (type == null)
                throw new InvalidOperationException($"Definition type '{meta.DefinitionType}' was not found.");

            if (!typeof(T).IsAssignableFrom(type))
                throw new ArgumentException(
                    $"Cannot resolve definition of type '{type.FullName}' as '{typeof(T).FullName}'.");

            if (string.IsNullOrWhiteSpace(meta.Data))
                throw new InvalidDataException($"Definition data for type '{type.FullName}' is missing.");

            var definition = JsonUtility.FromJson(meta.Data, type);

            if (definition is T value)
                return value;

            throw new InvalidDataException($"Failed to deserialize definition of type '{type.FullName}'.");
        }
    }
}
