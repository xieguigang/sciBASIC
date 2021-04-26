Public Class EM : Inherits HMMChainAlgorithm
    Sub New(HMM, forwardObj, backwardBetas, obSequence)
        Call MyBase.New(HMM, obSequence)
    End Sub

    Public Function initialGamma(stateI)
        Return gamma(forwardObj.alphas(0)(stateI), backwardBetas(0)(stateI), forwardObj.alphaF)
    End Function
    Public Function gammaTimesInState(stateI)
        Dim gammas = {}
        For t = 0 To obSequence.length - 1
            gammas.push(gamma(forwardObj.alphas(t)(stateI), backwardBetas(t)(stateI), forwardObj.alphaF))
        Next
        Return gammas.reduce(Function(tot, curr) tot + curr)
    End Function
    Public Function gammaTransFromState(stateI)
        Dim gammas = {}
        For t = 0 To obSequence.length - 2
            gammas.push(gamma(forwardObj.alphas(t)(stateI), backwardBetas(t)(stateI), forwardObj.alphaF))
        Next
        Return gammas.reduce(Function(tot, curr) tot + curr)
    End Function
    Public Function xiTransFromTo(stateI, stateJ)
        Dim xis = {}
        For t = 0 To obSequence.length - 2
            Dim alpha = forwardObj.alphas(t)(stateI)
            Dim trans = HMM.transMatrix(stateI)(stateJ)
            Dim emiss = HMM.emissionMatrix(HMM.observables.indexOf(obSequence(t + 1)))(stateJ)
            Dim beta = backwardBetas(t + 1)(stateJ)
            xis.push(xi(alpha, trans, emiss, beta, forwardObj.alphaF))
        Next
        Return xis.reduce(Function(tot, curr) tot + curr)
    End Function
    Public Function gammaTimesInStateWithOb(stateI, obIndex)
        Dim obsK = HMM.observables(obIndex)
        Dim stepsWithOb = obSequence.reduce(Function(tot, curr, i)
                                                If (curr = obsK) Then
                                                    tot.push(i)
                                                End If

                                                Return tot
                                            End Function, {})
        Dim gammas = {}
        stepsWithOb.forEach(Sub([step])
                                gammas.push(gamma(forwardObj.alphas([step])(stateI), backwardBetas([step])(stateI), forwardObj.alphaF))
                            End Sub)
        Return gammas.reduce(Function(tot, curr) tot + curr)
    End Function

End Class
