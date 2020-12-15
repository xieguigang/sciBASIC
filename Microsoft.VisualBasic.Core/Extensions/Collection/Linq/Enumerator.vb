
Imports System.Runtime.CompilerServices

Namespace Linq

    Friend Class Enumerator(Of T) : Implements IEnumerable(Of T)

        Public Enumeration As Enumeration(Of T)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Return Enumeration.GenericEnumerator
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Enumeration.GetEnumerator
        End Function
    End Class
End Namespace