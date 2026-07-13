#Region "Microsoft.VisualBasic::glTF_ModelBuilder, gr\Landscape\glTF\ModelBuilder.vb"

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

'     Module ModelBuilder
'         Function: BuildSceneModel, ExtractFloatData, ExtractIndices, GetMeshSurfaces, ReadColorFromMaterial, ReadColorFromPrimitive

' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports std = System.Math

Namespace Gltf

    ''' <summary>
    ''' glTF / GLB 共享的 SceneModel 构建逻辑。
    ''' 负责将已加载的 GltfRoot JSON 模型和二进制缓冲区数组转换为 Data.SceneModel。
    ''' </summary>
    Friend Module ModelBuilder

        Private Const DEFAULT_COLOR As String = "#C0C0C0"

        ''' <summary>
        ''' 从已解析的 glTF 根对象和缓冲区数组构建 SceneModel
        ''' </summary>
        ''' <param name="root">解析后的 glTF JSON 根对象</param>
        ''' <param name="binaryBuffers">按 Buffer 索引排列的原始字节数组列表</param>
        ''' <returns>统一的 SceneModel 对象</returns>
        Public Function BuildSceneModel(root As GltfRoot, binaryBuffers As List(Of Byte())) As Data.SceneModel
            If root Is Nothing OrElse root.Meshes Is Nothing OrElse root.Meshes.Length = 0 Then
                Return New Data.SceneModel With {.Surfaces = {}}
            End If

            Dim surfaces As New List(Of Data.Surface)

            ' 确定需要处理的网格索引
            Dim meshIndices As IEnumerable(Of Integer) = GetActiveMeshIndices(root)

            For Each meshIndex As Integer In meshIndices
                If meshIndex < root.Meshes.Length Then
                    Dim meshSurfaces = GetMeshSurfaces(root, meshIndex, binaryBuffers)
                    If meshSurfaces IsNot Nothing Then
                        surfaces.AddRange(meshSurfaces)
                    End If
                End If
            Next

            Return New Data.SceneModel With {
                .Surfaces = surfaces.ToArray
            }
        End Function

        ''' <summary>
        ''' 获取活跃的网格索引（默认场景引用的网格，若无场景则返回所有网格）
        ''' </summary>
        Private Function GetActiveMeshIndices(root As GltfRoot) As IEnumerable(Of Integer)
            ' 如果有默认场景，则只处理该场景中引用的网格
            If root.DefaultScene.HasValue AndAlso root.Scenes IsNot Nothing Then
                Dim sceneIdx = root.DefaultScene.Value
                If sceneIdx >= 0 AndAlso sceneIdx < root.Scenes.Length Then
                    Dim scene = root.Scenes(sceneIdx)
                    If scene.Nodes IsNot Nothing AndAlso scene.Nodes.Length > 0 Then
                        Return CollectMeshIndicesFromNodes(root, scene.Nodes)
                    End If
                End If
            End If

            ' 否则返回所有网格索引
            If root.Meshes IsNot Nothing Then
                Return Enumerable.Range(0, root.Meshes.Length)
            End If

            Return {}
        End Function

        ''' <summary>
        ''' 递归收集节点树中的所有网格索引
        ''' </summary>
        Private Function CollectMeshIndicesFromNodes(root As GltfRoot, nodeIndices As IEnumerable(Of Integer)) As List(Of Integer)
            Dim result As New List(Of Integer)

            For Each nodeIdx As Integer In nodeIndices
                If nodeIdx >= 0 AndAlso root.Nodes IsNot Nothing AndAlso nodeIdx < root.Nodes.Length Then
                    Dim node = root.Nodes(nodeIdx)

                    If node.MeshIndex.HasValue Then
                        result.Add(node.MeshIndex.Value)
                    End If

                    If node.Children IsNot Nothing AndAlso node.Children.Length > 0 Then
                        result.AddRange(CollectMeshIndicesFromNodes(root, node.Children))
                    End If
                End If
            Next

            Return result
        End Function

        ''' <summary>
        ''' 从指定网格中提取所有 Surface（三角面）
        ''' </summary>
        Public Function GetMeshSurfaces(root As GltfRoot, meshIndex As Integer, binaryBuffers As List(Of Byte())) As Data.Surface()
            Dim mesh = root.Meshes(meshIndex)
            If mesh.Primitives Is Nothing OrElse mesh.Primitives.Length = 0 Then
                Return {}
            End If

            Dim results As New List(Of Data.Surface)

            For Each primitive As Primitive In mesh.Primitives
                Dim primitiveSurfaces = ExtractPrimitiveSurfaces(root, primitive, binaryBuffers)
                If primitiveSurfaces IsNot Nothing Then
                    results.AddRange(primitiveSurfaces)
                End If
            Next

            Return results.ToArray
        End Function

        ''' <summary>
        ''' 从一个 Primitive 中提取所有三角面
        ''' </summary>
        Private Function ExtractPrimitiveSurfaces(root As GltfRoot, primitive As Primitive, binaryBuffers As List(Of Byte())) As Data.Surface()
            ' 获取顶点位置数据
            If primitive.Attributes Is Nothing OrElse Not primitive.Attributes.ContainsKey("POSITION") Then
                Return {}
            End If

            Dim positionAccessorIdx = primitive.Attributes("POSITION")
            Dim positions As Single()() = ExtractFloatDataArray(root, positionAccessorIdx, binaryBuffers)

            If positions Is Nothing OrElse positions.Length = 0 Then
                Return {}
            End If

            ' 获取颜色
            Dim paintColor = ReadColorFromPrimitive(root, primitive)

            ' 获取索引数据
            Dim indices As Integer() = Nothing
            If primitive.IndicesAccessor.HasValue Then
                indices = ExtractIndices(root, primitive.IndicesAccessor.Value, binaryBuffers)
            End If

            ' 确定渲染模式（默认为 TRIANGLES）
            Dim mode = If(primitive.Mode.HasValue, primitive.Mode.Value, 4)
            Dim surfaces As New List(Of Data.Surface)

            If indices IsNot Nothing AndAlso indices.Length > 0 Then
                ' 索引几何体
                Select Case mode
                    Case 4 ' TRIANGLES
                        For i As Integer = 0 To indices.Length - 3 Step 3
                            Dim i0 = indices(i)
                            Dim i1 = indices(i + 1)
                            Dim i2 = indices(i + 2)

                            If i0 < positions.Length AndAlso i1 < positions.Length AndAlso i2 < positions.Length Then
                                surfaces.Add(CreateSurface(positions(i0), positions(i1), positions(i2), paintColor))
                            End If
                        Next
                    Case 5 ' TRIANGLE_STRIP
                        For i As Integer = 0 To indices.Length - 3
                            Dim i0 = indices(i)
                            Dim i1 = indices(i + 1)
                            Dim i2 = indices(i + 2)

                            If i0 < positions.Length AndAlso i1 < positions.Length AndAlso i2 < positions.Length Then
                                ' 奇偶翻转
                                If (i And 1) = 1 Then
                                    surfaces.Add(CreateSurface(positions(i1), positions(i0), positions(i2), paintColor))
                                Else
                                    surfaces.Add(CreateSurface(positions(i0), positions(i1), positions(i2), paintColor))
                                End If
                            End If
                        Next
                    Case 6 ' TRIANGLE_FAN
                        If indices.Length >= 3 Then
                            Dim firstIdx = indices(0)
                            For i As Integer = 1 To indices.Length - 2
                                Dim i0 = firstIdx
                                Dim i1 = indices(i)
                                Dim i2 = indices(i + 1)

                                If i0 < positions.Length AndAlso i1 < positions.Length AndAlso i2 < positions.Length Then
                                    surfaces.Add(CreateSurface(positions(i0), positions(i1), positions(i2), paintColor))
                                End If
                            Next
                        End If
                    Case Else
                        ' 非三角形模式暂不处理
                End Select
            Else
                ' 非索引几何体：直接按顶点顺序
                Select Case mode
                    Case 4 ' TRIANGLES
                        For i As Integer = 0 To positions.Length - 3 Step 3
                            surfaces.Add(CreateSurface(positions(i), positions(i + 1), positions(i + 2), paintColor))
                        Next
                    Case 5 ' TRIANGLE_STRIP
                        For i As Integer = 0 To positions.Length - 3
                            If (i And 1) = 1 Then
                                surfaces.Add(CreateSurface(positions(i + 1), positions(i), positions(i + 2), paintColor))
                            Else
                                surfaces.Add(CreateSurface(positions(i), positions(i + 1), positions(i + 2), paintColor))
                            End If
                        Next
                    Case 6 ' TRIANGLE_FAN
                        If positions.Length >= 3 Then
                            For i As Integer = 1 To positions.Length - 2
                                surfaces.Add(CreateSurface(positions(0), positions(i), positions(i + 1), paintColor))
                            Next
                        End If
                End Select
            End If

            Return surfaces.ToArray
        End Function

        ''' <summary>
        ''' 创建单个三角面 Surface
        ''' </summary>
        Private Function CreateSurface(v1 As Single(), v2 As Single(), v3 As Single(), paintColor As String) As Data.Surface
            Return New Data.Surface With {
                .vertices = {
                    New Data.Vertex(New Point3D(v1(0), v1(1), v1(2))),
                    New Data.Vertex(New Point3D(v2(0), v2(1), v2(2))),
                    New Data.Vertex(New Point3D(v3(0), v3(1), v3(2)))
                },
                .paint = paintColor
            }
        End Function

        ''' <summary>
        ''' 从 Primitive 的颜色信息中提取 paint 字符串
        ''' </summary>
        Public Function ReadColorFromPrimitive(root As GltfRoot, primitive As Primitive) As String
            If primitive.MaterialIndex.HasValue AndAlso root.Materials IsNot Nothing Then
                Dim matIdx = primitive.MaterialIndex.Value
                If matIdx >= 0 AndAlso matIdx < root.Materials.Length Then
                    Dim color = ReadColorFromMaterial(root.Materials(matIdx))
                    If Not String.IsNullOrEmpty(color) Then
                        Return color
                    End If
                End If
            End If

            Return DEFAULT_COLOR
        End Function

        ''' <summary>
        ''' 从材质中提取颜色信息
        ''' </summary>
        Private Function ReadColorFromMaterial(material As GltfMaterial) As String
            If material Is Nothing Then Return Nothing

            If material.PbrMetallicRoughness IsNot Nothing AndAlso
               material.PbrMetallicRoughness.BaseColorFactor IsNot Nothing AndAlso
               material.PbrMetallicRoughness.BaseColorFactor.Length >= 3 Then

                Dim factor = material.PbrMetallicRoughness.BaseColorFactor
                Dim r = CInt(std.Min(factor(0) * 255, 255))
                Dim g = CInt(std.Min(factor(1) * 255, 255))
                Dim b = CInt(std.Min(factor(2) * 255, 255))

                Return $"#{r:X2}{g:X2}{b:X2}"
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' 从 Accessor 提取浮点顶点数据数组（每个元素是一个 Float 数组，长度由 accessor type 决定）
        ''' </summary>
        Public Function ExtractFloatDataArray(root As GltfRoot, accessorIndex As Integer, binaryBuffers As List(Of Byte())) As Single()()
            If accessorIndex < 0 OrElse root.Accessors Is Nothing OrElse accessorIndex >= root.Accessors.Length Then
                Return Nothing
            End If

            Dim accessor = root.Accessors(accessorIndex)
            Dim components = accessor.NumComponents

            If components <= 0 Then
                Return Nothing
            End If

            Dim rawValues As Single() = ExtractFloatData(root, accessorIndex, binaryBuffers)

            If rawValues Is Nothing OrElse rawValues.Length = 0 Then
                Return Nothing
            End If

            ' 将扁平化的 Float 数组按组件数分组
            Dim result As Single()() = New Single(accessor.Count - 1)() {}

            For i As Integer = 0 To accessor.Count - 1
                Dim startIdx = i * components
                If startIdx + components > rawValues.Length Then Exit For

                result(i) = New Single(components - 1) {}
                Array.Copy(rawValues, startIdx, result(i), 0, components)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 从 Accessor 中提取扁平化的 Float 数组
        ''' </summary>
        Public Function ExtractFloatData(root As GltfRoot, accessorIndex As Integer, binaryBuffers As List(Of Byte())) As Single()
            If accessorIndex < 0 OrElse root.Accessors Is Nothing OrElse accessorIndex >= root.Accessors.Length Then
                Return Nothing
            End If

            Dim accessor = root.Accessors(accessorIndex)

            If Not accessor.BufferViewIndex.HasValue Then
                Return Nothing
            End If

            Dim bufferView = root.BufferViews(accessor.BufferViewIndex.Value)
            Dim buffer As Byte() = binaryBuffers(bufferView.BufferIndex)

            If buffer Is Nothing Then
                Return Nothing
            End If

            Dim byteOffset = accessor.ByteOffset.GetValueOrDefault(0) + bufferView.ByteOffset.GetValueOrDefault(0)
            Dim byteStride = bufferView.ByteStride.GetValueOrDefault(0)
            Dim componentCount = accessor.NumComponents
            Dim componentSize = accessor.ComponentSize

            ' 如果没有显式的 byteStride，则紧凑排列
            If byteStride = 0 Then
                byteStride = componentCount * componentSize
            End If

            Dim totalFloats = accessor.Count * componentCount
            Dim result As Single() = New Single(totalFloats - 1) {}
            Dim valueIndex As Integer = 0

            For i As Integer = 0 To accessor.Count - 1
                Dim elementOffset = byteOffset + i * byteStride

                For j As Integer = 0 To componentCount - 1
                    Dim componentOffset = elementOffset + j * componentSize

                    If componentOffset + componentSize > buffer.Length Then
                        ' 越界保护
                        Dim truncated As Single() = New Single(valueIndex - 1) {}
                        Array.Copy(result, truncated, valueIndex)
                        Return truncated
                    End If

                    Select Case accessor.ComponentType
                        Case 5120 ' BYTE (signed byte -> float)
                            result(valueIndex) = CSng(CSByte(buffer(componentOffset)))
                        Case 5121 ' UNSIGNED_BYTE
                            result(valueIndex) = CSng(buffer(componentOffset))
                        Case 5122 ' SHORT
                            result(valueIndex) = CSng(BitConverter.ToInt16(buffer, componentOffset))
                        Case 5123 ' UNSIGNED_SHORT
                            result(valueIndex) = CSng(BitConverter.ToUInt16(buffer, componentOffset))
                        Case 5125 ' UNSIGNED_INT
                            result(valueIndex) = CSng(BitConverter.ToUInt32(buffer, componentOffset))
                        Case 5126 ' FLOAT
                            result(valueIndex) = BitConverter.ToSingle(buffer, componentOffset)
                        Case Else
                            result(valueIndex) = 0F
                    End Select

                    valueIndex += 1
                Next
            Next

            Return result
        End Function

        ''' <summary>
        ''' 从索引 Accessor 中提取索引数组
        ''' </summary>
        Public Function ExtractIndices(root As GltfRoot, accessorIndex As Integer, binaryBuffers As List(Of Byte())) As Integer()
            If accessorIndex < 0 OrElse root.Accessors Is Nothing OrElse accessorIndex >= root.Accessors.Length Then
                Return Nothing
            End If

            Dim accessor = root.Accessors(accessorIndex)

            If Not accessor.BufferViewIndex.HasValue Then
                Return Nothing
            End If

            Dim bufferView = root.BufferViews(accessor.BufferViewIndex.Value)
            Dim buffer As Byte() = binaryBuffers(bufferView.BufferIndex)

            If buffer Is Nothing Then
                Return Nothing
            End If

            Dim byteOffset = accessor.ByteOffset.GetValueOrDefault(0) + bufferView.ByteOffset.GetValueOrDefault(0)
            Dim result As Integer() = New Integer(accessor.Count - 1) {}

            For i As Integer = 0 To accessor.Count - 1
                Dim offset = byteOffset + i * accessor.ComponentSize

                If offset + accessor.ComponentSize > buffer.Length Then
                    ' 越界保护
                    Dim truncated As Integer() = New Integer(i - 1) {}
                    Array.Copy(result, truncated, i)
                    Return truncated
                End If

                Select Case accessor.ComponentType
                    Case 5121 ' UNSIGNED_BYTE
                        result(i) = CInt(buffer(offset))
                    Case 5123 ' UNSIGNED_SHORT
                        result(i) = CInt(BitConverter.ToUInt16(buffer, offset))
                    Case 5125 ' UNSIGNED_INT
                        result(i) = CInt(BitConverter.ToUInt32(buffer, offset))
                    Case Else
                        result(i) = 0
                End Select
            Next

            Return result
        End Function

    End Module

End Namespace
