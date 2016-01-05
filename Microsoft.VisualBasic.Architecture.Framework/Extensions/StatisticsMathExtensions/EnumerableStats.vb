Imports System.Diagnostics
Imports System.Collections.Generic
Imports System.Linq

Namespace StatisticsMathExtensions

    Public Module EnumerableStats

        <System.Runtime.CompilerServices.Extension> _
        Public Function Coalesce(Of T As Structure)(source As IEnumerable(Of System.Nullable(Of T))) As IEnumerable(Of T)
            Debug.Assert(source IsNot Nothing)
            Return source.Where(Function(x) x.HasValue).[Select](Function(x) CType(x, T))
        End Function
    End Module
End Namespace