Imports Microsoft.VisualBasic.Language.Java.Arrays
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace train
    Public Class RowSampler
        Public row_mask As List(Of Double?) = New List(Of Double?)()

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
        Private cols As List(Of Integer?) = New List(Of Integer?)()
        Public col_selected As List(Of Integer?)
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

    Public Class Sampling
        Public Shared Sub Main(args As String())
            'test case
            Dim rs As RowSampler = New RowSampler(1000000, 0.8)
            Console.WriteLine(rs.row_mask.subList(0, 20))
            rs.shuffle()
            Console.WriteLine(rs.row_mask.subList(0, 20))
            Dim sum = 0

            For Each v As Double In rs.row_mask
                sum += CInt(v)
            Next

            Console.WriteLine(sum)
            Dim cs As ColumnSampler = New ColumnSampler(1000, 0.6)
            Console.WriteLine(cs.col_selected.subList(0, 20))
            cs.shuffle()
            Console.WriteLine(cs.col_selected.subList(0, 20))
            Console.WriteLine(cs.col_selected.Count)
        End Sub
    End Class
End Namespace
