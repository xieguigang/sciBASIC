#Region "Microsoft.VisualBasic::5bbe3ee72174feaee1338edff3ac068d, www\Microsoft.VisualBasic.NETProtocol\Mailto\MailConfigure.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Structure MailConfigure
    ' 
    '         Properties: Account, GMail, HostAddress, LiveMail, Password
    '                     Port, qiye163, QQMail
    ' 
    '         Function: CreateFromUri, GenerateUri, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language.Default

Namespace Mailto

    ''' <summary>
    ''' The smtp mail server config
    ''' </summary>
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

        Public Shared ReadOnly Property qiye163(account$, password$, Optional SSL As Boolean = True) As MailConfigure
            Get
                Dim smtpSSLPort994 As [Default](Of Integer) = 994

                Return New MailConfigure With {
                    .Account = account,
                    .Password = password,
                    .HostAddress = "smtp.qiye.163.com",
                    .Port = 25 Or smtpSSLPort994.When(SSL)
                }
            End Get
        End Property

        Public Function GenerateUri(Encryption As Func(Of String, String)) As String
            Return String.Format("mailto://{0}:{1}/mail?account={2}%password={3}", HostAddress, Port, Encryption(Account), Encryption(Password))
        End Function

        Public Shared Function CreateFromUri(uri$, decrypt As Func(Of String, String)) As MailConfigure
            Dim addr As String = Regex.Match(uri, "[^/]+?:\d+").Value
            Dim p As Integer = InStr(uri, "/mail?", CompareMethod.Text)
            uri = Mid(uri, p + 6)
            Dim tokens As String() = uri.Split("%"c)
            Dim list = From str As String
                       In tokens
                       Let Key As String = str.Split("="c).First
                       Let value As String = Mid(str, Len(Key) + 2)
                       Select Key, value
            Dim args = list.ToDictionary(
                Function(k) k.Key.ToLower,
                Function(k) k.value)

            tokens = addr.Split(":"c)

            Return New MailConfigure With {
                .HostAddress = tokens(0),
                .Port = CInt(Val(tokens(1))),
                .Account = decrypt(args("account")),
                .Password = decrypt(args("password"))
            }
        End Function
    End Structure
End Namespace
