#Region "Microsoft.VisualBasic::a70ebb68d1c25b90b14115b30cff13f4, mime\application%json\Parser\JsonObject.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class JsonObject
    ' 
    '         Function: BuildJsonString, ContainsElement, ContainsKey, GetEnumerator, IEnumerable_GetEnumerator
    '                   Remove, ToString
    ' 
    '         Sub: (+2 Overloads) Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

Namespace Parser

    ''' <summary>
    ''' Dictionary/Array in javascript
    ''' </summary>
    Public Class JsonObject : Inherits JsonModel
        Implements IEnumerable(Of NamedValue(Of JsonElement))

        Dim array As New Dictionary(Of String, JsonElement)

        Public Sub Add(key As String, element As JsonElement)
            Call array.Add(key, element)
        End Sub

        Public Sub Add(key$, value$)
            Call array.Add(key, New JsonValue(value))
        End Sub

        Default Public Overloads Property Item(key As String) As JsonElement
            Get
                If array.ContainsKey(key) Then
                    Return array(key)
                Else
                    Return Nothing
                End If
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
            Return "JsonObject::[" & array.Keys.JoinBy(", ") & "]"
        End Function

        Public Overrides Function BuildJsonString() As String
            Dim a As New StringBuilder
            Dim array$() = Me _
                .array _
                .Select(Function(kp) $"""{kp.Key}"": {kp.Value.BuildJsonString}") _
                .ToArray

            Call a.AppendLine("{")
            Call a.AppendLine(array.JoinBy("," & vbLf))
            Call a.AppendLine("}")

            Return a.ToString
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of NamedValue(Of JsonElement)) Implements IEnumerable(Of NamedValue(Of JsonElement)).GetEnumerator
            For Each kp As KeyValuePair(Of String, JsonElement) In array
                Yield New NamedValue(Of JsonElement) With {
                .Name = kp.Key,
                .Value = kp.Value
            }
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
