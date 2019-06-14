Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace DecisionTree

    ''' <summary>
    ''' Node attribute value
    ''' </summary>
    Public Class Attributes

        Public ReadOnly Property name As String
        Public ReadOnly Property differentAttributeNames As String()

        Public Property InformationGain As Double

        Public Sub New(name As String, differentAttributenames$())
            Me.name = name
            Me.differentAttributeNames = differentAttributenames
        End Sub

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {differentAttributeNames.GetJson} = {InformationGain}"
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