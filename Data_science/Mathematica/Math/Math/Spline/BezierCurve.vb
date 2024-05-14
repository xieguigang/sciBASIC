#Region "Microsoft.VisualBasic::ee79a70c5370ab971af0f4a425e047f6, Data_science\Mathematica\Math\Math\Spline\BezierCurve.vb"

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

    '   Total Lines: 145
    '    Code Lines: 66
    ' Comment Lines: 61
    '   Blank Lines: 18
    '     File Size: 6.14 KB


    '     Class BezierCurve
    ' 
    '         Properties: BezierPoints, InitPointsList, Iterations
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: MidPoint, MidPoints, ReCalculate
    ' 
    '         Sub: CreateBezier, PopulateBezierPoints
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

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
        ''' store the list of points in the bezier curve.(贝塞尔曲线插值的结果列表)
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(ctrl1 As PointF, ctrl2 As PointF, ctrl3 As PointF, Optional iteration% = 3)
            Call ReCalculate(ctrl1, ctrl2, ctrl3, iteration)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New()
            _initPoints = New List(Of PointF)
        End Sub

        ''' <summary>
        ''' recreate the bezier curve.
        ''' </summary>
        ''' <param name="ctrl1">first initial point</param>
        ''' <param name="ctrl2">second initial point</param>
        ''' <param name="ctrl3">third initial point</param>
        ''' <param name="iteration">number of iteration of the algorithm</param>
        ''' <returns>the list of points in the curve</returns>
        Public Function ReCalculate(ctrl1 As PointF, ctrl2 As PointF, ctrl3 As PointF, iteration%) As List(Of PointF)
            Iterations = iteration
            _initPoints.Clear()
            _initPoints.AddRange({ctrl1, ctrl2, ctrl3})
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
        ''' <param name="control1">first control point</param>
        ''' <param name="control2">second control point</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function MidPoint(control1 As PointF, control2 As PointF) As PointF
            Return New PointF With {
                .X = (control1.X + control2.X) / 2,
                .Y = (control1.Y + control2.Y) / 2
            }
        End Function

        ''' <summary>
        ''' medial axes between node centres And also boundary lines for the grid
        ''' </summary>
        ''' <param name="a"></param>
        ''' <returns></returns>
        Public Shared Iterator Function MidPoints(a As Double()) As IEnumerable(Of Double)
            Dim gap = a(1) - a(0)

            Yield a(0) - gap / 2

            For i As Integer = 1 To a.Length - 1
                Yield (a(i) + a(i - 1)) / 2
            Next

            Yield a(a.Length - 1) + gap / 2
        End Function
    End Class
End Namespace
