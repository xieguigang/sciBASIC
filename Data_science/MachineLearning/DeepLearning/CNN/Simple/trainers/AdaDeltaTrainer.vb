Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports std = System.Math

Namespace CNN.trainers

    ''' <summary>
    ''' Adaptive delta will look at the differences between the expected result and the current result to train the network.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class AdaDeltaTrainer : Inherits TrainerAlgorithm

        Private ReadOnly ro As Double = 0.95

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
            gsumi(j) = ro * gsumi(j) + (1 - ro) * gij * gij
            Dim dx = -std.Sqrt((xsumi(j) + eps) / (gsumi(j) + eps)) * gij
            xsumi(j) = ro * xsumi(j) + (1 - ro) * dx * dx ' yes, xsum lags behind gsum by 1.
            p(j) += dx
        End Sub
    End Class


End Namespace
