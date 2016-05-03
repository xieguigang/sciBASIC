Public Module OperationTimeOut

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

    ''' <summary>
    ''' 类似于任务管理器的函数：Memory, CPU, ProcessName, PID, CommandLine
    ''' </summary>
    ''' <returns>Memory, CPU</returns>
    ''' <remarks></remarks>
    Public Function ProcessUsageDetails() As List(Of Hashtable)
        Dim CounterList As New List(Of Hashtable)

        Try
            Dim process As Process() = System.Diagnostics.Process.GetProcesses

            For Each P As Process In process
                Dim Table As New Hashtable
                Dim pCounter As PerformanceCounter = New PerformanceCounter("Process", "% Processor Time", P.ProcessName)

                Call Table.Add("Memory", P.WorkingSet64)
                Call Table.Add("CPU", Math.Round(pCounter.NextValue, 2))
                Call Table.Add("ProcessName", P.ProcessName)
                Call Table.Add("PID", P.Id)
                Call Table.Add("CommandLine", P.StartInfo.FileName & " " & P.StartInfo.Arguments)
                Call CounterList.Add(Table)
            Next
        Catch ex As Exception
            Call Console.WriteLine(ex.ToString)
        End Try

        Return CounterList
    End Function

    ''' <summary>
    ''' 获取CPU的使用率
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ProcessUsage() As Double
        Dim Hash = ProcessUsageDetails()
        Dim Usage As Double = (From Process In Hash.AsParallel Select CType(Process("CPU"), Double)).ToArray.Sum
        Return Usage
    End Function
End Module
