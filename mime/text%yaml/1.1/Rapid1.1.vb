Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.MIME.text.yaml.Syntax
Imports r = System.Text.RegularExpressions.Regex

Namespace Grammar11

    ''' <summary>
    ''' YAML document parser for level 1.1
    ''' </summary>
    Public Module YamlParser

        ''' <summary>
        ''' YAML文档使用``---``作为文档间的分隔符
        ''' </summary>
        Const multipleYamlDelimiter$ = "^[-]{3}(\s|\n)"

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
            Dim root As New YamlDocument

            Return root
        End Function
    End Module
End Namespace