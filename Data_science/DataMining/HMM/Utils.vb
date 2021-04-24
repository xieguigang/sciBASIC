Public Class Utils
    Function findSequence(sequence As Array, states)
        Return sequence.reduce(Function(all, curr)
                                   Return all.push(states.findIndex(x >= x.state = curr))
                                   Return all
                               End Function, {})
    End Function

    Function gamma(alpha#, beta#, forward#) As Double
        Return (alpha * beta) / forward
    End Function

    Function xi(alpha#, trans#, emiss#, beta#, forward#) As Double
        Return (alpha * trans * emiss * beta) / forward
    End Function

    Function calculateProb(stateTrans2, init, states)
        Return New With {
           .sequenceProb = Function(sequence)
                               Return findSequence(sequence, states).reduce(Function(total, curr As Array, i, arr)
                                                                                If (i = 0) Then
                                                                                    total += init(curr)
                                                                                Else
                                                                                    total *= stateTrans2(arr(i - 1))(curr)
                                                                                End If

                                                                                Return total
                                                                            End Function, 0)
                           End Function
        }
    End Function

    Function forwardFactory(HMM, obSequence)
        Return New With {
            .initForward = Function()
                               Dim initTrellis = {}
                               Dim obIndex = HMM.observables.indexOf(obSequence(0))
                               Dim obEmission = HMM.emissionMatrix(obIndex)
                               HMM.initialProb.forEach(Sub(p, i) initTrellis.push(p * obEmission(i)))
                               Return initTrellis
                           End Function,
            .recForward = Function(prevTrellis, i, alphas)
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
                          End Function,
            .termForward = Function(alphas)
                               Return alphas(alphas.length - 1).reduce(Function(tot, val) tot + val)
                           End Function
        }
    End Function


    Function backwardFactory(HMM, obSequence)
        Return New With {
            .recBackward = Function(prevBetas, i, betas)
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
                               Return this.recBackward(nextTrellis, obIndex - 1, betas)
                           End Function,
             .termBackward = Function(betas)
                                 Dim finalBetas = betas(betas.length - 1).reduce(Function(tot, curr, i)
                                                                                     Dim obIndex = HMM.observables.indexOf(obSequence(0))
                                                                                     Dim obEmission = HMM.emissionMatrix(obIndex)
                                                                                     tot.push(curr * HMM.initialProb(i) * obEmission(i))
                                                                                     Return tot
                                                                                 End Function, {})
                                 Return finalBetas.reduce(Function(tot, Val) tot + Val)
                             End Function
        }
    End Function

    Function EM(HMM, forwardObj, backwardBetas, obSequence)
        Return New With {
            .initialGamma = Function(stateI) gamma(forwardObj.alphas(0)(stateI), backwardBetas(0)(stateI), forwardObj.alphaF),
            .gammaTimesInState = Function(stateI)
                                     Dim gammas = {}
                                     For t = 0 To obSequence.length - 1
                                         gammas.push(gamma(forwardObj.alphas(t)(stateI), backwardBetas(t)(stateI), forwardObj.alphaF))
                                     Next
                                     Return gammas.reduce(Function(tot, curr) tot + curr)
                                 End Function,
            .gammaTransFromState = Function(stateI)
                                       Dim gammas = {}
                                       For t = 0 To obSequence.length - 2
                                           gammas.push(gamma(forwardObj.alphas(t)(stateI), backwardBetas(t)(stateI), forwardObj.alphaF))
                                       Next
                                       Return gammas.reduce(Function(tot, curr) tot + curr)
                                   End Function,
            .xiTransFromTo = Function(stateI, stateJ)
                                 Dim xis = {}
                                 For t = 0 To obSequence.length - 2
                                     Dim alpha = forwardObj.alphas(t)(stateI)
                                     Dim trans = HMM.transMatrix(stateI)(stateJ)
                                     Dim emiss = HMM.emissionMatrix(HMM.observables.indexOf(obSequence(t + 1)))(stateJ)
                                     Dim beta = backwardBetas(t + 1)(stateJ)
                                     xis.push(xi(alpha, trans, emiss, beta, forwardObj.alphaF))
                                 Next
                                 Return xis.reduce(Function(tot, curr) tot + curr)
                             End Function,
            .gammaTimesInStateWithOb = Function(stateI, obIndex)
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
        }
    End Function

    Function viterbiFactory(HMM, obSequence)
        Return New With {
            .initViterbi = Function()
                               Dim initTrellis = {}
                               Dim obIndex = HMM.observables.indexOf(obSequence(0))
                               Dim obEmission = HMM.emissionMatrix(obIndex)
                               HMM.initialProb.forEach(Sub(p, i)
                                                           initTrellis.push(p * obEmission(i))
                                                       End Sub)
                               Return initTrellis
                           End Function,
            .recViterbi = Function(prevTrellis, obIndex, psiArrays, trellisSequence)
                              If (obIndex = obSequence.length) Then Return {psiArrays, trellisSequence}
                              Dim nextTrellis = HMM.states.map(Function(state, stateIndex)
                                                                   Dim trellisArr = prevTrellis.map(Function(prob, i)
                                                                                                        Dim trans = HMM.transMatrix(i)(stateIndex)
                                                                                                        Dim emiss = HMM.emissionMatrix(HMM.observables.indexOf(obSequence(obIndex)))(stateIndex)
                                                                                                        Return prob * trans * emiss
                                                                                                    End Function)
                                                                   Dim maximized = Math.Max(trellisArr)
                                                                   psiArrays(stateIndex).push(trellisArr.indexOf(maximized))
                                                                   Return maximized
                                                               End Function, {})
                              trellisSequence.push(nextTrellis)
                              Return this.recViterbi(nextTrellis, obIndex + 1, psiArrays, trellisSequence)
                          End Function,
            .termViterbi = Function(recTrellisPsi)
                               Dim finalTrellis = recTrellisPsi.trellisSequence(recTrellisPsi.trellisSequence.length - 1)
                               Dim maximizedProbability = Math.Max(finalTrellis)
                               recTrellisPsi.psiArrays.forEach(Sub(psiArr) psiArr.push(finalTrellis.indexOf(maximizedProbability)))
                               Return New With {.maximizedProbability = maximizedProbability, .psiArrays = recTrellisPsi.psiArrays}
                           End Function,
            .backViterbi = Function(psiArrays)
                               Dim backtraceObj = obSequence.reduce(Function(acc, currS, i)
                                                                        If (acc.length = 0) Then
                                                                            Dim finalPsiIndex = psiArrays(0).length - 1
                                                                            Dim finalPsi = psiArrays(0)(finalPsiIndex)
                                                                            acc.push(New With {.psi = finalPsi, .Index = finalPsiIndex})
                                                                            Return acc
                                                                        End If
                                                                        Dim prevPsi = acc(acc.length - 1)
                                                                        Dim psi = psiArrays(prevPsi.psi)(prevPsi.index - 1)
                                                                        acc.push(New With {.psi = psi, .Index = prevPsi.index - 1})
                                                                        Return acc
                                                                    End Function, {})
                               Return backtraceObj.reverse().map(Function(e) HMM.states(e.psi))
                           End Function
        }
    End Function

    Function Bayes(HMM)
        Return New With {
            .bayesTheorem = Function(ob, hState)
                                Dim hStateIndex = HMM.states.indexOf(hState)
                                Dim obIndex = HMM.observables.indexOf(ob)
                                Dim emissionProb = HMM.emissionMatrix(obIndex)(hStateIndex)
                                Dim initHState = HMM.initialProb(hStateIndex)
                                Dim obProb = HMM.emissionMatrix(obIndex).reduce(Function(total, em, i)
                                                                                    total += (em() * HMM.initialProb(i))
                                                                                    Return total
                                                                                End Function, 0)
                                Dim bayesResult = (emissionProb * initHState) / obProb
                                Return bayesResult
                            End Function
        }
    End Function

    Function Forward(HMM)
        Return New With {
            .forwardAlgorithm = Function(obSequence)
                                    Dim forward = forwardFactory(HMM, obSequence)
                                    Dim initAlphas = forward.initForward()
                                    Dim allAlphas = forward.recForward(initAlphas, 1, [initAlphas])
                                    Return New With {.alphas = allAlphas, .alphaF = forward.termForward(allAlphas)}
                                End Function
        }
    End Function

    Function Backward(HMM)
        Return New With {
            .backwardAlgorithm = Function(obSequence)
                                     Dim backward = backwardFactory(HMM, obSequence)
                                     Dim initBetas = HMM.states.map(Function(s) 1)
                                     Dim allBetas = backward.recBackward(initBetas, obSequence.length - 1, {initBetas})
                                     Return New With {.betas = allBetas, .betaF = backward.termBackward(allBetas)}
                                 End Function
        }
    End Function

    Function Viterbi(HMM)
        Return New With {
            .viterbiAlgorithm = Function(obSequence)
                                    Dim viterbi = viterbiFactory(HMM, obSequence)
                                    Dim initTrellis = viterbi.initViterbi()
                                    Dim psiArrays = HMM.states.map(Function(s) {null}) ' Initialization Of psi arrays Is equal To 0, but I use null because 0 could later represent a state index
                                    Dim recTrellisPsi = viterbi.recViterbi(initTrellis, 1, psiArrays, {initTrellis})
                                    Dim pTerm = viterbi.termViterbi(recTrellisPsi)
                                    Dim backtrace = viterbi.backViterbi(pTerm.psiArrays)
                                    Return New With {.stateSequence = backtrace, .trellisSequence = recTrellisPsi.trellisSequence, .terminationProbability = pTerm.maximizedProbability}
                                End Function
        }
    End Function

    Function BaumWelch(HMM)
        Return New With {
            .baumWelchAlgorithm = Function(obSequence)
                                      Dim forwardObj = Forward(HMM).forwardAlgorithm(obSequence)
                                      Dim backwardBetas = Backward(HMM).backwardAlgorithm(obSequence).betas.reverse()
                                      Dim EMSteps = EM(HMM, forwardObj, backwardBetas, obSequence)
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
                                      Return HMM(hiddenStates, observables, initProb)
                                  End Function
        }
    End Function

    Function MarkovChain(states, init)
        Dim info = New With {
            .states = states.map(Function(s) s.state),
            .transMatrix = states.map(Function(s) s.prob),
            .initialProb = init
        }

        Return Object.assign({}, info, calculateProb(info.transMatrix, init, states))
    End Function

    Function HMM(states, observables, init)
        Dim hmm = New With {
            .states = states.map(Function(s) s.state),
            .transMatrix = states.map(Function(s) s.prob),
            .initialProb = init,
            .observables = observables.map(Function(o) o.obs),
            .emissionMatrix = observables.map(Function(o) o.prob)
        }

        Return Object.assign({}, hmm, Bayes(hmm), Viterbi(hmm), Forward(hmm), Backward(hmm), BaumWelch(hmm))
    End Function
End Class

