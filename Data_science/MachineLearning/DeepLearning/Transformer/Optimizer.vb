Imports Microsoft.VisualBasic.MachineLearning.Transformer.Utils

Namespace Transformer
    ''' <summary>
    ''' Implements the Adam optimizer
    ''' </summary>
    Public Class Optimizer
        Private Const beta1 As Double = 0.9
        Private Const beta2 As Double = 0.999
        Private Const eps As Double = 0.00000001

        Private M As Tensor
        Private V As Tensor

        Public Sub New(T As Tensor)
            M = New Tensor(T) * 0
            V = New Tensor(T) * 0
        End Sub

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer, T As Tensor)
            M = beta1 * M + (1.0 - beta1) * T.GetDerivatives()
            V = beta2 * V + (1.0 - beta2) * T.GetDerivatives().Pow(2)
            Dim m_hat = M / (1.0 - Math.Pow(beta1, [step]))
            Dim v_hat = V / (1.0 - Math.Pow(beta2, [step]))
            Dim correction = -learningRate * m_hat / (v_hat.Pow(0.5) + eps)
            T.MatAdd(correction)
            T.ClearDerivatives()
        End Sub


    End Class
End Namespace
