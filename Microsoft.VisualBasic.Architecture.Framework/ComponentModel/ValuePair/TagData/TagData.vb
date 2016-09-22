Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.TagData

    Public Class TagData(Of T, V) : Inherits Value(Of V)

        Public Property Tag As T

        Public Overloads Shared Narrowing Operator CType(t As TagData(Of T, V)) As KeyValuePair(Of T, V)
            Return New KeyValuePair(Of T, V)(t.Tag, t.value)
        End Operator
    End Class

    Public Class IntegerTagged(Of T) : Inherits TagData(Of Integer, T)

    End Class

    Public Class DoubleTagged(Of T) : Inherits TagData(Of Double, T)

    End Class

    Public Class LongTagged(Of T) : Inherits TagData(Of Long, T)

    End Class

    Public Class DateTagged(Of T) : Inherits TagData(Of Date, T)

    End Class

    Public Class VectorTagged(Of T) : Inherits TagData(Of Double(), T)

    End Class
End Namespace