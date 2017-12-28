Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language.Vectorization

    Public Class StringVector : Inherits Vector(Of String)

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return buffer.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(buffer As IEnumerable(Of String))
            Me.buffer = buffer.SafeQuery.ToArray
        End Sub

        Public Shared Widening Operator CType(list As List(Of String)) As StringVector
            Return New StringVector(list)
        End Operator

        Public Shared Widening Operator CType(array As String()) As StringVector
            Return New StringVector(array)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator &(s1 As StringVector, s2$) As StringVector
            Return New StringVector(s1.Select(Function(s) s & s2))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(s1 As StringVector, s2$) As BooleanVector
            Return Not s1 = s2
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(s1 As StringVector, s2$) As BooleanVector
            Return New BooleanVector(s1.Select(Function(s) s = s2))
        End Operator
    End Class
End Namespace