#Region "Microsoft.VisualBasic::e5f6c2d3a7b4e9d0c4h8f5d6a7b8c9, gr\Landscape\Stl\StlBinaryReader.vb"

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
    '         Function: ParseBinarySTL
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Stl

    ' STL (Stereolithography) Binary 格式解析部分
    Public Partial Module StlParser

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
        Public Function ParseBinarySTL(reader As BinaryReader) As Data.Surface()
            ' 跳过 80 字节文件头
            reader.ReadBytes(80)

            ' 读取三角面总数
            Dim triangleCount As UInteger = reader.ReadUInt32()

            If triangleCount = 0 Then
                Return {}
            End If

            Dim surfaces As Data.Surface() = New Data.Surface(CInt(triangleCount) - 1) {}

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
                            New Data.Vertex(New Point3D(v1x, v1y, v1z)),
                            New Data.Vertex(New Point3D(v2x, v2y, v2z)),
                            New Data.Vertex(New Point3D(v3x, v3y, v3z))
                        },
                        .paint = "#C0C0C0"
                    }
                Catch ex As EndOfStreamException
                    ' 文件提前结束，截断数组
                    Dim truncated As Data.Surface() = New Data.Surface(i - 1) {}
                    Array.Copy(surfaces, truncated, i)
                    Return truncated
                End Try
            Next

            Return surfaces
        End Function

    End Module
End Namespace
