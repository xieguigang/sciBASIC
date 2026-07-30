#Region "Microsoft.VisualBasic::00000000000000000000000000000000, sciBASIC#\vs_solutions\dev\vs_PDB\CodeView.vb"

    ' Copyright (c) 2018 GPL3 Licensed
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

#End Region

Imports System.Collections.Generic
Imports System.Text

Namespace sciBASIC.PDB

    ''' <summary>
    ''' CodeView symbol / type record constants and a small helper to walk the
    ''' length-prefixed record stream used by both the classic PDB public-symbol
    ''' stream and the TPI type stream.
    ''' </summary>
    Public Module CodeView

        ' ---- CodeView symbol records (from cvinfo.h) ----
        Public Const S_CONSTANT As UShort = &H1107
        Public Const S_UDT As UShort = &H1108
        Public Const S_LDATA32 As UShort = &H110D
        Public Const S_GDATA32 As UShort = &H110C
        Public Const S_PUB32 As UShort = &H110E
        Public Const S_LPROC32 As UShort = &H110F
        Public Const S_GPROC32 As UShort = &H1110
        Public Const S_REGREL32 As UShort = &H1111
        Public Const S_LTHREAD32 As UShort = &H1112
        Public Const S_GTHREAD32 As UShort = &H1113
        Public Const S_PROCREF As UShort = &H1125
        Public Const S_LPROCREF As UShort = &H1126

        ' ---- CodeView type leaves (from cvinfo.h) ----
        Public Const LF_MODIFIER As UShort = &H1001
        Public Const LF_POINTER As UShort = &H1002
        Public Const LF_PROCEDURE As UShort = &H1008
        Public Const LF_ARGLIST As UShort = &H1201
        Public Const LF_FIELDLIST As UShort = &H1203
        Public Const LF_ARRAY As UShort = &H1503
        Public Const LF_CLASS As UShort = &H1504
        Public Const LF_STRUCTURE As UShort = &H1505
        Public Const LF_UNION As UShort = &H1506
        Public Const LF_ENUM As UShort = &H1507
        Public Const LF_VTSHAPE As UShort = &H1509
        Public Const LF_FUNC_ID As UShort = &H1601
        Public Const LF_MFUNC_ID As UShort = &H1602
        Public Const LF_BUILDINFO As UShort = &H1603
        Public Const LF_STRING_ID As UShort = &H1605

        ''' <summary>
        ''' A single CodeView record: a 2-byte length (covering <see cref="Type"/> + <see cref="Payload"/>),
        ''' the 2-byte record <see cref="Type"/>, then the payload bytes.
        ''' </summary>
        Public Structure CvRecord
            Public Type As UShort
            Public Payload As Byte()
            ''' <summary>Offset of the 2-byte length field inside the original buffer.</summary>
            Public Offset As Integer
            ''' <summary>Number of bytes consumed (length field + type + payload), 4-byte aligned.</summary>
            Public RecordLength As Integer
        End Structure

        ''' <summary>
        ''' Enumerate length-prefixed CodeView records starting at <paramref name="offset"/> for
        ''' <paramref name="length"/> bytes. Each record is: <c>u16 length; u16 type; payload</c>,
        ''' where <c>length</c> = size of (type + payload). Records are 4-byte aligned.
        ''' </summary>
        Public Iterator Function Enumerate(data As Byte(), offset As Integer, length As Integer) As IEnumerable(Of CvRecord)
            Dim p As Integer = offset
            Dim endPos As Integer = offset + length

            While p + 4 <= endPos
                Dim recLen As Integer = BitConverter.ToUInt16(data, p)

                If recLen < 2 Then
                    Exit While
                End If

                Dim type As UShort = BitConverter.ToUInt16(data, p + 2)
                Dim payloadLen As Integer = recLen - 2
                Dim payloadStart As Integer = p + 4

                If payloadStart + payloadLen > endPos Then
                    Exit While
                End If

                Dim payload(payloadLen - 1) As Byte
                Array.Copy(data, payloadStart, payload, 0, payloadLen)

                Dim rec As New CvRecord With {
                    .Type = type,
                    .Payload = payload,
                    .Offset = p,
                    .RecordLength = recLen + 2
                }
                Yield rec

                p += rec.RecordLength
                p = (p + 3) And Not 3
            End While
        End Function

        ''' <summary>
        ''' Read a null-terminated string from <paramref name="data"/> at <paramref name="offset"/>.
        ''' </summary>
        Public Function ReadNullString(data As Byte(), offset As Integer, encoding As Encoding) As String
            Dim p As Integer = offset

            While p < data.Length AndAlso data(p) <> 0
                p += 1
            End While

            If p <= offset Then
                Return ""
            End If

            Return encoding.GetString(data, offset, p - offset)
        End Function
    End Module
End Namespace
