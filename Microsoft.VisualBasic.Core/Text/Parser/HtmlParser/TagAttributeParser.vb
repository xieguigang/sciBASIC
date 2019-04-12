Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
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
            Return r _
                .Matches(tag.GetBetween("<", ">"), attributeParse, RegexICSng) _
                .EachValue _
                .Select(Function(t)
                            Dim a = t.GetTagValue("=", trim:="""'")
                            Dim val = a.Value.GetStackValue("""", """").GetStackValue("'", "'")

                            Return New NamedValue(Of String)(a.Name, val)
                        End Function)
        End Function

        Const attributePattern$ = "%s\s*=\s*([""].+?[""])|(['].+?['])"

        <Extension>
        Public Function GetAttrValue(html$, attr$) As String
            If String.IsNullOrEmpty(html) Then
                Return ""
            Else
                attr = attributePattern.Replace("%s", attr)
                html = html.Match(attr, RegexICSng)
            End If

            If String.IsNullOrEmpty(html) Then
                Return ""
            Else
                Return html.GetTagValue("=", trim:=True) _
                    .Value _
                    .GetStackValue("""", """") _
                    .GetStackValue("'", "'")
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
        <Extension> Public Function href(<Parameter("HTML", "A string that contains the url string pattern like: href=""url_text""")> html$) As String
            Return html.GetAttrValue("href")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [class](tag As String) As String
            Return tag.GetAttrValue("class")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function classList(tag As String) As String()
            Return tag.GetAttrValue("class").StringSplit("\s+")
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
            Return img.GetAttrValue("src")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function src(img As (tag$, attrs As NamedValue(Of String)())) As String
            Return img.attrs.GetByKey("src", True).Value
        End Function

        <Extension>
        Public Function img(html$) As (tag$, attrs As NamedValue(Of String)())
            Return ("img", r.Match(html, imgHtmlTagPattern, RegexICSng).Value.TagAttributes.ToArray)
        End Function
#End Region
    End Module
End Namespace