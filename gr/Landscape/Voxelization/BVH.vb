#Region "Microsoft.VisualBasic::910b5744da95a385023f04fe4d17d756, gr\Landscape\Voxelization\BVH.vb"

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

    '   Total Lines: 398
    '    Code Lines: 244 (61.31%)
    ' Comment Lines: 89 (22.36%)
    '    - Xml Docs: 69.66%
    ' 
    '   Blank Lines: 65 (16.33%)
    '     File Size: 16.50 KB


    '     Structure SDFTriangle
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Normal
    ' 
    '     Class BVH
    ' 
    '         Function: BuildRecursive, ClosestPtPointTriangle, GetTriangle, PointAABBDistanceSq, QueryNearest
    ' 
    '         Sub: Build
    '         Class CentroidComparer
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: Compare
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports std = System.Math

Namespace Voxelization

    ''' <summary>
    ''' SDF 体素化使用的三角面片，预存三个顶点、轴对齐包围盒 (AABB) 与质心，
    ''' 以加速 BVH 的构建与最近三角面查询。
    ''' </summary>
    Public Structure SDFTriangle

        Public A As Point3D
        Public B As Point3D
        Public C As Point3D

        ''' <summary>三角形 AABB 最小角点</summary>
        Public MinCorner As Point3D
        ''' <summary>三角形 AABB 最大角点</summary>
        Public MaxCorner As Point3D
        ''' <summary>三角形质心（AABB 中位数分割用）</summary>
        Public Centroid As Point3D

        Public Sub New(a As Point3D, b As Point3D, c As Point3D)
            Me.A = a
            Me.B = b
            Me.C = c

            Dim minX As Double = std.Min(a.X, std.Min(b.X, c.X))
            Dim minY As Double = std.Min(a.Y, std.Min(b.Y, c.Y))
            Dim minZ As Double = std.Min(a.Z, std.Min(b.Z, c.Z))
            Dim maxX As Double = std.Max(a.X, std.Max(b.X, c.X))
            Dim maxY As Double = std.Max(a.Y, std.Max(b.Y, c.Y))
            Dim maxZ As Double = std.Max(a.Z, std.Max(b.Z, c.Z))

            Me.MinCorner = New Point3D(minX, minY, minZ)
            Me.MaxCorner = New Point3D(maxX, maxY, maxZ)
            Me.Centroid = New Point3D((minX + maxX) * 0.5, (minY + maxY) * 0.5, (minZ + maxZ) * 0.5)
        End Sub

        ''' <summary>
        ''' 三角面的几何法向 (未必单位化前的方向为 (B-A) × (C-A))，返回单位法向。
        ''' 退化三角形返回零向量。
        ''' </summary>
        Public Function Normal() As Point3D
            Dim n = Point3D.Cross(B.Subtract(A), C.Subtract(A))
            Return n.Normalize()
        End Function

    End Structure

    ''' <summary>
    ''' 三角形网格的包围盒层次结构 (Bounding Volume Hierarchy)，用于加速
    ''' "空间中任意一点到网格最近三角面" 的查询。
    '''
    ''' 采用质心 AABB 最长轴中位数分割递归构建 (非 SAH)，叶子节点三角形数量
    ''' 不超过 <see cref="LeafSize"/>。最近查询通过 "点到节点 AABB 的距离下界"
    ''' 做分支剪枝，将暴力 O(T) 查询降至近似 O(log T)。
    ''' </summary>
    ''' <remarks>
    ''' 参考:
    ''' - Ericson, C. (2004). Real-Time Collision Detection. (ClosestPtPointTriangle)
    ''' - 空间数据结构 BVH 中位数分割。
    ''' </remarks>
    Public Class BVH

        ''' <summary>叶子节点最大三角形数量</summary>
        Public Const LeafSize As Integer = 16

        ' ---- 节点数组 (结构化存储，避免对象开销) ----
        ' 每个节点用平行数组表示：
        '   nodeMin/nodeMax : 节点 AABB
        '   nodeLeft        : 左子节点索引 (>=0 表示内部节点，-1 表示叶子)
        '   nodeRight       : 右子节点索引
        '   nodeStart/nodeCount : 叶子节点在 tris 中的三角形起止范围
        Private nodeMin As Point3D()
        Private nodeMax As Point3D()
        Private nodeLeft As Integer()
        Private nodeRight As Integer()
        Private nodeStart As Integer()
        Private nodeCount As Integer()
        Private nodeUsed As Integer = 0

        ''' <summary>
        ''' 按 BVH 构建顺序重排后的三角形数组
        ''' </summary>
        Private tris As SDFTriangle()

        ''' <summary>根节点索引 (构建后固定为 0)</summary>
        Private rootIndex As Integer = -1

        ''' <summary>
        ''' 依据给定三角形列表构建 BVH。
        ''' </summary>
        ''' <param name="triangles">三角面数组</param>
        Public Sub Build(triangles As SDFTriangle())
            tris = triangles

            Dim n As Integer = triangles.Length
            If n = 0 Then
                rootIndex = -1
                Return
            End If

            ' 最大节点数 = 2 * ceil(n / LeafSize) 的上界，取 2n 保证充足
            Dim maxNodes As Integer = std.Max(1, 2 * n)
            nodeMin = New Point3D(maxNodes - 1) {}
            nodeMax = New Point3D(maxNodes - 1) {}
            nodeLeft = New Integer(maxNodes - 1) {}
            nodeRight = New Integer(maxNodes - 1) {}
            nodeStart = New Integer(maxNodes - 1) {}
            nodeCount = New Integer(maxNodes - 1) {}
            nodeUsed = 0

            rootIndex = BuildRecursive(0, n)
        End Sub

        ''' <summary>
        ''' 递归构建子树，处理 <c>tris[start, start+count)</c> 区间的三角形。
        ''' </summary>
        ''' <returns>新建节点的索引</returns>
        Private Function BuildRecursive(start As Integer, count As Integer) As Integer
            Dim nodeIdx As Integer = nodeUsed
            nodeUsed += 1

            ' 计算当前区间的 AABB
            Dim minX As Double = Double.MaxValue, minY As Double = Double.MaxValue, minZ As Double = Double.MaxValue
            Dim maxX As Double = Double.MinValue, maxY As Double = Double.MinValue, maxZ As Double = Double.MinValue
            ' 质心范围 (用于选择分割轴)
            Dim cMinX As Double = Double.MaxValue, cMinY As Double = Double.MaxValue, cMinZ As Double = Double.MaxValue
            Dim cMaxX As Double = Double.MinValue, cMaxY As Double = Double.MinValue, cMaxZ As Double = Double.MinValue

            For i As Integer = start To start + count - 1
                Dim tri = tris(i)
                If tri.MinCorner.X < minX Then minX = tri.MinCorner.X
                If tri.MinCorner.Y < minY Then minY = tri.MinCorner.Y
                If tri.MinCorner.Z < minZ Then minZ = tri.MinCorner.Z
                If tri.MaxCorner.X > maxX Then maxX = tri.MaxCorner.X
                If tri.MaxCorner.Y > maxY Then maxY = tri.MaxCorner.Y
                If tri.MaxCorner.Z > maxZ Then maxZ = tri.MaxCorner.Z

                If tri.Centroid.X < cMinX Then cMinX = tri.Centroid.X
                If tri.Centroid.Y < cMinY Then cMinY = tri.Centroid.Y
                If tri.Centroid.Z < cMinZ Then cMinZ = tri.Centroid.Z
                If tri.Centroid.X > cMaxX Then cMaxX = tri.Centroid.X
                If tri.Centroid.Y > cMaxY Then cMaxY = tri.Centroid.Y
                If tri.Centroid.Z > cMaxZ Then cMaxZ = tri.Centroid.Z
            Next

            nodeMin(nodeIdx) = New Point3D(minX, minY, minZ)
            nodeMax(nodeIdx) = New Point3D(maxX, maxY, maxZ)

            ' 达到叶子条件 → 叶子节点
            If count <= LeafSize Then
                nodeLeft(nodeIdx) = -1
                nodeRight(nodeIdx) = -1
                nodeStart(nodeIdx) = start
                nodeCount(nodeIdx) = count
                Return nodeIdx
            End If

            ' 选择质心分布最长的轴作为分割轴
            Dim extentX As Double = cMaxX - cMinX
            Dim extentY As Double = cMaxY - cMinY
            Dim extentZ As Double = cMaxZ - cMinZ
            Dim axis As Integer = 0
            Dim maxExtent As Double = extentX
            If extentY > maxExtent Then axis = 1 : maxExtent = extentY
            If extentZ > maxExtent Then axis = 2 : maxExtent = extentZ

            ' 质心退化 (所有三角形质心重合) → 强制叶子，避免死循环
            If maxExtent <= 0.0 Then
                nodeLeft(nodeIdx) = -1
                nodeRight(nodeIdx) = -1
                nodeStart(nodeIdx) = start
                nodeCount(nodeIdx) = count
                Return nodeIdx
            End If

            ' 按分割轴对区间内三角形排序 (简单排序，构建为一次性开销)
            Array.Sort(tris, start, count, New CentroidComparer(axis))

            Dim mid As Integer = count \ 2

            ' 先占位，递归后再回填左右子索引 (nodeIdx 已被占用)
            Dim leftIdx As Integer = BuildRecursive(start, mid)
            Dim rightIdx As Integer = BuildRecursive(start + mid, count - mid)

            nodeLeft(nodeIdx) = leftIdx
            nodeRight(nodeIdx) = rightIdx
            nodeStart(nodeIdx) = 0
            nodeCount(nodeIdx) = 0

            Return nodeIdx
        End Function

        ''' <summary>
        ''' 查询空间点 <paramref name="p"/> 到网格的最近三角面。
        ''' </summary>
        ''' <param name="p">查询点 (世界坐标)</param>
        ''' <returns>
        ''' (triIndex, closest, distance)：最近三角形在内部数组中的索引、
        ''' 三角面上的最近点、以及到该最近点的欧几里得距离。
        ''' 若 BVH 为空则 triIndex = -1。
        ''' </returns>
        Public Function QueryNearest(p As Point3D) As (triIndex As Integer, closest As Point3D, distance As Double)
            Dim bestDistSq As Double = Double.MaxValue
            Dim bestClosest As Point3D = New Point3D(0, 0, 0)
            Dim bestTri As Integer = -1

            If rootIndex < 0 Then
                Return (-1, bestClosest, Double.MaxValue)
            End If

            ' 用显式栈做迭代遍历，避免深递归开销
            Dim stack As Integer() = New Integer(63) {}
            Dim sp As Integer = 0
            stack(sp) = rootIndex : sp += 1

            While sp > 0
                sp -= 1
                Dim node As Integer = stack(sp)

                ' 剪枝：若点到该节点 AABB 的距离下界 ≥ 当前最优，跳过
                Dim lower As Double = PointAABBDistanceSq(p, nodeMin(node), nodeMax(node))
                If lower >= bestDistSq Then
                    Continue While
                End If

                If nodeLeft(node) < 0 Then
                    ' 叶子：逐三角形精确求最近点
                    Dim s As Integer = nodeStart(node)
                    Dim c As Integer = nodeCount(node)

                    For i As Integer = s To s + c - 1
                        Dim tri = tris(i)
                        Dim cp = ClosestPtPointTriangle(p, tri.A, tri.B, tri.C)
                        Dim dx As Double = p.X - cp.X
                        Dim dy As Double = p.Y - cp.Y
                        Dim dz As Double = p.Z - cp.Z
                        Dim dSq As Double = dx * dx + dy * dy + dz * dz

                        If dSq < bestDistSq Then
                            bestDistSq = dSq
                            bestClosest = cp
                            bestTri = i
                        End If
                    Next
                Else
                    ' 内部节点：先压入距离较远的子，后压入较近的子 (较近的先被弹出处理)
                    Dim l As Integer = nodeLeft(node)
                    Dim r As Integer = nodeRight(node)
                    Dim dl As Double = PointAABBDistanceSq(p, nodeMin(l), nodeMax(l))
                    Dim dr As Double = PointAABBDistanceSq(p, nodeMin(r), nodeMax(r))

                    ' 动态扩容检查
                    If sp + 2 > stack.Length Then
                        ReDim Preserve stack(stack.Length * 2 - 1)
                    End If

                    If dl < dr Then
                        stack(sp) = r : sp += 1
                        stack(sp) = l : sp += 1
                    Else
                        stack(sp) = l : sp += 1
                        stack(sp) = r : sp += 1
                    End If
                End If
            End While

            Return (bestTri, bestClosest, std.Sqrt(bestDistSq))
        End Function

        ''' <summary>
        ''' 依据内部三角形索引取回三角面 (供符号判定读取法向)。
        ''' </summary>
        Public Function GetTriangle(index As Integer) As SDFTriangle
            Return tris(index)
        End Function

        ''' <summary>
        ''' 点到轴对齐包围盒 (AABB) 的最短距离平方。点在盒内时返回 0。
        ''' 作为最近三角面距离的下界，用于 BVH 剪枝。
        ''' </summary>
        Private Shared Function PointAABBDistanceSq(p As Point3D, boxMin As Point3D, boxMax As Point3D) As Double
            Dim dx As Double = 0, dy As Double = 0, dz As Double = 0

            If p.X < boxMin.X Then
                dx = boxMin.X - p.X
            ElseIf p.X > boxMax.X Then
                dx = p.X - boxMax.X
            End If
            If p.Y < boxMin.Y Then
                dy = boxMin.Y - p.Y
            ElseIf p.Y > boxMax.Y Then
                dy = p.Y - boxMax.Y
            End If
            If p.Z < boxMin.Z Then
                dz = boxMin.Z - p.Z
            ElseIf p.Z > boxMax.Z Then
                dz = p.Z - boxMax.Z
            End If

            Return dx * dx + dy * dy + dz * dz
        End Function

        ''' <summary>
        ''' 计算空间点 <paramref name="p"/> 在三角形 (a,b,c) 上的最近点。
        ''' </summary>
        ''' <remarks>
        ''' 基于重心坐标的 Voronoi 区域判定，参考
        ''' Ericson, C. (2004). Real-Time Collision Detection, §5.1.5。
        ''' </remarks>
        Public Shared Function ClosestPtPointTriangle(p As Point3D, a As Point3D, b As Point3D, c As Point3D) As Point3D
            Dim ab = b.Subtract(a)
            Dim ac = c.Subtract(a)
            Dim ap = p.Subtract(a)

            Dim d1 As Double = Point3D.Dot(ab, ap)
            Dim d2 As Double = Point3D.Dot(ac, ap)

            ' 顶点 a 的 Voronoi 区域
            If d1 <= 0.0 AndAlso d2 <= 0.0 Then
                Return a
            End If

            Dim bp = p.Subtract(b)
            Dim d3 As Double = Point3D.Dot(ab, bp)
            Dim d4 As Double = Point3D.Dot(ac, bp)

            ' 顶点 b 的 Voronoi 区域
            If d3 >= 0.0 AndAlso d4 <= d3 Then
                Return b
            End If

            ' 边 ab 的 Voronoi 区域
            Dim vc As Double = d1 * d4 - d3 * d2
            If vc <= 0.0 AndAlso d1 >= 0.0 AndAlso d3 <= 0.0 Then
                Dim v As Double = d1 / (d1 - d3)
                Return a.Add(ab.Multiply(v))
            End If

            Dim cp = p.Subtract(c)
            Dim d5 As Double = Point3D.Dot(ab, cp)
            Dim d6 As Double = Point3D.Dot(ac, cp)

            ' 顶点 c 的 Voronoi 区域
            If d6 >= 0.0 AndAlso d5 <= d6 Then
                Return c
            End If

            ' 边 ac 的 Voronoi 区域
            Dim vb As Double = d5 * d2 - d1 * d6
            If vb <= 0.0 AndAlso d2 >= 0.0 AndAlso d6 <= 0.0 Then
                Dim w As Double = d2 / (d2 - d6)
                Return a.Add(ac.Multiply(w))
            End If

            ' 边 bc 的 Voronoi 区域
            Dim va As Double = d3 * d6 - d5 * d4
            If va <= 0.0 AndAlso (d4 - d3) >= 0.0 AndAlso (d5 - d6) >= 0.0 Then
                Dim w As Double = (d4 - d3) / ((d4 - d3) + (d5 - d6))
                Return b.Add(c.Subtract(b).Multiply(w))
            End If

            ' 面内部：用重心坐标合成
            Dim denom As Double = 1.0 / (va + vb + vc)
            Dim vv As Double = vb * denom
            Dim ww As Double = vc * denom
            Return a.Add(ab.Multiply(vv)).Add(ac.Multiply(ww))
        End Function

        ''' <summary>
        ''' 按指定坐标轴比较三角形质心，用于 BVH 中位数分割排序。
        ''' </summary>
        Private Class CentroidComparer : Implements IComparer(Of SDFTriangle)

            Private ReadOnly axis As Integer

            Public Sub New(axis As Integer)
                Me.axis = axis
            End Sub

            Public Function Compare(x As SDFTriangle, y As SDFTriangle) As Integer Implements IComparer(Of SDFTriangle).Compare
                Dim vx As Double, vy As Double

                Select Case axis
                    Case 0 : vx = x.Centroid.X : vy = y.Centroid.X
                    Case 1 : vx = x.Centroid.Y : vy = y.Centroid.Y
                    Case Else : vx = x.Centroid.Z : vy = y.Centroid.Z
                End Select

                Return vx.CompareTo(vy)
            End Function

        End Class

    End Class
End Namespace
