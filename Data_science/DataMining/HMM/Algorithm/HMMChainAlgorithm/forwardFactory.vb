Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm.HMMChainAlgorithm

    Public Class forwardFactory : Inherits HMMChainAlgorithm

        Sub New(HMM As HMM, obSequence As Chain)
            Call MyBase.New(HMM, obSequence)
        End Sub

        Public Function initForward() As List(Of Double)
            Dim initTrellis As New List(Of Double)
            Dim obIndex = HMM.observables.IndexOf(obSequence(0))
            Dim obEmission = HMM.emissionMatrix(obIndex)
            HMM.initialProb.ForEach(Sub(p, i) initTrellis.Add(p * obEmission(i)))
            Return initTrellis
        End Function

        Public Function recForward(prevTrellis As Double(), j As Integer, alphas As List(Of List(Of Double))) As List(Of List(Of Double))
            Dim obIndex = j
            If (obIndex = obSequence.length) Then
                Return alphas
            End If
            Dim nextTrellis As New List(Of Double)
            For s As Integer = 0 To HMM.states.Length - 1
                Dim trellisArr As New List(Of Double)
                Dim si As Integer = s

                Call prevTrellis _
                    .ForEach(Sub(prob As Double, i As Integer)
                                 Dim trans = HMM.transMatrix(i)(si)
                                 Dim emiss = HMM.emissionMatrix(HMM.observables.IndexOf(obSequence(obIndex)))(si)
                                 trellisArr.Add(prob * trans * emiss)
                             End Sub)

                nextTrellis.Add(trellisArr.reduce(Function(tot, curr) tot + curr, 0.0))
            Next
            alphas.Add(nextTrellis)
            Return recForward(nextTrellis.ToArray, obIndex + 1, alphas)
        End Function

        Public Function termForward(alphas As List(Of List(Of Double))) As Double
            Return alphas(alphas.Count - 1).reduce(Function(tot, val) tot + val, 0.0)
        End Function
    End Class
End Namespace