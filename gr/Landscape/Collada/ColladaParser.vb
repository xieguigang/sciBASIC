#Region "Microsoft.VisualBasic::a1c8e9d2f4b4c7a9b3e5d1c0a2f8b3, gr\Landscape\Collada\ColladaParser.vb"

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

'     Module ColladaParser
' 
'         Function: ReadFile, ReadGeometries
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq

Namespace Collada

    ''' <summary>
    ''' COLLADA (.dae) 文件几何数据解析器
    ''' </summary>
    Public Partial Module ColladaParser

        Private ReadOnly COLLADA_NS As XNamespace = "http://www.collada.org/2005/11/COLLADASchema"

        ''' <summary>
        ''' 从 .dae 文件读取所有几何数据并转换为 Surface 数组
        ''' </summary>
        ''' <param name="filePath$">DAE 文件路径</param>
        ''' <returns></returns>
        Public Function ReadFile(filePath$) As Data.SceneModel
            Dim surfaces As Data.Surface() = ReadGeometries(filePath)
            Return New Data.SceneModel With {
                .Surfaces = surfaces
            }
        End Function

        ''' <summary>
        ''' 从 .dae 文件读取几何数据并转换为 Surface 数组
        ''' </summary>
        ''' <param name="filePath$"></param>
        ''' <returns></returns>
        Public Function ReadGeometries(filePath$) As Data.Surface()
            Dim doc As XDocument = XDocument.Load(filePath)
            Return ReadGeometries(doc)
        End Function

        ''' <summary>
        ''' 从 XDocument 解析 COLLADA 几何数据
        ''' </summary>
        ''' <param name="doc"></param>
        ''' <returns></returns>
        Public Function ReadGeometries(doc As XDocument) As Data.Surface()
            If doc.Root Is Nothing Then
                Return {}
            End If

            Dim surfaces As New List(Of Data.Surface)
            Dim geometriesRoot = doc.Root.Element(COLLADA_NS + "library_geometries")

            If geometriesRoot Is Nothing Then
                Return {}
            End If

            For Each geomElem As XElement In geometriesRoot.Elements(COLLADA_NS + "geometry")
                Try
                    Dim meshSurfaces As Data.Surface() = ParseGeometry(geomElem)
                    If meshSurfaces IsNot Nothing Then
                        surfaces.AddRange(meshSurfaces)
                    End If
                Catch ex As Exception
                    ' 跳过解析失败的 geometry
                End Try
            Next

            Return surfaces.ToArray
        End Function

        ''' <summary>
        ''' 解析单个 &lt;geometry&gt; 元素，返回其包含的所有三角面
        ''' </summary>
        Private Function ParseGeometry(geomElem As XElement) As Data.Surface()
            Dim geomId As String = geomElem.Attribute("id")?.Value
            Dim meshElem As XElement = geomElem.Element(COLLADA_NS + "mesh")
            If meshElem Is Nothing Then Return Nothing

            ' 1. 解析所有 <source> 元素
            Dim sources As New Dictionary(Of String, GeometrySource)
            For Each srcElem As XElement In meshElem.Elements(COLLADA_NS + "source")
                ParseSource(srcElem, sources)
            Next

            ' 2. 解析 <vertices> 元素以确定位置和法向量的 source 引用
            Dim verticesElem As XElement = meshElem.Element(COLLADA_NS + "vertices")
            Dim positionSourceId As String = Nothing
            Dim normalSourceId As String = Nothing

            If verticesElem IsNot Nothing Then
                For Each input As XElement In verticesElem.Elements(COLLADA_NS + "input")
                    Dim semantic = input.Attribute("semantic")?.Value
                    Dim sourceRef = input.Attribute("source")?.Value?.TrimStart("#"c)
                    If semantic = "POSITION" Then
                        positionSourceId = sourceRef
                    ElseIf semantic = "NORMAL" Then
                        normalSourceId = sourceRef
                    End If
                Next
            End If

            ' 3. 获取位置和法向量数据
            Dim posSource As GeometrySource = Nothing
            Dim normSource As GeometrySource = Nothing
            If positionSourceId IsNot Nothing Then
                sources.TryGetValue(positionSourceId, posSource)
            End If
            If normalSourceId IsNot Nothing Then
                sources.TryGetValue(normalSourceId, normSource)
            End If

            If posSource Is Nothing OrElse posSource.floatArray Is Nothing Then
                Return Nothing
            End If

            ' 4. 解析 <triangles> 元素
            Dim surfaces As New List(Of Data.Surface)
            Dim trianglesElems = meshElem.Elements(COLLADA_NS + "triangles")

            For Each triElem As XElement In trianglesElems
                Dim material As String = triElem.Attribute("material")?.Value
                Dim pElem As XElement = triElem.Element(COLLADA_NS + "p")
                If pElem Is Nothing Then Continue For

                Dim indicesText As String = pElem.Value.Trim
                If String.IsNullOrEmpty(indicesText) Then Continue For

                Dim indices As Integer() = indicesText _
                    .Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries) _
                    .Select(Function(s) CInt(s)) _
                    .ToArray

                If indices.Length < 3 Then Continue For

                ' 将索引构建为三角面（每3个索引一组）
                Dim stride As Integer = posSource.stride
                For i As Integer = 0 To indices.Length - 3 Step 3
                    Dim v1 As Integer = indices(i)
                    Dim v2 As Integer = indices(i + 1)
                    Dim v3 As Integer = indices(i + 2)

                    ' 从扁平数组中取出顶点坐标 (stride = 3)
                    Dim base1 As Integer = v1 * stride
                    Dim base2 As Integer = v2 * stride
                    Dim base3 As Integer = v3 * stride

                    If base1 + stride <= posSource.floatArray.Length AndAlso
                       base2 + stride <= posSource.floatArray.Length AndAlso
                       base3 + stride <= posSource.floatArray.Length Then

                        surfaces.Add(New Data.Surface With {
                            .vertices = {
                                New Data.Vertex(New Point3D(
                                    posSource.floatArray(base1),
                                    posSource.floatArray(base1 + 1),
                                    posSource.floatArray(base1 + 2))),
                                New Data.Vertex(New Point3D(
                                    posSource.floatArray(base2),
                                    posSource.floatArray(base2 + 1),
                                    posSource.floatArray(base2 + 2))),
                                New Data.Vertex(New Point3D(
                                    posSource.floatArray(base3),
                                    posSource.floatArray(base3 + 1),
                                    posSource.floatArray(base3 + 2)))
                            },
                            .paint = If(String.IsNullOrEmpty(material), "#A0A0A0", material)
                        })
                    End If
                Next
            Next

            Return surfaces.ToArray
        End Function

        ''' <summary>
        ''' 解析单个 &lt;source&gt; 元素，提取 float_array 数据
        ''' </summary>
        Private Sub ParseSource(srcElem As XElement, ByRef sources As Dictionary(Of String, GeometrySource))
            Dim srcId As String = srcElem.Attribute("id")?.Value
            If String.IsNullOrEmpty(srcId) Then Return

            Dim floatArrayElem As XElement = srcElem.Element(COLLADA_NS + "float_array")
            If floatArrayElem Is Nothing Then Return

            Dim countStr As String = floatArrayElem.Attribute("count")?.Value
            Dim count As Integer = 0
            If Not Integer.TryParse(countStr, count) OrElse count <= 0 Then Return

            Dim floatText As String = floatArrayElem.Value.Trim
            If String.IsNullOrEmpty(floatText) Then Return

            Dim values As Single() = floatText _
                .Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries) _
                .Select(Function(s) CSng(s)) _
                .ToArray

            ' 从 <accessor> 获取 stride
            Dim stride As Integer = 3  ' 默认为3 (xyz)
            Dim techniqueElem As XElement = srcElem.Element(COLLADA_NS + "technique_common")
            If techniqueElem IsNot Nothing Then
                Dim accessorElem As XElement = techniqueElem.Element(COLLADA_NS + "accessor")
                If accessorElem IsNot Nothing Then
                    Dim strideStr As String = accessorElem.Attribute("stride")?.Value
                    If Not String.IsNullOrEmpty(strideStr) Then
                        Integer.TryParse(strideStr, stride)
                    End If
                End If
            End If

            sources(srcId) = New GeometrySource With {
                .sourceId = srcId,
                .floatArray = values,
                .stride = stride,
                .count = count
            }
        End Sub

    End Module
End Namespace
