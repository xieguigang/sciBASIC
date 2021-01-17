Namespace Language

    Public Class chr

        ReadOnly [char] As Char

        Sub New([char] As Char)
            Me.char = [char]
        End Sub

        Sub New(i32 As Integer)
            Me.char = ChrW(i32)
        End Sub

        Public Overrides Function ToString() As String
            Return [char]
        End Function

        Public Shared Widening Operator CType(c As Char) As chr
            Return New chr(c)
        End Operator

        Public Shared Widening Operator CType(i As Integer) As chr
            Return New chr(i)
        End Operator

        Public Shared Operator -(c As chr, w As Char) As chr
            Return New chr(AscW(c.char) - AscW(w))
        End Operator

        Public Shared Operator +(c As chr, i As Integer) As chr
            Return New chr(AscW(c.char) + i)
        End Operator

        Public Shared Operator <(c As chr, i As Integer) As Boolean
            Return AscW(c.char) < i
        End Operator

        Public Shared Operator >(c As chr, i As Integer) As Boolean
            Return AscW(c.char) > i
        End Operator

        Public Shared Operator <=(c As chr, i As Integer) As Boolean
            Return AscW(c.char) <= i
        End Operator

        Public Shared Operator >=(c As chr, i As Integer) As Boolean
            Return AscW(c.char) >= i
        End Operator

        Public Shared Operator <=(c As chr, w As Char) As Boolean
            Return c.char <= w
        End Operator

        Public Shared Operator >=(c As chr, w As Char) As Boolean
            Return c.char >= w
        End Operator

        Public Shared Operator >>(c As chr, i As Integer) As Integer
            Return AscW(c.char) >> i
        End Operator

        Public Shared Narrowing Operator CType(c As chr) As Integer
            Return AscW(c.char)
        End Operator

        Public Shared Narrowing Operator CType(c As chr) As Char
            Return c.char
        End Operator
    End Class
End Namespace