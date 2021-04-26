Public Class Forward : Inherits HMMAlgorithm

    Sub New(HMM As HMM)
        Call MyBase.New(HMM)
    End Sub

    Public Function forwardAlgorithm(obSequence As Chain) As Alpha
        Dim forward As New forwardFactory(HMM, obSequence)
        Dim initAlphas = forward.initForward()
        Dim allAlphas = forward.recForward(initAlphas.ToArray, 1, New List(Of List(Of Double)) From {initAlphas})

        Return New Alpha With {
            .alphas = allAlphas,
            .alphaF = forward.termForward(allAlphas)
        }
    End Function
End Class

Public Class Alpha

    Public Property alphaF As Double
    Public Property alphas As List(Of List(Of Double))
End Class