// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Organizing;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Organizing
{
    [ExportLanguageService(typeof(ITriviaLogicalOwnershipAssignmentService), LanguageNames.CSharp), Shared]
    internal class CSharpTriviaLogicalOwnershipAssignmentService : ITriviaLogicalOwnershipAssignmentService
    {
        public IEnumerable<Tuple<SyntaxTriviaList, SyntaxTriviaList>> AssignTriviaOwnership<T>(
            SyntaxToken previousToken,
            SeparatedSyntaxList<T> syntaxList,
            SyntaxToken nextToken) where T : SyntaxNode
        {
            IEnumerable<SyntaxTrivia> nextTokensLeadingTrivia = previousToken.TrailingTrivia;
            for (int i = 0; i < syntaxList.Count; i++)
            {
                var node = syntaxList[i];

                // set the node's leading trivia
                // for each node, all trivia after the previous separator's first newline should be prepended to the new node
                var leadingTrivia = nextTokensLeadingTrivia;

                // set the node's trailing trivia
                IEnumerable<SyntaxTrivia> trailingTrivia;
                if (i == syntaxList.Count - 1)
                {
                    // if we're looking at the last node then there is no separator so instead we use the trailing token's leading trivia
                    trailingTrivia = nextToken.LeadingTrivia;

                    // And if the last node is on its own line and the next token is also on that same line, then the next token's
                    // trailing trivia also belongs to the last node.  When determining if the last node is on its own line:
                    //   if there is only one node, it is considered to be on its own line unless the opening token is also on that line
                    //   else if there is a newline anywhere between the penultimate node and the last node
                    var isLastNodeOnOwnLine = syntaxList.Count == 1
                        ? ContainsNewLines(previousToken.TrailingTrivia) || ContainsNewLines(node.GetLeadingTrivia())
                        : ContainsNewLines(syntaxList[i - 1].GetTrailingTrivia()) ||
                          ContainsNewLines(syntaxList[i].GetLeadingTrivia()) ||
                          ContainsNewLines(syntaxList.GetSeparator(i - 1).GetAllTrivia());
                    if (isLastNodeOnOwnLine)
                    {
                        // check if the next token is on the same line as the last node
                        if (!ContainsNewLines(syntaxList[i].GetTrailingTrivia()) && !ContainsNewLines(nextToken.LeadingTrivia))
                        {
                            trailingTrivia = trailingTrivia.Concat(nextToken.TrailingTrivia);
                        }
                    }
                }
                else
                {
                    // get the separator following the node
                    var separator = syntaxList.GetSeparator(i);
                    if (!ContainsNewLines(node.GetTrailingTrivia()) && !ContainsNewLines(separator.LeadingTrivia))
                    {
                        // if the separator starts on the same line as the node ends, all of its leading trivia gets appended to the node
                        trailingTrivia = separator.LeadingTrivia;

                        // take the separator's trailing trivia up to and including the first newline
                        var newlineIndex = separator.TrailingTrivia.IndexOf(SyntaxKind.EndOfLineTrivia);
                        trailingTrivia = trailingTrivia.Concat(separator.TrailingTrivia.Take(newlineIndex + 1));

                        // and the trailing trivia after the newline will become leading trivia on the next node
                        nextTokensLeadingTrivia = separator.TrailingTrivia.Skip(newlineIndex + 1);
                    }
                    else
                    {
                        // otherwise no trivia is appended to the node and it's all saved for the next
                        trailingTrivia = SpecializedCollections.EmptyEnumerable<SyntaxTrivia>();
                        nextTokensLeadingTrivia = separator.LeadingTrivia.Concat(separator.TrailingTrivia);
                    }
                }

                yield return Tuple.Create(leadingTrivia.ToSyntaxTriviaList(), trailingTrivia.ToSyntaxTriviaList());
            }
        }

        public void RemoveNode<T>(
            SyntaxToken previousToken,
            SeparatedSyntaxList<T> syntaxList,
            SyntaxToken nextToken,
            int indexToRemove,
            out SyntaxToken newPreviousToken,
            out SeparatedSyntaxList<T> newSyntaxList,
            out SyntaxToken newNextToken,
            out Tuple<SyntaxTriviaList, SyntaxTriviaList> removedTrivia) where T : SyntaxNode
        {
            if (syntaxList.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(syntaxList));
            }

            if (indexToRemove < 0 || indexToRemove >= syntaxList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(indexToRemove));
            }

            newPreviousToken = previousToken;
            newNextToken = nextToken;

            var triviaParts = AssignTriviaOwnership(previousToken, syntaxList, nextToken).ToArray();

            // build new syntax list
            var newItems = new List<SyntaxNodeOrToken>();
            for (int i = 0; i < syntaxList.Count; i++)
            {
                if (i == indexToRemove)
                {
                    // skip the one we're removing for now
                }
                else
                {
                    // add the leading trivia only if it's not the first node, because that was actually trailing trivia on the previous token
                    var newItem = i == 0
                        ? syntaxList[i]
                        : syntaxList[i].WithLeadingTrivia(triviaParts[i].Item1);
                    newItems.Add(newItem);
                    if (i < syntaxList.Count - 1)
                    {
                        // add the separator with the appropriate trailing trivia
                        newItems.Add(syntaxList.GetSeparator(i).WithTrailingTrivia(triviaParts[i].Item2));
                    }
                }
            }

            // if removing the last item from a multi-item list, promote the last separator's trivia to be trailing on the last item and remove it
            if (indexToRemove == syntaxList.Count - 1 && syntaxList.Count > 1)
            {
                var lastSeparator = newItems.Last().AsToken();
                newItems.RemoveAt(newItems.Count - 1);
                var lastItem = newItems.Last().AsNode();
                newItems.RemoveAt(newItems.Count - 1);
                newItems.Add(lastItem.WithAppendedTrailingTrivia(lastSeparator.LeadingTrivia).WithAppendedTrailingTrivia(lastSeparator.TrailingTrivia));
            }

            if (indexToRemove == 0)
            {
                // remove the previous token's trailing trivia because it was already assigned and applied earlier
                newPreviousToken = previousToken.WithTrailingTrivia();
            }

            var finalTrailingTrivia = newItems.LastOrDefault().AsNode()?.GetTrailingTrivia() ?? new SyntaxTriviaList();
            if (newItems.Any() && (finalTrailingTrivia.Count == 0 || !finalTrailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia)))
            {
                newItems[newItems.Count - 1] = newItems.Last().AsNode().WithAppendedTrailingTrivia(SyntaxFactory.EndOfLine(string.Empty));
            }

            // add trivia from the removed node to the list, unless it's only whitespace
            var removedLeadingTrivia = new SyntaxTriviaList();
            var removedTrailingTrivia = new SyntaxTriviaList();
            var newLeadingTrivia = triviaParts[indexToRemove].Item1
                                    //.Concat(variableDeclarator.GetLeadingTrivia())
                                    //.Concat(variableDeclarator.GetTrailingTrivia())
                                    .Concat(triviaParts[indexToRemove].Item2)
                                    .ToArray();
            if (newLeadingTrivia.Any(t => !t.IsWhitespaceOrEndOfLine()))
            {
                // make sure comments start on their own line by inserting newlines after each comment if not present
                var newLeadingTriviaWithNewLines = new List<SyntaxTrivia>();
                for (int i = 0; i < newLeadingTrivia.Length; i++)
                {
                    newLeadingTriviaWithNewLines.Add(newLeadingTrivia[i]);
                    if (newLeadingTrivia[i].IsRegularOrDocComment() && i < newLeadingTrivia.Length - 1 && newLeadingTrivia[i + 1].Kind() != SyntaxKind.EndOfLineTrivia)
                    {
                        newLeadingTriviaWithNewLines.Add(SyntaxFactory.EndOfLine(string.Empty));
                    }
                }

                removedLeadingTrivia = SyntaxFactory.TriviaList(newLeadingTriviaWithNewLines);
            }

            newSyntaxList = SyntaxFactory.SeparatedList<T>(newItems);
            removedTrivia = Tuple.Create(removedLeadingTrivia, removedTrailingTrivia);
        }

        private static bool ContainsNewLines(IEnumerable<SyntaxTrivia> trivia)
        {
            return trivia.Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia));
        }
    }
}
