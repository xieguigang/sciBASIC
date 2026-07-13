#Region "Microsoft.VisualBasic::d4e5b1c2f6a4d8c9b3g7e4d5f6a7c8, gr\Landscape\Stl\StlAsciiReader.vb"

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

    '     Module StlParser
    ' 
    '         Function: ParseAsciiSTL, ParseNormalFromFacet, ParseVertex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Stl

    Partial Public Module StlParser

        ''' <summary>
        ''' 解析 ASCII 格式的 STL 文件
        ''' 
        ''' ASCII STL 格式：
        ''' solid &lt;name&gt;
        '''   facet normal ni nj nk
        '''     outer loop
        '''       vertex v1x v1y v1z
        '''       vertex v2x v2y v2z
        '''       vertex v3x v3y v3z
        '''     endloop
        '''   endfacet
        ''' endsolid &lt;name&gt;
        ''' </summary>
        ''' <param name="reader"></param>
        ''' <returns></returns>
        Public Function ParseAsciiSTL(reader As StreamReader) As Data.Surface()
            Dim surfaces As New List(Of Data.Surface)
            Dim line As String
            Dim currentNormal As Point3D = Nothing
            Dim vertices As New List(Of Point3D)
            Dim isReadingFacet As Boolean = False

            Do While Not reader.EndOfStream
                line = reader.ReadLine()
                If line Is Nothing Then Exit Do

                line = line.Trim()
                If line.Length = 0 Then Continue Do

                ' 忽略注释行
                If line.StartsWith("#"c) OrElse line.StartsWith("'"c) Then Continue Do

                Dim upperLine As String = line.ToUpperInvariant()

                If upperLine.StartsWith("SOLID") Then
                    ' 开始新的 solid 块
                    ' 如果之前有未完成的 facet，跳过
                    vertices.Clear()
                    isReadingFacet = False
                ElseIf upperLine.StartsWith("FACET") AndAlso upperLine.Contains("NORMAL") Then
                    ' 开始新的三角面：facet normal nx ny nz
                    isReadingFacet = True
                    vertices.Clear()
                    currentNormal = ParseNormalFromFacet(line)
                ElseIf upperLine.StartsWith("ENDSOLID") Then
                    ' solid 块结束
                    Exit Do
                ElseIf upperLine.StartsWith("OUTER LOOP") Then
                    ' 开始顶点循环，等待顶点数据
                    Continue Do
                ElseIf upperLine.StartsWith("ENDLOOP") Then
                    ' outer loop 结束
                    Continue Do
                ElseIf upperLine.StartsWith("ENDFACET") Then
                    ' 三角面结束，保存面数据
                    If isReadingFacet AndAlso vertices.Count >= 3 Then
                        surfaces.Add(New Data.Surface With {
                            .vertices = {
                                New Data.Vertex(vertices(0)),
                                New Data.Vertex(vertices(1)),
                                New Data.Vertex(vertices(2))
                            },
                            .paint = "#C0C0C0"
                        })
                    End If
                    isReadingFacet = False
                    vertices.Clear()
                ElseIf upperLine.StartsWith("VERTEX") Then
                    ' 顶点数据：vertex x y z
                    Dim vertex As Point3D? = ParseVertex(line)
                    If vertex IsNot Nothing Then
                        vertices.Add(vertex)
                    End If
                End If
            Loop

            Return surfaces.ToArray
        End Function

        ''' <summary>
        ''' 从 "facet normal nx ny nz" 行解析法向量
        ''' </summary>
        Private Function ParseNormalFromFacet(line As String) As Point3D
            Dim parts As String() = line.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
            If parts.Length >= 5 Then
                Dim nx As Single, ny As Single, nz As Single
                If Single.TryParse(parts(2), nx) AndAlso
                   Single.TryParse(parts(3), ny) AndAlso
                   Single.TryParse(parts(4), nz) Then
                    Return New Point3D(nx, ny, nz)
                End If
            End If
            Return Nothing
        End Function

        ''' <summary>
        ''' 从 "vertex x y z" 行解析顶点坐标
        ''' </summary>
        Private Function ParseVertex(line As String) As Point3D?
            Dim parts As String() = line.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
            If parts.Length >= 4 Then
                Dim x As Single, y As Single, z As Single
                If Single.TryParse(parts(1), x) AndAlso
                   Single.TryParse(parts(2), y) AndAlso
                   Single.TryParse(parts(3), z) Then
                    Return New Point3D(x, y, z)
                End If
            End If
            Return Nothing
        End Function

    End Module
End Namespace
