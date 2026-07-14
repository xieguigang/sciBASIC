#Region "Microsoft.VisualBasic::00000000000000000000000000000000, gr\Landscape\Voxelization\SDFVolume.vb"

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


    '     Class SDFVolume
    ' 
    '         Properties: Depth, Height, MinX, MinY, MinZ
    '                     Occupancy, SDF, VoxelSize, Width
    ' 
    '         Function: GetIndex, SampleSDF, ToString, ToVoxelModel, VoxelToWorld
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports std = System.Math

Namespace Voxelization

    ''' <summary>
    ''' 连续符号距离场 (Signed Distance Field, SDF) 体积数据模型，用于 CFD 高精度边界处理。
    '''
    ''' 与 <see cref="VoxelModel"/> 共享同一套三维网格定义与一维索引公式
    ''' <c>index = (x * Height + y) * Depth + z</c>，以保证 SDF 场与二进制体素模型
    ''' 在空间上完全对齐。
    '''
    ''' ## 数据含义
    '''
    ''' - <see cref="SDF"/>：每个体素中心到网格最近三角面的**带符号**欧几里得距离。
    '''   负值表示体素中心位于模型内部（固体），正值表示位于模型外部（流体空间），
    '''   零值位于表面上。相比二进制体素，连续距离场携带了亚体素级别的表面位置信息，
    '''   CFD 引擎可据此在边界上做高精度插值 (如 immersed boundary / cut-cell 方法)。
    ''' - <see cref="Occupancy"/>：亚体素采样得到的固体分数占用率 [0,1]，用于抗锯齿，
    '''   表征每个体素被固体覆盖的比例 (0 = 完全空, 1 = 完全实心)。
    ''' </summary>
    ''' <remarks>
    ''' SDF 的符号由最近三角面法向判定 (Baerentzen &amp; Aanaes, 2002)，因此要求输入网格
    ''' 尽量水密 (watertight) 且法向定向一致，否则内外符号可能在薄壁/非流形处出现错误。
    ''' </remarks>
    Public Class SDFVolume

        ''' <summary>
        ''' 体素中心的带符号距离数组 (世界坐标单位)。负值 = 内部，正值 = 外部。
        ''' 按 <c>index = (x * Height + y) * Depth + z</c> 展平存储。
        ''' </summary>
        Public Property SDF As Double()

        ''' <summary>
        ''' 亚体素采样得到的固体分数占用率数组，取值范围 [0,1]。
        ''' 用于抗锯齿，平滑二进制体素化产生的阶梯伪影。
        ''' </summary>
        Public Property Occupancy As Double()

        ''' <summary>
        ''' X 轴方向体素数量
        ''' </summary>
        Public Property Width As Integer

        ''' <summary>
        ''' Y 轴方向体素数量
        ''' </summary>
        Public Property Height As Integer

        ''' <summary>
        ''' Z 轴方向体素数量
        ''' </summary>
        Public Property Depth As Integer

        ''' <summary>
        ''' 模型包围盒在 X 轴上的最小值（世界坐标）
        ''' </summary>
        Public Property MinX As Double

        ''' <summary>
        ''' 模型包围盒在 Y 轴上的最小值（世界坐标）
        ''' </summary>
        Public Property MinY As Double

        ''' <summary>
        ''' 模型包围盒在 Z 轴上的最小值（世界坐标）
        ''' </summary>
        Public Property MinZ As Double

        ''' <summary>
        ''' 每个体素的物理尺寸（等轴测，即 X/Y/Z 方向尺寸相同）
        ''' </summary>
        Public Property VoxelSize As Double

        ''' <summary>
        ''' 将三维体素坐标转换为一维数组索引 (与 <see cref="VoxelModel.GetIndex"/> 保持一致)
        ''' </summary>
        ''' <param name="x">X 轴索引 [0, Width-1]</param>
        ''' <param name="y">Y 轴索引 [0, Height-1]</param>
        ''' <param name="z">Z 轴索引 [0, Depth-1]</param>
        ''' <returns>一维数组中的索引位置</returns>
        Public Function GetIndex(x As Integer, y As Integer, z As Integer) As Integer
            Return (x * Height + y) * Depth + z
        End Function

        ''' <summary>
        ''' 将体素索引坐标转换为世界坐标系中的体素中心位置
        ''' </summary>
        ''' <param name="x">X 轴体素索引</param>
        ''' <param name="y">Y 轴体素索引</param>
        ''' <param name="z">Z 轴体素索引</param>
        ''' <returns>体素中心在世界坐标系中的 (x, y, z) 坐标</returns>
        Public Function VoxelToWorld(x As Integer, y As Integer, z As Integer) As (x As Double, y As Double, z As Double)
            Return (MinX + (x + 0.5) * VoxelSize,
                    MinY + (y + 0.5) * VoxelSize,
                    MinZ + (z + 0.5) * VoxelSize)
        End Function

        ''' <summary>
        ''' 在任意世界坐标点 <paramref name="p"/> 处对 SDF 场做三线性插值查询。
        '''
        ''' 用于以高于体素分辨率的精度采样距离场，例如 CFD 求解器在切割单元
        ''' (cut-cell) 边界上定位精确的表面位置。
        ''' </summary>
        ''' <param name="p">世界坐标查询点</param>
        ''' <returns>该点处插值后的带符号距离值</returns>
        Public Function SampleSDF(p As Point3D) As Double
            ' 将世界坐标转换为以体素中心为格点的连续网格坐标
            Dim gx As Double = (p.X - MinX) / VoxelSize - 0.5
            Dim gy As Double = (p.Y - MinY) / VoxelSize - 0.5
            Dim gz As Double = (p.Z - MinZ) / VoxelSize - 0.5

            ' 夹取到有效范围内 [0, N-1]
            gx = Clamp(gx, 0, Width - 1)
            gy = Clamp(gy, 0, Height - 1)
            gz = Clamp(gz, 0, Depth - 1)

            Dim x0 As Integer = CInt(std.Floor(gx))
            Dim y0 As Integer = CInt(std.Floor(gy))
            Dim z0 As Integer = CInt(std.Floor(gz))
            Dim x1 As Integer = std.Min(x0 + 1, Width - 1)
            Dim y1 As Integer = std.Min(y0 + 1, Height - 1)
            Dim z1 As Integer = std.Min(z0 + 1, Depth - 1)

            Dim fx As Double = gx - x0
            Dim fy As Double = gy - y0
            Dim fz As Double = gz - z0

            ' 8 个角点的距离值
            Dim c000 As Double = SDF(GetIndex(x0, y0, z0))
            Dim c100 As Double = SDF(GetIndex(x1, y0, z0))
            Dim c010 As Double = SDF(GetIndex(x0, y1, z0))
            Dim c110 As Double = SDF(GetIndex(x1, y1, z0))
            Dim c001 As Double = SDF(GetIndex(x0, y0, z1))
            Dim c101 As Double = SDF(GetIndex(x1, y0, z1))
            Dim c011 As Double = SDF(GetIndex(x0, y1, z1))
            Dim c111 As Double = SDF(GetIndex(x1, y1, z1))

            ' 沿 X 插值
            Dim c00 As Double = c000 * (1 - fx) + c100 * fx
            Dim c10 As Double = c010 * (1 - fx) + c110 * fx
            Dim c01 As Double = c001 * (1 - fx) + c101 * fx
            Dim c11 As Double = c011 * (1 - fx) + c111 * fx

            ' 沿 Y 插值
            Dim c0 As Double = c00 * (1 - fy) + c10 * fy
            Dim c1 As Double = c01 * (1 - fy) + c11 * fy

            ' 沿 Z 插值
            Return c0 * (1 - fz) + c1 * fz
        End Function

        ''' <summary>
        ''' 依据分数占用率阈值将连续 SDF 场离散化为二进制 <see cref="VoxelModel"/>。
        ''' </summary>
        ''' <param name="threshold">
        ''' 占用率阈值 [0,1]，占用率 ≥ 阈值的体素标记为固体 (True)。默认 0.5，
        ''' 即体素中心大致位于表面时按 "半数子采样在内" 判定。
        ''' </param>
        ''' <returns>与本 SDF 场空间对齐的二进制体素模型</returns>
        Public Function ToVoxelModel(Optional threshold As Double = 0.5) As VoxelModel
            Dim shape As Boolean() = New Boolean(Occupancy.Length - 1) {}

            For i As Integer = 0 To Occupancy.Length - 1
                shape(i) = Occupancy(i) >= threshold
            Next

            Return New VoxelModel With {
                .Shape = shape,
                .Width = Width,
                .Height = Height,
                .Depth = Depth,
                .MinX = MinX,
                .MinY = MinY,
                .MinZ = MinZ,
                .VoxelSize = VoxelSize
            }
        End Function

        Private Shared Function Clamp(v As Double, lo As Double, hi As Double) As Double
            If v < lo Then Return lo
            If v > hi Then Return hi
            Return v
        End Function

        Public Overrides Function ToString() As String
            Return $"SDFVolume[{Width}x{Height}x{Depth}] voxelSize={VoxelSize.ToString("F4")}"
        End Function

    End Class
End Namespace
