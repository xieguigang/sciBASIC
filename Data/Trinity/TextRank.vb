#Region "Microsoft.VisualBasic::7d917a9aae1f227954924417a5286616, ..\sciBASIC#\Data\Trinity\TextRank.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Data.Graph.Analysis.PageRank
Imports Microsoft.VisualBasic.Text

''' <summary>
''' This module implements TextRank, an unsupervised keyword
''' significance scoring algorithm. TextRank builds a weighted
''' graph representation Of a document Using words As nodes
''' And coocurrence frequencies between pairs of words as edge 
''' weights.It then applies PageRank to this graph, And
''' treats the PageRank score of each word as its significance.
''' The original research paper proposing this algorithm Is
''' available here
'''
''' > https://web.eecs.umich.edu/~mihalcea/papers/mihalcea.emnlp04.pdf
''' 
''' </summary>
Public Module TextRank

    ReadOnly sdeli As Char() = {"."c, "?"c, "!"c, ";"c}
    ReadOnly allSymbols As Char() = ASCII.Symbols.Join({" "c, ASCII.TAB})

    <Extension>
    Public Function Sentences(text$) As String()
        Return text.Split(TextRank.sdeli)
    End Function

    <Extension>
    Public Function Words(text$) As String()
        Return text _
            .Split(allSymbols) _
            .Where(Function(s) Not String.IsNullOrEmpty(s)) _
            .ToArray
    End Function

    <Extension>
    Public Function TextGraph(sentences As IEnumerable(Of String)) As GraphMatrix
        Dim g As New Graph.Graph
        Dim source As String() = sentences _
            .Select(AddressOf Trim) _
            .Where(Function(s) Not String.IsNullOrEmpty(s)) _
            .ToArray

        For Each text As String In source

            ' 假设每一句话之中的单词之间的顺序就是网络连接的方向
            Dim words = text _
                .ToLower _
                .Words _
                .SlideWindows(2).ToArray

            For Each t In words

                For Each s In t
                    If Not g.ExistVertex(s) Then
                        Call g.AddVertex(s)
                    End If
                Next

                Call g.AddEdge(t.First, t.Last)
            Next
        Next

        Return New GraphMatrix(g, skipCount:=False)
    End Function
End Module
