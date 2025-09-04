Namespace ShapleyValue

    Public Class NodeValue

        Dim nextValuesList As IList(Of Integer)

        Public Overridable ReadOnly Property Value As Integer

        Public Overridable ReadOnly Property NextValues As IList(Of Integer)
            Get
                Return New List(Of Integer)(nextValuesList)
            End Get
        End Property

        Public Sub New(nextValues As IList(Of Integer))
            nextValuesList = New List(Of Integer)()
            CType(nextValuesList, List(Of Integer)).AddRange(nextValues)
            Value = nextValues(0)
            nextValuesList.RemoveAt(Value)
        End Sub

        Public Overridable Sub updateValue()
            If nextValuesList.Count > 0 Then
                _Value = nextValuesList(0)
            End If

            nextValuesList.RemoveAt(Value)
        End Sub

        Public Overrides Function ToString() As String
            Return "NodeValue [value=" & Value.ToString() & ", nextValues=" & nextValuesList.ToString() & "]"
        End Function

    End Class

End Namespace
