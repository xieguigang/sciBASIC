
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.measure

Namespace evolution

    ''' <summary>
    ''' Configuration for the Genetic Programming evolution.
    ''' </summary>
    Public Class GPConfiguration
        Inherits Configuration

        ''' <summary>
        ''' Maximal depth of initially generated trees. </summary>
        Public initTreeDepth As Integer

        ''' <summary>
        ''' Type of crossover operator. </summary>
        Public crossoverType As GPTreeUtils.TreeCrossoverType
        ''' <summary>
        ''' Type of mutation operator. </summary>
        Public mutationType As GPTreeUtils.TreeMutationType

        Public Overrides Function validate() As Boolean
            Return MyBase.validate() AndAlso initTreeDepth >= 0
        End Function

        ''' <returns> configuration with default values set </returns>
        Public Overloads Shared Function createDefaultConfig() As GPConfiguration
            Dim config As GPConfiguration = New GPConfiguration()

            config.objective = ObjectiveFunction.MSE

            config.initTreeDepth = 5

            config.populationSize = 500
            config.selectionSize = 200
            config.tournamentSize = 5

            config.crossoverType = GPTreeUtils.TreeCrossoverType.SUBTREE_CROSSOVER
            config.mutationType = GPTreeUtils.TreeMutationType.POINT_MUTATION
            config.mutationProbability = 0.2

            config.maxEpochs = 100
            config.fitnessThreshold = 0.01

            Return config
        End Function

    End Class

End Namespace
