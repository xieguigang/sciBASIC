' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning
'  Data_science\MachineLearning\MachineLearning\MultilayerPerceptron\LossFunctions.vb
' /********************************************************************************/

Imports System
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace MultilayerPerceptron

    ''' <summary>多层感知机使用的损失函数与输出层变换。</summary>
    Public Module LossFunctions

        ''' <summary>
        ''' 对矩阵的每一行执行 Softmax 归一化，结果直接覆盖输入矩阵。
        ''' </summary>
        Public Sub Softmax(matrix As NumericMatrix)
            If matrix Is Nothing Then
                Throw New ArgumentNullException(NameOf(matrix))
            End If

            For i As Integer = 0 To matrix.RowDimension - 1
                Dim maxValue As Double = Double.NegativeInfinity

                For j As Integer = 0 To matrix.ColumnDimension - 1
                    If matrix(i, j) > maxValue Then
                        maxValue = matrix(i, j)
                    End If
                Next

                If Double.IsInfinity(maxValue) OrElse Double.IsNaN(maxValue) Then
                    Throw New InvalidOperationException("Softmax 输入包含非有限值")
                End If

                Dim sum As Double = 0.0

                For j As Integer = 0 To matrix.ColumnDimension - 1
                    Dim e As Double = std.Exp(matrix(i, j) - maxValue)
                    matrix(i, j) = e
                    sum += e
                Next

                For j As Integer = 0 To matrix.ColumnDimension - 1
                    matrix(i, j) /= sum
                Next
            Next
        End Sub

        ''' <summary>计算分类任务的平均交叉熵损失。</summary>
        Public Function CrossEntropy(probabilities As NumericMatrix, labels As NumericMatrix) As Double
            AssertShape(probabilities, labels)
            Dim total As Double = 0.0

            For i As Integer = 0 To probabilities.RowDimension - 1
                For j As Integer = 0 To probabilities.ColumnDimension - 1
                    Dim p As Double = probabilities(i, j)

                    If p < 0 OrElse p > 1 OrElse Double.IsNaN(p) Then
                        Throw New InvalidOperationException("交叉熵损失要求输入为合法的概率值，请先执行 Softmax")
                    End If

                    If labels(i, j) <> 0.0 Then
                        total -= labels(i, j) * std.Log(std.Max(p, 0.0000000001))
                    End If
                Next
            Next

            Return total / probabilities.RowDimension
        End Function

        ''' <summary>计算回归任务的均方误差损失。</summary>
        Public Function MeanSquaredError(predictions As NumericMatrix, labels As NumericMatrix) As Double
            AssertShape(predictions, labels)
            Dim total As Double = 0.0

            For i As Integer = 0 To predictions.RowDimension - 1
                For j As Integer = 0 To predictions.ColumnDimension - 1
                    Dim diff As Double = predictions(i, j) - labels(i, j)
                    total += diff * diff
                Next
            Next

            Return total / (predictions.RowDimension * predictions.ColumnDimension)
        End Function

        ''' <summary>
        ''' 计算输出层梯度 dL/dz。分类任务假定已经施加 Softmax，
        ''' 因而梯度为 <c>(p - y)</c>；回归任务使用 <c>(p - y)</c>。
        ''' </summary>
        Public Function OutputDelta(predictions As NumericMatrix, labels As NumericMatrix,
                                    task As MlpTask) As Double()()
            AssertShape(predictions, labels)

            Dim batch As Integer = predictions.RowDimension
            Dim outputs As Integer = predictions.ColumnDimension
            Dim delta As Double()() = New Double(batch - 1)() {}

            For i As Integer = 0 To batch - 1
                delta(i) = New Double(outputs - 1) {}

                For j As Integer = 0 To outputs - 1
                    Dim factor As Double = If(task = MlpTask.Classification, 1.0, 2.0 / outputs)
                    delta(i)(j) = (predictions(i, j) - labels(i, j)) * factor
                Next
            Next

            Return delta
        End Function

        Private Sub AssertShape(a As NumericMatrix, b As NumericMatrix)
            If a Is Nothing OrElse b Is Nothing Then
                Throw New ArgumentNullException(If(a Is Nothing, NameOf(a), NameOf(b)))
            End If
            If a.RowDimension <> b.RowDimension OrElse a.ColumnDimension <> b.ColumnDimension Then
                Throw New ArgumentException("预测值与标签的矩阵形状不一致")
            End If
        End Sub

    End Module

End Namespace
