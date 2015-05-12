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
        private void Check(string separatedSyntaxList, params Tuple<string, string>[] expectedTrivia)
        {
            // if the last piece of trivia is a single line comment, the closing paren would be swallowed so we have to
            // add a newline to ensure that doesn't happen which we then trim off later
            var argumentList = ((InvocationExpressionSyntax)SyntaxFactory.ParseExpression($"M({separatedSyntaxList}\r\n)")).ArgumentList;
            var service = new CSharpTriviaLogicalOwnershipAssignmentService();
            var assignedTrivia = service.AssignTriviaOwnership(argumentList.OpenParenToken, argumentList.Arguments, argumentList.CloseParenToken).ToArray();
            Assert.Equal(expectedTrivia.Length, assignedTrivia.Length);
            for (int i = 0; i < expectedTrivia.Length; i++)
            {
                // we had to add a newline to the end of the syntax list so we now need to strip it off of the last item
                var trailingTrivia = assignedTrivia[i].Item2;
                if (i == expectedTrivia.Length - 1)
                {
                    Assert.Equal(SyntaxKind.EndOfLineTrivia, trailingTrivia.Last().Kind());
                    trailingTrivia = trailingTrivia.RemoveAt(trailingTrivia.Count - 1);
                }

                Assert.Equal(expectedTrivia[i].Item1, assignedTrivia[i].Item1.ToString());
                Assert.Equal(expectedTrivia[i].Item2, trailingTrivia.ToString());
            }
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void InlineNoSpaces()
        {
            Check("a,b,c",
                  Tuple.Create("", ""),
                  Tuple.Create("", ""),
                  Tuple.Create("", ""));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void InlineNoComments()
        {
            Check("a, b, c",
                  Tuple.Create("", ""),
                  Tuple.Create(" ", ""),
                  Tuple.Create(" ", ""));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void MultiLineNoComments()
        {
            Check(@"a,
b,
c",
                  Tuple.Create("", "\r\n"),
                  Tuple.Create("", "\r\n"),
                  Tuple.Create("", ""));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void MultiLineWithComments()
        {
            Check(@"// a leading
a, // a trailing
// b leading
/* b leading again */
b, // b trailing
/* c leading inline */ c // c trailing",
                  Tuple.Create("// a leading\r\n", " // a trailing\r\n"),
                  Tuple.Create("// b leading\r\n/* b leading again */\r\n", " // b trailing\r\n"),
                  Tuple.Create("/* c leading inline */ ", " // c trailing"));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Organizing)]
        public void SingleLineWithComments()
        {
            Check("/* a leading */ a /* a trailing */, /* b leading */ b /* b trailing */, /* c leading */ c /* c trailing */",
                  Tuple.Create("/* a leading */ ", " /* a trailing */"),
                  Tuple.Create(" /* b leading */ ", " /* b trailing */"),
                  Tuple.Create(" /* c leading */ ", " /* c trailing */"));
        }
    }
}
