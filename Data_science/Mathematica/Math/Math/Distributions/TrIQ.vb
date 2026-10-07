#Region "Microsoft.VisualBasic::48d33946b569a5ba00964227e6b1bb4e, Data_science\Mathematica\Math\Math\Distributions\TrIQ.vb"

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

    '   Total Lines: 146
    '    Code Lines: 57 (39.04%)
    ' Comment Lines: 71 (48.63%)
    '    - Xml Docs: 91.55%
    ' 
    '   Blank Lines: 18 (12.33%)
    '     File Size: 5.48 KB


    '     Module TrIQ
    ' 
    '         Function: CutThreshold, DiscreteLevels, FindThreshold, GetTrIQRange
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.SIMD

Namespace Distributions

    ''' <summary>
    ''' Contrast optimization of mass spectrometry imaging (MSI) data visualization by threshold intensity quantization (TrIQ)
    ''' </summary>
    ''' <remarks>
    ''' works based on the <see cref="ECDF"/>.
    ''' </remarks>
    Public Module TrIQ

        ''' <summary>
        ''' trim the head intensity data by a given cutoff threshold 
        ''' which is evaluated via the TrIQ algorithm.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="q"></param>
        ''' <param name="N"></param>
        ''' <param name="eps"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CutThreshold(data As IEnumerable(Of Double), q As Double,
                                     Optional N As Integer = 100,
                                     Optional eps As Double = 0.1) As IEnumerable(Of Double)

            Dim v As Double() = data.ToArray
            Dim cut As Double = v.FindThreshold(q, N, eps)

            ' 等价上界钳制：xi > cut 时取 cut，否则取 xi；xi = cut 时两式均为 cut。
            ' 改用 SIMD 逐元素向量化（SimdClamp = Min(Max(v, min), max)，端点到端内），
            ' 在硬件不支持或 SIMDConfiguration.disable 时自动标量回退，数值结果不变。
            Return v.SimdClamp(Double.MinValue, cut)
        End Function

        ''' <summary>
        ''' The Threshold Intensity Quantization addresses this issue by setting a new upper limit T
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="q"></param>
        ''' <param name="N"></param>
        ''' <param name="eps"></param>
        ''' <returns>the upper bound raw value of the threshold</returns>
        <Extension>
        Public Function FindThreshold(data As IEnumerable(Of Double), q As Double,
                                      Optional N As Integer = 100,
                                      Optional eps As Double = 0.1) As Double

            Return New ECDF(data, N).FindThreshold(q, eps)
        End Function

        ''' <summary>
        ''' get the best value range for level scaler via TrIQ algorithm. 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="q"></param>
        ''' <param name="N"></param>
        ''' <param name="eps"></param>
        ''' <returns>[min,max] of the data range.</returns>
        <Extension>
        Public Function GetTrIQRange(data As IEnumerable(Of Double), q As Double,
                                     Optional N As Integer = 100,
                                     Optional eps As Double = 0.1) As DoubleRange

            Dim raw As Double() = data.SafeQuery.ToArray

            If raw.Length = 0 Then
                Return New DoubleRange(0, 0)
            End If

            Dim max As Double = raw.FindThreshold(q, N, eps)
            Dim range As New DoubleRange(raw.Min, max)

            Return range
        End Function

        ''' <summary>
        ''' Quantization is a process for mapping a range 
        ''' of analog intensity values To a Single discrete 
        ''' value, known As a gray level.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="n"></param>
        ''' <param name="T"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Quantization is a process For mapping a range of 
        ''' analog intensity values To a Single discrete value, 
        ''' known As a gray level. Zero-memory Is a widely 
        ''' used quantization method. The zero-memory quantizer
        ''' computes equally spaced intensity bins Of width w:
        ''' 
        ''' ```
        ''' w = (max(f) - min(f)) / n
        ''' ```
        ''' 
        ''' where n represents the number Of discrete values, 
        ''' usually 256; min(f) And max(f) operators provide 
        ''' minimum And maximum intensity values. Quantization 
        ''' Is based On a comparison with the transition levels 
        ''' tk:
        ''' 
        ''' ```
        ''' tk = w + min(f), 2w + min(f), ..., nw + min(f)
        ''' ```
        ''' 
        ''' Finally, the discrete mapped value Q Is obtained:
        ''' 
        ''' ```
        ''' Q(f(x, y)) = {
        '''     
        '''     0,  f(x,y) &lt;= t1
        '''     k,  tk &lt; f(x,y) &lt; tk+1
        ''' }
        ''' ```
        ''' </remarks>
        <Extension>
        Public Function DiscreteLevels(data As IEnumerable(Of Double),
                                       Optional n As Integer = 30,
                                       Optional T As Double? = Nothing) As IEnumerable(Of Integer)

            Dim f As Double() = data.ToArray
            Dim minf As Double = f.SimdMin
            Dim maxf As Double = f.SimdMax

            If T Is Nothing Then
                T = maxf
            End If

            Dim span As Double = T - minf

            If span = 0.0 Then
                ' 所有值相等：原逻辑 w >= T 恒成立，统一返回 n - 1
                Return f.Select(Function(w) n - 1)
            End If

            ' ScaleMapping 公式为 (w - minf) / span * n，其中 span = T - minf、目标区间 [0, n]。
            ' 严格按原公式的运算顺序（减 -> 除 -> 乘）逐元素向量化仿射映射（SIMD 加速），
            ' 再在标量层精确保留原分支语义：w >= T 强制取 n - 1；w < T 时由 CInt(scaled)
            ' 决定（接近 T 的元素经四舍五入可能得到 n，与原实现一致，不可简单钳到 n - 1）。
            Dim scaled As Double() = f _
                .SimdAddScalar(-minf) _
                .SimdDivideScalar(span) _
                .SimdMultiplyScalar(n)

            Dim levels As Integer() = New Integer(f.Length - 1) {}

            For i As Integer = 0 To f.Length - 1
                levels(i) = If(f(i) >= T, n - 1, CInt(scaled(i)))
            Next

            Return levels
        End Function

    End Module
End Namespace
