Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.Math.Distributions
Imports std = System.Math

Namespace CNN.layers

    Public Class GaussianLayer : Inherits DataLink
        Implements Layer

        Public ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                Yield New BackPropResult(mu.w, mu.dw, l1_decay_mul, l2_decay_mul)
                Yield New BackPropResult(sigma.w, sigma.dw, l1_decay_mul, l2_decay_mul)
                Yield New BackPropResult(biases.w, biases.dw, 0, 0)
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Gaussian
            End Get
        End Property

        Dim w As Integer, h As Integer, depth As Integer
        Dim mu As DataBlock
        Dim sigma As DataBlock
        Dim biases As DataBlock
        Dim l1_decay_mul As Double = 0.0
        Dim l2_decay_mul As Double = 1.0

        Sub New(def As OutputDefinition)
            w = def.outX
            h = def.outY
            depth = def.depth
            mu = New DataBlock(w, h, depth)
            sigma = New DataBlock(w, h, depth)
            biases = New DataBlock(w, h, depth)
        End Sub

        Public Sub backward() Implements Layer.backward
            Dim V = in_act.clearGradient
            Dim chain_grad = out_act.dw
            Dim mu_dw = mu.dw
            Dim sigma_dw = sigma.dw
            Dim biases_dw = biases.dw

            For i As Integer = 0 To chain_grad.Length - 1
                V.addGradient(i, chain_grad(i) * std.Exp(mu_dw(i)) * 2 * sigma_dw(i) ^ -4)
                mu.addGradient(i, V.w(i) * chain_grad(i))
                sigma.addGradient(i, V.w(i) * chain_grad(i))
                biases.addGradient(i, chain_grad(i))
            Next
        End Sub

        Public Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            in_act = db
            out_act = db.clone

            Dim x As Double() = in_act.w
            Dim w As Double() = out_act.w
            Dim mu As Double() = Me.mu.w
            Dim sigma As Double() = Me.sigma.w
            Dim bias As Double() = Me.biases.w

            For i As Integer = 0 To w.Length - 1
                w(i) += pnorm.ProbabilityDensity(x(i), mu(i), sigma(i)) + bias(i)
            Next

            Return out_act
        End Function

        Public Overrides Function ToString() As String
            Return "gaussian()"
        End Function
    End Class
End Namespace

