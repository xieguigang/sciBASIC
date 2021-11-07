Namespace POSTagger
    Public Class PartOfSpeech
        Public Property Word As String
        Public Property Tag As String

        Private Overloads Function Equals(ByVal other As PartOfSpeech) As Boolean
            Return Equals(Word, other.Word) AndAlso Equals(Tag, other.Tag)
        End Function

        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            If ReferenceEquals(Nothing, obj) Then Return False
            If ReferenceEquals(Me, obj) Then Return True

            Return obj.GetType() Is [GetType]() AndAlso Equals(CType(obj, PartOfSpeech))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return ((If(Not Equals(Word, Nothing), Word.GetHashCode(), 0)) * 397) Xor (If(Not Equals(Tag, Nothing), Tag.GetHashCode(), 0))
        End Function
    End Class
End Namespace
