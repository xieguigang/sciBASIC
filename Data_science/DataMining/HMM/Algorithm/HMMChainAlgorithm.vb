Public MustInherit Class HMMChainAlgorithm : Inherits HMMAlgorithm

    Protected obSequence()

    Sub New(HMM As HMM, obSequence As Object())
        Call MyBase.New(HMM)

        Me.obSequence = obSequence
    End Sub
End Class

