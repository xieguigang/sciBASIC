Imports System.Text
Imports Json
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

''' <summary>
''' Dictionary/Array in javascript
''' </summary>
Public Class JsonObject : Inherits JsonElement
    Implements IEnumerable(Of NamedValue(Of JsonElement))

    Dim array As New Dictionary(Of String, JsonElement)

    Public Sub Add(key As String, element As JsonElement)
        Call array.Add(key, element)
    End Sub

    Default Public Overloads Property Item(key As String) As JsonElement
        Get
            Return array(key)
        End Get
        Set(value As JsonElement)
            array(key) = value
        End Set
    End Property

    Public Function Remove(key As String) As Boolean
        Return array.Remove(key)
    End Function

    Public Function ContainsKey(key As String) As Boolean
        Return array.ContainsKey(key)
    End Function

    Public Function ContainsElement(element As JsonElement) As Boolean
        Return array.ContainsValue(element)
    End Function

    Public Overrides Function ToString() As String
        Return "[" & array.Keys.JoinBy(", ") & "]"
    End Function

    Public Overloads Function BuildJsonString() As String
        Dim a As New StringBuilder

        Call a.AppendLine("{")
        For Each kp As KeyValuePair(Of String, JsonElement) In array
            Call a.AppendLine($"{kp.Key}: {kp.Value.BuildJsonString},")
        Next
        Call a.AppendLine("}")

        Return a.ToString
    End Function

    Public Iterator Function GetEnumerator() As IEnumerator(Of NamedValue(Of JsonElement)) Implements IEnumerable(Of NamedValue(Of JsonElement)).GetEnumerator
        For Each kp As KeyValuePair(Of String, JsonElement) In array
            Yield New NamedValue(Of JsonElement) With {
                .Name = kp.Key,
                .x = kp.Value
            }
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class