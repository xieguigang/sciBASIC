Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.measure

Namespace evolution

    ''' <summary>
    ''' Basic configuration for the evolution.
    ''' </summary>
    Public Class Configuration

        ''' <summary>
        ''' An objective function. </summary>
        Public objective As ObjectiveFunction

        ''' <summary>
        ''' Maximal size of the population. </summary>
        Public populationSize As Integer
        ''' <summary>
        ''' Size of selection to next generation. </summary>
        Public selectionSize As Integer
        ''' <summary>
        ''' Size of tournament selection. </summary>
        Public tournamentSize As Integer

        ''' <summary>
        ''' Probability of mutation. </summary>
        Public mutationProbability As Double

        ''' <summary>
        ''' Maximal number of epochs the evolution will last. </summary>
        Public maxEpochs As Integer
        ''' <summary>
        ''' Goal fitness threshold that evolution should reach. </summary>
        Public fitnessThreshold As Double

        ''' <returns> <tt>true</tt> IFF the configuration is valid </returns>
        Public Overridable Function validate() As Boolean
            Return objective IsNot Nothing AndAlso populationSize > 0 AndAlso selectionSize > 0 AndAlso tournamentSize > 0 AndAlso populationSize >= 2 * selectionSize AndAlso selectionSize Mod 2 = 0 AndAlso populationSize >= tournamentSize AndAlso mutationProbability >= 0.0 AndAlso mutationProbability <= 1.0 AndAlso maxEpochs > 0 AndAlso Not Double.IsNaN(fitnessThreshold)
        End Function

        ''' <returns> configuration with default values set </returns>
        Public Shared Function createDefaultConfig() As Configuration
            Dim config As Configuration = New Configuration()

            config.objective = ObjectiveFunction.MSE

            config.populationSize = 500
            config.selectionSize = 200
            config.tournamentSize = 5

            config.mutationProbability = 0.2

            config.maxEpochs = 100
            config.fitnessThreshold = 0.01

            Return config
        End Function

    End Class

End Namespace
