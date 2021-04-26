Public MustInherit Class HMMChainAlgorithm : Inherits HMMAlgorithm

    Protected obSequence

    Sub New(HMM, obSequence)
        Call MyBase.New(HMM)

        Me.obSequence = obSequence
    End Sub
End Class

