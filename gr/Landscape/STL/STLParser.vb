#Region "Microsoft.VisualBasic::STL_Parser, gr\Landscape\STL\STLParser.vb"

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

    '   Total Lines: 235
    '    Code Lines: 187 (79.57%)
    ' Comment Lines: 30 (12.77%)
    '    - Xml Docs: 86.67%
    ' 
    '   Blank Lines: 18 (7.66%)
    '     File Size: 9.50 KB


    '     Module STLParser
    ' 
    '         Function: IsAsciiSTL, ParseAsciiSTL, ParseBinarySTL, ParseSTL
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

Namespace STL

    ''' <summary>
    ''' STL (Stereolithography) 三角网格三维模型文件解析器
    ''' 
    ''' 支持 ASCII 文本格式和 Binary 二进制格式的 STL 文件读取，
    ''' 自动检测文件格式并将数据转换为统一的 Surface 数组。
    ''' </summary>
    Public Module STLParser

        ''' <summary>
        ''' 从文件路径自动检测格式并解析 STL 模型
        ''' </summary>
        ''' <param name="filePath$">.stl 文件路径</param>
        ''' <returns>解析后的 Surface 三角面数组</returns>
        Public Function ParseSTL(filePath$) As Data.Graphics
            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Return ParseSTL(fs)
            End Using
        End Function

        ''' <summary>
        ''' 从 Stream 自动检测格式并解析 STL 模型
        ''' </summary>
        ''' <param name="stream">包含 STL 数据的流</param>
        ''' <returns>解析后的 Surface 三角面数组</returns>
        Public Function ParseSTL(stream As Stream) As Data.Graphics
            ' 读取前5个字节判断是否为 ASCII ("solid")
            Dim header As Byte() = New Byte(4) {}
            Dim bytesRead As Integer = stream.Read(header, 0, 5)
            stream.Seek(0, SeekOrigin.Begin)

            Dim surfaces As Surface()

            If bytesRead = 5 AndAlso IsAsciiSTL(header) Then
                Using reader As New StreamReader(stream, Encoding.ASCII, detectEncodingFromByteOrderMarks:=False, bufferSize:=4096, leaveOpen:=True)
                    surfaces = ParseAsciiSTL(reader)
                End Using
            Else
                Using reader As New BinaryReader(stream, Encoding.ASCII, leaveOpen:=True)
                    surfaces = ParseBinarySTL(reader)
                End Using
            End If

            Return New Data.Graphics With {
                .Surfaces = surfaces
            }
        End Function

        ''' <summary>
        ''' 判断文件头是否为 ASCII STL 格式（以 "solid" 开头）
        ''' </summary>
        Private Function IsAsciiSTL(header As Byte()) As Boolean
            ' "solid" = 73, 111, 108, 105, 100
            Return header(0) = 115 AndAlso header(1) = 111 AndAlso
                   header(2) = 108 AndAlso header(3) = 105 AndAlso
                   header(4) = 100
        End Function

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
        Public Function ParseAsciiSTL(reader As StreamReader) As Surface()
            Dim surfaces As New List(Of Surface)
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
                                New Data.Vector(vertices(0)),
                                New Data.Vector(vertices(1)),
                                New Data.Vector(vertices(2))
                            },
                            .paint = "#C0C0C0"
                        })
                    End If
                    isReadingFacet = False
                    vertices.Clear()
                ElseIf upperLine.StartsWith("VERTEX") Then
                    ' 顶点数据：vertex x y z
                    Dim vertex As Point3D = ParseVertex(line)
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
        Private Function ParseVertex(line As String) As Point3D
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

        ''' <summary>
        ''' 解析 Binary 格式的 STL 文件
        ''' 
        ''' Binary STL 格式：
        ''' - 80 字节文件头（通常为注释或程序名）
        ''' - 4 字节 UInt32：三角面总数
        ''' - 每个三角面 50 字节：
        '''   - 12 字节（3×Float32）：法向量 (ni, nj, nk)
        '''   - 12 字节（3×Float32）：顶点1 (x1, y1, z1)
        '''   - 12 字节（3×Float32）：顶点2 (x2, y2, z2)
        '''   - 12 字节（3×Float32）：顶点3 (x3, y3, z3)
        '''   - 2 字节 UInt16：属性字节计数（通常为 0，忽略）
        ''' </summary>
        ''' <param name="reader"></param>
        ''' <returns></returns>
        Public Function ParseBinarySTL(reader As BinaryReader) As Surface()
            ' 跳过 80 字节文件头
            reader.ReadBytes(80)

            ' 读取三角面总数
            Dim triangleCount As UInteger = reader.ReadUInt32()

            If triangleCount = 0 Then
                Return {}
            End If

            Dim surfaces As Surface() = New Surface(CInt(triangleCount) - 1) {}

            For i As Integer = 0 To CInt(triangleCount) - 1
                Try
                    ' 读取法向量 (3 × Float32 = 12 bytes)
                    Dim nx As Single = reader.ReadSingle()
                    Dim ny As Single = reader.ReadSingle()
                    Dim nz As Single = reader.ReadSingle()

                    ' 读取顶点1
                    Dim v1x As Single = reader.ReadSingle()
                    Dim v1y As Single = reader.ReadSingle()
                    Dim v1z As Single = reader.ReadSingle()

                    ' 读取顶点2
                    Dim v2x As Single = reader.ReadSingle()
                    Dim v2y As Single = reader.ReadSingle()
                    Dim v2z As Single = reader.ReadSingle()

                    ' 读取顶点3
                    Dim v3x As Single = reader.ReadSingle()
                    Dim v3y As Single = reader.ReadSingle()
                    Dim v3z As Single = reader.ReadSingle()

                    ' 读取属性字节计数（2 字节，通常忽略）
                    reader.ReadUInt16()

                    surfaces(i) = New Data.Surface With {
                        .vertices = {
                            New Data.Vector(New Point3D(v1x, v1y, v1z)),
                            New Data.Vector(New Point3D(v2x, v2y, v2z)),
                            New Data.Vector(New Point3D(v3x, v3y, v3z))
                        },
                        .paint = "#C0C0C0"
                    }
                Catch ex As EndOfStreamException
                    ' 文件提前结束，截断数组
                    Dim truncated As Surface() = New Surface(i - 1) {}
                    Array.Copy(surfaces, truncated, i)
                    Return truncated
                End Try
            Next

            Return surfaces
        End Function

    End Module
End Namespace