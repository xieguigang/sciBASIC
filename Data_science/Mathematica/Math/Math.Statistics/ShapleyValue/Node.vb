
Namespace ShapleyValue

    Public Class Node
        Public Overridable ReadOnly Property Value As NodeValue
        Public Overridable Property NextNode As Node
        Public Overridable Property PrevNode As Node

        Public Sub New(nodeValue As NodeValue, prevNode As Node)
            _Value = nodeValue
            _PrevNode = prevNode

            If Value.NextValues.Count > 0 Then
                Dim values = Value.NextValues
                Dim nextValue As New NodeValue(values)
                NextNode = New Node(nextValue, Me)
            End If
        End Sub

        Public Overridable Sub updateValue()
            Value.updateValue()
        End Sub

        Public Overrides Function ToString() As String
            Return "Node [value=" & Value.ToString() & ", nextNode=" & NextNode.ToString() & "]"
        End Function
    End Class
End Namespace
