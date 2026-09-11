#Region "Microsoft.VisualBasic::07999500a551a2dcd45892866a9c4512, cuda\ILCuda\test\Metrics\CpuMetrics.vb"

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

    '   Total Lines: 110
    '    Code Lines: 72 (65.45%)
    ' Comment Lines: 14 (12.73%)
    '    - Xml Docs: 7.14%
    ' 
    '   Blank Lines: 24 (21.82%)
    '     File Size: 4.16 KB


    '     Module CpuMetrics
    ' 
    '         Function: Compute, MaxAbsError
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' CPU 参考实现
'
' 作用：
'   1) 在没有可用 GPU 的环境下作为兜底，保证命令行程序永远能跑出结果；
'   2) 作为 GPU 结果的正确性基准（最大绝对误差）与加速比基准。
'
' 这里用两遍（two-pass）算法以取得更好的数值稳定性：
'   皮尔逊: corr = Σ(x_i-μ_i)(x_j-μ_j) / sqrt(Σ(x_i-μ_i)^2 * Σ(x_j-μ_j)^2)
'   欧氏  : dist = sqrt(Σ (x_ik - x_jk)^2)
' ------------------------------------------------------------------------

Namespace Metrics

    Public Module CpuMetrics

        Public Function Compute(x As MatrixData) As MatrixMetricsResult
            Dim rows = x.Rows
            Dim cols = x.Cols
            Dim data = x.Data

            Dim watch = Stopwatch.StartNew()

            ' ---- 第一遍：均值与去中心化后的平方和 ----
            Dim means(rows - 1) As Double
            Dim variances(rows - 1) As Double

            For i As Integer = 0 To rows - 1
                Dim offset = i * cols
                Dim sum As Double = 0

                For k As Integer = 0 To cols - 1
                    sum += data(offset + k)
                Next

                Dim mean = sum / cols
                Dim variance As Double = 0

                For k As Integer = 0 To cols - 1
                    Dim d As Double = data(offset + k) - mean
                    variance += d * d
                Next

                means(i) = mean
                variances(i) = variance
            Next

            ' ---- 第二遍：利用对称性只计算上三角 ----
            Dim corr(rows * rows - 1) As Single
            Dim dist(rows * rows - 1) As Single

            For i As Integer = 0 To rows - 1
                Dim offsetI = i * cols
                corr(i * rows + i) = 1.0F
                dist(i * rows + i) = 0.0F

                For j As Integer = i + 1 To rows - 1
                    Dim offsetJ = j * cols
                    Dim covariance As Double = 0
                    Dim squaredDistance As Double = 0

                    For k As Integer = 0 To cols - 1
                        Dim a As Double = data(offsetI + k)
                        Dim b As Double = data(offsetJ + k)
                        covariance += (a - means(i)) * (b - means(j))

                        Dim diff As Double = a - b
                        squaredDistance += diff * diff
                    Next

                    Dim denominator = System.Math.Sqrt(variances(i) * variances(j))
                    Dim c As Double = If(denominator > 0, covariance / denominator, 0.0)

                    If c > 1.0 Then c = 1.0
                    If c < -1.0 Then c = -1.0

                    corr(i * rows + j) = CSng(c)
                    corr(j * rows + i) = CSng(c)
                    dist(i * rows + j) = CSng(System.Math.Sqrt(squaredDistance))
                    dist(j * rows + i) = dist(i * rows + j)
                Next
            Next

            watch.Stop()

            Return New MatrixMetricsResult With {
                .Correlation = New MetricResult(rows, corr),
                .Distance = New MetricResult(rows, dist),
                .KernelMs = watch.Elapsed.TotalMilliseconds,
                .CopyMs = 0
            }
        End Function

        ''' <summary>逐元素比较两个结果矩阵，返回最大绝对误差</summary>
        Public Function MaxAbsError(a As MetricResult, b As MetricResult) As Double
            If a Is Nothing OrElse b Is Nothing Then Return Double.NaN
            If a.Size <> b.Size Then Return Double.NaN

            Dim maxError As Double = 0

            For i As Integer = 0 To a.Data.Length - 1
                Dim diff As Double = System.Math.Abs(CDbl(a.Data(i)) - CDbl(b.Data(i)))
                If diff > maxError Then maxError = diff
                If Double.IsNaN(diff) Then Return Double.NaN
            Next

            Return maxError
        End Function
    End Module
End Namespace

