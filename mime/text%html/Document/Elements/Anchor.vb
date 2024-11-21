Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Html.XmlMeta

Namespace Document

    <XmlType("a")>
    Public Class Anchor : Inherits Node

        Public Property href As String
        Public Property target As String
        Public Property rel As String
        Public Property title As String
        Public Property download As String
        Public Property hreflang As String
        Public Property type As String
        Public Property media As String

        ''' <summary>
        ''' the inner plant/html text of current anchor
        ''' </summary>
        ''' <returns></returns>
        Public Property text As String

        Public Shared Function FromElement(element As HtmlElement) As Anchor
            Return New Anchor With {
                .id = element.id,
                .[class] = element.class,
                .href = element!href,
                .download = element!download,
                .hreflang = element!hreflang,
                .media = element!media,
                .rel = element!rel,
                .style = element!style,
                .target = element!target,
                .title = element!title,
                .type = element!type,
                .text = element.GetPlantText
            }
        End Function

    End Class
End Namespace