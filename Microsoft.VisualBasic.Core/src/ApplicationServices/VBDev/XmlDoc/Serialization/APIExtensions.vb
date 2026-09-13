#Region "Microsoft.VisualBasic::83bd69d6cfd51024ea5c96353d5ba9fe, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\XmlDoc\Serialization\APIExtensions.vb"

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

    '   Total Lines: 143
    '    Code Lines: 93 (65.03%)
    ' Comment Lines: 18 (12.59%)
    '    - Xml Docs: 55.56%
    ' 
    '   Blank Lines: 32 (22.38%)
    '     File Size: 5.24 KB


    '     Module APIExtensions
    ' 
    '         Properties: Types
    ' 
    '         Function: __boldParam, __trans, Load, TrimAssemblyDoc
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

Namespace ApplicationServices.Development.XmlDoc.Serialization

    <Package("Assembly.Doc.API")>
    Public Module APIExtensions

        ''' <summary>
        ''' NDoc supports this by recognising a special NamespaceDoc class located in each namespace
        ''' 
        ''' > https://stackoverflow.com/questions/793210/xml-documentation-for-a-namespace
        ''' 
        ''' 这个名字的对象类型主要是被用来标记命名空间的注释信息使用的
        ''' </summary>
        Public Const NamespaceDoc$ = NameOf(NamespaceDoc)

        Public ReadOnly Property Types As New Dictionary(Of Char, memberTypes) From {
 _
            {"T"c, memberTypes.Type},
            {"F"c, memberTypes.Filed},
            {"M"c, memberTypes.Method},
            {"P"c, memberTypes.Property},
            {"E"c, memberTypes.Event}
        }

        <ExportAPI("Load")>
        Public Function Load(path As String) As Doc
            Try
                Return path.LoadXml(Of Doc)(preprocess:=AddressOf TrimAssemblyDoc)
            Catch ex As Exception
                Call ex.PrintException
                Throw
            End Try
        End Function

        ''' <summary>
        ''' the url protocol prefix of a cross reference link that is generated from
        ''' the ``&lt;see cref="..." /&gt;`` / ``&lt;seealso cref="..." /&gt;`` xml
        ''' document tags. the api document generator will resolve this prefix into
        ''' the real target page url, and it will be rendered as a plain text when
        ''' the target could not be resolved.
        ''' </summary>
        Public Const CrefUrlPrefix$ = "cref:"

        Private ReadOnly RegexICS As RegexOptions = RegexOptions.IgnoreCase Or RegexOptions.Singleline

        Private ReadOnly inheritdocTag As New Regex("<inheritdoc\s*/>", RegexICS)
        Private ReadOnly listTag As New Regex("<list(?<attr>[^>]*)>(?<body>.*?)</list>", RegexICS)
        Private ReadOnly itemTag As New Regex("<item>(?<body>.*?)</item>", RegexICS)
        Private ReadOnly termTag As New Regex("<term>(?<text>.*?)</term>", RegexICS)
        Private ReadOnly descTag As New Regex("<description>(?<text>.*?)</description>", RegexICS)
        Private ReadOnly codeTag As New Regex("<code>(?<text>.*?)</code>", RegexICS)
        Private ReadOnly cTag As New Regex("<c>(?<text>.*?)</c>", RegexICS)
        Private ReadOnly seeCrefSelfTag As New Regex("<see(?:also)?\s+cref=""(?<cref>[^""]+)""[^>]*?/>", RegexICS)
        Private ReadOnly seeCrefTextTag As New Regex("<see(?:also)?\s+cref=""(?<cref>[^""]+)""[^>]*?>(?<text>.*?)</see(?:also)?>", RegexICS)
        Private ReadOnly seeLangwordTag As New Regex("<see\s+langword=""(?<word>[^""]+)""\s*/>", RegexICS)
        Private ReadOnly paramRefSelfTag As New Regex("<(?:type)?paramref\s+name=""(?<name>[^""]+)""\s*/>", RegexICS)
        Private ReadOnly paramRefTextTag As New Regex("<(?:type)?paramref\s+name=""(?<name>[^""]+)""[^>]*?>(?<text>.*?)</(?:type)?paramref>", RegexICS)
        Private ReadOnly brTag As New Regex("<br\s*/?>", RegexICS)
        Private ReadOnly anyTag As New Regex("<[^>]+>", RegexICS)

        ''' <summary>
        ''' Normalize the raw .net xml comment document into markdown text:
        ''' 1. ``&lt;code&gt;`` / ``&lt;c&gt;`` into code block / inline code;
        ''' 2. ``&lt;see cref="..." /&gt;`` / ``&lt;seealso /&gt;`` into a markdown
        '''    link that keeps the original cref identity (see cref link prefix);
        ''' 3. ``&lt;paramref /&gt;`` / ``&lt;typeparamref /&gt;`` into bold text;
        ''' 4. ``&lt;list&gt;`` / ``&lt;para&gt;`` / ``&lt;br/&gt;`` into the
        '''    markdown equivalent.
        ''' </summary>
        ''' <param name="doc"></param>
        ''' <returns></returns>
        <Extension>
        Public Function TrimAssemblyDoc(doc As String) As String
            If String.IsNullOrEmpty(doc) Then
                Return doc
            End If

            Dim s$ = doc

            ' block level tags should be processed before the inline tags
            s = inheritdocTag.Replace(s, "")
            s = listTag.Replace(s, AddressOf transList)
            s = codeTag.Replace(s, AddressOf transCodeBlock)
            s = cTag.Replace(s, AddressOf transInlineCode)

            ' cross references: the self closing tag must be handled before the
            ' tag with the inner text, otherwise the text form regex may swallow
            ' a large region when the self closing tag is not closed properly.
            s = seeCrefSelfTag.Replace(s, AddressOf transCref)
            s = seeCrefTextTag.Replace(s, AddressOf transCrefText)
            s = seeLangwordTag.Replace(s, AddressOf transLangword)

            s = paramRefSelfTag.Replace(s, AddressOf transParamRef)
            s = paramRefTextTag.Replace(s, AddressOf transParamRefText)

            s = s.Replace("<para>", vbLf & vbLf).Replace("</para>", vbLf & vbLf)
            s = brTag.Replace(s, vbLf)

            Return s
        End Function

        Private Function transCodeBlock(m As Match) As String
            Dim code = m.Groups("text").Value.Trim(ControlChars.Cr, ControlChars.Lf)
            Return vbLf & "```" & vbLf & code & vbLf & "```" & vbLf
        End Function

        Private Function transInlineCode(m As Match) As String
            Dim text = m.Groups("text").Value.Trim()
            If text.Contains("`") Then
                Return "`` " & text & " ``"
            Else
                Return "`" & text & "`"
            End If
        End Function

        Private Function transList(m As Match) As String
            Dim ordered As Boolean = m.Groups("attr").Value.IndexOf("number", StringComparison.OrdinalIgnoreCase) >= 0
            Dim sb As New StringBuilder
            Dim i As Integer = 0

            For Each item As Match In itemTag.Matches(m.Groups("body").Value)
                i += 1
                Dim body = item.Groups("body").Value
                Dim term = termTag.Match(body)
                Dim desc = descTag.Match(body)
                Dim text$

                If desc.Success Then
                    text = desc.Groups("text").Value.Trim()

                    If term.Success Then
                        text = "**" & term.Groups("text").Value.Trim() & "** — " & text
                    End If
                Else
                    text = anyTag.Replace(body, "").Trim()
                End If

                sb.Append(If(ordered, $"{i}. ", "- ")).Append(text).Append(vbLf)
            Next

            Return vbLf & sb.ToString & vbLf
        End Function

        Private Function transCref(m As Match) As String
            Return MakeCrefLink(m.Groups("cref").Value, Nothing)
        End Function

        Private Function transCrefText(m As Match) As String
            Return MakeCrefLink(m.Groups("cref").Value, m.Groups("text").Value)
        End Function

        Private Function transLangword(m As Match) As String
            Return "`" & m.Groups("word").Value & "`"
        End Function

        Private Function transParamRef(m As Match) As String
            Return "**" & m.Groups("name").Value & "**"
        End Function

        Private Function transParamRefText(m As Match) As String
            Dim text = m.Groups("text").Value.Trim()

            If String.IsNullOrEmpty(text) Then
                Return "**" & m.Groups("name").Value & "**"
            Else
                Return "**" & text & "**"
            End If
        End Function

        Private Function MakeCrefLink(cref As String, alt As String) As String
            Dim display$

            If String.IsNullOrWhiteSpace(alt) Then
                display = CrefDisplayName(cref)
            Else
                display = alt.Trim()
            End If

            ' escape the square bracket characters, otherwise the generated
            ' markdown link could be broken by a display text that contains them.
            display = display.Replace("[", "\[").Replace("]", "\]")

            Return $"[{display}]({CrefUrlPrefix}{cref.Trim()})"
        End Function

        ''' <summary>
        ''' Build a human readable display text for a xml document cref identity,
        ''' for example ``T:System.Console`` =&gt; ``Console``, and
        ''' ``M:Ns.ProjectSpace.Import(System.String)`` =&gt;
        ''' ``ProjectSpace.Import()``.
        ''' </summary>
        ''' <param name="cref"></param>
        ''' <returns></returns>
        Public Function CrefDisplayName(cref As String) As String
            If String.IsNullOrWhiteSpace(cref) Then
                Return ""
            End If

            Dim id$ = cref.Trim()
            Dim kind As Char = " "c

            If id.Length > 2 AndAlso id(1) = ":"c Then
                kind = id(0)
                id = id.Substring(2)
            End If

            Dim p = id.IndexOf("("c)
            If p > 0 Then
                id = id.Substring(0, p)
            End If

            Dim parts = id.Split("."c)
            Dim shortName = StripArity(parts(parts.Length - 1))

            If kind = "T"c OrElse kind = "N"c OrElse kind = "!"c OrElse kind = " "c Then
                Return shortName
            End If

            Dim owner$ = If(parts.Length >= 2, StripArity(parts(parts.Length - 2)), "")
            Dim text$ = If(owner.Length > 0, owner & "." & shortName, shortName)

            If kind = "M"c Then
                text &= "()"
            End If

            Return text
        End Function

        ''' <summary>
        ''' remove the generic arity suffix of a clr type name, for example
        ''' ``List`1`` =&gt; ``List``.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Private Function StripArity(name As String) As String
            Dim p = name.IndexOf("`"c)
            If p > 0 Then
                name = name.Substring(0, p)
            End If
            If name.Contains("#") Then
                name = name.Replace("#", ".")
            End If
            Return name
        End Function
    End Module
End Namespace
