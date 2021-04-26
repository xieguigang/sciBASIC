Public MustInherit Class HMMChainAlgorithm : Inherits HMMAlgorithm

    Protected obSequence As Chain

    Sub New(HMM As HMM, obSequence As Chain)
        Call MyBase.New(HMM)

        Me.obSequence = obSequence
    End Sub
End Class

