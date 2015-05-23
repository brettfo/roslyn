// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Organizing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Organizing
{
    public class TriviaOwnershipAssignmentTests
    {
        private void Check<T>(SyntaxToken previousToken, SeparatedSyntaxList<T> syntaxList, SyntaxToken nextToken, params Tuple<string, string>[] expectedTrivia) where T : SyntaxNode
        {
            var service = new CSharpTriviaLogicalOwnershipAssignmentService();
            var assignedTrivia = service.AssignTriviaOwnership(previousToken, syntaxList, nextToken).ToArray();
            Assert.Equal(expectedTrivia.Length, assignedTrivia.Length);
            for (int i = 0; i < expectedTrivia.Length; i++)
            {
                Assert.Equal(expectedTrivia[i].Item1, assignedTrivia[i].Item1.ToString());
                Assert.Equal(expectedTrivia[i].Item2, assignedTrivia[i].Item2.ToString());
            }
        }

        private void CheckInvocationExpression(string text, params Tuple<string, string>[] expectedTrivia)
        {
            var argumentList = ((InvocationExpressionSyntax)SyntaxFactory.ParseExpression(text)).ArgumentList;
            Check(argumentList.OpenParenToken, argumentList.Arguments, argumentList.CloseParenToken, expectedTrivia);
        }

        private void CheckGenericArguments(string text, params Tuple<string, string>[] expectedTrivia)
        {
            var argumentList = ((GenericNameSyntax)SyntaxFactory.ParseName($"Tuple{text}")).TypeArgumentList;
            Check(argumentList.LessThanToken, argumentList.Arguments, argumentList.GreaterThanToken, expectedTrivia);
        }

        private void CheckParameterList(string text, params Tuple<string, string>[] expectedTrivia)
        {
            var parameterList = SyntaxFactory.ParseParameterList(text);
            Check(parameterList.OpenParenToken, parameterList.Parameters, parameterList.CloseParenToken, expectedTrivia);
        }

        private void CheckLocalDeclarationStatement(string text, params Tuple<string, string>[] expectedTrivia)
        {
            var localDeclaration = (LocalDeclarationStatementSyntax)SyntaxFactory.ParseStatement(text);
            Check(localDeclaration.Declaration.Type.GetLastToken(), localDeclaration.Declaration.Variables, localDeclaration.SemicolonToken, expectedTrivia);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void InlineNoSpaces()
        {
            CheckInvocationExpression("M(a,b,c)",
                  Tuple.Create("", ""),
                  Tuple.Create("", ""),
                  Tuple.Create("", ""));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void InlineNoComments()
        {
            CheckInvocationExpression("M(a, b, c)",
                  Tuple.Create("", ""),
                  Tuple.Create(" ", ""),
                  Tuple.Create(" ", ""));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void MultiLineNoComments()
        {
            CheckInvocationExpression(@"M(a,
b,
c)",
                  Tuple.Create("", "\r\n"),
                  Tuple.Create("", "\r\n"),
                  Tuple.Create("", ""));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void MultiLineWithComments()
        {
            CheckInvocationExpression(@"M(// a leading
a, // a trailing
// b leading
/* b leading again */
b, // b trailing
/* c leading inline */ c // c trailing
)",
                  Tuple.Create("// a leading\r\n", " // a trailing\r\n"),
                  Tuple.Create("", " // b trailing\r\n"), // not expecting any of the 'b leading' comments because they're already leading trivia on the node 'b'
                  Tuple.Create("", "")); // not expecting any of the 'c' comments because they're already directly attached to the nodes
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void SingleLineWithComments()
        {
            CheckInvocationExpression("M(/* a leading */ a /* a trailing */, /* b leading */ b /* b trailing */, /* c leading */ c /* c trailing */)",
                  Tuple.Create("/* a leading */ ", ""), // not expecting '/* a trailing */' because it's already attached to 'a'
                  Tuple.Create(" /* b leading */ ", ""), // not expecting '/* b trailing */' because it's already attached to 'b'
                  Tuple.Create(" /* c leading */ ", "")); // not expecting '/* c trailing */' because it's already attached to 'c'
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void SingleLineTypeArgumentListWithComments()
        {
            CheckGenericArguments("</* int leading */ int /* int trailing*/, /* string leading */ string /* string trailing */>",
                Tuple.Create("/* int leading */ ", ""), // not expecting '/* int trailing */' because it's already attached to 'int'
                Tuple.Create(" /* string leading */ ", "")); // not expecting '/* string trailing */' because it's already attached to 'string'
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void SingleLineParameterListWithComments()
        {
            CheckParameterList("(/* int leading */ int i /* int trailing*/, /* string leading */ string s /* string trailing */)",
                Tuple.Create("/* int leading */ ", ""), // not expecting '/* int trailing */' because it's already attached to 'int'
                Tuple.Create(" /* string leading */ ", "")); // not expecting '/* string trailing */' because it's already attached to 'string'
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void MultiLineLocalDeclarationStatementWithComments1()
        {
            // 'c trailing' is really trailing trivia on the semicolon, but logically, it's tied to 'c'
            CheckLocalDeclarationStatement(@"
int a = 1,
    b = 2,
    c = 3; // c trailing",
                Tuple.Create(" ", "\r\n"),
                Tuple.Create("", "\r\n"),
                Tuple.Create("", " // c trailing"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void MultiLineLocalDeclarationStatementWithComments2()
        {
            // since the 'c' declaration isn't on its own line, the final comment should not be associated with it
            CheckLocalDeclarationStatement(@"
int a = 1,
    b = 2, c = 3; // final trailing",
                Tuple.Create(" ", "\r\n"),
                Tuple.Create("", ""),
                Tuple.Create(" ", ""));
        }
    }
}
