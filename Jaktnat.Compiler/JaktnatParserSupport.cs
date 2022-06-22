/*
 * [The "BSD license"]
 *  Copyright (c) 2014 Terence Parr
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *  1. Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 *  3. The name of the author may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 *  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 *  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 *  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 *  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 *  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 *  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 *  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE
 *
 * Converted from Apple's doc, http://tinyurl.com/n8rkoue, to ANTLR's
 * meta-language.
 */

using Antlr4.Runtime;
using Antlr4.Runtime.Sharpen;

namespace Jaktnat.Compiler;

internal static class JaktnatParserSupport
{
    public static readonly BitSet operatorHead = new BitSet(0x10000);
    public static readonly BitSet operatorCharacter;

    public static readonly BitSet leftWS = new BitSet(255);
    public static readonly BitSet rightWS = new BitSet(255);

    static JaktnatParserSupport()
    {
        // operator-head → /  =­  -­  +­  !­  *­  %­  <­  >­  &­  |­  ^­  ~­  ?­
        operatorHead.Set('/');
        operatorHead.Set('=');
        operatorHead.Set('-');
        operatorHead.Set('+');
        operatorHead.Set('!');
        operatorHead.Set('*');
        operatorHead.Set('%');
        operatorHead.Set('&');
        operatorHead.Set('|');
        operatorHead.Set('<');
        operatorHead.Set('>');
        operatorHead.Set('^');
        operatorHead.Set('~');
        operatorHead.Set('?');

        // operator-head → U+00A1–U+00A7
        //operatorHead.Set(0x00A1, 0x00A7 + 1);

        // operator-head → U+00A9 or U+00AB
        operatorHead.Set(0x00A9);
        operatorHead.Set(0x00AB);

        // operator-head → U+00AC or U+00AE
        operatorHead.Set(0x00AC);
        operatorHead.Set(0x00AE);

        // operator-head → U+00B0–U+00B1, U+00B6, U+00BB, U+00BF, U+00D7, or U+00F7
        //operatorHead.Set(0x00B0, 0x00B1 + 1);
        operatorHead.Set(0x00B6);
        operatorHead.Set(0x00BB);
        operatorHead.Set(0x00BF);
        operatorHead.Set(0x00D7);
        operatorHead.Set(0x00F7);

        // operator-head → U+2016–U+2017 or U+2020–U+2027
        //operatorHead.Set(0x2016, 0x2017 + 1);
        //operatorHead.Set(0x2020, 0x2027 + 1);

        // operator-head → U+2030–U+203E
        //operatorHead.Set(0x2030, 0x203E + 1);

        // operator-head → U+2041–U+2053
        //operatorHead.Set(0x2041, 0x2053 + 1);

        // operator-head → U+2055–U+205E
        //operatorHead.Set(0x2055, 0x205E + 1);

        // operator-head → U+2190–U+23FF
        //operatorHead.Set(0x2190, 0x23FF + 1);

        // operator-head → U+2500–U+2775
        //operatorHead.Set(0x2500, 0x2775 + 1);

        // operator-head → U+2794–U+2BFF
        //operatorHead.Set(0x2794, 0x2BFF + 1);

        // operator-head → U+2E00–U+2E7F
        //operatorHead.Set(0x2E00, 0x2E7F + 1);

        // operator-head → U+3001–U+3003
        //operatorHead.Set(0x3001, 0x3003 + 1);

        // operator-head → U+3008–U+3030
        //operatorHead.Set(0x3008, 0x3020 + 1);

        operatorHead.Set(0x3030);

        // operator-character → operator-head­
        operatorCharacter = (BitSet)operatorHead.Clone();

        // operator-character → U+0300–U+036F
        //operatorCharacter.Set(0x0300, 0x036F + 1);
        // operator-character → U+1DC0–U+1DFF
        //operatorCharacter.Set(0x1DC0, 0x1DFF + 1);
        // operator-character → U+20D0–U+20FF
        //operatorCharacter.Set(0x20D0, 0x20FF + 1);
        // operator-character → U+FE00–U+FE0F
        //operatorCharacter.Set(0xFE00, 0xFE0F + 1);
        // operator-character → U+FE20–U+FE2F
        //operatorCharacter.Set(0xFE20, 0xFE2F + 1);
        //operatorCharacter.Set(0xE0100, 0xE01EF + 1);

        // operator-character → U+E0100–U+E01EF
        // Java works with 16-bit unicode chars. However, it can work for targets in other languages, e.g. in Swift
        // operatorCharacter.Set(0xE0100,0xE01EF+1);

        leftWS.Set(JaktnatParser.WHITESPACE);
        leftWS.Set(JaktnatParser.LPAREN);
        //leftWS.Set(JaktnatParser.Interpolataion_multi_line);
        //leftWS.Set(JaktnatParser.Interpolataion_single_line);
        leftWS.Set(JaktnatParser.LBRACKET);
        leftWS.Set(JaktnatParser.LCURLY);
        leftWS.Set(JaktnatParser.COMMA);
        leftWS.Set(JaktnatParser.COLON);
        //leftWS.Set(JaktnatParser.SEMI);

        rightWS.Set(JaktnatParser.WHITESPACE);
        rightWS.Set(JaktnatParser.RPAREN);
        rightWS.Set(JaktnatParser.RBRACKET);
        rightWS.Set(JaktnatParser.RCURLY);
        rightWS.Set(JaktnatParser.COMMA);
        rightWS.Set(JaktnatParser.COLON);
        //rightWS.Set(JaktnatParser.SEMI);
        rightWS.Set(JaktnatParser.LINE_COMMENT);
        //rightWS.Set(JaktnatParser.Block_comment);
    }

