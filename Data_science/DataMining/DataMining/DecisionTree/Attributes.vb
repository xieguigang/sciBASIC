Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.DecisionTree.Data
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace DecisionTree

    ''' <summary>
    ''' Node attribute value
    ''' </summary>
    Public Class Attributes

        Public Property name As String
        Public Property differentAttributeNames As String()
        Public Property informationGain As Double

        Public Sub New(name As String, differentAttributenames$())
            Me.name = name
            Me.differentAttributeNames = differentAttributenames
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {differentAttributeNames.GetJson} = {informationGain}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetDifferentAttributeNamesOfColumn(data As DataTable, columnIndex As Integer) As String()
            Return data.rows _
                .Select(Function(d) d(columnIndex)) _
                .Distinct _
                .ToArray
        End Function
    End Class
End Namespace