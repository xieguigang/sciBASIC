#Region "Microsoft.VisualBasic::b0577f2b7be747f6ebe98b71dc6fdfc3, vs_solutions\JavaScript\Language\TokenType.vb"

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

    '   Total Lines: 16
    '    Code Lines: 10 (62.50%)
    ' Comment Lines: 5 (31.25%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (6.25%)
    '     File Size: 531 B


    ' Enum TokenType
    ' 
    '     [String], Eof, Identifier, Keyword, Number
    '     Punct, Regex, Template
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region


''' <summary>Token categories produced by <see cref="Lexer"/>.</summary>
Public Enum TokenType
    Number
    [String]
    Identifier
    Keyword
    Punct
    ''' <summary>Template literal `` `a${b}c` ``; <see cref="Token.Value"/> holds
    ''' the <see cref="TemplateChunk"/> list (cooked text + expression sources).</summary>
    Template
    ''' <summary>Regular-expression literal <c>/pattern/flags</c>; <see cref="Token.Value"/>
    ''' holds a source/flags pair object.</summary>
    Regex
    Eof
End Enum

