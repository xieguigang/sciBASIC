Imports Microsoft.VisualBasic.Linq

Namespace Language

    Public Class Uid

        Dim chars As List(Of Integer)

        ReadOnly __chars As Char() = "0123456789abcdefghijklmnopqrstuvwxyzZBCDEFGHIJKLMNOPQRSTUVWXYZ"
        ReadOnly __upbound As Integer = __chars.Length - 1

        Sub New(n As Integer)
            chars += -1

            For i As Integer = 0 To n - 1
                Call __plus(chars.Count - 1)
            Next
        End Sub

        Sub New(i As Uid)
            chars = New List(Of Integer)(i.chars)
        End Sub

        Sub New()
            Call Me.New(Scan0)
        End Sub

        Private Function __plus(l As Integer) As Integer
            Dim n As Integer = chars(l) + 1
            Dim move As Integer = 0

            If n > __upbound Then
                n = 0
                Dim pl = l - 1

                If pl < 0 Then
                    Call chars.Insert(0, 1)
                    l += 1
                    move = 1
                Else
                    l += __plus(pl)
                End If
            End If

            chars(l) = n

            Return move
        End Function

        Public Function Plus() As String
            Call __plus(chars.Count - 1)
            Return ToString()
        End Function

        Public Shared Operator +(i As Uid, n As Integer) As Uid
            For o As Integer = 0 To n - 1
                Call i.__plus(i.chars.Count - 1)
            Next

            Return i
        End Operator

        Public Shared Operator +(i As Uid) As Uid
            Call i.__plus(i.chars.Count - 1)
            Return i
        End Operator

        Public Overrides Function ToString() As String
            Return New String(
                chars.ToArray(Function(x) __chars(x)))
        End Function

        Public Shared Narrowing Operator CType(i As Uid) As String
            Return i.ToString
        End Operator
    End Class
End Namespace