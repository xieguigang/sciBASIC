Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.MachineLearning.Transformer.Utils
Imports std = System.Math

Namespace Transformer
    Public Class MultiHeadAttention
        Private mask As Boolean
        Private embeddingSize As Integer
        Private nr_heads As Integer       ' Number of attention heads
        Private dk As Integer             ' Key dimension
        Private dv As Integer             ' Value dimension

        ' Learned linear input layers
        Private Qm, Km, Vm As Tensor()

        ' Learned output layer
        Private Wo As Tensor

        Private QmOptimizer, KmOptimizer, VmOptimizer As Optimizer()
        Private WoOptimizer As Optimizer


        Public Sub New(dk As Integer, dv As Integer, nr_heads As Integer, embeddingSize As Integer, mask As Boolean)
            Me.dk = dk
            Me.dv = dv
            Me.nr_heads = nr_heads
            Me.embeddingSize = embeddingSize
            Me.mask = mask

            InitializeLinearFilters()
            InitalizeOptimizers()
        End Sub

        Public Function Update(inputData As Tensor) As Tensor
            Dim Qf As Tensor() = Nothing, Kf As Tensor() = Nothing, Vf As Tensor() = Nothing
            ApplyLinearInputFilters(inputData, Qf, Kf, Vf)
            Return CalculateScaledMultiHeadedAttention(Qf, Kf, Vf)
        End Function

        Public Function Update(encoderOutput As Tensor, queries As Tensor) As Tensor
            Dim Qf As Tensor() = Nothing, Kf As Tensor() = Nothing, Vf As Tensor() = Nothing
            ApplyLinearInputFilters(encoderOutput, queries, Qf, Kf, Vf)
            Return CalculateScaledMultiHeadedAttention(Qf, Kf, Vf)
        End Function

        Private Sub ApplyLinearInputFilters(inputData As Tensor, <Out> ByRef Qf As Tensor(), <Out> ByRef Kf As Tensor(), <Out> ByRef Vf As Tensor())
            Qf = New Tensor(nr_heads - 1) {}
            Kf = New Tensor(nr_heads - 1) {}
            Vf = New Tensor(nr_heads - 1) {}

            For h = 0 To nr_heads - 1
                Qf(h) = Tensor.MatMul(inputData, Qm(h))
                Kf(h) = Tensor.MatMul(inputData, Km(h))
                Vf(h) = Tensor.MatMul(inputData, Vm(h))
            Next
        End Sub

        Private Sub ApplyLinearInputFilters(encoderOutput As Tensor, queries As Tensor, <Out> ByRef Qf As Tensor(), <Out> ByRef Kf As Tensor(), <Out> ByRef Vf As Tensor())
            Qf = New Tensor(nr_heads - 1) {}
            Kf = New Tensor(nr_heads - 1) {}
            Vf = New Tensor(nr_heads - 1) {}

            For h = 0 To nr_heads - 1
                Qf(h) = Tensor.MatMul(queries, Qm(h))
                Kf(h) = Tensor.MatMul(encoderOutput, Km(h))
                Vf(h) = Tensor.MatMul(encoderOutput, Vm(h))
            Next
        End Sub

        Private Function CalculateScaledMultiHeadedAttention(Qf As Tensor(), Kf As Tensor(), Vf As Tensor()) As Tensor
            Dim AttentionHeads = New Tensor(nr_heads - 1) {}

            For h = 0 To nr_heads - 1
                Dim AttentionFilter As Tensor = Tensor.MatMul(Qf(h), Kf(h).Transpose())
                Dim scaledAttentionFilter = AttentionFilter.Scale(1 / std.Sqrt(dk))
                If mask Then scaledAttentionFilter.Mask()
                AttentionHeads(h) = Tensor.MatMul(scaledAttentionFilter.Softmax(), Vf(h))
            Next

            ' Apply linear output layer to get the correct output size
            Dim C = Tensor.Concat(AttentionHeads)
            Return Tensor.MatMul(C, Wo)

        End Function

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            For h = 0 To nr_heads - 1
                KmOptimizer(h).MakeTrainingStep(learningRate, [step], Km(h))
                QmOptimizer(h).MakeTrainingStep(learningRate, [step], Qm(h))
                VmOptimizer(h).MakeTrainingStep(learningRate, [step], Vm(h))
            Next
            WoOptimizer.MakeTrainingStep(learningRate, [step], Wo)
        End Sub

        Private Sub InitializeLinearFilters()
            Qm = New Tensor(nr_heads - 1) {}
            Km = New Tensor(nr_heads - 1) {}
            Vm = New Tensor(nr_heads - 1) {}

            For h = 0 To nr_heads - 1
                Qm(h) = New Tensor(embeddingSize, dk)
                Km(h) = New Tensor(embeddingSize, dk)
                Vm(h) = New Tensor(embeddingSize, dv)

                Qm(h).GenerateNormalRandomValues()
                Km(h).GenerateNormalRandomValues()
                Vm(h).GenerateNormalRandomValues()
            Next

            Wo = New Tensor(dv * nr_heads, embeddingSize)
            Wo.GenerateNormalRandomValues()
        End Sub

        Private Sub InitalizeOptimizers()
            QmOptimizer = New Optimizer(nr_heads - 1) {}
            KmOptimizer = New Optimizer(nr_heads - 1) {}
            VmOptimizer = New Optimizer(nr_heads - 1) {}

            For h = 0 To nr_heads - 1
                QmOptimizer(h) = New Optimizer(Qm(h))
                KmOptimizer(h) = New Optimizer(Km(h))
                VmOptimizer(h) = New Optimizer(Vm(h))
            Next

            WoOptimizer = New Optimizer(Wo)
        End Sub

    End Class
End Namespace
