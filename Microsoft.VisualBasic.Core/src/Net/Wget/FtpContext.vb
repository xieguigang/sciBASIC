Imports System.Net
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Net.WebClient

    Public Class FtpContext

        Public Property username As String
        Public Property password As String
        Public Property server As String

        Public Function CreateRequest(dir As String) As FtpWebRequest
            Dim ftpContext As String = $"ftp://{server}/{dir}"
#Disable Warning SYSLIB0014 ' 类型或成员已过时
            Dim request As FtpWebRequest = DirectCast(WebRequest.Create(ftpContext), FtpWebRequest)
#Enable Warning SYSLIB0014 ' 类型或成员已过时

            If Not (username.StringEmpty OrElse password.StringEmpty) Then
                request.Credentials = New NetworkCredential(username, password)
            End If

            Return request
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{username Or "anonymous".AsDefault}@ftp://{server}"
        End Function

    End Class
End Namespace