#Region "Microsoft.VisualBasic::f713b634324032480eab1dcf4f9d74a7, Data_science\Mathematica\Math\Math\HashMaps\MinHash\LSH.vb"

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

    '   Total Lines: 150
    '    Code Lines: 88 (58.67%)
    ' Comment Lines: 42 (28.00%)
    '    - Xml Docs: 47.62%
    ' 
    '   Blank Lines: 20 (13.33%)
    '     File Size: 7.04 KB


    '     Structure SimilarityIndex
    ' 
    '         Function: IsUniqueHit, ToString
    ' 
    '     Module LSH
    ' 
    '         Function: BuildLSHBuckets, CalculateSimilarity, FindSimilarItems
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Repository
Imports std = System.Math

Namespace HashMaps.MinHash

    Public Structure SimilarityIndex

        Dim U, V As Integer
        Dim Similarity As Double

        ''' <summary>
        ''' Check of the sequence <see cref="U"/> is a unique hit?
        ''' </summary>
        ''' <returns></returns>
        Public Function IsUniqueHit() As Boolean
            Return Similarity < 0 AndAlso V = Integer.MinValue
        End Function

        Public Overrides Function ToString() As String
            If IsUniqueHit() Then
                Return $"{U} is a unique hit"
            Else
                Return $"{U}-{V} similarity={Similarity}"
            End If
        End Function

    End Structure

    Public Module LSH

        <Extension>
        Public Function BuildLSHBuckets(allSequences As IEnumerable(Of SequenceItem), Num_Bands As Integer, Rows_Per_Band As Integer) As Dictionary(Of UInteger, List(Of Integer))
            ' 桶结构：Key = “波段索引_波段内签名的哈希”, Value = 序列ID列表
            Dim buckets As New Dictionary(Of UInteger, List(Of Integer))()
            ' 辅助变量：用于BitConverter的中间变量
            ' UInteger 是 4 字节
            Dim buffer As Byte() = New Byte(Rows_Per_Band * 4 - 1) {}

            ' 1. 分桶过程：将所有序列放入桶中
            For Each seq As SequenceItem In allSequences
                ' 遍历每个波段
                For bandIndex As Integer = 0 To Num_Bands - 1
                    Dim startIdx As Integer = bandIndex * Rows_Per_Band

                    ' A. 将当前波段的 UInteger 签名复制到 buffer 字节数组中
                    '    这比字符串拼接快得多，且内存占用极小
                    For r As Integer = 0 To Rows_Per_Band - 1
                        Dim val As UInteger = seq.Signature(startIdx + r)
                        Dim offset As Integer = r * 4

                        ' 使用位移操作代替 BitConverter.GetBytes
                        ' 注意：这会生成 Little-Endian (小端序) 数据，与 BitConverter 默认行为一致
                        buffer(offset) = CByte(val)               ' 第0字节 (低8位)
                        buffer(offset + 1) = CByte(val >> 8)      ' 第1字节
                        buffer(offset + 2) = CByte(val >> 16)     ' 第2字节
                        buffer(offset + 3) = CByte(val >> 24)     ' 第3字节 (高8位)
                    Next

                    ' B. 计算 Bucket Key
                    '    使用 bandIndex 作为种子，确保不同波段的相同值不会碰撞
                    '    API 调用：MurmurHash.MurmurHashCode3_x86_32(字节数组, 种子)
                    Dim bucketKey As UInteger = MurmurHash.MurmurHashCode3_x86_32(buffer, CUInt(bandIndex))

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
        ''' <param name="Num_Bands">
        ''' LSH波段数
        ''' </param>
        ''' <param name="Rows_Per_Band">
        ''' 每个波段的行数 (100 / 20 = 5)
        ''' </param>
        Public Iterator Function FindSimilarItems(allSequences As SequenceItem(),
                                                  Optional Num_Bands As Integer = 20,
                                                  Optional Rows_Per_Band As Integer = 5,
                                                  Optional produceUniqueHit As Boolean = False) As IEnumerable(Of SimilarityIndex)

            Dim buckets As Dictionary(Of UInteger, List(Of Integer)) = allSequences.BuildLSHBuckets(Num_Bands, Rows_Per_Band)
            ' 用于去重
            Dim alreadyFound As New HashSet(Of (Integer, Integer))()
            Dim seqIndex = allSequences.ToDictionary(Function(s) s.ID)

            ' for each bucket of the sequnece id
            For Each bucket As List(Of Integer) In buckets.Values
                ' 如果一个桶里有多个序列，说明它们在某个波段完全匹配，是相似候选
                If bucket.Count > 1 Then
                    ' 简单两两组合 (实际优化可以用排序等)
                    For i As Integer = 0 To bucket.Count - 1
                        For j As Integer = i + 1 To bucket.Count - 1
                            ' 记录候选对 (ID1, ID2)，确保ID小的在前防止重复
                            Dim u = std.Min(bucket(i), bucket(j))
                            Dim v = std.Max(bucket(i), bucket(j))
                            Dim pairKey = (u, v)

                            ' HashSet 的 Add 方法返回 Boolean，指示是否添加成功
                            ' 如果添加成功 说明之前不存在，即这是一个新的候选对
                            ' 这种写法比 Contains + Add 更高效（只需一次哈希查找）
                            If alreadyFound.Add(pairKey) Then
                                Yield New SimilarityIndex With {
                                    .U = u,
                                    .V = v,
                                    .Similarity = CalculateSimilarity(
                                        seqIndex(u).Signature,
                                        seqIndex(v).Signature
                                    )
                                }
                            End If
                        Next
                    Next
                ElseIf produceUniqueHit Then
                    ' count = 1 is unique item
                    Dim key As Integer = bucket(0)

                    Yield New SimilarityIndex With {
                        .U = key,
                        .Similarity = -1,
                        .V = Integer.MinValue
                    }
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
