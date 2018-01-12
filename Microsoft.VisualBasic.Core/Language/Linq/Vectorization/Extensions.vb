Imports System.Runtime.CompilerServices

Namespace Language.Vectorization

    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsVector(booleans As IEnumerable(Of Boolean)) As BooleanVector
            Return New BooleanVector(booleans)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsVector(Of T)(list As IEnumerable(Of T)) As Vector(Of T)
            Return New Vector(Of T)(list)
        End Function
    End Module
End Namespace