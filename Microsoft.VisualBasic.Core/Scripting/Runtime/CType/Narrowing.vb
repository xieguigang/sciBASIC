Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Scripting.Runtime

    Public Module NarrowingReflection

        Const NarrowingOperator As BindingFlags = BindingFlags.Public Or BindingFlags.Static

        Public Delegate Function INarrowingOperator(Of T)(obj As Object) As T

        <Extension>
        Public Function GetNarrowingOperator(Of T)(type As Type) As INarrowingOperator(Of T)
            ' 函数找不到会返回Nothing
            Dim methods = type.GetMethod("op_Explict", NarrowingOperator)

            If methods Is Nothing Then
                Return Nothing
            Else
                Dim op_Explict As INarrowingOperator(Of T) = methods.CreateDelegate(GetType(INarrowingOperator(Of T)))
                Return op_Explict
            End If
        End Function
    End Module
End Namespace