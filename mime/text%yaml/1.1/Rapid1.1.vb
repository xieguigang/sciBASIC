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
        Const multipleYamlDelimiter$ = "^[-]{3}.*?$"

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

            root.Root = yaml.parseRoot("")

            'Do While Not yaml.EndRead

            'Loop

            Return root
        End Function

        <Extension>
        Private Function parseRoot(yaml As Pointer(Of Char), indent$) As DataItem
            Dim root As New Mapping With {.Enties = New List(Of MappingEntry)}
            Dim c As Char
            Dim name As New List(Of Char)
            Dim entry As MappingEntry
            Dim curIndent$ = yaml.getIndent

            ' 如果当前的indent小于传递进来的indent参数
            ' 说明节点已经结束了
            If Len(curIndent) < Len(indent) Then
                Return Nothing
            End If

            Do While Not yaml.EndRead
                c = ++yaml

                If c = ":"c Then

                    ' 递归解析成员
                    Do While yaml.Current = " "c
                        c = ++yaml
                    Loop

                    entry = New MappingEntry With {
                        .Key = New Scalar With {.Text = name.CharString},
                        .Value = yaml.parseRoot(curIndent)
                    }
                    If entry.Value Is Nothing Then
                        ' 可能是单独一行的数据
                        entry.Value = yaml.parseLine
                    Else
                        root.Enties.Add(entry)
                    End If
                Else
                    name += c
                End If
            Loop

            Return root
        End Function

        ''' <summary>
        ''' 解析数据直到遇见换行符
        ''' </summary>
        ''' <param name="yaml"></param>
        ''' <returns></returns>
        <Extension>
        Private Function parseLine(yaml As Pointer(Of Char)) As DataItem
            Dim c As Char = ++yaml
            Dim buffer As New List(Of Char)

            If c = "{"c Then
                Dim maps As New Mapping With {.Enties = New List(Of MappingEntry)}
                Dim tuple As MappingEntry

                Do While Not yaml.endLine
                    ' 迭代直到出现：
                    Do While yaml.Current <> ":"c
                        buffer += ++yaml
                    Loop

                    tuple = New MappingEntry With {
                        .Key = New Scalar With {.Text = buffer.CharString},
                        .Value = yaml.parseLine
                    }
                    maps.Enties.Add(tuple)
                Loop

                Return maps
            ElseIf c = "["c Then
                Dim list As New Sequence With {
                    .Enties = New List(Of DataItem)
                }

                Do While Not yaml.endLine
                    list.Enties.Add(yaml.parseLine)
                Loop

                Return list
            Else
                buffer += c

                Do While Not yaml.endLine
                    buffer += ++yaml
                Loop

                Return New Scalar With {.Text = buffer.CharString}
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function endLine(yaml As Pointer(Of Char)) As Boolean
            Return yaml.EndRead OrElse yaml.Current = ASCII.CR OrElse yaml.Current = ASCII.LF
        End Function

        <Extension>
        Private Function getIndent(yaml As Pointer(Of Char)) As String
            Dim b%
            Dim c As Char

            Do While Not yaml.EndRead
                c = ++yaml

                If c = " "c Then
                    b += 1
                ElseIf c = ASCII.CR OrElse c = ASCII.LF AndAlso b = 0 Then
                    ' do nothing
                Else
                    Exit Do
                End If
            Loop

            Return New String(" "c, b)
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
                Dim prefix As New TagPrefix With {
                    .Prefix = tagValue.Name.AsList
                }
                Dim handle As New NamedTagHandle With {
                    .Name = tagValue.Value.AsList
                }

                Return New TagDirective With {.Prefix = prefix, .Handle = handle}
            Else
                Throw New NotImplementedException($"%{name.CharString} {value.CharString}")
            End If
        End Function
    End Module
End Namespace