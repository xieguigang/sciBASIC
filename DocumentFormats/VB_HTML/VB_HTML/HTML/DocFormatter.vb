Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace HTML

    ''' <summary>
    ''' Module provides some method for text document
    ''' </summary>
    <PackageNamespace("Doc.Formatter",
                  Category:=APICategories.UtilityTools,
                  Publisher:="xie.guigang@live.com")>
    Public Module DocFormatter

        ''' <summary>
        ''' High light all of the links in the text document automatically.
        ''' </summary>
        ''' <param name="s">Assuming that the input text is plant text.</param>
        ''' <returns></returns>
        <ExportAPI("Links.Highlights")>
        Public Function HighlightLinks(s As String) As String
            Return HighlightEMail(HighlightURL(s))
        End Function

        ''' <summary>
        ''' Highligh links in the text.(将文档里面的url使用html标记出来)
        ''' </summary>
        ''' <param name="s">假设这里面没有任何html标记</param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("URL.Highlights")>
        Public Function HighlightURL(s As String) As String
            Dim formatter As New StringBuilder(s)
            Dim urls As String() = StringHelpers.GetURLs(s)

            For Each url As String In urls.Distinct.ToArray
                Call formatter.Replace(url, $"<a href=""{url}"">{url}</a>")
            Next

            Return formatter.ToString
        End Function

        ''' <summary>
        ''' Highlights the email address in the text.(将文档里面的电子邮件地址使用html标记出来)
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("EMail.Highlights")>
        Public Function HighlightEMail(s As String) As String
            Dim formatter As New StringBuilder(s)
            Dim emails As String() = StringHelpers.GetEMails(s)

            For Each email As String In emails.Distinct.ToArray
                Call formatter.Replace(email, $"<a href=""mailto://{email}"">{email}</a>")
            Next

            Return formatter.ToString
        End Function
    End Module
End Namespace