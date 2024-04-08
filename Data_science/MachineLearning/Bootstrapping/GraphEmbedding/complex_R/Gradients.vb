﻿Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.struct
Imports std = System.Math

Namespace GraphEmbedding.complex_R

    Public Class Gradients
        Public curTriple As Triple
        Public dLabel As Double
        Public Real_MatrixE As Matrix
        Public Real_MatrixR As Matrix
        Public Imag_MatrixE As Matrix
        Public Imag_MatrixR As Matrix
        Public Real_MatrixEGradient As Matrix
        Public Real_MatrixRGradient As Matrix
        Public Imag_MatrixEGradient As Matrix
        Public Imag_MatrixRGradient As Matrix
        Public dLambda As Double
        Public dSize As Double

        Public Sub New(inTriple As Triple, inLabel As Double, in_Real_MatrixE As Matrix, in_Real_MatrixR As Matrix, in_Imag_MatrixE As Matrix, in_Imag_MatrixR As Matrix, in_Real_MatrixEGradient As Matrix, in_Real_MatrixRGradient As Matrix, in_Imag_MatrixEGradient As Matrix, in_Imag_MatrixRGradient As Matrix, inLambda As Double, inSize As Double)
            curTriple = inTriple
            dLabel = inLabel
            Real_MatrixE = in_Real_MatrixE
            Real_MatrixR = in_Real_MatrixR
            Imag_MatrixE = in_Imag_MatrixE
            Imag_MatrixR = in_Imag_MatrixR
            Real_MatrixEGradient = in_Real_MatrixEGradient
            Real_MatrixRGradient = in_Real_MatrixRGradient
            Imag_MatrixEGradient = in_Imag_MatrixEGradient
            Imag_MatrixRGradient = in_Imag_MatrixRGradient
            dLambda = inLambda
            dSize = inSize
        End Sub

        Public Overridable Function sigmoid(x As Double) As Double
            Dim y = 0.0
            If x > 10.0 Then
                y = 1.0
            ElseIf x < -10.0 Then
                y = 0.0
            Else
                y = 1.0 / (1.0 + std.Exp(-x))
            End If
            Return y
        End Function

        Public Overridable Sub calculateGradients()
            Dim iNumberOfFactors As Integer = Real_MatrixE.columns()
            Dim iHead As Integer = curTriple.head()
            Dim iTail As Integer = curTriple.tail()
            Dim iRelation As Integer = curTriple.relation()
            Dim dEta = 0.0
            For p = 0 To iNumberOfFactors - 1
                dEta += Real_MatrixE.get(iHead, p) * Real_MatrixR.get(iRelation, p) * Real_MatrixE.get(iTail, p) - Imag_MatrixE.get(iHead, p) * Imag_MatrixR.get(iRelation, p) * Real_MatrixE.get(iTail, p) + Real_MatrixE.get(iHead, p) * Imag_MatrixR.get(iRelation, p) * Imag_MatrixE.get(iTail, p) + Imag_MatrixE.get(iHead, p) * Real_MatrixR.get(iRelation, p) * Imag_MatrixE.get(iTail, p)
            Next
            Dim dPartial = -dLabel * sigmoid(-dLabel * dEta)

            For p = 0 To iNumberOfFactors - 1
                Dim dRealHead = 0.0
                Dim dRealTail = 0.0
                Dim dRealRel = 0.0
                Dim dImagHead = 0.0
                Dim dImagTail = 0.0
                Dim dImagRel = 0.0

                dRealHead = Real_MatrixR.get(iRelation, p) * Real_MatrixE.get(iTail, p) + Imag_MatrixR.get(iRelation, p) * Imag_MatrixE.get(iTail, p)
                dRealTail = Real_MatrixR.get(iRelation, p) * Real_MatrixE.get(iHead, p) - Imag_MatrixR.get(iRelation, p) * Imag_MatrixE.get(iHead, p)
                dRealRel = Real_MatrixE.get(iHead, p) * Real_MatrixE.get(iTail, p) + Imag_MatrixE.get(iHead, p) * Imag_MatrixE.get(iTail, p)
                dImagHead = Real_MatrixR.get(iRelation, p) * Imag_MatrixE.get(iTail, p) - Imag_MatrixR.get(iRelation, p) * Real_MatrixE.get(iTail, p)
                dImagTail = Real_MatrixR.get(iRelation, p) * Imag_MatrixE.get(iHead, p) + Imag_MatrixR.get(iRelation, p) * Real_MatrixE.get(iHead, p)
                dImagRel = Real_MatrixE.get(iHead, p) * Imag_MatrixE.get(iTail, p) - Imag_MatrixE.get(iHead, p) * Real_MatrixE.get(iTail, p)

                Real_MatrixEGradient.add(iHead, p, (dPartial * dRealHead + 2.0 * (dLambda / iNumberOfFactors) * Real_MatrixE.get(iHead, p)) / dSize)
                Real_MatrixEGradient.add(iTail, p, (dPartial * dRealTail + 2.0 * (dLambda / iNumberOfFactors) * Real_MatrixE.get(iTail, p)) / dSize)
                Real_MatrixRGradient.add(iRelation, p, (dPartial * dRealRel + 2.0 * (dLambda / iNumberOfFactors) * Real_MatrixR.get(iRelation, p)) / dSize)
                Imag_MatrixEGradient.add(iHead, p, (dPartial * dImagHead + 2.0 * (dLambda / iNumberOfFactors) * Imag_MatrixE.get(iHead, p)) / dSize)
                Imag_MatrixEGradient.add(iTail, p, (dPartial * dImagTail + 2.0 * (dLambda / iNumberOfFactors) * Imag_MatrixE.get(iTail, p)) / dSize)
                Imag_MatrixRGradient.add(iRelation, p, (dPartial * dImagRel + 2.0 * (dLambda / iNumberOfFactors) * Imag_MatrixR.get(iRelation, p)) / dSize)
            Next
        End Sub
    End Class

End Namespace
