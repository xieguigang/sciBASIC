#Region "Microsoft.VisualBasic::GLB_Reader, gr\Landscape\glTF\GlbReader.vb"

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

    '     Module GlbReader
    '         Function: ReadFile, ReadStream

    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.Json

Namespace Gltf

    ''' <summary>
    ''' GLB (GL Transmission Format Binary) 二进制格式 3D 模型文件解析器
    ''' 
    ''' GLB 是 glTF 的二进制容器格式，将所有 JSON 和二进制数据打包在单个文件中。
    ''' 
    ''' GLB 文件结构：
    ''' ┌──────────────────────────────┐
    ''' │ Header (12 bytes)           │
    ''' │  magic: uint32 = 0x46546C67 │  ("glTF")
    ''' │  version: uint32 = 2        │
    ''' │  length: uint32             │  文件总长度
    ''' ├──────────────────────────────┤
    ''' │ Chunk 0: JSON               │
    ''' │  chunkLength: uint32        │
    ''' │  chunkType: uint32          │  0x4E4F534A ("JSON")
    ''' │  chunkData: byte[]          │  UTF-8 JSON 文本
    ''' ├──────────────────────────────┤
    ''' │ Chunk 1: BIN (可选)         │
    ''' │  chunkLength: uint32        │
    ''' │  chunkType: uint32          │  0x004E4942 ("BIN\0")
    ''' │  chunkData: byte[]          │  二进制缓冲区数据
    ''' └──────────────────────────────┘
    ''' </summary>
    Public Module GlbReader

        ' "glTF" magic number
        Private Const GLB_MAGIC As UInteger = &H46546C67UI
        ' "JSON" chunk type
        Private Const CHUNK_TYPE_JSON As UInteger = &H4E4F534AUI
        ' "BIN\0" chunk type
        Private Const CHUNK_TYPE_BIN As UInteger = &H004E4942UI

        Private ReadOnly JsonOptions As New JsonSerializerOptions With {
            .PropertyNameCaseInsensitive = True,
            .ReadCommentHandling = JsonCommentHandling.Skip,
            .AllowTrailingCommas = True
        }

        ''' <summary>
        ''' 从 .glb 文件路径读取 3D 模型
        ''' </summary>
        ''' <param name="filePath$">.glb 文件的完整路径</param>
        ''' <returns>解析后的 SceneModel</returns>
        <Extension>
        Public Function ReadFile(filePath$) As Data.SceneModel
            Using stream = File.OpenRead(filePath)
                Return ReadStream(stream)
            End Using
        End Function

        ''' <summary>
        ''' 从 Stream 读取 GLB 格式 3D 模型
        ''' </summary>
        ''' <param name="stream">包含 GLB 数据的流</param>
        ''' <returns>解析后的 SceneModel</returns>
        <Extension>
        Public Function ReadStream(stream As Stream) As Data.SceneModel
            Using reader As New BinaryReader(stream, Encoding.UTF8, leaveOpen:=True)
                ' 读取 12 字节文件头
                Dim magic = reader.ReadUInt32()
                Dim version = reader.ReadUInt32()
                Dim totalLength = reader.ReadUInt32()

                ' 验证 magic number
                If magic <> GLB_MAGIC Then
                    Throw New InvalidDataException($"无效的 GLB 文件：magic number 不匹配 (期望 0x{GLB_MAGIC:X8}, 实际 0x{magic:X8})")
                End If

                ' 验证版本号（通常为 2）
                If version <> 2 Then
                    Throw New InvalidDataException($"不支持的 GLB 版本：{version}（仅支持版本 2）")
                End If

                Dim jsonText As String = Nothing
                Dim binaryBuffer As Byte() = Nothing

                ' 读取 Chunks
                While stream.Position < totalLength
                    Dim chunkLength = CInt(reader.ReadUInt32())
                    Dim chunkType = reader.ReadUInt32()

                    If chunkLength <= 0 Then Continue While

                    Dim chunkData = reader.ReadBytes(chunkLength)

                    Select Case chunkType
                        Case CHUNK_TYPE_JSON
                            jsonText = Encoding.UTF8.GetString(chunkData)

                        Case CHUNK_TYPE_BIN
                            binaryBuffer = chunkData
                    End Select
                End While

                If String.IsNullOrEmpty(jsonText) Then
                    Throw New InvalidDataException("GLB 文件中未找到 JSON Chunk")
                End If

                ' 解析 JSON
                Dim root = JsonSerializer.Deserialize(Of GltfRoot)(jsonText, JsonOptions)

                ' 构建缓冲区列表
                ' GLB 中的 BIN Chunk 默认为 buffer[0]
                Dim binaryBuffers = BuildBufferList(root, binaryBuffer)

                ' 构建 SceneModel
                Return ModelBuilder.BuildSceneModel(root, binaryBuffers)
            End Using
        End Function

        ''' <summary>
        ''' 构建与 glTF Buffer 定义对应的二进制数据列表
        ''' </summary>
        ''' <param name="root">glTF 根对象</param>
        ''' <param name="binChunkData">GLB BIN Chunk 的原始数据</param>
        ''' <returns>按 Buffer 索引排列的字节数组列表</returns>
        Private Function BuildBufferList(root As GltfRoot, binChunkData As Byte()) As List(Of Byte())
            Dim result As New List(Of Byte())

            If root.Buffers Is Nothing OrElse root.Buffers.Length = 0 Then
                ' 无显式 buffers 定义，直接使用 BIN Chunk 作为 buffer[0]
                result.Add(binChunkData)
                Return result
            End If

            For i As Integer = 0 To root.Buffers.Length - 1
                Dim buffer = root.Buffers(i)

                If i = 0 AndAlso String.IsNullOrEmpty(buffer.Uri) Then
                    ' buffer[0] 通常对应 GLB 的 BIN Chunk
                    result.Add(binChunkData)
                ElseIf Not String.IsNullOrEmpty(buffer.Uri) Then
                    ' 外部缓冲区引用（GLB 中较少见）
                    result.Add(Nothing)
                Else
                    ' 空的 buffer 占位
                    result.Add(Nothing)
                End If
            Next

            Return result
        End Function

    End Module

End Namespace
