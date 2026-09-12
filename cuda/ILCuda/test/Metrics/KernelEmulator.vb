#Region "Microsoft.VisualBasic::8a55d806fe2fef2aa99a634803f9d8eb, cuda\ILCuda\test\Metrics\KernelEmulator.vb"

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

    '   Total Lines: 154
    '    Code Lines: 100 (64.94%)
    ' Comment Lines: 23 (14.94%)
    '    - Xml Docs: 26.09%
    ' 
    '   Blank Lines: 31 (20.13%)
    '     File Size: 6.76 KB


    '     Module KernelEmulator
    ' 
    '         Function: Emulate, RunSelfTest
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' 内核逻辑的 CPU 精确模拟（自检用）
'
' 由于 CUDA 工具包版本与显卡驱动版本可能不匹配（例如工具包 13.3 + 只支持
' CUDA 12.8 的驱动），真实 GPU 执行并不一定总是可用。
'
' 这里用 CPU 逐个模拟 rowStatsKernel / gramKernel / finalizeKernel 的
' blockIdx、threadIdx 与分块共享内存的索引方式（并使用与设备端一致的
' 单精度 float 运算），从而在无法上机时也能验证内核的索引与公式是否正确。
' ------------------------------------------------------------------------

Namespace Metrics

    Public Module KernelEmulator

        ''' <summary>与 metrics.cu 中的 TILE 保持一致</summary>
        Private Const Tile As Integer = 16

        ''' <summary>按 GPU 内核的分块方式精确模拟一遍，返回相关矩阵与距离矩阵</summary>
        Public Function Emulate(x As MatrixData) As MatrixMetricsResult
            Dim rows = x.Rows
            Dim cols = x.Cols
            Dim data = x.Data

            Dim watch = Stopwatch.StartNew()

            ' ---------- 模拟 rowStatsKernel ----------
            ' 每个 block 处理一行，blockDim.x = 256，树形归约
            Dim rowSum(rows - 1) As Single
            Dim rowSumSq(rows - 1) As Single

            For row As Integer = 0 To rows - 1
                Dim sum As Single = 0
                Dim sumSq As Single = 0
                Dim baseOffset = row * cols

                For c As Integer = 0 To cols - 1
                    Dim v = data(baseOffset + c)
                    sum += v
                    sumSq += v * v
                Next

                rowSum(row) = sum
                rowSumSq(row) = sumSq
            Next

            ' ---------- 模拟 gramKernel ----------
            Dim dot(rows * rows - 1) As Single
            Dim grid = (rows + Tile - 1) \ Tile

            For by As Integer = 0 To grid - 1
                For bx As Integer = 0 To grid - 1
                    If bx > by Then Continue For ' 与内核中的 "if (bx > by) return;" 对应

                    For ty As Integer = 0 To Tile - 1
                        For tx As Integer = 0 To Tile - 1
                            Dim i = by * Tile + ty
                            Dim j = bx * Tile + tx
                            Dim acc As Single = 0
                            Dim nTiles = (cols + Tile - 1) \ Tile

                            For t As Integer = 0 To nTiles - 1
                                ' aTile[ty][k] / bTile[k][tx] 的取值（含越界补 0）
                                For k As Integer = 0 To Tile - 1
                                    Dim ca = t * Tile + k
                                    Dim cb = t * Tile + k

                                    Dim a As Single = If(i < rows AndAlso ca < cols, data(i * cols + ca), 0.0F)
                                    Dim b As Single = If(j < rows AndAlso cb < cols, data(j * cols + cb), 0.0F)

                                    acc += a * b
                                Next
                            Next

                            If i < rows AndAlso j < rows Then
                                dot(i * rows + j) = acc
                                dot(j * rows + i) = acc
                            End If
                        Next
                    Next
                Next
            Next

            ' ---------- 模拟 finalizeKernel ----------
            Dim corr(rows * rows - 1) As Single
            Dim dist(rows * rows - 1) As Single
            Dim n As Single = CSng(cols)

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To rows - 1
                    Dim index = i * rows + j

                    ' 与 finalizeKernel 的对角线分支保持一致
                    If i = j Then
                        corr(index) = 1.0F
                        dist(index) = 0.0F
                        Continue For
                    End If

                    Dim meanI = rowSum(i) / n
                    Dim meanJ = rowSum(j) / n
                    Dim varI = rowSumSq(i) - n * meanI * meanI
                    Dim varJ = rowSumSq(j) - n * meanJ * meanJ
                    Dim d = dot(index)

                    Dim cov = d - n * meanI * meanJ
                    Dim denom = CSng(System.Math.Sqrt(System.Math.Max(varI, 0.0F)) *
                                     System.Math.Sqrt(System.Math.Max(varJ, 0.0F)))
                    Dim c As Single = If(denom > 1.0E-12F, cov / denom, 0.0F)

                    corr(index) = System.Math.Min(1.0F, System.Math.Max(-1.0F, c))

                    Dim d2 = System.Math.Max(rowSumSq(i) + rowSumSq(j) - 2.0F * d, 0.0F)
                    dist(index) = CSng(System.Math.Sqrt(d2))
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

        ''' <summary>
        ''' 自检：把"内核模拟结果"与"CPU 两遍算法参考实现"对比，
        ''' 用于在没有可用 GPU 时验证内核的索引与公式。
        ''' </summary>
        Public Function RunSelfTest(rows As Integer, cols As Integer, Optional seed As Integer = 42) As Boolean
            Dim x = MatrixData.CreateRandom(rows, cols, seed)
            Dim simulated = Emulate(x)
            Dim reference = CpuMetrics.Compute(x)

            Dim corrError = CpuMetrics.MaxAbsError(simulated.Correlation, reference.Correlation)
            Dim distError = CpuMetrics.MaxAbsError(simulated.Distance, reference.Distance)

            Console.WriteLine()
            Console.WriteLine($"  矩阵规模              : {rows} x {cols}")
            Console.WriteLine($"  相关系数矩阵最大绝对误差: {corrError:E3}")
            Console.WriteLine($"  距离矩阵最大绝对误差    : {distError:E3}")

            ' 单精度 + 一遍式方差，1e-3 以内的误差属于正常范围
            Dim tolerance = 0.001
            Dim passed = corrError <= tolerance AndAlso distError <= tolerance

            Console.WriteLine($"  结论                  : {If(passed, "通过（内核索引与公式与 CPU 参考实现一致）", "未通过（误差超出容差）")}")

            Return passed
        End Function
    End Module
End Namespace
