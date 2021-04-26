Imports Microsoft.VisualBasic.My.JavaScript

Public Class backwardFactory : Inherits HMMChainAlgorithm

    Sub New(HMM As HMM, obSequence As Chain)
        Call MyBase.New(HMM, obSequence)
    End Sub

    Public Function recBackward(prevBetas As Double(), j As Integer, betas As List(Of Double())) As Double()()
        Dim obIndex = j
        Dim nextTrellis As New List(Of Double)

        If (obIndex = 0) Then
            Return betas.ToArray
        End If

        For s As Integer = 0 To HMM.states.Length - 1
            Dim trellisArr As New List(Of Double)
            Dim si As Integer = s

            prevBetas.ForEach(Sub(prob, i)
                                  Dim trans = HMM.transMatrix(si)(i)
                                  Dim emiss = HMM.emissionMatrix(HMM.observables.IndexOf(obSequence(obIndex)))(i)
                                  trellisArr.Add(prob * trans * emiss)
                              End Sub)
            nextTrellis.Add(trellisArr.reduce(Function(tot, curr) tot + curr))
        Next

        betas.Add(nextTrellis.ToArray)

        Return recBackward(nextTrellis.ToArray, obIndex - 1, betas)
    End Function

    Public Function termBackward(betas As Double()()) As Double
        Dim finalBetas = betas(betas.Length - 1) _
            .reduce(Function(tot As List(Of Double), curr As Double, i As Integer)
                        Dim obIndex = HMM.observables.IndexOf(obSequence(0))
                        Dim obEmission = HMM.emissionMatrix(obIndex)
                        tot.Add(curr * HMM.initialProb(i) * obEmission(i))
                        Return tot
                    End Function, New List(Of Double))

        Return finalBetas.reduce(Function(tot, Val) tot + Val, 0.0)
    End Function
End Class
