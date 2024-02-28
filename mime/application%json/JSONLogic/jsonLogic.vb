Imports System.Linq.Expressions
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace JSONLogic

    Public Module jsonLogic

        Public Function apply(logic As JsonElement, Optional data As JsonElement = Nothing) As JsonElement
            Dim code As Expression = TreeBuilder.Parse(logic)

        End Function

        Public Function apply(logic As String, Optional data As String = Nothing) As Object
            Dim logic_json As JsonElement = JsonParser.Parse(logic)
            Dim data_json As JsonElement = JsonParser.Parse(data)
            Dim result_json As JsonElement = jsonLogic.apply(logic_json, data_json)

            If result_json Is Nothing Then
                Return Nothing
            ElseIf TypeOf result_json Is JsonValue Then
                Return DirectCast(result_json, JsonValue).value
            Else
                Throw New NotImplementedException(result_json.GetType.FullName)
            End If
        End Function

    End Module
End Namespace