Imports Microsoft.VisualBasic.Serialization

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

        Public Function Run(script As String) As ShellValue
            Dim tmp As String = App.GetAppSysTempFile(__ext)
            Call script.SaveTo(tmp, Encodings.ASCII.GetEncodings)
            Return Shell(path:=tmp)
        End Function

        Public Function Shell(path As String) As ShellValue

        End Function

        Public Overrides Function ToString() As String
            Return __host
        End Function
    End Class

    Public Structure ShellValue
        Public STD_OUT As String
        Public STD_ERR As String
        Public state As Integer

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace