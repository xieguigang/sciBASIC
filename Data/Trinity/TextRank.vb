#Region "Microsoft.VisualBasic::bba5a8d8a1df3c9f1069f946038617da, Data\Trinity\TextRank.vb"

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

    '   Total Lines: 248
    '    Code Lines: 163 (65.73%)
    ' Comment Lines: 50 (20.16%)
    '    - Xml Docs: 82.00%
    ' 
    '   Blank Lines: 35 (14.11%)
    '     File Size: 9.74 KB


    ' Module TextRank
    ' 
    '     Function: IsEmpty, Removes, Sentences, Similarity, StripMessy
    '               TextGraph, TextRankGraph, Words
    ' 
    '     Sub: TextRankGraph
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.PageRank
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text
Imports r = System.Text.RegularExpressions.Regex
Imports stdNum = System.Math

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
        Return text.Split(TextRank.sdeli) _
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

    <Extension> Public Function StripMessy(text$) As String
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
    ''' ##### 使用TextRank提取关键字
    ''' 
    ''' 将原文本拆分为句子，在每个句子中过滤掉停用词（可选），并只保留指定词性的单词（可选）。由此可以得到句子的集合和单词的集合。
    ''' 每个单词作为pagerank中的一个节点。设定窗口大小为k，假设一个句子依次由下面的单词组成
    ''' 
    ''' ```
    ''' w1, w2, w3, w4, w5, ..., wn
    ''' ```
    ''' 
    ''' ``w1, w2, ..., wk``、``w2, w3, ...,wk+1``、``w3, w4, ...,wk+2``等都是一个窗口。在一个窗口中的任两个单词对应的节点之间存在一个无向无权的边。
    ''' 基于上面构成图，可以计算出每个单词节点的重要性。最重要的若干单词可以作为关键词。
    ''' </summary>
    ''' <param name="sentences"></param>
    ''' <returns></returns>
    <Extension>
    Public Function TextRankGraph(sentences As IEnumerable(Of String), Optional win_size% = 2, Optional stopwords As StopWords = Nothing) As GraphMatrix
        Dim g As New Graph
        Dim source As String() = sentences _
            .Select(AddressOf Trim) _
            .Where(Function(s) Not String.IsNullOrEmpty(s)) _
            .ToArray

        stopwords = stopwords Or StopWords.DefaultStopWords

        For Each text As String In source
            ' 假设每一句话之中的单词之间的顺序就是网络连接的方向
            Dim blocks = text _
                .ToLower _
                .Words _
                .Removes(stopwords) _
                .ToArray

            Call g.TextRankGraph(blocks, win_size)
        Next

        Return New GraphMatrix(g)
    End Function

    <Extension>
    Public Sub TextRankGraph(g As Graph, text As String(), Optional win_size% = 2, Optional directed As Boolean = True)
        Dim blocks As SlideWindow(Of String)() = text.SlideWindows(win_size).ToArray

        For Each textBlock As SlideWindow(Of String) In blocks
            For Each word As String In textBlock
                If Not g.ExistVertex(word) Then
                    Call g.AddVertex(word)
                End If
            Next

            If directed Then
                For i As Integer = 0 To textBlock.Length - 1
                    For j As Integer = i To textBlock.Length - 1
                        ' direction is i -> j
                        ' so i should always less than j
                        If i < j Then
                            Dim a = textBlock(i)
                            Dim b = textBlock(j)
                            Dim edge As VertexEdge = g.FindEdge(a, b)

                            If edge Is Nothing Then
                                Call g.AddEdge(a, b)
                            Else
                                edge.weight += 1
                            End If
                        End If
                    Next
                Next
            Else
                For Each combine As (a$, b$) In textBlock.FullCombination
                    If combine.a <> combine.b Then
                        Dim edge As VertexEdge = g.FindEdge(combine.a, combine.b)

                        If edge Is Nothing Then
                            Call g.AddEdge(combine.a, combine.b)
                        Else
                            edge.weight += 1
                        End If
                    End If
                Next
            End If
        Next
    End Sub

    ''' <summary>
    ''' Using for generate article's <see cref="NLPExtensions.Abstract(WeightedPRGraph, Integer, Double)"/>
    ''' </summary>
    ''' <param name="text"></param>
    ''' <param name="similarityCut"></param>
    ''' <returns></returns>
    <Extension>
    Public Function TextGraph(text$, Optional similarityCut# = 0.05) As WeightedPRGraph
        Dim list$() = text.StripMessy.Sentences.ToArray
        Dim words$()() = list _
            .Select(AddressOf LCase) _
            .Select(AddressOf TextRank.Words) _
            .ToArray
        Dim g As New WeightedPRGraph

        For Each sentence As String In list
            Call g.AddVertex(sentence)
        Next

        Call "build text graph...".info

        For Each x As Integer In Tqdm.Range(0, words.Length)
            Dim refIndex = x
            Dim vector = seq(x, words.Length - 1, by:=1) _
                .Select(Function(y)
                            Dim i% = CInt(y)
                            Dim similarity# = TextRank.Similarity(words(refIndex), words(i))
                            Return (y:=i, similarity:=similarity)
                        End Function) _
                .AsParallel _
                .ToArray

            For Each sentence As (i%, similarity#) In vector
                Dim similarity = sentence.similarity
                Dim i% = sentence.i

                If similarity >= similarityCut Then
                    Call g.AddEdge(list(x), list(i), weight:=similarity)
                    Call g.AddEdge(list(i), list(x), weight:=similarity)
                End If
            Next
        Next

        Return g
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

        Dim denominator = stdNum.Log(wordList1.Count) + stdNum.Log(wordList2.Count)

        If stdNum.Abs(denominator) = 0R Then
            Return 0
        End If

        Return coOccurNum / denominator
    End Function
End Module
