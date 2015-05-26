// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Organizing
{
    public interface ITriviaLogicalOwnershipAssignmentService
    {
        IEnumerable<Tuple<SyntaxTriviaList, SyntaxTriviaList>> AssignTriviaOwnership<T>(SyntaxToken previousToken, SeparatedSyntaxList<T> syntaxList, SyntaxToken nextToken) where T : SyntaxNode;
        void RemoveNode<T>(
            SyntaxToken previousToken,
            SeparatedSyntaxList<T> syntaxList,
            SyntaxToken nextToken,
            int indexToRemove,
            out SyntaxToken newPreviousToken,
            out SeparatedSyntaxList<T> newSyntaxList,
            out SyntaxToken newNextToken,
            out Tuple<SyntaxTriviaList, SyntaxTriviaList> removedTrivia) where T : SyntaxNode;
    }
}
