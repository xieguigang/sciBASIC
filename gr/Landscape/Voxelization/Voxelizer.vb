#Region "Microsoft.VisualBasic::d901888725f5910781720a4c2fcd3bff, gr\Landscape\Voxelization\Voxelizer.vb"

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

    '   Total Lines: 679
    '    Code Lines: 404 (59.50%)
    ' Comment Lines: 156 (22.97%)
    '    - Xml Docs: 74.36%
    ' 
    '   Blank Lines: 119 (17.53%)
    '     File Size: 31.36 KB


    '     Module Voxelizer
    ' 
    '         Function: BuildBatch, BuildColumnBins, ClampIndex, CollectAllVertices, ComputeBoundingBox
    '                   ComputeVoxelGridDimensions, ExtractTriangles, MergeCloseIntersections, ScalarIntersect, (+2 Overloads) Voxelize
    '                   VoxelizeTriangles
    ' 
    '         Sub: CollectIntersections
    '     Class TriangleBatch
    ' 
    ' 
    ' 
    '     Class ColumnBins
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Numerics
Imports System.Threading.Tasks
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Landscape.Data
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Voxelization

    ''' <summary>
    ''' 三角网格模型 → CFD 实心体素模型的转换器。
    ''' 
    ''' 接受任意格式（STL、OBJ、glTF/GLB、COLLADA、3DS、3MF）解析后的
    ''' <see cref="Data.SceneModel"/> 作为输入，将其转换为可用于 CFD
    ''' 流体动力学仿真的实心体素模型。
    '''
    ''' ## 体素化算法
    '''
    ''' 采用 **列扫描射线投射 (Column-based Ray Casting)** 算法：
    '''
    ''' 1. 对体素网格中的每一列 ``(x, y)``，沿 Z 轴正方向发射一条射线
    ''' 2. 使用 Möller–Trumbore 算法计算射线与每个三角面的交点
    ''' 3. 收集当前列上所有交点的 Z 坐标并排序
    ''' 4. 交点对 (z0,z1), (z2,z3), ... 之间的体素标记为 ``True``（固体内部）
    ''' 5. 最终体素模型为实心体，CFD 引擎在 True/False 边界上做流体计算
    '''
    ''' ## 前提条件
    '''
    ''' - 输入模型必须是水密的 (watertight / manifold)，否则射线可能穿透内部
    ''' - 如果模型有孔洞，内部区域可能被标记为外部（False），请预先修复模型
    ''' </summary>
    Public Module Voxelizer

