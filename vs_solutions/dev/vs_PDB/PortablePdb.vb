#Region "Microsoft.VisualBasic::d4cb9ba949e0ae1bcadd5d707ceee283, vs_solutions\dev\vs_PDB\PortablePdb.vb"

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

    '   Total Lines: 870
    '    Code Lines: 657 (75.52%)
    ' Comment Lines: 54 (6.21%)
    '    - Xml Docs: 40.74%
    ' 
    '   Blank Lines: 159 (18.28%)
    '     File Size: 35.91 KB


    ' Class PortablePdbReader
    ' 
    '     Properties: Documents, LineNumbers
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: FindMetadataInPe, ReadBlobBytes, ReadBlobString, ReadCompressed, ReadCompressedSigned
    '               ReadGuid, ReadHeapIndex, ReadPdbBlob, ReadPdbString, RvaToOffset
    ' 
    '     Sub: DecodeDocuments, DecodeMethodDebugInfo, DecodeSequencePoints, Parse, ParsePdbStreamHeader
    '          ParseStreamHeaders, ParseTablesStream
    '     Structure SeqPoint
    ' 
    ' 
    ' 
    '     Enum ColKind
    ' 
    ' 
    ' 
    ' 
    '     Structure Col
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: C, CodedIndexSize, CodedTagBits, CodedTagTables, RowSize
    ' 
    '     Sub: (+2 Overloads) Dispose, InitSchemas
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.Models
Imports std = System.Math

