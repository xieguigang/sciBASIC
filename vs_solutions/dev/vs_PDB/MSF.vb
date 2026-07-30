#Region "Microsoft.VisualBasic::00000000000000000000000000000000, sciBASIC#\vs_solutions\dev\vs_PDB\MSF.vb"

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

Namespace sciBASIC.PDB

    ''' <summary>
    ''' Reader for the classic MSF (multi-stream file) PDB container, produced by
    ''' Visual C++ / .NET Framework builds. The file is organised as a fixed-size
    ''' page store; the header (SuperBlock) describes the page size and the location
    ''' of the stream directory.
    ''' </summary>
    Public Class MSFReader
        Implements IDisposable

        ''' <summary>
        ''' Canonical magic prefix of a classic PDB file
        ''' ("Microsoft C/C++ MSF 7.00\r\n\x1aDS").
        ''' </summary>
        Public Const Magic As String = "Microsoft C/C++ MSF 7.00" & vbCrLf & vbNullChar & "DS" & vbNullChar & vbNullChar & vbNullChar

        Public Const StreamPdb As Integer = 1
        Public Const StreamTpi As Integer = 2
        Public Const StreamDbi As Integer = 3

        ''' <summary>
        ''' Page size of the MSF file.
        ''' </summary>
        Public ReadOnly Property PageSize As Integer

        ''' <summary>
        ''' Total number of pages in the file.
        ''' </summary>
        Public ReadOnly Property NumPages As Integer

        ''' <summary>
        ''' Size in bytes of the stream directory (from the SuperBlock).
        ''' </summary>
        Private dirBytes As Integer

        ''' <summary>
        ''' GUID / signature / age parsed from the PDB stream.
        ''' </summary>
        Public ReadOnly Property PdbInfo As PdbStreamInfo

        Private ReadOnly file As FileStream
        Private ReadOnly streams As New List(Of Stream)()

        Sub New(filePath As String)
            file = New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            PageSize = ReadPageSize()

            If PageSize <= 0 Then
                Throw New InvalidDataException("Invalid MSF page size.")
            End If

            Dim directory As Byte() = ReadDirectory()
            ParseStreamTable(directory)
            PdbInfo = ParsePdbStream()
        End Sub

        ''' <summary>
        ''' Detect whether the given file starts with the MSF magic.
        ''' </summary>
        Public Shared Function IsMagic(filePath As String) As Boolean
            Try
                Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    Dim head As Byte() = New Byte(31) {}
                    If fs.Read(head, 0, head.Length) < head.Length Then
                        Return False
                    End If

                    Dim magic As Byte() = Text.Encoding.ASCII.GetBytes(Magic)
                    For i As Integer = 0 To magic.Length - 1
                        If head(i) <> magic(i) Then
                            Return False
                        End If
                    Next

                    Return True
                End Using
            Catch
                Return False
            End Try
        End Function

        Private Function ReadPageSize() As Integer
            ' SuperBlock layout:
            '   char  Magic[44];
            '   u32   PageSize;          offset 44
            '   u32   FreePageMap;       offset 48
            '   u32   NumPages;          offset 52
            '   u32   NumDirectoryBytes; offset 56
            '   u32   Reserved;          offset 60
            '   u32   BlockMapAddr;      offset 64
            file.Seek(44, SeekOrigin.Begin)
            Using br As New BinaryDataReader(file, ByteOrder.LittleEndian, leaveOpen:=True)
                Dim pageSize As Integer = br.ReadInt32()
                _NumPages = br.ReadInt32()
                dirBytes = br.ReadInt32() ' directory bytes
                _ = br.ReadInt32() ' reserved
                blockMapAddr = br.ReadInt32()
            End Using

            Return PageSize
        End Function

        Private blockMapAddr As Integer

        ''' <summary>
        ''' Read the stream directory: a list of page numbers (stored in the page at
        ''' <see cref="blockMapAddr"/>) that themselves hold the directory content.
        ''' </summary>
        Private Function ReadDirectory() As Byte()
            file.Seek(CLng(blockMapAddr) * PageSize, SeekOrigin.Begin)

            Dim dirPageCount As Integer = (dirBytes + PageSize - 1) \ PageSize
            Dim dirPages As Integer() = New Integer(dirPageCount - 1) {}

            Using br As New BinaryDataReader(file, ByteOrder.LittleEndian, leaveOpen:=True)
                For i As Integer = 0 To dirPageCount - 1
                    dirPages(i) = br.ReadInt32()
                Next
            End Using

            Dim buffer As Byte() = New Byte(dirBytes - 1) {}
            Dim offset As Integer = 0

            For Each p As Integer In dirPages
                file.Seek(CLng(p) * PageSize, SeekOrigin.Begin)
                Dim chunk As Integer = Math.Min(PageSize, buffer.Length - offset)
                file.Read(buffer, offset, chunk)
                offset += chunk
            Next

            Return buffer
        End Function

        ''' <summary>
        ''' Parse the stream table: number of streams, their sizes, and the page lists.
        ''' </summary>
        Private Sub ParseStreamTable(directory As Byte())
            Using ms As New MemoryStream(directory)
                Using br As New BinaryDataReader(ms, ByteOrder.LittleEndian, leaveOpen:=False)
                    Dim numStreams As Integer = br.ReadInt32()
                    Dim sizes As Integer() = New Integer(numStreams - 1) {}

                    For i As Integer = 0 To numStreams - 1
                        sizes(i) = br.ReadInt32()
                    Next

                    For i As Integer = 0 To numStreams - 1
                        If sizes(i) <= 0 Then
                            ' Stream not present (size 0 or -1).
                            streams.Add(Nothing)
                        Else
                            Dim numPages As Integer = (sizes(i) + PageSize - 1) \ PageSize
                            Dim pages As Integer() = New Integer(numPages - 1) {}

                            For p As Integer = 0 To numPages - 1
                                pages(p) = br.ReadInt32()
                            Next

                            streams.Add(New Stream(i, sizes(i), pages, PageSize, file))
                        End If
                    Next
                End Using
            End Using
        End Sub

        ''' <summary>
        ''' Get a parsed stream by its (0-based) index, or Nothing when not present.
        ''' </summary>
        Public Function GetStream(index As Integer) As Stream
            If index < 0 OrElse index >= streams.Count Then
                Return Nothing
            End If

            Return streams(index)
        End Function

        ''' <summary>
        ''' The PDB stream (stream #1) carries the GUID/signature/age and a named-stream
        ''' map. We only decode the header portion here.
        ''' </summary>
        Private Function ParsePdbStream() As PdbStreamInfo
            Dim info As New PdbStreamInfo()
            Dim s As Stream = GetStream(StreamPdb)

            If s Is Nothing Then
                Return info
            End If

            Using reader As BinaryDataReader = s.AsReader()
                info.Version = reader.ReadInt32()
                info.Signature = reader.ReadInt32()
                info.Age = reader.ReadInt32()
                info.Guid = New Guid(reader.ReadBytes(16))
            End Using

            Return info
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If file IsNot Nothing Then
                file.Dispose()
            End If
        End Sub
    End Class

    ''' <summary>
    ''' Header information decoded from the PDB stream (stream #1).
    ''' </summary>
    Public Class PdbStreamInfo
        Public Property Version As Integer
        Public Property Signature As Integer
        Public Property Age As Integer
        Public Property Guid As Guid

        Public Overrides Function ToString() As String
            Return $"v{Version} sig={Signature} age={Age} {Guid:B}"
        End Function
    End Class
End Namespace
