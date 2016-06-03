Namespace Parallel

    Public Module TimeOutAPI

        ''' <summary>
        ''' The returns value of TRUE represent of the target operation has been time out.(返回真，表示操作超时)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="Handle"></param>
        ''' <param name="Out"></param>
        ''' <param name="TimeOut">The time unit of this parameter is second.(单位为秒)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function OperationTimeOut(Of T, TOut)(Handle As Func(Of T, TOut), [In] As T, ByRef Out As TOut, TimeOut As Double) As Boolean
            Dim ar = Handle.BeginInvoke(arg:=[In], callback:=Nothing, [object]:=Nothing)
            Dim i As Integer

            TimeOut = TimeOut * 1000 / 5

            Do While i < TimeOut

                If ar.IsCompleted Then
                    Out = Handle.EndInvoke(ar)
                    Return False
                End If

                i += 1
                Call Threading.Thread.Sleep(5)
            Loop

            Return True
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="Handle"></param>
        ''' <param name="Out"></param>
        ''' <param name="TimeOut">The time unit of this parameter is second.(单位为秒)</param>
        ''' <returns></returns>
        Public Function OperationTimeOut(Of T)(Handle As Func(Of T), ByRef Out As T, TimeOut As Double) As Boolean
            Dim ar = Handle.BeginInvoke(Nothing, Nothing)
            Dim i As Integer

            TimeOut = TimeOut * 1000 / 5

            Do While i < TimeOut
                If ar.IsCompleted Then
                    Out = Handle.EndInvoke(ar)
                    Return False
                End If

                i += 1
                Call Threading.Thread.Sleep(5)
            Loop

            Return True
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Handle"></param>
        ''' <param name="TimeOut">The time unit of this parameter is second.(单位为秒)</param>
        ''' <returns></returns>
        Public Function OperationTimeOut(Handle As Action, TimeOut As Double) As Boolean
            Dim ar = Handle.BeginInvoke(Nothing, Nothing)
            Dim i As Integer

            TimeOut = TimeOut * 1000 / 5

            Do While i < TimeOut
                If ar.IsCompleted Then
                    Return False
                End If

                i += 1
                Call Threading.Thread.Sleep(5)
            Loop

            Return True
        End Function
    End Module
End Namespace