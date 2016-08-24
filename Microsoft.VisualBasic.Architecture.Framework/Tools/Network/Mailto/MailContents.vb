#Region "Microsoft.VisualBasic::9dd42a22af2c168e56cdc689fe6538e9, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Tools\Network\Mailto\MailContents.vb"

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

Namespace Net.Mailto

    Public Class MailContents

        Const MessageHtml As String = "<html><body><table border=2><tr width=100%><td><img src=cid:Logo alt=companyname /></td><td>MY COMPANY DESCRIPTION</td></tr></table><hr/></body></html>"

        Public Property Subject As String
        Public Property Body As String

        ''' <summary>
        ''' The path list of the attachments file.
        ''' </summary>
        ''' <remarks></remarks>
        Public Property Attatchments As New List(Of String)
        ''' <summary>
        ''' The file path of the logo image.
        ''' </summary>
        ''' <remarks></remarks>
        Public Property Logo As String

        Public Shared Narrowing Operator CType(content As MailContents) As MailMessage
            Dim altView As AlternateView
            Dim msg As MailMessage = New MailMessage With {
                .Subject = content.Subject,
                .Body = content.Body
            }

            If Not content.Attatchments.IsNullOrEmpty Then
                For Each path As String In content.Attatchments
                    Call msg.Attachments.Add(New Attachment(fileName:=path))
                Next
            End If

            If Len(content.Body) = 0 Then content.Body = String.Empty
            If Len(content.Logo) > 0 Then
                Dim logo As New LinkedResource(content.Logo) With {
                    .ContentId = "Logo"
                }
                altView = AlternateView.CreateAlternateViewFromString(MessageHtml & content.Body, Nothing, MediaTypeNames.Text.Html)
                altView.LinkedResources.Add(logo)
            Else
                altView = AlternateView.CreateAlternateViewFromString(content.Body, Nothing, MediaTypeNames.Text.Html)
            End If

            msg.AlternateViews.Add(altView)
            msg.IsBodyHtml = True

            Return msg
        End Operator

        Public Overrides Function ToString() As String
            Return String.Format("{0} -- {1}", Subject, Body)
        End Function
    End Class
End Namespace
