Namespace NeuralNetwork

    Module Helpers

#Region "-- Globals --"
        ReadOnly Random As New Random()
#End Region

#Region "-- Helpers --"

        Public Function GetRandom() As Double
            Return 2 * Random.NextDouble() - 1
        End Function
#End Region
    End Module
End Namespace