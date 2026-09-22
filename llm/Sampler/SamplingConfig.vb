Namespace Sampler

    ''' <summary>采样超参。</summary>
    Public Class SamplingConfig

        ''' <summary>是否退化为贪心解码（等价于温度 → 0）。</summary>
        Public Property Greedy As Boolean = False

        ''' <summary>温度 τ；&lt;= 0 时按 1 处理（贪心请改用 <see cref="Greedy"/>）。</summary>
        Public Property Temperature As Double = 1.0

        ''' <summary>Top-k 的 k；&lt;= 0 表示不启用。</summary>
        Public Property TopK As Integer = 0

        ''' <summary>Top-p 的 p；&gt;= 1 表示不启用。</summary>
        Public Property TopP As Double = 1.0

        ''' <summary>重复惩罚系数；&gt; 1 抑制重复，&lt; 1 鼓励重复，= 1 关闭。</summary>
        Public Property RepetitionPenalty As Double = 1.0

        ''' <summary>随机种子；设定后采样可复现。</summary>
        Public Property Seed As Integer? = Nothing

        ''' <summary>贪心配置。</summary>
        Public Shared Function GreedySampling() As SamplingConfig
            Return New SamplingConfig With {.Greedy = True}
        End Function

        ''' <summary>纯温度采样。</summary>
        Public Shared Function WithTemperature(temperature As Double) As SamplingConfig
            Return New SamplingConfig With {.Temperature = temperature}
        End Function

        ''' <summary>Top-k 采样。</summary>
        Public Shared Function WithTopK(k As Integer, Optional temperature As Double = 1.0) As SamplingConfig
            Return New SamplingConfig With {.TopK = k, .Temperature = temperature}
        End Function

        ''' <summary>核采样（Top-p）。</summary>
        Public Shared Function WithTopP(p As Double, Optional temperature As Double = 1.0) As SamplingConfig
            Return New SamplingConfig With {.TopP = p, .Temperature = temperature}
        End Function

        ''' <summary>Returns a short description of the active sampling parameters.</summary>
        ''' <returns>A text such as <c>greedy</c> or <c>T=0.8, top-k=40</c>.</returns>
        Public Overrides Function ToString() As String
            If Greedy Then Return "greedy"

            Dim parts As New List(Of String) From {$"T={Temperature}"}

            If TopK > 0 Then parts.Add($"top-k={TopK}")
            If TopP > 0 AndAlso TopP < 1 Then parts.Add($"top-p={TopP}")
            If RepetitionPenalty <> 1 Then parts.Add($"repeat={RepetitionPenalty}")

            Return String.Join(", ", parts)
        End Function

    End Class

End Namespace