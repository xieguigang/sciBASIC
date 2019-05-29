Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MIME.application.json.Parser
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module Deserializer

    <Extension>
    Public Function CreateObject(json As JsonElement, schema As Type) As Object
        If TypeOf json Is JsonArray Then
            If Not schema.IsArray Then
                Throw New InvalidCastException
            Else
                Return DirectCast(json, JsonArray).createArray(schema.GetElementType)
            End If
        ElseIf TypeOf json Is JsonObject Then
            Return DirectCast(json, JsonObject).createObject(schema)
        ElseIf TypeOf json Is JsonValue Then
            Return DirectCast(json, JsonValue).Literal(schema)
        Else
            Throw New InvalidCastException
        End If
    End Function

    <Extension>
    Private Function createArray(json As JsonArray, elementType As Type) As Object
        Dim array As Array = Array.CreateInstance(elementType, json.Length)
        Dim obj As Object
        Dim element As JsonElement

        For i As Integer = 0 To array.Length - 1
            element = json(i)
            obj = element.CreateObject(elementType)
            array.SetValue(obj, i)
        Next

        Return array
    End Function

    <Extension>
    Private Function createObject(json As JsonObject, schema As Type) As Object
        Dim obj As Object = Activator.CreateInstance(schema)
        Dim isTable As Boolean = schema.IsInheritsFrom(GetType(DictionaryBase))
        Dim writers = schema.Schema(PropertyAccess.Writeable, PublicProperty, nonIndex:=True)
        Dim writer As PropertyInfo
        Dim addMethod As MethodInfo = schema.GetMethods _
            .Where(Function(m)
                       Dim params = m.GetParameters

                       Return Not m.IsStatic AndAlso
                           Not params.IsNullOrEmpty AndAlso
                           params.Length = 2 AndAlso
                           m.Name = "Add"
                   End Function) _
            .FirstOrDefault
        Dim valueType As Type = Nothing
        Dim inputs As Object()

        If isTable Then
            valueType = schema.GetGenericArguments(1)
        End If

        For Each [property] As NamedValue(Of JsonElement) In json
            If writers.ContainsKey([property].Name) Then
                writer = writers([property].Name)
                writer.SetValue(obj, [property].Value.CreateObject(writer.PropertyType))
            ElseIf isTable AndAlso Not addMethod Is Nothing Then
                inputs = {
                    [property].Name,
                    [property].Value.CreateObject(valueType)
                }
                addMethod.Invoke(obj, inputs)
            End If
        Next

        Return obj
    End Function
End Module