''' <summary>
''' Reader for Portable PDB files. A Portable PDB is a PE file (starting with the DOS
''' <c>MZ</c> header) that carries the CLI metadata in the <c>#~</c> table stream together with
''' the custom <c>#Pdb</c> stream. This reader walks the PE / COR20 / metadata root, locates the
''' Document (0x30) and MethodDebugInformation (0x31) tables and decodes the sequence points
''' (lines / columns) following the ECMA-335 §23.2.4 rules.
''' </summary>
Public Class PortablePdbReader : Implements IDisposable

    ''' <summary>Source documents decoded from the Document table.</summary>
    Public ReadOnly Property Documents As New List(Of SourceDocument)()

    ''' <summary>Line-number / sequence-point records decoded from MethodDebugInformation.</summary>
    Public ReadOnly Property LineNumbers As New List(Of LineInfo)()

    ' ---- metadata heap file offsets / sizes (within the raw file bytes) ----
    Private raw As Byte() = Nothing
    Private tablesStreamOffset As Integer = -1
    Private tablesStreamSize As Integer = -1
    Private strHeapOffset As Integer = -1
    Private strHeapSize As Integer
    Private usHeapOffset As Integer = -1
    Private usHeapSize As Integer
    Private blobHeapOffset As Integer = -1
    Private blobHeapSize As Integer
    Private guidHeapOffset As Integer = -1
    Private guidHeapSize As Integer
    Private pdbStreamOffset As Integer = -1
    Private pdbStreamSize As Integer = -1
    Private pdbStringHeapOffset As Integer = -1
    Private pdbStringHeapSize As Integer = -1
    Private pdbBlobHeapOffset As Integer = -1
    Private pdbBlobHeapSize As Integer = -1
    Private usePdbHeap As Boolean = False

    ' ---- table schema ----
    Private schemas(63)() As Col
    Private disposedValue As Boolean

    ' ---- a decoded sequence point ----
    Private Structure SeqPoint
        Public DocumentIndex As Integer
        Public ILOffset As Integer
        Public StartLine As Integer
        Public StartColumn As Integer
        Public EndLine As Integer
        Public EndColumn As Integer
        Public IsHidden As Boolean
    End Structure

    Sub New(pePath As String)
        Using fs As New FileStream(pePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            raw = New Byte(fs.Length - 1) {}
            Dim total As Integer = 0

            While total < raw.Length
                Dim n As Integer = fs.Read(raw, total, raw.Length - total)
                If n = 0 Then Exit While
                total += n
            End While
        End Using

        InitSchemas()
        Parse(raw)
    End Sub

    Sub New(data As Byte())
        raw = data
        InitSchemas()
        Parse(raw)
    End Sub

#Region "PE / metadata discovery"

    Private Sub Parse(data As Byte())
        If data.Length < 4 Then
            Return
        End If

        raw = data
        InitSchemas()

        Dim magic4 As UInteger = BitConverter.ToUInt32(data, 0)
        Dim metaFile As Integer

        If data(0) = &H4D AndAlso data(1) = &H5A Then
            ' Wrapped in a PE file (DOS "MZ" header) — metadata lives behind the COR20 header.
            metaFile = FindMetadataInPe(data)
        ElseIf magic4 = &H424A5342UI Then
            ' Standalone Portable PDB: the metadata root ("BSJB") is at offset 0.
            metaFile = 0
        Else
            Return
        End If

        If metaFile < 0 Then
            Return
        End If

        ParseStreamHeaders(metaFile, data)
        ParsePdbStreamHeader()

        If tablesStreamOffset >= 0 AndAlso tablesStreamSize > 0 Then
            Dim tablesBytes As Byte() = New Byte(tablesStreamSize - 1) {}
            Array.Copy(data, tablesStreamOffset, tablesBytes, 0, tablesStreamSize)
            ParseTablesStream(tablesBytes)
        End If
    End Sub

    ''' <summary>
    ''' Walk a PE file to the CLI metadata root, returning its file offset, or -1 if not a
    ''' managed PE. Returns -1 (no metadata) for a PE without a CLR runtime header.
    ''' </summary>
    Private Function FindMetadataInPe(data As Byte()) As Integer
        If data.Length < 64 Then
            Return -1
        End If

        Dim dosSig As UShort = BitConverter.ToUInt16(data, 0)

        If dosSig <> &H5A4D Then ' "MZ"
            Return -1
        End If

        Dim e_lfanew As Integer = BitConverter.ToInt32(data, 60)

        If e_lfanew + 4 > data.Length Then
            Return -1
        End If

        Dim peSig As UInteger = BitConverter.ToUInt32(data, e_lfanew)

        If peSig <> &H4550UI Then ' "PE\0\0"
            Return -1
        End If

        Dim coffOffset As Integer = e_lfanew + 4
        Dim numSections As Integer = BitConverter.ToUInt16(data, coffOffset + 2)
        Dim sizeOfOptHeader As Integer = BitConverter.ToUInt16(data, coffOffset + 16)
        Dim optHeaderOffset As Integer = coffOffset + 20

        If optHeaderOffset + 2 > data.Length Then
            Return -1
        End If

        Dim magic As UShort = BitConverter.ToUInt16(data, optHeaderOffset)
        Dim ddOffset As Integer

        If magic = &H10B Then ' PE32
            ddOffset = optHeaderOffset + 96
        Else ' PE32+
            ddOffset = optHeaderOffset + 112
        End If

        If ddOffset + 14 * 8 + 8 > data.Length Then
            Return -1
        End If

        ' Data directory #14 is the CLR Runtime Header (COR20).
        Dim clrRva As UInteger = BitConverter.ToUInt32(data, ddOffset + 14 * 8)

        If clrRva = 0 Then
            Return -1
        End If

        Dim clrFile As Integer = RvaToOffset(clrRva, data, optHeaderOffset, numSections, sizeOfOptHeader)

        If clrFile < 0 OrElse clrFile + 12 > data.Length Then
            Return -1
        End If

        ' COR20: MetaDataRva / MetaDataSize live at offset 8 / 12.
        Dim metaRva As UInteger = BitConverter.ToUInt32(data, clrFile + 8)
        Return RvaToOffset(metaRva, data, optHeaderOffset, numSections, sizeOfOptHeader)
    End Function

    Private Function RvaToOffset(rva As UInteger, data As Byte(), optHeaderOffset As Integer, numSections As Integer, sizeOfOptHeader As Integer) As Integer
        Dim secTable As Integer = optHeaderOffset + sizeOfOptHeader

        For i As Integer = 0 To numSections - 1
            Dim so As Integer = secTable + i * 40

            If so + 20 > data.Length Then
                Exit For
            End If

            Dim virtSize As UInteger = BitConverter.ToUInt32(data, so + 8)
            Dim virtAddr As UInteger = BitConverter.ToUInt32(data, so + 12)
            Dim rawSize As UInteger = BitConverter.ToUInt32(data, so + 16)
            Dim rawPtr As UInteger = BitConverter.ToUInt32(data, so + 20)

            If rva >= virtAddr AndAlso rva < virtAddr + std.Max(virtSize, rawSize) Then
                Return CInt(rva - virtAddr + rawPtr)
            End If
        Next

        Return -1
    End Function

    Private Sub ParseStreamHeaders(metadataRootOffset As Integer, data As Byte())
        If metadataRootOffset + 20 > data.Length Then
            Return
        End If

        Dim sig As UInteger = BitConverter.ToUInt32(data, metadataRootOffset)

        If sig <> &H424A5342UI Then ' "BSJB"
            Return
        End If

        Dim versionLen As Integer = BitConverter.ToInt32(data, metadataRootOffset + 12)

        ' stream headers start right after the (4-byte aligned) version string + u16 flags + u16 count
        Dim shPos As Integer = metadataRootOffset + 20 + ((versionLen + 3) And Not 3)
        Dim numStreams As Integer = BitConverter.ToUInt16(data, shPos - 2)

        For i As Integer = 0 To numStreams - 1
            If shPos + 8 > data.Length Then
                Exit For
            End If

            Dim off As Integer = BitConverter.ToInt32(data, shPos)
            Dim sz As Integer = BitConverter.ToInt32(data, shPos + 4)
            shPos += 8

            Dim nameStart As Integer = shPos
            Dim np As Integer = nameStart

            While np < data.Length AndAlso data(np) <> 0
                np += 1
            End While

            Dim name As String = Encoding.ASCII.GetString(data, nameStart, np - nameStart)
            shPos = (np + 1 + 3) And Not 3

            Dim heapOffset As Integer = metadataRootOffset + off

            Select Case name
                Case "#~" : tablesStreamOffset = heapOffset : tablesStreamSize = sz
                Case "#Strings" : strHeapOffset = heapOffset : strHeapSize = sz
                Case "#US" : usHeapOffset = heapOffset : usHeapSize = sz
                Case "#Blob" : blobHeapOffset = heapOffset : blobHeapSize = sz
                Case "#GUID" : guidHeapOffset = heapOffset : guidHeapSize = sz
                Case "#Pdb" : pdbStreamOffset = heapOffset : pdbStreamSize = sz
            End Select
        Next
    End Sub

    Private pdbHeapValid As Boolean = False

    Private Sub ParsePdbStreamHeader()
        usePdbHeap = False

        If pdbStreamOffset < 0 OrElse pdbStreamSize < 12 Then
            Return
        End If

        ' The #Pdb stream header is either 20 or 24 bytes (a version/Reserved difference). Only when
        ' the well-known signature is present do we treat the document strings / sequence points as
        ' living in the #Pdb heaps; otherwise we fall back to the ECMA-335 default (#Blob heap).
        Dim sig As UInteger = BitConverter.ToUInt32(raw, pdbStreamOffset)

        If sig <> &H50316442UI Then
            Return
        End If

        For Each hdrLen As Integer In {20, 24}
            If pdbStreamSize >= hdrLen + 8 Then
                Dim sSize As Integer = BitConverter.ToInt32(raw, pdbStreamOffset + hdrLen - 8)
                Dim bSize As Integer = BitConverter.ToInt32(raw, pdbStreamOffset + hdrLen - 4)

                If sSize >= 0 AndAlso bSize >= 0 AndAlso hdrLen + sSize + bSize <= pdbStreamSize Then
                    pdbStringHeapOffset = pdbStreamOffset + hdrLen
                    pdbStringHeapSize = sSize
                    pdbBlobHeapOffset = pdbStringHeapOffset + sSize
                    pdbBlobHeapSize = bSize
                    usePdbHeap = True
                    Return
                End If
            End If
        Next
    End Sub

#End Region

#Region "Table decoding (#~)"

    Private Sub ParseTablesStream(tables As Byte())
        If tables.Length < 24 Then
            Return
        End If

        Dim valid As Long = BitConverter.ToUInt64(tables, 8)
        Dim heapSizes As Integer = tables(6)

        Dim rowCounts(63) As Long
        Dim present As New List(Of Integer)()
        Dim p As Integer = 24

        For i As Integer = 0 To 63
            If (valid And (CLng(1) << i)) <> 0 Then
                rowCounts(i) = BitConverter.ToUInt32(tables, p)
                p += 4
                present.Add(i)
            End If
        Next

        Dim rowsStart As Integer = p
        Dim docOffset As Integer = -1, docCount As Integer = 0, docRowSize As Integer = 0
        Dim mdiOffset As Integer = -1, mdiCount As Integer = 0, mdiRowSize As Integer = 0
        Dim cur As Integer = rowsStart

        For Each t As Integer In present
            Dim rs As Integer = RowSize(t, rowCounts, heapSizes)

            If t = 48 Then
                docOffset = cur
                docCount = CInt(rowCounts(48))
                docRowSize = rs
            ElseIf t = 49 Then
                mdiOffset = cur
                mdiCount = CInt(rowCounts(49))
                mdiRowSize = rs
            End If

            cur += CInt(rowCounts(t)) * rs
        Next

        If docOffset >= 0 Then
            DecodeDocuments(tables, docOffset, docCount, docRowSize, heapSizes)
        End If

        If mdiOffset >= 0 AndAlso docOffset >= 0 Then
            DecodeMethodDebugInfo(tables, mdiOffset, mdiCount, mdiRowSize, heapSizes)
        End If
    End Sub

    Private Sub DecodeDocuments(tables As Byte(), offset As Integer, count As Integer, rowSize As Integer, heapSizes As Integer)
        Dim guidSize As Integer = If((heapSizes And 2) <> 0, 4, 2)
        Dim blobSize As Integer = If((heapSizes And 4) <> 0, 4, 2)

        For i As Integer = 0 To count - 1
            Dim rowStart As Integer = offset + i * rowSize

            If rowStart + rowSize > tables.Length Then
                Exit For
            End If

            Dim nameIdx As UInteger
            Dim langIdx As UInteger
            Dim hashAlgIdx As UInteger
            Dim hashIdx As UInteger

            If usePdbHeap Then
                ' #Pdb layout: Name (4-byte pdb string), Language (#GUID), HashAlgorithm (#GUID), Hash (4-byte pdb blob).
                nameIdx = BitConverter.ToUInt32(tables, rowStart)
                langIdx = ReadHeapIndex(tables, rowStart + 4, guidSize)
                hashAlgIdx = ReadHeapIndex(tables, rowStart + 4 + guidSize, guidSize)
                hashIdx = BitConverter.ToUInt32(tables, rowStart + 4 + 2 * guidSize)
            Else
                ' ECMA-335 default: Name (#Blob), Language (#GUID), HashAlgorithm (#GUID), Hash (#Blob).
                nameIdx = ReadHeapIndex(tables, rowStart, blobSize)
                langIdx = ReadHeapIndex(tables, rowStart + guidSize, guidSize)
                hashAlgIdx = ReadHeapIndex(tables, rowStart + 2 * guidSize, guidSize)
                hashIdx = ReadHeapIndex(tables, rowStart + 3 * guidSize, blobSize)
            End If

            Dim doc As New SourceDocument With {
                .FilePath = If(usePdbHeap, ReadPdbString(nameIdx), ReadBlobString(nameIdx)),
                .Language = ReadGuid(langIdx),
                .HashAlgorithm = ReadGuid(hashAlgIdx),
                .Checksum = If(usePdbHeap, ReadPdbBlob(hashIdx), ReadBlobBytes(hashIdx))
            }
            Documents.Add(doc)
        Next
    End Sub

    Private Sub DecodeMethodDebugInfo(tables As Byte(), offset As Integer, count As Integer, rowSize As Integer, heapSizes As Integer)
        Dim docRidSize As Integer = If(Documents.Count < &H10000, 2, 4)
        Dim blobSize As Integer = If((heapSizes And 4) <> 0, 4, 2)

        For i As Integer = 0 To count - 1
            Dim rowStart As Integer = offset + i * rowSize

            If rowStart + rowSize > tables.Length Then
                Exit For
            End If

            Dim docIndex As UInteger = ReadHeapIndex(tables, rowStart, docRidSize)

            Dim seqIdx As UInteger

            If usePdbHeap Then
                seqIdx = BitConverter.ToUInt32(tables, rowStart + docRidSize)
            Else
                seqIdx = ReadHeapIndex(tables, rowStart + docRidSize, blobSize)
            End If

            If docIndex < 1 OrElse docIndex > Documents.Count Then
                Continue For
            End If

            Dim blob As Byte() = If(usePdbHeap, ReadPdbBlob(seqIdx), ReadBlobBytes(seqIdx))
            Dim sps As New List(Of SeqPoint)()
            DecodeSequencePoints(blob, CInt(docIndex), sps)

            For Each sp As SeqPoint In sps
                If sp.IsHidden Then
                    Continue For
                End If

                If sp.DocumentIndex < 1 OrElse sp.DocumentIndex > Documents.Count Then
                    Continue For
                End If

                LineNumbers.Add(New LineInfo With {
                    .Document = Documents(sp.DocumentIndex - 1),
                    .Offset = sp.ILOffset,
                    .StartLine = sp.StartLine,
                    .EndLine = sp.EndLine,
                    .StartColumn = sp.StartColumn,
                    .EndColumn = sp.EndColumn
                })
            Next
        Next
    End Sub

    ''' <summary>
    ''' Decode a sequence-points blob (ECMA-335 §23.2.4). Each record carries an IL offset, a
    ''' line/column span and (optionally) a document change. A record with zero line/column delta
    ''' is a "hidden" sequence point (its start-line has the 0x8000 flag set).
    ''' </summary>
    Private Sub DecodeSequencePoints(blob As Byte(), baseDoc As Integer, ByRef out As List(Of SeqPoint))
        out = New List(Of SeqPoint)()

        If blob Is Nothing OrElse blob.Length = 0 Then
            Return
        End If

        Dim prevILOffset As Integer = 0
        Dim prevStartLine As Integer = 0
        Dim prevStartColumn As Integer = 0
        Dim prevEndLine As Integer = 0
        Dim prevEndColumn As Integer = 0
        Dim docIndex As Integer = baseDoc
        Dim pos As Integer = 0
        Dim first As Boolean = True

        While pos < blob.Length
            Try
                ' Document index: a delta from the previous record's document (the first one is a
                ' delta from the method's base document held in the MethodDebugInformation table).
                Dim docDelta As Integer = ReadCompressed(blob, pos)

                If first Then
                    docIndex = baseDoc + docDelta
                Else
                    docIndex += docDelta
                End If

                Dim ilOffset As Integer

                If first Then
                    ilOffset = ReadCompressed(blob, pos)
                Else
                    ilOffset = prevILOffset + ReadCompressedSigned(blob, pos)
                End If

                Dim deltaIL As Integer = ReadCompressed(blob, pos)
                Dim deltaLines As Integer = ReadCompressedSigned(blob, pos)
                Dim deltaColumns As Integer = ReadCompressedSigned(blob, pos)

                Dim sp As New SeqPoint With {.DocumentIndex = docIndex, .ILOffset = ilOffset}
                Dim nextILOffset As Integer = ilOffset + deltaIL

                If deltaLines = 0 AndAlso deltaColumns = 0 Then
                    ' Hidden sequence point.
                    Dim startLine As Integer = ReadCompressed(blob, pos)
                    sp.IsHidden = (startLine And &H8000) <> 0
                    sp.StartLine = startLine And &H7FFF
                    sp.StartColumn = 0
                    sp.EndLine = sp.StartLine
                    sp.EndColumn = 0
                    prevStartLine = 0
                    prevStartColumn = 0
                    prevEndLine = 0
                    prevEndColumn = 0
                Else
                    Dim startLine As Integer = prevStartLine + ReadCompressedSigned(blob, pos)
                    Dim startColumn As Integer = prevStartColumn + ReadCompressedSigned(blob, pos)
                    Dim endLine As Integer = prevEndLine + ReadCompressedSigned(blob, pos)
                    Dim endColumn As Integer = prevEndColumn + ReadCompressedSigned(blob, pos)

                    sp.StartLine = startLine
                    sp.StartColumn = startColumn
                    sp.EndLine = endLine
                    sp.EndColumn = endColumn

                    prevStartLine = startLine
                    prevStartColumn = startColumn
                    prevEndLine = endLine
                    prevEndColumn = endColumn
                End If

                prevILOffset = nextILOffset
                first = False
                out.Add(sp)
            Catch
                Exit While
            End Try
        End While
    End Sub

#End Region

#Region "Heap readers"

    Private Shared Function ReadHeapIndex(b As Byte(), pos As Integer, size As Integer) As UInteger
        If size = 4 Then
            Return BitConverter.ToUInt32(b, pos)
        End If

        Return BitConverter.ToUInt16(b, pos)
    End Function

    Private Function ReadPdbString(idx As UInteger) As String
        If idx = 0 OrElse pdbStringHeapOffset < 0 Then
            Return ""
        End If

        Dim start As Integer = pdbStringHeapOffset + CInt(idx)

        If start >= raw.Length Then
            Return ""
        End If

        Dim p As Integer = start

        While p < raw.Length AndAlso raw(p) <> 0
            p += 1
        End While

        Return Encoding.UTF8.GetString(raw, start, p - start)
    End Function

    Private Function ReadPdbBlob(idx As UInteger) As Byte()
        If idx = 0 OrElse pdbBlobHeapOffset < 0 Then
            Return New Byte(-1) {}
        End If

        Dim pos As Integer = pdbBlobHeapOffset + CInt(idx)

        If pos >= raw.Length Then
            Return New Byte(-1) {}
        End If

        Dim len As Integer = ReadCompressed(raw, pos)

        If len < 0 OrElse pos + len > raw.Length Then
            Return New Byte(-1) {}
        End If

        Dim buf As Byte() = New Byte(len - 1) {}
        Array.Copy(raw, pos, buf, 0, len)
        Return buf
    End Function

    Private Function ReadGuid(idx As UInteger) As Guid
        If idx = 0 OrElse guidHeapOffset < 0 Then
            Return Guid.Empty
        End If

        Dim pos As Integer = guidHeapOffset + CInt((idx - 1UI) * 16UI)

        If pos + 16 > raw.Length Then
            Return Guid.Empty
        End If

        Dim g(15) As Byte
        Array.Copy(raw, pos, g, 0, 16)
        Return New Guid(g)
    End Function

    ''' <summary>Read a length-prefixed UTF-8 string from the standard #Blob heap (ECMA-335 default Document layout).</summary>
    Private Function ReadBlobString(idx As UInteger) As String
        If idx = 0 OrElse blobHeapOffset < 0 Then
            Return ""
        End If

        Dim pos As Integer = blobHeapOffset + CInt(idx)

        If pos >= raw.Length Then
            Return ""
        End If

        Dim len As Integer = ReadCompressed(raw, pos)

        If len < 0 OrElse pos + len > raw.Length Then
            Return ""
        End If

        Return Encoding.UTF8.GetString(raw, pos, len)
    End Function

    ''' <summary>Read a length-prefixed byte blob from the standard #Blob heap (ECMA-335 default Document.Hash).</summary>
    Private Function ReadBlobBytes(idx As UInteger) As Byte()
        If idx = 0 OrElse blobHeapOffset < 0 Then
            Return New Byte() {}
        End If

        Dim pos As Integer = blobHeapOffset + CInt(idx)

        If pos >= raw.Length Then
            Return New Byte() {}
        End If

        Dim len As Integer = ReadCompressed(raw, pos)

        If len < 0 OrElse pos + len > raw.Length Then
            Return New Byte() {}
        End If

        Dim buf As Byte() = New Byte(len - 1) {}
        Array.Copy(raw, pos, buf, 0, len)
        Return buf
    End Function

    ''' <summary>Read an ECMA-335 compressed (unsigned) integer; advances <paramref name="pos"/>.</summary>
    Private Shared Function ReadCompressed(blob As Byte(), ByRef pos As Integer) As Integer
        Dim b0 As Byte = blob(pos)

        If (b0 And &H80) = 0 Then
            pos += 1
            Return b0
        ElseIf (b0 And &HC0) = &H80 Then
            Dim v As Integer = ((b0 And &H3F) << 8) Or blob(pos + 1)
            pos += 2
            Return v
        Else
            Dim v As Integer = ((b0 And &H1F) << 24) Or (blob(pos + 1) << 16) Or (blob(pos + 2) << 8) Or blob(pos + 3)
            pos += 4
            Return v
        End If
    End Function

    ''' <summary>Read an ECMA-335 compressed signed integer; advances <paramref name="pos"/>.</summary>
    Private Shared Function ReadCompressedSigned(blob As Byte(), ByRef pos As Integer) As Integer
        Dim b0 As Byte = blob(pos)

        If (b0 And &H80) = 0 Then
            ' 1-byte form: sign bit is 0x40.
            Dim v As Integer = b0 And &H3F
            If (b0 And &H40) <> 0 Then v -= &H40
            pos += 1
            Return v
        ElseIf (b0 And &HC0) = &H80 Then
            ' 2-byte form: sign bit is 0x2000.
            Dim v As Integer = ((b0 And &H3F) << 8) Or blob(pos + 1)
            If (v And &H2000) <> 0 Then v -= &H4000
            pos += 2
            Return v
        Else
            ' 4-byte form: sign bit is 0x20000000.
            Dim v As Integer = ((b0 And &H1F) << 24) Or (blob(pos + 1) << 16) Or (blob(pos + 2) << 8) Or blob(pos + 3)
            If (v And &H20000000) <> 0 Then v -= &H40000000
            pos += 4
            Return v
        End If
    End Function

#End Region

#Region "Table schema / row-size computation"

    Private Enum ColKind
        UInt8 = 0
        UInt16 = 1
        UInt32 = 2
        StringHeap = 3
        GuidHeap = 4
        BlobHeap = 5
        Rid = 6
        Coded = 7
        PdbString = 8
        PdbBlob = 9
    End Enum

    Private Structure Col
        Public Kind As ColKind
        Public Arg As Integer
    End Structure

    Private Shared Function C(k As ColKind, a As Integer) As Col
        Return New Col With {.Kind = k, .Arg = a}
    End Function

    Private Sub InitSchemas()
        schemas(0) = New Col() {C(ColKind.UInt16, 0), C(ColKind.StringHeap, 0), C(ColKind.GuidHeap, 0), C(ColKind.GuidHeap, 0), C(ColKind.GuidHeap, 0)} ' Module
        schemas(1) = New Col() {C(ColKind.Coded, 11), C(ColKind.StringHeap, 0), C(ColKind.StringHeap, 0)} ' TypeRef
        schemas(2) = New Col() {C(ColKind.UInt32, 0), C(ColKind.StringHeap, 0), C(ColKind.StringHeap, 0), C(ColKind.Coded, 0), C(ColKind.Rid, 4), C(ColKind.Rid, 6)} ' TypeDef
        schemas(3) = New Col() {C(ColKind.Rid, 4)} ' FieldPtr
        schemas(4) = New Col() {C(ColKind.UInt16, 0), C(ColKind.StringHeap, 0), C(ColKind.BlobHeap, 0)} ' Field
        schemas(5) = New Col() {C(ColKind.Rid, 6)} ' MethodPtr
        schemas(6) = New Col() {C(ColKind.UInt32, 0), C(ColKind.UInt16, 0), C(ColKind.UInt16, 0), C(ColKind.StringHeap, 0), C(ColKind.BlobHeap, 0), C(ColKind.Rid, 8)} ' MethodDef
        schemas(7) = New Col() {C(ColKind.Rid, 8)} ' ParamPtr
        schemas(8) = New Col() {C(ColKind.UInt16, 0), C(ColKind.UInt16, 0), C(ColKind.StringHeap, 0)} ' Param
        schemas(9) = New Col() {C(ColKind.Rid, 2), C(ColKind.Coded, 0)} ' InterfaceImpl
        schemas(10) = New Col() {C(ColKind.Coded, 5), C(ColKind.StringHeap, 0), C(ColKind.BlobHeap, 0)} ' MemberRef
        schemas(11) = New Col() {C(ColKind.UInt8, 0), C(ColKind.Coded, 1), C(ColKind.BlobHeap, 0)} ' Constant
        schemas(12) = New Col() {C(ColKind.Coded, 2), C(ColKind.Coded, 10), C(ColKind.BlobHeap, 0)} ' CustomAttribute
        schemas(13) = New Col() {C(ColKind.Coded, 3), C(ColKind.BlobHeap, 0)} ' FieldMarshal
        schemas(14) = New Col() {C(ColKind.UInt16, 0), C(ColKind.Coded, 4), C(ColKind.BlobHeap, 0)} ' DeclSecurity
        schemas(15) = New Col() {C(ColKind.UInt16, 0), C(ColKind.UInt32, 0), C(ColKind.Rid, 2)} ' ClassLayout
        schemas(16) = New Col() {C(ColKind.UInt32, 0), C(ColKind.Rid, 4)} ' FieldLayout
        schemas(17) = New Col() {C(ColKind.BlobHeap, 0)} ' StandAloneSig
        schemas(18) = New Col() {C(ColKind.Rid, 2), C(ColKind.Rid, 20)} ' EventMap
        schemas(19) = New Col() {C(ColKind.Rid, 20)} ' EventPtr
        schemas(20) = New Col() {C(ColKind.UInt16, 0), C(ColKind.StringHeap, 0), C(ColKind.Coded, 0)} ' Event
        schemas(21) = New Col() {C(ColKind.Rid, 2), C(ColKind.Rid, 23)} ' PropertyMap
        schemas(22) = New Col() {C(ColKind.Rid, 23)} ' PropertyPtr
        schemas(23) = New Col() {C(ColKind.UInt16, 0), C(ColKind.StringHeap, 0), C(ColKind.BlobHeap, 0)} ' Property
        schemas(24) = New Col() {C(ColKind.UInt16, 0), C(ColKind.Rid, 6), C(ColKind.Coded, 6)} ' MethodSemantics
        schemas(25) = New Col() {C(ColKind.Rid, 2), C(ColKind.Coded, 7), C(ColKind.Coded, 7)} ' MethodImpl
        schemas(26) = New Col() {C(ColKind.StringHeap, 0)} ' ModuleRef
        schemas(27) = New Col() {C(ColKind.BlobHeap, 0)} ' TypeSpec
        schemas(28) = New Col() {C(ColKind.UInt16, 0), C(ColKind.Coded, 8), C(ColKind.StringHeap, 0), C(ColKind.Rid, 26)} ' ImplMap
        schemas(29) = New Col() {C(ColKind.UInt32, 0), C(ColKind.Rid, 4)} ' FieldRVA
        schemas(30) = New Col() {C(ColKind.UInt32, 0), C(ColKind.UInt32, 0)} ' ENCLog
        schemas(31) = New Col() {C(ColKind.UInt32, 0)} ' ENCMap
        schemas(32) = New Col() {C(ColKind.UInt32, 0), C(ColKind.UInt16, 0), C(ColKind.UInt16, 0), C(ColKind.UInt16, 0), C(ColKind.UInt16, 0), C(ColKind.UInt32, 0), C(ColKind.BlobHeap, 0), C(ColKind.StringHeap, 0), C(ColKind.StringHeap, 0)} ' Assembly
        schemas(33) = New Col() {C(ColKind.UInt32, 0)} ' AssemblyProcessor
        schemas(34) = New Col() {C(ColKind.UInt32, 0), C(ColKind.UInt32, 0), C(ColKind.UInt32, 0)} ' AssemblyOS
        schemas(35) = New Col() {C(ColKind.UInt16, 0), C(ColKind.UInt16, 0), C(ColKind.UInt16, 0), C(ColKind.UInt16, 0), C(ColKind.UInt32, 0), C(ColKind.BlobHeap, 0), C(ColKind.StringHeap, 0), C(ColKind.StringHeap, 0), C(ColKind.BlobHeap, 0)} ' AssemblyRef
        schemas(36) = New Col() {C(ColKind.UInt32, 0), C(ColKind.Rid, 35)} ' AssemblyRefProcessor
        schemas(37) = New Col() {C(ColKind.UInt32, 0), C(ColKind.UInt32, 0), C(ColKind.UInt32, 0), C(ColKind.Rid, 35)} ' AssemblyRefOS
        schemas(38) = New Col() {C(ColKind.UInt32, 0), C(ColKind.StringHeap, 0), C(ColKind.BlobHeap, 0)} ' File
        schemas(39) = New Col() {C(ColKind.UInt32, 0), C(ColKind.UInt32, 0), C(ColKind.StringHeap, 0), C(ColKind.StringHeap, 0), C(ColKind.Coded, 9)} ' ExportedType
        schemas(40) = New Col() {C(ColKind.UInt32, 0), C(ColKind.UInt32, 0), C(ColKind.StringHeap, 0), C(ColKind.Coded, 9)} ' ManifestResource
        schemas(41) = New Col() {C(ColKind.Rid, 2), C(ColKind.Rid, 2)} ' NestedClass
        schemas(42) = New Col() {C(ColKind.UInt16, 0), C(ColKind.UInt16, 0), C(ColKind.Coded, 12), C(ColKind.StringHeap, 0)} ' GenericParam
        schemas(43) = New Col() {C(ColKind.Coded, 7), C(ColKind.BlobHeap, 0)} ' MethodSpec
        schemas(44) = New Col() {C(ColKind.Rid, 42), C(ColKind.Coded, 0)} ' GenericParamConstraint
        schemas(45) = New Col() {} ' reserved
        schemas(46) = New Col() {} ' reserved
        schemas(47) = New Col() {} ' reserved
        ' Document (0x30): Name (#Pdb string), Language (#GUID), HashAlgorithm (#GUID), Hash (#Pdb blob)
        schemas(48) = New Col() {C(ColKind.PdbString, 0), C(ColKind.GuidHeap, 0), C(ColKind.GuidHeap, 0), C(ColKind.PdbBlob, 0)}
        ' MethodDebugInformation (0x31): Document (RID), SequencePoints (#Pdb blob)
        schemas(49) = New Col() {C(ColKind.Rid, 48), C(ColKind.PdbBlob, 0)}
        ' Portable-PDB-specific tables (may be present alongside the debug tables).
        schemas(50) = New Col() {C(ColKind.Rid, 2), C(ColKind.Rid, 53), C(ColKind.Rid, 51), C(ColKind.Rid, 52)} ' LocalScope
        schemas(51) = New Col() {C(ColKind.UInt16, 0), C(ColKind.BlobHeap, 0)} ' LocalVariable
        schemas(52) = New Col() {C(ColKind.StringHeap, 0), C(ColKind.BlobHeap, 0)} ' LocalConstant
        schemas(53) = New Col() {C(ColKind.Rid, 53), C(ColKind.BlobHeap, 0)} ' ImportScope
        schemas(54) = New Col() {C(ColKind.Rid, 6), C(ColKind.Rid, 6)} ' StateMachineMethod
        schemas(55) = New Col() {C(ColKind.Coded, 2), C(ColKind.GuidHeap, 0), C(ColKind.BlobHeap, 0)} ' CustomDebugInformation
    End Sub

    Private Function RowSize(tableIdx As Integer, rowCounts As Long(), heapSizes As Integer) As Integer
        Dim cols As Col() = schemas(tableIdx)

        If cols Is Nothing Then
            Return 0
        End If
        Dim size As Integer = 0

        For Each c As Col In cols
            Select Case c.Kind
                Case ColKind.UInt8 : size += 1
                Case ColKind.UInt16 : size += 2
                Case ColKind.UInt32 : size += 4
                Case ColKind.StringHeap : size += If((heapSizes And 1) <> 0, 4, 2)
                Case ColKind.GuidHeap : size += If((heapSizes And 2) <> 0, 4, 2)
                Case ColKind.BlobHeap : size += If((heapSizes And 4) <> 0, 4, 2)
                Case ColKind.PdbString, ColKind.PdbBlob : size += 4
                Case ColKind.Rid : size += If(rowCounts(c.Arg) < &H10000L, 2, 4)
                Case ColKind.Coded : size += CodedIndexSize(c.Arg, rowCounts)
            End Select
        Next

        Return size
    End Function

    Private Function CodedIndexSize(tagId As Integer, rowCounts As Long()) As Integer
        Dim bits As Integer = CodedTagBits(tagId)
        Dim maxRows As Long = 0

        For Each t As Integer In CodedTagTables(tagId)
            If rowCounts(t) > maxRows Then
                maxRows = rowCounts(t)
            End If
        Next

        If maxRows < (CLng(1) << (16 - bits)) Then
            Return 2
        End If

        Return 4
    End Function

    Private Shared Function CodedTagBits(tagId As Integer) As Integer
        Select Case tagId
            Case 0 : Return 2 ' TypeDefOrRef
            Case 1 : Return 2 ' HasConstant
            Case 2 : Return 5 ' HasCustomAttribute
            Case 3 : Return 1 ' HasFieldMarshal
            Case 4 : Return 2 ' HasDeclSecurity
            Case 5 : Return 3 ' MemberRefParent
            Case 6 : Return 1 ' HasSemantics
            Case 7 : Return 1 ' MethodDefOrRef
            Case 8 : Return 1 ' MemberForwarded
            Case 9 : Return 2 ' Implementation
            Case 10 : Return 3 ' CustomAttributeType
            Case 11 : Return 2 ' ResolutionScope
            Case 12 : Return 1 ' TypeOrMethodDef
        End Select
        Return 2
    End Function

    Private Shared Function CodedTagTables(tagId As Integer) As Integer()
        Select Case tagId
            Case 0 : Return New Integer() {2, 1, 27}
            Case 1 : Return New Integer() {4, 8, 23}
            Case 2 : Return New Integer() {6, 4, 1, 2, 8, 9, 10, 0, 23, 20}
            Case 3 : Return New Integer() {4, 8}
            Case 4 : Return New Integer() {2, 6, 32}
            Case 5 : Return New Integer() {2, 1, 26, 6, 27}
            Case 6 : Return New Integer() {20, 23}
            Case 7 : Return New Integer() {6, 10}
            Case 8 : Return New Integer() {4, 6}
            Case 9 : Return New Integer() {38, 35, 39}
            Case 10 : Return New Integer() {6, 10}
            Case 11 : Return New Integer() {0, 26, 35, 1}
            Case 12 : Return New Integer() {2, 6}
        End Select
        Return New Integer() {}
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects)
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
            ' TODO: set large fields to null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
    ' Protected Overrides Sub Finalize()
    '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region
End Class
