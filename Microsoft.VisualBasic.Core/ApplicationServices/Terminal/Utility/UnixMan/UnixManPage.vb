Imports System.Text

Namespace ApplicationServices.Terminal.Utility

    Public Class Index

        Public Property index As String
        Public Property category As Integer
        Public Property [date] As Date = Now
        Public Property keyword As String
        Public Property title As String

        Public Overrides Function ToString() As String
            Return $".TH {Strings.UCase(index)} {category} {[date].Year}-{[date].Month.ToString.PadLeft(2, "0"c)}-{[date].Day.ToString.PadLeft(2, "0")} ""{keyword}"" ""{title}"""
        End Function

    End Class

    ''' <summary>
    ''' man手册页（manual pages，“手册”），是类UNIX系统最重要的手册工具。多数类UNIX都预装了它，这也包括Arch。使用man手册页的命令是：man。
    ''' </summary>
    Public Class UnixManPage

        Public Property index As Index

        Public Property PROLOG As String

        ''' <summary>
        ''' 手册叙述对象名称，及简要描述。
        ''' </summary>
        ''' <returns></returns>
        Public Property NAME As String
        ''' <summary>
        ''' 命令参数格式，或者函数调用格式等。
        ''' </summary>
        ''' <returns></returns>
        Public Property SYNOPSIS As String
        ''' <summary>
        ''' 对叙述对象更加详细的描述。
        ''' </summary>
        ''' <returns></returns>
        Public Property DESCRIPTION As String
        ''' <summary>
        ''' 由浅入深的使用示例。
        ''' </summary>
        ''' <returns></returns>
        Public Property EXAMPLES As String
        ''' <summary>
        ''' 命令行或者函数调用参数的意义。
        ''' </summary>
        ''' <returns></returns>
        Public Property OPTIONS As String
        ''' <summary>
        ''' 不同返回（退出）代码的含义。
        ''' </summary>
        ''' <returns></returns>
        Public Property EXIT_STATUS As String
        ''' <summary>
        ''' 与叙述对象相关的文件。
        ''' </summary>
        ''' <returns></returns>
        Public Property FILES As String
        ''' <summary>
        ''' 已知的bug。
        ''' </summary>
        ''' <returns></returns>
        Public Property BUGS As String
        ''' <summary>
        ''' 相关内容列表。
        ''' </summary>
        ''' <returns></returns>
        Public Property SEE_ALSO As String
        Public Property AUTHOR As String
        Public Property HISTORY As String
        Public Property COPYRIGHT As String
        Public Property LICENSE As String
        Public Property WARRANTY As String

        ''' <summary>
        ''' 生成man page的文本内容
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return ToString(Me, Nothing)
        End Function

        Public Overloads Shared Function ToString(man As UnixManPage, Optional comments$ = Nothing) As String
            Dim text As New StringBuilder

            If Not comments.StringEmpty Then
                For Each line As String In comments.LineTokens
                    Call text.AppendLine($".\"" {line}")
                Next
            End If

            Call text.AppendLine(man.index.ToString)

            Return text.ToString
        End Function
    End Class
End Namespace