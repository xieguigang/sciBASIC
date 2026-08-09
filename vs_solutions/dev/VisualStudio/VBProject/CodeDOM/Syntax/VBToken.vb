#Region "Microsoft.VisualBasic::7f105a42020ac512e30260bd594d3132, vs_solutions\dev\VisualStudio\VBProject\Syntax\VBToken.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 30
    '    Code Lines: 20 (66.67%)
    ' Comment Lines: 6 (20.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (13.33%)
    '     File Size: 718 B


    '     Enum TokenKind
    ' 
    '         [Attribute], [String], CharLiteral, Identifier, Keyword
    '         Number, Punctuation, XmlDoc
    ' 
    '  
    ' 
    ' 
    ' 
    '     Structure Token
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace VBProj.Syntax

    ''' <summary>
    ''' the kind of a lexical token produced by <see cref="VBScanner"/>
    ''' </summary>
    Public Enum TokenKind
        Keyword
        Identifier
        [String]
        CharLiteral
        Number
        Punctuation
        XmlDoc
        [Attribute]
    End Enum

    ''' <summary>
    ''' a single lexical token with its source text and line number
    ''' </summary>
    Public Structure Token

        Public Kind As TokenKind
        Public Text As String
        Public Line As Integer

        Public Overrides Function ToString() As String
            Return $"[{Kind}] {Text}"
        End Function
    End Structure

End Namespace
