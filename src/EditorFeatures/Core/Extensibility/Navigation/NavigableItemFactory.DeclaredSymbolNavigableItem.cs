// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Editor.Navigation
{
    internal partial class NavigableItemFactory
    {
        internal class DeclaredSymbolNavigableItem : INavigableItem
        {
            public string DisplayString => _declaredSymbolInfo.DisplayName;
            public Document Document { get; }
            public Glyph Glyph => GetGlyph(_declaredSymbolInfo);
            public TextSpan SourceSpan => _declaredSymbolInfo.Span;
            public ImmutableArray<INavigableItem> ChildItems => ImmutableArray<INavigableItem>.Empty;

            public bool DisplayFileLocation => false;

            private readonly DeclaredSymbolInfo _declaredSymbolInfo;

            public DeclaredSymbolNavigableItem(Document document, DeclaredSymbolInfo declaredSymbolInfo)
            {
                Document = document;
                _declaredSymbolInfo = declaredSymbolInfo;
            }

            private static Glyph GetGlyph(DeclaredSymbolInfo declaredSymbolInfo)
            {
                switch (declaredSymbolInfo.Kind)
                {
                    case DeclaredSymbolInfoKind.Class:
                        return GetGlyphFromAccessibility(declaredSymbolInfo.Accessibility, Glyph.ClassPrivate, Glyph.ClassProtected, Glyph.ClassInternal, Glyph.ClassPublic);
                    case DeclaredSymbolInfoKind.Constant:
                        return GetGlyphFromAccessibility(declaredSymbolInfo.Accessibility, Glyph.ConstantPrivate, Glyph.ConstantProtected, Glyph.ConstantInternal, Glyph.ConstantPublic);
                    case DeclaredSymbolInfoKind.Constructor:
                    case DeclaredSymbolInfoKind.Method:
                        return GetGlyphFromAccessibility(declaredSymbolInfo.Accessibility, Glyph.MethodPrivate, Glyph.MethodProtected, Glyph.MethodInternal, Glyph.MethodPublic);
                    case DeclaredSymbolInfoKind.Delegate:
                        return GetGlyphFromAccessibility(declaredSymbolInfo.Accessibility, Glyph.DelegatePrivate, Glyph.DelegateProtected, Glyph.DelegateInternal, Glyph.DelegatePublic);
                    case DeclaredSymbolInfoKind.Enum:
                        return GetGlyphFromAccessibility(declaredSymbolInfo.Accessibility, Glyph.EnumPrivate, Glyph.EnumProtected, Glyph.EnumInternal, Glyph.EnumPublic);
                    case DeclaredSymbolInfoKind.EnumMember:
                        return Glyph.EnumMember;
                    case DeclaredSymbolInfoKind.Event:
                        return GetGlyphFromAccessibility(declaredSymbolInfo.Accessibility, Glyph.EventPrivate, Glyph.EventProtected, Glyph.EventInternal, Glyph.EventPublic);
                    case DeclaredSymbolInfoKind.Field:
                        return GetGlyphFromAccessibility(declaredSymbolInfo.Accessibility, Glyph.FieldPrivate, Glyph.FieldProtected, Glyph.FieldInternal, Glyph.FieldPublic);
                    case DeclaredSymbolInfoKind.Interface:
                        return GetGlyphFromAccessibility(declaredSymbolInfo.Accessibility, Glyph.InterfacePrivate, Glyph.InterfaceProtected, Glyph.InterfaceInternal, Glyph.InterfacePublic);
                    case DeclaredSymbolInfoKind.Module:
                        return GetGlyphFromAccessibility(declaredSymbolInfo.Accessibility, Glyph.ModulePrivate, Glyph.ModuleProtected, Glyph.ModuleInternal, Glyph.ModulePublic);
                    case DeclaredSymbolInfoKind.Indexer:
                    case DeclaredSymbolInfoKind.Property:
                        return GetGlyphFromAccessibility(declaredSymbolInfo.Accessibility, Glyph.PropertyPrivate, Glyph.PropertyProtected, Glyph.PropertyInternal, Glyph.PropertyPublic);
                    case DeclaredSymbolInfoKind.Struct:
                        return GetGlyphFromAccessibility(declaredSymbolInfo.Accessibility, Glyph.StructurePrivate, Glyph.StructureProtected, Glyph.StructureInternal, Glyph.StructurePublic);
                    default:
                        return Glyph.Error;
                }
            }

            private static Glyph GetGlyphFromAccessibility(Accessibility accessibility, Glyph privateGlyph, Glyph protectedGlyph, Glyph internalGlyph, Glyph publicGlyph)
            {
                switch (accessibility)
                {
                    case Accessibility.Private:
                        return privateGlyph;
                    case Accessibility.Protected:
                        return protectedGlyph;
                    case Accessibility.Internal:
                        return internalGlyph;
                    case Accessibility.Public:
                        return publicGlyph;
                    default:
                        throw new ArgumentException(nameof(accessibility));
                }
            }
        }
    }
}
