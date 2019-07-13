#Region "Microsoft.VisualBasic::6b9c567440de81f6bea54dddfb57d878, mime\application%json\Parser\JsonArray.vb"

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

    '     Class JsonArray
    ' 
    '         Properties: Length
    ' 
    '         Function: BuildJsonString, ContainsElement, GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: Add, Insert, Remove
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Linq

Namespace Parser

    Public Class JsonArray : Inherits JsonModel
        Implements IEnumerable(Of JsonElement)

        Dim list As New List(Of JsonElement)

        Public ReadOnly Property Length As Integer
            Get
                Return list.Count
            End Get
        End Property

        Public Sub Add(element As JsonElement)
            Call list.Add(element)
        End Sub

        Public Sub Insert(index As Integer, element As JsonElement)
            Call list.Insert(index, element)
        End Sub

        ''' <summary>
        ''' Gets/Set elements by index
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(index As Integer) As JsonElement
            Get
                Return list(index)
            End Get
            Set(value As JsonElement)
                list(index) = value
            End Set
        End Property

        Public Sub Remove(index As Integer)
            list.RemoveAt(index)
        End Sub

        Public Function ContainsElement(element As JsonElement) As Boolean
            Return list.Contains(element)
        End Function

        Public Overrides Function ToString() As String
            Return "JsonArray: {count: " & list.Count & "}"
        End Function

        Public Overrides Function BuildJsonString() As String
            Dim a As New StringBuilder
            Dim array$() = list _
                .Select(Function(x) x.BuildJsonString) _
                .ToArray

            a.AppendLine("[")
            a.AppendLine(array.JoinBy(", "))
            a.AppendLine("]")

            Return a.ToString
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of JsonElement) Implements IEnumerable(Of JsonElement).GetEnumerator
            For Each x As JsonElement In list
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
