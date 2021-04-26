Public Class backwardFactory : Inherits HMMChainAlgorithm

    Sub New(HMM, obSequence)
        Call MyBase.New(HMM, obSequence)
    End Sub

    Public Function recBackward(prevBetas, i, betas)
        Dim obIndex = i
        If (obIndex = 0) Then Return betas
        Dim nextTrellis = {}
        For s = 0 To HMM.states.length - 1
            Dim trellisArr = {}
            prevBetas.forEach(Sub(prob, i)
                                  Dim trans = HMM.transMatrix(s)(i)
                                  Dim emiss = HMM.emissionMatrix(HMM.observables.indexOf(obSequence(obIndex)))(i)
                                  trellisArr.push(prob * trans * emiss)
                              End Sub)
            nextTrellis.push(trellisArr.reduce(Function(tot, curr) tot + curr))
        Next
        betas.push(nextTrellis)
        Return recBackward(nextTrellis, obIndex - 1, betas)
    End Function
    Public Function termBackward(betas)
        Dim finalBetas = betas(betas.length - 1).reduce(Function(tot, curr, i)
                                                            Dim obIndex = HMM.observables.indexOf(obSequence(0))
                                                            Dim obEmission = HMM.emissionMatrix(obIndex)
                                                            tot.push(curr * HMM.initialProb(i) * obEmission(i))
                                                            Return tot
                                                        End Function, {})
        Return finalBetas.reduce(Function(tot, Val) tot + Val)
    End Function
End Class
