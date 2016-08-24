#Region "Microsoft.VisualBasic::4ba0f75dfa7478eeff942c0250057611, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Tools\Network\Mailto\GMailClient.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Net.Mail
Imports System.Net.Mime
Imports System.Xml.Serialization
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Net.Mailto

    ''' <summary>
    ''' A client of gmail.com
    ''' </summary>
    ''' <remarks></remarks>
    Public Class EMailClient

        Dim Account As System.Net.NetworkCredential
        Dim SmtpServerPort As Integer
        Dim SmtpServerHostAddress As String

        Sub New(Account As String, Password As String, Port As Integer, HostAddress As String)
            Me.Account = New System.Net.NetworkCredential(userName:=Account, password:=Password)
            Me.SmtpServerPort = Port
            Me.SmtpServerHostAddress = HostAddress
        End Sub

        Sub New(Config As MailConfigure)
            Account = New System.Net.NetworkCredential(userName:=Config.Account, password:=Config.Password)
            SmtpServerHostAddress = Config.HostAddress
            SmtpServerPort = Config.Port
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="emailTitle">标题</param>
        ''' <param name="emailNote">内容</param>
        ''' <param name="emailUser">收件人地址</param>
        ''' <remarks></remarks>
        Public Function SendMessagesTo(emailTitle As String, emailNote As String, emailUser As String) As Boolean
            Dim Mail As MailContents = New MailContents With {
                .Subject = emailTitle,
                .Body = emailNote
            }
            Return SendEMail(Mail, Account.UserName, emailUser)
        End Function

        Public Function SendEMail(MailContents As MailContents, displayName As String, ParamArray Receivers As String()) As Boolean
            Dim SmtpClient As New SmtpClient()
            SmtpClient.Credentials = Me.Account
            SmtpClient.Port = SmtpServerPort
            SmtpClient.Host = SmtpServerHostAddress
            SmtpClient.EnableSsl = True

            Dim msg As MailMessage = MailContents

            msg.From = New MailAddress(Account.UserName, displayName, System.Text.Encoding.UTF8)

            For Each addr As String In Receivers
                Call msg.To.Add(addr)
            Next

            msg.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure

            Return __sendMail(SmtpClient, msg)
        End Function

        Private Function __sendMail(SmtpClient As SmtpClient, MailMessage As MailMessage) As Boolean
            Try
                Call SmtpClient.Send(MailMessage)
            Catch ex As Exception
                ex = New Exception(SmtpClient.GetJson, ex)
                ex = New Exception(MailMessage.GetJson, ex)

                _ErrMessage = ex.ToString

                Call ex.PrintException
                Call App.LogException(ex)

                Return False
            End Try

            Return True
        End Function

        Public ReadOnly Property ErrMessage As String

        ''' <summary>
        ''' Gmail
        ''' </summary>
        ''' <param name="account"></param>
        ''' <param name="pass"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GmailClient(account As String, pass As String) As EMailClient
            Return New EMailClient(account, pass, Port:=587, HostAddress:="smtp.gmail.com")
        End Function

        Public Shared Function QQMail(account As String, pass As String) As EMailClient
            Return New EMailClient(account, pass, Port:=587, HostAddress:="smtp.qq.com")
        End Function
    End Class
End Namespace
