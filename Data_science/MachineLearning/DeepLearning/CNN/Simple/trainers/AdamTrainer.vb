Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports std = System.Math

Namespace CNN.trainers

    ''' <summary>
    ''' Adaptive Moment Estimation is an update to RMSProp optimizer. In this running average of both the
    ''' gradients and their magnitudes are used.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>

    Public Class AdamTrainer : Inherits TrainerAlgorithm

        Private ReadOnly beta1 As Double = 0.9
        Private ReadOnly beta2 As Double = 0.999

        Public Sub New(batch_size As Integer, l2_decay As Single)
            MyBase.New(batch_size, l2_decay)
        End Sub

        Public Overrides Sub initTrainData(bpr As BackPropResult)
            Dim newXSumArr = New Double(bpr.Weights.Length - 1) {}
            newXSumArr.fill(0)
            xsum.Add(newXSumArr)
        End Sub

        Public Overrides Sub update(i As Integer, j As Integer, gij As Double, p As Double())
            Dim gsumi = gsum(i)
            Dim xsumi = xsum(i)
            gsumi(j) = gsumi(j) * beta1 + (1 - beta1) * gij ' update biased first moment estimate
            xsumi(j) = xsumi(j) * beta2 + (1 - beta2) * gij * gij ' update biased second moment estimate
            Dim biasCorr1 = gsumi(j) * (1 - std.Pow(beta1, k)) ' correct bias first moment estimate
            Dim biasCorr2 = xsumi(j) * (1 - std.Pow(beta2, k)) ' correct bias second moment estimate
            Dim dx = -learning_rate * biasCorr1 / (std.Sqrt(biasCorr2) + eps)
            p(j) += dx
        End Sub
    End Class

End Namespace
