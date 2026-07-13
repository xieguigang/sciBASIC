#Region "Microsoft.VisualBasic::ca366eb6c99f2d6e4c850ef5a7343618, gr\Landscape\Wavefront\ObjTextParser.vb"

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

    '   Total Lines: 158
    '    Code Lines: 112 (70.89%)
    ' Comment Lines: 31 (19.62%)
    '    - Xml Docs: 61.29%
    ' 
    '   Blank Lines: 15 (9.49%)
    '     File Size: 7.18 KB


    '     Module ObjTextParser
    ' 
    '         Function: CreateObjGroup, ParseFile, ParseVertex
    ' 
    '         Sub: ParseFace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Namespace Wavefront

    ''' <summary>
    ''' OBJ (Wavefront) 文件文本解析器
    ''' </summary>
    Public Module ObjTextParser

        ''' <summary>
        ''' 解析 OBJ 文件，使用 g/o 指令作为 ObjGroup 分隔边界
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ParseFile(buf As StreamReader) As ObjModel
            Dim line As New Value(Of String)
            Dim comments As New StringBuilder
            Dim mtllib As String = Nothing
            Dim parts As New List(Of ObjGroup)
            Dim vertex As New List(Of Point3D)
            Dim vn As New List(Of Point3D)
            Dim currentGroup As String = Nothing
            Dim usemtl As String = Nothing
            Dim f As New List(Of Triangle)

            Do While Not (line = buf.ReadLine) Is Nothing
                If line.First = "#"c Then
                    comments.AppendLine(line.Trim("#"c, " "c))
                ElseIf Not line.Value.StringEmpty Then

                    If line.StartsWith("mtllib") Then
                        mtllib = Mid(line.Value, "mtllib".Length + 1).Trim
                    Else
                        Dim tokens As String() = line.Split

                        Select Case tokens(0)
                            Case "v"
                                Call vertex.Add(ParseVertex(tokens))
                            Case "vn"
                                Call vn.Add(ParseVertex(tokens))
                            Case "g", "o"
                                ' g/o 指令作为新的 ObjGroup 分隔边界
                                If vertex.Count > 0 OrElse f.Count > 0 Then
                                    Dim part = CreateObjGroup(vertex, vn, currentGroup, usemtl, f)
                                    If Not part.IsEmpty Then
                                        parts.Add(part)
                                    End If
                                    vertex.Clear()
                                    vn.Clear()
                                    f.Clear()
                                End If
                                currentGroup = tokens.Skip(1).JoinBy(" ")
                                usemtl = Nothing
                            Case "usemtl"
                                usemtl = tokens.Skip(1).JoinBy(" ")
                            Case "f"
                                ' 解析面数据（支持三角面、四边形及更多顶点的多边形）
                                ParseFace(tokens, f)
                            Case "s", "l"
                                ' smoothing group / line — 暂不处理，但不应抛异常
                            Case Else
                                ' 忽略未知指令，而非中断整个文件读取
                        End Select
                    End If
                End If
                ' 空行忽略，不作为分隔条件
            Loop

            ' 处理最后一个 ObjGroup 的剩余数据
            If vertex.Count > 0 OrElse f.Count > 0 Then
                Dim part = CreateObjGroup(vertex, vn, currentGroup, usemtl, f)
                If Not part.IsEmpty Then
                    parts.Add(part)
                End If
            End If

            Return New ObjModel With {
                .comment = comments.ToString,
                .mtllib = mtllib,
                .parts = parts.ToArray
            }
        End Function

        ''' <summary>
        ''' 解析单个顶点行（v 或 vn 指令），使用 CSng 保证浮点精度
        ''' </summary>
        Private Function ParseVertex(tokens As String()) As Point3D
            Dim x As Single = CSng(tokens(1))
            Dim y As Single = CSng(tokens(2))
            Dim z As Single = CSng(tokens(3))
            Return New Point3D(x, y, z)
        End Function

        ''' <summary>
        ''' 解析面（f 指令）数据。支持三角面（3顶点）、四边形（4顶点）及更多顶点的多边形。
        ''' 对于多于3个顶点的多边形，使用三角扇（fan triangulation）分解为多个三角面。
        ''' 兼容 "v"、"v/vt"、"v/vt/vn"、"v//vn" 等 OBJ 面索引格式。
        ''' </summary>
        Private Sub ParseFace(tokens As String(), faces As List(Of Triangle))
            ' tokens(0) = "f", 后续元素为顶点索引组
            Dim faceVertices As Integer = tokens.Length - 1

            If faceVertices < 3 Then
                ' 少于3个顶点不是有效面，跳过
                Return
            End If

            ' 解析所有顶点索引
            Dim parsedIndices As (v As Integer, vn As Integer)() =
                tokens.Skip(1).Select(Function(token)
                                          Dim parts = token.Split("/"c)
                                          Dim vIndex As Integer = std.Abs(CInt(Val(parts(0))))
                                          ' 处理 vn 索引，兼容 v、v/vt、v/vt/vn、v//vn 格式
                                          Dim vnIndex As Integer = 0
                                          If parts.Length >= 3 AndAlso Not String.IsNullOrEmpty(parts(2)) Then
                                              vnIndex = std.Abs(CInt(Val(parts(2))))
                                          End If
                                          Return (v:=vIndex, vn:=vnIndex)
                                      End Function).ToArray

            ' 三角扇分解：对于 N 个顶点的面，产生 N-2 个三角面
            ' 以第一个顶点为扇心，依次取 (v0, v1, v2), (v0, v2, v3), ...
            For i As Integer = 0 To faceVertices - 3
                faces.Add(New Triangle With {
                    .v3 = {parsedIndices(0).v, parsedIndices(i + 1).v, parsedIndices(i + 2).v},
                    .vn3 = {parsedIndices(0).vn, parsedIndices(i + 1).vn, parsedIndices(i + 2).vn},
                    .comment = Nothing
                })
            Next
        End Sub

        ''' <summary>
        ''' 从当前解析缓冲区创建 ObjGroup
        ''' </summary>
        Private Function CreateObjGroup(vertex As List(Of Point3D),
                                          vn As List(Of Point3D),
                                          g As String,
                                          usemtl As String,
                                          f As List(Of Triangle)) As ObjGroup
            Return New ObjGroup With {
                .vertex = vertex.ToArray,
                .vn = vn.ToArray,
                .g = g,
                .usemtl = usemtl,
                .f = f.ToArray
            }
        End Function

    End Module
End Namespace
