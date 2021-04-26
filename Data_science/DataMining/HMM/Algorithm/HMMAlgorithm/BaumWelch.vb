Public Class BaumWelch : Inherits HMMAlgorithm
    Sub New(HMM)
        Call MyBase.New(HMM)
    End Sub

    Public Function baumWelchAlgorithm(obSequence)
        Dim forwardObj = New Forward(HMM).forwardAlgorithm(obSequence)
        Dim backwardBetas = New Backward(HMM).backwardAlgorithm(obSequence).betas.reverse()
        Dim EMSteps = New EM(HMM, forwardObj, backwardBetas, obSequence)
        Dim initProb = {}
        Dim transMatrix = {}
        Dim emissMatrix = {}
        For i = 0 To HMM.states.length - 1
            initProb.push(EMSteps.initialGamma(i))
            Dim stateTrans = {}
            For j = 0 To HMM.states.length - 1
                stateTrans.push(EMSteps.xiTransFromTo(i, j) / EMSteps.gammaTransFromState(i))
            Next
            transMatrix.push(stateTrans)
        Next
        For o = 0 To HMM.observables.length - 1
            Dim obsEmiss = {}
            For i = 0 To HMM.states.length - 1
                obsEmiss.push(EMSteps.gammaTimesInStateWithOb(i, o) / EMSteps.gammaTimesInState(i))
            Next
            emissMatrix.push(obsEmiss)
        Next
        Dim hiddenStates = transMatrix
        .reduce(Function(tot, curr, i)
                    Dim stateObj = New With {.state = HMM.states(i), .prob = curr}
                    tot.push(stateObj)
                    Return tot
                End Function, {})
        Dim observables = emissMatrix
        .reduce(Function(tot, curr, i)
                    Dim obsObj = New With {.obs = HMM.observables(i), .prob = curr}
                    tot.push(obsObj)
                    Return tot
                End Function, {})
        Return New HMM(hiddenStates, observables, initProb)
    End Function
End Class
