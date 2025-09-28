#Region "Microsoft.VisualBasic::84a2287d6603edc3b4688bc31b87af2e, Data_science\MachineLearning\DeepLearning\CNN\Layers\GaussianLayer.vb"

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

    '   Total Lines: 101
    '    Code Lines: 76 (75.25%)
    ' Comment Lines: 7 (6.93%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (17.82%)
    '     File Size: 4.30 KB


    '     Class GaussianLayer
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: forward, ToString
    ' 
    '         Sub: backward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.Math.Distributions

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
            Dim dL_dy() As Double = out_act.dw ' 上游梯度 ∂L/∂y
            Dim x() As Double = in_act.w
            Dim mu_vals() As Double = mu.w
            Dim sigma_vals() As Double = sigma.w

            ' 初始化本层参数的梯度
            Dim dL_dmu(sigma_vals.Length - 1) As Double
            Dim dL_dsigma(sigma_vals.Length - 1) As Double
            Dim dL_dbias(sigma_vals.Length - 1) As Double
            Dim dL_dx(sigma_vals.Length - 1) As Double ' 传向上一层的梯度

            For i As Integer = 0 To dL_dy.Length - 1
                ' 计算前向传播时的高斯函数输出f(x)
                Dim fx As Double = pnorm.ProbabilityDensity(x(i), mu_vals(i), sigma_vals(i))

                ' 计算中间导数 ∂f/∂μ, ∂f/∂σ, ∂f/∂x
                Dim diff As Double = (x(i) - mu_vals(i))
                Dim sigma_sq As Double = sigma_vals(i) * sigma_vals(i)
                Dim sigma_cubed As Double = sigma_sq * sigma_vals(i)

                Dim df_dmu As Double = fx * (diff / sigma_sq)
                Dim df_dsigma As Double = fx * ((diff * diff / sigma_cubed) - (1.0 / sigma_vals(i)))
                Dim df_dx As Double = -fx * (diff / sigma_sq) ' 注意负号

                ' 应用链式法则 ∂L/∂θ = (∂L/∂y) * (∂y/∂θ)
                dL_dbias(i) = dL_dy(i) * 1.0 ' ∂y/∂bias = 1
                dL_dmu(i) = dL_dy(i) * df_dmu
                dL_dsigma(i) = dL_dy(i) * df_dsigma
                dL_dx(i) = dL_dy(i) * df_dx ' 注意：如果前向是 y = x + f(x) + b, 则 ∂y/∂x = 1 + ∂f/∂x

                ' 将梯度存储到相应的DataBlock中
                mu.addGradient(i, dL_dmu(i))
                sigma.addGradient(i, dL_dsigma(i))
                biases.addGradient(i, dL_dbias(i))
                in_act.addGradient(i, dL_dx(i)) ' 注意：这里需要根据前向传播的实际公式调整。
                ' 如果前向是 y = f(x) + b, 则 dL_dx(i) 如上所述。
                ' 如果前向是 y = x + f(x) + b, 则 dL_dx(i) = dL_dy(i) * (1 + df_dx)
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
                w(i) = pnorm.ProbabilityDensity(x(i), mu(i), sigma(i)) + bias(i)
            Next

            Return out_act
        End Function

        Public Overrides Function ToString() As String
            Return "gaussian()"
        End Function
    End Class
End Namespace
