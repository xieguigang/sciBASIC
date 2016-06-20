Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Parallel.Tasks

Namespace Parallel

    Public Module TimeOutAPI

        ''' <summary>
        ''' The returns value of TRUE represent of the target operation has been time out.(返回真，表示操作超时)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="handle"></param>
        ''' <param name="Out"></param>
        ''' <param name="TimeOut">The time unit of this parameter is second.(单位为秒)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension>
        Public Function OperationTimeOut(Of T, TOut)(handle As Func(Of T, TOut), [In] As T, ByRef Out As TOut, TimeOut As Double) As Boolean
            Dim invoke As New __backgroundTask(Of TOut)(Function() handle([In]))
            Dim i As Integer

            TimeOut = TimeOut * 1000
            invoke.Start()

            Do While i < TimeOut
                If invoke.TaskComplete Then
                    Out = invoke.Value
                    Return False
                End If

                i += 1
                Call Threading.Thread.Sleep(1)
            Loop

            Call invoke.Abort()

            Return True
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="handle"></param>
        ''' <param name="Out"></param>
        ''' <param name="TimeOut">The time unit of this parameter is second.(单位为秒)</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function OperationTimeOut(Of T)(handle As Func(Of T), ByRef Out As T, TimeOut As Double) As Boolean
            Return OperationTimeOut(Of Boolean, T)(Function(b) handle(), True, Out, TimeOut)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="handle"></param>
        ''' <param name="TimeOut">The time unit of this parameter is second.(单位为秒)</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function OperationTimeOut(handle As Action, TimeOut As Double) As Boolean
            Dim invoke As Func(Of Boolean, Boolean) =
                Function(b) As Boolean
                    Call handle()
                    Return True
                End Function
            Return OperationTimeOut(invoke, True, True, TimeOut)
        End Function
    End Module
End Namespace