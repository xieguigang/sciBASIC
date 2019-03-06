Namespace Language

    Public Class CharStream : Implements IEnumerable(Of Char)

        Dim chars As New List(Of Char)

        Public Sub Add(c As Char)
            chars.Add(c)
        End Sub

        Public Shared Narrowing Operator CType(chars As CharStream) As SByte()
            Return chars.Select(Function(c) CSByte(AscW(c))).ToArray
        End Operator

        Public Shared Narrowing Operator CType(chars As CharStream) As String
            Return New String(chars.ToArray)
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of Char) Implements IEnumerable(Of Char).GetEnumerator
            For Each c As Char In chars
                Yield c
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace