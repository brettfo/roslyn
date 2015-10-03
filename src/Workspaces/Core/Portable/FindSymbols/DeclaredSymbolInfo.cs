// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.FindSymbols
{
    internal enum DeclaredSymbolInfoKind : byte
    {
        Class,
        Constant,
        Constructor,
        Delegate,
        Enum,
        EnumMember,
        Event,
        Field,
        Indexer,
        Interface,
        Method,
        Module,
        Property,
        Struct
    }

    internal struct DeclaredSymbolInfo
    {
        public string Name { get; }
        public string DisplayName { get; }
        public string ContainerDisplayName { get; }
        public string FullyQualifiedContainerName { get; }
        public DeclaredSymbolInfoKind Kind { get; }
        public Accessibility Accessibility { get; }
        public TextSpan Span { get; }
        public ushort ParameterCount { get; }
        public ushort TypeParameterCount { get; }

        public DeclaredSymbolInfo(string name, string displayName, string containerDisplayName, string fullyQualifiedContainerName, DeclaredSymbolInfoKind kind, Accessibility accessibility, TextSpan span, ushort parameterCount = 0, ushort typeParameterCount = 0)
            : this()
        {
            Name = name;
            DisplayName = displayName;
            ContainerDisplayName = containerDisplayName;
            FullyQualifiedContainerName = fullyQualifiedContainerName;
            Kind = kind;
            Accessibility = accessibility;
            Span = span;
            ParameterCount = parameterCount;
            TypeParameterCount = typeParameterCount;
        }

        internal void WriteTo(ObjectWriter writer)
        {
            writer.WriteString(Name);
            writer.WriteString(DisplayName);
            writer.WriteString(ContainerDisplayName);
            writer.WriteString(FullyQualifiedContainerName);

            // DeclaredSymbolInfoKind only has 14 members and Accessibility only has 7, so they can each be represented in 4 bits
            var kindAndAcc = (int)Kind << 4 | (int)Accessibility;
            writer.WriteByte((byte)kindAndAcc);

            writer.WriteInt32(Span.Start);
            writer.WriteInt32(Span.Length);
            writer.WriteUInt16(ParameterCount);
            writer.WriteUInt16(TypeParameterCount);
        }

        internal static DeclaredSymbolInfo ReadFrom(ObjectReader reader)
        {
            try
            {
                var name = reader.ReadString();
                var displayName = reader.ReadString();
                var immediateContainer = reader.ReadString();
                var entireContainer = reader.ReadString();

                var kindAndAcc = reader.ReadByte();
                var kind = (DeclaredSymbolInfoKind)(kindAndAcc >> 4);
                var accessibility = (Accessibility)(kindAndAcc & 0x0F);

                var spanStart = reader.ReadInt32();
                var spanLength = reader.ReadInt32();
                var parameterCount = reader.ReadUInt16();
                var typeParameterCount = reader.ReadUInt16();

                return new DeclaredSymbolInfo(name, displayName, immediateContainer, entireContainer, kind, accessibility, new TextSpan(spanStart, spanLength), parameterCount, typeParameterCount);
            }
            catch
            {
                return default(DeclaredSymbolInfo);
            }
        }
    }
}
