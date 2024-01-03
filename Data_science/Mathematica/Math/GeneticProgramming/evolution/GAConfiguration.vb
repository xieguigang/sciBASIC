
Namespace evolution

    ''' <summary>
    ''' Configuration for the Genetic Algorithm evolution.
    ''' </summary>
    Public Class GAConfiguration
        Inherits Configuration

        ''' <summary>
        ''' Initial order of generate polynomial. </summary>
        Public initPolyOrder As Integer

        ''' <summary>
        ''' Beginning of the range for parameters of polynomial. </summary>
        Public paramRangeFrom As Double
        ''' <summary>
        ''' End of the range for parameters of polynomial. </summary>
        Public paramRangeTo As Double

        ''' <summary>
        ''' Type of crossover operator. </summary>
        Public crossoverType As GAPolynomialUtils.PolyCrossoverType
        ''' <summary>
        ''' Type of mutation operator. </summary>
        Public mutationType As GAPolynomialUtils.PolyMutationType

        Public Overrides Function validate() As Boolean
            Return MyBase.validate() AndAlso initPolyOrder >= 0 AndAlso paramRangeFrom < paramRangeTo AndAlso crossoverType IsNot Nothing AndAlso mutationType IsNot Nothing
        End Function

        ''' <returns> configuration with default values set </returns>
        Public Shared Function createDefaultConfig() As GAConfiguration
            Dim config As GAConfiguration = New GAConfiguration()

            config.objective = ObjectiveFunction.MSE

            config.initPolyOrder = 5

            config.paramRangeFrom = -1.0
            config.paramRangeTo = +1.0

            config.populationSize = 500
            config.selectionSize = 200
            config.tournamentSize = 5

            config.crossoverType = GAPolynomialUtils.PolyCrossoverType.SIMULATED_BINARY_CROSSOVER
            config.mutationType = GAPolynomialUtils.PolyMutationType.GAUSSIAN_POINT_MUTATION
            config.mutationProbability = 0.2

            config.maxEpochs = 100
            config.fitnessThreshold = 0.01

            Return config
        End Function

    End Class

End Namespace
