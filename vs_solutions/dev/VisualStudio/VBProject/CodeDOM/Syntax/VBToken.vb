#Region "Microsoft.VisualBasic::0c82a66474141de3718684bb9020552a, vs_solutions\dev\VisualStudio\VBProject\CodeDOM\Syntax\VBToken.vb"

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

    '   Total Lines: 36
    '    Code Lines: 20 (55.56%)
    ' Comment Lines: 11 (30.56%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 5 (13.89%)
    '     File Size: 992 B


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

Namespace VBProj.CodeDOM.Syntax

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
    ''' a single lexical token with its source text and line number.
    '''
    ''' <see cref="Line"/> holds the 1-based physical line of the logical
    ''' statement this token belongs to (a statement may span several physical
    ''' lines after continuation merging); it is not the exact line of the
    ''' token itself.
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
