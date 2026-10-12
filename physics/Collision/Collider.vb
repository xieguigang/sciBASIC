#Region "Microsoft.VisualBasic::76363f66d749458b64d07559aa36e5bb, gr\physics\Collision\Collider.vb"

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

    '   Total Lines: 200
    '    Code Lines: 145 (72.50%)
    ' Comment Lines: 18 (9.00%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 37 (18.50%)
    '     File Size: 7.22 KB


    '     Structure AABB
    ' 
    '         Properties: Center, Extents
    ' 
    '         Function: Contains, Overlaps, Union
    ' 
    '     Class Collider
    ' 
    ' 
    '         Enum ShapeKind
    ' 
    '             Circle, Polygon
    ' 
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class CircleCollider
    ' 
    '         Properties: Kind
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ComputeInertia, GetAABB
    ' 
    '     Class PolygonCollider
    ' 
    '         Properties: Count, Kind
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ComputeInertia, GetAABB, WorldNormal, WorldSupport, WorldVertex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 碰撞几何体：AABB 包围盒、圆形碰撞体、多边形碰撞体（含质心相对顶点、质量与惯量计算）。

Imports System.Collections.Generic
Imports System.Linq
Imports System.Math
Imports std = System.Math

Namespace Collision

    ''' <summary>轴对齐包围盒</summary>
    Public Structure AABB
        Public min As Vector2
        Public max As Vector2

        Public ReadOnly Property Center As Vector2
            Get
                Return (max + min) * 0.5
            End Get
        End Property

        Public ReadOnly Property Extents As Vector2
            Get
                Return (max - min) * 0.5
            End Get
        End Property

        Public Function Overlaps(other As AABB) As Boolean
            Return min.x <= other.max.x AndAlso max.x >= other.min.x AndAlso
                   min.y <= other.max.y AndAlso max.y >= other.min.y
        End Function

        Public Function Contains(p As Vector2) As Boolean
            Return p.x >= min.x AndAlso p.x <= max.x AndAlso p.y >= min.y AndAlso p.y <= max.y
        End Function

        Public Shared Function Union(a As AABB, b As AABB) As AABB
            Return New AABB With {
                .min = New Vector2(std.Min(a.min.x, b.min.x), std.Min(a.min.y, b.min.y)),
                .max = New Vector2(std.Max(a.max.x, b.max.x), std.Max(a.max.y, b.max.y))
            }
        End Function
    End Structure

    ''' <summary>碰撞几何体基类</summary>
    Public MustInherit Class Collider
        Public Enum ShapeKind
            Circle
            Polygon
        End Enum

        ''' <summary>几何体类型</summary>
        Public MustOverride ReadOnly Property Kind As ShapeKind

        ''' <summary>给定质量，计算绕质心的转动惯量</summary>
        Public MustOverride Function ComputeInertia(mass As Double) As Double

        ''' <summary>在给定位置/朝向下的世界 AABB</summary>
        Public MustOverride Function GetAABB(pos As Vector2, rot As Double) As AABB
    End Class

    ''' <summary>圆形碰撞体，圆心位于局部原点（= 质心）</summary>
    Public Class CircleCollider : Inherits Collider
        Public Radius As Double

        Sub New(radius As Double)
            Me.Radius = radius
        End Sub

        Public Overrides ReadOnly Property Kind As ShapeKind
            Get
                Return ShapeKind.Circle
            End Get
        End Property

        Public Overrides Function ComputeInertia(mass As Double) As Double
            Return 0.5 * mass * Radius * Radius
        End Function

        Public Overrides Function GetAABB(pos As Vector2, rot As Double) As AABB
            Return New AABB With {
                .min = New Vector2(pos.x - Radius, pos.y - Radius),
                .max = New Vector2(pos.x + Radius, pos.y + Radius)
            }
        End Function
    End Class

    ''' <summary>凸多边形碰撞体，顶点存储为相对质心（中心）的局部坐标</summary>
    Public Class PolygonCollider : Inherits Collider
        ''' <summary>局部顶点（已相对质心居中）</summary>
        Public vertices As List(Of Vector2)

        ''' <summary>每条边的外法向量（局部）</summary>
        Public normals As List(Of Vector2)

        ''' <summary>
        ''' 由局部顶点构造。会自动计算质心并将所有顶点平移到质心相对坐标，
        ''' 同时预计算外法向量。
        ''' </summary>
        Sub New(localVertices As IEnumerable(Of Vector2))
            Dim pts = localVertices.ToArray()
            Dim n = pts.Length
            Dim area = 0.0, cx = 0.0, cy = 0.0

            For i = 0 To n - 1
                Dim p1 = pts(i)
                Dim p2 = pts((i + 1) Mod n)
                Dim cr = p1.x * p2.y - p1.y * p2.x
                area += cr
                cx += (p1.x + p2.x) * cr
                cy += (p1.y + p2.y) * cr
            Next

            area *= 0.5
            cx /= (6.0 * area)
            cy /= (6.0 * area)

            Dim centroid = New Vector2(cx, cy)
            vertices = pts.Select(Function(p) p - centroid).ToList()

            normals = New List(Of Vector2)
            For i = 0 To n - 1
                Dim a = vertices(i)
                Dim b = vertices((i + 1) Mod n)
                Dim edge = b - a
                normals.Add(Normalize(New Vector2(edge.y, -edge.x)))
            Next
        End Sub

        Public Overrides ReadOnly Property Kind As ShapeKind
            Get
                Return ShapeKind.Polygon
            End Get
        End Property

        Public ReadOnly Property Count As Integer
            Get
                Return vertices.Count
            End Get
        End Property

        ''' <summary>第 i 个顶点在世界坐标系中的位置</summary>
        Public Function WorldVertex(i As Integer, pos As Vector2, rot As Double) As Vector2
            Return pos + Rotate(vertices(i), rot)
        End Function

        ''' <summary>第 i 条边的法向量在世界坐标系中</summary>
        Public Function WorldNormal(i As Integer, rot As Double) As Vector2
            Return Rotate(normals(i), rot)
        End Function

        ''' <summary>在给定世界方向 <paramref name="dir"/> 上的支撑点（世界坐标）</summary>
        Public Function WorldSupport(dir As Vector2, pos As Vector2, rot As Double) As Vector2
            Dim best = Double.NegativeInfinity
            Dim bestV = vertices(0)

            For Each v In vertices
                Dim wv = pos + Rotate(v, rot)
                Dim proj = Dot(wv, dir)
                If proj > best Then
                    best = proj
                    bestV = wv
                End If
            Next

            Return bestV
        End Function

        Public Overrides Function ComputeInertia(mass As Double) As Double
            Dim num = 0.0, den = 0.0
            Dim n = vertices.Count

            For i = 0 To n - 1
                Dim p1 = vertices(i)
                Dim p2 = vertices((i + 1) Mod n)
                Dim cr = std.Abs(p1.x * p2.y - p1.y * p2.x)
                Dim p1p1 = p1.x * p1.x + p1.y * p1.y
                Dim p1p2 = p1.x * p2.x + p1.y * p2.y
                Dim p2p2 = p2.x * p2.x + p2.y * p2.y
                num += cr * (p1p1 + p1p2 + p2p2)
                den += cr
            Next

            If den < 1.0e-12 Then Return mass
            Return mass * num / (6.0 * den)
        End Function

        Public Overrides Function GetAABB(pos As Vector2, rot As Double) As AABB
            Dim box = New AABB With {.min = pos, .max = pos}

            For Each v In vertices
                Dim wv = pos + Rotate(v, rot)
                box.min = New Vector2(std.Min(box.min.x, wv.x), std.Min(box.min.y, wv.y))
                box.max = New Vector2(std.Max(box.max.x, wv.x), std.Max(box.max.y, wv.y))
            Next

            Return box
        End Function
    End Class
End Namespace
