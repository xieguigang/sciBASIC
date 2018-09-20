Imports System.Runtime.CompilerServices

Namespace ComponentModel.Ranges

    Public Module SelectorExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function OrderSelector(Of T As IComparable)(src As IEnumerable(Of T), Optional asc As Boolean = True) As OrderSelector(Of T)
            Return New OrderSelector(Of T)(src, asc)
        End Function
    End Module
End Namespace