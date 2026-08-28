#Region "Microsoft.VisualBasic::203209fed71f4749cfa884d29cc6aa31, Microsoft.VisualBasic.Core\src\Data\Trinity\NLP\TextSplit.vb"

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
    '    Code Lines: 23 (63.89%)
    ' Comment Lines: 6 (16.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (19.44%)
    '     File Size: 1.34 KB


    '     Module TextSplit
    ' 
    '         Function: MakeWords
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace Data.Trinity.NLP

    Public Module TextSplit

        ''' <summary>
        ''' 自定义分词器：兼容英文/数字词与中文（CJK）按单字切分。
        ''' 中英文统一小写、去首尾空白后：
        ''' - 连续的 [a-z0-9] 视为一个英文/数字词；
        ''' - 连续的汉字逐个作为 token（与 <see cref="Search"/> 对查询关键词采用同一分词逻辑，保证粒度一致）。
        ''' </summary>
        Public Function MakeWords(text As String) As IEnumerable(Of String)
            If String.IsNullOrWhiteSpace(text) Then
                Return Enumerable.Empty(Of String)()
            End If

            text = Strings.Trim(text).ToLower
            Dim tokens As New List(Of String)

            For Each m As Match In Regex.Matches(text, "[a-z0-9]+")
                tokens.Add(m.Value)
            Next

            For Each m As Match In Regex.Matches(text, "[一-鿿]+")
                Dim s As String = m.Value
                For i As Integer = 0 To s.Length - 1
                    tokens.Add(Char.ToString(s(i)))
                Next
            Next

            Return tokens.Distinct()
        End Function
    End Module
End Namespace
