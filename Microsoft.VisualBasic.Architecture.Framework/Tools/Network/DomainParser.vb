Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Net

    Public Structure DomainName : Implements IKeyValuePairObject(Of String, String)

        Public Property Domain As String Implements IKeyValuePairObject(Of String, String).Identifier
        ''' <summary>
        ''' 顶级域名
        ''' </summary>
        ''' <returns></returns>
        Public Property TLD As String Implements IKeyValuePairObject(Of String, String).Value

        Sub New(url As String)
            url = TryParse(url)
            Dim Tokens As String() = url.Split(CChar("."))
            Domain = Tokens(0)
            TLD = Tokens.Skip(1).JoinBy(".")
        End Sub

        Public ReadOnly Property Invalid As Boolean
            Get
                Return (String.IsNullOrEmpty(Domain) OrElse String.IsNullOrEmpty(TLD))
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Domain}.{TLD}"
        End Function
    End Structure

    ''' <summary>
    ''' http://sub.domain.com/somefolder/index.html -> domain.com
    ''' somedomain.info -> somedomain.info
    ''' http://anotherdomain.org/home -> anotherdomain.org
    ''' www.subdomain.anothersubdomain.maindomain.com/something/ -> maindomain.com
    ''' </summary>
    Public Module DomainParser

        ''' <summary>
        ''' 解析错误会返回空字符串
        ''' </summary>
        ''' <param name="url"></param>
        ''' <returns></returns>
        Public Function TryParse(url As String) As String
            url = Trim(url)
            url = TrimPathAndQuery(url)
            Return url
        End Function

        Public Function TryParse(url As String, ByRef DomainName As DomainName) As Boolean
            If String.IsNullOrEmpty(TryParse(url).ShadowCopy(url)) Then
                Return False
            End If

            DomainName = New DomainName(url)
            Return True
        End Function

        Private Function TrimPathAndQuery(url As String) As String
            url = url.Split(CChar("/")).First
            Dim Tokens = url.Split(CChar("."))
            If Tokens.Length = 2 Then
                Return url
            ElseIf Tokens.Length = 1 Then
                Return ""
            ElseIf Tokens.Length = 3 Then
                Dim tld2 As String = Tokens(1)

                If InStr("com|org|net|edu", tld2, CompareMethod.Text) > 0 Then
                    Return url
                End If
            Else
                Dim d As Integer = Tokens.Length - 2
                Tokens = Tokens.Skip(d).ToArray
            End If

            Return $"{Tokens(0)}.{Tokens(1)}"
        End Function

        Private Function Trim(url As String) As String

            For Each protocol As String In {"http://", "file://", "https://", "ftp://"}
                If InStr(url, protocol, CompareMethod.Text) = 1 Then
                    url = Mid(url, Len(protocol) + 1)
                    Return url
                End If
            Next

            If InStr(url, "mailto://", CompareMethod.Text) = 1 Then
                url = url.Split("@"c).Last
            End If

            Return url
        End Function
    End Module
End Namespace