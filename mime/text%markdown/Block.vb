#Region "Microsoft.VisualBasic::a79503a53d136ac385c967f97a0a6907, sciBASIC#\mime\text%markdown\Block.vb"

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

    '   Total Lines: 29
    '    Code Lines: 22
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 1.29 KB


    ' Module Block
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Block-Level Grammar
''' </summary>
Module Block

    Public Const newline = "^\n+"
    Public Const code = "^( {4}[^\n]+\n*)+"
    Public Const fences = "noop"
    Public Const hr = "^( *[-*_]){3,} *(?:\n+|$)"
    Public Const heading = "^ *(#{1,6}) *([^\n]+?) *#* *(?:\n+|$)"
    Public Const nptable = "noop"
    Public Const lheading = "^([^\n]+)\n *(=|-){2,} *(?\n+|$)"
    Public Const blockquote = "^( *>[^\n]+(\n(?!def)[^\n]+)*\n*)+"
    Public Const list = "^( *)(bull) [\s\S]+?(?hr|def|\n{2,}(?! )(?!\1bull )\n*|\s*$)"
    Public Const html = "^ *(?:comment *(?:\n|\s*$)|closed *(?:\n{2,}|\s*$)|closing *(?:\n{2,}|\s*$))"
    Public Const def = "^ *\[([^\]]+)\] *<?([^\s>]+)>?(?: +[""(]([^\n]+)["")])? *(?:\n+|$)"
    Public Const table = "noop"
    Public Const paragraph = "^((?:[^\n]+\n?(?!hr|heading|lheading|blockquote|tag|def))+)\n*"
    Public Const text = "^[^\n]+"

    Public Const bullet = "(?[*+-]|\d+\.)"
    Public Const item = "^( *)(bull) [^\n]*(?:\n(?!\1bull )[^\n]*)*"

    Public Const tag As String = "(?!(?:" &
        "a|em|strong|small|s|cite|q|dfn|abbr|data|time|code" &
        "|var|samp|kbd|sub|sup|i|b|u|mark|ruby|rt|rp|bdi|bdo" &
        "|span|br|wbr|ins|del|img)\\b)\\w+(?!:/|[^\\w\\s@]*@)\\b"

End Module
