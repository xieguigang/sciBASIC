Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.application.json.LenientJson

Namespace JSONSchema

    ''' <summary>
    ''' Markdown render helper for LLM outputs
    ''' </summary>
    Public Module JSONRenderer

        Public Iterator Function Parse(jsonstr As String) As IEnumerable(Of Block)
            Dim json As JsonArray = LenientJsonParser.ParseJSON(jsonstr)

            For Each item As JsonObject In json
                Dim type As String = item!type.AsString(False)

                Select Case Strings.LCase(type)
                    Case "table"
                        Yield New Block With {
                            .type = "table",
                            .headers = item!headers.AsStringVector(True),
                            .alignments = item!alignments.AsStringVector(False),
                            .rows = If(item!rows Is Nothing,
                                       Nothing,
                                       DirectCast(item!rows, JsonArray) _
                                           .SafeQuery _
                                           .Select(Function(r) r.AsStringVector(True)) _
                                           .ToArray)
                        }
                    Case "heading", "h"
                        Yield New Block With {
                            .type = "heading",
                            .content = item!content.AsString(True),
                            .level = item!level.AsString(True).ParseInteger
                        }
                    Case "paragraph", "p"
                        Yield New Block With {
                            .type = "paragraph",
                            .content = item!content.AsString(True)
                        }
                    Case "code"
                        Yield New Block With {
                            .type = "code",
                            .content = item!content.AsString(True),
                            .language = item!language.AsString(True)
                        }
                    Case "list", "li"
                        Yield New Block With {
                            .type = "list",
                            .ordered = item!ordered.AsString(False).ParseBoolean,
                            .items = item!items.AsStringVector(True)
                        }
                    Case "blockquote"
                        Yield New Block With {
                            .type = "blockquote",
                            .content = item!content.AsString(True)
                        }
                    Case "hr", "horizontal-rule", "horizontalrule", "thematic-break"
                        Yield New Block With {
                            .type = "hr"
                        }
                    Case "image", "img"
                        Yield New Block With {
                            .type = "image",
                            .url = item!url.AsString(True),
                            .alt = item!alt.AsString(True),
                            .title = item!title.AsString(False)
                        }
                    Case "html", "raw"
                        Yield New Block With {
                            .type = "html",
                            .content = item!content.AsString(True)
                        }
                    Case "math", "equation", "tex", "latex"
                        Yield New Block With {
                            .type = "math",
                            .content = item!content.AsString(True),
                            .language = item!language.AsString(False)
                        }
                    Case "link", "a"
                        Yield New Block With {
                            .type = "link",
                            .url = item!url.AsString(True),
                            .alt = item!alt.AsString(True),
                            .title = item!title.AsString(False)
                        }
                    Case "tasklist", "tasks", "todo"
                        Yield New Block With {
                            .type = "tasklist",
                            .ordered = item!ordered.AsString(False).ParseBoolean,
                            .items = item!items.AsStringVector(True),
                            .checked = If(item!checked Is Nothing,
                                          Nothing,
                                          DirectCast(item!checked, JsonArray) _
                                              .SafeQuery _
                                              .Select(Function(b) b.AsString(False).ParseBoolean) _
                                              .ToArray())
                        }
                    Case "footnote", "note"
                        Yield New Block With {
                            .type = "footnote",
                            .id = item!id.AsString(True),
                            .content = item!content.AsString(True)
                        }
                    Case "deflist", "definition", "dl"
                        Yield New Block With {
                            .type = "deflist",
                            .terms = item!terms.AsStringVector(True),
                            .definitions = item!definitions.AsStringVector(True)
                        }
                    Case Else
                        ' 默认当作纯文本
                        Yield New Block With {
                            .type = "paragraph",
                            .content = item!content.AsString(True)
                        }
                End Select
            Next
        End Function

        <Extension>
        Public Function ToMarkdown(docBlocks As IEnumerable(Of Block)) As String
            Dim blocks As New List(Of String)

            For Each block As Block In docBlocks.SafeQuery
                Dim part As String = block.ToMarkdownBlock

                If Not String.IsNullOrEmpty(part) Then
                    Call blocks.Add(part)
                End If
            Next

            Return blocks.JoinBy(vbCrLf & vbCrLf)
        End Function

        <Extension>
        Public Function ToHtml(docBlocks As IEnumerable(Of Block)) As String
            Dim blocks As New List(Of String)

            For Each block As Block In docBlocks.SafeQuery
                Dim part As String = block.ToHtmlBlock

                If Not String.IsNullOrEmpty(part) Then
                    Call blocks.Add(part)
                End If
            Next

            Return blocks.JoinBy(vbCrLf & vbCrLf)
        End Function

    End Module

End Namespace