#Region "Microsoft.VisualBasic::7f8ad31d9079aeed071b1375e6458c81, ..\sciBASIC#\Data_science\Mathematical\Math\Spline\CubicSpline.vb"

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
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Serialization.JSON

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

        Public Shared Function RecalcSpline(source As IEnumerable(Of Point)) As IEnumerable(Of Point)
            Return RecalcSpline(source.Select(Function(pt) New PointF(pt.X, pt.Y)))
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
