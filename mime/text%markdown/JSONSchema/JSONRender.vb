Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.application.json.LenientJson

Namespace JSONSchema

    ''' <summary>
    ''' Markdown render helper for LLM outputs
    ''' </summary>
    Public Module JSONRender

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
                            .rows = DirectCast(item!rows, JsonArray) _
                                .Select(Function(r) r.AsStringVector(True)) _
                                .ToArray
                        }
                    Case "heading", "h"
                        Yield New Block With {
                            .type = "paragraph",
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

        End Function

        Public Function ToHtml(docBlocks As IEnumerable(Of Block)) As String

        End Function

    End Module
End Namespace