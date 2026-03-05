Imports std = System.Math

Namespace HashMaps.MinHash

    Public Class LSHParameterEstimator

        ''' <summary>
        ''' 核心推荐函数：根据目标序列一致性，计算应该使用的 MinHash 相似度阈值
        ''' </summary>
        ''' <param name="targetIdentity">目标序列一致性 (如 CD-hit 的 0.8)</param>
        ''' <param name="k">k-mer 大小</param>
        ''' <returns>建议的 MinHash Jaccard 阈值</returns>
        Public Shared Function GetThresholdFromIdentity(targetIdentity As Double, k As Integer) As Double
            If targetIdentity <= 0 OrElse targetIdentity > 1 Then
                Throw New ArgumentException("Identity must be between 0 and 1")
            End If

            ' 公式：J = I^k / (2 - I^k)
            Dim pKmerMatch As Double = targetIdentity ^ k
            Dim jaccardEstimate As Double = pKmerMatch / (2.0 - pKmerMatch)

            Return jaccardEstimate
        End Function

        ''' <summary>
        ''' 估算当前 LSH 参数对应的“临界相似度”
        ''' 相似度高于此值的序列有极高概率被选中，低于此值的极大概率被过滤
        ''' </summary>
        ''' <param name="numBands">波段数</param>
        ''' <param name="rowsPerBand">每波段行数</param>
        ''' <returns>LSH 临界阈值</returns>
        Public Shared Function EstimateLSHThreshold(numBands As Integer, rowsPerBand As Integer) As Double
            ' 公式：s ≈ (1/b)^(1/r)
            ' 使用更精确的 P=0.5 公式：s = (1 - 0.5^(1/b))^(1/r)
            Dim threshold As Double = std.Pow(1.0 - std.Pow(0.5, 1.0 / numBands), 1.0 / rowsPerBand)
            Return threshold
        End Function

        ''' <summary>
        ''' 辅助工具：根据目标相似度推荐 b 和 r 的配置
        ''' </summary>
        ''' <param name="targetJaccard">目标 Jaccard 相似度</param>
        ''' <param name="totalHashFunctions">总哈希函数数量 (如 100)</param>
        ''' <returns>建议的波段数和每波段行数</returns>
        Public Shared Function RecommendConfig(targetJaccard As Double, totalHashFunctions As Integer) As (NumBands As Integer, RowsPerBand As Integer)
            ' 这是一个简单的搜索逻辑，寻找最接近目标的配置
            ' 我们希望 threshold 略低于 targetJaccard，以保证召回率
            Dim bestDiff As Double = Double.MaxValue
            Dim bestB As Integer = 0
            Dim bestR As Integer = 0

            ' 遍历可能的因子
            For r = 1 To totalHashFunctions
                If totalHashFunctions Mod r = 0 Then
                    Dim b = totalHashFunctions / r
                    Dim s = EstimateLSHThreshold(b, r)
                    Dim diff = std.Abs(s - targetJaccard)

                    ' 倾向于选择阈值略低于目标的配置
                    If s <= targetJaccard AndAlso diff < bestDiff Then
                        bestDiff = diff
                        bestB = b
                        bestR = r
                    End If
                End If
            Next

            Return (bestB, bestR)
        End Function

    End Class

End Namespace