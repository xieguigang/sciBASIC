#Region "Microsoft.VisualBasic::2606f70a84ab8b368f7f9420b71b7e42, sciBASIC#\Microsoft.VisualBasic.Core\src\Extensions\Image\Math\Polygon2D.vb"

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

'   Total Lines: 270
'    Code Lines: 173
' Comment Lines: 53
'   Blank Lines: 44
'     File Size: 9.81 KB


'     Class Polygon2D
' 
'         Properties: length, xpoints, ypoints
' 
'         Constructor: (+5 Overloads) Sub New
' 
'         Function: boundingInside, checkInside, GenericEnumerator, GetArea, GetEnumerator
'                   GetRandomPoint, GetShoelaceArea, (+3 Overloads) inside
' 
'         Sub: calculateBounds
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports stdNum = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' polygon object in 2D plain
    ''' </summary>
    Public Class Polygon2D : Implements Enumeration(Of PointF)

        Public ReadOnly Property length As Integer = 0

        Public Property xpoints As Double() = New Double(3) {}
        Public Property ypoints As Double() = New Double(3) {}

        ''' <summary>
        ''' [left, top]
        ''' </summary>
        Protected Friend bounds1 As Vector2D = Nothing
        ''' <summary>
        ''' [right, bottom]
        ''' </summary>
        Protected Friend bounds2 As Vector2D = Nothing

        Public ReadOnly Property height As Double
            Get
                Return ypoints.Max - ypoints.Min
            End Get
        End Property

        Default Public Property Item(index As Integer) As PointF
            Get
                Return New PointF(xpoints(index), ypoints(index))
            End Get
            Set(value As PointF)
                xpoints(index) = value.X
                ypoints(index) = value.Y
            End Set
        End Property

        Sub New()
        End Sub

        Sub New(x As Double(), y As Double())
            If x.Length <> y.Length Then
                Throw New InvalidProgramException($"the point size of x should be equals to y!")
            End If

            Me.length = x.Length
            Me.xpoints = New Double(length - 1) {}
            Me.ypoints = New Double(length - 1) {}

            Array.Copy(x, 0, Me.xpoints, 0, length)
            Array.Copy(y, 0, Me.ypoints, 0, length)

            Call calculateBounds(x, y, length)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Sub New(points As PointF())
            Call Me.New(
                x:=points.Select(Function(p) CDbl(p.X)).ToArray,
                y:=points.Select(Function(p) CDbl(p.Y)).ToArray
            )
        End Sub

        Public Sub New(points As IEnumerable(Of Point))
            Call Me.New(points.Select(Function(p) New PointF(p.X, p.Y)).ToArray)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(rect As Rectangle)
            Call Me.New(
                x:={rect.Left, rect.Right, rect.Right, rect.Left},
                y:={rect.Top, rect.Top, rect.Bottom, rect.Bottom}
            )
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="rect">
        ''' 四个顶点是具有前后顺序的，按照顺序构建出一个四方形
        ''' </param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(rect As RectangleF)
            Call Me.New(
                x:={rect.Left, rect.Right, rect.Right, rect.Left},
                y:={rect.Top, rect.Top, rect.Bottom, rect.Bottom}
            )
        End Sub

        ''' <summary>
        ''' get a random point that inside current polygon
        ''' </summary>
        ''' <returns></returns>
        Public Function GetRandomPoint() As PointF
            Dim i As Integer = randf.NextInteger(length)
            Dim x As Double = xpoints(i)
            Dim y As Double = ypoints(i)

            Return New PointF(x, y)
        End Function

        ''' <summary>
        ''' measure the [top,left] and [bottom, right] as rectangle bound
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="n"></param>
        Public Overridable Sub calculateBounds(x As Double(), y As Double(), n As Integer)
            Dim d1 As Double = Double.MaxValue
            Dim d2 As Double = Double.MaxValue
            Dim d3 As Double = Double.MinValue
            Dim d4 As Double = Double.MinValue

            For i As Integer = 0 To n - 1
                Dim d5 As Double = x(i)
                d1 = stdNum.Min(d1, d5)
                d3 = stdNum.Max(d3, d5)

                Dim d6 As Double = y(i)
                d2 = stdNum.Min(d2, d6)
                d4 = stdNum.Max(d4, d6)
            Next

            Me.bounds1 = New Vector2D(d1, d2)
            Me.bounds2 = New Vector2D(d3, d4)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Overridable Function boundingInside(x As Double, y As Double) As Boolean
            Return (x >= Me.bounds1.x) AndAlso (x <= Me.bounds2.x) AndAlso (y >= Me.bounds1.y) AndAlso (y <= Me.bounds2.y)
        End Function

        Public Function inside(par2D As PointF) As Boolean
            If Not boundingInside(par2D.X, par2D.Y) Then
                Return False
            Else
                Return checkInside(par2D.X, par2D.Y)
            End If
        End Function

        ''' <summary>
        ''' check data point is inside current polygon?
        ''' </summary>
        ''' <param name="par2D"></param>
        ''' <returns></returns>
        Public Overridable Function inside(par2D As Vector2D) As Boolean
            If Not boundingInside(par2D.x, par2D.y) Then
                Return False
            Else
                Return checkInside(par2D.x, par2D.y)
            End If
        End Function

        Private Function checkInside(x As Double, y As Double) As Boolean
            Dim i As Integer = 0
            Dim d1 As Double = 0.0
            Dim j As Integer = 0

            While (j < Me.length) AndAlso (Me.ypoints(j) = y)
                j += 1
            End While

            For k As Integer = 0 To Me.length - 1
                Dim m As Integer = (j + 1) Mod Me.length
                Dim d2 As Double = Me.xpoints(m) - Me.xpoints(j)
                Dim d3 As Double = Me.ypoints(m) - Me.ypoints(j)

                If d3 <> 0.0 Then
                    Dim d4 As Double = x - Me.xpoints(j)
                    Dim d5 As Double = y - Me.ypoints(j)

                    If (Me.ypoints(m) = y) AndAlso (Me.xpoints(m) >= x) Then
                        d1 = Me.ypoints(j)
                    End If

                    If (Me.ypoints(j) = y) AndAlso (Me.xpoints(j) >= x) Then
                        If (If(d1 > y, 1, 0)) <> (If(Me.ypoints(m) > y, 1, 0)) Then
                            i -= 1
                        End If
                    End If

                    Dim f As Single = CSng(d5) / CSng(d3)

                    If (f >= 0.0) AndAlso (f <= 1.0) AndAlso (f * d2 >= d4) Then
                        i += 1
                    End If
                End If

                j = m
            Next

            Return i Mod 2 <> 0
        End Function

        ''' <summary>
        ''' @deprecated
        ''' </summary>
        Public Overridable Function inside(x As Double, y As Double) As Boolean
            If length = 0 Then
                Return False
            End If
            If Not boundingInside(x, y) Then
                Return False
            Else
                Return checkInside(x, y)
            End If
        End Function

        ''' <summary>
        ''' Calculates the area of a simple polygon using the shoelace algorithm.
        ''' 
        ''' https://myengineeringworld.net/2014/06/shoelace-polygon-area-excel.html
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetArea() As Double
            Return GetShoelaceArea(xpoints, ypoints)
        End Function

        ''' <summary>
        ''' Calculates the area of a simple polygon using the shoelace algorithm.
        ''' 
        ''' https://myengineeringworld.net/2014/06/shoelace-polygon-area-excel.html
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetShoelaceArea(Xs As Double(), Ys As Double()) As Double
            Dim area As Double = 0

            ' Check if the coordinates of the last points are equal to the coordinates of the first point.
            ' In other words, check if the polygon is closed and then apply the shoelace algorithm.
            If Xs(Xs.Length - 1) = Xs(0) And Ys(Ys.Length - 1) = Ys(0) Then
                ' Polygon is closed (last point = first point).
                For i As Integer = 0 To Xs.Length - 2
                    area = area + (Xs(i + 1) + Xs(i)) * (Ys(i + 1) - Ys(i))
                Next
            Else
                ' The polygon is not considered closed.
                For i As Integer = 0 To Xs.Length - 2
                    area = area + (Xs(i + 1) + Xs(i)) * (Ys(i + 1) - Ys(i))
                Next

                ' Use the coordinates of the first point to "close" the polygon.
                area = area + (Xs(0) + Xs(Xs.Length - 1)) * (Ys(0) - Ys(Ys.Length - 1))
            End If

            ' Finally, calculate the polygon area.
            area = stdNum.Abs(area / 2)

            Return area
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of PointF) Implements Enumeration(Of PointF).GenericEnumerator
            For i As Integer = 0 To length - 1
                Yield New PointF(xpoints(i), ypoints(i))
            Next
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of PointF).GetEnumerator
            Yield GenericEnumerator()
        End Function

        Public Shared Widening Operator CType(points As PointF()) As Polygon2D
            Dim x As Double() = points.Select(Function(p) CDbl(p.X)).ToArray
            Dim y As Double() = points.Select(Function(p) CDbl(p.Y)).ToArray

            Return New Polygon2D(x, y)
        End Operator
    End Class
End Namespace
