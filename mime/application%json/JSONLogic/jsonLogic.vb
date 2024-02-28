Imports System.Linq.Expressions
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace JSONLogic

    Public Module jsonLogic

        Private Function GetValueType(val As JsonElement, ByRef input As Object) As Type
            If val Is Nothing Then
                input = Nothing
                Return GetType(Object)
            ElseIf TypeOf val Is JsonValue Then
                input = DirectCast(val, JsonValue).value
                Return DirectCast(val, JsonValue).UnderlyingType
            ElseIf TypeOf val Is JsonArray Then
                Dim types As New List(Of Type)
                Dim vals As New List(Of Object)
                Dim list As JsonArray = val

                For Each item As JsonElement In list
                    types.Add(GetValueType(item, input))
                    vals.Add(input)
                Next

                Dim type As Type = MeasureType(types)
                Dim array As Array = Array.CreateInstance(type, vals.Count)

                Array.ConstrainedCopy(vals.ToArray, Scan0, array, Scan0, array.Length)
                input = array

                Return type
            Else
                Throw New NotImplementedException
            End If
        End Function

        Private Function MeasureType(elements As IEnumerable(Of Type)) As Type
            Dim pullAll = elements.ToArray
            Dim first As Type = pullAll.First

            If pullAll.All(Function(t) t Is first) Then
                Return first
            Else
                Throw New NotImplementedException
            End If
        End Function

        Public Function apply(logic As JsonElement, Optional data As JsonElement = Nothing) As JsonElement
            Dim pars As New List(Of (String, Type))
            Dim input As New List(Of Object)

            If Not data Is Nothing Then
                If TypeOf data Is JsonObject Then
                    Dim list As JsonObject = data
                    Dim ref As Object = Nothing

                    For Each key As String In list.ObjectKeys
                        Call pars.Add((key, GetValueType(list(key), ref)))
                        Call input.Add(ref)
                    Next
                End If
            End If

            Dim env As New TreeBuilder(pars)
            Dim code As Expression = env.Parse(logic)
            Dim lambda As LambdaExpression = Expression.Lambda(code, env.Parameters.ToArray)
            Dim func = lambda.Compile
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