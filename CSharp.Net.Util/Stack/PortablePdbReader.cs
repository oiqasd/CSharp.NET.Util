#if NET
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace CSharp.Net.Util.Stack
{
    /// <summary>
    /// from Ben.Demystifier
    /// </summary>
    internal class PortablePdbReader : IDisposable
    {
        private readonly Dictionary<string, MetadataReaderProvider> _cache = new Dictionary<string, MetadataReaderProvider>(StringComparer.Ordinal);

        public void PopulateStackFrame(StackFrame frameInfo, MethodBase method, int IlOffset, out string fileName, out int row, out int column)
        {
            fileName = "";
            row = 0;
            column = 0;
            if (method.Module.Assembly.IsDynamic)
            {
                return;
            }

            MetadataReader metadataReader = GetMetadataReader(method.Module.Assembly.Location);
            if (metadataReader == null)
            {
                return;
            }

            MethodDebugInformationHandle handle = ((MethodDefinitionHandle)MetadataTokens.Handle(method.MetadataToken)).ToDebugInformationHandle();
            if (handle.IsNil)
            {
                return;
            }

            SequencePointCollection sequencePoints = metadataReader.GetMethodDebugInformation(handle).GetSequencePoints();
            SequencePoint? sequencePoint = null;
            foreach (SequencePoint item in sequencePoints)
            {
                if (item.Offset > IlOffset)
                {
                    break;
                }

                if (item.StartLine != 16707566)
                {
                    sequencePoint = item;
                }
            }

            if (sequencePoint.HasValue)
            {
                row = sequencePoint.Value.StartLine;
                column = sequencePoint.Value.StartColumn;
                fileName = metadataReader.GetString(metadataReader.GetDocument(sequencePoint.Value.Document).Name);
            }
        }

        private MetadataReader? GetMetadataReader(string assemblyPath)
        {
            if (!_cache.TryGetValue(assemblyPath, out var value) && value != null)
            {
                string pdbPath = GetPdbPath(assemblyPath);
                if (!string.IsNullOrEmpty(pdbPath) && File.Exists(pdbPath) && IsPortable(pdbPath))
                {
                    value = MetadataReaderProvider.FromPortablePdbStream(File.OpenRead(pdbPath));
                }

                _cache[assemblyPath] = value;
            }

            return value?.GetMetadataReader();
        }

        private static string? GetPdbPath(string assemblyPath)
        {
            if (string.IsNullOrEmpty(assemblyPath))
            {
                return null;
            }

            if (File.Exists(assemblyPath))
            {
                using PEReader pEReader = new PEReader(File.OpenRead(assemblyPath));
                ImmutableArray<DebugDirectoryEntry>.Enumerator enumerator = pEReader.ReadDebugDirectory().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DebugDirectoryEntry current = enumerator.Current;
                    if (current.Type == DebugDirectoryEntryType.CodeView)
                    {
                        CodeViewDebugDirectoryData codeViewDebugDirectoryData = pEReader.ReadCodeViewDebugDirectoryData(current);
                        string directoryName = Path.GetDirectoryName(assemblyPath);
                        return (directoryName == null) ? null : Path.Combine(directoryName, Path.GetFileName(codeViewDebugDirectoryData.Path));
                    }
                }
            }

            return null;
        }

        private static bool IsPortable(string pdbPath)
        {
            using FileStream fileStream = File.OpenRead(pdbPath);
            return fileStream.ReadByte() == 66 && fileStream.ReadByte() == 83 && fileStream.ReadByte() == 74 && fileStream.ReadByte() == 66;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<string, MetadataReaderProvider> item in _cache)
            {
                item.Value?.Dispose();
            }

            _cache.Clear();
        }
    }
}
#endif