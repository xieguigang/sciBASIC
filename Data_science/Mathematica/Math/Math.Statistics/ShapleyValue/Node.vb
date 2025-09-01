
Namespace ShapleyValue

    Public Class Node

        Public Overrides Function ToString() As String
            Return "Node [value=" & valueField.ToString() & ", nextNode=" & nextNodeField.ToString() & "]"
        End Function

        Private valueField As NodeValue

        Private nextNodeField As Node
        Private prevNodeField As Node

        Public Sub New(nodeValue As NodeValue, prevNode As Node)
            valueField = nodeValue
            prevNodeField = prevNode

            If valueField.NextValues.Count > 0 Then
                Dim values = valueField.NextValues
                Dim nextValue As NodeValue = New NodeValue(values)
                nextNodeField = New Node(nextValue, Me)
            End If
        End Sub

        Public Overridable ReadOnly Property Value As NodeValue
            Get
                Return valueField
            End Get
        End Property

        Public Overridable Property NextNode As Node
            Get
                Return nextNodeField
            End Get
            Set(value As Node)
                nextNodeField = value

            End Set
        End Property

        Public Overridable Sub updateValue()
            valueField.updateValue()
        End Sub


        Public Overridable Property PrevNode As Node
            Get
                Return prevNodeField
            End Get
            Set(value As Node)
                prevNodeField = value
            End Set
        End Property


    End Class

End Namespace
