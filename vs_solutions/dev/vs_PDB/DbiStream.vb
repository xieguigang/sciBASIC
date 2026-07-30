#Region "Microsoft.VisualBasic::00000000000000000000000000000000, sciBASIC#\vs_solutions\dev\vs_PDB\DbiStream.vb"

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
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.Models
Imports Microsoft.VisualBasic.Data.IO
Imports std = System.Math

''' <summary>
''' Parses the DBI stream (stream #3) of a classic PDB: it exposes the module list,
''' the referenced source documents, and (best-effort) the line-number information.
''' </summary>
Public Class DbiReader

    ''' <summary>
    ''' Header information carried by the DBI stream.
    ''' </summary>
    Public Class DbiHeader
        Public VersionSignature As Integer
        Public VersionHeader As Integer
        Public Age As Integer
        Public GlobalStreamIndex As UShort
        Public PublicStreamIndex As UShort
        Public SymRecordStreamIndex As UShort
        Public ModInfoSize As Integer
        Public SectionContributionSize As Integer
        Public SectionMapSize As Integer
        Public SourceInfoSize As Integer
        Public TypeServerMapSize As Integer
        Public OptionalDbgHdrSize As Integer
        Public ECSubstreamSize As Integer
        Public Machine As UShort
        Public Property PdbDllVersion As UShort
    End Class

    ''' <summary>
    ''' One module entry from the module-info substream.
    ''' </summary>
    Public Class ModuleInfo
        Public ModuleName As String
        Public ObjFileName As String
        ''' <summary>Indices into the source-info substream file table.</summary>
        Public FileIndices As Integer()
        ''' <summary>Offset of this module's C13 line info, relative to the DBI debug-data substream.</summary>
        Public C13Offset As Integer
        Public C13Size As Integer
        ''' <summary>Offset/length of this module's symbols within the symbol stream.</summary>
        Public SymbolOffset As Integer
        Public SymbolSize As Integer
    End Class

    Public Property Header As DbiHeader
    Public ReadOnly Property Modules As New List(Of ModuleInfo)()
    Public ReadOnly Property SourceDocuments As New List(Of SourceDocument)()
    Public ReadOnly Property LineNumbers As New List(Of LineInfo)()

    ' C13 sub-section types.
    Private Const DEBUG_S_LINES As UInteger = &HF2
    Private Const DEBUG_S_LINES_2 As UInteger = &HF9

    Sub New(dbi As Stream)
        Dim bytes As Byte() = dbi.GetBytes()
        Parse(bytes)
    End Sub

    Private Sub Parse(data As Byte())
        If data.Length < 64 Then
            Return
        End If

        Header = ReadHeader(data)
        Dim pos As Integer = 64

        ' 1) module-info substream.
        Dim modInfoEnd As Integer = pos + Header.ModInfoSize
        ParseModuleInfo(data, pos, modInfoEnd)

        ' 2) skip the fixed substreams up to the source-info substream.
        pos = modInfoEnd + Header.SectionContributionSize + Header.SectionMapSize
        Dim srcInfoEnd As Integer = pos + Header.SourceInfoSize
        ParseSourceInfo(data, pos, srcInfoEnd)
        pos = srcInfoEnd + Header.TypeServerMapSize + Header.ECSubstreamSize + Header.OptionalDbgHdrSize

        ' 3) the remaining bytes are the C13 debug-data substream; each module's
        '    line info lives at [pos + module.C13Offset, + module.C13Size].
        For Each m As ModuleInfo In Modules
            If m.C13Size > 0 AndAlso m.C13Offset >= 0 Then
                Dim start As Integer = pos + m.C13Offset
                Dim [end] As Integer = std.Min(start + m.C13Size, data.Length)

                If start >= 0 AndAlso [end] <= data.Length Then
                    ParseC13(data, start, [end], m)
                End If
            End If
        Next
    End Sub

    Private Function ReadHeader(data As Byte()) As DbiHeader
        Dim h As New DbiHeader()

        Using ms As New MemoryStream(data)
            Using br As New BinaryDataReader(ms, leaveOpen:=False) With {.ByteOrder = ByteOrder.LittleEndian}
                h.VersionSignature = br.ReadInt32()
                h.VersionHeader = br.ReadInt32()
                h.Age = br.ReadInt32()
                h.GlobalStreamIndex = br.ReadUInt16()
                h.PublicStreamIndex = CUShort(br.ReadUInt16())
                h.SymRecordStreamIndex = CUShort(br.ReadUInt16())
                h.PdbDllVersion = CUShort(br.ReadUInt16())
                ' pdbDllRbld
                h.PublicStreamIndex = h.PublicStreamIndex ' (kept)
                br.ReadUInt16()
                h.ModInfoSize = br.ReadInt32()
                h.SectionContributionSize = br.ReadInt32()
                h.SectionMapSize = br.ReadInt32()
                h.SourceInfoSize = br.ReadInt32()
                h.TypeServerMapSize = br.ReadInt32()
                h.OptionalDbgHdrSize = br.ReadInt32()
                h.ECSubstreamSize = br.ReadInt32()
                br.ReadUInt16() ' flags
                h.Machine = CUShort(br.ReadUInt16())
                br.ReadUInt32() ' padding
            End Using
        End Using

        Return h
    End Function

    Private Sub ParseModuleInfo(data As Byte(), start As Integer, [end] As Integer)
        Dim p As Integer = start

        While p < [end]
            Dim modStart As Integer = p
            ' Fixed part is 40 bytes (before the inline srcFiles array).
            If p + 40 > [end] Then Exit While

            Dim un1 As Integer = BitConverter.ToInt32(data, p)
            Dim section As UShort = BitConverter.ToUInt16(data, p + 4)
            Dim flags As UShort = BitConverter.ToUInt16(data, p + 6)
            Dim dataCrc As Integer = BitConverter.ToInt32(data, p + 8)
            Dim relocCrc As Integer = BitConverter.ToInt32(data, p + 12)
            Dim symOffset As Integer = BitConverter.ToInt32(data, p + 16)
            Dim symSize As Integer = BitConverter.ToInt32(data, p + 20)
            Dim c11Offset As Integer = BitConverter.ToInt32(data, p + 24)
            Dim c11Size As Integer = BitConverter.ToInt32(data, p + 28)
            Dim c13Offset As Integer = BitConverter.ToInt32(data, p + 32)
            Dim c13Size As Integer = BitConverter.ToInt32(data, p + 36)
            Dim numFiles As UShort = BitConverter.ToUInt16(data, p + 40)
            Dim pdbFile As UShort = BitConverter.ToUInt16(data, p + 42)

            p += 44

            Dim fileIndices As Integer() = New Integer(numFiles - 1) {}

            For i As Integer = 0 To numFiles - 1
                fileIndices(i) = BitConverter.ToInt32(data, p)
                p += 4
            Next

            Dim moduleName As String = ReadNullString(data, p)
            p += moduleName.Length + 1
            Dim objName As String = ReadNullString(data, p)
            p += objName.Length + 1

            Dim m As New ModuleInfo With {
                .ModuleName = moduleName,
                .ObjFileName = objName,
                .FileIndices = fileIndices,
                .C13Offset = c13Offset,
                .C13Size = c13Size,
                .SymbolOffset = symOffset,
                .SymbolSize = symSize
            }
            Modules.Add(m)
        End While
    End Sub

    Private Sub ParseSourceInfo(data As Byte(), start As Integer, [end] As Integer)
        If start + 4 > [end] Then
            Return
        End If

        Dim numModules As UShort = BitConverter.ToUInt16(data, start)
        Dim numSources As UShort = BitConverter.ToUInt16(data, start + 2)
        Dim p As Integer = start + 4

        ' modIndices[numModules], modFileCounts[numModules]
        p += numModules * 2
        p += numModules * 2

        If numSources = 0 Then
            Return
        End If

        Dim offsets As Integer() = New Integer(numSources - 1) {}

        For i As Integer = 0 To numSources - 1
            ' Source-file name offsets are 4-byte (u32) entries.
            offsets(i) = BitConverter.ToInt32(data, p)
            p += 4
        Next

        ' The string table begins right after the offset array.
        Dim stringsBase As Integer = p

        For i As Integer = 0 To numSources - 1
            Dim strPos As Integer = stringsBase + offsets(i)
            Dim path As String = ReadNullString(data, strPos)

            SourceDocuments.Add(New SourceDocument With {.FilePath = path})
        Next
    End Sub

    Private Sub ParseC13(data As Byte(), start As Integer, [end] As Integer, m As ModuleInfo)
        Dim p As Integer = start

        While p + 8 <= [end]
            Dim subType As UInteger = BitConverter.ToUInt32(data, p)
            Dim length As Integer = BitConverter.ToInt32(data, p + 4)
            Dim payloadStart As Integer = p + 8
            Dim payloadEnd As Integer = std.Min(payloadStart + length, [end])

            If subType = DEBUG_S_LINES OrElse subType = DEBUG_S_LINES_2 Then
                Try
                    ParseLines(data, payloadStart, payloadEnd, m, subType = DEBUG_S_LINES_2)
                Catch
                    ' Skip a malformed line subsection.
                End Try
            End If

            ' Advance to the next sub-section (4-byte aligned).
            p = payloadEnd
            p = (p + 3) And Not 3

            If length = 0 Then
                Exit While
            End If
        End While
    End Sub

    Private Sub ParseLines(data As Byte(), start As Integer, [end] As Integer, m As ModuleInfo, withColumns As Boolean)
        Dim p As Integer = start

        If p + 12 > [end] Then
            Return
        End If

        Dim off As Integer = BitConverter.ToInt32(data, p)
        Dim seg As UShort = BitConverter.ToUInt16(data, p + 4)
        Dim flags As UShort = BitConverter.ToUInt16(data, p + 6)
        Dim numFiles As Integer = BitConverter.ToInt32(data, p + 8)
        p += 12

        For f As Integer = 0 To numFiles - 1
            If p + 12 > [end] Then
                Exit For
            End If

            ' Remember where this file entry starts so we can advance by blockSize.
            Dim fileEntryStart As Integer = p
            Dim fileId As Integer = BitConverter.ToInt32(data, p)
            Dim numLines As Integer = BitConverter.ToInt32(data, p + 4)
            Dim blockSize As Integer = BitConverter.ToInt32(data, p + 8)
            p += 12

            Dim doc As SourceDocument = Nothing

            If fileId >= 0 AndAlso fileId < m.FileIndices.Length Then
                Dim srcIndex As Integer = m.FileIndices(fileId)

                If srcIndex >= 0 AndAlso srcIndex < SourceDocuments.Count Then
                    doc = SourceDocuments(srcIndex)
                End If
            End If

            ' 1) the line-number records (each is u32 offset, u32 linenum).
            Dim lineEnd As Integer = p + numLines * 8

            If lineEnd > [end] Then
                lineEnd = [end]
            End If

            For l As Integer = 0 To numLines - 1
                If p + 8 > lineEnd Then
                    Exit For
                End If

                Dim lineOffset As UInteger = BitConverter.ToUInt32(data, p)
                Dim lineNum As UInteger = BitConverter.ToUInt32(data, p + 4)
                p += 8

                Dim li As New LineInfo With {
                    .Document = doc,
                    .Offset = lineOffset
                }
                li.StartLine = CInt(lineNum And &HFFFFFF)
                li.EndLine = li.StartLine + CInt((lineNum >> 24) And &HFF)
                LineNumbers.Add(li)
            Next

            ' 2) for DEBUG_S_LINES_2 the column records follow as a separate array
            '    (u16 colStart, u16 colEnd) per line.
            If withColumns Then
                For l As Integer = 0 To numLines - 1
                    If p + 4 > [end] Then
                        Exit For
                    End If

                    Dim colStart As UShort = BitConverter.ToUInt16(data, p)
                    Dim colEnd As UShort = BitConverter.ToUInt16(data, p + 2)
                    p += 4

                    Dim idx As Integer = LineNumbers.Count - numLines + l

                    If idx >= 0 AndAlso idx < LineNumbers.Count Then
                        LineNumbers(idx).StartColumn = colStart
                        LineNumbers(idx).EndColumn = colEnd
                    End If
                Next
            End If

            ' Advance to the next file entry using the declared block size.
            p = fileEntryStart + blockSize
            p = (p + 3) And Not 3
        Next
    End Sub

    ''' <summary>
    ''' Read a null-terminated (UTF-8) string starting at <paramref name="offset"/>.
    ''' </summary>
    Friend Shared Function ReadNullString(data As Byte(), offset As Integer) As String
        Dim p As Integer = offset

        While p < data.Length AndAlso data(p) <> 0
            p += 1
        End While

        Dim len As Integer = p - offset
        Return Encoding.UTF8.GetString(data, offset, len)
    End Function
End Class

