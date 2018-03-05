#Region "Microsoft.VisualBasic::7f378b24487000a1815b02881967f9ad, Microsoft.VisualBasic.Core\Extensions\Webpage.vb"

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

    ' Module Webpage
    ' 
    '     Function: Get_href, TrimHTMLTag
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports System.Runtime.CompilerServices

Public Module Webpage

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="s_Data">A string that contains the url string pattern like: href="url_text"</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Get_href(s_Data As String) As String
        Dim url As String = Regex.Match(s_Data, "href="".+?""", RegexOptions.IgnoreCase).Value
        If String.IsNullOrEmpty(url) Then
            Return ""
        Else
            url = Mid(url, 6)
            url = Mid(url, 2, Len(url) - 2)
            Return url
        End If
    End Function

    Const HTML_TAG As String = "</?.+?(\s+.+?="".+?"")*>"

    <Extension> Public Function TrimHTMLTag(str As String) As String
        If String.IsNullOrEmpty(str) Then
            Return ""
        End If

        str = Regex.Replace(str, HTML_TAG, "")
        Return str
    End Function
End Module
