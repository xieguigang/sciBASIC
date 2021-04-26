Public Class Forward : Inherits HMMAlgorithm
    Sub New(HMM)
        Call MyBase.New(HMM)
    End Sub
    Public Function forwardAlgorithm(obSequence)
        Dim forward As New forwardFactory(HMM, obSequence)
        Dim initAlphas = forward.initForward()
        Dim allAlphas = forward.recForward(initAlphas, 1, {initAlphas})
        Return New With {.alphas = allAlphas, .alphaF = forward.termForward(allAlphas)}
    End Function
End Class
