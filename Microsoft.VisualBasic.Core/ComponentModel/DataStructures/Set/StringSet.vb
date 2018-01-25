Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataStructures

    Public Class StringSet : Inherits [Set]

        Sub New(strings As IEnumerable(Of String), Optional caseSensitive As CompareMethod = CompareMethod.Binary)
            Call MyBase.New

            Dim compare As StringComparison = caseSensitive.GetCompareType

            MyBase._equals = Function(s1, s2)
                                 Return String.Equals(s1, s2, compare)
                             End Function
            MyBase._members = New [Set](strings, MyBase._equals)._members
        End Sub

        Friend Sub New([set] As [Set])
            Call MyBase.New

            MyBase._members = [set]._members
            MyBase._equals = [set]._equals
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator Or(s1 As StringSet, s2 As IEnumerable(Of String)) As StringSet
            Return New StringSet(DirectCast(s1, [Set]) Or New [Set](s2, s1._equals))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(s As StringSet) As String()
            Return s.ToArray(Of String)
        End Operator
    End Class
End Namespace