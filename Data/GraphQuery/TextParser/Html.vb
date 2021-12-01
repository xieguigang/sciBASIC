Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text

Namespace TextParser

    Module Html

        <ExportAPI("html")>
        Public Function html(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Return ParserFunction.ParseDocument(document, Function(i) New InnerPlantText With {.InnerText = i.GetHtmlText}, isArray)
        End Function

        <ExportAPI("urlQuery")>
        Public Function urlQuery(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Dim argName As String = parameters(Scan0)

            Return ParserFunction.ParseDocument(
                document:=document,
                pip:=Function(i)
                         Return New InnerPlantText With {
                             .InnerText = URL.Parse(i.GetHtmlText)(argName)
                         }
                     End Function,
                isArray:=isArray
            )
        End Function

        <ExportAPI("cssValue")>
        Public Function cssValue(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Dim keyName As String = parameters(0)

            Return ParserFunction.ParseDocument(
                document:=document,
                pip:=Function(i)
                         Dim css = CssParser.ParseStyle(i.GetHtmlText)
                         Dim cssVal As String = css(keyName)

                         Return New InnerPlantText With {
                             .InnerText = cssVal
                         }
                     End Function,
                isArray:=isArray
            )
        End Function
    End Module
End Namespace