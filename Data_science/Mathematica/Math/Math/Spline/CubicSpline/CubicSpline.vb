#Region "Microsoft.VisualBasic::3679a5d4c05d4dda3d1519054aa5b2bb, Data_science\Mathematica\Math\Math\Spline\CubicSpline\CubicSpline.vb"

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

    '   Total Lines: 126
    '    Code Lines: 78 (61.90%)
    ' Comment Lines: 26 (20.63%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 22 (17.46%)
    '     File Size: 4.88 KB


    '     Class CubicSpline
    ' 
    '         Properties: Count
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetEnumerator, GetPoint, IEnumerable_GetEnumerator, (+3 Overloads) RecalcSpline
    ' 
    '         Sub: AddPoint, CalcSpline
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging

Namespace Interpolation

    ''' <summary>
    ''' Cubic spline interpolation
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/CrushedPixel/CubicSplineDemo
    ''' </remarks>
    Public Class CubicSpline : Implements IEnumerable(Of PointF)

        Dim xCubics As CubicSplineVector
        Dim yCubics As CubicSplineVector

        Public ReadOnly Property Count As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return xCubics.Count
            End Get
        End Property

        Sub New()
            xCubics = New CubicSplineVector
            yCubics = New CubicSplineVector
        End Sub

        Sub New(points As IEnumerable(Of PointF))
            Dim array As PointF() = points.ToArray

            xCubics = New CubicSplineVector(array.Select(Function(x) x.X))
            yCubics = New CubicSplineVector(array.Select(Function(x) x.Y))
        End Sub

        Public Sub AddPoint(point As PointF)
            Call xCubics.Add(point.X)
            Call yCubics.Add(point.Y)
        End Sub

        Public Sub CalcSpline()
            Call xCubics.CalcSpline()
            Call yCubics.CalcSpline()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPoint(position As Single) As PointF
            Return New PointF(xCubics.GetPoint(position), yCubics.GetPoint(position))
        End Function

        ''' <summary>
        ''' 三次样本曲线插值
        ''' </summary>
        ''' <param name="source">原始数据点集合，请注意，这些数据点之间都是有顺序分别的</param>
        ''' <param name="expected">所期望的数据点的个数</param>
        ''' <returns></returns>
        Public Shared Iterator Function RecalcSpline(source As IEnumerable(Of PointF), Optional expected# = 100) As IEnumerable(Of PointF)
            Dim spline As New CubicSpline(source)

            If spline.Count <= 2 Then
                Return  ' 只有两个点，无法进行插值，直接返回空集合
            Else
                Call spline.CalcSpline()
            End If

            Dim delta! = spline.Count * expected
            delta = 1 / delta

            For f! = 0 To 1.0! Step delta
                Yield spline.GetPoint(f)
            Next
        End Function

        ''' <summary>
        ''' 应用于3维空间的点对象的三次插值
        ''' </summary>
        ''' <typeparam name="Point"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="newPoint">如何进行点对象的创建工作？</param>
        ''' <param name="expected#">所期望的数据点的个数</param>
        ''' <returns></returns>
        Public Shared Iterator Function RecalcSpline(Of Point As PointF3D)(
                                                       source As IEnumerable(Of Point),
                                                     newPoint As Func(Of Single, Single, Single, Point),
                                            Optional expected# = 100) As IEnumerable(Of Point)

            Dim array As Point() = source.ToArray
            Dim x As New CubicSplineVector(array.Select(Function(p) p.X))
            Dim y As New CubicSplineVector(array.Select(Function(p) p.Y))
            Dim z As New CubicSplineVector(array.Select(Function(p) p.Z))

            Call x.CalcSpline()
            Call y.CalcSpline()
            Call z.CalcSpline()

            Dim delta! = 1 / expected

            For f! = 0 To 1.0! Step delta!
                Yield newPoint(x.GetPoint(f), y.GetPoint(f), z.GetPoint(f))
            Next
        End Function

        ''' <summary>
        ''' 对一个二维点集合进行三次插值处理
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="expected#"></param>
        ''' <returns></returns>
        Public Shared Function RecalcSpline(source As IEnumerable(Of Point), Optional expected# = 100) As IEnumerable(Of Point)
            Dim pointfs = source.Select(Function(pt) New PointF(pt.X, pt.Y))
            Return RecalcSpline(pointfs, expected) _
                .Select(Function(pt) pt.ToPoint)
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of PointF) Implements IEnumerable(Of PointF).GetEnumerator
            For f As Single = 0 To 1 Step 0.01
                Yield GetPoint(f)
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
