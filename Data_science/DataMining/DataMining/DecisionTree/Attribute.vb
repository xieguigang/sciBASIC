Namespace DecisionTree

    Public Class MyAttribute

        Public ReadOnly Property name As String
        Public ReadOnly Property differentAttributeNames As List(Of String)

        Public Property InformationGain As Double

        Public Sub New(name As String, differentAttributenames As List(Of String))
            Me.name = name
            Me.differentAttributeNames = differentAttributenames
        End Sub

        Public Shared Function GetDifferentAttributeNamesOfColumn(data As DataTable, columnIndex As Integer) As List(Of String)
            Dim differentAttributes = New List(Of String)()

            For i As Integer = 0 To data.Rows.Count - 1
                Dim index = i
                Dim found = differentAttributes.Any(Function(t) t.ToUpper().Equals(data.Rows(index)(columnIndex).ToString().ToUpper()))

                If Not found Then
                    differentAttributes.Add(data.Rows(i)(columnIndex).ToString())
                End If
            Next

            Return differentAttributes
        End Function
    End Class
End Namespace