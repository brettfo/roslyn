' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.VisualBasic.Organizing
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.Organizing
    Public Class TriviaOwnershipAssignmentTests
        Private Sub Check(separatedSyntaxList As String, ByVal ParamArray expectedTrivia() As Tuple(Of String, String))
            ' if the last piece of trivia is a single line comment, the closing paren would be swallowed so we have to
            ' add a newline to ensure that doesn't happen which we then trim off later
            Dim argumentList = CType(SyntaxFactory.ParseExpression($"M({separatedSyntaxList}{vbCrLf})"), InvocationExpressionSyntax).ArgumentList
            Dim service = New VisualBasicTriviaLogicalOwnershipAssignmentService()
            Dim assignedTrivia = service.AssignTriviaOwnership(argumentList.OpenParenToken, argumentList.Arguments, argumentList.CloseParenToken).ToArray()
            Assert.Equal(expectedTrivia.Length, assignedTrivia.Length)
            For i = 0 To expectedTrivia.Length - 1
                ' we had to add a newline to the end of the syntax list so we now need to strip it off of the last item
                Dim trailingTrivia = assignedTrivia(i).Item2
                If i = expectedTrivia.Length - 1 Then
                    Assert.Equal(SyntaxKind.EndOfLineTrivia, trailingTrivia.Last().Kind())
                    trailingTrivia = trailingTrivia.RemoveAt(trailingTrivia.Count - 1)
                End If

                Assert.Equal(expectedTrivia(i).Item1, assignedTrivia(i).Item1.ToString())
                Assert.Equal(expectedTrivia(i).Item2, trailingTrivia.ToString())
            Next
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Organizing)>
        Public Sub InlineNoSpaces()
            Check("a,b,c",
                  Tuple.Create("", ""),
                  Tuple.Create("", ""),
                  Tuple.Create("", ""))
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Organizing)>
        Public Sub InlineNoComments()
            Check("a, b, c",
                  Tuple.Create("", ""),
                  Tuple.Create(" ", ""),
                  Tuple.Create(" ", ""))
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Organizing)>
        Public Sub MultiLineNoComments()
            Check("a,
b,
c",
                  Tuple.Create("", vbCrLf),
                  Tuple.Create("", vbCrLf),
                  Tuple.Create("", ""))
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Organizing)>
        Public Sub MultiLineWithComments()
            Check("' a leading
a, ' a trailing
b, ' b trailing
c ' c trailing",
                  Tuple.Create("' a leading" + vbCrLf, " ' a trailing" + vbCrLf),
                  Tuple.Create("", " ' b trailing" + vbCrLf),
                  Tuple.Create("", " ' c trailing"))
        End Sub
    End Class
End Namespace
