#Region "Microsoft.VisualBasic::7fc88297afe4cbd28d065f34d8e9f0b1, Microsoft.VisualBasic.Core\src\Drawing\Math\Geometry\GeometryMath.vb"

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

    '   Total Lines: 333
    '    Code Lines: 218 (65.47%)
    ' Comment Lines: 75 (22.52%)
    '    - Xml Docs: 89.33%
    ' 
    '   Blank Lines: 40 (12.01%)
    '     File Size: 13.02 KB


    '     Module GeometryMath
    ' 
    '         Function: (+2 Overloads) angleBetween2Lines, GetLineIntersection, (+4 Overloads) IntersectionOf, (+2 Overloads) QuadrantRegion
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' https://stackoverflow.com/questions/30080/how-to-know-if-a-line-intersects-a-plane-in-c-basic-2d-geometry
    ''' 
    ''' 与几何相关的辅助类
    ''' </summary>
    Public Module GeometryMath

        ''' <summary>
        ''' cos(a,b)
        ''' </summary>
        ''' <param name="x1#"></param>
        ''' <param name="y1#"></param>
        ''' <param name="z1#"></param>
        ''' <param name="x2#"></param>
        ''' <param name="y2#"></param>
        ''' <param name="z2#"></param>
        ''' <returns></returns>
        Public Function angleBetween2Lines(x1#, y1#, z1#, x2#, y2#, z2#) As Double
            Dim ab = x1 * x2 + y1 * y2 + z1 * z2
            Dim a = stdNum.Sqrt(x1 ^ 2 + y1 ^ 2 + z1 ^ 2)
            Dim b = stdNum.Sqrt(x2 ^ 2 + y2 ^ 2 + z2 ^ 2)

            If ab = 0R Then
                Return 0
            Else
                Dim cos = ab / (a * b)
                Return cos
            End If
        End Function

        Public Function angleBetween2Lines(line1 As Point2D(), line2 As Point2D()) As Double
            Dim angle1 = stdNum.Atan2(line1(0).Y - line1(1).Y, line1(0).X - line1(1).X)
            Dim angle2 = stdNum.Atan2(line2(0).Y - line2(1).Y, line2(0).X - line2(1).X)
            Dim diff = angle1 - angle2

            If diff > stdNum.PI OrElse diff < -stdNum.PI Then
                diff = angle2 - angle1
            End If

            Return diff
        End Function

        ''' <summary>
        ''' 判断线段与多边形的关系
        ''' </summary>
        ''' <param name="line"></param>
        ''' <param name="polygon"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function IntersectionOf(line As Line, polygon As Polygon2D) As Intersections
            If polygon.length = 0 Then
                Return Intersections.None
            End If
            If polygon.length = 1 Then
                Return IntersectionOf(polygon(0), line)
            End If
            Dim tangent As Boolean = False
            For index As Integer = 0 To polygon.length - 1
                Dim index2 As Integer = (index + 1) Mod polygon.length
                Dim intersection As Intersections = IntersectionOf(line, New Line(polygon(index), polygon(index2)))
                If intersection = Intersections.Intersection Then
                    Return intersection
                End If
                If intersection = Intersections.Tangent Then
                    tangent = True
                End If
            Next
            Return If(tangent, Intersections.Tangent, IntersectionOf(line.P1, polygon))
        End Function

        ''' <summary>
        ''' 判断点与多边形的关系
        ''' </summary>
        ''' <param name="point"></param>
        ''' <param name="polygon"></param>
        ''' <returns></returns>
        Public Function IntersectionOf(point As PointF, polygon As Polygon2D) As Intersections
            Select Case polygon.length
                Case 0
                    Return Intersections.None
                Case 1
                    If polygon(0).X = point.X AndAlso polygon(0).Y = point.Y Then
                        Return Intersections.Tangent
                    Else
                        Return Intersections.None
                    End If
                Case 2
                    Return IntersectionOf(point, New Line(polygon(0), polygon(1)))
            End Select

            Dim counter As Integer = 0
            Dim i As Integer
            Dim p1 As PointF
            Dim n As Integer = polygon.length

            p1 = polygon(0)

            If point = p1 Then
                Return Intersections.Tangent
            End If

            For i = 1 To n
                Dim p2 As PointF = polygon(i Mod n)

                If point = p2 Then
                    Return Intersections.Tangent
                End If

                If point.Y > stdNum.Min(p1.Y, p2.Y) Then
                    If point.Y <= stdNum.Max(p1.Y, p2.Y) Then
                        If point.X <= stdNum.Max(p1.X, p2.X) Then
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

            Return If((counter Mod 2 = 1), Intersections.Containment, Intersections.None)
        End Function

        ''' <summary>
        ''' 判断点与直线的关系
        ''' </summary>
        ''' <param name="point"></param>
        ''' <param name="line"></param>
        ''' <returns></returns>
        Public Function IntersectionOf(point As PointF, line As Line) As Intersections
            Dim bottomY As Single = stdNum.Min(line.Y1, line.Y2)
            Dim topY As Single = stdNum.Max(line.Y1, line.Y2)
            Dim heightIsRight As Boolean = point.Y >= bottomY AndAlso point.Y <= topY
            'Vertical line, slope is divideByZero error!
            If line.X1 = line.X2 Then
                If point.X = line.X1 AndAlso heightIsRight Then
                    Return Intersections.Tangent
                Else
                    Return Intersections.None
                End If
            End If
            Dim slope As Single = (line.X2 - line.X1) / (line.Y2 - line.Y1)
            Dim onLine As Boolean = (line.Y1 - point.Y) = (slope * (line.X1 - point.X))
            If onLine AndAlso heightIsRight Then
                Return Intersections.Tangent
            Else
                Return Intersections.None
            End If
        End Function

        ''' <summary>
        ''' 判断直线与直线的关系
        ''' </summary>
        ''' <param name="line1"></param>
        ''' <param name="line2"></param>
        ''' <param name="i">可以从这个参数取得交点</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function IntersectionOf(line1 As Line, line2 As Line, Optional ByRef i As PointF = Nothing) As Intersections
            '  Fail if either line segment is zero-length.
            If line1.Length = 0R OrElse line2.Length = 0R Then
                Return Intersections.None
            End If

            If (line1.X1 = line2.X1 AndAlso line1.Y1 = line2.Y1) OrElse
               (line1.X1 = line2.X2 AndAlso line1.Y1 = line2.Y2) Then

                i = line1.P1
                Return Intersections.Intersection
            ElseIf (line1.X2 = line2.X1 AndAlso line1.Y2 = line2.Y1) OrElse
                   (line1.X2 = line2.X2 AndAlso line1.Y2 = line2.Y2) Then

                i = line1.P2
                Return Intersections.Intersection
            Else

                Return GetLineIntersection(
                    line1.X1, line1.Y1, line1.X2, line1.Y2,
                    line2.X1, line2.Y1, line2.X2, line2.Y2,
                    i:=i
                )

            End If
        End Function

        ''' <summary>
        ''' + [(<paramref name="AX"/>, <paramref name="AY"/>), (<paramref name="BX"/>, <paramref name="BY"/>)]
        ''' + [(<paramref name="CX"/>, <paramref name="CY"/>), (<paramref name="DX"/>, <paramref name="DY"/>)]
        ''' </summary>
        ''' <param name="AX!"></param>
        ''' <param name="AY!"></param>
        ''' <param name="BX!"></param>
        ''' <param name="BY!"></param>
        ''' <param name="CX!"></param>
        ''' <param name="CY!"></param>
        ''' <param name="DX!"></param>
        ''' <param name="DY!"></param>
        ''' <param name="i">可以从这个参数取得交点</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect/565282#
        ''' </remarks>
        Public Function GetLineIntersection(AX!, AY!, BX!, BY!, CX!, CY!, DX!, DY!, Optional ByRef i As PointF = Nothing) As Intersections
            Dim s02_x As Single
            Dim s02_y As Single
            Dim s10_x As Single
            Dim s10_y As Single
            Dim s32_x As Single
            Dim s32_y As Single
            Dim s_numer As Single
            Dim t_numer As Single
            Dim denom As Single
            Dim t As Single

            s10_x = BX - AX
            s10_y = BY - AY
            s32_x = DX - CX
            s32_y = DY - CY

            denom = s10_x * s32_y - s32_x * s10_y

            If denom = 0 Then
                ' Collinear(平行或共线)       
                Return Intersections.None
            End If

            Dim denomPositive As Boolean = denom > 0

            s02_x = AX - CX
            s02_y = AY - CY
            s_numer = s10_x * s02_y - s10_y * s02_x

            If (s_numer < 0) = denomPositive Then
                ' 参数是大于等亿且小于等亿的，分子分母必须同号且分子小于等于分毿        
                Return Intersections.None
            End If

            t_numer = s32_x * s02_y - s32_y * s02_x

            If (t_numer < 0) = denomPositive Then
                Return Intersections.None
            End If

            If ((s_numer > denom) = denomPositive) OrElse ((t_numer > denom) = denomPositive) Then
                Return Intersections.None
            End If

            ' Collision detected
            t = t_numer / denom
            i = New PointF With {
                .X = AX + (t * s10_x),
                .Y = AY + (t * s10_y)
            }

            Return Intersections.Intersection
        End Function

        ''' <summary>
        ''' 获取角度所指向的象限位置
        ''' </summary>
        ''' <param name="degree"></param>
        ''' <returns></returns>
        <Extension>
        Public Function QuadrantRegion(degree As Double) As QuadrantRegions
            If degree > 360 Then
                degree = degree Mod 360
            ElseIf degree < -360 Then
                degree = degree Mod 360
            End If

            If (degree >= -90 AndAlso degree < 0) OrElse (degree >= 270 AndAlso degree < 360) Then
                Return QuadrantRegions.RightTop
            ElseIf (degree >= -180 AndAlso degree < -90) OrElse (degree >= 180 AndAlso degree < 270) Then
                Return QuadrantRegions.LeftTop
            ElseIf (degree >= -270 AndAlso degree < -180) OrElse (degree >= 90 AndAlso degree < 180) Then
                Return QuadrantRegions.LeftBottom
            Else
                Return QuadrantRegions.RightBottom
            End If
        End Function

        ''' <summary>
        ''' 获取坐标点相对于原点<paramref name="origin"/>的象限位置
        ''' </summary>
        ''' <param name="origin"></param>
        ''' <param name="p"></param>
        ''' <returns></returns>
        <Extension>
        Public Function QuadrantRegion(origin As PointF, p As PointF, Optional d! = 5) As QuadrantRegions
            If stdNum.Abs(p.X - origin.X) <= d AndAlso stdNum.Abs(p.Y - origin.Y) <= d Then
                Return QuadrantRegions.Origin
            End If

            If stdNum.Abs(p.X - origin.X) <= d AndAlso p.Y < origin.Y Then
                Return QuadrantRegions.YTop
            End If
            If stdNum.Abs(p.X - origin.X) <= d AndAlso p.Y > origin.Y Then
                Return QuadrantRegions.YBottom
            End If
            If p.X > origin.X AndAlso stdNum.Abs(p.Y - origin.Y) <= d Then
                Return QuadrantRegions.XRight
            End If
            If p.X < origin.X AndAlso stdNum.Abs(p.Y - origin.Y) <= d Then
                Return QuadrantRegions.XLeft
            End If

            If p.X > origin.X AndAlso p.Y < origin.Y Then
                Return QuadrantRegions.RightTop
            ElseIf p.X < origin.X AndAlso p.Y < origin.Y Then
                Return QuadrantRegions.LeftTop
            ElseIf p.X < origin.X AndAlso p.Y > origin.Y Then
                Return QuadrantRegions.LeftBottom
            ElseIf p.X > origin.X AndAlso p.Y > origin.Y Then
                Return QuadrantRegions.RightBottom
            Else
                Throw New EvaluateException({origin, p}.GetJson)
            End If
        End Function
    End Module
End Namespace
