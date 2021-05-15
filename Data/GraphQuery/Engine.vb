Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.Markup
Imports Microsoft.VisualBasic.MIME.Markup.HTML

''' <summary>
''' the engine of run graph query
''' </summary>
Public Class Engine

    Public Function Execute(document As XElement, query As Query) As JsonElement
        Return Execute(document.CreateDocument, query)
    End Function

    Public Function Execute(document As HtmlElement, query As Query) As JsonElement
        If Not query.members.IsNullOrEmpty Then
            ' object
            Dim subDocument As HtmlElement
            Dim obj As JsonElement

            If query.parser Is Nothing Then
                subDocument = document
            Else
                subDocument = query.parser.Parse(document, query.isArray, Me)
            End If

            If query.isArray Then
                obj = QueryObjectArray(subDocument, query)
            Else
                obj = QueryObject(subDocument, query)
            End If

            Return obj
        Else
            If query.isArray Then
                Return QueryValueArray(document, query)
            Else
                ' value
                Return QueryValue(document, query)
            End If
        End If
    End Function

    Private Function QueryValueArray(document As HtmlElement, query As Query) As JsonArray
        Dim array As New JsonArray

        For Each item In document.HtmlElements
            array.Add(QueryValue(item, query))
        Next

        Return array
    End Function

    Private Function QueryValue(document As HtmlElement, query As Query) As JsonValue
        Dim value As InnerPlantText = query.parser.Parse(document, query.isArray, Me)
        Dim valStr As String = value.GetPlantText
        Dim json As New JsonValue(valStr)

        Return json
    End Function

    Private Function QueryObjectArray(document As HtmlElement, query As Query) As JsonArray
        Dim array As New JsonArray

        For Each item In document.HtmlElements
            array.Add(QueryObject(item, query))
        Next

        Return array
    End Function

    Private Function QueryObject(document As HtmlElement, query As Query) As JsonObject
        Dim obj As New JsonObject

        For Each member As Query In query.members
            obj.Add(member.name, Execute(document, member))
        Next

        Return obj
    End Function
End Class
