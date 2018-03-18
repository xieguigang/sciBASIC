Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Scripting.Runtime

    Public Module NarrowingReflection

        ''' <summary>
        ''' 2070 = SpecialName
        ''' </summary>
        Const NarrowingOperator As BindingFlags = BindingFlags.Public Or BindingFlags.Static Or 2070
        Const op_Explicit$ = NameOf(op_Explicit)

        Public Delegate Function INarrowingOperator(Of TIn, TOut)(obj As TIn) As TOut

        Public Function GetNarrowingOperator(Of TIn, TOut)() As INarrowingOperator(Of TIn, TOut)
            ' 函数找不到会返回Nothing
            Dim methods = GetType(TIn).GetMethod(op_Explicit, NarrowingOperator)

            If methods Is Nothing Then
                Return Nothing
            Else
                Dim op_Explicit As INarrowingOperator(Of TIn, TOut) = methods.CreateDelegate(GetType(INarrowingOperator(Of TIn, TOut)))
                Return op_Explicit
            End If
        End Function

        <Extension>
        Public Function GetNarrowingOperator(Of T)(type As Type) As INarrowingOperator(Of Object, T)
            ' 函数找不到会返回Nothing
            Dim methods = type.GetMethod(op_Explicit, NarrowingOperator)

            If methods Is Nothing Then
                Return Nothing
            Else
                Dim op_Explicit As INarrowingOperator(Of Object, T) = Function(obj) DirectCast(methods.Invoke(Nothing, {obj}), T)
                Return op_Explicit
            End If
        End Function
    End Module
End Namespace