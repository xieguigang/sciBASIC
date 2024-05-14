#Region "Microsoft.VisualBasic::01208aa46597ee2016adc32233d5e27c, mime\text%html\Parser\HtmlParser.vb"

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

    '   Total Lines: 118
    '    Code Lines: 92
    ' Comment Lines: 4
    '   Blank Lines: 22
    '     File Size: 3.94 KB


    '     Class HtmlParser
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetHtmlTokens, ParseTree
    ' 
    '         Sub: parseNewTag, walkTag, walkToken
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

Namespace Language

    Public Class HtmlParser

        Shared ReadOnly tagsBreakStack As Index(Of String) = {
            "meta", "link", "img",
            "br", "hr", "input",
            "source"
        }

        Dim html As New HtmlDocument With {.TagName = "!DOCTYPE html"}
        Dim tagStack As New Stack(Of HtmlElement)
        Dim a As New Value(Of Token)
        Dim i As Pointer(Of Token)

        Private Sub New(i As Pointer(Of Token))
            Me.i = i
            Me.tagStack.Push(html)
        End Sub

        Private Sub walkToken()
            Select Case (a = ++i).name
                Case HtmlTokens.openTag
                    Call walkTag()
                Case Else
                    Call New InnerPlantText(CType(a, Token).text) _
                        .DoCall(AddressOf tagStack.Peek.Add)
            End Select
        End Sub

        Private Sub walkTag()
            Dim name As String = Strings.Trim((++i).text).Trim(ASCII.CR, ASCII.LF)

            If name = "/" Then
                name = (++i).text

                If name = tagStack.Peek.TagName Then
                    tagStack.Pop()
                End If

                i.MoveNext()
            ElseIf name = "=" Then
                ' 语法错误或者文本没有转义
                ' kegg数据库中的化学反应过程符号 <=>
                tagStack.Peek.Add(New InnerPlantText("<="))

                If i.Current.name = HtmlTokens.closeTag Then
                    i.MoveNext()
                    tagStack.Peek.Add(New InnerPlantText(">"))
                End If
            Else
                Call parseNewTag(name)
            End If
        End Sub

        Private Sub parseNewTag(name As String)
            Dim newTag As New HtmlElement With {.TagName = name}
            Dim tagClosed As Boolean = False

            Do While Not i.EndRead AndAlso (a = ++i).name <> HtmlTokens.closeTag
                If i.EndRead Then
                    Exit Do
                End If

                ' name=value
                If i.Current.name = HtmlTokens.equalsSymbol Then
                    i.MoveNext()
                    newTag.Add(CType(a, Token).text, (++i).text)
                ElseIf CType(a, Token).name = HtmlTokens.splash AndAlso i.Current.name = HtmlTokens.closeTag Then
                    ' <.../>
                    i.MoveNext()
                    tagClosed = True
                    Exit Do
                Else
                    newTag.Add(CType(a, Token).text, "")
                End If
            Loop

            tagStack.Peek.Add(newTag)

            If Not tagClosed Then
                If Not Strings.LCase(newTag.TagName) Like tagsBreakStack Then
                    tagStack.Push(newTag)
                End If
            End If
        End Sub

        Private Shared Function GetHtmlTokens(document As String) As Token()
            Dim tokens As Token()

            document = document.Replace("<!DOCTYPE html>", "")
            tokens = New TokenIcer(New StringBuilder(document).RemovesHtmlComments) _
                .GetTokens _
                .ToArray

            Return tokens
        End Function

        Public Shared Function ParseTree(document As String) As HtmlDocument
            Dim builder As New HtmlParser(GetHtmlTokens(document))

            Do While builder.i
                Call builder.walkToken()
            Loop

            Return builder.html
        End Function
    End Class
End Namespace
