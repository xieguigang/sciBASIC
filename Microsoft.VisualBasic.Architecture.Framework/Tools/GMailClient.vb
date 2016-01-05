Imports System.Net.Mail
Imports System.Net.Mime

Namespace DotNET_Wrapper.Tools

    ''' <summary>
    ''' A client of gmail.com
    ''' </summary>
    ''' <remarks></remarks>
    Public Class EMailClient

        Dim Account As System.Net.NetworkCredential
        Dim SmtpServerPort As Integer
        Dim SmtpServerHostAddress As String

        Sub New(Account As String, Password As String, Port As Integer, HostAddress As String)
            Me.Account = New Net.NetworkCredential(userName:=Account, password:=Password)
            Me.SmtpServerPort = Port
            Me.SmtpServerHostAddress = HostAddress
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="emailTitle">标题</param>
        ''' <param name="emailNote">内容</param>
        ''' <param name="emailUser">收件人地址</param>
        ''' <remarks></remarks>
        Public Sub SendMessagesTo(emailTitle As String, emailNote As String, emailUser As String)
            Dim Mail As MailContents = New MailContents With {.Subject = emailTitle, .Body = emailNote}
            Call SendEMail(Mail, Account.UserName, emailUser)
        End Sub

        Public Function SendEMail(MailContents As MailContents, displayName As String, ParamArray Receivers As String()) As Boolean
            Dim SmtpServer As New SmtpClient()
            SmtpServer.Credentials = Me.Account
            SmtpServer.Port = SmtpServerPort
            SmtpServer.Host = SmtpServerHostAddress
            SmtpServer.EnableSsl = True

            Dim MailMessage As MailMessage = MailContents

            Try
                MailMessage.From = New MailAddress(Account.UserName, displayName, System.Text.Encoding.UTF8)

                For Each Addr In Receivers
                    MailMessage.To.Add(Addr)
                Next

                MailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
                Call SmtpServer.Send(MailMessage)

                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Gmail
        ''' </summary>
        ''' <param name="Account"></param>
        ''' <param name="Password"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GmailClient(Account As String, Password As String) As EMailClient
            Return New EMailClient(Account, Password, Port:=587, HostAddress:="smtp.gmail.com")
        End Function

        Public Structure MailConfigure

            <Xml.Serialization.XmlAttribute> Dim Account As String
            <Xml.Serialization.XmlAttribute> Dim Port As Integer
            <Xml.Serialization.XmlAttribute> Dim HostAddress As String

            ''' <summary>
            ''' 存储至文件之前请先加密
            ''' </summary>
            ''' <remarks></remarks>
            <Xml.Serialization.XmlText> Dim Password As String

            Public Shared ReadOnly Property GMail(account As String, password As String) As MailConfigure
                Get
                    Return New MailConfigure With
                           {
                               .Account = account,
                               .HostAddress = "smtp.gmail.com",
                               .Port = 587,
                               .Password = password
                           }
                End Get
            End Property
        End Structure

        Public Structure MailContents

            Const MessageHtml As String = "<html><body><table border=2><tr width=100%><td><img src=cid:Logo alt=companyname /></td><td>MY COMPANY DESCRIPTION</td></tr></table><hr/></body></html>"

            Dim Subject As String
            Dim Body As String

            ''' <summary>
            ''' The path list of the attachments file.
            ''' </summary>
            ''' <remarks></remarks>
            Dim Attatchments As String()
            ''' <summary>
            ''' The file path of the logo image.
            ''' </summary>
            ''' <remarks></remarks>
            Dim Logo As String

            Public Shared Narrowing Operator CType(e As MailContents) As MailMessage
                Dim NewMailMessage As MailMessage = New MailMessage
                Dim AlternateView As AlternateView

                NewMailMessage.Subject = e.Subject
                NewMailMessage.Body = e.Body

                For Each FilePath As String In e.Attatchments
                    NewMailMessage.Attachments.Add(New Attachment(fileName:=FilePath))
                Next

                If Len(e.Body) = 0 Then e.Body = String.Empty
                If Len(e.Logo) > 0 Then
                    Dim Logo As New LinkedResource(e.Logo)
                    Logo.ContentId = "Logo"
                    AlternateView = AlternateView.CreateAlternateViewFromString(MessageHtml & e.Body, Nothing, MediaTypeNames.Text.Html)
                    AlternateView.LinkedResources.Add(Logo)
                Else
                    AlternateView = AlternateView.CreateAlternateViewFromString(e.Body, Nothing, MediaTypeNames.Text.Html)
                End If

                NewMailMessage.AlternateViews.Add(AlternateView)
                NewMailMessage.IsBodyHtml = True

                Return NewMailMessage
            End Operator

            Public Overrides Function ToString() As String
                Return String.Format("{0} -- {1}", Subject, Body)
            End Function
        End Structure
    End Class
End Namespace