Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.struct

Namespace GraphEmbedding.complex_R

    Public Class RGradients
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
                ' 
                ' 				if(real_body > real_head){
                ' 				    //Calculate gradients of head
                ' 				    Real_MatrixRGradient.add(ruleHead.rid(), p, -1 * dMu * confidence);
                ' 				    //Calculate gradients of body
                ' 				    Real_MatrixRGradient.add(ruleBody.rid(), p, dMu * confidence);
                ' 				}
                'Calculate gradients of head
                Real_MatrixRGradient.add(ruleHead.rid(), p, -2 * (real_body - real_head) * dMu * confidence)
                'Calculate gradients of body
                Real_MatrixRGradient.add(ruleBody.rid(), p, 2 * (real_body - real_head) * dMu * confidence)
                'Calculate gradinets of head
                Imag_MatrixRGradient.add(ruleHead.rid(), p, -2 * (imag_body - imag_head) * dMu * confidence)
                'Calculate gradients of body
                Imag_MatrixRGradient.add(ruleBody.rid(), p, 2 * (imag_body - imag_head) * ruleBody.direction() * dMu * confidence)
            Next
        End Sub
    End Class

End Namespace
