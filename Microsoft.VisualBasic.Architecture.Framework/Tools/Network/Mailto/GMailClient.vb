Imports System.Net.Mail
Imports System.Net.Mime
Imports System.Text.RegularExpressions

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
            Dim Mail As MailContents = New MailContents With {.Subject = emailTitle, .Body = emailNote}
            Return SendEMail(Mail, Account.UserName, emailUser)
        End Function

        Public Function SendEMail(MailContents As MailContents, displayName As String, ParamArray Receivers As String()) As Boolean
            Dim SmtpClient As New SmtpClient()
            SmtpClient.Credentials = Me.Account
            SmtpClient.Port = SmtpServerPort
            SmtpClient.Host = SmtpServerHostAddress
            SmtpClient.EnableSsl = True

            Dim MailMessage As MailMessage = MailContents

            MailMessage.From = New MailAddress(Account.UserName, displayName, System.Text.Encoding.UTF8)

            For Each Addr In Receivers
                Call MailMessage.To.Add(Addr)
            Next

            MailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure

            Return __sendMail(SmtpClient, MailMessage)
        End Function


        Private Function __sendMail(SmtpClient As SmtpClient, MailMessage As MailMessage) As Boolean
            Try
                Call SmtpClient.Send(MailMessage)
            Catch ex As Exception
                _ErrMessage = ex.ToString
                Call ex.PrintException
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

        Public Structure MailConfigure

            <Xml.Serialization.XmlAttribute> Public Property Account As String
            <Xml.Serialization.XmlAttribute> Public Property Port As Integer
            <Xml.Serialization.XmlAttribute> Public Property HostAddress As String

            ''' <summary>
            ''' 存储至文件之前请先加密
            ''' </summary>
            ''' <remarks></remarks>
            <Xml.Serialization.XmlText> Public Property Password As String

            Public Overrides Function ToString() As String
                Return $"({Account})  -->  https://{HostAddress}:{Port}/?{Password}"
            End Function

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

            Public Shared ReadOnly Property QQMail(Account As String, Password As String) As MailConfigure
                Get
                    Return New MailConfigure With {
                        .Account = Account,
                        .Password = Password,
                        .Port = 465,
                        .HostAddress = "smtp.qq.com"}
                End Get
            End Property

            Public Shared ReadOnly Property LiveMail(Account As String, Password As String) As MailConfigure
                Get
                    Return New MailConfigure With {
                        .Account = Account,
                        .Password = Password,
                        .Port = 25,
                        .HostAddress = "smtp.live.com"}
                End Get
            End Property

            Public Function GenerateUri(Encryption As Func(Of String, String)) As String
                Return String.Format("mailto://{0}:{1}/mail?account={2}%password={3}", HostAddress, Port, Encryption(Account), Encryption(Password))
            End Function

            Public Shared Function CreateFromUri(uri As String, Decryption As Func(Of String, String)) As MailConfigure
                Dim Addr As String = Regex.Match(uri, "[^/]+?:\d+").Value
                Dim p As Integer = InStr(uri, "/mail?", CompareMethod.Text)
                uri = Mid(uri, p + 6)
                Dim Tokens As String() = uri.Split("%"c)
                Dim Parameters = (From str As String
                                  In Tokens
                                  Let Key As String = str.Split("="c).First
                                  Let value As String = Mid(str, Len(Key) + 2)
                                  Select Key, value).ToArray.ToDictionary(Function(obj) obj.Key.ToLower, elementSelector:=Function(obj) obj.value)
                Tokens = Addr.Split(":"c)
                Return New MailConfigure With {.HostAddress = Tokens(0), .Port = CInt(Val(Tokens(1))), .Account = Decryption(Parameters("account")), .Password = Decryption(Parameters("password"))}
            End Function
        End Structure
    End Class
End Namespace