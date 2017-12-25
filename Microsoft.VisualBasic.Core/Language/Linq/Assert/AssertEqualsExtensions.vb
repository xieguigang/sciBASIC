Imports System.Runtime.CompilerServices

Namespace Language

    Public Module AssertEqualsExtensions

        <Extension>
        Public Function All(Of T)(vector As Vector(Of T)) As AssertAll(Of T)
            Return New AssertAll(Of T)(vector, Function(x, y) x = y)
        End Function

        <Extension>
        Public Function Any(Of T)(vector As Vector(Of T)) As AssertAny(Of T)
            Return New AssertAny(Of T)(vector, Function(x, y) x = y)
        End Function
    End Module
End Namespace