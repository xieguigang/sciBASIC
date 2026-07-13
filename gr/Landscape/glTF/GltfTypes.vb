#Region "Microsoft.VisualBasic::glTF_Types, gr\Landscape\glTF\GltfTypes.vb"

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

    '   Total Lines: 0
    '    Code Lines: 0
    ' Comment Lines: 0
    ' 
    '   Blank Lines: 0
    '     File Size: 0 B


    '     Class GltfRoot
    '         Properties: Accessors, Asset, BufferViews, Buffers, DefaultScene, Materials, Meshes, Nodes, Samplers, Scenes, Textures
    '     Class AssetInfo
    '         Properties: Generator, Version
    '     Class Scene
    '         Properties: Name, Nodes
    '     Class Node
    '         Properties: Children, Matrix, MeshIndex, Name, Rotation, Scale, Translation
    '     Class GltfMesh
    '         Properties: Name, Primitives
    '     Class Primitive
    '         Properties: Attributes, IndicesAccessor, MaterialIndex, Mode
    '     Class Accessor
    '         Properties: BufferViewIndex, ByteOffset, ComponentType, Count, ElementType, Max, Min
    '     Class BufferView
    '         Properties: BufferIndex, ByteLength, ByteOffset, ByteStride, Target
    '     Class GltfBuffer
    '         Properties: ByteLength, Uri
    '     Class GltfMaterial
    '         Properties: Name, PbrMetallicRoughness
    '     Class PbrMetallicRoughness
    '         Properties: BaseColorFactor

    ' /********************************************************************************/

#End Region

Imports System.Text.Json.Serialization

