// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Editor.Navigation;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;

namespace Microsoft.CodeAnalysis.Editor.Implementation.NavigateTo
{
    internal abstract partial class AbstractNavigateToSearchService
    {
        private class SearchResult : INavigateToSearchResult
        {
            public string AdditionalInformation { get; }
            public string Name => _declaredSymbolInfo.Name;
            public string Summary => "TODO: summary"; // declaredNavigableItem.Symbol?.GetDocumentationComment()?.SummaryText);

            public string Kind { get; }
            public MatchKind MatchKind { get; }
            public INavigableItem NavigableItem { get; }
            public string SecondarySort { get; }
            public bool IsCaseSensitive { get; }

            private Document _document;
            private DeclaredSymbolInfo _declaredSymbolInfo;

            public SearchResult(Document document, DeclaredSymbolInfo declaredSymbolInfo, string kind, MatchKind matchKind, bool isCaseSensitive, INavigableItem navigableItem)
            {
                _document = document;
                _declaredSymbolInfo = declaredSymbolInfo;
                Kind = kind;
                MatchKind = matchKind;
                IsCaseSensitive = isCaseSensitive;
                NavigableItem = navigableItem;
                SecondarySort = ConstructSecondarySortString(declaredSymbolInfo);

                var declaredNavigableItem = navigableItem as NavigableItemFactory.DeclaredSymbolNavigableItem;
                Debug.Assert(declaredNavigableItem != null);

                switch (declaredSymbolInfo.Kind)
                {
                    case DeclaredSymbolInfoKind.Class:
                    case DeclaredSymbolInfoKind.Enum:
                    case DeclaredSymbolInfoKind.Interface:
                    case DeclaredSymbolInfoKind.Module:
                    case DeclaredSymbolInfoKind.Struct:
                        AdditionalInformation = EditorFeaturesResources.Project + document.Project.Name;
                        break;
                    default:
                        AdditionalInformation = EditorFeaturesResources.Type + declaredSymbolInfo.ContainerDisplayName;
                        break;
                }
            }

            private static string ConstructSecondarySortString(DeclaredSymbolInfo declaredSymbolInfo)
            {
                var secondarySortString = string.Concat(
                    declaredSymbolInfo.ParameterCount.ToString("X4"),
                    declaredSymbolInfo.TypeParameterCount.ToString("X4"),
                    declaredSymbolInfo.Name);
                return secondarySortString;
            }
        }
    }
}
