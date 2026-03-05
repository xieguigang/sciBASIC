Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB.BKLDLT
Imports std = System.Math

Namespace HashMaps.MinHash

    Public Structure SimilarityIndex

        Dim U, V As Integer
        Dim Jaccard As Double

    End Structure

    Public Module LSH

        ''' <summary>
        ''' LSH 处理过程：将数据分桶，找出相似候选对
        ''' </summary>
        ''' <param name="allSequences"></param>
        Public Iterator Function FindSimilarItems(allSequences As List(Of SequenceItem)) As IEnumerable(Of SimilarityIndex)
            ' 桶结构：Key = “波段索引_波段内签名的哈希”, Value = 序列ID列表
            Dim buckets As New Dictionary(Of String, List(Of Integer))()

            ' 1. 将所有序列放入桶中
            For Each seq As SequenceItem In allSequences
                ' 遍历每个波段
                For bandIndex As Integer = 0 To Config.Num_Bands - 1
                    ' 提取当前波段的签名片段
                    Dim startIdx As Integer = bandIndex * Config.Rows_Per_Band
                    Dim bandSignature As New List(Of Integer)
                    For r As Integer = 0 To Config.Rows_Per_Band - 1
                        bandSignature.Add(seq.Signature(startIdx + r))
                    Next

                    ' 生成桶的Key。这里简单地将波段内的数字拼接字符串
                    ' 实际生产中通常使用更高效的哈希算法（如MurmurHash）
                    Dim bucketKey As String = bandIndex & "_" & String.Join("|", bandSignature)

                    ' 放入桶中
                    If Not buckets.ContainsKey(bucketKey) Then
                        buckets(bucketKey) = New List(Of Integer)()
                    End If
                    buckets(bucketKey).Add(seq.ID)
                Next
            Next

            For Each bucket As List(Of Integer) In buckets.Values
                ' 如果一个桶里有多个序列，说明它们在某个波段完全匹配，是相似候选
                If bucket.Count > 1 Then
                    ' 简单两两组合 (实际优化可以用排序等)
                    For i As Integer = 0 To bucket.Count - 1
                        For j As Integer = i + 1 To bucket.Count - 1
                            ' 记录候选对 (ID1, ID2)，确保ID小的在前防止重复
                            Dim u = std.Min(bucket(i), bucket(j))
                            Dim v = std.Max(bucket(i), bucket(j))

                            Yield New SimilarityIndex With {
                                .U = u,
                                .V = v,
                                .Jaccard = CalculateSimilarity(allSequences(u).Signature, allSequences(v).Signature)
                            }
                        Next
                    Next
                End If
            Next
        End Function

        ' 辅助：计算两个签名的相似度 (海明距离或匹配度)
        Public Function CalculateSimilarity(sig1 As List(Of Integer), sig2 As List(Of Integer)) As Double
            Dim matches As Integer = 0
            For i As Integer = 0 To sig1.Count - 1
                If sig1(i) = sig2(i) Then matches += 1
            Next
            Return matches / sig1.Count
        End Function
    End Module
End Namespace