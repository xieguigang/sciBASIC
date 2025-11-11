Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein

Namespace ComponentModel.DataSourceModel.Repository

    Public Class QGramIndex

        ''' <summary>
        ''' q-gram的长度
        ''' </summary>
        ReadOnly _q As Integer
        ReadOnly _index As Dictionary(Of String, HashSet(Of Integer))
        ReadOnly _strings As List(Of String)
        ReadOnly _counts As New Dictionary(Of String, Integer)

        Public Sub New(q As Integer)
            _q = q
            _index = New Dictionary(Of String, HashSet(Of Integer))(StringComparer.OrdinalIgnoreCase)
            _strings = New List(Of String)()
        End Sub

        ''' <summary>
        ''' 向索引中添加字符串
        ''' </summary>
        Public Sub AddString(s As String)
            If String.IsNullOrEmpty(s) Then Return

            ' 生成q-grams
            Dim grams = GenerateQGrams(s)
            Dim stringIndex = _strings.Count

            _strings.Add(s)
            _counts(s) = grams.Count

            For Each gram As String In grams
                If Not _index.ContainsKey(gram) Then
                    _index(gram) = New HashSet(Of Integer)()
                End If
                _index(gram).Add(stringIndex)
            Next
        End Sub

        ''' <summary>
        ''' 生成字符串的所有q-grams
        ''' </summary>
        Private Function GenerateQGrams(s As String) As List(Of String)
            Dim grams = New List(Of String)()

            ' 对短于q的字符串添加首尾填充
            Dim padded = PadString(s)

            For i As Integer = 0 To padded.Length - _q
                grams.Add(padded.Substring(i, _q))
            Next

            Return grams
        End Function

        ''' <summary>
        ''' 为短字符串添加填充字符
        ''' </summary>
        Private Function PadString(s As String) As String
            If s.Length >= _q Then Return s

            ' 在首尾添加特殊字符进行填充
            Dim padding = New String("$"c, _q - 1)
            Return padding & s & padding
        End Function

        ''' <summary>
        ''' 基于q-gram重叠度查找相似字符串
        ''' </summary>
        Public Function FindSimilar(query As String, threshold As Double) As IEnumerable(Of FindResult)
            If String.IsNullOrEmpty(query) Then
                Return New FindResult() {}
            End If

            Dim queryGrams = GenerateQGrams(query)
            Dim candidateCounts = New Dictionary(Of Integer, Integer)()

            ' 统计每个候选字符串的匹配q-gram数量
            For Each gram In queryGrams
                If _index.ContainsKey(gram) Then
                    For Each strIndex In _index(gram)
                        If candidateCounts.ContainsKey(strIndex) Then
                            candidateCounts(strIndex) += 1
                        Else
                            candidateCounts(strIndex) = 1
                        End If
                    Next
                End If
            Next

            Dim results As New List(Of FindResult)

            ' 计算Jaccard相似度并筛选结果
            For Each kvp As KeyValuePair(Of Integer, Integer) In candidateCounts
                Dim strIndex = kvp.Key
                Dim commonGrams = kvp.Value
                Dim targetGrams = _counts(_strings(strIndex))
                Dim unionGrams = queryGrams.Count + targetGrams - commonGrams

                Dim similarity = commonGrams / CDbl(unionGrams)
                If similarity >= threshold Then
                    ' 计算编辑距离作为二次验证
                    Dim dist = LevenshteinDistance.ComputeDistance(query, _strings(strIndex))
                    Dim distance = If(dist Is Nothing, Double.PositiveInfinity, dist.Distance)

                    Call results.Add(New FindResult(_strings(strIndex), similarity, distance) With {.index = strIndex})
                End If
            Next

            Return results.OrderByDescending(Function(r) r.similarity)
        End Function

        ''' <summary>
        ''' 获取索引统计信息
        ''' </summary>
        Public Function GetIndexStats() As (Integer, Integer, Integer)
            Return (_strings.Count, _index.Count, _index.Sum(Function(x) x.Value.Count))
        End Function
    End Class

    Public Class FindResult

        Public Property text As String
        Public Property similarity As Double
        Public Property levenshtein As Double
        Public Property index As Integer

        Sub New()
        End Sub

        Sub New(text As String, similairty As Double, levenshtein As Double)
            _text = text
            _similarity = similairty
            _levenshtein = levenshtein
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{index}] {text} = {similarity}"
        End Function
    End Class
End Namespace