Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model

Namespace evolution

    Public Class EvolutionResult
        Public ReadOnly result As Expression
        Public ReadOnly fitness As Double
        Public ReadOnly time As Long
        Public ReadOnly epochs As Integer
        Public ReadOnly fitnessProgress As IList(Of Double)
        Public ReadOnly timeProgress As IList(Of Long)

        Public Sub New(result As Expression, fitness As Double, time As Long, epochs As Integer, fitnessProgress As IList(Of Double), timeProgress As IList(Of Long))
            Me.result = result
            Me.fitness = fitness
            Me.time = time
            Me.epochs = epochs
            Me.fitnessProgress = fitnessProgress
            Me.timeProgress = timeProgress
        End Sub

        Public Overrides Function ToString() As String
            Return $"fitness:{fitness} - {result.toStringExpression}"
        End Function
    End Class
End Namespace