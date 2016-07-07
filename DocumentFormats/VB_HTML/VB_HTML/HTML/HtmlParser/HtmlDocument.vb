#Region "Microsoft.VisualBasic::d577ac6ac5c674bec5bff7264eac5099, ..\VisualBasic_AppFramework\DocumentFormats\VB_HTML\VB_HTML\HTML\HtmlParser\HtmlDocument.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Net
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Text

Namespace HTML

    Public Class HtmlDocument

        Public Const HTML_PAGE_CONTENT_TITLE As String = "<title>.+?</title>"

        Public Property Tags As InnerPlantText()

        ''' <summary>
        ''' 假设所加载的html文档是完好的格式的，即没有不匹配的标签的
        ''' </summary>
        ''' <param name="url"></param>
        ''' <returns></returns>
        Public Function LoadDocument(url As String) As HtmlDocument
            Dim html As String = url.GET.Replace(vbCr, "").Replace(vbLf, "") '是使用<br />标签来分行的
            Dim List As New List(Of InnerPlantText)

            html = Regex.Replace(html, "<!--.+?-->", "")

            Do While html.Length > 0
                Dim element As InnerPlantText = DocParserAPI.TextParse(html)
                If element Is Nothing Then
                    Exit Do
                Else
                    If Not element.IsEmpty Then
                        Call List.Add(element)
                    End If
                End If
            Loop

            Me.Tags = List

            Return Me
        End Function

        Public Shared Function Load(url As String) As HtmlDocument
            Return New HtmlDocument().LoadDocument(url)
        End Function
    End Class
End Namespace
