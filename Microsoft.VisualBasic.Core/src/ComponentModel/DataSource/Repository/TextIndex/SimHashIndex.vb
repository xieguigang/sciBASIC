Imports Microsoft.VisualBasic.SecurityString
Imports std = System.Math

Namespace ComponentModel.DataSourceModel.Repository

    Public Class SimHashIndex

        ''' <summary>
        ''' 哈希位数（通常为64或128）
        ''' </summary>
        Private ReadOnly _hashSize As Integer
        ''' <summary>
        ''' 波段数量
        ''' </summary>
        Private ReadOnly _bands As Integer
        ''' <summary>
        ''' 每波段行数
        ''' </summary>
        Private ReadOnly _rows As Integer
        Private ReadOnly _lshIndex As Dictionary(Of String, HashSet(Of Integer))
        Private ReadOnly _stringHashes As Dictionary(Of Integer, Long)
        Private ReadOnly _strings As List(Of String)

        Public Sub New(Optional hashSize As Integer = 64, Optional bands As Integer = 8)
            _hashSize = hashSize
            _bands = bands
            _rows = hashSize \ bands
            _lshIndex = New Dictionary(Of String, HashSet(Of Integer))()
            _stringHashes = New Dictionary(Of Integer, Long)()
            _strings = New List(Of String)()
        End Sub

        ''' <summary>
        ''' 计算字符串的SimHash指纹
        ''' </summary>
        Public Function ComputeSimHash(text As String) As Long
            If String.IsNullOrEmpty(text) Then Return 0

            Dim vector As Integer() = New Integer(_hashSize - 1) {}
            Dim features = ExtractFeatures(text)

            Static hashProvider As New Md5HashProvider

            For Each feature As String In features
                Dim hash As Long = hashProvider.ComputeMD5Hash(feature)

                For i As Integer = 0 To _hashSize - 1
                    ' 检查哈希值的第i位
                    Dim bit = (hash >> i) And 1
                    If bit = 1 Then
                        vector(i) += 1
                    Else
                        vector(i) -= 1
                    End If
                Next
            Next

            ' 将向量转换为指纹
            Dim fingerprint As Long = 0
            For i As Integer = 0 To _hashSize - 1
                If vector(i) > 0 Then
                    fingerprint = fingerprint Or (1L << i)
                End If
            Next

            Return fingerprint
        End Function

        ''' <summary>
        ''' 从文本中提取特征（使用3-grams作为特征）
        ''' </summary>
        Private Function ExtractFeatures(text As String) As List(Of String)
            Dim features = New List(Of String)()
            Dim cleanText = text.ToLowerInvariant().Where(Function(c) Char.IsLetterOrDigit(c) Or Char.IsWhiteSpace(c)).ToArray()
            Dim cleaned = New String(cleanText)

            ' 使用3-grams作为特征
            For i As Integer = 0 To cleaned.Length - 3
                features.Add(cleaned.Substring(i, 3))
            Next

            ' 添加整个文本作为特征（适用于短文本）
            If cleaned.Length > 0 Then
                features.Add(cleaned)
            End If

            Return features
        End Function

        ''' <summary>
        ''' 向索引中添加字符串
        ''' </summary>
        Public Sub AddString(s As String)
            If String.IsNullOrEmpty(s) Then Return

            Dim stringIndex = _strings.Count
            _strings.Add(s)
            Dim fingerprint = ComputeSimHash(s)
            _stringHashes(stringIndex) = fingerprint

            ' 为LSH创建band哈希
            For band As Integer = 0 To _bands - 1
                Dim bandHash = GetBandHash(fingerprint, band)
                Dim bandKey = $"{band}_{bandHash}"

                If Not _lshIndex.ContainsKey(bandKey) Then
                    _lshIndex(bandKey) = New HashSet(Of Integer)()
                End If
                _lshIndex(bandKey).Add(stringIndex)
            Next
        End Sub

        ''' <summary>
        ''' 获取指定波段的哈希值
        ''' </summary>
        Private Function GetBandHash(fingerprint As Long, band As Integer) As Long
            Dim startBit = band * _rows
            Dim mask = (1L << _rows) - 1
            Return (fingerprint >> startBit) And mask
        End Function

        ''' <summary>
        ''' 查找相似字符串（基于汉明距离）
        ''' </summary>
        Public Function FindSimilar(query As String, maxHammingDistance As Integer) As List(Of (String, Integer, Integer))
            Dim results = New List(Of (String, Integer, Integer))()
            If String.IsNullOrEmpty(query) Then Return results

            Dim queryFingerprint = ComputeSimHash(query)
            Dim candidates = FindCandidates(queryFingerprint)

            For Each candidateIndex In candidates
                Dim candidateFingerprint = _stringHashes(candidateIndex)
                Dim hammingDist = ComputeHammingDistance(queryFingerprint, candidateFingerprint)

                If hammingDist <= maxHammingDistance Then
                    ' 计算编辑距离作为验证
                    Dim editDist = ComputeLevenshtein(query, _strings(candidateIndex))
                    results.Add((_strings(candidateIndex), hammingDist, editDist))
                End If
            Next

            Return results.OrderBy(Function(r) r.Item2).ToList()
        End Function

        ''' <summary>
        ''' 通过LSH查找候选字符串
        ''' </summary>
        Private Function FindCandidates(fingerprint As Long) As HashSet(Of Integer)
            Dim candidates = New HashSet(Of Integer)()

            For band As Integer = 0 To _bands - 1
                Dim bandHash = GetBandHash(fingerprint, band)
                Dim bandKey = $"{band}_{bandHash}"

                If _lshIndex.ContainsKey(bandKey) Then
                    candidates.UnionWith(_lshIndex(bandKey))
                End If
            Next

            Return candidates
        End Function

        ''' <summary>
        ''' 计算两个指纹的汉明距离
        ''' </summary>
        Public Function ComputeHammingDistance(hash1 As Long, hash2 As Long) As Integer
            Dim xorResult = hash1 Xor hash2
            Dim distance = 0

            While xorResult > 0
                distance += CInt(xorResult And 1)
                xorResult >>= 1
            End While

            Return distance
        End Function

        ''' <summary>
        ''' 计算Levenshtein编辑距离（与QGramIndex中相同）
        ''' </summary>
        Public Function ComputeLevenshtein(s1 As String, s2 As String) As Integer
            If String.IsNullOrEmpty(s1) Then Return If(String.IsNullOrEmpty(s2), 0, s2.Length)
            If String.IsNullOrEmpty(s2) Then Return s1.Length

            Dim n = s1.Length, m = s2.Length
            Dim d = New Integer(n, m) {}

            For i = 0 To n
                d(i, 0) = i
            Next
            For j = 0 To m
                d(0, j) = j
            Next

            For i = 1 To n
                For j = 1 To m
                    Dim cost = If(s1(i - 1) = s2(j - 1), 0, 1)
                    d(i, j) = std.Min(std.Min(d(i - 1, j) + 1, d(i, j - 1) + 1), d(i - 1, j - 1) + cost)
                Next
            Next

            Return d(n, m)
        End Function
    End Class
End Namespace