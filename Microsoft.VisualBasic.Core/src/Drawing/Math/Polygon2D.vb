#Region "Microsoft.VisualBasic::7576087c9097f2013f3adc54766a53f3, Microsoft.VisualBasic.Core\src\Drawing\Math\Polygon2D.vb"

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

    '   Total Lines: 482
    '    Code Lines: 293 (60.79%)
    ' Comment Lines: 113 (23.44%)
    '    - Xml Docs: 87.61%
    ' 
    '   Blank Lines: 76 (15.77%)
    '     File Size: 17.31 KB


    '     Class Polygon2D
    ' 
    '         Properties: centroid, height, length, width, xpoints
    '                     ypoints
    ' 
    '         Constructor: (+11 Overloads) Sub New
    ' 
    '         Function: boundingInside, checkInside, GenericEnumerator, GetArea, GetDimension
    '                   GetFillPoints, GetRandomPoint, GetRectangle, GetShoelaceArea, GetSize
    '                   GetSizeF, (+4 Overloads) inside, (+2 Overloads) rectanglesX, (+2 Overloads) rectanglesY
    ' 
    '         Sub: calculateBounds
    ' 
    '         Operators: -, +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' polygon object in 2D plain
    ''' </summary>
    ''' <remarks>
    ''' this object is a vector of x and y or a collection of the 2d points.
    ''' </remarks>
    Public Class Polygon2D : Implements Enumeration(Of PointF)

        <XmlAttribute> Public Property xpoints As Double() = New Double(3) {}
        <XmlAttribute> Public Property ypoints As Double() = New Double(3) {}

        ''' <summary>
        ''' get/set the point data by a given index
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Default Public Property Item(index As Integer) As PointF
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New PointF(xpoints(index), ypoints(index))
            End Get
            Set(value As PointF)
                xpoints(index) = value.X
                ypoints(index) = value.Y
            End Set
        End Property

        ''' <summary>
        ''' get centroid point of this 2d polygon shape
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property centroid As PointF
            Get
                If length = 0 Then
                    Return Nothing
                End If

                Return New PointF(xpoints.Average, ypoints.Average)
            End Get
        End Property

        ''' <summary>
        ''' [left, top]
        ''' </summary>
        Protected Friend bounds1 As Vector2D = Nothing
        ''' <summary>
        ''' [right, bottom]
        ''' </summary>
        Protected Friend bounds2 As Vector2D = Nothing

        ''' <summary>
        ''' the size of the polygon points collection
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property length As Integer = 0

        ''' <summary>
        ''' max y - min y
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property height As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If ypoints.Length = 0 Then
                    Return 0
                Else
                    Return ypoints.Max - ypoints.Min
                End If
            End Get
        End Property

        ''' <summary>
        ''' max x - min x
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property width As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If xpoints.Length = 0 Then
                    Return 0
                Else
                    Return xpoints.Max - xpoints.Min
                End If
            End Get
        End Property

        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(x As Integer(), y As Integer())
            Call Me.New(
                x:=x.Select(Function(i) CDbl(i)).ToArray,
                y:=y.Select(Function(i) CDbl(i)).ToArray
            )
        End Sub

        Sub New(x As Double(), y As Double())
            If x.Length <> y.Length Then
                Throw New InvalidProgramException($"the point size of x should be equals to y!")
            End If

            Me.length = x.Length
            Me.xpoints = New Double(length - 1) {}
            Me.ypoints = New Double(length - 1) {}

            ' try to break the clr class object reference at here
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(points As Layout2D())
            Call Me.New(
                x:=points.Select(Function(p) p.X).ToArray,
                y:=points.Select(Function(p) p.Y).ToArray
            )
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(pixels As RasterPixel())
            Call Me.New(
                x:=pixels.Select(Function(p) CDbl(p.X)).ToArray,
                y:=pixels.Select(Function(p) CDbl(p.Y)).ToArray
            )
        End Sub

        ''' <summary>
        ''' union multiple polygon
        ''' </summary>
        ''' <param name="polygons"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(polygons As Polygon2D())
            Call Me.New(
                x:=polygons.Select(Function(p) p.xpoints).IteratesALL.ToArray,
                y:=polygons.Select(Function(p) p.ypoints).IteratesALL.ToArray
            )
        End Sub

        ''' <summary>
        ''' Construct a polygon 2d shape object from a point collection
        ''' </summary>
        ''' <param name="points"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(points As IEnumerable(Of Point))
            Call Me.New(points.Select(Function(p) New PointF(p.X, p.Y)).ToArray)
        End Sub

        Sub New(points As IEnumerable(Of Vector2D))
            Call Me.New((From p As Vector2D In points.SafeQuery Select New PointF(p.x, p.y)).ToArray)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(ParamArray rect As Rectangle())
            Call Me.New(x:=rectanglesX(rect), y:=rectanglesY(rect))
        End Sub

        Private Shared Function rectanglesX(rect As Rectangle()) As Double()
            Dim vec As New List(Of Double)

            For Each r As Rectangle In rect
                Call vec.AddRange({r.Left, r.Right, r.Right, r.Left})
            Next

            Return vec.ToArray
        End Function

        Private Shared Function rectanglesY(rect As Rectangle()) As Double()
            Dim vec As New List(Of Double)

            For Each r As Rectangle In rect
                Call vec.AddRange({r.Top, r.Top, r.Bottom, r.Bottom})
            Next

            Return vec.ToArray
        End Function

        Private Shared Function rectanglesX(rect As RectangleF()) As Double()
            Dim vec As New List(Of Double)

            For Each r As RectangleF In rect
                Call vec.AddRange({r.Left, r.Right, r.Right, r.Left})
            Next

            Return vec.ToArray
        End Function

        Private Shared Function rectanglesY(rect As RectangleF()) As Double()
            Dim vec As New List(Of Double)

            For Each r As RectangleF In rect
                Call vec.AddRange({r.Top, r.Top, r.Bottom, r.Bottom})
            Next

            Return vec.ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="rect">
        ''' 四个顶点是具有前后顺序的，按照顺序构建出一个四方形
        ''' </param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(ParamArray rect As RectangleF())
            Call Me.New(x:=rectanglesX(rect), y:=rectanglesY(rect))
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
                d1 = std.Min(d1, d5)
                d3 = std.Max(d3, d5)

                Dim d6 As Double = y(i)
                d2 = std.Min(d2, d6)
                d4 = std.Max(d4, d6)
            Next

            Me.bounds1 = New Vector2D(d1, d2)
            Me.bounds2 = New Vector2D(d3, d4)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Overridable Function boundingInside(x As Double, y As Double) As Boolean
            Return (x >= Me.bounds1.x) AndAlso (x <= Me.bounds2.x) AndAlso (y >= Me.bounds1.y) AndAlso (y <= Me.bounds2.y)
        End Function

        Public Function inside(par2d As Point) As Boolean
            If Not boundingInside(par2d.X, par2d.Y) Then
                Return False
            Else
                Return checkInside(par2d.X, par2d.Y)
            End If
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
        ''' Check the given target point is inside of this polygon object or not
        ''' </summary>
        ''' <param name="x">
        ''' p.x of the target point
        ''' </param>
        ''' <param name="y">
        ''' p.y of the target point
        ''' </param>
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
        ''' Get the layout rectangle of current polygon object
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetRectangle() As RectangleF
            Return New RectangleF(xpoints.Min, ypoints.Min, width, height)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSizeF() As SizeF
            Return New SizeF(width, height)
        End Function

        ''' <summary>
        ''' get the polygon rectangle size
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' width and height is generated from the rectangle width and height
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSize() As Size
            ' delta value between the max - min from the x,y
            Return New Size(width, height)
        End Function

        ''' <summary>
        ''' get the dimension size via the max x and max y
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' the meaning of <see cref="GetDimension()"/> is different with <see cref="GetSize()"/>
        ''' </remarks>
        Public Function GetDimension() As Size
            Return New Size(xpoints.Max, ypoints.Max)
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
            area = std.Abs(area / 2)

            Return area
        End Function

        Public Function GetFillPoints() As IEnumerable(Of PointF)
            Return PolygonFiller.FillPolygon(Me.AsEnumerable.ToPoints.ToList).PointF
        End Function

        ''' <summary>
        ''' just populate all input points data
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GenericEnumerator() As IEnumerator(Of PointF) Implements Enumeration(Of PointF).GenericEnumerator
            For i As Integer = 0 To length - 1
                Yield New PointF(xpoints(i), ypoints(i))
            Next
        End Function

        Public Shared Widening Operator CType(points As PointF()) As Polygon2D
            Dim x As Double() = points.Select(Function(p) CDbl(p.X)).ToArray
            Dim y As Double() = points.Select(Function(p) CDbl(p.Y)).ToArray

            Return New Polygon2D(x, y)
        End Operator

        ''' <summary>
        ''' move current polygon object by a given offset
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        Public Shared Operator +(p As Polygon2D, offset As PointF) As Polygon2D
            Dim x = SIMD.Add.f64_op_add_f64_scalar(p.xpoints, offset.X)
            Dim y = SIMD.Add.f64_op_add_f64_scalar(p.ypoints, offset.Y)

            Return New Polygon2D(x, y)
        End Operator

        Public Shared Operator -(p As Polygon2D, offset As PointF) As Polygon2D
            Dim x = SIMD.Subtract.f64_op_subtract_f64_scalar(p.xpoints, offset.X)
            Dim y = SIMD.Subtract.f64_op_subtract_f64_scalar(p.ypoints, offset.Y)

            Return New Polygon2D(x, y)
        End Operator
    End Class
End Namespace
