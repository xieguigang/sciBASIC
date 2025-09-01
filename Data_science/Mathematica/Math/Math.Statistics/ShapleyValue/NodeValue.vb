Namespace ShapleyValue

    Public Class NodeValue

        Public Overrides Function ToString() As String
            Return "NodeValue [value=" & valueField.ToString() & ", nextValues=" & nextValuesField.ToString() & "]"
        End Function

        Private valueField As Integer

        Private nextValuesField As IList(Of Integer)

        Public Sub New(nextValues As IList(Of Integer))

            nextValuesField = New List(Of Integer)()
            CType(nextValuesField, List(Of Integer)).AddRange(nextValues)
            valueField = nextValues(0)
            nextValuesField.RemoveAt(valueField)
        End Sub

        Public Overridable ReadOnly Property Value As Integer
            Get
                Return valueField
            End Get
        End Property

        Public Overridable ReadOnly Property NextValues As IList(Of Integer)
            Get
                Return New List(Of Integer)(nextValuesField)
            End Get
        End Property

        Public Overridable Sub updateValue()
            If nextValuesField.Count > 0 Then
                valueField = nextValuesField(0)
            End If
            nextValuesField.RemoveAt(valueField)

        End Sub



    End Class

End Namespace
