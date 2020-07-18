#Region "Microsoft.VisualBasic::1033cb650817654b80646f6a8867d3f7, Data_science\Mathematica\SignalProcessing\SignalProcessing\PeakFinding\Algorithm.vb"

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

    '     Class ElevationAlgorithm
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: filterBySinAngles, FindAllSignalPeaks, sin
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Scripting
Imports stdNum = System.Math

Namespace PeakFinding

    ''' <summary>
    ''' 通过累加线的高度落差进行峰识别
    ''' 
    ''' 1. 首先计算出信号的累加线
    ''' 2. 然后删除所有的坡度小于给定角度值的平行段
    ''' 3. 剩下的片段就是信号峰对应的时间s区间
    ''' </summary>
    Public Class ElevationAlgorithm

        ReadOnly sin_angle As Double
        ReadOnly baseline_quantile As Double

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="angle">这个是一个角度值，取值区间为[0,90]</param>
        ''' <param name="baselineQuantile"></param>
        Sub New(angle As Double, baselineQuantile As Double)
            Me.sin_angle = stdNum.Sin((angle / 90) * (1 / 2 * stdNum.PI))
            Me.baseline_quantile = baselineQuantile
        End Sub

        Public Iterator Function FindAllSignalPeaks(signals As IEnumerable(Of ITimeSignal)) As IEnumerable(Of SignalPeak)
            Dim data As ITimeSignal() = signals.OrderBy(Function(t) t.time).ToArray
            Dim baseline As Double = data.SignalBaseline(baseline_quantile)
            Dim line As IVector(Of Vector2D) = data.AccumulateLine(baseline).Shadows
            ' 计算出角度
            Dim angles As Vector2D() = sin(line.Array).ToArray
            ' 删掉所有角度低于阈值的片段
            ' 剩下所有递增的坡度片段
            Dim slopes As SeqValue(Of Vector2D())() = filterBySinAngles(angles).ToArray
            Dim rawSignals As IVector(Of ITimeSignal) = data.Shadows
            Dim rtmin, rtmax As Double
            Dim time As Vector = rawSignals.Select(Function(t) t.time).AsVector
            Dim area As Vector2D()

            For Each region As SeqValue(Of Vector2D()) In slopes
                'If region.value.Length = 1 Then
                '    Dim t As Single = region.value(Scan0).x
                '    Dim i As Integer = Which(angles.Select(Function(a) a.x = t)).First

                '    region = New SeqValue(Of Vector2D()) With {
                '        .value = {angles(i - 1), region.value(Scan0), angles(i + 1)}
                '    }
                'End If

                rtmin = region.value.First.x
                rtmax = region.value.Last.x
                area = line((time >= rtmin) & (time <= rtmax))

                ' 因为Y是累加曲线的值，所以可以近似的看作为峰面积积分值
                ' 在这里将区间的上限的积分值减去区间的下限的积分值即可得到当前的这个区间的积分值（近似于定积分）
                Yield New SignalPeak With {
                    .integration = area.Last.y - area.First.y,
                    .region = rawSignals((time >= rtmin) & (time <= rtmax)),
                    .baseline = baseline
                }
            Next
        End Function

        Private Iterator Function filterBySinAngles(angles As Vector2D()) As IEnumerable(Of SeqValue(Of Vector2D()))
            Dim buffer As New List(Of Vector2D)
            Dim i As i32 = 0

            For Each a As Vector2D In angles
                If a.y < sin_angle Then
                    If buffer > 0 Then
                        buffer += a

                        Yield New SeqValue(Of Vector2D()) With {
                            .i = ++i,
                            .value = buffer.PopAll
                        }
                    End If
                Else
                    buffer += a
                End If
            Next

            If buffer > 0 Then
                Yield New SeqValue(Of Vector2D()) With {
                    .i = ++i,
                    .value = buffer.PopAll
                }
            End If
        End Function

        Private Iterator Function sin(line As Vector2D()) As IEnumerable(Of Vector2D)
            Dim A As Vector2D
            Dim B As Vector2D
            Dim sinA, sinC As Double

            For Each con As SlideWindow(Of Vector2D) In line.SlideWindows(winSize:=2, offset:=1)
                If con.Length = 2 Then
                    A = con.First
                    B = con.Last
                    sinA = B.y - A.y
                    sinC = EuclideanDistance(New Double() {A.x, A.y}, New Double() {B.x, B.y})

                    '      B
                    '     /|
                    '    / |
                    '   /  |
                    ' A ---- X

                    Yield New Vector2D(A.x, sinA / sinC)
                End If
            Next
        End Function

    End Class
End Namespace
