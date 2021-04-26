Public Class Backward : Inherits HMMAlgorithm
    Sub New(HMM)
        Call MyBase.New(HMM)
    End Sub
    Public Function backwardAlgorithm(obSequence)
        Dim backward As New backwardFactory(HMM, obSequence)
        Dim initBetas = HMM.states.map(Function(s) 1)
        Dim allBetas = backward.recBackward(initBetas, obSequence.length - 1, {initBetas})
        Return New With {.betas = allBetas, .betaF = backward.termBackward(allBetas)}
    End Function
End Class
