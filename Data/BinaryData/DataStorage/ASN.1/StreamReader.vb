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

        Public Function hexByte(b As Byte) As String
            Return hexDigits((b >> 4) And &HF) & hexDigits(b And &HF)
        End Function
    End Module

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
    End Class
End Namespace