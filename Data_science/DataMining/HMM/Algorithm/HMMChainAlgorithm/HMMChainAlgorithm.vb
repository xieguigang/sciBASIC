Namespace Algorithm.HMMChainAlgorithm

    Public MustInherit Class HMMChainAlgorithm : Inherits HMMAlgorithmBase

        Protected obSequence As Chain

        Sub New(HMM As HMM, obSequence As Chain)
            Call MyBase.New(HMM)

            Me.obSequence = obSequence
        End Sub
    End Class
End Namespace
