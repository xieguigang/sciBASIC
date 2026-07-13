#Region "Microsoft.VisualBasic::Voxelizer, gr\Landscape\Voxelization\Voxelizer.vb"

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

    '   Total Lines: 513
    '    Code Lines: 337 (65.69%)
    ' Comment Lines: 110 (21.44%)
    '    - Xml Docs: 100.00%
    '
    '   Blank Lines: 66 (12.87%)
    '     File Size: 19.2 KB


    '     Module Voxelizer
    '
    '         Function: (+2 Overloads) Voxelize
    '
    '         (内部算法辅助方法)
    '         Function: CollectAllVertices, ComputeBoundingBox, ExtractTriangles,
    '                   RayTriangleIntersect, VoxelizeTriangles,
    '                   ComputeVoxelGridDimensions


    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Landscape.Data
Imports Microsoft.VisualBasic.Language
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
        ''' 计算点集的轴对齐包围盒
        ''' </summary>
        ''' <returns>(minX, minY, minZ, maxX, maxY, maxZ)</returns>
        Private Function ComputeBoundingBox(vertices As Point3D()) As (minX As Double, minY As Double, minZ As Double, maxX As Double, maxY As Double, maxZ As Double)
            Dim minX As Double = Double.MaxValue
            Dim minY As Double = Double.MaxValue
            Dim minZ As Double = Double.MaxValue
            Dim maxX As Double = Double.MinValue
            Dim maxY As Double = Double.MinValue
            Dim maxZ As Double = Double.MinValue

            For Each v In vertices
                If v.X < minX Then minX = v.X
                If v.Y < minY Then minY = v.Y
                If v.Z < minZ Then minZ = v.Z
                If v.X > maxX Then maxX = v.X
                If v.Y > maxY Then maxY = v.Y
                If v.Z > maxZ Then maxZ = v.Z
            Next

            Return (minX, minY, minZ, maxX, maxY, maxZ)
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

#Region "核心算法 —— Möller–Trumbore 射线-三角形相交"

        ''' <summary>
        ''' Möller–Trumbore 射线-三角形快速相交检测。
        ''' </summary>
        ''' <param name="rayOrigin">射线起点</param>
        ''' <param name="rayDir">射线方向（不需要单位化）</param>
        ''' <param name="v0">三角形顶点0</param>
        ''' <param name="v1">三角形顶点1</param>
        ''' <param name="v2">三角形顶点2</param>
        ''' <param name="t">输出：交点到射线起点的参数化距离</param>
        ''' <returns>True 表示射线与三角形相交，结果写入 t；否则 False</returns>
        ''' <remarks>
        ''' 参考资料: Möller, T., &amp; Trumbore, B. (1997).
        ''' "Fast, Minimum Storage Ray-Triangle Intersection."
        ''' Journal of Graphics Tools, 2(1), 21–28.
        ''' </remarks>
        Private Function RayTriangleIntersect(
                rayOrigin As Point3D,
                rayDir As Point3D,
                v0 As Point3D,
                v1 As Point3D,
                v2 As Point3D,
                ByRef t As Double) As Boolean

            Const EPSILON As Double = 0.0000001

            ' 边向量
            Dim edge1 = v1.subtract(v0)   ' v1 - v0
            Dim edge2 = v2.subtract(v0)   ' v2 - v0

            ' h = rayDir × edge2
            Dim h = Point3D.Cross(rayDir, edge2)

            ' a = edge1 · h  (行列式的值)
            Dim a = Point3D.Dot(edge1, h)

            ' 射线平行于三角形平面 → 不相交
            If std.Abs(a) < EPSILON Then
                Return False
            End If

            Dim f = 1.0 / a

            ' s = rayOrigin - v0
            Dim s = rayOrigin.subtract(v0)

            ' u = f * (s · h)
            Dim u = f * Point3D.Dot(s, h)

            ' u 超出 [0, 1] → 不相交
            If u < 0.0 OrElse u > 1.0 Then
                Return False
            End If

            ' q = s × edge1
            Dim q = Point3D.Cross(s, edge1)

            ' v = f * (rayDir · q)
            Dim v = f * Point3D.Dot(rayDir, q)

            ' v 超出 [0, 1] 或者 u+v 超出 [0, 1] → 不相交
            If v < 0.0 OrElse u + v > 1.0 Then
                Return False
            End If

            ' t = f * (edge2 · q)
            t = f * Point3D.Dot(edge2, q)

            ' 只接受正向射线方向上的交点 (t > 0)
            Return t > EPSILON
        End Function

#End Region

#Region "核心算法 —— 列扫描体素化"

        ''' <summary>
        ''' 列扫描体素化的内部实现。
        '''
        ''' 对每个 (x, y) 列沿 Z 轴发射射线，收集与所有三角形的交点，
        ''' 排序后在交点对之间将体素标记为 True（固体内部）。
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

            ' 射线方向（沿 +Z 轴，长度覆盖整个 depth）
            Dim rayDir = New Point3D(0, 0, 1)

            ' 用于暂存每条射线的交点列表
            Dim intersections As New List(Of Double)(capacity:=128)

            ' 遍历每一列 (x, y)
            For x As Integer = 0 To width - 1
                Dim worldX As Double = minX + (x + 0.5) * voxelSize

                For y As Integer = 0 To height - 1
                    Dim worldY As Double = minY + (y + 0.5) * voxelSize

                    ' 射线起点：列中心，Z 轴从包围盒下方略微偏移开始
                    Dim rayOrigin = New Point3D(worldX, worldY, minZ - voxelSize * 0.5)
                    intersections.Clear()

                    ' 与所有三角形求交，收集交点 Z 坐标
                    For Each tri In triangles
                        Dim t As Double = 0
                        If RayTriangleIntersect(rayOrigin, rayDir, tri.v1, tri.v2, tri.v3, t) Then
                            Dim zHit As Double = rayOrigin.Z + t
                            intersections.Add(zHit)
                        End If
                    Next

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
            Next

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
