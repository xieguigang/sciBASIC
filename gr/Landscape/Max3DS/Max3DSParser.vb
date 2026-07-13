#Region "Microsoft.VisualBasic::3DS_Parser, gr\Landscape\Max3DS\Max3DSParser.vb"

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

'     Module Max3DSParser
' 
'         Function: Parse3DS, ReadAllChunks, ReadChunk, ReadChunkTree
' 
'                   ParseEdit3DS, ParseMainChunk, ParseMaterialBlock, ParseMaterialColor,
'                   ParseObjectBlock, ParseMeshBlock, ParseMaterialName, ParseTextureMap
' 
'                   ReadColorByte, ReadColorFloat, ReadFaceList, ReadLocalMatrix,
'                   ReadMaterialFaceMap, ReadNullTerminatedString, ReadTextureVertices,
'                   ReadVertexList, ApplyLocalMatrix, BuildSceneModel, TransformVertex
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Max3DS

    ' ============================================================================
    ' Chunk ID 常量定义
    ' ============================================================================
    Module ChunkId

        ' 主 Chunk
        Friend Const MAIN3DS As UShort = &H4D4D            ' 根 Chunk
        Friend Const EDIT3DS_VERSION As UShort = &H2        ' 3DS 编辑器版本号
        Friend Const EDIT3DS_MESH_VERSION As UShort = &H3D3E ' Mesh 版本号

        ' 编辑器数据
        Friend Const EDIT3DS As UShort = &H3D3D             ' 3D 编辑器 Chunk
        Friend Const EDIT_OBJECT As UShort = &H4000         ' 对象定义块
        Friend Const EDIT_MATERIAL As UShort = &HAFFF       ' 材质定义块

        ' 网格数据
        Friend Const OBJ_MESH As UShort = &H4100            ' 三角网格对象
        Friend Const MESH_VERTICES As UShort = &H4110       ' 顶点列表
        Friend Const MESH_FACES As UShort = &H4120          ' 三角面列表
        Friend Const MESH_FACE_MAT As UShort = &H4130       ' 面材质分配
        Friend Const MESH_TEX_VERT As UShort = &H4140       ' 纹理坐标
        Friend Const MESH_SMOOTH As UShort = &H4150         ' 平滑组
        Friend Const MESH_MATRIX As UShort = &H4160         ' 局部变换矩阵

        ' 材质属性
        Friend Const MAT_NAME01 As UShort = &HA000          ' 材质名称
        Friend Const MAT_AMBIENT As UShort = &HA010         ' 环境光颜色
        Friend Const MAT_DIFFUSE As UShort = &HA020         ' 漫反射颜色
        Friend Const MAT_SPECULAR As UShort = &HA030        ' 镜面反射颜色
        Friend Const MAT_SHININESS As UShort = &HA040       ' 光泽度
        Friend Const MAT_TEXMAP As UShort = &HA200          ' 纹理贴图
        Friend Const MAT_MAPNAME As UShort = &HA300         ' 贴图文件名
        Friend Const MAT_MAPFILE As UShort = &HA300         ' 贴图文件名（别名）

        ' 颜色子 Chunk
        Friend Const COLOR_F As UShort = &H10               ' 浮点 RGB 颜色
        Friend Const COLOR_24 As UShort = &H11              ' 字节 RGB 颜色

        ' 关键帧数据（跳过不解析）
        Friend Const KEYF3DS As UShort = &HB000             ' 关键帧 Chunk
    End Module

    ' ============================================================================
    ' 主解析模块
    ' ============================================================================

    ''' <summary>
    ''' 3DS (3D-Studio) 三维模型文件解析器。
    ''' 
    ''' 3DS 是 Autodesk 3ds Max 使用的旧版标准交换格式。
    ''' 采用分块（Chunk）存储结构，支持网格几何数据、材质定义、
    ''' 纹理坐标以及局部变换矩阵的解析。
    ''' 
    ''' 注意：.max 是 3ds Max 的专有私有格式（无公开文档，无法可靠解析），
    ''' 本模块解析的是 .3ds 格式文件。
    ''' </summary>
    Public Module Max3DSParser

        ''' <summary>
        ''' 从文件路径解析 3DS 模型文件
        ''' </summary>
        ''' <param name="filePath$">.3ds 文件路径</param>
        ''' <returns>解析后的 SceneModel</returns>
        <Extension>
        Public Function Parse3DS(filePath$) As Data.SceneModel
            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Return Parse3DS(fs)
            End Using
        End Function

        ''' <summary>
        ''' 从 Stream 解析 3DS 模型数据
        ''' </summary>
        ''' <param name="stream">包含 3DS 数据的流</param>
        ''' <returns>解析后的 SceneModel</returns>
        <Extension>
        Public Function Parse3DS(stream As Stream) As Data.SceneModel
            Using reader As New BinaryReader(stream, Encoding.ASCII, leaveOpen:=True)
                Dim rootChunk As Chunk3DS = ReadChunkTree(reader)
                Return ParseMainChunk(reader, rootChunk)
            End Using
        End Function

        ' === Chunk 读取 ===

        ''' <summary>
        ''' 递归读取 Chunk 树结构。
        ''' 从当前流位置开始，读取所有子 Chunk 直到父 Chunk 结束。
        ''' </summary>
        Private Function ReadChunkTree(reader As BinaryReader, Optional parentChunk As Chunk3DS = Nothing) As Chunk3DS
            Dim chunk As Chunk3DS

            If parentChunk Is Nothing Then
                ' 根 Chunk：读取全局头部
                chunk = ReadChunk(reader)
                chunk.dataOffset = reader.BaseStream.Position
            Else
                chunk = parentChunk
            End If

            Dim endPos As Long = chunk.endOffset

            ' 递归读取所有子 Chunk
            While reader.BaseStream.Position < endPos
                Try
                    Dim child As Chunk3DS = ReadChunk(reader)

                    ' 检查子 Chunk 是否越界
                    If child.endOffset > endPos Then
                        ' 子 Chunk 长度异常，跳过
                        reader.BaseStream.Position = endPos
                        Exit While
                    End If

                    child.dataOffset = reader.BaseStream.Position

                    ' 对于包含子节点的 Chunk，递归读取
                    If HasSubChunks(child.chunkId) Then
                        ReadChunkTree(reader, child)
                    End If

                    ' 将流位置移动到当前子 Chunk 的末尾
                    reader.BaseStream.Position = child.endOffset
                    chunk.childChunks.Add(child)

                Catch ex As EndOfStreamException
                    Exit While
                Catch ex As Exception
                    Exit While
                End Try
            End While

            Return chunk
        End Function

        ''' <summary>
        ''' 从 BinaryReader 读取单个 Chunk 的头部信息（6 字节）
        ''' </summary>
        Private Function ReadChunk(reader As BinaryReader) As Chunk3DS
            Return New Chunk3DS With {
                .chunkId = reader.ReadUInt16(),
                .chunkLength = reader.ReadUInt32()
            }
        End Function

        ''' <summary>
        ''' 判断该 Chunk 类型是否包含子 Chunk
        ''' </summary>
        Private Function HasSubChunks(chunkId As UShort) As Boolean
            Select Case chunkId
                Case MAIN3DS, EDIT3DS, EDIT_OBJECT, OBJ_MESH,
                     EDIT_MATERIAL, MAT_TEXMAP,
                     MAT_AMBIENT, MAT_DIFFUSE, MAT_SPECULAR
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        ' === 主 Chunk 解析 ===

        ''' <summary>
        ''' 解析根 Chunk（MAIN3DS 0x4D4D）
        ''' </summary>
        Private Function ParseMainChunk(reader As BinaryReader, rootChunk As Chunk3DS) As Data.SceneModel
            Dim meshes As New List(Of Mesh3DS)()
            Dim materials As New Dictionary(Of String, Material3DS)()

            For Each child As Chunk3DS In rootChunk.childChunks
                Select Case child.chunkId
                    Case EDIT3DS_VERSION
                        ' 版本号，跳过
                    Case EDIT3DS
                        ParseEdit3DS(reader, child, meshes, materials)
                    Case KEYF3DS
                        ' 关键帧数据，跳过不解析
                End Select
            Next

            Return BuildSceneModel(meshes, materials)
        End Function

        ''' <summary>
        ''' 解析 3D 编辑器 Chunk（EDIT3DS 0x3D3D）
        ''' </summary>
        Private Sub ParseEdit3DS(reader As BinaryReader, editChunk As Chunk3DS,
                                 meshes As List(Of Mesh3DS),
                                 materials As Dictionary(Of String, Material3DS))

            For Each child As Chunk3DS In editChunk.childChunks
                Select Case child.chunkId
                    Case EDIT3DS_MESH_VERSION
                        ' Mesh 版本号，跳过

                    Case EDIT_MATERIAL
                        ' 材质定义
                        Dim mat As Material3DS = ParseMaterialBlock(reader, child)
                        If mat IsNot Nothing AndAlso Not String.IsNullOrEmpty(mat.name) Then
                            If Not materials.ContainsKey(mat.name) Then
                                materials.Add(mat.name, mat)
                            End If
                        End If

                    Case EDIT_OBJECT
                        ' 对象定义
                        Dim mesh As Mesh3DS = ParseObjectBlock(reader, child)
                        If mesh IsNot Nothing Then
                            meshes.Add(mesh)
                        End If

                End Select
            Next
        End Sub

        ' === 对象 / 网格解析 ===

        ''' <summary>
        ''' 解析对象定义块（EDIT_OBJECT 0x4000）
        ''' </summary>
        Private Function ParseObjectBlock(reader As BinaryReader, objChunk As Chunk3DS) As Mesh3DS
            ' 首先读取对象名称（以 null 结尾的字符串）
            reader.BaseStream.Position = objChunk.dataOffset
            Dim objName As String = ReadNullTerminatedString(reader)

            Dim mesh As Mesh3DS = Nothing

            For Each child As Chunk3DS In objChunk.childChunks
                If child.chunkId = OBJ_MESH Then
                    mesh = ParseMeshBlock(reader, child)
                    If mesh IsNot Nothing Then
                        mesh.name = objName
                    End If
                End If
            Next

            Return mesh
        End Function

        ''' <summary>
        ''' 解析网格数据块（OBJ_MESH 0x4100）
        ''' </summary>
        Private Function ParseMeshBlock(reader As BinaryReader, meshChunk As Chunk3DS) As Mesh3DS
            Dim mesh As New Mesh3DS()

            For Each child As Chunk3DS In meshChunk.childChunks
                Select Case child.chunkId
                    Case MESH_VERTICES
                        ' 顶点列表
                        reader.BaseStream.Position = child.dataOffset
                        ReadVertexList(reader, mesh)

                    Case MESH_FACES
                        ' 三角面列表
                        reader.BaseStream.Position = child.dataOffset
                        ReadFaceList(reader, mesh)

                    Case MESH_FACE_MAT
                        ' 面材质分配
                        reader.BaseStream.Position = child.dataOffset
                        ReadMaterialFaceMap(reader, mesh)

                    Case MESH_TEX_VERT
                        ' 纹理坐标
                        reader.BaseStream.Position = child.dataOffset
                        ReadTextureVertices(reader, mesh)

                    Case MESH_MATRIX
                        ' 局部变换矩阵
                        reader.BaseStream.Position = child.dataOffset
                        ReadLocalMatrix(reader, mesh)

                    Case MESH_SMOOTH
                        ' 平滑组，跳过

                End Select
            Next

            Return mesh
        End Function

        ''' <summary>
        ''' 读取顶点列表（MESH_VERTICES 0x4110）
        ''' 格式：UInt16 count + count × (Single X, Single Y, Single Z)
        ''' </summary>
        Private Sub ReadVertexList(reader As BinaryReader, mesh As Mesh3DS)
            Dim count As UShort = reader.ReadUInt16()

            For i As Integer = 0 To CInt(count) - 1
                Dim x As Single = reader.ReadSingle()
                Dim z As Single = reader.ReadSingle() ' 3DS 使用 Z-up 坐标系
                Dim y As Single = -reader.ReadSingle()
                mesh.vertices.Add(New Point3D(x, y, z))
            Next
        End Sub

        ''' <summary>
        ''' 读取三角面列表（MESH_FACES 0x4120）
        ''' 格式：UInt16 count + count × (UInt16 A, UInt16 B, UInt16 C, UInt16 faceInfo)
        ''' faceInfo: bit0=AC可见, bit1=BC可见, bit2=AB可见
        ''' </summary>
        Private Sub ReadFaceList(reader As BinaryReader, mesh As Mesh3DS)
            Dim count As UShort = reader.ReadUInt16()

            For i As Integer = 0 To CInt(count) - 1
                Dim a As UShort = reader.ReadUInt16()
                Dim b As UShort = reader.ReadUInt16()
                Dim c As UShort = reader.ReadUInt16()
                Dim faceInfo As UShort = reader.ReadUInt16()
                mesh.faces.Add((a, b, c, faceInfo))
            Next
        End Sub

        ''' <summary>
        ''' 读取面材质分配表（MESH_FACE_MAT 0x4130）
        ''' 格式：以 null 结尾的材质名 + UInt16 count + count × UInt16 faceIndex
        ''' </summary>
        Private Sub ReadMaterialFaceMap(reader As BinaryReader, mesh As Mesh3DS)
            Dim matName As String = ReadNullTerminatedString(reader)
            Dim count As UShort = reader.ReadUInt16()

            If String.IsNullOrEmpty(matName) Then Return

            Dim faceIndices As New List(Of UShort)(CInt(count))

            For i As Integer = 0 To CInt(count) - 1
                faceIndices.Add(reader.ReadUInt16())
            Next

            mesh.faceMaterials(matName) = faceIndices
        End Sub

        ''' <summary>
        ''' 读取纹理坐标（MESH_TEX_VERT 0x4140）
        ''' 格式：UInt16 count + count × (Single U, Single V)
        ''' </summary>
        Private Sub ReadTextureVertices(reader As BinaryReader, mesh As Mesh3DS)
            Dim count As UShort = reader.ReadUInt16()

            For i As Integer = 0 To CInt(count) - 1
                Dim u As Single = reader.ReadSingle()
                Dim v As Single = reader.ReadSingle()
                mesh.textureVertices.Add((u, v))
            Next
        End Sub

        ''' <summary>
        ''' 读取局部变换矩阵（MESH_MATRIX 0x4160）
        ''' 格式：12 × Single（4×3 矩阵，行主序）
        ''' 第 4 列隐含为 [0, 0, 0, 1]
        ''' </summary>
        Private Sub ReadLocalMatrix(reader As BinaryReader, mesh As Mesh3DS)
            Dim matrix As Single() = New Single(11) {}

            For i As Integer = 0 To 11
                matrix(i) = reader.ReadSingle()
            Next

            mesh.localMatrix = matrix
        End Sub

        ''' <summary>
        ''' 应用局部变换矩阵到顶点。
        ''' 矩阵为 4×3 行主序：前三行每行 3 个元素为旋转/缩放，
        ''' 第四行 3 个元素为平移。
        ''' </summary>
        Private Sub ApplyLocalMatrix(mesh As Mesh3DS)
            If mesh.localMatrix Is Nothing Then Return
            If mesh.localMatrix.Length < 12 Then Return

            Dim m As Single() = mesh.localMatrix

            For i As Integer = 0 To mesh.vertices.Count - 1
                Dim pt As Point3D = mesh.vertices(i)
                Dim x As Single = pt.X
                Dim y As Single = pt.Y
                Dim z As Single = pt.Z

                mesh.vertices(i) = New Point3D(
                    x * m(0) + y * m(1) + z * m(2) + m(9),
                    x * m(3) + y * m(4) + z * m(5) + m(10),
                    x * m(6) + y * m(7) + z * m(8) + m(11)
                )
            Next
        End Sub

        ' === 材质解析 ===

        ''' <summary>
        ''' 解析材质定义块（EDIT_MATERIAL 0xAFFF）
        ''' </summary>
        Private Function ParseMaterialBlock(reader As BinaryReader, matChunk As Chunk3DS) As Material3DS
            Dim material As New Material3DS()

            For Each child As Chunk3DS In matChunk.childChunks
                Select Case child.chunkId
                    Case MAT_NAME01
                        ' 材质名称
                        reader.BaseStream.Position = child.dataOffset
                        material.name = ReadNullTerminatedString(reader)

                    Case MAT_DIFFUSE
                        ' 漫反射颜色
                        ParseMaterialColor(reader, child, material, "diffuse")

                    Case MAT_AMBIENT
                        ' 环境光颜色（暂不处理）
                        ' ParseMaterialColor(reader, child, material, "ambient")

                    Case MAT_SPECULAR
                        ' 镜面反射颜色（暂不处理）
                        ' ParseMaterialColor(reader, child, material, "specular")

                    Case MAT_TEXMAP
                        ' 纹理贴图
                        ParseTextureMap(reader, child, material)

                End Select
            Next

            Return material
        End Function

        ''' <summary>
        ''' 解析材质颜色子 Chunk（MAT_DIFFUSE / MAT_AMBIENT / MAT_SPECULAR）
        ''' 这些 Chunk 包含 COLOR_F 或 COLOR_24 子 Chunk
        ''' </summary>
        Private Sub ParseMaterialColor(reader As BinaryReader, colorChunk As Chunk3DS,
                                       material As Material3DS, colorType As String)
            For Each child As Chunk3DS In colorChunk.childChunks
                If child.chunkId = COLOR_F Then
                    reader.BaseStream.Position = child.dataOffset
                    Dim color = ReadColorFloat(reader)
                    material.diffuseColor = color
                    Return
                ElseIf child.chunkId = COLOR_24 Then
                    reader.BaseStream.Position = child.dataOffset
                    Dim color = ReadColorByte(reader)
                    material.diffuseColor = color
                    Return
                End If
            Next
        End Sub

        ''' <summary>
        ''' 解析纹理贴图（MAT_TEXMAP 0xA200）
        ''' </summary>
        Private Sub ParseTextureMap(reader As BinaryReader, texmapChunk As Chunk3DS,
                                    material As Material3DS)
            For Each child As Chunk3DS In texmapChunk.childChunks
                If child.chunkId = MAT_MAPNAME Then
                    reader.BaseStream.Position = child.dataOffset
                    material.textureFile = ReadNullTerminatedString(reader)
                    Return
                End If
            Next
        End Sub

        ' === 颜色读取工具方法 ===

        ''' <summary>
        ''' 读取浮点 RGB 颜色（COLOR_F）：3 × Single (R, G, B)
        ''' </summary>
        Private Function ReadColorFloat(reader As BinaryReader) As (R As Single, G As Single, B As Single)
            Return (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
        End Function

        ''' <summary>
        ''' 读取字节 RGB 颜色（COLOR_24）：3 × Byte (R, G, B)，映射到 0.0 ~ 1.0
        ''' </summary>
        Private Function ReadColorByte(reader As BinaryReader) As (R As Single, G As Single, B As Single)
            Return (
                CSng(reader.ReadByte()) / 255.0F,
                CSng(reader.ReadByte()) / 255.0F,
                CSng(reader.ReadByte()) / 255.0F
            )
        End Function

        ' === 字符串读取工具方法 ===

        ''' <summary>
        ''' 读取以 null（&amp;H00）结尾的 ASCII 字符串
        ''' </summary>
        Private Function ReadNullTerminatedString(reader As BinaryReader) As String
            Dim bytes As New List(Of Byte)()

            Try
                While True
                    Dim b As Byte = reader.ReadByte()
                    If b = 0 Then Exit While
                    bytes.Add(b)
                End While
            Catch ex As EndOfStreamException
                ' 达到流末尾，停止读取
            End Try

            If bytes.Count = 0 Then Return String.Empty
            Return Encoding.ASCII.GetString(bytes.ToArray())
        End Function

        ' === 场景模型构建 ===

        ''' <summary>
        ''' 将所有解析出的网格和材质组装为 SceneModel
        ''' </summary>
        Private Function BuildSceneModel(meshes As List(Of Mesh3DS),
                                         materials As Dictionary(Of String, Material3DS)) As Data.SceneModel
            Dim allSurfaces As New List(Of Data.Surface)()

            For Each mesh As Mesh3DS In meshes
                If mesh Is Nothing OrElse mesh.vertices.Count = 0 OrElse mesh.faces.Count = 0 Then
                    Continue For
                End If

                ' 应用局部变换矩阵
                ApplyLocalMatrix(mesh)

                ' 构建每个面的材质查找表
                ' Key: 面索引, Value: 材质名称
                Dim faceMaterialMap As New Dictionary(Of UShort, String)()
                For Each kvp In mesh.faceMaterials
                    For Each faceIdx As UShort In kvp.Value
                        If Not faceMaterialMap.ContainsKey(faceIdx) Then
                            faceMaterialMap(faceIdx) = kvp.Key
                        End If
                    Next
                Next

                ' 为每个三角面生成 Surface
                For faceIdx As Integer = 0 To mesh.faces.Count - 1
                    Dim face = mesh.faces(faceIdx)

                    ' 检查顶点索引有效性
                    If face.A >= mesh.vertices.Count OrElse
                       face.B >= mesh.vertices.Count OrElse
                       face.C >= mesh.vertices.Count Then
                        Continue For
                    End If

                    Dim v1 As Point3D = mesh.vertices(face.A)
                    Dim v2 As Point3D = mesh.vertices(face.B)
                    Dim v3 As Point3D = mesh.vertices(face.C)

                    ' 获取该面的颜色
                    Dim paint As String = "#C0C0C0" ' 默认银色
                    Dim faceIdxU As UShort = CUShort(faceIdx)

                    If faceMaterialMap.ContainsKey(faceIdxU) Then
                        Dim matName As String = faceMaterialMap(faceIdxU)
                        If materials.ContainsKey(matName) Then
                            paint = materials(matName).ToHtmlColor()
                        End If
                    End If

                    Try
                        allSurfaces.Add(New Data.Surface With {
                            .vertices = {
                                New Data.Vertex(v1),
                                New Data.Vertex(v2),
                                New Data.Vertex(v3)
                            },
                            .paint = paint
                        })
                    Catch ex As Exception
                        ' 跳过无法构建的面
                    End Try
                Next
            Next

            Return New Data.SceneModel With {
                .Surfaces = allSurfaces.ToArray()
            }
        End Function

    End Module

End Namespace
