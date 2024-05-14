#Region "Microsoft.VisualBasic::e0138834c495bca6ecf2bd45b72987c5, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\complex_NNE_AER\AERGradients.vb"

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

    '   Total Lines: 49
    '    Code Lines: 39
    ' Comment Lines: 4
    '   Blank Lines: 6
    '     File Size: 2.36 KB


    '     Class AERGradients
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: calculateGradients
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.struct

Namespace GraphEmbedding.complex_NNE_AER

    Public Class AERGradients
        Public dRule As Rule
        Public Real_MatrixR As Matrix
        Public Imag_MatrixR As Matrix
        Public Real_MatrixRGradient As Matrix
        Public Imag_MatrixRGradient As Matrix
        Public dMu As Double
        Public dLambda As Double

        Public Sub New(inRule As Rule, inReal_MatrixR As Matrix, inImag_MatrixR As Matrix, inReal_MatrixRGradient As Matrix, inImag_MatrixRGradient As Matrix, inMu As Double, inLambda As Double)
            dRule = inRule
            Real_MatrixR = inReal_MatrixR
            Imag_MatrixR = inImag_MatrixR
            Real_MatrixRGradient = inReal_MatrixRGradient
            Imag_MatrixRGradient = inImag_MatrixRGradient
            dMu = inMu
            dLambda = inLambda
        End Sub

        Public Overridable Sub calculateGradients()
            Dim confidence As Double = dRule.confidence()
            Dim ruleHead As Relation = dRule.relations()(0)
            Dim ruleBody As Relation = dRule.relations()(1)

            Dim numOfFactors As Integer = Real_MatrixR.columns()
            For p = 0 To numOfFactors - 1
                Dim real_body As Double = Real_MatrixR.get(ruleBody.rid(), p)
                Dim imag_body As Double = Imag_MatrixR.get(ruleBody.rid(), p) * ruleBody.direction()
                Dim real_head As Double = Real_MatrixR.get(ruleHead.rid(), p)
                Dim imag_head As Double = Imag_MatrixR.get(ruleHead.rid(), p)
                If real_body > real_head Then
                    'Calculate gradients of head
                    Real_MatrixRGradient.add(ruleHead.rid(), p, -1 * dMu * confidence)
                    'Calculate gradients of body
                    Real_MatrixRGradient.add(ruleBody.rid(), p, dMu * confidence)
                End If
                'Calculate gradinets of head
                Imag_MatrixRGradient.add(ruleHead.rid(), p, -2 * (imag_body - imag_head) * dMu * confidence)
                'Calculate gradients of body
                Imag_MatrixRGradient.add(ruleBody.rid(), p, 2 * (imag_body - imag_head) * ruleBody.direction() * dMu * confidence)
            Next
        End Sub
    End Class

End Namespace
