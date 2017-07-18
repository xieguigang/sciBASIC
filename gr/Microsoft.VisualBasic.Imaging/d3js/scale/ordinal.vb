Namespace d3js.scale

    Public Class ordinal

        Public Function domain(values#()) As ordinal

        End Function

        Public Function domain(values$()) As ordinal

        End Function

        Public Function domain(values%()) As ordinal
            Return domain(values.Select(Function(n) CDbl(n)).ToArray)
        End Function

        Public Function rangeBands() As ordinal

        End Function

        Public Function range(Optional values#() = Nothing) As ordinal

        End Function

        Public Shared Narrowing Operator CType(ordinal As ordinal) As Double()

        End Operator
    End Class
End Namespace