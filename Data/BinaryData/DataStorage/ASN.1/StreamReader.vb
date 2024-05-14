#Region "Microsoft.VisualBasic::e577eb24481de28e455573da93359a80, Data\BinaryData\DataStorage\ASN.1\StreamReader.vb"

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

    '   Total Lines: 190
    '    Code Lines: 136
    ' Comment Lines: 18
    '   Blank Lines: 36
    '     File Size: 6.08 KB


    '     Class StreamReader
    ' 
    '         Properties: [get]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ex, hexDump, isASCII, parseStringBMP, parseStringISO
    '                   parseStringUTF, parseTime, surrogate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace ASN1

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/lapo-luchini/asn1js
    ''' </remarks>
    Public Class StreamReader

        Dim enc As Byte()
        Dim pos As i32

        Public ReadOnly Property [get](Optional pos As Integer = -1) As Byte
            Get
                If pos < 0 Then
                    pos = ++pos
                End If
                If pos > enc.Length Then
                    Throw New IndexOutOfRangeException($"Requesting byte offset {pos} on a stream of length {enc.Length}")
                End If

                Return enc(pos)
            End Get
        End Property

        Sub New(enc As [Variant](Of Byte(), String), pos As Integer)
            Me.pos = pos

            If enc Like GetType(String) Then
                Me.enc = enc.TryCast(Of String) _
                    .Select(Function(a) CByte(Asc(a))) _
                    .ToArray
            Else
                Me.enc = enc.TryCast(Of Byte()).ToArray
            End If
        End Sub

        Public Function hexDump(start, [end], raw) As String
            Dim s = ""

            For i As Integer = start To [end] - 1
                s &= hexByte(Me.get(i))
                If raw <> True Then
                    Select Case i And &HF
                        Case &H7 : s &= "  "
                        Case &HF : s &= "\n"
                        Case Else
                            s += " "
                    End Select
                End If
            Next

            Return s
        End Function

        Public Function isASCII(start%, end%) As Boolean
            For i As Integer = start To end% - 1
                Dim c = Me.get(i)

                If (c < 32 OrElse c > 176) Then
                    Return False
                End If
            Next

            Return True
        End Function

        Public Function parseStringISO(start%, end%) As String
            Dim s = ""
            For i As Integer = start To end% - 1
                s &= Chr(Me.get(i))
            Next
            Return s
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="c">must be 10xxxxxx</param>
        ''' <returns></returns>
        Function ex(c As Integer) As String
            If ((c < &H80) OrElse (c >= &HC0)) Then
                Throw New Exception("Invalid UTF-8 continuation byte: " & c)
            End If

            Return (c & &H3F)
        End Function

        Function surrogate(cp As Integer) As String
            If (cp < &H10000) Then
                Throw New Exception("UTF-8 overlong encoding, codepoint encoded in 4 bytes: " + cp)
            End If

            ' we could use String.fromCodePoint(cp) but Let's be nice to older browsers and use surrogate pairs
            cp -= &H10000

            Return Chr((cp >> 10) + &HD800) & Chr((cp And &H3FF) + &HDC00)
        End Function

        Public Function parseStringUTF(start%, end%) As String
            Dim s = ""

            For j As Integer = start To end% - 1
                Dim i As i32 = j
                Dim C = Me.get(++i)

                If (C < &H80) Then
                    ' 0xxxxxxx (7 bit)
                    s += Chr(C)
                ElseIf (C < &HC0) Then
                    Throw New Exception("Invalid UTF-8 starting byte: " + C)
                ElseIf (C < &HE0) Then
                    ' 110xxxxx 10xxxxxx (11 bit)
                    s += Chr(((C & &H1F) << 6) Or ex(Me.get(++i)))
                ElseIf (C < &HF0) Then
                    ' 1110xxxx 10xxxxxx 10xxxxxx (16 bit)
                    s += Chr(((C & &HF) << 12) Or (ex(Me.get(++i)) << 6) Or ex(Me.get(++i)))
                ElseIf (C < &HF8) Then
                    ' 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx (21 bit)
                    s += surrogate(((C And &H7) << 18) Or (ex(Me.get(++i)) << 12) Or (ex(Me.get(++i)) << 6) Or ex(Me.get(++i)))
                Else
                    Throw New Exception("Invalid UTF-8 starting byte (since 2003 it is restricted to 4 bytes): " + C)
                End If

                j = i
            Next

            Return s
        End Function

        Public Function parseStringBMP(start%, end%) As String
            Dim str = ""
            Dim hi, lo

            For j As Integer = start To end% - 1
                Dim i As i32 = j

                hi = Me.get(++i)
                lo = Me.get(++i)
                str += Chr((hi << 8) Or lo)
            Next

            Return str
        End Function

        Public Function parseTime(start%, end%, shortYear As Boolean) As String
            Dim s = parseStringISO(start, end%)
            Dim m = If(shortYear, reTimeS, reTimeL).Matches(s).ToArray

            If Not m.Count = 0 Then
                Return "Unrecognized time: " + s
            End If

            If (shortYear) Then
                ' to avoid querying the timer, use the fixed range [1970, 2069]
                ' it will conform with ITU X.400 [-10, +40] sliding window until 2030
                m(1) = +m(1)
                m(1) += If(m(1) < 70, 2000, 1900)
            End If

            s = m(1) + "-" + m(2) + "-" + m(3) + " " + m(4)

            If (m(5)) Then
                s += ":" + m(5)
                If (m(6)) Then
                    s += ":" + m(6)
                    If (m(7)) Then
                        s += "." + m(7)
                    End If
                End If
            End If

            If (m(8)) Then
                s += " UTC"
                If (m(8) <> "Z") Then
                    s += m(8)
                    If (m(9)) Then
                        s += ":" + m(9)
                    End If
                End If
            End If

            Return s
        End Function

    End Class
End Namespace
