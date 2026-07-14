#Region "Microsoft.VisualBasic::00000000000000000000000000000000, gr\Landscape\Voxelization\SDFVoxelizer.vb"

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


    '     Module SDFVoxelizer
    ' 
    '         Function: ComputeBoundingBox, ComputeSDF, ComputeVoxelGridDimensions, ExtractTriangles, SignedDistance
    '                   (+2 Overloads) Voxelize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Landscape.Data
Imports std = System.Math

Namespace Voxelization

    ''' <summary>
    ''' 基于符号距离场 (Signed Distance Field, SDF) 的高精度三角网格体素化器。
    '''
    ''' 相比 <see cref="Voxelizer"/> 的列扫描射线投射算法 (仅做布尔内外判定、边界呈
    ''' 明显阶梯伪影)，本模块为 CFD 高精度仿真提供更精细的体素化结果：
    '''
    ''' ## 算法概述
    '''
    ''' 1. **精确 SDF**：对每个采样点，通过 <see cref="BVH"/> 查询到网格的最近三角面
    '''    及最近点，得到无符号欧几里得距离 ``d``；再用最近三角面法向判定内外符号
    '''    (Baerentzen &amp; Aanaes, 2002)：若 ``(p - closest) · n &lt; 0`` 则点在内部，
    '''    SDF = -d，否则 SDF = +d。
    ''' 2. **BVH 加速**：以三角形包围盒层次结构将 "每采样点找最近三角形" 从
    '''    O(采样数 × 三角形数) 降至近似 O(采样数 × log 三角形数)。
    ''' 3. **亚体素抗锯齿**：每个体素内按 N×N×N 子网格采样 SDF 符号，统计固体子点
    '''    占比作为分数占用率 (occupancy)，显著平滑表面、消除阶梯伪影。
    '''
    ''' ## 输出
    '''
    ''' - <see cref="Voxelize"/>：抗锯齿后的二进制 <see cref="VoxelModel"/> (可直接替换现有体素模型)。
    ''' - <see cref="ComputeSDF"/>：连续 <see cref="SDFVolume"/> (体素中心带符号距离 + 抗锯齿占用率)，
    '''   供 CFD 引擎在边界上做高精度处理。
    '''
    ''' ## 前提条件
    '''
    ''' 符号判定依赖最近三角面法向，因此要求输入网格尽量水密 (watertight) 且法向定向一致；
    ''' 对薄壁 / 非流形 / 破洞模型，内外符号可能局部出错，建议预先修复模型。
    ''' </summary>
    Public Module SDFVoxelizer

#Region "公共 API —— SDF 体素化入口"

        ''' <summary>
        ''' 以指定分辨率将 SceneModel 体素化为抗锯齿的二进制体素模型 (SDF 算法)。
        ''' </summary>
        ''' <param name="model">已解析的 3D 场景模型 (任意格式转换后的 SceneModel)</param>
        ''' <param name="resolution">最长边上的体素分辨率，默认 128。实际各轴体素数量按比例缩放</param>
        ''' <param name="subSamples">每个体素每轴的子采样数 (抗锯齿)，默认 2 (即 2³=8 个子采样点)</param>
        ''' <param name="threshold">占用率阈值 [0,1]，占用率 ≥ 阈值的体素判为固体，默认 0.5</param>
        ''' <returns>抗锯齿二进制体素模型，若输入无效则返回 Nothing</returns>
        <Extension>
        Public Function Voxelize(model As SceneModel,
                                 Optional resolution As Integer = 128,
                                 Optional subSamples As Integer = 2,
                                 Optional threshold As Double = 0.5) As VoxelModel

            Dim volume As SDFVolume = model.ComputeSDF(resolution, subSamples)
            If volume Is Nothing Then Return Nothing
            Return volume.ToVoxelModel(threshold)
        End Function

        ''' <summary>
        ''' 以指定体素物理尺寸将 SceneModel 体素化为抗锯齿的二进制体素模型 (SDF 算法)。
        ''' </summary>
        ''' <param name="model">已解析的 3D 场景模型</param>
        ''' <param name="voxelSize">每个体素的物理尺寸 (世界坐标单位)</param>
        ''' <param name="subSamples">每个体素每轴的子采样数 (抗锯齿)，默认 2</param>
        ''' <param name="threshold">占用率阈值 [0,1]，默认 0.5</param>
        ''' <returns>抗锯齿二进制体素模型，若输入无效则返回 Nothing</returns>
        <Extension>
        Public Function Voxelize(model As SceneModel,
                                 voxelSize As Double,
                                 Optional subSamples As Integer = 2,
                                 Optional threshold As Double = 0.5) As VoxelModel

            Dim volume As SDFVolume = model.ComputeSDF(voxelSize, subSamples)
            If volume Is Nothing Then Return Nothing
            Return volume.ToVoxelModel(threshold)
        End Function

        ''' <summary>
        ''' 计算 SceneModel 的连续符号距离场 (SDFVolume)，以指定分辨率。
        ''' </summary>
        ''' <param name="model">已解析的 3D 场景模型</param>
        ''' <param name="resolution">最长边上的体素分辨率，默认 128</param>
        ''' <param name="subSamples">每个体素每轴的子采样数 (抗锯齿)，默认 2</param>
        ''' <returns>连续 SDF 场，若输入无效则返回 Nothing</returns>
        <Extension>
        Public Function ComputeSDF(model As SceneModel,
                                   Optional resolution As Integer = 128,
                                   Optional subSamples As Integer = 2) As SDFVolume

            If model Is Nothing Then Return Nothing
            If model.Surfaces Is Nothing OrElse model.Surfaces.Length = 0 Then Return Nothing

            Dim triangles = ExtractTriangles(model)
            If triangles Is Nothing OrElse triangles.Length = 0 Then Return Nothing

            Dim bbox = ComputeBoundingBox(triangles)
            ApplyMargin(bbox)

            Dim dims = ComputeVoxelGridDimensions(bbox.maxX - bbox.minX, bbox.maxY - bbox.minY, bbox.maxZ - bbox.minZ, resolution)

            Return BuildSDFVolume(triangles, dims.width, dims.height, dims.depth,
                                  bbox.minX, bbox.minY, bbox.minZ, dims.voxelSize, subSamples)
        End Function

        ''' <summary>
        ''' 计算 SceneModel 的连续符号距离场 (SDFVolume)，以指定体素物理尺寸。
        ''' </summary>
        ''' <param name="model">已解析的 3D 场景模型</param>
        ''' <param name="voxelSize">每个体素的物理尺寸 (世界坐标单位)</param>
        ''' <param name="subSamples">每个体素每轴的子采样数 (抗锯齿)，默认 2</param>
        ''' <returns>连续 SDF 场，若输入无效则返回 Nothing</returns>
        <Extension>
        Public Function ComputeSDF(model As SceneModel,
                                   voxelSize As Double,
                                   Optional subSamples As Integer = 2) As SDFVolume

            If model Is Nothing Then Return Nothing
            If model.Surfaces Is Nothing OrElse model.Surfaces.Length = 0 Then Return Nothing
            If voxelSize <= 0 Then
                Throw New ArgumentException("voxelSize 必须大于 0", NameOf(voxelSize))
            End If

            Dim triangles = ExtractTriangles(model)
            If triangles Is Nothing OrElse triangles.Length = 0 Then Return Nothing

            Dim bbox = ComputeBoundingBox(triangles)
            ApplyMargin(bbox)

            Dim width As Integer = std.Max(1, CInt(std.Ceiling((bbox.maxX - bbox.minX) / voxelSize)))
            Dim height As Integer = std.Max(1, CInt(std.Ceiling((bbox.maxY - bbox.minY) / voxelSize)))
            Dim depth As Integer = std.Max(1, CInt(std.Ceiling((bbox.maxZ - bbox.minZ) / voxelSize)))

            Return BuildSDFVolume(triangles, width, height, depth,
                                  bbox.minX, bbox.minY, bbox.minZ, voxelSize, subSamples)
        End Function

#End Region

#Region "核心算法 —— SDF 体素网格构建"

        ''' <summary>
        ''' 构建 SDF 体积：对每个体素做亚体素子采样，通过 BVH 查询最近三角面并用
        ''' 法向判定内外符号，得到体素中心的连续 SDF 与抗锯齿分数占用率。
        ''' </summary>
        Private Function BuildSDFVolume(triangles As SDFTriangle(),
                                        width As Integer,
                                        height As Integer,
                                        depth As Integer,
                                        minX As Double,
                                        minY As Double,
                                        minZ As Double,
                                        voxelSize As Double,
                                        subSamples As Integer) As SDFVolume

            If subSamples < 1 Then subSamples = 1

            ' 构建 BVH 加速结构
            Dim accel As New BVH()
            Call accel.Build(triangles)

            Dim total As Integer = width * height * depth
            Dim sdf As Double() = New Double(total - 1) {}
            Dim occ As Double() = New Double(total - 1) {}

            Dim subTotal As Integer = subSamples * subSamples * subSamples
            Dim subStep As Double = voxelSize / subSamples

            ' 并行遍历 X 列 (各体素查询相互独立，写入互不重叠的索引)
            Threading.Tasks.Parallel.For(0, width,
                Sub(x As Integer)
                    For y As Integer = 0 To height - 1
                        For z As Integer = 0 To depth - 1
                            Dim idx As Integer = (x * height + y) * depth + z

                            ' 体素原点 (角点) 世界坐标
                            Dim ox As Double = minX + x * voxelSize
                            Dim oy As Double = minY + y * voxelSize
                            Dim oz As Double = minZ + z * voxelSize

                            ' 体素中心的精确带符号距离 (供连续 SDF 场使用)
                            Dim center As New Point3D(ox + voxelSize * 0.5, oy + voxelSize * 0.5, oz + voxelSize * 0.5)
                            sdf(idx) = SignedDistance(accel, center)

                            ' 亚体素子采样统计固体占比 (抗锯齿)
                            Dim insideCount As Integer = 0

                            For sx As Integer = 0 To subSamples - 1
                                Dim px As Double = ox + (sx + 0.5) * subStep
                                For sy As Integer = 0 To subSamples - 1
                                    Dim py As Double = oy + (sy + 0.5) * subStep
                                    For sz As Integer = 0 To subSamples - 1
                                        Dim pz As Double = oz + (sz + 0.5) * subStep
                                        Dim d As Double = SignedDistance(accel, New Point3D(px, py, pz))
                                        If d < 0.0 Then insideCount += 1
                                    Next
                                Next
                            Next

                            occ(idx) = insideCount / subTotal
                        Next
                    Next
                End Sub)

            Return New SDFVolume With {
                .SDF = sdf,
                .Occupancy = occ,
                .Width = width,
                .Height = height,
                .Depth = depth,
                .MinX = minX,
                .MinY = minY,
                .MinZ = minZ,
                .VoxelSize = voxelSize
            }
        End Function

        ''' <summary>
        ''' 计算空间点 <paramref name="p"/> 到网格的带符号距离。
        '''
        ''' 无符号距离由 BVH 最近三角面查询得到；符号由最近三角面法向判定：
        ''' 令 closest 为最近点、n 为该三角面单位法向，若 ``(p - closest) · n &lt; 0``
        ''' 则点位于表面内侧 (返回负值)，否则位于外侧 (返回正值)。
        ''' </summary>
        Private Function SignedDistance(accel As BVH, p As Point3D) As Double
            Dim hit = accel.QueryNearest(p)

            If hit.triIndex < 0 Then
                ' 空网格：视为全部在外部
                Return Double.MaxValue
            End If

            Dim tri = accel.GetTriangle(hit.triIndex)
            Dim n = tri.Normal()
            Dim dir = p.Subtract(hit.closest)
            Dim side As Double = Point3D.Dot(dir, n)

            If side < 0.0 Then
                Return -hit.distance
            Else
                Return hit.distance
            End If
        End Function

#End Region

#Region "内部辅助方法 —— 三角形提取与包围盒"

        ''' <summary>
        ''' 从 SceneModel 中提取所有三角面 (每个 Surface 为一个三角形)。
        ''' </summary>
        Private Function ExtractTriangles(model As SceneModel) As SDFTriangle()
            Dim list As New List(Of SDFTriangle)

            For Each surf In model.Surfaces
                If surf.vertices Is Nothing OrElse surf.vertices.Length < 3 Then Continue For

                Dim v1 = surf.vertices(0).PointData
                Dim v2 = surf.vertices(1).PointData
                Dim v3 = surf.vertices(2).PointData

                list.Add(New SDFTriangle(v1, v2, v3))
            Next

            Return list.ToArray
        End Function

        ''' <summary>
        ''' 计算三角面集合的轴对齐包围盒。
        ''' </summary>
        Private Function ComputeBoundingBox(triangles As SDFTriangle()) As BoundingBox
            Dim box As New BoundingBox With {
                .minX = Double.MaxValue, .minY = Double.MaxValue, .minZ = Double.MaxValue,
                .maxX = Double.MinValue, .maxY = Double.MinValue, .maxZ = Double.MinValue
            }

            For Each tri In triangles
                If tri.MinCorner.X < box.minX Then box.minX = tri.MinCorner.X
                If tri.MinCorner.Y < box.minY Then box.minY = tri.MinCorner.Y
                If tri.MinCorner.Z < box.minZ Then box.minZ = tri.MinCorner.Z
                If tri.MaxCorner.X > box.maxX Then box.maxX = tri.MaxCorner.X
                If tri.MaxCorner.Y > box.maxY Then box.maxY = tri.MaxCorner.Y
                If tri.MaxCorner.Z > box.maxZ Then box.maxZ = tri.MaxCorner.Z
            Next

            Return box
        End Function

        ''' <summary>
        ''' 为包围盒添加 0.1% 微小边距 (最小 0.0001)，避免表面体素因浮点误差被判在外。
        ''' 与 <see cref="Voxelizer"/> 保持一致。
        ''' </summary>
        Private Sub ApplyMargin(ByRef box As BoundingBox)
            Dim margin As Double = (box.maxX - box.minX) * 0.001
            If margin < 0.0001 Then margin = 0.0001
            box.minX -= margin : box.minY -= margin : box.minZ -= margin
            box.maxX += margin : box.maxY += margin : box.maxZ += margin
        End Sub

        ''' <summary>
        ''' 根据模型尺寸和分辨率计算各轴体素数量与体素尺寸 (等轴测)。
        ''' 与 <see cref="Voxelizer"/> 保持一致。
        ''' </summary>
        Private Function ComputeVoxelGridDimensions(sizeX As Double, sizeY As Double, sizeZ As Double, resolution As Integer) As (width As Integer, height As Integer, depth As Integer, voxelSize As Double)
            Dim maxSize As Double = std.Max(std.Max(sizeX, sizeY), sizeZ)
            Dim voxelSize As Double = maxSize / resolution

            Dim width As Integer = std.Max(1, CInt(std.Ceiling(sizeX / voxelSize)))
            Dim height As Integer = std.Max(1, CInt(std.Ceiling(sizeY / voxelSize)))
            Dim depth As Integer = std.Max(1, CInt(std.Ceiling(sizeZ / voxelSize)))

            Return (width, height, depth, voxelSize)
        End Function

        ''' <summary>
        ''' 内部包围盒结构 (可变，供 <see cref="ApplyMargin"/> ByRef 修改)。
        ''' </summary>
        Private Structure BoundingBox
            Public minX As Double, minY As Double, minZ As Double
            Public maxX As Double, maxY As Double, maxZ As Double
        End Structure

#End Region

    End Module
End Namespace
