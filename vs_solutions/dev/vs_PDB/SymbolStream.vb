#Region "Microsoft.VisualBasic::ea64aa161257185aa2ec680cb552d66c, vs_solutions\dev\vs_PDB\SymbolStream.vb"

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

    '   Total Lines: 186
    '    Code Lines: 134 (72.04%)
    ' Comment Lines: 24 (12.90%)
    '    - Xml Docs: 29.17%
    ' 
    '   Blank Lines: 28 (15.05%)
    '     File Size: 7.08 KB


    ' Class PublicSymbolReader
    ' 
    '     Properties: Symbols
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: DecodeRecord, ReadName
    ' 
    '     Sub: Parse
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase.Models

''' <summary>
''' Parses the public-symbol stream of a classic PDB. The stream is located through
''' <see cref="DbiReader.Header.PublicStreamIndex"/> (falling back to stream #0 for the old
''' format) and contains CodeView symbol records (S_PUB32 and friends).
''' </summary>
Public Class PublicSymbolReader

    ''' <summary>Decoded public / global symbols.</summary>
    Public ReadOnly Property Symbols As New List(Of Symbol)()

    Sub New(msf As MSFReader, dbi As DbiReader)
        Dim idx As Integer = dbi.Header.PublicStreamIndex
        Dim s As Stream = Nothing

        If idx < &HFFFE Then
            s = msf.GetStream(idx)
        End If

        If s Is Nothing Then
            s = msf.GetStream(0)
        End If

        If s Is Nothing Then
            Return
        End If

        Parse(s.GetBytes())
    End Sub

    Private Sub Parse(data As Byte())
        If data.Length < 4 Then
            Return
        End If

        ' Public symbol stream header (PSGSIHDR):
        '   u32 cbSymHash      (size of trailing symbol-hash table)
        '   u32 cbAddrMap
        '   u32 cNumThunks
        '   u32 cbSizeOfThunk
        '   u16 tiMinHint
        '   u16 tiMacHint      (=> 20 bytes total)
        ' Followed by the thunk table, then the CodeView symbol records, then the hash table.
        Dim symHashSize As Integer = BitConverter.ToInt32(data, 0)
        Dim thunkCount As Integer = BitConverter.ToInt32(data, 8)
        Dim thunkSize As Integer = BitConverter.ToInt32(data, 12)

        Dim recordsStart As Integer = 20 + thunkCount * thunkSize

        If recordsStart < 0 OrElse recordsStart >= data.Length Then
            recordsStart = 0
        End If

        Dim recordsLen As Integer = data.Length - recordsStart - symHashSize

        If recordsLen <= 0 OrElse recordsLen > data.Length - recordsStart Then
            recordsLen = data.Length - recordsStart
        End If

        If recordsLen <= 0 Then
            Return
        End If

        Dim found As Integer = 0

        For Each rec As CvRecord In CodeView.Enumerate(data, recordsStart, recordsLen)
            Try
                If DecodeRecord(rec) Then
                    found += 1
                End If
            Catch
                ' Skip a malformed symbol record.
            End Try
        Next

        ' Fallback: if the header parsing produced nothing, re-scan from the start.
        If found = 0 Then
            For Each rec As CvRecord In CodeView.Enumerate(data, 0, data.Length)
                Try
                    DecodeRecord(rec)
                Catch
                End Try
            Next
        End If
    End Sub

    ''' <summary>Decode a single symbol record; returns True when it contributed a symbol.</summary>
    Private Function DecodeRecord(rec As CvRecord) As Boolean
        Dim p As Byte() = rec.Payload
        Dim sym As Symbol = Nothing

        Select Case rec.Type
            Case CodeView.S_PUB32
                ' u32 flags; u32 offset; u16 section; char name[]
                If p.Length < 10 Then Return False
                Dim flags As UInteger = BitConverter.ToUInt32(p, 0)
                Dim off As UInteger = BitConverter.ToUInt32(p, 4)
                Dim seg As UShort = BitConverter.ToUInt16(p, 8)
                Dim name As String = CodeView.ReadNullString(p, 10, Encoding.UTF8)
                sym = New Symbol With {
                    .Name = name,
                    .Offset = off,
                    .Section = seg,
                    .Flags = CUShort(flags And &HFFFFUI),
                    .Kind = If((flags And 2UI) <> 0, SymbolKind.Function_, SymbolKind.Public_)
                }

            Case CodeView.S_GDATA32, CodeView.S_LDATA32
                ' u32 type; u32 offset; u16 section; char name[]
                If p.Length < 10 Then Return False
                Dim off As UInteger = BitConverter.ToUInt32(p, 4)
                Dim seg As UShort = BitConverter.ToUInt16(p, 8)
                Dim name As String = CodeView.ReadNullString(p, 10, Encoding.UTF8)
                sym = New Symbol With {
                    .Name = name,
                    .Offset = off,
                    .Section = seg,
                    .Kind = SymbolKind.Data
                }

            Case CodeView.S_GPROC32, CodeView.S_LPROC32
                ' u32 pParent; u32 pEnd; u32 pNext; u32 len; u32 dbgStart; u32 dbgEnd;
                ' u32 offset; u16 seg; u8 flags; u8 tOn; u16 tStart; u16 tEnd; char name[]
                If p.Length < 36 Then Return False
                Dim length As UInteger = BitConverter.ToUInt32(p, 12)
                Dim off As UInteger = BitConverter.ToUInt32(p, 24)
                Dim seg As UShort = BitConverter.ToUInt16(p, 28)
                Dim name As String = CodeView.ReadNullString(p, 36, Encoding.UTF8)
                sym = New Symbol With {
                    .Name = name,
                    .Offset = off,
                    .Section = seg,
                    .Length = length,
                    .Kind = SymbolKind.Function_
                }

            Case CodeView.S_PROCREF, CodeView.S_LPROCREF
                ' u32 sumName; u32 ibSym; u16 imod; char name[]
                If p.Length < 10 Then Return False
                Dim name As String = CodeView.ReadNullString(p, 10, Encoding.UTF8)
                sym = New Symbol With {
                    .Name = name,
                    .Kind = SymbolKind.Procedure
                }

            Case CodeView.S_CONSTANT
                ' u32 type; numeric leaf value; char name[]
                If p.Length < 4 Then Return False
                Dim pos As Integer = 4
                Dim skip As Integer = 0
                TpiReader.ReadNumericLeaf(p, pos, skip)
                pos += skip
                Dim name As String = ReadName(p, pos)
                sym = New Symbol With {
                    .Name = name,
                    .Kind = SymbolKind.Constant
                }

            Case CodeView.S_UDT
                ' u32 type; char name[]
                If p.Length < 4 Then Return False
                Dim name As String = CodeView.ReadNullString(p, 4, Encoding.UTF8)
                sym = New Symbol With {
                    .Name = name,
                    .Kind = SymbolKind.Unknown
                }
        End Select

        If sym IsNot Nothing AndAlso sym.Name.Length > 0 Then
            Symbols.Add(sym)
            Return True
        End If

        Return False
    End Function

    Private Shared Function ReadName(p As Byte(), pos As Integer) As String
        If pos < 0 OrElse pos >= p.Length Then
            Return ""
        End If

        Return CodeView.ReadNullString(p, pos, Encoding.UTF8)
    End Function
End Class
