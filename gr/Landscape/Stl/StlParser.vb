#Region "Microsoft.VisualBasic::STL_Parser, gr\Landscape\Stl\StlParser.vb"

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

    '     Module StlParser
    ' 
    '         Function: IsAsciiSTL, ParseSTL
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

Namespace Stl

    ''' <summary>
    ''' STL (Stereolithography) 三角网格三维模型文件解析器
    ''' 
    ''' 支持 ASCII 文本格式和 Binary 二进制格式的 STL 文件读取，
    ''' 自动检测文件格式并将数据转换为统一的 Surface 数组。
    ''' 具体的 ASCII / Binary 解析逻辑分别在 StlAsciiReader 与
    ''' StlBinaryReader 这两个 Partial Module 中实现。
    ''' </summary>
    Partial Public Module StlParser

        ''' <summary>
        ''' 从文件路径自动检测格式并解析 STL 模型
        ''' </summary>
        ''' <param name="filePath$">.stl 文件路径</param>
        ''' <returns>解析后的 SceneModel</returns>
        <Extension>
        Public Function ParseSTL(filePath$) As Data.SceneModel
            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Return ParseSTL(fs)
            End Using
        End Function

        ''' <summary>
        ''' 从 Stream 自动检测格式并解析 STL 模型
        ''' </summary>
        ''' <param name="stream">包含 STL 数据的流</param>
        ''' <returns>解析后的 SceneModel</returns>
        <Extension>
        Public Function ParseSTL(stream As Stream) As Data.SceneModel
            ' 读取前5个字节判断是否为 ASCII ("solid")
            Dim header As Byte() = New Byte(4) {}
            Dim bytesRead As Integer = stream.Read(header, 0, 5)
            stream.Seek(0, SeekOrigin.Begin)

            Dim surfaces As Data.Surface()

            If bytesRead = 5 AndAlso IsAsciiSTL(header) Then
                Using reader As New StreamReader(stream, Encoding.ASCII, detectEncodingFromByteOrderMarks:=False, bufferSize:=4096, leaveOpen:=True)
                    surfaces = ParseAsciiSTL(reader)
                End Using
            Else
                Using reader As New BinaryReader(stream, Encoding.ASCII, leaveOpen:=True)
                    surfaces = ParseBinarySTL(reader)
                End Using
            End If

            Return New Data.SceneModel With {
                .Surfaces = surfaces
            }
        End Function

        ''' <summary>
        ''' 判断文件头是否为 ASCII STL 格式（以 "solid" 开头）
        ''' </summary>
        Private Function IsAsciiSTL(header As Byte()) As Boolean
            ' "solid" = 115, 111, 108, 105, 100
            Return header(0) = 115 AndAlso header(1) = 111 AndAlso
                   header(2) = 108 AndAlso header(3) = 105 AndAlso
                   header(4) = 100
        End Function

    End Module
End Namespace
