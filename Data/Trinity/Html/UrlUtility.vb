#Region "Microsoft.VisualBasic::36644267a65f266f15f8569f0c5a816d, Data\Trinity\Html\UrlUtility.vb"

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

    '   Total Lines: 39
    '    Code Lines: 23 (58.97%)
    ' Comment Lines: 8 (20.51%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (20.51%)
    '     File Size: 1.18 KB


    ' Class UrlUtility
    ' 
    '     Function: FixUrl, regexMatch
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions

''' <summary>
''' Url处理辅助类
''' </summary>
Public Class UrlUtility

    Const Find$ = "(?is)(href|src)=(""|')([^(""|')]+)(""|')"

    ''' <summary>
    ''' 基于baseUrl，补全html代码中的链接
    ''' </summary>
    ''' <param name="baseUrl"></param>
    ''' <param name="html"></param>
    Public Shared Function FixUrl(baseUrl As String, html As String) As String
        Dim url As New Uri(baseUrl)
        html = Regex.Replace(html, Find, Function(match) regexMatch(match, url))
        Return html
    End Function

    Private Shared Function regexMatch(match As Match, uri As Uri) As String
        Dim org As String = match.Value
        Dim link As String = match.Groups(3).Value

        If link.StartsWith("http") Then
            Return org
        End If

        Try
            Dim thisUri As New Uri(uri, link)
            Dim fullUrl As String = String.Format("{0}=""{1}""", match.Groups(1).Value, thisUri.AbsoluteUri)

            Return fullUrl
        Catch generatedExceptionName As Exception
            Return org
        End Try
    End Function

End Class
