Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Language

    Public Class StringVector : Inherits Vector(Of String)

        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(buffer As IEnumerable(Of String))
            Me.buffer = buffer.SafeQuery.ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator &(s1 As StringVector, s2$) As StringVector
            Return New StringVector(s1.Select(Function(s) s & s2))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(s1 As StringVector, s2$) As IEnumerable(Of Boolean)
            Return (s1 = s2).Select(Function(b) Not b)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(s1 As StringVector, s2$) As IEnumerable(Of Boolean)
            Return s1.Select(Function(s) s = s2)
        End Operator
    End Class
End Namespace