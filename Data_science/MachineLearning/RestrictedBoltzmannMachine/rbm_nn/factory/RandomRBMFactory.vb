Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace nn.rbm.factory

    ''' <summary>
    ''' Created by kenny on 5/12/14.
    ''' </summary>
    Public Class RandomRBMFactory
        Implements RBMFactory

        Public Sub New()
        End Sub

        Public Function build(numVisibleNodes As Integer, numHiddenNodes As Integer) As RBM Implements RBMFactory.build
            Dim rbm As RBM = New RBM(numVisibleNodes, numHiddenNodes)

            Dim weights = rbm.Weights
            For i = 0 To numVisibleNodes - 1
                For j = 0 To numHiddenNodes - 1
                    weights.set(i, j, randomWeight())
                Next
            Next
            Return rbm
        End Function

        Private Shared Function randomWeight() As Double
            Return randf.NextGaussian() * 0.1
        End Function

    End Class

End Namespace
