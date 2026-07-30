#Region "Microsoft.VisualBasic::50d78f84fe9741478a1f09ce125898b1, sciBASIC#\vs_solutions\dev\vs_PDB\Stream.vb"

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

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Binary

''' <summary>
''' Sub file for multiple stream pdb file.
''' 
''' Each stream in the PDB occupies several pages, which aren't necessarily consecutively numbered. 
''' The stream has a number and a length. The stream content is the concatenation of its pages, 
''' truncated to the stream's length.
''' </summary>
Public Class Stream

    ''' <summary>
    ''' The (0-based) index of this stream inside the PDB.
    ''' </summary>
    Public Property StreamNumber As Integer

    ''' <summary>
    ''' Logical length of the stream content in bytes (the page concatenation is truncated to this).
    ''' </summary>
    Public Property Length As Long

    ''' <summary>
    ''' The page numbers that make up this stream, in order. Page numbers are not necessarily consecutive.
    ''' </summary>
    Public Property PageList As Integer()

    ''' <summary>
    ''' Page size of the owning MSF file.
    ''' </summary>
    Friend Property PageSize As Integer

    ''' <summary>
    ''' The underlying MSF file stream, used to read page content on demand.
    ''' </summary>
    Friend Property File As FileStream

    ''' <summary>
    ''' Assemble the full stream content by concatenating its pages and truncating to <see cref="Length"/>.
    ''' The result is cached so repeated reads do not re-read the file.
    ''' </summary>
    Private _bytes As Byte() = Nothing

    ''' <summary>
    ''' Create a stream descriptor.
    ''' </summary>
    ''' <param name="streamNumber">0-based stream index.</param>
    ''' <param name="length">Logical stream length in bytes.</param>
    ''' <param name="pageList">Ordered page numbers composing the stream.</param>
    ''' <param name="pageSize">Page size of the owning MSF file.</param>
    ''' <param name="file">Opened MSF file stream (kept for lazy assembly).</param>
    Sub New(streamNumber As Integer, length As Long, pageList As Integer(), pageSize As Integer, file As FileStream)
        Me.StreamNumber = streamNumber
        Me.Length = length
        Me.PageList = pageList
        Me.PageSize = pageSize
        Me.File = file
    End Sub

    ''' <summary>
    ''' Assemble and return the full (truncated) stream content.
    ''' </summary>
    Public Function GetBytes() As Byte()
        If _bytes IsNot Nothing Then
            Return _bytes
        End If

        If PageList Is Nothing OrElse PageList.Length = 0 Then
            _bytes = New Byte(-1) {}
            Return _bytes
        End If

        Dim total As Long = CLng(PageList.Length) * PageSize
        Dim buffer As Byte() = New Byte(total - 1) {}
        Dim offset As Integer = 0

        SyncLock File
            For Each page As Integer In PageList
                File.Seek(CLng(page) * PageSize, SeekOrigin.Begin)
                Dim read As Integer = File.Read(buffer, offset, PageSize)

                ' A stream can reference fewer bytes than a full page; pad the remainder.
                If read < PageSize Then
                    Array.Clear(buffer, offset + read, PageSize - read)
                End If

                offset += PageSize
            Next
        End SyncLock

        ' Truncate to the logical stream length.
        If Length < total Then
            _bytes = New Byte(CInt(Length) - 1) {}
            Array.Copy(buffer, _bytes, CInt(Length))
        Else
            _bytes = buffer
        End If

        Return _bytes
    End Function

    ''' <summary>
    ''' Assemble the stream content and wrap it in a little-endian <see cref="BinaryDataReader"/>.
    ''' Remember to dispose the returned reader (which will not close the underlying file stream).
    ''' </summary>
    Public Function AsReader() As BinaryDataReader
        Return New BinaryDataReader(New MemoryStream(GetBytes()), ByteOrder.LittleEndian, leaveOpen:=True)
    End Function

    Public Overrides Function ToString() As String
        Return $"Stream#{StreamNumber} (length={Length}, pages={If(PageList?.Length, 0)})"
    End Function
End Class
