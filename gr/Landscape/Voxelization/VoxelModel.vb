#Region "Microsoft.VisualBasic::28fcb96aa9da1afb02b9778a63658d3a, gr\Landscape\Voxelization\VoxelModel.vb"

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

    '   Total Lines: 116
    '    Code Lines: 40 (34.48%)
    ' Comment Lines: 61 (52.59%)
    '    - Xml Docs: 96.72%
    ' 
    '   Blank Lines: 15 (12.93%)
    '     File Size: 4.43 KB


    '     Class VoxelModel
    ' 
    '         Properties: Depth, Height, MinX, MinY, MinZ
    '                     Shape, SolidCount, VoxelSize, Width
    ' 
    '         Function: GetIndex, ToString, VoxelToWorld
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Voxelization

    ''' <summary>
    ''' 三维体素模型，用于 CFD 流体动力学仿真计算。
    '''
    ''' 使用一维 <see cref="Boolean"/> 数组表示三维空间中每个体素的状态：
    ''' ``False`` = 空/外部空间（无计算数据），
    ''' ``True``  = 固体区域（模拟环境空间的一部分，在 CFD 中作为固体边界处理）。
    '''
    ''' 一维索引公式: <c>index = (x * Height + y) * Depth + z</c>
    ''' </summary>
    ''' <remarks>
    ''' 体素化后的模型为实心体：所有位于三角网格内部的体素均标记为 True。
    ''' CFD 计算引擎会在计算时基于模型的外表面（由 True/False 边界定义）做流体动力学计算。
    ''' </remarks>
    Public Class VoxelModel

        ''' <summary>
        ''' 体素数据的一维数组。按 (x, y, z) 顺序展平存储：
        ''' ``index = (x * Height + y) * Depth + z``
        ''' </summary>
        Public Property Shape As Boolean()

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
        ''' 将三维体素坐标转换为一维数组索引
        ''' </summary>
        ''' <param name="x">X 轴索引 [0, Width-1]</param>
        ''' <param name="y">Y 轴索引 [0, Height-1]</param>
        ''' <param name="z">Z 轴索引 [0, Depth-1]</param>
        ''' <returns>一维数组中的索引位置</returns>
        Public Function GetIndex(x As Integer, y As Integer, z As Integer) As Integer
            Return (x * Height + y) * Depth + z
        End Function

        ''' <summary>
        ''' 通过三维索引进出体素数据的默认属性
        ''' </summary>
        ''' <param name="x">X 轴索引</param>
        ''' <param name="y">Y 轴索引</param>
        ''' <param name="z">Z 轴索引</param>
        Default Public Property Cell(x As Integer, y As Integer, z As Integer) As Boolean
            Get
                Return Shape(GetIndex(x, y, z))
            End Get
            Set(value As Boolean)
                Shape(GetIndex(x, y, z)) = value
            End Set
        End Property

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
        ''' 体素模型中的固体体素总数
        ''' </summary>
        Public ReadOnly Property SolidCount As Integer
            Get
                Dim count As Integer = 0
                For Each v In Shape
                    If v Then count += 1
                Next
                Return count
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"VoxelModel[{Width}x{Height}x{Depth}] solid={SolidCount}/{Shape.Length}"
        End Function

    End Class
End Namespace