#Region "公共 API —— 体素化入口"

        ''' <summary>
        ''' 以指定的分辨率（最长边体素数量）将 SceneModel 体素化为实心体素模型。
        ''' </summary>
        ''' <param name="model">已解析的 3D 场景模型（任意格式转换后的 SceneModel）</param>
        ''' <param name="resolution">最长边上的体素分辨率，默认 128。实际各轴体素数量按比例缩放</param>
        ''' <returns>实心体素模型，若输入无效则返回 Nothing</returns>
        <Extension>
        Public Function Voxelize(model As Data.SceneModel, Optional resolution As Integer = 128) As VoxelModel
            If model Is Nothing Then Return Nothing
            If model.Surfaces Is Nothing OrElse model.Surfaces.Length = 0 Then Return Nothing

            Dim allVerts = CollectAllVertices(model)
            If allVerts Is Nothing OrElse allVerts.Length = 0 Then Return Nothing

            ' 计算包围盒并添加微小边距
            Dim bbox As (minX#, minY#, minZ#, maxX#, maxY#, maxZ#) = ComputeBoundingBox(allVerts)

            Dim margin As Double = (bbox.maxX - bbox.minX) * 0.001
            If margin < 0.0001 Then margin = 0.0001
            bbox.minX -= margin : bbox.minY -= margin : bbox.minZ -= margin
            bbox.maxX += margin : bbox.maxY += margin : bbox.maxZ += margin

            ' 计算体素网格维度
            Dim dims As (width%, height%, depth%, voxelSize#) = ComputeVoxelGridDimensions(bbox.maxX - bbox.minX, bbox.maxY - bbox.minY, bbox.maxZ - bbox.minZ, resolution)
            ' 提取三角形数据
            Dim triangles = ExtractTriangles(model)

            ' 执行体素化
            Dim shape = VoxelizeTriangles(triangles, dims.width, dims.height, dims.depth,
                                         bbox.minX, bbox.minY, bbox.minZ, dims.voxelSize)

            Return New VoxelModel With {
                .Shape = shape,
                .Width = dims.width,
                .Height = dims.height,
                .Depth = dims.depth,
                .MinX = bbox.minX,
                .MinY = bbox.minY,
                .MinZ = bbox.minZ,
                .VoxelSize = dims.voxelSize
            }
        End Function

        ''' <summary>
        ''' 以指定的体素物理尺寸将 SceneModel 体素化为实心体素模型。
        ''' </summary>
        ''' <param name="model">已解析的 3D 场景模型（任意格式转换后的 SceneModel）</param>
        ''' <param name="voxelSize">每个体素的物理尺寸（世界坐标单位）</param>
        ''' <returns>实心体素模型，若输入无效则返回 Nothing</returns>
        <Extension>
        Public Function Voxelize(model As Data.SceneModel, voxelSize As Double) As VoxelModel
            If model Is Nothing Then Return Nothing
            If model.Surfaces Is Nothing OrElse model.Surfaces.Length = 0 Then Return Nothing

            Dim allVerts = CollectAllVertices(model)
            If allVerts Is Nothing OrElse allVerts.Length = 0 Then Return Nothing

            If voxelSize <= 0 Then
                Throw New ArgumentException("voxelSize 必须大于 0", NameOf(voxelSize))
            End If

            Dim bbox As (minX#, minY#, minZ#, maxX#, maxY#, maxZ#) = ComputeBoundingBox(allVerts)

            Dim margin As Double = (bbox.maxX - bbox.minX) * 0.001
            If margin < 0.0001 Then margin = 0.0001
            bbox.minX -= margin : bbox.minY -= margin : bbox.minZ -= margin
            bbox.maxX += margin : bbox.maxY += margin : bbox.maxZ += margin

            Dim width As Integer = std.Max(1, CInt(std.Ceiling((bbox.maxX - bbox.minX) / voxelSize)))
            Dim height As Integer = std.Max(1, CInt(std.Ceiling((bbox.maxY - bbox.minY) / voxelSize)))
            Dim depth As Integer = std.Max(1, CInt(std.Ceiling((bbox.maxZ - bbox.minZ) / voxelSize)))

            Dim triangles = ExtractTriangles(model)
            Dim shape = VoxelizeTriangles(triangles, width, height, depth,
                                          bbox.minX, bbox.minY, bbox.minZ, voxelSize)

            Return New VoxelModel With {
                .Shape = shape,
                .Width = width,
                .Height = height,
                .Depth = depth,
                .MinX = bbox.minX,
                .MinY = bbox.minY,
                .MinZ = bbox.minZ,
                .VoxelSize = voxelSize
            }
        End Function

#End Region

#Region "内部辅助方法 —— 顶点收集与包围盒"

        ''' <summary>
        ''' 从 SceneModel 中收集所有三角面的顶点坐标（展平为一维数组）
        ''' </summary>
        Private Function CollectAllVertices(model As Data.SceneModel) As Point3D()
            Dim list As New List(Of Point3D)

            For Each surf In model.Surfaces
                If surf.vertices Is Nothing Then Continue For
                For Each v As Vertex In surf.vertices
                    If v.Point3D IsNot Nothing Then
                        list.Add(v.PointData)
                    End If
                Next
            Next

            Return list.ToArray
        End Function

        ''' <summary>
        ''' 计算点集的轴对齐包围盒（SoA 化 + SIMD 归约，SimdReduce 基于 Vector(Of Double)）
        ''' </summary>
        ''' <returns>(minX, minY, minZ, maxX, maxY, maxZ)</returns>
        Private Function ComputeBoundingBox(vertices As Point3D()) As (minX As Double, minY As Double, minZ As Double, maxX As Double, maxY As Double, maxZ As Double)
            Dim n As Integer = vertices.Length
            Dim xs(n - 1) As Double
            Dim ys(n - 1) As Double
            Dim zs(n - 1) As Double

            For i As Integer = 0 To n - 1
                xs(i) = vertices(i).X
                ys(i) = vertices(i).Y
                zs(i) = vertices(i).Z
            Next

            ' SimdReduce：Vector(Of Double) 逐块 Min/Max，顺序确定性内核
            Return (SimdReduce.Min(xs), SimdReduce.Min(ys), SimdReduce.Min(zs),
                    SimdReduce.Max(xs), SimdReduce.Max(ys), SimdReduce.Max(zs))
        End Function

        ''' <summary>
        ''' 根据模型尺寸和分辨率计算各轴体素数量与体素尺寸。
        ''' 保证等轴测（voxel 在 X/Y/Z 方向尺寸相同）。
        ''' </summary>
        Private Function ComputeVoxelGridDimensions(sizeX As Double, sizeY As Double, sizeZ As Double, resolution As Integer) As (width As Integer, height As Integer, depth As Integer, voxelSize As Double)
            Dim maxSize As Double = std.Max(std.Max(sizeX, sizeY), sizeZ)
            Dim voxelSize As Double = maxSize / resolution

            Dim width As Integer = std.Max(1, CInt(std.Ceiling(sizeX / voxelSize)))
            Dim height As Integer = std.Max(1, CInt(std.Ceiling(sizeY / voxelSize)))
            Dim depth As Integer = std.Max(1, CInt(std.Ceiling(sizeZ / voxelSize)))

            Return (width, height, depth, voxelSize)
        End Function

#End Region

#Region "内部辅助方法 —— 三角形数据提取"

        ''' <summary>
        ''' 将 SceneModel 中的三角面数组转换为 (v1, v2, v3) 三角形元组列表
        ''' </summary>
        Private Function ExtractTriangles(model As Data.SceneModel) As List(Of (v1 As Point3D, v2 As Point3D, v3 As Point3D))
            Dim list As New List(Of (v1 As Point3D, v2 As Point3D, v3 As Point3D))

            For Each surf In model.Surfaces
                If surf.vertices Is Nothing OrElse surf.vertices.Length < 3 Then Continue For

                ' SceneModel 中每个 Surface 正好是 3 个顶点的三角形
                Dim v1 = surf.vertices(0).PointData
                Dim v2 = surf.vertices(1).PointData
                Dim v3 = surf.vertices(2).PointData

                list.Add((v1, v2, v3))
            Next

            Return list
        End Function

#End Region

#Region "核心算法 —— 三角形常量预计算（SoA）与 XY 空间分箱"

    ''' <summary>
    ''' Möller–Trumbore 退化判定阈值（与原实现一致的 EPSILON）
    ''' </summary>
    Friend Const RayEpsilon As Double = 0.0000001

    ''' <summary>
    ''' 三角形批量求交的预计算数据（SoA 布局）。
    '''
    ''' 列扫描的射线方向固定为 +Z，因此
    ''' <c>h = rayDir × e2 = (−e2y, e2x, 0)</c>，从而
    ''' <c>a = e1·h</c> 与 <c>f = 1/a</c> 均为三角形常量；
    ''' 又因所有射线的起点 Z 坐标相同（<c>rayOriginZ</c>），
    ''' <c>sz = rayOriginZ − v0z</c> 亦为常量。
    ''' 预计算后每对（射线，三角形）的求交化简为纯标量乘加，
    ''' 且可对整段 SoA 数据做 <see cref="Vector(Of Double)"/> 批量运算。
    ''' </summary>
    Private Class TriangleBatch

        ''' <summary>通过退化过滤（|a| ≥ eps）的三角形数量。</summary>
        Public Count As Integer

        Public V0x As Double(), V0y As Double(), Sz As Double()
        ''' <summary>1/a（行列式倒数）。</summary>
        Public F As Double()
        Public E1x As Double(), E1y As Double(), E1z As Double()
        Public E2x As Double(), E2y As Double(), E2z As Double()

        ''' <summary>三角形 XY 包围盒（世界坐标，分箱用）。</summary>
        Public BMinX As Double(), BMaxX As Double(), BMinY As Double(), BMaxY As Double()

    End Class

    ''' <summary>
    ''' 从三角形列表构建 SoA 预计算批数据：
    ''' 预计算 f = 1/(e1·h)、sz = rayOriginZ − v0z 与 XY 包围盒，
    ''' 过滤 |a| &lt; eps 的退化三角形（与原 RayTriangleIntersect 行为一致）。
    ''' </summary>
    Private Function BuildBatch(triangles As List(Of (v1 As Point3D, v2 As Point3D, v3 As Point3D)),
                                rayOriginZ As Double) As TriangleBatch
        Dim n As Integer = triangles.Count
        Dim b As New TriangleBatch With {
            .Count = 0,
            .V0x = New Double(n - 1) {}, .V0y = New Double(n - 1) {}, .Sz = New Double(n - 1) {},
            .F = New Double(n - 1) {},
            .E1x = New Double(n - 1) {}, .E1y = New Double(n - 1) {}, .E1z = New Double(n - 1) {},
            .E2x = New Double(n - 1) {}, .E2y = New Double(n - 1) {}, .E2z = New Double(n - 1) {},
            .BMinX = New Double(n - 1) {}, .BMaxX = New Double(n - 1) {},
            .BMinY = New Double(n - 1) {}, .BMaxY = New Double(n - 1) {}
        }

        For Each tri In triangles
            Dim v0x = tri.v1.X, v0y = tri.v1.Y, v0z = tri.v1.Z
            Dim e1x = tri.v2.X - v0x, e1y = tri.v2.Y - v0y, e1z = tri.v2.Z - v0z
            Dim e2x = tri.v3.X - v0x, e2y = tri.v3.Y - v0y, e2z = tri.v3.Z - v0z

            ' h = rayDir × e2 = (−e2y, e2x, 0)，a = e1·h
            Dim a As Double = -e1x * e2y + e1y * e2x

            If std.Abs(a) < RayEpsilon Then
                Continue For   ' 射线平行于三角形平面，永不相交
            End If

            Dim m As Integer = b.Count

            b.V0x(m) = v0x
            b.V0y(m) = v0y
            b.Sz(m) = rayOriginZ - v0z
            b.F(m) = 1.0 / a
            b.E1x(m) = e1x : b.E1y(m) = e1y : b.E1z(m) = e1z
            b.E2x(m) = e2x : b.E2y(m) = e2y : b.E2z(m) = e2z
            b.BMinX(m) = std.Min(v0x, std.Min(v0x + e1x, v0x + e2x))
            b.BMaxX(m) = std.Max(v0x, std.Max(v0x + e1x, v0x + e2x))
            b.BMinY(m) = std.Min(v0y, std.Min(v0y + e1y, v0y + e2y))
            b.BMaxY(m) = std.Max(v0y, std.Max(v0y + e1y, v0y + e2y))

            b.Count = m + 1
        Next

        Return b
    End Function

    ''' <summary>
    ''' XY 空间分箱结果（CSR 紧凑布局）：
    ''' 列 c 的三角形批数据位于各 SoA 数组的 [ColPtr(c), ColPtr(c+1)) 区间，
    ''' 列内连续 —— 使单列批量求交可以走无 gather 的
    ''' <see cref="Vector(Of Double)"/> 切片运算。
    ''' </summary>
    Private Class ColumnBins

        ''' <summary>列前缀和（长度 width*height + 1）。</summary>
        Public ColPtr As Integer()

        ''' <summary>分箱后的三角形常量总数。</summary>
        Public Total As Integer

        Public F As Double(), V0x As Double(), V0y As Double(), Sz As Double()
        Public E1x As Double(), E1y As Double(), E1z As Double()
        Public E2x As Double(), E2y As Double(), E2z As Double()

    End Class

    ''' <summary>
    ''' 按三角形 XY 包围盒落入的体素列区间建桶（CSR 两趟填充）：
    ''' 每列只测试覆盖它的三角形，消除「每列遍历全部三角形」的无效求交。
    ''' </summary>
    Private Function BuildColumnBins(b As TriangleBatch,
                                     width As Integer, height As Integer,
                                     minX As Double, minY As Double,
                                     voxelSize As Double) As ColumnBins
        Dim cols As Integer = width * height
        Dim counts(cols) As Integer   ' 末位留给前缀和
        Dim invVoxel As Double = 1.0 / voxelSize

        ' ---- Pass 1：统计每列桶大小 ----
        For t As Integer = 0 To b.Count - 1
            Dim x0 As Integer = ClampIndex(std.Floor((b.BMinX(t) - minX) * invVoxel - 0.5), width)
            Dim x1 As Integer = ClampIndex(std.Floor((b.BMaxX(t) - minX) * invVoxel - 0.5), width)
            Dim y0 As Integer = ClampIndex(std.Floor((b.BMinY(t) - minY) * invVoxel - 0.5), height)
            Dim y1 As Integer = ClampIndex(std.Floor((b.BMaxY(t) - minY) * invVoxel - 0.5), height)

            For x As Integer = x0 To x1
                Dim rowBase As Integer = x * height
                For y As Integer = y0 To y1
                    counts(rowBase + y) += 1
                Next
            Next
        Next

        ' ---- 前缀和 ----
        Dim bins As New ColumnBins With {.ColPtr = New Integer(cols) {}}

        For c As Integer = 0 To cols - 1
            bins.ColPtr(c + 1) = bins.ColPtr(c) + counts(c)
        Next

        bins.Total = bins.ColPtr(cols)

        If bins.Total = 0 Then
            Return bins
        End If

        ' ---- CSR SoA 分配 ----
        Dim total As Integer = bins.Total

        bins.F = New Double(total - 1) {}
        bins.V0x = New Double(total - 1) {} : bins.V0y = New Double(total - 1) {}
        bins.Sz = New Double(total - 1) {}
        bins.E1x = New Double(total - 1) {} : bins.E1y = New Double(total - 1) {} : bins.E1z = New Double(total - 1) {}
        bins.E2x = New Double(total - 1) {} : bins.E2y = New Double(total - 1) {} : bins.E2z = New Double(total - 1) {}

        Dim cursor(cols - 1) As Integer
        Call Array.Copy(bins.ColPtr, cursor, cols)

        ' ---- Pass 2：按同一遍历顺序填充（保证与 Pass 1 计数一致）----
        For t As Integer = 0 To b.Count - 1
            Dim x0 As Integer = ClampIndex(std.Floor((b.BMinX(t) - minX) * invVoxel - 0.5), width)
            Dim x1 As Integer = ClampIndex(std.Floor((b.BMaxX(t) - minX) * invVoxel - 0.5), width)
            Dim y0 As Integer = ClampIndex(std.Floor((b.BMinY(t) - minY) * invVoxel - 0.5), height)
            Dim y1 As Integer = ClampIndex(std.Floor((b.BMaxY(t) - minY) * invVoxel - 0.5), height)

            For x As Integer = x0 To x1
                Dim rowBase As Integer = x * height
                For y As Integer = y0 To y1
                    Dim p As Integer = cursor(rowBase + y)
                    cursor(rowBase + y) = p + 1

                    bins.F(p) = b.F(t)
                    bins.V0x(p) = b.V0x(t) : bins.V0y(p) = b.V0y(t)
                    bins.Sz(p) = b.Sz(t)
                    bins.E1x(p) = b.E1x(t) : bins.E1y(p) = b.E1y(t) : bins.E1z(p) = b.E1z(t)
                    bins.E2x(p) = b.E2x(t) : bins.E2y(p) = b.E2y(t) : bins.E2z(p) = b.E2z(t)
                Next
            Next
        Next

        Return bins
    End Function

    ''' <summary>体素列索引夹取到 [0, n-1]。</summary>
    Private Function ClampIndex(v As Double, n As Integer) As Integer
        Dim i As Integer = CInt(v)
        If i < 0 Then i = 0
        If i > n - 1 Then i = n - 1
        Return i
    End Function

#End Region

#Region "核心算法 —— SIMD 批量射线-三角形求交"

    ''' <summary>
    ''' 对单列的一个连续分箱段（SoA，[offset, offset+count)）做
    ''' <see cref="Vector(Of Double)"/> 批量 Möller–Trumbore 求交：
    '''
    ''' 对固定 +Z 射线（起点 (ox, oy, rayOriginZ)）：
    ''' <code>
    ''' s  = (ox − v0x, oy − v0y, rayOriginZ − v0z)   [z 分量为三角形常量 sz]
    ''' u  = f·(−sx·e2y + sy·e2x)                     [= f·(s·h)]
    ''' v  = f·( sx·e1y − sy·e1x)                     [= f·(rayDir·q), q = s×e1]
    ''' t  = f·(e2·q)
    ''' </code>
    ''' u/v/t 全部以向量切片计算，掩码筛出命中通道后标量复核收集交点 Z 值。
    ''' </summary>
    Private Sub CollectIntersections(bins As ColumnBins, offset As Integer, count As Integer,
                                     ox As Double, oy As Double, rayOriginZ As Double,
                                     hits As List(Of Double))
        Dim f = bins.F, v0x = bins.V0x, v0y = bins.V0y, sz = bins.Sz
        Dim e1x = bins.E1x, e1y = bins.E1y, e1z = bins.E1z
        Dim e2x = bins.E2x, e2y = bins.E2y, e2z = bins.E2z

        Dim vOx As New Vector(Of Double)(ox)
        Dim vOy As New Vector(Of Double)(oy)
        Dim vZero As New Vector(Of Double)(0.0)
        Dim vOne As New Vector(Of Double)(1.0)
        Dim vEps As New Vector(Of Double)(RayEpsilon)

        Dim W As Integer = Vector(Of Double).Count
        Dim i As Integer = offset
        Dim vecEnd As Integer = offset + (count \ W) * W

        ' ---- SIMD 主批处理 ----
        While i < vecEnd
            Dim svx As Vector(Of Double) = Vector.Subtract(vOx, New Vector(Of Double)(v0x, i))
            Dim svy As Vector(Of Double) = Vector.Subtract(vOy, New Vector(Of Double)(v0y, i))
            Dim fv As New Vector(Of Double)(f, i)
            Dim e1xv As New Vector(Of Double)(e1x, i)
            Dim e1yv As New Vector(Of Double)(e1y, i)
            Dim e1zv As New Vector(Of Double)(e1z, i)
            Dim e2xv As New Vector(Of Double)(e2x, i)
            Dim e2yv As New Vector(Of Double)(e2y, i)
            Dim e2zv As New Vector(Of Double)(e2z, i)
            Dim szv As New Vector(Of Double)(sz, i)

            ' u = f·(−sx·e2y + sy·e2x)
            Dim u As Vector(Of Double) = Vector.Multiply(fv,
                Vector.Add(Vector.Multiply(Vector.Negate(svx), e2yv),
                           Vector.Multiply(svy, e2xv)))

            ' v = f·(sx·e1y − sy·e1x)
            Dim v As Vector(Of Double) = Vector.Multiply(fv,
                Vector.Subtract(Vector.Multiply(svx, e1yv),
                                Vector.Multiply(svy, e1xv)))

            ' q = s×e1, t = f·(e2·q)
            Dim qx As Vector(Of Double) = Vector.Subtract(Vector.Multiply(svy, e1zv), Vector.Multiply(szv, e1yv))
            Dim qy As Vector(Of Double) = Vector.Subtract(Vector.Multiply(szv, e1xv), Vector.Multiply(svx, e1zv))
            Dim qz As Vector(Of Double) = Vector.Subtract(Vector.Multiply(svx, e1yv), Vector.Multiply(svy, e1xv))
            Dim t As Vector(Of Double) = Vector.Multiply(fv,
                Vector.Add(Vector.Add(Vector.Multiply(e2xv, qx), Vector.Multiply(e2yv, qy)),
                           Vector.Multiply(e2zv, qz)))

            ' 掩码：u∈[0,1] && v≥0 && u+v≤1 && t>eps
            Dim m As Vector(Of Double) =
                Vector.BitwiseAnd(
                    Vector.BitwiseAnd(Vector.GreaterThanOrEqual(u, vZero),
                                      Vector.LessThanOrEqual(u, vOne)),
                    Vector.BitwiseAnd(
                        Vector.BitwiseAnd(Vector.GreaterThanOrEqual(v, vZero),
                                          Vector.LessThanOrEqual(Vector.Add(u, v), vOne)),
                        Vector.GreaterThan(t, vEps)))

            If Not Vector.EqualsAll(m, vZero) Then
                For k As Integer = 0 To W - 1
                    If m(k) <> 0.0 Then
                        ' 标量复核（同一公式，过滤掩码通道的浮点边缘差异）
                        Dim hit As Double = ScalarIntersect(f, v0x, v0y, sz, e1x, e1y, e1z,
                                                            e2x, e2y, e2z, i + k, ox, oy)
                        If Not Double.IsNaN(hit) Then
                            hits.Add(rayOriginZ + hit)
                        End If
                    End If
                Next
            End If

            i += W
        End While

        ' ---- 标量尾部 ----
        While i < offset + count
            Dim hit As Double = ScalarIntersect(f, v0x, v0y, sz, e1x, e1y, e1z,
                                                e2x, e2y, e2z, i, ox, oy)
            If Not Double.IsNaN(hit) Then
                hits.Add(rayOriginZ + hit)
            End If
            i += 1
        End While
    End Sub

    ''' <summary>
    ''' 标量 Möller–Trumbore 求交（使用预计算常量）。
    ''' 返回参数化距离 t；不相交返回 NaN。
    ''' </summary>
    Private Function ScalarIntersect(f As Double(), v0x As Double(), v0y As Double(), sz As Double(),
                                            e1x As Double(), e1y As Double(), e1z As Double(),
                                            e2x As Double(), e2y As Double(), e2z As Double(),
                                            t As Integer, ox As Double, oy As Double) As Double
        Dim sx As Double = ox - v0x(t)
        Dim sy As Double = oy - v0y(t)
        Dim ff As Double = f(t)

        Dim u As Double = ff * (-sx * e2y(t) + sy * e2x(t))
        If u < 0.0 OrElse u > 1.0 Then
            Return Double.NaN
        End If

        Dim v As Double = ff * (sx * e1y(t) - sy * e1x(t))
        If v < 0.0 OrElse u + v > 1.0 Then
            Return Double.NaN
        End If

        Dim szt As Double = sz(t)
        Dim qx As Double = sy * e1z(t) - szt * e1y(t)
        Dim qy As Double = szt * e1x(t) - sx * e1z(t)
        Dim qz As Double = sx * e1y(t) - sy * e1x(t)
        Dim result As Double = ff * (e2x(t) * qx + e2y(t) * qy + e2z(t) * qz)

        Return If(result > RayEpsilon, result, Double.NaN)
    End Function

#End Region

#Region "核心算法 —— 列扫描体素化"

        ''' <summary>
        ''' 列扫描体素化的内部实现（并行 + SIMD 版本）。
        '''
        ''' 优化结构：
        '''     1. 三角形常量预计算（SoA）：h/a/f/sz 只算一次，求交化简为纯乘加；
        '''     2. XY 空间分箱（CSR）：每列只测试覆盖它的三角形；
        '''     3. Parallel.For 按 X 行并行：各列写 shape 的索引区间不相交，
        '''        交点列表 / 排序 / 合并全部线程局部化；
        '''     4. 列内连续分箱段走 Vector(Of Double) 批量求交（标量复核 + 尾部）。
        '''
        ''' 结果语义与原串行实现一致（浮点求交顺序差异可能引起
        ''' 边界格点的微量差异，见公共 API 文档）。
        ''' </summary>
        ''' <param name="triangles">三角面列表（已从 SceneModel 中提取）</param>
        ''' <param name="width">X 轴体素数量</param>
        ''' <param name="height">Y 轴体素数量</param>
        ''' <param name="depth">Z 轴体素数量</param>
        ''' <param name="minX">模型包围盒 X 最小值（世界坐标）</param>
        ''' <param name="minY">模型包围盒 Y 最小值（世界坐标）</param>
        ''' <param name="minZ">模型包围盒 Z 最小值（世界坐标）</param>
        ''' <param name="voxelSize">体素物理尺寸</param>
        ''' <returns>体素化后的一维 Boolean 数组</returns>
        Private Function VoxelizeTriangles(
                triangles As List(Of (v1 As Point3D, v2 As Point3D, v3 As Point3D)),
                width As Integer,
                height As Integer,
                depth As Integer,
                minX As Double,
                minY As Double,
                minZ As Double,
                voxelSize As Double) As Boolean()

            Dim totalVoxels As Long = CLng(width) * height * depth
            Dim shape As Boolean() = New Boolean(CInt(totalVoxels - 1)) {}

            If triangles.Count = 0 Then
                Return shape
            End If

            ' 射线起点：列中心，Z 轴从包围盒下方略微偏移开始（与原实现一致）
            Dim rayOriginZ As Double = minZ - voxelSize * 0.5

            ' ---- 1. 常量预计算 + 2. XY 空间分箱 ----
            Dim batch As TriangleBatch = BuildBatch(triangles, rayOriginZ)

            If batch.Count = 0 Then
                Return shape
            End If

            Dim bins As ColumnBins = BuildColumnBins(batch, width, height, minX, minY, voxelSize)

            ' ---- 3. 按行并行扫描 ----
            System.Threading.Tasks.Parallel.For(0, width,
                Sub(x As Integer)
                    ' 线程局部交点列表（原共享 List 在并行下不安全，已局部化）
                    Dim intersections As New List(Of Double)(capacity:=64)
                    Dim worldX As Double = minX + (x + 0.5) * voxelSize
                    Dim rowBase As Integer = x * height

                    For y As Integer = 0 To height - 1
                        Dim col As Integer = rowBase + y
                        Dim p0 As Integer = bins.ColPtr(col)
                        Dim cnt As Integer = bins.ColPtr(col + 1) - p0
                        intersections.Clear()

                        ' 与该列覆盖的三角形求交，收集交点 Z 坐标
                        If cnt > 0 Then
                            Call CollectIntersections(bins, p0, cnt,
                                                      worldX, minY + (y + 0.5) * voxelSize,
                                                      rayOriginZ, intersections)
                        End If

                        ' 如果没有交点或只有一个交点（射线擦过边缘），整列标记为 False
                        If intersections.Count < 2 Then
                            Continue For
                        End If

                        ' 按 Z 坐标排序交点
                        intersections.Sort()

                        ' 过滤掉过于接近的重复交点（同一顶点/边的多次命中）
                        intersections = MergeCloseIntersections(intersections, voxelSize * 0.01)

                        ' 交点对之间的体素标记为 True（固体内部）
                        ' z0,z1 → internal, z2,z3 → internal, ...
                        For i As Integer = 0 To intersections.Count - 1 Step 2
                            If i + 1 >= intersections.Count Then Exit For

                            Dim zEntry As Double = intersections(i)
                            Dim zExit As Double = intersections(i + 1)

                            ' 找到对应的体素 Z 索引范围
                            Dim zStart As Integer = CInt(std.Floor((zEntry - minZ) / voxelSize))
                            Dim zEnd As Integer = CInt(std.Ceiling((zExit - minZ) / voxelSize)) - 1

                            If zStart < 0 Then zStart = 0
                            If zEnd >= depth Then zEnd = depth - 1

                            For z As Integer = zStart To zEnd
                                If z >= 0 AndAlso z < depth Then
                                    Dim idx As Integer = (x * height + y) * depth + z
                                    shape(idx) = True
                                End If
                            Next
                        Next
                    Next
                End Sub)

            Return shape
        End Function

        ''' <summary>
        ''' 合并因边/顶点共享而导致的重复交点。
        ''' 当两个交点的 Z 坐标差距小于阈值时，视为同一交点。
        ''' </summary>
        Private Function MergeCloseIntersections(intersections As List(Of Double), threshold As Double) As List(Of Double)
            If intersections.Count <= 1 Then Return intersections

            Dim merged As New List(Of Double)(intersections.Count)

            Dim i As Integer = 0
            While i < intersections.Count
                Dim current As Double = intersections(i)
                Dim sum As Double = current
                Dim count As Integer = 1

                ' 聚合所有在阈值范围内的连续交点
                Dim j As Integer = i + 1
                While j < intersections.Count AndAlso std.Abs(intersections(j) - current) < threshold
                    sum += intersections(j)
                    count += 1
                    j += 1
                End While

                merged.Add(sum / count)  ' 取平均值作为合并后的交点
                i = j
            End While

            Return merged
        End Function

#End Region

    End Module
End Namespace
