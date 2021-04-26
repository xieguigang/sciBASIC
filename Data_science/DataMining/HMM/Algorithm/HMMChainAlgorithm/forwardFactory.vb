Public Class forwardFactory : Inherits HMMChainAlgorithm

    Sub New(HMM, obSequence)
        Call MyBase.New(HMM, obSequence)
    End Sub

    Public Function initForward()
        Dim initTrellis = {}
        Dim obIndex = HMM.observables.indexOf(obSequence(0))
        Dim obEmission = HMM.emissionMatrix(obIndex)
        HMM.initialProb.forEach(Sub(p, i) initTrellis.push(p * obEmission(i)))
        Return initTrellis
    End Function

    Public Function recForward(prevTrellis, i, alphas)
        Dim obIndex = i
        If (obIndex = obSequence.length) Then
            Return alphas
        End If
        Dim nextTrellis = {}
        For s = 0 To HMM.states.length - 1
            Dim trellisArr = {}
            prevTrellis.forEach(Sub(prob, i)
                                    Dim trans = HMM.transMatrix(i)(s)
                                    Dim emiss = HMM.emissionMatrix(HMM.observables.indexOf(obSequence(obIndex)))(s)
                                    trellisArr.push(prob * trans * emiss)
                                End Sub)
            nextTrellis.push(trellisArr.reduce(Function(tot, curr) tot + curr))
        Next
        alphas.push(nextTrellis)
        Return this.recForward(nextTrellis, obIndex + 1, alphas)
    End Function

    Public Function termForward(alphas)
        Return alphas(alphas.length - 1).reduce(Function(tot, val) tot + val)
    End Function
End Class
