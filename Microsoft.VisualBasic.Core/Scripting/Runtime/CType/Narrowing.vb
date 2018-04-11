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
            Dim op As MethodInfo = CType(GetType(TIn), TypeInfo).GetOperatorMethod(Of TOut)

            If op Is Nothing Then
                Return Nothing
            Else
                Dim op_Explicit As INarrowingOperator(Of TIn, TOut) = op.CreateDelegate(GetType(INarrowingOperator(Of TIn, TOut)))
                Return op_Explicit
            End If
        End Function

        ''' <summary>
        ''' 直接使用GetMethod方法仍然会出错？？如果目标类型是继承类型，基类型也有一个收缩的操作符的话，会爆出目标不明确的错误
        ''' 
        ''' ```vbnet
        ''' type.GetMethod(op_Explicit, NarrowingOperator)
        ''' ```
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="type"></param>
        ''' <returns>函数找不到会返回Nothing</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function GetOperatorMethod(Of T)(type As TypeInfo) As MethodInfo
            Return type.DeclaredMethods _
                       .Where(Function(m) m.Name = op_Explicit AndAlso m.ReturnType Is GetType(T)) _
                       .FirstOrDefault
        End Function

        <Extension>
        Public Function GetNarrowingOperator(Of T)(type As Type) As INarrowingOperator(Of Object, T)
            ' 函数找不到会返回Nothing
            Dim op As MethodInfo = CType(type, TypeInfo).GetOperatorMethod(Of T)

            If op Is Nothing Then
                Return Nothing
            Else
                Dim op_Explicit As INarrowingOperator(Of Object, T) = Function(obj) DirectCast(op.Invoke(Nothing, {obj}), T)
                Return op_Explicit
            End If
        End Function
    End Module
End Namespace