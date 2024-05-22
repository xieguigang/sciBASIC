#Region "Microsoft.VisualBasic::834bd397716749a4a0b1f9dde7e5f434, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\complex_R\AdaGrad.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 120
    '    Code Lines: 109 (90.83%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (9.17%)
    '     File Size: 6.40 KB


    '     Class AdaGrad
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: gradientDescent
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.struct
Imports std = System.Math

Namespace GraphEmbedding.complex_R

    Public Class AdaGrad
        Public lstPosTriples As List(Of Triple)
        Public lstHeadNegTriples As List(Of Triple)
        Public lstTailNegTriples As List(Of Triple)
        Public lstRules As List(Of Rule)
        Public Real_MatrixE As Matrix
        Public Real_MatrixR As Matrix
        Public Imag_MatrixE As Matrix
        Public Imag_MatrixR As Matrix
        Public Real_MatrixEGradient As Matrix
        Public Real_MatrixRGradient As Matrix
        Public Imag_MatrixEGradient As Matrix
        Public Imag_MatrixRGradient As Matrix
        Public Real_MatrixEGSquare As Matrix
        Public Real_MatrixRGSquare As Matrix
        Public Imag_MatrixEGSquare As Matrix
        Public Imag_MatrixRGSquare As Matrix
        Public dGamma As Double
        Public dLambda As Double
        Public dMu As Double

        Public Sub New(inLstPosTriples As List(Of Triple), inLstHeadNegTriples As List(Of Triple), inLstTailNegTriples As List(Of Triple), inLstRules As List(Of Rule), in_Real_MatrixE As Matrix, in_Real_MatrixR As Matrix, in_Imag_MatrixE As Matrix, in_Imag_MatrixR As Matrix, in_Real_MatrixEGradient As Matrix, in_Real_MatrixRGradient As Matrix, in_Imag_MatrixEGradient As Matrix, in_Imag_MatrixRGradient As Matrix, in_Real_MatrixEGSquare As Matrix, in_Real_MatrixRGSquare As Matrix, in_Imag_MatrixEGSquare As Matrix, in_Imag_MatrixRGSquare As Matrix, inGamma As Double, inLambda As Double, inMu As Double)
            lstPosTriples = inLstPosTriples
            lstHeadNegTriples = inLstHeadNegTriples
            lstTailNegTriples = inLstTailNegTriples
            lstRules = inLstRules
            Real_MatrixE = in_Real_MatrixE
            Real_MatrixR = in_Real_MatrixR
            Imag_MatrixE = in_Imag_MatrixE
            Imag_MatrixR = in_Imag_MatrixR
            Real_MatrixEGradient = in_Real_MatrixEGradient
            Real_MatrixRGradient = in_Real_MatrixRGradient
            Imag_MatrixEGradient = in_Imag_MatrixEGradient
            Imag_MatrixRGradient = in_Imag_MatrixRGradient
            Real_MatrixEGSquare = in_Real_MatrixEGSquare
            Real_MatrixRGSquare = in_Real_MatrixRGSquare
            Imag_MatrixEGSquare = in_Imag_MatrixEGSquare
            Imag_MatrixRGSquare = in_Imag_MatrixRGSquare
            dGamma = inGamma
            dLambda = inLambda
            dMu = inMu
        End Sub

        Public Overridable Sub gradientDescent()
            Real_MatrixEGradient.ToValue = 0.0
            Real_MatrixRGradient.ToValue = 0.0
            Imag_MatrixEGradient.ToValue = 0.0
            Imag_MatrixRGradient.ToValue = 0.0

            Dim iSize = lstPosTriples.Count + lstHeadNegTriples.Count + lstTailNegTriples.Count
            For iID = 0 To lstPosTriples.Count - 1
                Dim PosTriple = lstPosTriples(iID)
                Dim posGradients As Gradients = New Gradients(PosTriple, 1.0, Real_MatrixE, Real_MatrixR, Imag_MatrixE, Imag_MatrixR, Real_MatrixEGradient, Real_MatrixRGradient, Imag_MatrixEGradient, Imag_MatrixRGradient, dLambda, iSize)
                posGradients.calculateGradients()
            Next
            For iID = 0 To lstHeadNegTriples.Count - 1
                Dim HeadNegTriple = lstHeadNegTriples(iID)
                Dim headGradients As Gradients = New Gradients(HeadNegTriple, -1.0, Real_MatrixE, Real_MatrixR, Imag_MatrixE, Imag_MatrixR, Real_MatrixEGradient, Real_MatrixRGradient, Imag_MatrixEGradient, Imag_MatrixRGradient, dLambda, iSize)
                headGradients.calculateGradients()
            Next
            For iID = 0 To lstTailNegTriples.Count - 1
                Dim TailNegTriple = lstTailNegTriples(iID)
                Dim tailGradients As Gradients = New Gradients(TailNegTriple, -1.0, Real_MatrixE, Real_MatrixR, Imag_MatrixE, Imag_MatrixR, Real_MatrixEGradient, Real_MatrixRGradient, Imag_MatrixEGradient, Imag_MatrixRGradient, dLambda, iSize)
                tailGradients.calculateGradients()
            Next
            For iID = 0 To lstRules.Count - 1
                Dim rule = lstRules(iID)
                Dim gradients As RGradients = New RGradients(rule, Real_MatrixR, Imag_MatrixR, Real_MatrixRGradient, Imag_MatrixRGradient, dMu, dLambda)
                gradients.calculateGradients()
            Next
            Real_MatrixEGradient.rescaleByRow()
            Real_MatrixRGradient.rescaleByRow()
            Imag_MatrixEGradient.rescaleByRow()
            Imag_MatrixRGradient.rescaleByRow()

            For i = 0 To Real_MatrixE.rows() - 1
                For j = 0 To Real_MatrixE.columns() - 1
                    Dim dG = Real_MatrixEGradient.get(i, j)
                    Real_MatrixEGSquare.add(i, j, dG * dG)
                    Dim dH = std.Sqrt(Real_MatrixEGSquare.get(i, j)) + 0.00000001
                    Real_MatrixE.add(i, j, -1.0 * dGamma * dG / dH)
                Next
            Next

            For i = 0 To Real_MatrixR.rows() - 1
                For j = 0 To Real_MatrixR.columns() - 1
                    Dim dG = Real_MatrixRGradient.get(i, j)
                    Real_MatrixRGSquare.add(i, j, dG * dG)
                    Dim dH = std.Sqrt(Real_MatrixRGSquare.get(i, j)) + 0.00000001
                    Real_MatrixR.add(i, j, -1.0 * dGamma * dG / dH)
                Next
            Next

            For i = 0 To Imag_MatrixE.rows() - 1
                For j = 0 To Imag_MatrixE.columns() - 1
                    Dim dG = Imag_MatrixEGradient.get(i, j)
                    Imag_MatrixEGSquare.add(i, j, dG * dG)
                    Dim dH = std.Sqrt(Imag_MatrixEGSquare.get(i, j)) + 0.00000001
                    Imag_MatrixE.add(i, j, -1.0 * dGamma * dG / dH)

                Next
            Next

            For i = 0 To Imag_MatrixR.rows() - 1
                For j = 0 To Imag_MatrixR.columns() - 1
                    Dim dG = Imag_MatrixRGradient.get(i, j)
                    Imag_MatrixRGSquare.add(i, j, dG * dG)
                    Dim dH = std.Sqrt(Imag_MatrixRGSquare.get(i, j)) + 0.00000001
                    Imag_MatrixR.add(i, j, -1.0 * dGamma * dG / dH)
                Next
            Next
        End Sub
    End Class

End Namespace
