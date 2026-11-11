Imports System

Namespace Ensemble

    ''' <summary>集成模型与单模型共用的评估指标</summary>
    Public Module EnsembleMetrics

        ''' <summary>分类准确率（输入是类别索引）</summary>
        Public Function Accuracy(predicted As Integer(), actual As Integer()) As Double
            RequireSameLength(predicted, actual, NameOf(predicted))

            If predicted.Length = 0 Then
                Return 0.0R
            End If

            Dim hit As Integer = 0

            For i As Integer = 0 To predicted.Length - 1
                If predicted(i) = actual(i) Then
                    hit += 1
                End If
            Next

            Return hit / CDbl(predicted.Length)
        End Function

        ''' <summary>
        ''' 多分类对数损失：<c>-mean(log(max(p[y], floor)))</c>，
        ''' 概率向量长度必须等于类别数。
        ''' </summary>
        Public Function LogLoss(probabilities As Double()(), yIndex As Integer(), classCount As Integer) As Double
            If probabilities Is Nothing Then
                Throw New ArgumentNullException(NameOf(probabilities))
            End If
            If yIndex Is Nothing Then
                Throw New ArgumentNullException(NameOf(yIndex))
            End If
            If probabilities.Length <> yIndex.Length Then
                Throw New ArgumentException(
                    $"the probability rows ({probabilities.Length}) and labels ({yIndex.Length}) do not agree!",
                    NameOf(probabilities))
            End If
            If probabilities.Length = 0 Then
                Return 0.0R
            End If

            Dim total As Double = 0.0R

            For i As Integer = 0 To probabilities.Length - 1
                Dim row As Double() = probabilities(i)
                Dim slot As Integer = yIndex(i)

                If row Is Nothing OrElse row.Length <> classCount Then
                    Throw New ArgumentException(
                        $"the probability row at index {i} has an invalid length!", NameOf(probabilities))
                End If
                If slot < 0 OrElse slot >= classCount Then
                    Throw New ArgumentOutOfRangeException(
                        NameOf(yIndex), $"the class index {slot} at row {i} is out of range [0, {classCount})!")
                End If

                Dim p As Double = row(slot)

                If Double.IsNaN(p) OrElse Double.IsInfinity(p) OrElse p < 0.0R Then
                    Throw New ArgumentException(
                        $"the probability at row {i} / class {slot} is invalid ({p})!", NameOf(probabilities))
                End If

                total += System.Math.Log(System.Math.Max(p, ProbabilityMath.ProbabilityFloor))
            Next

            Return -total / probabilities.Length
        End Function

        ''' <summary>混淆矩阵：行是实际类别，列是预测类别</summary>
        Public Function ConfusionMatrix(predicted As Integer(), actual As Integer(), classCount As Integer) As Integer(,)
            RequireSameLength(predicted, actual, NameOf(predicted))

            If classCount <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(classCount))
            End If

            Dim matrix As Integer(,) = New Integer(classCount - 1, classCount - 1) {}

            For i As Integer = 0 To predicted.Length - 1
                Dim actualSlot As Integer = actual(i)
                Dim predictedSlot As Integer = predicted(i)

                If actualSlot < 0 OrElse actualSlot >= classCount Then
                    Throw New ArgumentOutOfRangeException(NameOf(actual), $"actual class index {actualSlot} is out of range!")
                End If
                If predictedSlot < 0 OrElse predictedSlot >= classCount Then
                    Throw New ArgumentOutOfRangeException(NameOf(predicted), $"predicted class index {predictedSlot} is out of range!")
                End If

                matrix(actualSlot, predictedSlot) += 1
            Next

            Return matrix
        End Function

        ''' <summary>均方根误差</summary>
        Public Function RMSE(predicted As Double(), actual As Double()) As Double
            RequireSameLength(predicted, actual, NameOf(predicted))

            If predicted.Length = 0 Then
                Return 0.0R
            End If

            Dim sum As Double = 0.0R

            For i As Integer = 0 To predicted.Length - 1
                Dim d As Double = predicted(i) - actual(i)
                sum += d * d
            Next

            Return System.Math.Sqrt(sum / predicted.Length)
        End Function

        ''' <summary>平均绝对误差</summary>
        Public Function MAE(predicted As Double(), actual As Double()) As Double
            RequireSameLength(predicted, actual, NameOf(predicted))

            If predicted.Length = 0 Then
                Return 0.0R
            End If

            Dim sum As Double = 0.0R

            For i As Integer = 0 To predicted.Length - 1
                sum += System.Math.Abs(predicted(i) - actual(i))
            Next

            Return sum / predicted.Length
        End Function

        ''' <summary>决定系数 <c>1 - SSE/SST</c>，零方差时按惯例退化</summary>
        Public Function R2(predicted As Double(), actual As Double()) As Double
            RequireSameLength(predicted, actual, NameOf(predicted))

            If predicted.Length = 0 Then
                Return 0.0R
            End If

            Dim mean As Double = 0.0R

            For i As Integer = 0 To actual.Length - 1
                mean += actual(i)
            Next

            mean /= actual.Length

            Dim sse As Double = 0.0R
            Dim sst As Double = 0.0R

            For i As Integer = 0 To predicted.Length - 1
                sse += (predicted(i) - actual(i)) ^ 2
                sst += (actual(i) - mean) ^ 2
            Next

            Dim value As Double = If(sst > 0.0R, 1.0R - sse / sst, If(sse <= 0.0R, 1.0R, 0.0R))

            Return If(Double.IsNaN(value) OrElse Double.IsInfinity(value), 0.0R, value)
        End Function

        Private Sub RequireSameLength(predicted As Array, actual As Array, name As String)
            If predicted Is Nothing Then
                Throw New ArgumentNullException(name)
            End If
            If actual Is Nothing Then
                Throw New ArgumentNullException(NameOf(actual))
            End If
            If predicted.Length <> actual.Length Then
                Throw New ArgumentException(
                    $"the predicted values ({predicted.Length}) and actual values ({actual.Length}) do not agree!", name)
            End If
        End Sub

    End Module
End Namespace
