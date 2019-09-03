Imports Microsoft.VisualBasic.Language

Namespace ASN1

    Module Extensions

        Public Function stringCut(str$, len%) As String
            If (str.Length > len) Then
                str = str.Substring(0, len) & "..."
            End If

            Return str
        End Function
    End Module

    Public Class StreamReader

        Dim enc As Byte()
        Dim pos As i32

        Public ReadOnly Property [get](Optional pos As Integer = -1)
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
    End Class
End Namespace