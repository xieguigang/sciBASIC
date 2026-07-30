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
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO
Imports std = System.Math

''' <summary>
''' Reader for the classic MSF (multi-stream file) PDB container, produced by
''' Visual C++ / .NET Framework builds. The file is organised as a fixed-size
''' page store; the header (SuperBlock) describes the page size and the location
''' of the stream directory.
''' </summary>
Public Class MSFReader : Implements IDisposable

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
    Private _pageSize As Integer
    Public ReadOnly Property PageSize As Integer
        Get
            Return _pageSize
        End Get
    End Property

    ''' <summary>
    ''' Total number of pages in the file.
    ''' </summary>
    Private _numPages As Integer
    Public ReadOnly Property NumPages As Integer
        Get
            Return _numPages
        End Get
    End Property

    ''' <summary>
    ''' Number of pages marked free (unallocated) in the free-page-map.
    ''' </summary>
    Public ReadOnly Property FreePageCount As Integer
        Get
            Return _freePageCount
        End Get
    End Property

    ''' <summary>
    ''' Size in bytes of the stream directory (from the SuperBlock).
    ''' </summary>
    Private dirBytes As Integer

    ''' <summary>
    ''' Block number of the first free-page-map (FPM0).
    ''' </summary>
    Private freePageMapAddr As Integer

    ''' <summary>
    ''' Raw bytes of the FPM0 page (1 bit per page; 1 = free / unallocated).
    ''' </summary>
    Private freePageMap As Byte()

    ''' <summary>
    ''' Number of pages marked free in <see cref="freePageMap"/>.
    ''' </summary>
    Private _freePageCount As Integer

    ''' <summary>
    ''' Named-stream map decoded from the PDB stream (stream #1), name -> stream index.
    ''' </summary>
    Private ReadOnly _namedStreams As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>
    ''' GUID / signature / age parsed from the PDB stream.
    ''' </summary>
    Public ReadOnly Property PdbInfo As PdbStreamInfo

    Private ReadOnly file As FileStream
    Private ReadOnly streams As New List(Of Stream)()

    Sub New(filePath As String)
        file = New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
        ReadSuperBlock()

        If _pageSize <= 0 Then
            Throw New InvalidDataException("Invalid MSF page size.")
        End If

        ReadFreePageMap()

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

                Dim magic As Byte() = Encoding.ASCII.GetBytes(MSFReader.Magic)
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

    ''' <summary>
    ''' Read the SuperBlock and populate the page-size, free-page-map block index,
    ''' page count, directory size and directory block-map address.
    ''' </summary>
    Private Sub ReadSuperBlock()
        ' SuperBlock layout:
        '   char  Magic[44];          offset 0
        '   u32   PageSize;           offset 44
        '   u32   FreePageMapBlock;   offset 48  (block# of FPM0)
        '   u32   NumPages;           offset 52
        '   u32   NumDirectoryBytes;  offset 56
        '   u32   Reserved;           offset 60
        '   u32   BlockMapAddr;       offset 64
        file.Seek(44, SeekOrigin.Begin)

        Using br As New BinaryDataReader(file, leaveOpen:=True) With {.ByteOrder = ByteOrder.LittleEndian}
            _pageSize = br.ReadInt32()           ' BlockSize           @44
            freePageMapAddr = br.ReadInt32()     ' FreePageMapBlock    @48
            _numPages = br.ReadInt32()           ' NumPages            @52
            dirBytes = br.ReadInt32()            ' NumDirectoryBytes   @56
            br.ReadInt32()                       ' Reserved            @60
            blockMapAddr = br.ReadInt32()        ' BlockMapAddr        @64
        End Using
    End Sub

    ''' <summary>
    ''' Read the first free-page-map (FPM0) page and count the free pages.
    ''' The FPM is a bit array (1 bit per page); bit set = page is free / unallocated.
    ''' </summary>
    Private Sub ReadFreePageMap()
        _freePageCount = 0
        freePageMap = Nothing

        If freePageMapAddr < 0 Then
            Return
        End If

        file.Seek(CLng(freePageMapAddr) * _pageSize, SeekOrigin.Begin)
        freePageMap = New Byte(_pageSize - 1) {}
        file.Read(freePageMap, 0, _pageSize)

        For i As Integer = 0 To std.Min(_numPages, freePageMap.Length * 8) - 1
            If IsPageFree(i) Then
                _freePageCount += 1
            End If
        Next
    End Sub

    ''' <summary>
    ''' Returns True when the given page is marked free (unallocated) in the FPM.
    ''' </summary>
    Public Function IsPageFree(page As Integer) As Boolean
        If freePageMap Is Nothing OrElse page < 0 Then
            Return False
        End If

        Dim byteIndex As Integer = page \ 8

        If byteIndex >= freePageMap.Length Then
            Return False
        End If

        Return (freePageMap(byteIndex) And CByte(1 << (page Mod 8))) <> 0
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

        Using br As New BinaryDataReader(file, leaveOpen:=True) With {.ByteOrder = ByteOrder.LittleEndian}
            For i As Integer = 0 To dirPageCount - 1
                dirPages(i) = br.ReadInt32()
            Next
        End Using

        Dim buffer As Byte() = New Byte(dirBytes - 1) {}
        Dim offset As Integer = 0

        For Each p As Integer In dirPages
            file.Seek(CLng(p) * PageSize, SeekOrigin.Begin)
            Dim chunk As Integer = std.Min(PageSize, buffer.Length - offset)
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
            Using br As New BinaryDataReader(ms, leaveOpen:=False) With {.ByteOrder = ByteOrder.LittleEndian}
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
    ''' Get a stream by its name using the named-stream map decoded from the PDB stream.
    ''' </summary>
    Public Function GetStreamByName(name As String) As Stream
        Dim index As Integer

        If NamedStreams.TryGetValue(name, index) Then
            Return GetStream(index)
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' The named-stream map (stream name -> stream index) decoded from the PDB stream.
    ''' </summary>
    Public ReadOnly Property NamedStreams As IReadOnlyDictionary(Of String, Integer)
        Get
            Return _namedStreams
        End Get
    End Property

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

            ' After the GUID follows a named-stream map:
            '   u32 NamesSize;                       size in bytes of the table below
            '   repeated until NamesSize consumed:
            '     u32 StreamIndex;
            '     u32 NameLenInBytes;
            '     byte Name[NameLenInBytes];         (UTF-8, not null terminated)
            If reader.BaseStream.Position + 4 <= reader.BaseStream.Length Then
                Dim namesSize As Integer = reader.ReadInt32()

                If namesSize > 0 AndAlso namesSize <= reader.BaseStream.Length - reader.BaseStream.Position Then
                    Dim nameBlock As Byte() = reader.ReadBytes(namesSize)
                    Dim i As Integer = 0

                    While i + 8 <= namesSize
                        Dim streamIndex As Integer = BitConverter.ToInt32(nameBlock, i)
                        Dim nameLen As Integer = BitConverter.ToInt32(nameBlock, i + 4)
                        i += 8

                        If i + nameLen > namesSize Then
                            Exit While
                        End If

                        Dim name As String = Encoding.UTF8.GetString(nameBlock, i, nameLen).TrimEnd(ControlChars.NullChar)
                        i += nameLen

                        If name.Length > 0 AndAlso Not NamedStreams.ContainsKey(name) Then
                            _namedStreams(name) = streamIndex
                        End If
                    End While
                End If
            End If
        End Using

        Return info
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If file IsNot Nothing Then
            file.Dispose()
        End If
    End Sub
End Class



