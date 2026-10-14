Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Module Clone

    <Extension>
    Public Function DeepClone(json As JsonElement) As JsonElement
        If json Is Nothing Then
            Return JsonValue.NULL
        End If

        If TypeOf json Is JsonValue Then
            Return New JsonValue(DirectCast(json, JsonValue))
        ElseIf TypeOf json Is JsonArray Then
            Return DirectCast(json, JsonArray).DeepClone
        ElseIf TypeOf json Is JsonObject Then
            Return DirectCast(json, JsonObject).DeepClone
        Else
            Throw New NotImplementedException("unknown json element type for make deep copy: " & json.GetType.FullName)
        End If
    End Function

    <Extension>
    Public Function DeepClone(json As JsonArray) As JsonArray
        Dim list As New JsonArray

        For i As Integer = 0 To json.Length - 1
            list(i) = json(i).DeepClone
        Next

        Return list
    End Function

    <Extension>
    Public Function DeepClone(json As JsonObject) As JsonObject
        Dim obj As New JsonObject

        For Each name As String In json.ObjectKeys
            Call obj.Add(name, json(name).DeepClone)
        Next

        Return obj
    End Function

End Module
