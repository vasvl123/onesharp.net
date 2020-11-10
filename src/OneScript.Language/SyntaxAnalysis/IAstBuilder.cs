/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    public interface IAstBuilder
    {
        BslSyntaxNode CreateNode(NodeKind kind, in Lexem startLexem);

        void AddChild(BslSyntaxNode parent, BslSyntaxNode child);

        void HandleParseError(in ParseError error, in Lexem lexem, ILexemGenerator lexer);
    }
}