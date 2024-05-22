#Region "Microsoft.VisualBasic::a13d90c82220608e2c644ced97636caf, www\Microsoft.VisualBasic.Webservices.Bing\MicrosoftBing\WebResult.vb"

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

    '   Total Lines: 69
    '    Code Lines: 47 (68.12%)
    ' Comment Lines: 11 (15.94%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (15.94%)
    '     File Size: 2.33 KB


    '     Class WebResult
    ' 
    '         Function: TryParse
    ' 
    '     Class SearchResult
    ' 
    '         Properties: [Next], CurrentPage, HaveNext, Results, Title
    ' 
    '         Function: NextPage, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

Namespace Bing

    ''' <summary>
    ''' 一个结果条目
    ''' </summary>
    Public Class WebResult : Inherits Http.WebResult

        Public Shared Function TryParse(html As String) As WebResult
            Dim tokens As String() = Strings.Split(html, "</h2>", -1, CompareMethod.Text)
            Dim Title As String = tokens(Scan0)
            Dim URL As String = Title.href
            Title = Regex.Match(Title, """>.+</a>").Value
            Title = Title.GetValue.Trim
            html = tokens(1)
            html = Regex.Replace(html, "<cite>.+?</cite>", "", RegexICSng)
            html = Regex.Replace(html, "<.+?>", "", RegexICSng)

            Dim [date] As String = Regex.Match(New String(html.Reverse.ToArray), "\d+-\d+-\d+", RegexICSng).Value
            [date] = New String([date].Reverse.ToArray)

            Dim BriefText As String = html
            If Not String.IsNullOrEmpty([date]) Then
                BriefText = BriefText.Replace([date], "")
            End If

            Return New WebResult With {
                .Title = Title,
                .URL = URL,
                .BriefText = BriefText,
                .Update = [date]
            }
        End Function
    End Class

    Public Class SearchResult

        Public Property Title As String
        ''' <summary>
        ''' 总的结果数目
        ''' </summary>
        ''' <returns></returns>
        Public Property Results As Integer
        Public Property CurrentPage As WebResult()
        Public Property [Next] As String

        Public Function NextPage() As SearchResult
            Return SearchEngineProvider.DownloadResult([Next])
        End Function

        ''' <summary>
        ''' Is there have next page?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HaveNext As Boolean
            Get
                Return Not String.IsNullOrEmpty([Next])
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
