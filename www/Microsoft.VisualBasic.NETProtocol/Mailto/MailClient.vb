#Region "Microsoft.VisualBasic::2c122848f5dc01b16d812e37fcae6887, www\Microsoft.VisualBasic.NETProtocol\Mailto\MailClient.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 108
    '    Code Lines: 70 (64.81%)
    ' Comment Lines: 19 (17.59%)
    '    - Xml Docs: 89.47%
    ' 
    '   Blank Lines: 19 (17.59%)
    '     File Size: 3.60 KB


    '     Class EMailClient
    ' 
    '         Properties: ErrMessage
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: __sendMail, GmailClient, QQMail, SendEMail, SendMessagesTo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net
Imports System.Net.Mail
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Mailto

    ''' <summary>
    ''' A client for send E-mail
    ''' </summary>
    ''' <remarks></remarks>
    Public Class EMailClient

        Dim account As NetworkCredential
        Dim smtpPort%
        Dim smtpHost$

        Public ReadOnly Property ErrMessage As String

        Sub New(account$, password$, port%, host$)
            Me.account = New NetworkCredential(userName:=account, password:=password)
            Me.smtpPort = port
            Me.smtpHost = host
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(config As MailConfigure)
            Call Me.New(
                account:=config.Account,
                password:=config.Password,
                port:=config.Port,
                host:=config.HostAddress
            )
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="emailTitle">标题</param>
        ''' <param name="emailNote">内容</param>
        ''' <param name="emailUser">收件人地址</param>
        ''' <remarks></remarks>
        Public Function SendMessagesTo(emailTitle As String, emailNote As String, emailUser As String) As Boolean
            Dim Mail As New MailContents With {
                .Subject = emailTitle,
                .Body = emailNote
            }
            Return SendEMail(Mail, account.UserName, emailUser)
        End Function

        Public Function SendEMail(mail As MailContents, displayName$, ParamArray receivers$()) As Boolean
            Dim smtp As New SmtpClient With {
                .Credentials = Me.account,
                .Port = smtpPort,
                .Host = smtpHost,
                .EnableSsl = True
            }
            Dim msg As MailMessage = mail

            msg.From = New MailAddress(account.UserName, displayName, Encoding.UTF8)

            For Each addr As String In receivers
                Call msg.To.Add(addr)
            Next

            msg.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure

            Return __sendMail(smtp, msg)
        End Function

        Private Function __sendMail(smtp As SmtpClient, mail As MailMessage) As Boolean
            Try
                Call smtp.Send(mail)
            Catch ex As Exception
                ex = New Exception(smtp.GetJson, ex)
                ex = New Exception(mail.GetJson, ex)

                _ErrMessage = ex.ToString

                Call ex.PrintException
                Call App.LogException(ex)

                Return False
            End Try

            Return True
        End Function

        ''' <summary>
        ''' Gmail
        ''' </summary>
        ''' <param name="account"></param>
        ''' <param name="pass"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GmailClient(account As String, pass As String) As EMailClient
            Return New EMailClient(MailConfigure.GMail(account, pass))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function QQMail(account As String, pass As String) As EMailClient
            Return New EMailClient(MailConfigure.QQMail(account, pass))
        End Function
    End Class
End Namespace
