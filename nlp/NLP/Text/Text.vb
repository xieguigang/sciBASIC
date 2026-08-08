#Region "Microsoft.VisualBasic::63d0bdb3db1b183684258c8bcc2d312c, nlp\NLP\Text\Text.vb"

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

    '   Total Lines: 94
    '    Code Lines: 65 (69.15%)
    ' Comment Lines: 13 (13.83%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 16 (17.02%)
    '     File Size: 3.24 KB


    ' Module Text
    ' 
    '     Function: IsEmpty, Removes, Sentences, Similarity, StripMessy
    '               Words
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text
Imports r = System.Text.RegularExpressions.Regex
Imports std = System.Math

Public Module Text

    ''' <summary>
    ''' Delimiter that using for split the large text block into seperated sentenses.
    ''' </summary>
    ReadOnly sdeli As Char() = {"."c, "?"c, "!"c, ";"c}
    ''' <summary>
    ''' Split text as words
    ''' </summary>
    ReadOnly allSymbols As Char() = ASCII.Symbols.AsList + {" "c, ASCII.TAB}

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Sentences(text$) As String()
        Return text.Split(sdeli) _
            .Select(AddressOf Trim) _
            .Where(Function(s) Not s.StringEmpty) _
            .ToArray
    End Function

    Public Function IsEmpty(str As String) As Boolean
        If Strings.Trim(str).StringEmpty Then
            Return True
        End If

        Static symbols As Index(Of Char) = ASCII.Symbols _
            .JoinIterates({" "c, ASCII.TAB, ASCII.CR, ASCII.LF}) _
            .Indexing

        If str.All(Function(c) c Like symbols) Then
            Return True
        Else
            Return False
        End If
    End Function

    <Extension>
    Public Function StripMessy(text$) As String
        text = r.Replace(text, "\s+", " ")

        Return text
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Words(text$) As String()
        Return text _
            .Split(allSymbols) _
            .Where(Function(s) Not String.IsNullOrEmpty(s)) _
            .ToArray
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Removes(words As IEnumerable(Of String), stopwords As StopWords) As IEnumerable(Of String)
        Return stopwords.Removes(words)
    End Function

    ''' <summary>
    ''' 默认的用于计算两个句子相似度的函数。
    ''' </summary>
    ''' <param name="wordList1">分别代表两个句子，都是由单词组成的列表</param>
    ''' <param name="wordList2">分别代表两个句子，都是由单词组成的列表</param>
    ''' <returns></returns>
    Public Function Similarity(wordList1$(), wordList2$()) As Double
        Dim words$() = (wordList1.AsList + wordList2) _
            .Distinct _
            .ToArray
        Dim vector1 As New Vector(From word As String In words Select wordList1.Count(word))
        Dim vector2 As New Vector(From word As String In words Select wordList2.Count(word))

        ' 使用乘法计算出共同出现的单词的数量
        Dim vector3 = vector1 * vector2
        Dim coOccurNum = vector3.Where(Function(n) n > 0).Count

        If coOccurNum <= 0 Then
            Return 0
        End If

        Dim denominator = std.Log(wordList1.Count) + std.Log(wordList2.Count)

        If std.Abs(denominator) = 0R Then
            Return 0
        End If

        Return coOccurNum / denominator
    End Function
End Module

