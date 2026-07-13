#Region "Microsoft.VisualBasic::6fde865093c452a3523ee40a4c14925e, gr\Landscape\Max3DS\Max3DSTypes.vb"

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

    '   Total Lines: 157
    '    Code Lines: 58 (36.94%)
    ' Comment Lines: 69 (43.95%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 30 (19.11%)
    '     File Size: 5.29 KB


    '     Class Chunk3DS
    ' 
    '         Properties: childChunks, chunkId, chunkLength, dataOffset, endOffset
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Mesh3DS
    ' 
    '         Properties: faceMaterials, faces, localMatrix, materialName, name
    '                     textureVertices, vertices
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Material3DS
    ' 
    '         Properties: diffuseColor, name, textureFile
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToHtmlColor, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports std = System.Math

Namespace Max3DS

    ''' <summary>
    ''' 3DS 文件块（Chunk）结构。
    ''' 3DS 格式使用类似 RIFF/IFF 的分块存储：
    ''' - 2 字节 Chunk ID (UInt16)
    ''' - 4 字节 Chunk 长度 (UInt32)，包含自身这 6 字节头部
    ''' - N 字节数据
    ''' </summary>
    Friend Class Chunk3DS

        ''' <summary>
        ''' Chunk 类型标识符
        ''' </summary>
        Public Property chunkId As UShort

        ''' <summary>
        ''' Chunk 的总长度（包含头部 6 字节）
        ''' </summary>
        Public Property chunkLength As UInteger

        ''' <summary>
        ''' Chunk 数据部分在文件中的起始偏移
        ''' </summary>
        Public Property dataOffset As Long

        ''' <summary>
        ''' Chunk 的结束偏移（dataOffset + chunkLength - 6）
        ''' </summary>
        Public ReadOnly Property endOffset As Long
            Get
                Return dataOffset + CLng(chunkLength) - 6
            End Get
        End Property

        ''' <summary>
        ''' 该 Chunk 内的子 Chunk 列表
        ''' </summary>
        Public Property childChunks As List(Of Chunk3DS)

        Public Sub New()
            childChunks = New List(Of Chunk3DS)()
        End Sub

        Public Overrides Function ToString() As String
            Return $"Chunk 0x{chunkId:X4}, Length: {chunkLength}, Children: {childChunks.Count}"
        End Function

    End Class

    ''' <summary>
    ''' 3DS 网格数据内部表示。
    ''' 包含从单个 0x4100 (OBJ_MESH) Chunk 中解析出的所有几何信息。
    ''' </summary>
    Friend Class Mesh3DS

        ''' <summary>
        ''' 网格名称
        ''' </summary>
        Public Property name As String

        ''' <summary>
        ''' 顶点数组（Point3D 格式）
        ''' </summary>
        Public Property vertices As List(Of Point3D)

        ''' <summary>
        ''' 三角面索引数组。
        ''' 每个 Face 包含 3 个 UInt16 顶点索引（按 A, B, C 顺序），
        ''' 以及 1 个 UInt16 的 faceInfo（可见性标志位）。
        ''' </summary>
        ''' <remarks>
        ''' .faces(i).Item1/Item2/Item3 = 顶点索引
        ''' .faces(i).Item4 = faceInfo (bit0=AC可见, bit1=BC可见, bit2=AB可见)
        ''' </remarks>
        Public Property faces As List(Of (A As UShort, B As UShort, C As UShort, FaceInfo As UShort))

        ''' <summary>
        ''' 每个面对应的材质名称。
        ''' Key: 材质名称, Value: 使用该材质的面的索引列表
        ''' </summary>
        Public Property faceMaterials As Dictionary(Of String, List(Of UShort))

        ''' <summary>
        ''' 纹理坐标（UV）数组
        ''' </summary>
        Public Property textureVertices As List(Of (U As Single, V As Single))

        ''' <summary>
        ''' 局部变换矩阵（4×3 矩阵，共 12 个 Single）
        ''' </summary>
        Public Property localMatrix As Single()

        ''' <summary>
        ''' 该网格默认使用的材质名称
        ''' </summary>
        Public Property materialName As String

        Public Sub New()
            vertices = New List(Of Point3D)()
            faces = New List(Of (UShort, UShort, UShort, UShort))()
            faceMaterials = New Dictionary(Of String, List(Of UShort))()
            textureVertices = New List(Of (Single, Single))()
            localMatrix = Nothing
        End Sub

        Public Overrides Function ToString() As String
            Return $"Mesh '{name}': {vertices.Count} vertices, {faces.Count} faces"
        End Function

    End Class

    ''' <summary>
    ''' 3DS 材质定义。
    ''' </summary>
    Friend Class Material3DS

        ''' <summary>
        ''' 材质名称
        ''' </summary>
        Public Property name As String

        ''' <summary>
        ''' 漫反射颜色（RGB 三个分量，范围 0.0 ~ 1.0）
        ''' </summary>
        Public Property diffuseColor As (R As Single, G As Single, B As Single)

        ''' <summary>
        ''' 纹理贴图文件名
        ''' </summary>
        Public Property textureFile As String

        Public Sub New()
            diffuseColor = (0.75F, 0.75F, 0.75F)
        End Sub

        ''' <summary>
        ''' 将漫反射颜色转换为 #RRGGBB 格式的十六进制颜色字符串
        ''' </summary>
        Public Function ToHtmlColor() As String
            Dim r As Integer = CInt(std.Min(diffuseColor.R * 255.0F, 255.0F))
            Dim g As Integer = CInt(std.Min(diffuseColor.G * 255.0F, 255.0F))
            Dim b As Integer = CInt(std.Min(diffuseColor.B * 255.0F, 255.0F))
            Return $"#{r:X2}{g:X2}{b:X2}"
        End Function

        Public Overrides Function ToString() As String
            Dim texInfo As String = If(String.IsNullOrEmpty(textureFile), "", $", Texture: {textureFile}")
            Return $"Material '{name}': Diffuse=({diffuseColor.R:F2},{diffuseColor.G:F2},{diffuseColor.B:F2}){texInfo}"
        End Function

    End Class

End Namespace
