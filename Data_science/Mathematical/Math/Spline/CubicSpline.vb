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
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Interpolation

    ''' <summary>
    ''' Cubic spline interpolation
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/CrushedPixel/CubicSplineDemo
    ''' </remarks>
    Public Class CubicSpline : Implements IEnumerable(Of PointF)

        Dim _points As New List(Of PointF)
        Dim xCubics As New List(Of Cubic)
        Dim yCubics As New List(Of Cubic)

        Sub New()
        End Sub

        Sub New(points As IEnumerable(Of PointF))
            Call _points.AddRange(points)
        End Sub

        Public Sub AddPoint(point As PointF)
            Me._points.Add(point)
        End Sub

        Private Enum PosField
            X
            Y
        End Enum

        Private Function __extractValues(points As IList(Of PointF), field As PosField) As IList(Of Single)
            Dim ints As New List(Of Single)()
            For Each p As PointF In points
                Select Case field
                    Case PosField.X
                        ints.Add(p.X)
                    Case PosField.Y
                        ints.Add(p.Y)
                End Select
            Next

            Return ints
        End Function

        Public Sub CalcSpline()
            CalcNaturalCubic(__extractValues(_points, PosField.X), xCubics)
            CalcNaturalCubic(__extractValues(_points, PosField.Y), yCubics)
        End Sub

        Public Function GetPoint(position As Single) As PointF
            position = position * xCubics.Count
            Dim cubicNum As Integer = CInt(Fix(Math.Min(xCubics.Count - 1, position)))
            Dim cubicPos As Single = (position - cubicNum)

            Return New PointF(xCubics(cubicNum).Eval(cubicPos), yCubics(cubicNum).Eval(cubicPos))
        End Function

        Public Sub CalcNaturalCubic(values As IList(Of Single), cubics As ICollection(Of Cubic))
            Dim num As Integer = values.Count - 1

            Dim gamma As Double() = New Double(num) {}
            Dim delta As Double() = New Double(num) {}
            Dim D As Double() = New Double(num) {}

            Dim i As Integer
            '		
            '               We solve the equation
            '	          [2 1       ] [D[0]]   [3(x[1] - x[0])  ]
            '	          |1 4 1     | |D[1]|   |3(x[2] - x[0])  |
            '	          |  1 4 1   | | .  | = |      .         |
            '	          |    ..... | | .  |   |      .         |
            '	          |     1 4 1| | .  |   |3(x[n] - x[n-2])|
            '	          [       1 2] [D[n]]   [3(x[n] - x[n-1])]
            '
            '	          by using row operations to convert the matrix to upper triangular
            '	          and then back substitution.  The D[i] are the derivatives at the knots.
            '		 
            gamma(0) = 1.0F / 2.0F
            For i = 1 To num - 1
                gamma(i) = 1.0F / (4.0F - gamma(i - 1))
            Next
            gamma(num) = 1.0F / (2.0F - gamma(num - 1))

            Dim p0 As Single = values(0)
            Dim p1 As Single = values(1)

            delta(0) = 3.0F * (p1 - p0) * gamma(0)
            For i = 1 To num - 1
                p0 = values(i - 1)
                p1 = values(i + 1)
                delta(i) = (3.0F * (p1 - p0) - delta(i - 1)) * gamma(i)
            Next

            p0 = values(num - 1)
            p1 = values(num)

            delta(num) = (3.0F * (p1 - p0) - delta(num - 1)) * gamma(num)

            D(num) = delta(num)
            For i = num - 1 To 0 Step -1
                D(i) = delta(i) - gamma(i) * D(i + 1)
            Next

            'now compute the coefficients of the cubics
            cubics.Clear()

            For i = 0 To num - 1
                p0 = values(i)
                p1 = values(i + 1)

                cubics.Add(New Cubic(p0, D(i), 3 * (p1 - p0) - 2 * D(i) - D(i + 1), 2 * (p0 - p1) + D(i) + D(i + 1)))
            Next
        End Sub

        ''' <summary>
        ''' 三次样本曲线插值
        ''' </summary>
        ''' <param name="source">原始数据点集合，请注意，这些数据点之间都是有顺序分别的</param>
        ''' <param name="expected">所期望的数据点的个数</param>
        ''' <returns></returns>
        Public Shared Iterator Function RecalcSpline(source As IEnumerable(Of PointF), Optional expected# = 100) As IEnumerable(Of PointF)
            Dim spline As New CubicSpline()
            Dim PointFs As PointF() = source.ToArray

            If PointFs.Length <= 2 Then Return  ' 只有两个点，无法进行插值，直接返回空集合

            For Each p As PointF In PointFs
                spline._points.Add(p)
            Next

            Call spline.CalcSpline()

            Dim delta! = spline._points.Count * expected
            delta = 1 / delta
            For f! = 0 To 1.0! Step delta
                Yield spline.GetPoint(f)
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
