Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Linq

Namespace Scripting

    Public Class ExternalCall

        ''' <summary>
        ''' 脚本宿主的可执行文件的路径
        ''' </summary>
        ReadOnly __host As String
        ReadOnly __ext As String

        Sub New(host As String, Optional ext As String = ".txt")
            __host = FileIO.FileSystem.GetFileInfo(host).FullName
            __ext = ext
        End Sub

        Public Function Run(script As String, Optional args As Specialized.NameValueCollection = Nothing) As ShellValue
            Dim tmp As String = App.GetAppSysTempFile(__ext)
            Call script.SaveTo(tmp, Encodings.ASCII.GetEncodings)
            Return Shell(path:=tmp, args:=args)
        End Function

        Public Function Shell(path As String, Optional args As Specialized.NameValueCollection = Nothing) As ShellValue
            Dim param As String =
                If(args Is Nothing,
                   "",
                   String.Join(" ", args.AllKeys.ToArray(Function(s) $"{s} {args.Get(s).CliToken}")))
            Dim IO As New IORedirect(__host, path & " " & param)
            Dim code As Integer = IO.Start(WaitForExit:=True)
            Return New ShellValue(IO, code)
        End Function

        Public Overrides Function ToString() As String
            Return __host
        End Function
    End Class

    Public Structure ShellValue
        Public STD_OUT As String
        Public STD_ERR As String
        Public state As Integer

        Sub New(io As IORedirect, exitCode As Integer)
            state = exitCode
            STD_OUT = io.StandardOutput
            STD_ERR = io.GetError
        End Sub

        Public Function GetObject(Of T)(parser As Func(Of String, T)) As T
            Return parser(STD_OUT)
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace