Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace JSONLogic

    Public Module jsonLogic

        Public Function apply(logic As JsonElement, data As JsonElement) As JsonElement

        End Function

        Public Function apply(logic As String, data As String) As Object
            Dim logic_json As JsonElement = JsonParser.Parse(logic)
            Dim data_json As JsonElement = JsonParser.Parse(logic)
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