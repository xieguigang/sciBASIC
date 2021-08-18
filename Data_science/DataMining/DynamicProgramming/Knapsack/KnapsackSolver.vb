Imports stdNum = System.Math

Namespace Knapsack

    Public Class KnapsackSolver

        Dim table As Double(,)
        Dim Items As IList(Of Item)
        Dim Capacity As Integer

        Sub New(items As IList(Of Item), capacity As Integer)
            Me.Items = items
            Me.Capacity = capacity
        End Sub

        Public Function GetWeight(items As IList(Of Item)) As Double
            Return items.Sum(Function(i) i.Weight)
        End Function

        Public Function GetValue(items As IList(Of Item)) As Double
            Return items.Sum(Function(i) i.Value)
        End Function

        Public Function Solve() As KnapsackSolution
            Return FillTable().TakeItems()
        End Function

        Private Function TakeItems() As KnapsackSolution
            Dim best = New KnapsackSolution() With {
                .Items = New List(Of Item)()
            }
            Dim row = Items.Count, col = Capacity

            While row > 0

                If table(row, col) <> table(row - 1, col) Then
                    best.Items.Add(Items(row - 1))
                    col -= Items(row - 1).Weight
                End If

                row -= 1
            End While

            best.TotalWeight = GetWeight(best.Items)
            best.Value = GetValue(best.Items)

            Return best
        End Function

        Private Function FillTable() As KnapsackSolver
            table = New Double(Items.Count + 1 - 1, Capacity + 1 - 1) {}

            For row = 1 To Items.Count
                Dim item = Items(row - 1)

                For col = 0 To Capacity

                    If item.Weight > col Then
                        table(row, col) = table(row - 1, col)
                    Else
                        table(row, col) = stdNum.Max(table(row - 1, col), table(row - 1, col - item.Weight) + item.Value)
                    End If
                Next
            Next

            Return Me
        End Function
    End Class
End Namespace
