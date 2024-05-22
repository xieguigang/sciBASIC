#Region "Microsoft.VisualBasic::84bd4f053a2a34e258291a29ad82d11d, Data_science\Mathematica\SignalProcessing\SignalProcessing\PeakFinding\Implement.vb"

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

    '   Total Lines: 90
    '    Code Lines: 61 (67.78%)
    ' Comment Lines: 18 (20.00%)
    '    - Xml Docs: 61.11%
    ' 
    '   Blank Lines: 11 (12.22%)
    '     File Size: 3.71 KB


    '     Module Implement
    ' 
    '         Function: AccumulateLine, SignalBaseline, Triming
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Quantile
Imports stdNum = System.Math

Namespace PeakFinding

    Public Module Implement

        ' 算法原理，每当出现一个峰的时候，累加线就会明显升高一个高度
        ' 当升高的时候，曲线的斜率大于零
        ' 当处于基线水平的时候，曲线的斜率接近于零
        ' 则可以利用这个特性将色谱峰给识别出来
        ' 这个方法仅局限于色谱峰都是各自相互独立的情况之下

        ''' <summary>
        ''' 通过这个函数得到的累加线是一个单调递增曲线
        ''' </summary>
        ''' <param name="signals">应该是按照时间升序排序过了的</param>
        ''' <param name="baseline"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AccumulateLine(signals As ITimeSignal(), baseline As Double) As Vector2D()
            Dim accumulate#
            Dim sumALL# = Aggregate t As ITimeSignal In signals
                          Let x As Double = t.intensity - baseline
                          Where x > 0
                          Into Sum(x)
            Dim ay As Func(Of Double, Double) =
                Function(into As Double) As Double
                    ' 在这里已经消除了本底噪声的影响了
                    into -= baseline
                    accumulate += If(into < 0, 0, into)
                    Return (accumulate / sumALL) * 100
                End Function
            Dim accumulates As Vector2D() = signals _
                .Select(Function(tick)
                            Return New Vector2D(tick.time, ay(tick.intensity))
                        End Function) _
                .ToArray

            Return accumulates
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="signals"></param>
        ''' <param name="quantile">一般建议值为0.65</param>
        ''' <returns></returns>
        <Extension>
        Public Function SignalBaseline(signals As ITimeSignal(), quantile As Double) As Double
            Dim allIntensity As Double() = signals.Select(Function(t) t.intensity).ToArray
            Dim q As QuantileEstimationGK = allIntensity.GKQuantile
            Dim baseline As Double = q.Query(quantile)

            Return baseline
        End Function

        <Extension>
        Public Iterator Function Triming(regions As IEnumerable(Of SignalPeak), peakwidth As DoubleRange) As IEnumerable(Of SignalPeak)
            Dim rt As Double
            Dim rtmin, rtmax As Double
            Dim dt As Double
            Dim halfPeakWidth As Double = peakwidth.Length / 2

            For Each region In regions
                rt = region(which.Max(region.region.Select(Function(a) a.intensity))).time
                rtmin = region.rtmin
                rtmax = region.rtmax
                dt = rtmax - rtmin

                If dt < peakwidth.Min Then
                    Yield region
                ElseIf dt > peakwidth.Max Then
                    rtmin = stdNum.Max(rtmin, rt - halfPeakWidth)
                    rtmax = stdNum.Min(rtmax, rt + halfPeakWidth)

                    Yield region.Subset(rtmin, rtmax)
                Else
                    Yield region
                End If
            Next
        End Function
    End Module
End Namespace
