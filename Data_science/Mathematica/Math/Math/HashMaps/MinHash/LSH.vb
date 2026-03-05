Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Repository
Imports std = System.Math

Namespace HashMaps.MinHash

    Public Structure SimilarityIndex

        Dim U, V As Integer
        Dim Similarity As Double

    End Structure

    Public Module LSH

        <Extension>
        Public Function BuildLSHBuckets(allSequences As IEnumerable(Of SequenceItem)) As Dictionary(Of UInteger, List(Of Integer))
            ' 桶结构：Key = “波段索引_波段内签名的哈希”, Value = 序列ID列表
            Dim buckets As New Dictionary(Of UInteger, List(Of Integer))()
            ' 辅助变量：用于BitConverter的中间变量
            ' UInteger 是 4 字节
            Dim buffer As Byte() = New Byte(Config.Rows_Per_Band * 4 - 1) {}

            ' 1. 分桶过程：将所有序列放入桶中
            For Each seq As SequenceItem In allSequences
                ' 遍历每个波段
                For bandIndex As Integer = 0 To Config.Num_Bands - 1
                    Dim startIdx As Integer = bandIndex * Config.Rows_Per_Band

                    ' A. 将当前波段的 UInteger 签名复制到 buffer 字节数组中
                    '    这比字符串拼接快得多，且内存占用极小
                    For r As Integer = 0 To Config.Rows_Per_Band - 1
                        Dim val As UInteger = seq.Signature(startIdx + r)
                        ' 使用 BitConverter 将整数转为字节填入 buffer
                        ' 注意：这里假设是小端序，通常对哈希结果无影响，只要一致即可
                        Array.Copy(BitConverter.GetBytes(val), 0, buffer, r * 4, 4)
                    Next

                    ' B. 计算 Bucket Key
                    '    使用 bandIndex 作为种子，确保不同波段的相同值不会碰撞
                    '    API 调用：MurmurHash.MurmurHashCode3_x86_32(字节数组, 种子)
                    Dim bucketKey As Integer = CInt(MurmurHash.MurmurHashCode3_x86_32(buffer, CUInt(bandIndex)))

                    ' C. 入桶
                    If Not buckets.ContainsKey(bucketKey) Then
                        buckets(bucketKey) = New List(Of Integer)()
                    End If
                    buckets(bucketKey).Add(seq.ID)
                Next
            Next

            Return buckets
        End Function

        ''' <summary>
        ''' LSH 处理过程：将数据分桶，找出相似候选对
        ''' </summary>
        ''' <param name="allSequences"></param>
        Public Iterator Function FindSimilarItems(allSequences As SequenceItem()) As IEnumerable(Of SimilarityIndex)
            Dim buckets As Dictionary(Of UInteger, List(Of Integer)) = allSequences.BuildLSHBuckets

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
                                .Similarity = CalculateSimilarity(allSequences(u).Signature, allSequences(v).Signature)
                            }
                        Next
                    Next
                End If
            Next
        End Function

        ''' <summary>
        ''' 辅助：计算两个签名的相似度 (海明距离或匹配度)
        ''' </summary>
        ''' <param name="sig1"></param>
        ''' <param name="sig2"></param>
        ''' <returns></returns>
        Private Function CalculateSimilarity(ByRef sig1 As UInteger(), ByRef sig2 As UInteger()) As Double
            Dim matches As Integer = 0
            For i As Integer = 0 To sig1.Length - 1
                If sig1(i) = sig2(i) Then matches += 1
            Next
            Return matches / sig1.Length
        End Function
    End Module
End Namespace