Namespace Gltf

    ''' <summary>
    ''' glTF JSON 根对象，对应 glTF 2.0 规范的顶层结构
    ''' </summary>
    Public Class GltfRoot

        <JsonPropertyName("asset")>
        Public Property Asset As AssetInfo

        <JsonPropertyName("scene")>
        Public Property DefaultScene As Integer?

        <JsonPropertyName("scenes")>
        Public Property Scenes As Scene()

        <JsonPropertyName("nodes")>
        Public Property Nodes As Node()

        <JsonPropertyName("meshes")>
        Public Property Meshes As GltfMesh()

        <JsonPropertyName("accessors")>
        Public Property Accessors As Accessor()

        <JsonPropertyName("bufferViews")>
        Public Property BufferViews As BufferView()

        <JsonPropertyName("buffers")>
        Public Property Buffers As GltfBuffer()

        <JsonPropertyName("materials")>
        Public Property Materials As GltfMaterial()

    End Class

    ''' <summary>
    ''' 资产描述信息
    ''' </summary>
    Public Class AssetInfo

        <JsonPropertyName("version")>
        Public Property Version As String

        <JsonPropertyName("generator")>
        Public Property Generator As String

    End Class

    ''' <summary>
    ''' 场景定义：包含一组根节点索引
    ''' </summary>
    Public Class Scene

        <JsonPropertyName("name")>
        Public Property Name As String

        <JsonPropertyName("nodes")>
        Public Property Nodes As Integer()

    End Class

    ''' <summary>
    ''' 场景图中的节点，可包含网格引用、变换矩阵和子节点
    ''' </summary>
    Public Class Node

        <JsonPropertyName("name")>
        Public Property Name As String

        <JsonPropertyName("mesh")>
        Public Property MeshIndex As Integer?

        <JsonPropertyName("children")>
        Public Property Children As Integer()

        <JsonPropertyName("matrix")>
        Public Property Matrix As Single()

        <JsonPropertyName("translation")>
        Public Property Translation As Single()

        <JsonPropertyName("rotation")>
        Public Property Rotation As Single()

        <JsonPropertyName("scale")>
        Public Property Scale As Single()

    End Class

    ''' <summary>
    ''' 网格定义：包含多个图元 (Primitive)
    ''' </summary>
    Public Class GltfMesh

        <JsonPropertyName("name")>
        Public Property Name As String

        <JsonPropertyName("primitives")>
        Public Property Primitives As Primitive()

    End Class

    ''' <summary>
    ''' 图元：描述一个可渲染的几何体单元
    ''' </summary>
    Public Class Primitive

        ''' <summary>
        ''' 顶点属性字典，Key 为语义名 (如 "POSITION", "NORMAL", "TEXCOORD_0")，Value 为 Accessor 索引
        ''' </summary>
        <JsonPropertyName("attributes")>
        Public Property Attributes As Dictionary(Of String, Integer)

        ''' <summary>
        ''' 索引 Accessor 的索引（可选，若不存在则为非索引几何体）
        ''' </summary>
        <JsonPropertyName("indices")>
        Public Property IndicesAccessor As Integer?

        ''' <summary>
        ''' 材质索引
        ''' </summary>
        <JsonPropertyName("material")>
        Public Property MaterialIndex As Integer?

        ''' <summary>
        ''' 渲染模式：0=POINTS, 1=LINES, 2=LINE_LOOP, 3=LINE_STRIP, 4=TRIANGLES, 5=TRIANGLE_STRIP, 6=TRIANGLE_FAN
        ''' </summary>
        <JsonPropertyName("mode")>
        Public Property Mode As Integer?

    End Class

    ''' <summary>
    ''' 访问器：描述如何从 BufferView 中读取类型化数组数据
    ''' </summary>
    Public Class Accessor

        <JsonPropertyName("bufferView")>
        Public Property BufferViewIndex As Integer?

        <JsonPropertyName("byteOffset")>
        Public Property ByteOffset As Integer?

        ''' <summary>
        ''' 组件类型：5120=BYTE, 5121=UNSIGNED_BYTE, 5122=SHORT, 5123=UNSIGNED_SHORT, 5125=UNSIGNED_INT, 5126=FLOAT
        ''' </summary>
        <JsonPropertyName("componentType")>
        Public Property ComponentType As Integer

        <JsonPropertyName("count")>
        Public Property Count As Integer

        ''' <summary>
        ''' 数据类型："SCALAR", "VEC2", "VEC3", "VEC4", "MAT2", "MAT3", "MAT4"
        ''' </summary>
        <JsonPropertyName("type")>
        Public Property ElementType As String

        <JsonPropertyName("max")>
        Public Property Max As Single()

        <JsonPropertyName("min")>
        Public Property Min As Single()

        ''' <summary>
        ''' 获取每个元素的组件数量
        ''' </summary>
        Public ReadOnly Property NumComponents As Integer
            Get
                Select Case ElementType
                    Case "SCALAR" : Return 1
                    Case "VEC2" : Return 2
                    Case "VEC3" : Return 3
                    Case "VEC4" : Return 4
                    Case "MAT2" : Return 4
                    Case "MAT3" : Return 9
                    Case "MAT4" : Return 16
                    Case Else : Return 0
                End Select
            End Get
        End Property

        ''' <summary>
        ''' 获取每个组件的字节大小
        ''' </summary>
        Public ReadOnly Property ComponentSize As Integer
            Get
                Select Case ComponentType
                    Case 5120, 5121 : Return 1  ' BYTE / UNSIGNED_BYTE
                    Case 5122, 5123 : Return 2  ' SHORT / UNSIGNED_SHORT
                    Case 5125 : Return 4          ' UNSIGNED_INT
                    Case 5126 : Return 4          ' FLOAT
                    Case Else : Return 0
                End Select
            End Get
        End Property

    End Class

    ''' <summary>
    ''' 缓冲区视图：描述 Buffer 中的一个子区域
    ''' </summary>
    Public Class BufferView

        <JsonPropertyName("buffer")>
        Public Property BufferIndex As Integer

        <JsonPropertyName("byteOffset")>
        Public Property ByteOffset As Integer?

        <JsonPropertyName("byteLength")>
        Public Property ByteLength As Integer

        <JsonPropertyName("byteStride")>
        Public Property ByteStride As Integer?

        ''' <summary>
        ''' 目标用途：34962=ARRAY_BUFFER, 34963=ELEMENT_ARRAY_BUFFER
        ''' </summary>
        <JsonPropertyName("target")>
        Public Property Target As Integer?

    End Class

    ''' <summary>
    ''' 缓冲区：存储实际的二进制数据
    ''' </summary>
    Public Class GltfBuffer

        <JsonPropertyName("uri")>
        Public Property Uri As String

        <JsonPropertyName("byteLength")>
        Public Property ByteLength As Integer

    End Class

    ''' <summary>
    ''' 材质定义
    ''' </summary>
    Public Class GltfMaterial

        <JsonPropertyName("name")>
        Public Property Name As String

        <JsonPropertyName("pbrMetallicRoughness")>
        Public Property PbrMetallicRoughness As PbrMetallicRoughness

    End Class

    ''' <summary>
    ''' PBR 金属粗糙度材质属性
    ''' </summary>
    Public Class PbrMetallicRoughness

        ''' <summary>
        ''' 基础颜色因子 RGBA: [R, G, B, A]，每个分量范围 [0, 1]
        ''' </summary>
        <JsonPropertyName("baseColorFactor")>
        Public Property BaseColorFactor As Single()

    End Class

    ''' <summary>
    ''' 访问器组件类型常量
    ''' </summary>
    Friend Enum GltfComponentType As Integer
        [BYTE] = 5120
        UNSIGNED_BYTE = 5121
        [SHORT] = 5122
        UNSIGNED_SHORT = 5123
        UNSIGNED_INT = 5125
        FLOAT = 5126
    End Enum

    ''' <summary>
    ''' 图元渲染模式常量
    ''' </summary>
    Friend Enum GltfPrimitiveMode As Integer
        POINTS = 0
        LINES = 1
        LINE_LOOP = 2
        LINE_STRIP = 3
        TRIANGLES = 4
        TRIANGLE_STRIP = 5
        TRIANGLE_FAN = 6
    End Enum

End Namespace
