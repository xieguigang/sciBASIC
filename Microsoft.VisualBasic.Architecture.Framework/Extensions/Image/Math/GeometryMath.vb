#Region "Microsoft.VisualBasic::0d7628a480f03c9b018528c12fbae40a, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\Math\GeometryMath.vb"

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
Imports sys = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' https://stackoverflow.com/questions/30080/how-to-know-if-a-line-intersects-a-plane-in-c-basic-2d-geometry
    ''' 
    ''' 与几何相关的辅助类
    ''' </summary>
    Public Module GeometryMath

        ''' <summary>
        ''' 判断线段与多边形的关系
        ''' </summary>
        ''' <param name="line"></param>
        ''' <param name="polygon"></param>
        ''' <returns></returns>
        Public Function IntersectionOf(line As Line, polygon As Polygon) As Intersection
            If polygon.Length = 0 Then
                Return Intersection.None
            End If
            If polygon.Length = 1 Then
                Return IntersectionOf(polygon(0), line)
            End If
            Dim tangent As Boolean = False
            For index As Integer = 0 To polygon.Length - 1
                Dim index2 As Integer = (index + 1) Mod polygon.Length
                Dim intersection__1 As Intersection = IntersectionOf(line, New Line(polygon(index), polygon(index2)))
                If intersection__1 = Intersection.Intersection Then
                    Return intersection__1
                End If
                If intersection__1 = Intersection.Tangent Then
                    tangent = True
                End If
            Next
            Return If(tangent, Intersection.Tangent, IntersectionOf(line.P1, polygon))
        End Function

        ''' <summary>
        ''' 判断点与多边形的关系
        ''' </summary>
        ''' <param name="point"></param>
        ''' <param name="polygon"></param>
        ''' <returns></returns>
        Public Function IntersectionOf(point As PointF, polygon As Polygon) As Intersection
            Select Case polygon.Length
                Case 0
                    Return Intersection.None
                Case 1
                    If polygon(0).X = point.X AndAlso polygon(0).Y = point.Y Then
                        Return Intersection.Tangent
                    Else
                        Return Intersection.None
                    End If
                Case 2
                    Return IntersectionOf(point, New Line(polygon(0), polygon(1)))
            End Select

            Dim counter As Integer = 0
            Dim i As Integer
            Dim p1 As PointF
            Dim n As Integer = polygon.Length
            p1 = polygon(0)
            If point = p1 Then
                Return Intersection.Tangent
            End If

            For i = 1 To n
                Dim p2 As PointF = polygon(i Mod n)
                If point = p2 Then
                    Return Intersection.Tangent
                End If
                If point.Y > sys.Min(p1.Y, p2.Y) Then
                    If point.Y <= Math.Max(p1.Y, p2.Y) Then
                        If point.X <= Math.Max(p1.X, p2.X) Then
                            If p1.Y <> p2.Y Then
                                Dim xinters As Double = (point.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X
                                If p1.X = p2.X OrElse point.X <= xinters Then
                                    counter += 1
                                End If
                            End If
                        End If
                    End If
                End If
                p1 = p2
            Next

            Return If((counter Mod 2 = 1), Intersection.Containment, Intersection.None)
        End Function

        ''' <summary>
        ''' 判断点与直线的关系
        ''' </summary>
        ''' <param name="point"></param>
        ''' <param name="line"></param>
        ''' <returns></returns>
        Public Function IntersectionOf(point As PointF, line As Line) As Intersection
            Dim bottomY As Single = sys.Min(line.Y1, line.Y2)
            Dim topY As Single = Math.Max(line.Y1, line.Y2)
            Dim heightIsRight As Boolean = point.Y >= bottomY AndAlso point.Y <= topY
            'Vertical line, slope is divideByZero error!
            If line.X1 = line.X2 Then
                If point.X = line.X1 AndAlso heightIsRight Then
                    Return Intersection.Tangent
                Else
                    Return Intersection.None
                End If
            End If
            Dim slope As Single = (line.X2 - line.X1) / (line.Y2 - line.Y1)
            Dim onLine As Boolean = (line.Y1 - point.Y) = (slope * (line.X1 - point.X))
            If onLine AndAlso heightIsRight Then
                Return Intersection.Tangent
            Else
                Return Intersection.None
            End If
        End Function

        ''' <summary>
        ''' 判断直线与直线的关系
        ''' </summary>
        ''' <param name="line1"></param>
        ''' <param name="line2"></param>
        ''' <returns></returns>
        Public Function IntersectionOf(line1 As Line, line2 As Line) As Intersection
            '  Fail if either line segment is zero-length.
            If line1.X1 = line1.X2 AndAlso line1.Y1 = line1.Y2 OrElse line2.X1 = line2.X2 AndAlso line2.Y1 = line2.Y2 Then
                Return Intersection.None
            End If

            If line1.X1 = line2.X1 AndAlso line1.Y1 = line2.Y1 OrElse line1.X2 = line2.X1 AndAlso line1.Y2 = line2.Y1 Then
                Return Intersection.Intersection
            End If
            If line1.X1 = line2.X2 AndAlso line1.Y1 = line2.Y2 OrElse line1.X2 = line2.X2 AndAlso line1.Y2 = line2.Y2 Then
                Return Intersection.Intersection
            End If

            '  (1) Translate the system so that point A is on the origin.
            line1.X2 -= line1.X1
            line1.Y2 -= line1.Y1
            line2.X1 -= line1.X1
            line2.Y1 -= line1.Y1
            line2.X2 -= line1.X1
            line2.Y2 -= line1.Y1

            '  Discover the length of segment A-B.
            Dim distAB As Double = Math.Sqrt(line1.X2 * line1.X2 + line1.Y2 * line1.Y2)

            '  (2) Rotate the system so that point B is on the positive X axis.
            Dim theCos As Double = line1.X2 / distAB
            Dim theSin As Double = line1.Y2 / distAB
            Dim newX As Double = line2.X1 * theCos + line2.Y1 * theSin
            line2.Y1 = CSng(line2.Y1 * theCos - line2.X1 * theSin)
            line2.X1 = CSng(newX)
            newX = line2.X2 * theCos + line2.Y2 * theSin
            line2.Y2 = CSng(line2.Y2 * theCos - line2.X2 * theSin)
            line2.X2 = CSng(newX)

            '  Fail if segment C-D doesn't cross line A-B.
            If line2.Y1 < 0 AndAlso line2.Y2 < 0 OrElse line2.Y1 >= 0 AndAlso line2.Y2 >= 0 Then
                Return Intersection.None
            End If

            '  (3) Discover the position of the intersection point along line A-B.
            Dim posAB As Double = line2.X2 + (line2.X1 - line2.X2) * line2.Y2 / (line2.Y2 - line2.Y1)

            '  Fail if segment C-D crosses line A-B outside of segment A-B.
            If posAB < 0 OrElse posAB > distAB Then
                Return Intersection.None
            End If

            '  (4) Apply the discovered position to line A-B in the original coordinate system.
            Return Intersection.Intersection
        End Function

        ''' <summary>
        ''' 几何体之间的关系类型
        ''' </summary>
        Public Enum Intersection As Byte
            None
            Tangent
            Intersection
            Containment
        End Enum
    End Module

    Public Structure Line

        Public Shared ReadOnly Empty As New Line

        Public Sub New(p1 As PointF, p2 As PointF)
            Me.P1 = p1
            Me.P2 = p2
        End Sub

        Public P1 As PointF
        Public P2 As PointF

        Public Property X1() As Single
            Get
                Return P1.X
            End Get
            Set
                P1.X = Value
            End Set
        End Property

        Public Property X2() As Single
            Get
                Return P2.X
            End Get
            Set
                P2.X = Value
            End Set
        End Property

        Public Property Y1() As Single
            Get
                Return P1.Y
            End Get
            Set
                P1.Y = Value
            End Set
        End Property

        Public Property Y2() As Single
            Get
                Return P2.Y
            End Get
            Set
                P2.Y = Value
            End Set
        End Property
    End Structure

    Public Structure Polygon : Implements IEnumerable(Of PointF)

        Public Sub New(points As PointF())
            Me.Points = points
        End Sub

        Public Points As PointF()

        Public ReadOnly Property Length() As Integer
            Get
                Return Points.Length
            End Get
        End Property

        Default Public Property Item(index As Integer) As PointF
            Get
                Return Points(index)
            End Get
            Set
                Points(index) = Value
            End Set
        End Property

        Public Shared Widening Operator CType(polygon As Polygon) As PointF()
            Return polygon.Points
        End Operator

        Public Shared Widening Operator CType(points As PointF()) As Polygon
            Return New Polygon(points)
        End Operator

        Private Function IEnumerable_GetEnumerator() As IEnumerator(Of PointF) Implements IEnumerable(Of PointF).GetEnumerator
            Return DirectCast(Points.GetEnumerator(), IEnumerator(Of PointF))
        End Function

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Points.GetEnumerator()
        End Function
    End Structure
End Namespace
