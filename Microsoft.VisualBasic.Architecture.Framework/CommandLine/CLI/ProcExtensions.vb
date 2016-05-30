Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace CommandLine

    ''' <summary>
    ''' How to found the process by CLI
    ''' </summary>
    Public Module ProcExtensions

        Public Function GetProc(pid As Integer) As Process
            Return System.Diagnostics.Process.GetProcessById(pid)
        End Function

        ''' <summary>
        ''' Get process by command line parameter.(按照命令行参数来获取进程实例)
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <returns></returns>
        <Extension> Public Function GetProc(CLI As String) As Process
            Dim CLICompared As CommandLine = CLI
            Dim lstProc As Process() = System.Diagnostics.Process.GetProcesses
            Dim LQuery As Process =
                LinqAPI.DefaultFirst(Of Process) <= From proc As Process
                                                    In lstProc
                                                    Let args = TryParse(proc.StartInfo.Arguments)
                                                    Where CLITools.Equals(CLICompared, args)  ' 由于参数的顺序可能会有些不一样，所以不可以直接按照字符串比较来获取
                                                    Select proc
            Return LQuery
        End Function

        ''' <summary>
        ''' 这个主要是为了<see cref="IORedirectFile"/>对象进行相关进程的查找而设置的，
        ''' 对于<see cref="IORedirect"/>而言则直接可以从其属性<see cref="IORedirect.ProcessInfo"/>之中获取相关的进程信息
        ''' </summary>
        ''' <param name="IO"></param>
        ''' <returns></returns>
        Public Function FindProc(IO As IIORedirectAbstract) As Process
            Dim proc As System.Diagnostics.Process = IO.CLIArguments.GetProc
            If proc Is Nothing Then '空值说明进程还没有启动或者已经终止了，所以查找将不会查找到进程的信息
                Dim msg As String = String.Format(NoProcessFound, IO.ToString)
                Call VBDebugger.Warning(msg)
            End If

            Return proc
        End Function

        Const NoProcessFound As String = "Unable to found associated process {0}, it maybe haven't been started or already terminated."
    End Module
End Namespace