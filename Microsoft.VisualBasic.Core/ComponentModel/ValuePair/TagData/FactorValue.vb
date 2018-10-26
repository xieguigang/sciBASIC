Namespace ComponentModel.TagData

    Public Class FactorValue(Of T As {Structure, IComparable(Of T)}, V)

        Public Property Factor As T
        Public Property Value As V

    End Class

    Public Class FactorString(Of T As {Structure, IComparable(Of T)})

        Public Property Factor As T
        Public Property text As String

    End Class
End Namespace