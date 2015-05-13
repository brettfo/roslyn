' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.VisualBasic.Organizing
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.Organizing
    Public Class TriviaOwnershipAssignmentTests
        Private Sub Check(Of T As SyntaxNode)(previousToken As SyntaxToken, syntaxList As SeparatedSyntaxList(Of T), nextToken As SyntaxToken, ByVal ParamArray expectedTrivia() As Tuple(Of String, String))
            Dim service = New VisualBasicTriviaLogicalOwnershipAssignmentService()
            Dim assignedTrivia = service.AssignTriviaOwnership(previousToken, syntaxList, nextToken).ToArray()
            Assert.Equal(expectedTrivia.Length, assignedTrivia.Length)
            For i = 0 To expectedTrivia.Length - 1
                Assert.Equal(expectedTrivia(i).Item1, assignedTrivia(i).Item1.ToString())
                Assert.Equal(expectedTrivia(i).Item2, assignedTrivia(i).Item2.ToString())
            Next
        End Sub

        Private Sub CheckInvocationExpression(text As String, ByVal ParamArray expectedTrivia() As Tuple(Of String, String))
            Dim argumentList = CType(SyntaxFactory.ParseExpression(text), InvocationExpressionSyntax).ArgumentList
            Check(argumentList.OpenParenToken, argumentList.Arguments, argumentList.CloseParenToken, expectedTrivia)
        End Sub

        Private Sub CheckGenericArguments(text As String, ByVal ParamArray expectedTrivia() As Tuple(Of String, String))
            Dim argumentList = CType(SyntaxFactory.ParseName($"Tuple{text}"), GenericNameSyntax).TypeArgumentList
            Check(argumentList.OfKeyword, argumentList.Arguments, argumentList.CloseParenToken, expectedTrivia)
        End Sub

        Private Sub CheckParameterList(text As String, ByVal ParamArray expectedTrivia() As Tuple(Of String, String))
            Dim parameterList = SyntaxFactory.ParseParameterList(text)
            Check(parameterList.OpenParenToken, parameterList.Parameters, parameterList.CloseParenToken, expectedTrivia)
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Organizing)>
        Public Sub InlineNoSpaces()
            CheckInvocationExpression("M(a,b,c)",
                Tuple.Create("", ""),
                Tuple.Create("", ""),
                Tuple.Create("", ""))
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Organizing)>
        Public Sub InlineNoComments()
            CheckInvocationExpression("M(a, b, c)",
                Tuple.Create("", ""),
                Tuple.Create(" ", ""),
                Tuple.Create(" ", ""))
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Organizing)>
        Public Sub MultiLineNoComments()
            CheckInvocationExpression("M(a,
b,
c)",
                Tuple.Create("", vbCrLf),
                Tuple.Create("", vbCrLf),
                Tuple.Create("", ""))
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Organizing)>
        Public Sub MultiLineWithComments()
            CheckInvocationExpression("M(' a leading
a, ' a trailing
b, ' b trailing
c
)",
                Tuple.Create("' a leading" + vbCrLf, " ' a trailing" + vbCrLf),
                Tuple.Create("", " ' b trailing" + vbCrLf),
                Tuple.Create("", "")) ' not expecting ['c trailing] because it's already attached to the node [c]
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Organizing)>
        Public Sub TypeArgumentList()
            CheckGenericArguments("(Of ' Integer leading
Integer,
String)",
                Tuple.Create(" ' Integer leading" + vbCrLf, vbCrLf),
                Tuple.Create("", ""))
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Organizing)>
        Public Sub ParameterList()
            CheckParameterList("( ' i leading
i As Integer, ' i trailing
s As String)",
                Tuple.Create(" ' i leading" + vbCrLf, " ' i trailing" + vbCrLf),
                Tuple.Create("", ""))
        End Sub
    End Class
End Namespace
