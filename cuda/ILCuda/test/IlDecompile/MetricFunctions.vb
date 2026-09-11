#Region "Microsoft.VisualBasic::c1b5414373f76bf8eb1cb46130a32a6c, cuda\ILCuda\test\IlDecompile\MetricFunctions.vb"

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

    '   Total Lines: 143
    '    Code Lines: 66 (46.15%)
    ' Comment Lines: 50 (34.97%)
    '    - Xml Docs: 22.00%
    ' 
    '   Blank Lines: 27 (18.88%)
    '     File Size: 6.25 KB


    '     Module MetricFunctions
    ' 
    '         Function: CorrelationCell, DistanceCell, GramDot, PearsonClamp, RowSum
    '                   RowSumSq
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 待反编译的 VB.NET 度量函数
'
' 这些函数与 Kernels\metrics.cu 里的三个内核一一对应，但完全用普通 VB 写成，
' 不掺任何 CUDA 概念。IL -> AST -> CUDA 流水线要做的，就是把它们反编译成
' 表达式树、再发射成能做同样计算的 __global__ 内核。
'
' 覆盖的反编译特性：
'   RowSum / RowSumSq / GramDot : for 循环 + 数组取元素 + 复合赋值
'   CorrelationCell             : guard clause（if + return）+ if/else 菱形
'                                 （两条分支给同一个变量赋值，会触发 SSA phi）
'   DistanceCell                : guard clause + MathF 调用
'   PearsonClamp                : 纯标量（无数组），走"逐元素自动包裹"约定
'
' 约定（也适用于没有标注的普通方法）：
'   * 名为 i / j 的整型形参被视为线程索引，不会占用内核参数位；
'   * 只有 i        -> 一维内核，i = blockIdx.x * blockDim.x + threadIdx.x
'   * 同时有 i 与 j -> 二维内核，i 取行（blockIdx.y）、j 取列（blockIdx.x）
' ---------------------------------------------------------------------------

Namespace IlDecompile

    Public Module MetricFunctions

        ' ------------------------------------------------------------------
        ' 对应 rowStatsKernel 的 sum 部分
        ' ------------------------------------------------------------------

        ''' <summary>第 i 行的元素和：Σ x(i, k)</summary>
        Public Function RowSum(x As Single(), cols As Integer, i As Integer) As Single
            Dim sum As Single = 0.0F

            For k As Integer = 0 To cols - 1
                sum += x(i * cols + k)
            Next

            Return sum
        End Function

        ' ------------------------------------------------------------------
        ' 对应 rowStatsKernel 的 sumSq 部分
        ' ------------------------------------------------------------------

        ''' <summary>第 i 行的平方和：Σ x(i, k)^2</summary>
        Public Function RowSumSq(x As Single(), cols As Integer, i As Integer) As Single
            Dim sumSq As Single = 0.0F

            For k As Integer = 0 To cols - 1
                Dim v As Single = x(i * cols + k)
                sumSq += v * v
            Next

            Return sumSq
        End Function

        ' ------------------------------------------------------------------
        ' 对应 gramKernel 的点积
        ' ------------------------------------------------------------------

        ''' <summary>第 i 行与第 j 行的点积：Σ x(i, k) * x(j, k)</summary>
        Public Function GramDot(x As Single(), cols As Integer, i As Integer, j As Integer) As Single
            Dim acc As Single = 0.0F

            For k As Integer = 0 To cols - 1
                acc += x(i * cols + k) * x(j * cols + k)
            Next

            Return acc
        End Function

        ' ------------------------------------------------------------------
        ' 对应 finalizeKernel 的相关系数部分
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' 由行统计量与点积还原皮尔逊相关系数。
        ''' 与 metrics.cu 的 finalizeKernel 公式逐项一致（含对角线直接返回 1）。
        ''' </summary>
        Public Function CorrelationCell(dot As Single(), rowSum As Single(), rowSumSq As Single(),
                                               rows As Integer, cols As Integer,
                                               i As Integer, j As Integer) As Single
            ' guard clause：对角线在数学上恒为 1，
            ' 走通用公式会因为两个相近大数相减而在单精度下产生 ~1e-2 的误差
            If i = j Then
                Return 1.0F
            End If

            Dim n As Single = CSng(cols)
            Dim meanI As Single = rowSum(i) / n
            Dim meanJ As Single = rowSum(j) / n
            Dim varI As Single = rowSumSq(i) - n * meanI * meanI
            Dim varJ As Single = rowSumSq(j) - n * meanJ * meanJ
            Dim d As Single = dot(i * rows + j)
            Dim cov As Single = d - n * meanI * meanJ
            Dim denom As Single = MathF.Sqrt(MathF.Max(varI, 0.0F)) * MathF.Sqrt(MathF.Max(varJ, 0.0F))
            Dim c As Single

            If denom > 1.0E-12F Then
                c = cov / denom
            Else
                c = 0.0F
            End If

            Return MathF.Min(1.0F, MathF.Max(-1.0F, c))
        End Function

        ' ------------------------------------------------------------------
        ' 对应 finalizeKernel 的欧氏距离部分
        ' ------------------------------------------------------------------

        ''' <summary>
        ''' 由行平方和与点积还原欧氏距离：sqrt(sumSq_i + sumSq_j - 2 * dot_ij)
        ''' </summary>
        Public Function DistanceCell(dot As Single(), rowSumSq As Single(),
                                            rows As Integer, i As Integer, j As Integer) As Single
            If i = j Then
                Return 0.0F
            End If

            Dim d As Single = dot(i * rows + j)
            Dim d2 As Single = MathF.Max(rowSumSq(i) + rowSumSq(j) - 2.0F * d, 0.0F)

            Return MathF.Sqrt(d2)
        End Function

        ' ------------------------------------------------------------------
        ' 纯标量：验证"没有 i/j、也没有数组"时的逐元素自动包裹约定
        ' ------------------------------------------------------------------

        ''' <summary>协方差 / 分母 -> 截断到 [-1, 1] 的相关系数</summary>
        Public Function PearsonClamp(cov As Single, denom As Single) As Single
            Dim c As Single

            If denom > 1.0E-12F Then
                c = cov / denom
            Else
                c = 0.0F
            End If

            Return MathF.Min(1.0F, MathF.Max(-1.0F, c))
        End Function
    End Module
End Namespace
