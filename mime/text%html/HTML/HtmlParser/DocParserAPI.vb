#Region "Microsoft.VisualBasic::b153ff7749d7659e97d65ec7a8702d99, mime\text%html\HTML\HtmlParser\DocParserAPI.vb"

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

    '     Module DocParserAPI
    ' 
    '         Function: __getElementAttrs, __getElementName, __innerTextParser, __takesWhile, TextParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace HTML

    Public Module DocParserAPI

        Public Const HTML_ELEMENT_REGEX As String = "<[^/].+?>"
        Public Const HTML_SINGLE_ELEMENT As String = "<[^/].+? />"
        Public Const ATTRIBUTE_STRING As String = "\S+="".+?"""

        ''' <summary>
        ''' 解析标签开始和结束的位置之间的内部html文本
        ''' </summary>
        ''' <param name="doc"></param>
        ''' <returns></returns>
        ''' <remarks>这个方法是最开始的解析函数，非递归的</remarks>
        Public Function TextParse(ByRef doc As String) As InnerPlantText
            Dim strElement As String = Regex.Match(doc, HTML_ELEMENT_REGEX).Value  ' 得到开始的标签

            If String.IsNullOrEmpty(strElement) Then
                Return New InnerPlantText With {.InnerText = doc} '找不到开始的标签，则为纯文本
            End If

            Dim p As Integer

            If String.Equals(strElement, SpecialHtmlElements.HTML_DOCTYPE, StringComparison.OrdinalIgnoreCase) Then
                p = InStr(doc, SpecialHtmlElements.HTML_DOCTYPE)
                doc = Mid(doc, p + Len(SpecialHtmlElements.HTML_DOCTYPE) + 1)
                Return SpecialHtmlElements.DocumentType
            End If

            Dim el As HtmlElement = New HtmlElement With {
            .Name = __getElementName(strElement),
            .Attributes = __getElementAttrs(strElement)
        }

            p = InStr(doc, strElement)  '由于前面的文本已经解析完了，所以前面的文本全部扔掉
            doc = Mid(doc, p + Len(strElement))

            If String.Equals(el.Name, "?xml", StringComparison.OrdinalIgnoreCase) Then
                '这个是在解析XML文档，并且这个是头部，则跳过后续
                Return el
            End If

            '解析内部文本

            Do While True
                Dim parentEnd As Boolean = False
                Dim node As InnerPlantText = __innerTextParser(doc, el.Name, parentEnd)
                If node Is Nothing Then
                    Exit Do
                End If

                If Not node.IsEmpty Then
                    Call el.Add(node)
                End If

                If parentEnd Then
                    Exit Do
                End If
            Loop

            Return el
        End Function

        ''' <summary>
        ''' 在得到一个标签之后前面的数据会被扔掉，开始解析标签后面的数据
        ''' </summary>
        ''' <param name="innerText"></param>
        ''' <param name="parent"></param>
        ''' <returns>这个函数是一个递归函数</returns>
        Private Function __innerTextParser(ByRef innerText As String, parent As String, ByRef parentEnd As Boolean) As InnerPlantText
            If String.IsNullOrEmpty(innerText) Then
                Return Nothing
            End If

            Dim strElement = Regex.Match(innerText, HTML_ELEMENT_REGEX).Value '匹配下一个标签
            Dim p As Integer = InStr(innerText, strElement)
            ' 下一个标签和父节点标签之间的文本为内部文本
            Dim innerDoc As String = Mid(innerText, 1, p - 1)  ' 如果内部文档里面含有父节点的结束标签，则父节点结束
            parent = $"</{parent}>"
            Dim endTag As String = Regex.Match(innerDoc, parent, RegexOptions.IgnoreCase).Value
            If Not String.IsNullOrEmpty(endTag) Then
                Dim innerLen As Integer = Len(innerDoc)

                p = InStr(innerDoc, endTag)
                innerDoc = Mid(innerDoc, 1, p - 1)
                parentEnd = True

                If p = 1 Then
                    innerLen = Len(endTag)
                Else

                End If

                innerText = Mid(innerText, 1 + innerLen)

                Return New InnerPlantText With {.InnerText = innerDoc}
            End If

            If Not String.IsNullOrEmpty(innerDoc) Then
                ' 这部分的文本是纯文本，也是父节点的一部分
                innerText = Mid(innerText, Len(innerDoc) + 1)
                parentEnd = False
                Return New InnerPlantText With {.InnerText = innerDoc}
            End If

            If String.IsNullOrEmpty(strElement) Then '准备结束了，因为已经没有新的节点了
                p = InStr(innerText, parent, CompareMethod.Text)
                Dim lenth = p - 1
                If lenth < 0 Then
                    lenth = 0
                End If
                innerDoc = Mid(innerText, 1, lenth)
                innerText = Mid(innerText, p + Len(parent))
                parentEnd = True
                Return New InnerPlantText With {.InnerText = innerDoc}
            End If

            '新的子节点的解析开始了

            Dim x As New HtmlElement With {
            .Name = __getElementName(strElement),
            .Attributes = __getElementAttrs(strElement)
        }

            innerText = Mid(innerText, Len(strElement) + 1) '由于父节点的内部文本已经在前面清除掉了，所以这里的子节点直接从第一个字符开始

            If SpecialHtmlElements.IsBrChangeLine(strElement) Then
                parentEnd = False
                Return x
            End If

            If String.Equals(x.Name, "img", StringComparison.OrdinalIgnoreCase) Then
                parentEnd = False
                Return x
            End If

            If String.Equals(x.Name, "meta", StringComparison.OrdinalIgnoreCase) Then
                If String.Equals(parent, "</head>", StringComparison.OrdinalIgnoreCase) Then
                    Return x '头部区域的元数据，没有子节点的
                End If
            End If

            Do While True
                Dim innerParentEnd As Boolean = False
                Dim node As InnerPlantText = __innerTextParser(innerText, x.Name, innerParentEnd)
                If node Is Nothing Then
                    Exit Do
                End If

                If Not node.IsEmpty Then
                    Call x.Add(node)
                End If

                If innerParentEnd Then
                    Exit Do
                End If
            Loop

            parentEnd = False

            Return x
        End Function

        Private Function __getElementAttrs(s As String) As ValueAttribute()
            Dim tokens As String() = CommandLine.GetTokens(s).Skip(1).ToArray
            If tokens.Length > 0 Then
                tokens(tokens.Length - 1) =
                Mid(tokens.Last, 1, Len(tokens.Last) - 1)
            End If
            Return tokens _
                .TakeWhile(AddressOf __takesWhile) _
                .Select(Function(attr) New ValueAttribute(attr)) _
                .ToArray
        End Function

        Private Function __takesWhile(attr As String) As Boolean
            Return Not String.Equals(attr, "\") AndAlso Not String.Equals(attr, "/")
        End Function

        Private Function __getElementName(s As String) As String
            Dim p As Integer = InStr(s, " ")
            If p = 0 Then
                Return Mid(s, 2, Len(s) - 2)
            Else
                Dim Name As String = Mid(s, 2, p - 2)
                Return Name
            End If
        End Function
    End Module
End Namespace
