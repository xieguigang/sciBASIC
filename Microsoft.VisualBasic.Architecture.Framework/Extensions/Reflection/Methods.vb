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
End Module
