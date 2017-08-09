#Region "Microsoft.VisualBasic::f700c97f9220b97477dc1c639cb661bd, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Spline\BezierCurve.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Interpolation

    ''' <summary>
    ''' A Bezier curve is a parametric curve frequently used in computer graphics and related fields. 
    ''' In vector graphics, Bezier curves are used to model smooth curves that can be scaled indefinitely. 
    ''' There are many ways to construct a Bezier curve. This simple program uses the midpoint algorithm 
    ''' of constructing a Bezier curve. To show the nature of the divide and conquer approach in the 
    ''' algorithm, a recursive function has been used to implement the construction of the piece of 
    ''' Bezier curve.
    ''' </summary>
    ''' <remarks>
    ''' http://www.codeproject.com/Articles/223159/Midpoint-Algorithm-Divide-and-Conquer-Method-for-D
    ''' </remarks> 
    Public Class BezierCurve

        ''' <summary>
        ''' store the list of points in the bezier curve
        ''' </summary>
        Public ReadOnly Property BezierPoints() As List(Of PointF)

        Dim _initPoints As New List(Of PointF)()

        ''' <summary>
        ''' store the list of initial points
        ''' </summary>
        Public ReadOnly Property InitPointsList() As List(Of PointF)
            Get
                If _initPoints Is Nothing Then
                    _initPoints = New List(Of PointF)()
                End If
                Return _initPoints
            End Get
        End Property

        ''' <summary>
        ''' store the number of iterations
        ''' </summary>
        Public Property Iterations As Integer

        Public Sub New(ctrl1 As PointF, ctrl2 As PointF, ctrl3 As PointF, iteration As Integer)
            ReCalculate(ctrl1, ctrl2, ctrl3, iteration)
        End Sub

        ''' <summary>
        ''' recreate the bezier curve.
        ''' </summary>
        ''' <param name="ctrl1">first initial point</param>
        ''' <param name="ctrl2">second initial point</param>
        ''' <param name="ctrl3">third initial point</param>
        ''' <param name="iteration">number of iteration of the algorithm</param>
        ''' <returns>the list of points in the curve</returns>
        Public Function ReCalculate(ctrl1 As PointF, ctrl2 As PointF, ctrl3 As PointF, iteration As Integer) As List(Of PointF)
            Iterations = iteration
            _initPoints.Clear()
            _initPoints.AddRange(New PointF() {ctrl1, ctrl2, ctrl3})
            CreateBezier(ctrl1, ctrl2, ctrl3)
            Return _BezierPoints
        End Function

        ''' <summary>
        ''' create a bezier curve
        ''' </summary>
        ''' <param name="ctrl1">first initial point</param>
        ''' <param name="ctrl2">second initial point</param>
        ''' <param name="ctrl3">third initial point</param>
        Private Sub CreateBezier(ctrl1 As PointF, ctrl2 As PointF, ctrl3 As PointF)
            _BezierPoints = New List(Of PointF)()
            _BezierPoints.Clear()
            _BezierPoints.Add(ctrl1)  ' add the first control point

            Call PopulateBezierPoints(ctrl1, ctrl2, ctrl3, 0)

            _BezierPoints.Add(ctrl3)  ' add the last control point
        End Sub

        ''' <summary>
        ''' Recursivly call to construct the bezier curve with control points
        ''' </summary>
        ''' <param name="ctrl1">first control point of bezier curve segment</param>
        ''' <param name="ctrl2">second control point of bezier curve segment</param>
        ''' <param name="ctrl3">third control point of bezier curve segment</param>
        ''' <param name="currentIteration">the current interation of a branch</param>
        ''' <remarks>
        ''' http://www.codeproject.com/Articles/223159/Midpoint-Algorithm-Divide-and-Conquer-Method-for-D
        ''' </remarks> 
        Private Sub PopulateBezierPoints(ctrl1 As PointF, ctrl2 As PointF, ctrl3 As PointF, currentIteration%)
            If currentIteration < _Iterations Then
                'calculate next mid points
                Dim midPoint1 As PointF = MidPoint(ctrl1, ctrl2)
                Dim midPoint2 As PointF = MidPoint(ctrl2, ctrl3)
                Dim midPoint3 As PointF = MidPoint(midPoint1, midPoint2)
                'the next control point
                currentIteration += 1
                PopulateBezierPoints(ctrl1, midPoint1, midPoint3, currentIteration)
                'left branch
                _BezierPoints.Add(midPoint3)
                'add the next control point
                'right branch
                PopulateBezierPoints(midPoint3, midPoint2, ctrl3, currentIteration)
            End If
        End Sub

        ''' <summary>
        ''' Find mid point
        ''' </summary>
        ''' <param name="controlPoint1">first control point</param>
        ''' <param name="controlPoint2">second control point</param>
        ''' <returns></returns>
        Private Function MidPoint(controlPoint1 As PointF, controlPoint2 As PointF) As PointF
            Return New PointF((controlPoint1.X + controlPoint2.X) / 2, (controlPoint1.Y + controlPoint2.Y) / 2)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="parallel">并行版本的</param>
        ''' <param name="windowSize">数据采样的窗口大小，默认大小是<paramref name="data"></paramref>的百分之1</param>
        ''' <returns></returns>
        ''' <remarks>先对数据进行采样，然后插值，最后返回插值后的平滑曲线数据以用于下一步分析</remarks>
        Public Shared Function BezierSmoothInterpolation(
                               data#(),
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
                .CreateSlideWindows(slideWindowSize:=windowSize,
                                    offset:=windowSize - 1)

            If parallel Then
                LQuery = LinqAPI.Exec(Of SeqValue(Of Double())) <=
 _
                    From win
                    In slideWindows.AsParallel
                    Let value = __interpolation(
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
                    Let value = __interpolation(
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
        Private Shared Function __interpolation(X#(), iteration%) As Double()
            Dim data As Double() = New Double(2) {}

            data(0) = X(Scan0)
            data(1) = X(X.Length / 2)
            data(2) = X.Last

            Dim tmp As New BezierCurve(
                New PointF(0, data(0)),
                New PointF(1, data(1)),
                New PointF(2, data(2)),
                iteration)

            X = tmp.BezierPoints _
                .Select(Function(p) CDbl(p.Y)) _
                .ToArray

            Return X
        End Function
    End Class
End Namespace
