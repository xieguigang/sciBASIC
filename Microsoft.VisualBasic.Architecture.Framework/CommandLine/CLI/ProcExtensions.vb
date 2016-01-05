Imports System.Runtime.CompilerServices

Namespace CommandLine

    Public Module ProcExtensions

        Public Function GetProc(pid As Integer) As System.Diagnostics.Process
            Return System.Diagnostics.Process.GetProcessById(pid)
        End Function

        ''' <summary>
        ''' 按照命令行参数来获取进程实例
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <returns></returns>
        <Extension> Public Function GetProc(CLI As String) As System.Diagnostics.Process
            Dim CLICompared As CommandLine = CLI
            Dim lstProc As Process() = System.Diagnostics.Process.GetProcesses
            Dim LQuery = (From proc As Process In lstProc
                          Where CLITools.Equals(CLICompared, TryParse(proc.StartInfo.Arguments))  ' 由于参数的顺序可能会有些不一样，所以不可以直接按照字符串比较来获取
                          Select proc).FirstOrDefault
            Return LQuery
        End Function

        ''' <summary>
        ''' 这个主要是为了<see cref="IORedirectFile"/>对象进行相关进程的查找而设置的，
        ''' 对于<see cref="IORedirect"/>而言则直接可以从其属性<see cref="IORedirect.ProcessInfo"/>之中获取相关的进程信息
        ''' </summary>
        ''' <param name="IO"></param>
        ''' <returns></returns>
        Public Function FindProc(IO As IIORedirectAbstract) As System.Diagnostics.Process
            Dim proc As System.Diagnostics.Process = IO.CLIArguments.GetProc
            If proc Is Nothing Then '空值说明进程还没有启动或者已经终止了，所以查找将不会查找到进程的信息
                Call $"Unable to found associated process {IO.ToString}, it maybe haven't been started or already terminated.".__DEBUG_ECHO
            End If

            Return proc
        End Function
    End Module
End Namespace