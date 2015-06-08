' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis.Host.Mef
Imports Microsoft.CodeAnalysis.Organizing

Namespace Microsoft.CodeAnalysis.VisualBasic.Organizing
    <ExportLanguageService(GetType(ITriviaLogicalOwnershipAssignmentService), LanguageNames.VisualBasic), [Shared]>
    Friend Class VisualBasicTriviaLogicalOwnershipAssignmentService
        Implements ITriviaLogicalOwnershipAssignmentService

        Public Iterator Function AssignTriviaOwnership(Of T As SyntaxNode)(previousToken As SyntaxToken, syntaxList As SeparatedSyntaxList(Of T), nextToken As SyntaxToken) As IEnumerable(Of Tuple(Of SyntaxTriviaList, SyntaxTriviaList)) Implements ITriviaLogicalOwnershipAssignmentService.AssignTriviaOwnership
            Dim nextTokensLeadingTrivia As IEnumerable(Of SyntaxTrivia) = previousToken.TrailingTrivia
            For i = 0 To syntaxList.Count - 1
                Dim node = syntaxList(i)

                ' set the node's leading trivia
                ' for each node, all trivia after the previous separator's first newline should be prepended to the new node
                Dim leadingTrivia = nextTokensLeadingTrivia

                ' set the node's trailing trivia
                Dim trailingTrivia As IEnumerable(Of SyntaxTrivia)
                If i = syntaxList.Count - 1 Then
                    ' if we're looking at the last node then there is no separator so instead we use the trailing token's leading trivia
                    trailingTrivia = nextToken.LeadingTrivia
                Else
                    ' get the separator following the node
                    Dim separator = syntaxList.GetSeparator(i)
                    If Not ContainsNewLines(node.GetTrailingTrivia()) AndAlso Not ContainsNewLines(separator.LeadingTrivia) Then
                        ' if the separator starts on the same line as the node ends, all of its leading trivia gets appended to the node
                        trailingTrivia = separator.LeadingTrivia

                        ' take the separator's trailing trivia up to and including the first newline
                        Dim newlineIndex = separator.TrailingTrivia.IndexOf(SyntaxKind.EndOfLineTrivia)
                        trailingTrivia = trailingTrivia.Concat(separator.TrailingTrivia.Take(newlineIndex + 1))

                        ' and the trailing trivia after the newline will become leading trivia on the next node
                        nextTokensLeadingTrivia = separator.TrailingTrivia.Skip(newlineIndex + 1)
                    Else
                        ' otherwise no trivia is appended to the node and it's all saved for the next
                        trailingTrivia = SpecializedCollections.EmptyEnumerable(Of SyntaxTrivia)
                        nextTokensLeadingTrivia = separator.LeadingTrivia.Concat(separator.TrailingTrivia)
                    End If
                End If

                Yield Tuple.Create(leadingTrivia.ToSyntaxTriviaList(), trailingTrivia.ToSyntaxTriviaList())
            Next
        End Function

        Private Shared Function ContainsNewLines(trivia As SyntaxTriviaList) As Boolean
            Return trivia.Any(Function(t) t.IsKind(SyntaxKind.EndOfLineTrivia))
        End Function

        Public Sub RemoveNode(Of T As SyntaxNode)(previousToken As SyntaxToken, syntaxList As SeparatedSyntaxList(Of T), nextToken As SyntaxToken, indexToRemove As Integer, ByRef newPreviousToken As SyntaxToken, ByRef newSyntaxList As SeparatedSyntaxList(Of T), ByRef newNextToken As SyntaxToken, ByRef removedTrivia As Tuple(Of SyntaxTriviaList, SyntaxTriviaList)) Implements ITriviaLogicalOwnershipAssignmentService.RemoveNode
            Throw New NotImplementedException()
        End Sub
    End Class

End Namespace
