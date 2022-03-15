#Region "Microsoft.VisualBasic::3e9594161dc849d35076616d9837eb1f, sciBASIC#\Data\word2vec\VectorModel.vb"

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

    '   Total Lines: 273
    '    Code Lines: 176
    ' Comment Lines: 35
    '   Blank Lines: 62
    '     File Size: 9.25 KB


    '     Class VectorModel
    ' 
    '         Properties: topNSize, vectorSize, wordMap
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: analogy, getWordVector, loadFromFile, (+2 Overloads) similar
    ' 
    '         Sub: saveModel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports stdNum = System.Math

Namespace NlpVec

    ''' <summary>
    ''' User: fangy
    ''' Date: 13-12-9
    ''' Time: 下午2:30
    ''' </summary>
    Public Class VectorModel

        Public Property wordMap As New Dictionary(Of String, Single())

        ''' <summary>
        ''' 获取最相似词的数量 </summary>
        ''' <returns> 最相似词的数量 </returns>
        Public Property topNSize As Integer = 40

        ''' <summary>
        ''' 特征数
        ''' </summary>
        ''' <returns></returns>
        Public Property vectorSize As Integer = 200

        ''' <summary>
        ''' 私有构造函数 </summary>
        ''' <param name="wordMap"> 词向量哈希表 </param>
        ''' <param name="vectorSize"> 词向量长度 </param>
        Public Sub New(wordMap As IDictionary(Of String, Single()), vectorSize As Integer)
            If wordMap Is Nothing OrElse wordMap.Count = 0 Then
                Throw New ArgumentException("word2vec的词向量为空，请先训练模型。")
            End If

            If vectorSize <= 0 Then
                Throw New ArgumentException("词向量长度（layerSize）应大于0")
            End If

            Me.wordMap = wordMap
            Me.vectorSize = vectorSize
        End Sub


        ''' <summary>
        ''' 使用Word2Vec保存的模型加载词向量模型 </summary>
        ''' <param name="path"> 模型文件路径 </param>
        ''' <returns> 词向量模型 </returns>
        Public Shared Function loadFromFile(path As String) As VectorModel
            If ReferenceEquals(path, Nothing) OrElse path.Length = 0 Then
                Throw New ArgumentException("模型路径可以为null或空。")
            End If

            Dim dis As BinaryReader = Nothing
            Dim wordCount As Integer, layerSizeLoaded = 0
            Dim wordMapLoaded As IDictionary(Of String, Single()) = New Dictionary(Of String, Single())()

            Try
                dis = New BinaryReader(path.Open)
                wordCount = dis.ReadInt32
                layerSizeLoaded = dis.ReadInt32
                Dim vector As Single
                Dim key As String
                Dim value As Single()

                For i = 0 To wordCount - 1
                    key = dis.ReadString
                    value = New Single(layerSizeLoaded - 1) {}
                    Dim len As Double = 0

                    For j = 0 To layerSizeLoaded - 1
                        vector = dis.ReadSingle
                        len += vector * vector
                        value(j) = vector
                    Next

                    len = stdNum.Sqrt(len)

                    For j = 0 To layerSizeLoaded - 1
                        value(j) /= CSng(len)
                    Next

                    wordMapLoaded(key) = value
                Next

            Catch ioe As Exception
                Console.WriteLine(ioe.ToString())
                Console.Write(ioe.StackTrace)
            Finally

                Try

                    If dis IsNot Nothing Then
                        dis.Close()
                    End If

                Catch ioe As Exception
                    Console.WriteLine(ioe.ToString())
                    Console.Write(ioe.StackTrace)
                End Try
            End Try

            Return New VectorModel(wordMapLoaded, layerSizeLoaded)
        End Function

        ''' <summary>
        ''' 保存词向量模型 </summary>
        ''' <param name="file"> 模型存放路径 </param>
        Public Sub saveModel(file As FileStream)
            Dim dataOutputStream As BinaryWriter = Nothing

            Try
                dataOutputStream = New BinaryWriter(file)
                dataOutputStream.Write(wordMap.Count)
                dataOutputStream.Write(vectorSize)

                For Each element In wordMap.SetOfKeyValuePairs()
                    dataOutputStream.Write(element.Key)

                    For Each d In element.Value
                        dataOutputStream.Write(d)
                    Next
                Next

            Catch e As Exception
                Console.WriteLine(e.ToString())
                Console.Write(e.StackTrace)
            Finally

                Try

                    If dataOutputStream IsNot Nothing Then
                        dataOutputStream.Close()
                    End If

                Catch ioe As Exception
                    Console.WriteLine(ioe.ToString())
                    Console.Write(ioe.StackTrace)
                End Try
            End Try
        End Sub

        ''' <summary>
        ''' 获取与词word最相近topNSize个词 </summary>
        ''' <param name="queryWord"> 词 </param>
        ''' <returns> 相近词集，若模型不包含词word，则返回空集 </returns>
        Public Function similar(queryWord As String) As IEnumerable(Of WordScore)
            Dim center = wordMap.GetValueOrNull(queryWord)

            If center Is Nothing Then
                Return {}
            End If

            Dim resultSize = If(wordMap.Count < topNSize, wordMap.Count, topNSize + 1)
            Dim result As SortedSet(Of WordScore) = New SortedSet(Of WordScore)()

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

        Public Function similar(center As Single()) As IEnumerable(Of WordScore)
            If center Is Nothing OrElse center.Length <> vectorSize Then
                Return {}
            End If

            Dim resultSize = If(wordMap.Count < topNSize, wordMap.Count, topNSize)
            Dim result As SortedSet(Of WordScore) = New SortedSet(Of WordScore)()

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
        ''' <returns> 与结果最相近的前topNSize个词 </returns>
        Public Function analogy(word0 As String, word1 As String, word2 As String) As SortedSet(Of WordScore)
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

    End Class
End Namespace
