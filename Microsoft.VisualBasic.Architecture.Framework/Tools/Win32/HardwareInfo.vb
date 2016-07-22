Namespace Win32

    ''' <summary>
    ''' Get mother board serial numbers and CPU IDs in Visual Basic .NET
    ''' </summary>
    Public Module HardwareInfo

        ''' <summary>
        ''' The following function gets a WMI object and then gets a collection of WMI_BaseBoard objects 
        ''' representing the system's mother boards. It loops through them getting their serial numbers. 
        ''' </summary>
        ''' <returns></returns>
        Public Function SystemSerialNumber() As String
            ' Get the Windows Management Instrumentation object.
            Dim wmi As Object = GetObject("WinMgmts:")
            ' Get the "base boards" (mother boards).
            Dim serial_numbers As New List(Of String)
            Dim mother_boards As Object = wmi.InstancesOf("Win32_BaseBoard")

            For Each board As Object In mother_boards
                serial_numbers += CStr(board.SerialNumber)
            Next

            Dim uid As String = String.Join("-", serial_numbers.ToArray)
            Return uid
        End Function

        ''' <summary>
        ''' The following code gets a WMI object and selects Win32_Processor objects. It loops through them getting their processor IDs. 
        ''' </summary>
        ''' <returns></returns>
        Public Function CpuId() As String
            Dim computer As String = "."
            Dim wmi As Object = GetObject("winmgmts:" & "{impersonationLevel=impersonate}!\\" & computer & "\root\cimv2")
            Dim processors As Object = wmi.ExecQuery("Select * from Win32_Processor")
            Dim cpu_ids As New List(Of String)

            For Each cpu As Object In processors
                cpu_ids += CStr(cpu.ProcessorId)
            Next

            Dim uid As String = String.Join("-", cpu_ids.ToArray)
            Return uid
        End Function
    End Module
End Namespace