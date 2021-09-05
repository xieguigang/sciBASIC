Imports Microsoft.VisualBasic.Language.Java.Arrays
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace train
    Public Class RowSampler

        Public row_mask As New List(Of Double)()

        Public Sub New(n As Integer, sampling_rate As Double)
            For i = 0 To n - 1
                row_mask.Add(If(randf.NextDouble <= sampling_rate, 1.0, 0.0))
            Next
        End Sub

        Public Overridable Sub shuffle()
            Dim rands = row_mask.Shuffles

            row_mask.Clear()
            row_mask.AddRange(rands)
        End Sub
    End Class

    Public Class ColumnSampler

        Private cols As New List(Of Integer)()
        Public col_selected As List(Of Integer)
        Private n_selected As Integer

        Public Sub New(n As Integer, sampling_rate As Double)
            For i = 0 To n - 1
                cols.Add(i)
            Next

            n_selected = CInt(n * sampling_rate)
            col_selected = cols.subList(0, n_selected)
        End Sub

        Public Overridable Sub shuffle()
            Dim rands = cols.Shuffles

            cols.Clear()
            cols.AddRange(rands)
            col_selected = cols.subList(0, n_selected)
        End Sub
    End Class
End Namespace
