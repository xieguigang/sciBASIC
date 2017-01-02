Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
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
        Dim net As New Network
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
                Call net.AddEdges(t.First, {t.Last})
            Next
        Next

        Return New GraphMatrix(net)
    End Function
End Module
