﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Editor.Implementation.Highlighting
{
    internal abstract class AbstractKeywordHighlighter<TNode> : AbstractKeywordHighlighter where TNode : SyntaxNode
    {
        protected sealed override bool IsHighlightableNode(SyntaxNode node) => node is TNode;

        protected sealed override void AddHighlightsForNode(SyntaxNode node, List<TextSpan> highlights, CancellationToken cancellationToken)
            => AddHighlights((TNode)node, highlights, cancellationToken);

        protected abstract void AddHighlights(TNode node, List<TextSpan> highlights, CancellationToken cancellationToken);
    }

    internal abstract class AbstractKeywordHighlighter : IHighlighter
    {
        private static readonly ObjectPool<List<TextSpan>> s_textSpanListPool = new ObjectPool<List<TextSpan>>(() => new List<TextSpan>());
        private static readonly ObjectPool<List<SyntaxToken>> s_tokenListPool = new ObjectPool<List<SyntaxToken>>(() => new List<SyntaxToken>());

        protected abstract bool IsHighlightableNode(SyntaxNode node);

        public void AddHighlights(
            SyntaxNode root, int position, List<TextSpan> highlights, CancellationToken cancellationToken)
        {
            using var highlightsListPooledObject = s_textSpanListPool.GetPooledObject();
            using var tokensListPooledObject = s_tokenListPool.GetPooledObject();

            var tempHighlights = highlightsListPooledObject.Object;
            var touchingTokens = tokensListPooledObject.Object;
            AddTouchingTokens(root, position, touchingTokens);

            foreach (var token in touchingTokens)
            {
                for (var parent = token.Parent; parent != null; parent = parent.Parent)
                {
                    if (IsHighlightableNode(parent))
                    {
                        tempHighlights.Clear();
                        AddHighlightsForNode(parent, tempHighlights, cancellationToken);

                        if (AnyIntersects(position, tempHighlights))
                        {
                            highlights.AddRange(tempHighlights);
                            return;
                        }
                    }
                }
            }
        }

        private static bool AnyIntersects(int position, List<TextSpan> highlights)
        {
            foreach (var highlight in highlights)
            {
                if (highlight.IntersectsWith(position))
                {
                    return true;
                }
            }

            return false;
        }

        protected abstract void AddHighlightsForNode(SyntaxNode node, List<TextSpan> highlights, CancellationToken cancellationToken);

        protected TextSpan EmptySpan(int position)
        {
            return new TextSpan(position, 0);
        }

        internal static void AddTouchingTokens(SyntaxNode root, int position, List<SyntaxToken> tokens)
        {
            AddTouchingTokens(root, position, tokens, findInsideTrivia: true);
            AddTouchingTokens(root, position, tokens, findInsideTrivia: false);
        }

        private static void AddTouchingTokens(SyntaxNode root, int position, List<SyntaxToken> tokens, bool findInsideTrivia)
        {
            var token = root.FindToken(position, findInsideTrivia);
            if (!tokens.Contains(token))
                tokens.Add(token);

            if (position == 0)
                return;

            var previous = root.FindToken(position - 1, findInsideTrivia);
            if (previous.Span.End == position && !tokens.Contains(previous))
                tokens.Add(previous);
        }
    }
}
