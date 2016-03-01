Imports System.Text.RegularExpressions
Imports System.Xml.Serialization

Namespace Net.Mailto

    Public Structure MailConfigure

        <XmlAttribute> Public Property Account As String
        <XmlAttribute> Public Property Port As Integer
        <XmlAttribute> Public Property HostAddress As String

        ''' <summary>
        ''' 存储至文件之前请先加密
        ''' </summary>
        ''' <remarks></remarks>
        <XmlText> Public Property Password As String

        Public Overrides Function ToString() As String
            Return $"({Account})  -->  https://{HostAddress}:{Port}/?{Password}"
        End Function

        Public Shared ReadOnly Property GMail(account As String, password As String) As MailConfigure
            Get
                Return New MailConfigure With {
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
                    .HostAddress = "smtp.qq.com"
                }
            End Get
        End Property

        Public Shared ReadOnly Property LiveMail(Account As String, Password As String) As MailConfigure
            Get
                Return New MailConfigure With {
                    .Account = Account,
                    .Password = Password,
                    .Port = 25,
                    .HostAddress = "smtp.live.com"
                }
            End Get
        End Property

        Public Function GenerateUri(Encryption As Func(Of String, String)) As String
            Return String.Format("mailto://{0}:{1}/mail?account={2}%password={3}", HostAddress, Port, Encryption(Account), Encryption(Password))
        End Function

        Public Shared Function CreateFromUri(uri As String, decrypt As Func(Of String, String)) As MailConfigure
            Dim addr As String = Regex.Match(uri, "[^/]+?:\d+").Value
            Dim p As Integer = InStr(uri, "/mail?", CompareMethod.Text)
            uri = Mid(uri, p + 6)
            Dim Tokens As String() = uri.Split("%"c)
            Dim Parameters = (From str As String
                              In Tokens
                              Let Key As String = str.Split("="c).First
                              Let value As String = Mid(str, Len(Key) + 2)
                              Select Key, value).ToDictionary(Function(obj) obj.Key.ToLower,
                                                              Function(obj) obj.value)
            Tokens = addr.Split(":"c)
            Return New MailConfigure With {
                .HostAddress = Tokens(0),
                .Port = CInt(Val(Tokens(1))),
                .Account = decrypt(Parameters("account")),
                .Password = decrypt(Parameters("password"))
            }
        End Function
    End Structure
End Namespace