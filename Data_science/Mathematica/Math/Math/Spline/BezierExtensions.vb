#Region "Microsoft.VisualBasic::c397db5dec68966b92f48a7d91620b01, Data_science\Mathematica\Math\Math\Spline\BezierExtensions.vb"

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

    '   Total Lines: 98
    '    Code Lines: 69
    ' Comment Lines: 15
    '   Blank Lines: 14
    '     File Size: 3.52 KB


    '     Module BezierExtensions
    ' 
    '         Function: BezierSmoothInterpolation, doInterpolation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Interpolation

    Public Module BezierExtensions

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="parallel">并行版本的</param>
        ''' <param name="windowSize">数据采样的窗口大小，默认大小是<paramref name="data"></paramref>的百分之1</param>
        ''' <returns></returns>
        ''' <remarks>先对数据进行采样，然后插值，最后返回插值后的平滑曲线数据以用于下一步分析</remarks>
        Public Function BezierSmoothInterpolation(data#(),
                                                  Optional windowSize% = -1,
                                                  Optional iteration% = 3,
                                                  Optional parallel As Boolean = False) As Double()

            If windowSize <= 0 Then
                windowSize = data.Length / 100
            End If

            If windowSize < 3 Then
                windowSize = 3 ' 最少需要3个点进行插值
            End If

            Dim LQuery As SeqValue(Of Double())()
            Dim slideWindows = data _
                .CreateSlideWindows(winSize:=windowSize,
                                    offset:=windowSize - 1)

            If parallel Then
                LQuery = LinqAPI.Exec(Of SeqValue(Of Double())) <=
 _
                    From win
                    In slideWindows.AsParallel
                    Let value = doInterpolation(
                        win.Items, iteration)
                    Select x = New SeqValue(Of Double()) With {
                        .i = win.Index,
                        .value = value
                    }
                    Order By x.i Ascending
            Else
                LQuery = LinqAPI.Exec(Of SeqValue(Of Double())) <=
 _
                    From win As SlideWindow(Of Double)
                    In slideWindows
                    Let value = doInterpolation(
                        win.Items, iteration)
                    Select x = New SeqValue(Of Double()) With {
                        .i = win.Index,
                        .value = value
                    }
                    Order By x.i Ascending
            End If

            Dim out#() = LQuery _
                .Select(Function(win) +win) _
                .IteratesALL _
                .ToArray

            Return out
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="X"></param>
        ''' <param name="iteration"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function doInterpolation(X#(), iteration%) As Double()
            Dim data As Double() = New Double(2) {}

            data(0) = X(Scan0)
            data(1) = X(X.Length / 2)
            data(2) = X.Last

            Dim tmp As New BezierCurve(
                ctrl1:=New PointF(0, data(0)),
                ctrl2:=New PointF(1, data(1)),
                ctrl3:=New PointF(2, data(2)),
                iteration:=iteration
            )

            X = tmp.BezierPoints _
                .Select(Function(p) CDbl(p.Y)) _
                .ToArray

            Return X
        End Function
    End Module
End Namespace
