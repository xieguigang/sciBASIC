#Region "Microsoft.VisualBasic::5021cb21334bb902d6d727c27963d906, Data\BinaryData\HDSPack\BinaryStream\TreeParser.vb"

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

    '   Total Lines: 85
    '    Code Lines: 60
    ' Comment Lines: 14
    '   Blank Lines: 11
    '     File Size: 3.20 KB


    ' Module TreeParser
    ' 
    '     Function: getCurrentDirectory, getCurrentFile, Parse
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.FileIO.Path
Imports Microsoft.VisualBasic.Net.Http

Friend Module TreeParser

    ''' <summary>
    ''' header tree data is compressed in gzip
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <param name="registry"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' the attribute data in the tree element has 
    ''' already been unpack from the raw data 
    ''' buffer.
    ''' </remarks>
    Public Function Parse(buffer As Stream, registry As Dictionary(Of String, String)) As StreamGroup
        Dim size As Integer
        Dim bin As New BinaryDataReader(buffer) With {.ByteOrder = ByteOrder.BigEndian}
        Dim root As StreamGroup
        Dim rawStream As Stream

        size = bin.ReadInt32
        rawStream = New SubStream(buffer, buffer.Position, size)
        ' decomparession via gzip
        rawStream = rawStream.UnGzipStream

        If DirectCast(rawStream, MemoryStream).Length = 0 Then
            root = StreamGroup.CreateRootTree
        Else
            bin = New BinaryDataReader(rawStream) With {.ByteOrder = ByteOrder.BigEndian}
            root = bin.getCurrentDirectory(registry)
        End If

        Return root
    End Function

    <Extension>
    Private Function getCurrentDirectory(bin As BinaryDataReader, registry As Dictionary(Of String, String)) As StreamGroup
        Dim nfiles As Integer = bin.ReadInt32
        Dim path As String = bin.ReadString(BinaryStringFormat.ZeroTerminated)
        Dim tree As New Dictionary(Of String, StreamObject)
        Dim attrSize As Integer = bin.ReadInt32
        Dim attrBuf As Byte() = bin.ReadBytes(attrSize)

        For i As Integer = 1 To nfiles
            Dim flag As Integer = bin.ReadInt32
            Dim obj As StreamObject

            If flag = 0 Then
                ' is file
                obj = bin.getCurrentFile(registry)
            Else
                ' is folder
                obj = bin.getCurrentDirectory(registry)
            End If

            Call tree.Add(obj.fileName, obj)
        Next

        Dim dir As New StreamGroup(New FilePath(path), tree)
        dir.attributes = New MemoryStream(attrBuf).UnPack(dir.description, registry)
        Return dir
    End Function

    <Extension>
    Private Function getCurrentFile(bin As BinaryDataReader, registry As Dictionary(Of String, String)) As StreamBlock
        Dim name As String = bin.ReadString(BinaryStringFormat.ZeroTerminated)
        Dim offset As Long = bin.ReadInt64
        Dim size As Long = bin.ReadInt64
        Dim attrSize As Integer = bin.ReadInt32
        Dim attrBuf As Byte() = bin.ReadBytes(attrSize)
        Dim file As New StreamBlock(New FilePath(name)) With {
            .offset = offset,
            .size = size
        }
        file.attributes = New MemoryStream(attrBuf).UnPack(file.description, registry)
        Return file
    End Function
End Module
