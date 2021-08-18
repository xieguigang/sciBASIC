Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Namespace Knapsack

    Public Class KnapsackSolver

        Dim table As Double(,)
        Dim items As Item()
        Dim capacity As Integer

        Sub New(items As IEnumerable(Of Item), capacity As Integer)
            Me.items = items.ToArray
            Me.capacity = capacity
            Me.table = New Double(Me.items.Length + 1 - 1, capacity + 1 - 1) {}
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetWeight(items As IList(Of Item)) As Double
            Return Aggregate i As Item In items Into Sum(i.Weight)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetValue(items As IList(Of Item)) As Double
            Return Aggregate i As Item In items Into Sum(i.Value)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Solve() As KnapsackSolution
            Return FillTable().TakeItems()
        End Function

        Private Function TakeItems() As KnapsackSolution
            Dim row As Integer = items.Length
            Dim col As Integer = capacity
            Dim best As New KnapsackSolution() With {
                .Items = New List(Of Item)()
            }

            While row > 0
                If table(row, col) <> table(row - 1, col) Then
                    best.Items.Add(items(row - 1))
                    col -= items(row - 1).Weight
                End If

                row -= 1
            End While

            best.TotalWeight = GetWeight(best.Items)
            best.Value = GetValue(best.Items)

            Return best
        End Function

        Private Function FillTable() As KnapsackSolver
            Dim item As Item

            For row As Integer = 1 To items.Length
                item = items(row - 1)

                For col As Integer = 0 To capacity
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
