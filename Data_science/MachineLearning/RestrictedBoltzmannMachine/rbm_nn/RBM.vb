#Region "Microsoft.VisualBasic::5745e47ddb1e7dc33591406f939038aa, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\RBM.vb"

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

    '     Class RBM
    ' 
    '         Properties: HBias, HiddenSize, VBias, VisibleSize, Weights
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FreeEnergy, HiddenProbabilities, ReconstructionError, Reconstruct
    '                   SampleHidden, SampleVisible, ToString, VisibleProbabilities
    ' 
    '         Sub: addVisibleNodes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.functions
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.functions.doubledouble.rbm
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace nn.rbm

    ''' <summary>
    ''' 伯努利-伯努利受限玻尔兹曼机（Restricted Boltzmann Machine）。
    ''' 
    ''' 能量函数定义为 <c>E(v,h) = -v·a - h·b - v·W·h</c>，其中 <c>a</c> 为可见层偏置，
    ''' <c>b</c> 为隐藏层偏置，<c>W</c> 为两层之间的权重矩阵。当偏置全部为<c>0</c>的时候，
    ''' 模型退化为历史版本之中只使用权重矩阵的无偏置形式。
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/kennycason/rbm/tree/master
    ''' </remarks>
    Public Class RBM

        Private Shared ReadOnly SIGMOID As DoubleFunction = New Sigmoid()

        Private Shared ReadOnly ACTIVATION As DoubleDoubleFunction = New ActivationState()

        Public ReadOnly Property VisibleSize As Integer
            Get
                Return Weights.rows()
            End Get
        End Property

        Public ReadOnly Property HiddenSize As Integer
            Get
                Return Weights.columns()
            End Get
        End Property

        ''' <summary>
        ''' 可见层与隐藏层之间的权重矩阵，尺寸为<c>可见层单元数 × 隐藏层单元数</c>
        ''' </summary>
        Public Property Weights As DenseMatrix

        ''' <summary>
        ''' 可见层偏置<c>a</c>，长度为<see cref="VisibleSize"/>
        ''' </summary>
        Public Property VBias As Double()

        ''' <summary>
        ''' 隐藏层偏置<c>b</c>，长度为<see cref="HiddenSize"/>
        ''' </summary>
        Public Property HBias As Double()

        Public Sub New(visibleSize As Integer, hiddenSize As Integer, Optional bias As Double = 0.0)
            Weights = DenseMatrix.randomGaussian(visibleSize, hiddenSize)
            VBias = fillBias(visibleSize, bias)
            HBias = fillBias(hiddenSize, bias)
        End Sub

        Private Shared Function fillBias(size As Integer, value As Double) As Double()
            Dim v As Double() = New Double(size - 1) {}

            If value <> 0 Then
                For i As Integer = 0 To size - 1
                    v(i) = value
                Next
            End If

            Return v
        End Function

        ''' <summary>
        ''' 计算给定可见层状态的时候，隐藏层单元被激活的概率：<c>P(h=1|v) = σ(vW + b)</c>
        ''' </summary>
        Public Function HiddenProbabilities(visible As DenseMatrix) As DenseMatrix
            If visible Is Nothing Then
                Throw New ArgumentNullException(NameOf(visible))
            End If
            If visible.columns() <> VisibleSize Then
                Throw New ArgumentException($"visible layer expects {VisibleSize} units but the input matrix has {visible.columns()} columns")
            End If

            Return visible.dot(Weights).addRowVector(HBias).apply(SIGMOID)
        End Function

        ''' <summary>
        ''' 计算给定隐藏层状态的时候，可见层单元被激活的概率：<c>P(v=1|h) = σ(hW^T + a)</c>
        ''' </summary>
        Public Function VisibleProbabilities(hidden As DenseMatrix) As DenseMatrix
            If hidden Is Nothing Then
                Throw New ArgumentNullException(NameOf(hidden))
            End If
            If hidden.columns() <> HiddenSize Then
                Throw New ArgumentException($"hidden layer expects {HiddenSize} units but the input matrix has {hidden.columns()} columns")
            End If

            Return hidden.dot(Weights.transpose()).addRowVector(VBias).apply(SIGMOID)
        End Function

        ''' <summary>
        ''' 按照<c>P(h=1|v)</c>进行伯努利采样，得到隐藏层的二值状态。
        ''' </summary>
        Public Function SampleHidden(visible As DenseMatrix) As DenseMatrix
            Dim probabilities As DenseMatrix = HiddenProbabilities(visible)

            Return probabilities.apply(DenseMatrix.random(probabilities.rows(), HiddenSize), ACTIVATION)
        End Function

        ''' <summary>
        ''' 按照<c>P(v=1|h)</c>进行伯努利采样，得到可见层的二值状态。
        ''' </summary>
        Public Function SampleVisible(hidden As DenseMatrix) As DenseMatrix
            Dim probabilities As DenseMatrix = VisibleProbabilities(hidden)

            Return probabilities.apply(DenseMatrix.random(probabilities.rows(), VisibleSize), ACTIVATION)
        End Function

        ''' <summary>
        ''' 使用均值场的方式重构输入数据（不进行二值采样）：<c>v' = σ(σ(vW + b)W^T + a)</c>
        ''' </summary>
        Public Function Reconstruct(visible As DenseMatrix) As DenseMatrix
            Return VisibleProbabilities(HiddenProbabilities(visible))
        End Function

        ''' <summary>
        ''' 输入数据的总重构均方误差。
        ''' </summary>
        Public Function ReconstructionError(visible As DenseMatrix) As Double
            Dim reconstructed As DenseMatrix = Reconstruct(visible)
            Dim n As Integer = visible.rows() * visible.columns()

            Return visible.copy().subtract(reconstructed).pow(2).sum() / n
        End Function

        ''' <summary>
        ''' 计算每一个样本的自由能（把隐藏层解析地边缘化掉）：
        ''' <c>F(v) = -(a·v) - Σ_j softplus(b_j + Σ_i v_i W_ij)</c>
        ''' </summary>
        ''' <remarks>
        ''' 自由能是RBM训练的可靠监控指标：训练良好时，数据样本的平均自由能应该随着训练而下降。
        ''' </remarks>
        Public Function FreeEnergy(visible As DenseMatrix) As Double()
            If visible.columns() <> VisibleSize Then
                Throw New ArgumentException($"visible layer expects {VisibleSize} units but the input matrix has {visible.columns()} columns")
            End If

            Dim energies As Double() = New Double(visible.rows() - 1) {}

            For n As Integer = 0 To visible.rows() - 1
                Dim energy As Double = 0

                For i As Integer = 0 To VisibleSize - 1
                    energy -= VBias(i) * visible.get(n, i)
                Next

                For j As Integer = 0 To HiddenSize - 1
                    Dim z As Double = HBias(j)

                    For i As Integer = 0 To VisibleSize - 1
                        z += visible.get(n, i) * Weights.get(i, j)
                    Next

                    energy -= softplus(z)
                Next

                energies(n) = energy
            Next

            Return energies
        End Function

        ''' <summary>
        ''' <c>log(1 + exp(x))</c>的数值稳定实现，避免<c>exp(x)</c>在<c>x</c>较大的时候溢出。
        ''' </summary>
        Private Shared Function softplus(x As Double) As Double
            Return System.Math.Max(x, 0) + System.Math.Log(1 + System.Math.Exp(-System.Math.Abs(x)))
        End Function

        Public Sub addVisibleNodes(n As Integer)
            If n <= 0 Then
                Return
            End If

            Dim weights = DenseMatrix.make(VisibleSize + n, HiddenSize)

            ' copy original values
            For i = 0 To VisibleSize - 1
                For j = 0 To HiddenSize - 1
                    Call weights.set(i, j, Weights.get(i, j))
                Next
            Next
            ' randomly init new weights;
            For i = VisibleSize To weights.rows() - 1
                For j = 0 To HiddenSize - 1
                    Call weights.set(i, j, randf.NextGaussian() * 0.1)
                Next
            Next

            Dim bias As Double() = New Double(weights.rows() - 1) {}
            For i = 0 To VBias.Length - 1
                bias(i) = VBias(i)
            Next

            Me.Weights = weights
            Me.VBias = bias
        End Sub

        Public Overrides Function ToString() As String
            Return "RBM{" & "visibleSize=" & VisibleSize.ToString() & ", hiddenSize=" & HiddenSize.ToString() & ", weights=" & Weights.ToString() & "}"c.ToString()
        End Function

    End Class

End Namespace
