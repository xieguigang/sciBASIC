Namespace Script

    ''' <summary>
    ''' 一个被 <c>#include</c> 引入的 VB 脚本。
    ''' </summary>
    Public Class IncludedScript

        ''' <summary>被引入脚本的绝对路径</summary>
        Public Property FilePath As String

        ''' <summary>声明该 include 的脚本文件绝对路径</summary>
        Public Property DeclaredIn As String

        Public Overrides Function ToString() As String
            Return FilePath
        End Function
    End Class
End Namespace
