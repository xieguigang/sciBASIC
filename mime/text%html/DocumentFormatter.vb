#Region "Microsoft.VisualBasic::72e4b70fd5a8764990683fecf5ffac91, mime\text%html\DocumentFormatter.vb"

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

    '   Total Lines: 50
    '    Code Lines: 22 (44.00%)
    ' Comment Lines: 20 (40.00%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 8 (16.00%)
    '     File Size: 1.70 KB


    ' Module DocumentFormatter
    ' 
    '     Function: HighlightEMail, HighlightLinks, HighlightURL
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

''' <summary>
''' Module provides some method for text document
''' </summary>
Public Module DocumentFormatter

    ''' <summary>
    ''' High light all of the links in the text document automatically.
    ''' </summary>
    ''' <param name="s">Assuming that the input text is plant text.</param>
    ''' <returns></returns>
    Public Function HighlightLinks(s As String) As String
        Return HighlightEMail(HighlightURL(s))
    End Function

    ''' <summary>
    ''' Highligh links in the text.(将文档里面的url使用html标记出来)
    ''' </summary>
    ''' <param name="s">假设这里面没有任何html标记</param>
    ''' <returns></returns>
    ''' 
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
    Public Function HighlightEMail(s As String) As String
        Dim formatter As New StringBuilder(s)
        Dim emails As String() = StringHelpers.GetEMails(s)

        For Each email As String In emails.Distinct.ToArray
            Call formatter.Replace(email, $"<a href=""mailto://{email}"">{email}</a>")
        Next

        Return formatter.ToString
    End Function
End Module
