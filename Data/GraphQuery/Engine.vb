Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.Markup.HTML

''' <summary>
''' the engine of run graph query
''' </summary>
Public Class Engine

    Public Function Execute(document As HtmlElement, query As Query) As JsonElement
        If Not query.members.IsNullOrEmpty Then
            ' object
            Return QueryObject(document, query)
        Else
            ' value
            Return QueryValue(document, query)
        End If
    End Function

    Private Function QueryValue(document As HtmlElement, query As Query) As JsonValue
        Dim value As InnerPlantText = query.parser.Parse(document, query.isArray, Me)
        Dim valStr As String = value.GetPlantText
        Dim json As New JsonValue(valStr)

        Return json
    End Function

    Private Function QueryObject(document As HtmlElement, query As Query) As JsonObject
        Dim obj As New JsonObject

        For Each member As Query In query.members
            obj.Add(member.name, Execute(document, member))
        Next

        Return obj
    End Function
End Class
