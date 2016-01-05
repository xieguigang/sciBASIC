Imports System.Net.Mail
Imports System.Net.Mime

Namespace Net.Mailto

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

        Public Shared Narrowing Operator CType(content As MailContents) As MailMessage
            Dim NewMailMessage As MailMessage = New MailMessage
            Dim AlternateView As AlternateView

            NewMailMessage.Subject = content.Subject
            NewMailMessage.Body = content.Body

            If Not content.Attatchments.IsNullOrEmpty Then
                For Each FilePath As String In content.Attatchments
                    Call NewMailMessage.Attachments.Add(New Attachment(fileName:=FilePath))
                Next
            End If

            If Len(content.Body) = 0 Then content.Body = String.Empty
            If Len(content.Logo) > 0 Then
                Dim Logo As New LinkedResource(content.Logo)
                Logo.ContentId = "Logo"
                AlternateView = AlternateView.CreateAlternateViewFromString(MessageHtml & content.Body, Nothing, MediaTypeNames.Text.Html)
                AlternateView.LinkedResources.Add(Logo)
            Else
                AlternateView = AlternateView.CreateAlternateViewFromString(content.Body, Nothing, MediaTypeNames.Text.Html)
            End If

            NewMailMessage.AlternateViews.Add(AlternateView)
            NewMailMessage.IsBodyHtml = True

            Return NewMailMessage
        End Operator

        Public Overrides Function ToString() As String
            Return String.Format("{0} -- {1}", Subject, Body)
        End Function
    End Structure
End Namespace