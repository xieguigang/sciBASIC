#Region "Microsoft.VisualBasic::5ebc6928209db7e800b3868f87167b9c, ..\sciBASIC#\Data\BinaryData\BinaryData\Extensions\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Text

Public Module Extensions

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <param name="buffered%">
    ''' 默认的缓存临界大小是10MB，当超过这个大小的时候则不会进行缓存，假若没有操作这个临界值，则程序会一次性读取所有数据到内存之中以提高IO性能
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function OpenBinaryReader(path As String, Optional encoding As Encodings = Encodings.ASCII, Optional buffered& = 1024 * 1024 * 10) As BinaryDataReader
        If FileIO.FileSystem.GetFileInfo(path).Length <= buffered Then
            Dim byts As Byte() = FileIO.FileSystem.ReadAllBytes(path)   ' 文件数据将会被缓存
            Dim ms As New MemoryStream(byts)
            Return New BinaryDataReader(ms, encoding)
        Else
            Return New BinaryDataReader(File.OpenRead(path), encoding)
        End If
    End Function
End Module
