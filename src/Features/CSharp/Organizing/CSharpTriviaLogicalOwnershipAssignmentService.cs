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
        public IEnumerable<Tuple<SyntaxTriviaList, SyntaxTriviaList>> AssignTriviaOwnership<T>(SyntaxToken previousToken, SeparatedSyntaxList<T> syntaxList, SyntaxToken nextToken) where T : SyntaxNode
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
                    //   if there is only one node, it's always on its own line
                    //   else if there is a newline anywhere between the penultimate node and the last node
                    var isLastNodeOnOwnLine = syntaxList.Count == 1
                        ? true
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

        private static bool ContainsNewLines(IEnumerable<SyntaxTrivia> trivia)
        {
            return trivia.Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia));
        }
    }
}
