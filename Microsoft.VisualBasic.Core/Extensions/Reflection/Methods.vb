#Region "Microsoft.VisualBasic::965a865aa28a8655735ad083bc1825d6, Microsoft.VisualBasic.Core\Extensions\Reflection\Methods.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module MethodsExtension
    ' 
    '     Function: (+2 Overloads) AsLazy, (+2 Overloads) Invoke, (+2 Overloads) TryInvoke
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices

Public Module MethodsExtension

    ''' <summary>
    ''' 尝试将目标对象放入到函数指针之中来运行，运行失败的时候回返回<paramref name="default"/>默认值
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="TOut"></typeparam>
    ''' <param name="input"></param>
    ''' <param name="proc"></param>
    ''' <param name="[default]"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function TryInvoke(Of T, TOut)(proc As Func(Of T, TOut), input As T, Optional [default] As TOut = Nothing) As TOut
        Return New Func(Of TOut)(Function() proc(input)).TryInvoke([default])
    End Function

    <Extension> Public Function TryInvoke(Of T)(proc As Func(Of T), Optional [default] As T = Nothing) As T
        Try
            Return proc()
        Catch ex As Exception
            Call App.LogException(ex)
            Return [default]
        End Try
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsLazy(Of T)(factory As Func(Of T)) As Lazy(Of T)
        Return New Lazy(Of T)(factory)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsLazy(Of T)(lambda As LambdaExpression) As Lazy(Of T)
        Return DirectCast(lambda.Compile, Func(Of T)).AsLazy
    End Function

    ''' <summary>
    ''' Invoke a static method without parameters
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="method"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Invoke(Of T)(method As MethodInfo) As T
        Return method.Invoke(Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Invoke a static method without parameters
    ''' </summary>
    ''' <param name="method"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Invoke(method As MethodInfo) As Object
        Return method.Invoke(Nothing, Nothing)
    End Function
End Module
