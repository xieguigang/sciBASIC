Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Knapsack

    Public Class KnapsackSolution

        Public Property Items As IList(Of Item)
        Public Property TotalWeight As Double
        Public Property Value As Double

        Public Overrides Function ToString() As String
            Dim output = New StringBuilder()
            output.AppendLine(String.Format("value: {0}, total weight: {1}", Value, TotalWeight))
            output.AppendLine(" Products:" & String.Join(", ", Items))
            Return output.ToString()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Solve(items As IEnumerable(Of Item), capacity As Integer) As KnapsackSolution
            Return New KnapsackSolver(items, capacity).Solve
        End Function
    End Class
End Namespace
