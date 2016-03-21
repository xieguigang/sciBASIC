Imports System.Runtime.CompilerServices

Namespace NeuralNetwork

    Public Module Helpers

        Public Const MaxEpochs As Integer = 5000
        Public Const MinimumError As Double = 0.01

#Region "-- Globals --"
        Private ReadOnly Random As New Random()
#End Region

#Region "-- Helpers --"

        Public Function GetRandom() As Double
            Return 2 * Random.NextDouble() - 1
        End Function
#End Region

        <Extension>
        Public Sub Train(ByRef neuron As Network, data As List(Of DataSet), Optional trainingType As TrainingType = TrainingType.Epoch)
            If trainingType = TrainingType.Epoch Then
                Call neuron.Train(data, Helpers.MaxEpochs)
            Else
                Call neuron.Train(data, Helpers.MinimumError)
            End If
        End Sub
    End Module

    Public Enum TrainingType
        ''' <summary>
        ''' <see cref="Helpers.MaxEpochs"/>
        ''' </summary>
        Epoch
        ''' <summary>
        ''' <see cref="Helpers.MinimumError"/>
        ''' </summary>
        MinimumError
    End Enum
End Namespace