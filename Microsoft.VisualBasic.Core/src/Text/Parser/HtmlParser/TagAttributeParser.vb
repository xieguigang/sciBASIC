#Region "Microsoft.VisualBasic::ec96caa198a36c9665a3d873063f0ccc, Microsoft.VisualBasic.Core\src\Text\Parser\HtmlParser\TagAttributeParser.vb"

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

    '   Total Lines: 157
    '    Code Lines: 103
    ' Comment Lines: 31
    '   Blank Lines: 23
    '     File Size: 5.88 KB


    '     Module TagAttributeParser
    ' 
    '         Function: [class], attr, classList, GetImageLinks, href
    '                   img, parseAttrValImpl, (+2 Overloads) src, TagAttributes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports r = System.Text.RegularExpressions.Regex

Namespace Text.Parser.HtmlParser

    Public Module TagAttributeParser

        ' <area shape=rect	coords=40,45,168,70	href="/dbget-bin/www_bget?hsa05034"	title="hsa05034: Alcoholism" onmouseover="popupTimer(&quot;hsa05034&quot;, &quot;hsa05034: Alcoholism&quot;, &quot;#ffffff&quot;)" onmouseout="hideMapTn()" />
        ''' <summary>
        ''' The regexp pattern for the attributes in a html tag.
        ''' </summary>
        Const attributeParse$ = "\S+?\s*[=]\s*(("".+?"")|(\S+)|('.+?'))"

        ''' <summary>
        ''' 获取一个html标签之中的所有的attribute属性数据
        ''' </summary>
        ''' <param name="tag$"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function TagAttributes(tag As String) As IEnumerable(Of NamedValue(Of String))
            If tag.StringEmpty Then
                Return {}
            End If

            Return r _
                .Matches(tag.GetBetween("<", ">"), attributeParse, RegexICSng) _
                .EachValue _
                .Select(Function(t)
                            Dim a = t.GetTagValue("=", trim:="""'")
                            Dim val As String = parseAttrValImpl(a.Value)

                            Return New NamedValue(Of String)(a.Name, val)
                        End Function)
        End Function

        Const attributePattern$ = "%s\s*=\s*([""].+?[""])|(['].+?['])"

        Private Function parseAttrValImpl(value As String) As String
            If value.Length = 1 AndAlso value.First <> """"c AndAlso value.First <> "'"c Then
                Return value
            Else
                Return value _
                    .GetStackValue("""", """") _
                    .GetStackValue("'", "'")
            End If
        End Function

        ''' <summary>
        ''' Get element attribute value
        ''' </summary>
        ''' <param name="html$"></param>
        ''' <param name="attrName$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function attr(html$, attrName$) As String
            If String.IsNullOrEmpty(html) Then
                Return ""
            Else
                attrName = attributePattern.Replace("%s", attrName)
                html = html.Match(attrName, RegexICSng)
            End If

            If String.IsNullOrEmpty(html) Then
                Return ""
            Else
                Return html _
                    .GetTagValue("=", trim:="""'") _
                    .Value _
                    .DoCall(AddressOf parseAttrValImpl)
            End If
        End Function

        ''' <summary>
        ''' Gets the link text in the html fragement text.
        ''' </summary>
        ''' <param name="html">A string that contains the url string pattern like: href="url_text"</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Html.Href")>
        <Extension>
        Public Function href(<Parameter("HTML", "A string that contains the url string pattern like: href=""url_text""")> html$, Optional default$ = "") As String
            Dim url As String = html.attr("href")

            If url.StringEmpty Then
                Return [default]
            Else
                Return url
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [class](tag As String) As String
            Return tag.attr("class")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function classList(tag As String) As String()
            Return tag.attr("class").StringSplit("\s+")
        End Function

#Region "Parsing image source url from the img html tag."

        Public Const imgHtmlTagPattern As String = "<img.+?src=.+?>"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetImageLinks(html As String) As String()
            Dim list$() = r _
                .Matches(html, imgHtmlTagPattern, RegexICSng) _
                .EachValue(Function(img) img.src) _
                .ToArray

            Return list
        End Function

        ''' <summary>
        ''' Parsing image source url from the img html tag.
        ''' </summary>
        ''' <param name="img"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function src(img As String) As String
            Return img.attr("src")
        End Function

#If NET_48 = 1 Or netcore5 = 1 Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function src(img As (tag$, attrs As NamedValue(Of String)())) As String
            Return img.attrs.GetByKey("src", True).Value
        End Function

        ''' <summary>
        ''' parse a image tag
        ''' </summary>
        ''' <param name="html$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function img(html$) As (tag$, attrs As NamedValue(Of String)())
            Return ("img", r.Match(html, imgHtmlTagPattern, RegexICSng).Value.TagAttributes.ToArray)
        End Function
#End If

#End Region
    End Module
End Namespace
