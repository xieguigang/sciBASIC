Namespace Win32

    ''' <summary>
    ''' Windows的任务管理器的接口
    ''' </summary>
    Public Module TaskManager

        ''' <summary>
        ''' Using this property you can display the CPU usage (over all CPU usage like you would find on the task manager)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CPU_Usages As New PerformanceCounter("Processor", "% Processor Time", "_Total")

        ''' <summary>
        ''' 类似于任务管理器的函数：Memory, CPU, ProcessName, PID, CommandLine
        ''' </summary>
        ''' <returns>Memory, CPU</returns>
        ''' <remarks></remarks>
        Public Function ProcessUsageDetails() As List(Of Hashtable)
            Dim counterList As New List(Of Hashtable)

            Try
                Dim process As Process() = System.Diagnostics.Process.GetProcesses

                For Each P As Process In process
                    Dim Table As New Hashtable
                    Dim pCounter As New PerformanceCounter("Process", "% Processor Time", P.ProcessName)

                    Call Table.Add("Memory", P.WorkingSet64)
                    Call Table.Add("CPU", Math.Round(pCounter.NextValue, 2))
                    Call Table.Add("ProcessName", P.ProcessName)
                    Call Table.Add("PID", P.Id)
                    Call Table.Add("CommandLine", P.StartInfo.FileName & " " & P.StartInfo.Arguments)
                    Call counterList.Add(Table)
                Next
            Catch ex As Exception
                Call App.LogException(ex)
                Call ex.PrintException
            End Try

            Return counterList
        End Function

        ''' <summary>
        ''' 获取CPU的使用率
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ProcessUsage() As Double
            Dim Hash = ProcessUsageDetails()
            Dim Usage As Double = (From Process In Hash.AsParallel Select CType(Process("CPU"), Double)).Sum
            Return Usage
        End Function
    End Module
End Namespace