    private static bool IsCharacterFromSet(IToken token, BitSet bitSet)
    {
        if (token.Type == TokenConstants.Eof)
        {
            return false;
        }
        else
        {
            var text = token.Text;
            int codepoint = char.ConvertToUtf32(text, 0);
            if (CharCount(codepoint) != text.Length)
            {
                // not a single character
                return false;
            }
            else
            {
                return bitSet.Get(codepoint);
            }
        }
    }

    public static int CharCount(int codePoint) =>
        codePoint >= 0x10000 ? 2 : 1;

    public static bool IsOperatorHead(IToken token)
    {
        return IsCharacterFromSet(token, operatorHead)
               || token.Text is "++" or "--" or "not";
    }

    public static int GetLastOpTokenIndex(ITokenStream tokens)
    {
        int currentTokenIndex = tokens.Index; // current on-channel lookahead token index
        var currentToken = tokens.Get(currentTokenIndex);

        //System.out.println("getLastOpTokenIndex: "+currentToken.getText());


        tokens.GetText(); // Ensures that tokens can be read
        // operator → dot-operator-head­ dot-operator-characters
        if (currentToken.Type == JaktnatParser.DOT && tokens.Get(currentTokenIndex + 1).Type == JaktnatParser.DOT)
        {
            //System.out.println("DOT");


            // dot-operator
            currentTokenIndex += 2; // point at token after ".."
            currentToken = tokens.Get(currentTokenIndex);

            // dot-operator-character → .­ | operator-character­
            while (currentToken.Type == JaktnatParser.DOT || IsOperatorHead(currentToken))
            {
                //System.out.println("DOT");
                currentTokenIndex++;
                currentToken = tokens.Get(currentTokenIndex);
            }

            //System.out.println("result: "+(currentTokenIndex - 1));
            return currentTokenIndex - 1;
        }

        // operator → operator-head­ operator-characters­?

        if (IsOperatorHead(currentToken))
        {
            //System.out.println("isOperatorHead");

            currentToken = tokens.Get(currentTokenIndex);
            while (IsOperatorHead(currentToken))
            {
                //System.out.println("isOperatorCharacter");
                currentTokenIndex++;
                currentToken = tokens.Get(currentTokenIndex);
            }
            //System.out.println("result: "+(currentTokenIndex - 1));
            return currentTokenIndex - 1;
        }
        else
        {
            //System.out.println("result: "+(-1));
            return -1;
        }
    }

    public static bool IsPrefixOp(ITokenStream tokens)
    {
        int stop = GetLastOpTokenIndex(tokens);
        if (stop == -1) return false;

        int start = tokens.Index;
        var prevToken = tokens.Get(start - 1); // includes hidden-channel tokens
        var nextToken = tokens.Get(stop + 1);
        bool prevIsWS = IsLeftOperatorWS(prevToken);
        bool nextIsWS = IsRightOperatorWS(nextToken);

        return prevIsWS && !nextIsWS;
    }

    public static bool IsPostfixOp(ITokenStream tokens)
    {
        int stop = GetLastOpTokenIndex(tokens);
        if (stop == -1) return false;

        int start = tokens.Index;
        var prevToken = tokens.Get(start - 1); // includes hidden-channel tokens
        var nextToken = tokens.Get(stop + 1);
        bool prevIsWS = IsLeftOperatorWS(prevToken);
        bool nextIsWS = IsRightOperatorWS(nextToken);
        return !prevIsWS && nextIsWS ||
            !prevIsWS && nextToken.Type == JaktnatParser.DOT;
    }

    public static bool IsLeftOperatorWS(IToken t)
    {
        return leftWS.Get(t.Type);
    }

    public static bool IsRightOperatorWS(IToken t)
    {
        return rightWS.Get(t.Type) || t.Type == TokenConstants.Eof;
    }
}