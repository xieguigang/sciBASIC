#Region "Microsoft.VisualBasic::49f2e8ba21bd96ee6694c43c6b36c2af, www\Microsoft.VisualBasic.NETProtocol\Mailto\Extensions.vb"

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

    '   Total Lines: 82
    '    Code Lines: 65
    ' Comment Lines: 3
    '   Blank Lines: 14
    '     File Size: 2.57 KB


    '     Module Extensions
    ' 
    '         Function: CreateMailMessage, msgView
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net.Mail
Imports System.Net.Mime
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder

Namespace Mailto

    Module Extensions

        ''' <summary>
        ''' The message html text template.
        ''' </summary>
        ReadOnly msgHTML As String =
            <html>
                <body>
                    <table border="2">
                        <tr width="100%">
                            <td><img src="cid:Logo" alt="companyname"/></td>
                            <td>{$Company}</td>
                        </tr>
                    </table>
                    <hr/>

                    {$Msg}
                </body>
            </html>

        <Extension>
        Public Function CreateMailMessage(content As MailContents) As MailMessage
            Dim altView As AlternateView
            Dim msg As New MailMessage With {
                .Subject = content.Subject,
                .Body = content.Body
            }
            Dim html$

            If Not content.Attatchments.IsNullOrEmpty Then
                Dim attatchs = msg.Attachments
                Dim file As Attachment

                For Each path As String In content.Attatchments
                    file = New Attachment(fileName:=path)
                    attatchs.Add(file)
                Next
            End If

            If Len(content.Body) = 0 Then content.Body = ""
            If Len(content.Logo) > 0 Then
                Dim logo As New LinkedResource(content.Logo) With {
                    .ContentId = "Logo"
                }

                With New ScriptBuilder(msgHTML)
                    !Msg = content.Body
                    !Company = ""

                    html = .ToString
                End With

                altView = msgView(html)
                altView.LinkedResources.Add(logo)
            Else
                html = content.Body
                altView = msgView(html)
            End If

            msg.AlternateViews.Add(altView)
            msg.IsBodyHtml = True

            Return msg
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function msgView(html As String) As AlternateView
            Return AlternateView.CreateAlternateViewFromString(
                content:=html,
                contentEncoding:=Nothing,
                mediaType:=MediaTypeNames.Text.Html
            )
        End Function
    End Module
End Namespace
