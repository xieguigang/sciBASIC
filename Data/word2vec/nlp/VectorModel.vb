Imports System.IO

Namespace NlpVec

    ''' <summary>
    ''' User: fangy
    ''' Date: 13-12-9
    ''' Time: 下午2:30
    ''' </summary>
    Public Class VectorModel

        Private wordMap_Renamed As IDictionary(Of String, Single()) = New Dictionary(Of String, Single())()
        Private vectorSize_Renamed As Integer = 200 '特征数
        Private topNSize_Renamed As Integer = 40

        Public Property wordMap As IDictionary(Of String, Single())
            Get
                Return wordMap_Renamed
            End Get
            Set(value As IDictionary(Of String, Single()))
                wordMap_Renamed = value
            End Set
        End Property


        ''' <summary>
        ''' 获取最相似词的数量 </summary>
        ''' <returns> 最相似词的数量 </returns>
        Public Property topNSize As Integer
            Get
                Return topNSize_Renamed
            End Get
            Set(value As Integer)
                topNSize_Renamed = value
            End Set
        End Property

        Public Property vectorSize As Integer
            Get
                Return vectorSize_Renamed
            End Get
            Set(value As Integer)
                vectorSize_Renamed = value
            End Set
        End Property


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

            wordMap_Renamed = wordMap
            vectorSize_Renamed = vectorSize
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

                    len = Math.Sqrt(len)

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
                        dis.close()
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
                dataOutputStream.Write(wordMap_Renamed.Count)
                dataOutputStream.Write(vectorSize_Renamed)

                For Each element In wordMap_Renamed.SetOfKeyValuePairs()
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
                        dataOutputStream.close()
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
        Public Function similar(queryWord As String) As ISet(Of WordScore)
            Dim center = wordMap_Renamed.GetValueOrNull(queryWord)

            If center Is Nothing Then
                Return New [Set](Of WordScore)
            End If

            Dim resultSize = If(wordMap_Renamed.Count < topNSize_Renamed, wordMap_Renamed.Count, topNSize_Renamed + 1)
            Dim result As SortedSet(Of WordScore) = New SortedSet(Of WordScore)()

            For i = 0 To resultSize - 1
                result.Add(New WordScore(Me, "^_^", -Single.MaxValue))
            Next

            Dim minDist = -Single.MaxValue

            For Each entry In wordMap_Renamed.SetOfKeyValuePairs()
                Dim vector = entry.Value
                Dim dist As Single = 0

                For i = 0 To vector.Length - 1
                    dist += center(i) * vector(i)
                Next

                If dist > minDist Then
                    result.Add(New WordScore(Me, entry.Key, dist))
                    minDist = result.pollLast().score
                End If
            Next

            result.pollFirst()
            Return result
        End Function

        Public Function similar(center As Single()) As ISet(Of WordScore)
            If center Is Nothing OrElse center.Length <> vectorSize_Renamed Then
                Return [Set](Of WordScore)()
            End If

            Dim resultSize = If(wordMap_Renamed.Count < topNSize_Renamed, wordMap_Renamed.Count, topNSize_Renamed)
            Dim result As SortedSet(Of WordScore) = New SortedSet(Of WordScore)()

            For i = 0 To resultSize - 1
                result.Add(New WordScore(Me, "^_^", -Single.MaxValue))
            Next

            Dim minDist = -Single.MaxValue

            For Each entry In wordMap_Renamed.SetOfKeyValuePairs()
                Dim vector = entry.Value
                Dim dist As Single = 0

                For i = 0 To vector.Length - 1
                    dist += center(i) * vector(i)
                Next

                If dist > minDist Then
                    result.Add(New WordScore(Me, entry.Key, dist))
                    minDist = result.pollLast().score
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
            Dim wv0 = wordMap_Renamed.GetValueOrNull(word0)
            Dim wv1 = wordMap_Renamed.GetValueOrNull(word1)
            Dim wv2 = wordMap_Renamed.GetValueOrNull(word2)

            If wv1 Is Nothing OrElse wv2 Is Nothing OrElse wv0 Is Nothing Then
                Return Nothing
            End If

            Dim center = New Single(vectorSize_Renamed - 1) {}

            For i = 0 To vectorSize_Renamed - 1
                center(i) = wv1(i) - wv0(i) + wv2(i)
            Next

            Dim resultSize = If(wordMap_Renamed.Count < topNSize_Renamed, wordMap_Renamed.Count, topNSize_Renamed)
            Dim result As SortedSet(Of WordScore) = New SortedSet(Of WordScore)()

            For i = 0 To resultSize - 1
                result.Add(New WordScore(Me, "^_^", -Single.MaxValue))
            Next

            Dim name As String
            Dim minDist = -Single.MaxValue

            For Each entry In wordMap_Renamed.SetOfKeyValuePairs()
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
                    result.Add(New WordScore(Me, entry.Key, dist))
                    minDist = result.pollLast().score
                End If
            Next

            Return result
        End Function

        Public Function getWordVector(word As String) As Single()
            Return wordMap_Renamed.GetValueOrNull(word)
        End Function

        Public Class WordScore
            Implements IComparable(Of WordScore)

            Private ReadOnly outerInstance As VectorModel
            Public name As String
            Public score As Single

            Public Sub New(outerInstance As VectorModel, name As String, score As Single)
                Me.outerInstance = outerInstance
                Me.name = name
                Me.score = score
            End Sub

            Public Overrides Function ToString() As String
                Return name & vbTab & score
            End Function

            Public Function CompareTo(o As WordScore) As Integer Implements IComparable(Of WordScore).CompareTo
                If score < o.score Then
                    Return 1
                Else
                    Return -1
                End If
            End Function
        End Class
    End Class
End Namespace
