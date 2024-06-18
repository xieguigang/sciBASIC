#Region "Microsoft.VisualBasic::ca2fadfb98bd62bb591752f6f03c675f, Data\word2vec\VectorModel.vb"

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

'   Total Lines: 197
'    Code Lines: 113 (57.36%)
' Comment Lines: 45 (22.84%)
'    - Xml Docs: 95.56%
' 
'   Blank Lines: 39 (19.80%)
'     File Size: 6.55 KB


' Class VectorModel
' 
'     Properties: vectorSize, wordMap, words
' 
'     Constructor: (+2 Overloads) Sub New
'     Function: analogy, GenericEnumerator, getWordVector, (+2 Overloads) similar, ToString
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' the word embedding vector set
''' </summary>
Public Class VectorModel : Implements Enumeration(Of NamedCollection(Of Single))

    ''' <summary>
    ''' the word embedding vector set
    ''' </summary>
    ''' <returns></returns>
    Public Property wordMap As New Dictionary(Of String, Single())

    ''' <summary>
    ''' the number of features, or the dimension of the word embedding vector
    ''' </summary>
    ''' <returns></returns>
    Public Property vectorSize As Integer = 200

    ''' <summary>
    ''' the number of the word tokens insdie the <see cref="wordMap"/>.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property words As Integer
        Get
            Return wordMap.TryCount
        End Get
    End Property

    ''' <summary>
    ''' 私有构造函数 </summary>
    ''' <param name="wordMap"> 词向量哈希表 </param>
    ''' <param name="vectorSize"> 词向量长度 </param>
    Public Sub New(wordMap As IDictionary(Of String, Single()), vectorSize As Integer)
        Me.wordMap = New Dictionary(Of String, Single())(wordMap)
        Me.vectorSize = vectorSize
    End Sub

    Sub New()
    End Sub

    ''' <summary>
    ''' 获取与词word最相近topNSize个词 </summary>
    ''' <param name="queryWord"> 词 </param>
    ''' <param name="topNSize">
    ''' 获取最相似词的数量
    ''' </param>
    ''' <returns> 相近词集，若模型不包含词word，则返回空集 </returns>
    Public Function similar(queryWord As String, Optional topNSize As Integer = 40) As IEnumerable(Of WordScore)
        Dim center = wordMap.GetValueOrNull(queryWord)

        If center Is Nothing Then
            Return {}
        End If

        Dim resultSize = If(wordMap.Count < topNSize, wordMap.Count, topNSize + 1)
        Dim result As New SortedSet(Of WordScore)()

        For i = 0 To resultSize - 1
            result.Add(New WordScore("^_^", -Single.MaxValue))
        Next

        Dim minDist = -Single.MaxValue

        For Each entry In wordMap.SetOfKeyValuePairs()
            Dim vector = entry.Value
            Dim dist As Single = 0

            For i = 0 To vector.Length - 1
                dist += center(i) * vector(i)
            Next

            If dist > minDist Then
                result.Add(New WordScore(entry.Key, dist))
                minDist = result.PollLast().score
            End If
        Next

        result.PollFirst()
        Return result
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="center"></param>
    ''' <param name="topNSize">
    ''' 获取最相似词的数量
    ''' </param>
    ''' <returns></returns>
    Public Function similar(center As Single(), Optional topNSize As Integer = 40) As IEnumerable(Of WordScore)
        If center Is Nothing OrElse center.Length <> vectorSize Then
            Return {}
        End If

        Dim resultSize = If(wordMap.Count < topNSize, wordMap.Count, topNSize)
        Dim result As New SortedSet(Of WordScore)()

        For i = 0 To resultSize - 1
            result.Add(New WordScore("^_^", -Single.MaxValue))
        Next

        Dim minDist = -Single.MaxValue

        For Each entry In wordMap.SetOfKeyValuePairs()
            Dim vector = entry.Value
            Dim dist As Single = 0

            For i = 0 To vector.Length - 1
                dist += center(i) * vector(i)
            Next

            If dist > minDist Then
                result.Add(New WordScore(entry.Key, dist))
                minDist = result.PollLast().score
            End If
        Next
        '        result.pollFirst();

        Return result
    End Function

    ''' <summary>
    ''' 词迁移，即word1 - word0 + word2 的结果，若三个词中有一个不在模型中，
    ''' 也就是没有词向量，则返回空集 </summary>
    ''' <param name="word0"> 词 </param>
    ''' <param name="word1"> 词 </param>
    ''' <param name="word2"> 词 </param>
    ''' <param name="topNSize">
    ''' 获取最相似词的数量
    ''' </param>
    ''' <returns> 与结果最相近的前topNSize个词 </returns>
    Public Function analogy(word0 As String, word1 As String, word2 As String, Optional topNSize As Integer = 40) As SortedSet(Of WordScore)
        Dim wv0 = wordMap.GetValueOrNull(word0)
        Dim wv1 = wordMap.GetValueOrNull(word1)
        Dim wv2 = wordMap.GetValueOrNull(word2)

        If wv1 Is Nothing OrElse wv2 Is Nothing OrElse wv0 Is Nothing Then
            Return Nothing
        End If

        Dim center = New Single(vectorSize - 1) {}

        For i = 0 To vectorSize - 1
            center(i) = wv1(i) - wv0(i) + wv2(i)
        Next

        Dim resultSize = If(wordMap.Count < topNSize, wordMap.Count, topNSize)
        Dim result As SortedSet(Of WordScore) = New SortedSet(Of WordScore)()

        For i = 0 To resultSize - 1
            result.Add(New WordScore("^_^", -Single.MaxValue))
        Next

        Dim name As String
        Dim minDist = -Single.MaxValue

        For Each entry In wordMap.SetOfKeyValuePairs()
            name = entry.Key

            If name.Equals(word1) OrElse name.Equals(word2) Then
                Continue For
            End If

            Dim vector = entry.Value
            Dim dist As Single = 0

            For i = 0 To vector.Length - 1
                dist += center(i) * vector(i)
            Next

            If dist > minDist Then
                result.Add(New WordScore(entry.Key, dist))
                minDist = result.PollLast().score
            End If
        Next

        Return result
    End Function

    Public Function getWordVector(word As String) As Single()
        Return wordMap.GetValueOrNull(word)
    End Function

    Public Overrides Function ToString() As String
        Return wordMap.GetJson
    End Function

    Public Iterator Function GenericEnumerator() As IEnumerator(Of NamedCollection(Of Single)) Implements Enumeration(Of NamedCollection(Of Single)).GenericEnumerator
        For Each word In wordMap
            Yield New NamedCollection(Of Single)(word.Key, word.Value)
        Next
    End Function
End Class
