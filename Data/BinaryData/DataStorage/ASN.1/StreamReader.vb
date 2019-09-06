Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

Namespace ASN1

    Module Extensions

        Public Function stringCut(str$, len%) As String
            If (str.Length > len) Then
                str = str.Substring(0, len) & "..."
            End If

            Return str
        End Function

        Public Const hexDigits = "0123456789ABCDEF"
        Public Const b64Safe = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"

        Public ReadOnly reTimeS As New Regex("^(\d\d)(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])([01]\d|2[0-3])(?:([0-5]\d)(?:([0-5]\d)(?:[.,](\d{1,3}))?)?)?(Z|[-+](?:[0]\d|1[0-2])([0-5]\d)?)?$", RegexICMul)
        Public ReadOnly reTimeL As New Regex("^(\d\d\d\d)(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])([01]\d|2[0-3])(?:([0-5]\d)(?:([0-5]\d)(?:[.,](\d{1,3}))?)?)?(Z|[-+](?:[0]\d|1[0-2])([0-5]\d)?)?$", RegexICMul)

        Public Function hexByte(b As Byte) As String
            Return hexDigits((b >> 4) And &HF) & hexDigits(b And &HF)
        End Function
    End Module

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

        Public Function parseStringISO(start%, end%)
            Dim s = ""
            For i As Integer = start To end% - 1
                s &= Chr(Me.get(i))
            Next
            Return s
        End Function

        Function ex(c) ' must be 10xxxxxx
            If ((c < &H80) OrElse (c >= &HC0)) Then
                Throw New Exception("Invalid UTF-8 continuation byte: " & c)
            End If
            Return (c & &H3F)
        End Function

        Function surrogate(cp)
            If (cp < &H10000) Then
                Throw New Exception("UTF-8 overlong encoding, codepoint encoded in 4 bytes: " + cp)
            End If
            ' we could use String.fromCodePoint(cp) but Let's be nice to older browsers and use surrogate pairs
            cp -= &H10000
            Return Chr((cp >> 10) + &HD800) & Chr((cp And &H3FF) + &HDC00)
        End Function

        Public Function parseStringUTF(start%, end%)
            Dim s = ""

            For j As Integer = start To end% - 1
                Dim i As i32 = j
                Dim C = Me.get(++i)

                If (C < &H80) Then  ' 0xxxxxxx (7 bit)
                    s += Chr(C)
                ElseIf (C < &HC0) Then
                    Throw New Exception("Invalid UTF-8 starting byte: " + C)
                ElseIf (C < &HE0) Then ' 110xxxxx 10xxxxxx (11 bit)
                    s += Chr(((C & &H1F) << 6) Or ex(Me.get(++i)))
                ElseIf (C < &HF0) Then ' 1110xxxx 10xxxxxx 10xxxxxx (16 bit)
                    s += Chr(((C & &HF) << 12) Or (ex(Me.get(++i)) << 6) Or ex(Me.get(++i)))
                ElseIf (C < &HF8) Then ' 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx (21 bit)
                    s += surrogate(((C And &H7) << 18) Or (ex(Me.get(++i)) << 12) Or (ex(Me.get(++i)) << 6) Or ex(Me.get(++i)))
                Else
                    Throw New Exception("Invalid UTF-8 starting byte (since 2003 it is restricted to 4 bytes): " + C)
                End If

                j = i
            Next

            Return s
        End Function

        Public Function parseStringBMP(start%, end%)
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

        Public Function parseTime(start%, end%, shortYear As Boolean)
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