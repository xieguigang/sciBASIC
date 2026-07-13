#Region "Microsoft.VisualBasic::glTF_Reader, gr\Landscape\glTF\GltfReader.vb"

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

    '     Module GltfReader
    '         Function: LoadBuffers, ReadFile

    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.Json

Namespace Gltf

    ''' <summary>
    ''' glTF (.gltf) 文本格式 3D 模型文件解析器
    ''' 
    ''' glTF (GL Transmission Format) 是 Khronos Group 制定的开放标准 3D 场景格式。
    ''' 文本格式 (.gltf) 使用 JSON 描述场景结构，外部二进制文件 (.bin) 存储顶点数据。
    ''' </summary>
    Public Module GltfReader

        Private ReadOnly JsonOptions As New JsonSerializerOptions With {
            .PropertyNameCaseInsensitive = True,
            .ReadCommentHandling = JsonCommentHandling.Skip,
            .AllowTrailingCommas = True
        }

        ''' <summary>
        ''' 从 .gltf 文件路径读取 3D 模型
        ''' </summary>
        ''' <param name="filePath$">.gltf 文件的完整路径</param>
        ''' <returns>解析后的 SceneModel</returns>
        <Extension>
        Public Function ReadFile(filePath$) As Data.SceneModel
            Dim baseDir = Path.GetDirectoryName(filePath)

            ' 读取并解析 JSON
            Dim jsonText = File.ReadAllText(filePath, Encoding.UTF8)
            Dim root = JsonSerializer.Deserialize(Of GltfRoot)(jsonText, JsonOptions)

            ' 加载所有二进制缓冲区（外部 .bin 文件或 data URI）
            Dim binaryBuffers = LoadBuffers(root, baseDir)

            ' 构建 SceneModel
            Return ModelBuilder.BuildSceneModel(root, binaryBuffers)
        End Function

        ''' <summary>
        ''' 加载 glTF JSON 中引用的所有二进制缓冲区
        ''' </summary>
        ''' <param name="root">glTF 根对象</param>
        ''' <param name="baseDir">.gltf 文件所在目录，用于解析相对路径</param>
        ''' <returns>按 Buffer 索引排列的原始字节数组列表</returns>
        Public Function LoadBuffers(root As GltfRoot, baseDir As String) As List(Of Byte())
            Dim result As New List(Of Byte())

            If root.Buffers Is Nothing OrElse root.Buffers.Length = 0 Then
                Return result
            End If

            For Each buffer As GltfBuffer In root.Buffers
                Dim data As Byte() = Nothing

                If String.IsNullOrEmpty(buffer.Uri) Then
                    ' GLB 内嵌缓冲区（由 GlbReader 填充），这里创建一个空占位
                    result.Add(Nothing)
                    Continue For
                End If

                If buffer.Uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase) Then
                    ' data URI 内嵌二进制数据
                    data = ParseDataUri(buffer.Uri)
                Else
                    ' 外部文件引用
                    Dim bufferPath = Path.Combine(baseDir, buffer.Uri)
                    If File.Exists(bufferPath) Then
                        data = File.ReadAllBytes(bufferPath)
                    End If
                End If

                result.Add(data)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 解析 data: URI 为字节数组
        ''' 格式: data:[&lt;mediatype>][;base64],&lt;data>
        ''' </summary>
        Private Function ParseDataUri(uri As String) As Byte()
            Try
                ' data:application/octet-stream;base64,AAAA...
                Dim commaIndex = uri.IndexOf(","c)
                If commaIndex < 0 Then Return Nothing

                Dim header = uri.Substring(0, commaIndex)
                Dim data = uri.Substring(commaIndex + 1)

                If header.IndexOf("base64", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    Return Convert.FromBase64String(data)
                Else
                    ' URL-encoded binary (rare)
                    Return Encoding.UTF8.GetBytes(System.Uri.UnescapeDataString(data))
                End If
            Catch
                Return Nothing
            End Try
        End Function

    End Module

End Namespace
