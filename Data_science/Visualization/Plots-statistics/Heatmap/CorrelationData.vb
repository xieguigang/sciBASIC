Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.DataFrame

Namespace Heatmap

    Public Class CorrelationData

        Friend data As DistanceMatrix
        Friend min#, max#
        Friend range As DoubleRange

        Sub New(range As DoubleRange, data As DistanceMatrix)
            With range Or data _
                .PopulateRows _
                .IteratesALL _
                .ToArray _
                .Range _
                .AsDefault

                min = .Min
                max = .Max

                range = {0, .Max}
            End With

            Me.data = data
            Me.range = range
        End Sub

        Public Function GetMatrix() As Double()()
            Dim rows As New List(Of Double())

            For Each row As IReadOnlyCollection(Of Double) In data.PopulateRows
                rows.Add(row.ToArray)
            Next

            Return rows.ToArray
        End Function
    End Class

End Namespace


