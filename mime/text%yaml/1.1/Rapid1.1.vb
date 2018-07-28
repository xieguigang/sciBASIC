Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.text.yaml.Syntax
Imports Microsoft.VisualBasic.Text
Imports r = System.Text.RegularExpressions.Regex

Namespace Grammar11

    ''' <summary>
    ''' YAML document parser for level 1.1
    ''' </summary>
    Public Module YamlParser

        ''' <summary>
        ''' YAML文档使用``---``作为文档间的分隔符
        ''' </summary>
        Const multipleYamlDelimiter$ = "^[-]{3}(\s|\n|.+$)"

        Public Iterator Function PopulateDocuments(yaml As String) As IEnumerable(Of YamlDocument)
            Dim yamlDoc$() = r.Split(yaml.SolveStream, multipleYamlDelimiter, RegexICMul) _
                              .Where(Function(part)
                                         ' 空白的行就是yaml文档的分隔符所处的行
                                         Return Not part.Trim.StringEmpty
                                     End Function) _
                              .ToArray

            For Each document As String In yamlDoc
                Yield New Pointer(Of Char)(document).ParseDocument
            Next
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="yaml"></param>
        ''' <returns></returns>
        <Extension>
        Private Function ParseDocument(yaml As Pointer(Of Char)) As YamlDocument
            Dim root As New YamlDocument With {
                .Directives = New List(Of Directive)
            }
            Dim directive As New Value(Of Directive)

            Do While Not (directive = yaml.parseDirective) Is Nothing
                root.Directives.Add(directive)
            Loop

            Return root
        End Function

        <Extension>
        Private Function parseDirective(yaml As Pointer(Of Char)) As Directive
            If yaml.Current <> "%"c Then
                Return Nothing
            Else
                yaml.MoveNext()
            End If

            Dim c As Char
            Dim name As New List(Of Char)
            Dim value As New List(Of Char)

            Do While Not yaml.EndRead
                c = ++yaml

                If c = " "c Then
                    Exit Do
                Else
                    name += c
                End If
            Loop

            Do While Not yaml.EndRead
                c = ++yaml

                If c = ASCII.CR OrElse c = ASCII.LF Then
                    Exit Do
                Else
                    value += c
                End If
            Loop

            If name = "YAML" Then
                Dim ver$() = value.CharString.Split("."c)
                Dim yamlVersion As New YamlVersion With {
                    .Major = Val(ver(0)),
                    .Minor = Val(ver(1))
                }

                Return New YamlDirective With {.Version = yamlVersion}
            ElseIf name = "TAG" Then
                Dim tagValue = value.CharString.GetTagValue

            End If
        End Function
    End Module
End Namespace