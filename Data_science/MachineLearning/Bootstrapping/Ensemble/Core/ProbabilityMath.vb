Imports System
Imports System.Runtime.CompilerServices

Namespace Ensemble

    ''' <summary>概率向量的展开、归一化、类别对齐与 argmax 工具</summary>
    Public Module ProbabilityMath

        ''' <summary>概率下溢截断，保证 <c>log(0)</c> 不会出现</summary>
        Public Const ProbabilityFloor As Double = 0.000000000000001R

        ''' <summary>把二分类单概率展开为 <c>[1-p, p]</c></summary>
        Public Function ExpandBinary(p As Double) As Double()
            If Double.IsNaN(p) OrElse p < 0.0R OrElse p > 1.0R Then
                Throw New ArgumentOutOfRangeException(NameOf(p), $"the binary probability must be within [0,1], but got {p}!")
            End If

            Return New Double() {1.0R - p, p}
        End Function

        ''' <summary>
        ''' 校验概率向量并做归一化；全零向量视为非法输入而不是静默返回均匀分布。
        ''' </summary>
        Public Function Normalize(probability As Double(), Optional context As String = Nothing) As Double()
            If probability Is Nothing Then
                Throw New ArgumentNullException(NameOf(probability))
            End If
            If probability.Length = 0 Then
                Throw New ArgumentException("the probability vector can not be empty!", NameOf(probability))
            End If

            Dim result As Double() = New Double(probability.Length - 1) {}
            Dim total As Double = 0.0R

            For i As Integer = 0 To probability.Length - 1
                Dim p As Double = probability(i)

                If Double.IsNaN(p) OrElse Double.IsInfinity(p) OrElse p < 0.0R Then
                    Throw New ArgumentException(
                        $"the probability at index {i} is invalid ({p}){(If(String.IsNullOrEmpty(context), "", " in " & context))}!",
                        NameOf(probability))
                End If

                result(i) = p
                total += p
            Next

            If total <= 0.0R Then
                Throw New ArgumentException(
                    $"the probability vector sums to zero{(If(String.IsNullOrEmpty(context), "", " in " & context))}!",
                    NameOf(probability))
            End If

            For i As Integer = 0 To result.Length - 1
                result(i) /= total
            Next

            Return result
        End Function

        ''' <summary>最大值所在下标（数值相等时取较小下标）</summary>
        Public Function ArgMax(probability As Double()) As Integer
            If probability Is Nothing OrElse probability.Length = 0 Then
                Throw New ArgumentException("the probability vector can not be empty!", NameOf(probability))
            End If

            Dim best As Integer = 0

            For i As Integer = 1 To probability.Length - 1
                If probability(i) > probability(best) Then
                    best = i
                End If
            Next

            Return best
        End Function

        ''' <summary>
        ''' 把 <paramref name="source"/> 的概率向量按类别对齐到 <paramref name="targetClasses"/>。
        ''' 目标类别若在源中不存在则填 0。
        ''' </summary>
        Public Function AlignTo(source As Double(),
                                sourceClasses As Double(),
                                targetClasses As Double()) As Double()
            If source Is Nothing OrElse sourceClasses Is Nothing OrElse targetClasses Is Nothing Then
                Throw New ArgumentNullException(NameOf(source))
            End If
            If source.Length <> sourceClasses.Length Then
                Throw New ArgumentException(
                    $"the probability vector ({source.Length}) and its classes ({sourceClasses.Length}) do not agree!",
                    NameOf(source))
            End If

            Dim result As Double() = New Double(targetClasses.Length - 1) {}

            For j As Integer = 0 To targetClasses.Length - 1
                Dim slot As Integer = Array.IndexOf(sourceClasses, targetClasses(j))

                If slot >= 0 Then
                    result(j) = source(slot)
                End If
            Next

            Return result
        End Function

        ''' <summary>
        ''' 把概率向量裁剪到下溢阈值以上并重新归一化，用于累加大量基模型输出后的数值稳定。
        ''' </summary>
        Public Function Stabilize(probability As Double()) As Double()
            If probability Is Nothing Then
                Throw New ArgumentNullException(NameOf(probability))
            End If

            Dim result As Double() = New Double(probability.Length - 1) {}
            Dim total As Double = 0.0R

            For i As Integer = 0 To probability.Length - 1
                Dim p As Double = probability(i)

                If Double.IsNaN(p) OrElse Double.IsInfinity(p) OrElse p < 0.0R Then
                    Throw New ArgumentException($"the probability at index {i} is invalid ({p})!", NameOf(probability))
                End If

                result(i) = System.Math.Max(p, ProbabilityFloor)
                total += result(i)
            Next

            If total <= 0.0R Then
                Throw New ArgumentException("the probability vector sums to zero!", NameOf(probability))
            End If

            For i As Integer = 0 To result.Length - 1
                result(i) /= total
            Next

            Return result
        End Function

        ''' <summary>
        ''' 校验并归一化集成权重：为空时等权；否则长度必须等于模型数，
        ''' 全部非负且和大于 0。
        ''' </summary>
        Public Function NormalizeWeights(weights As Double(), modelCount As Integer) As Double()
            If modelCount <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(modelCount), $"the model count must be positive, but got {modelCount}!")
            End If

            Dim result As Double() = New Double(modelCount - 1) {}

            If weights Is Nothing OrElse weights.Length = 0 Then
                ' 等权也必须归一化：回归器直接做加权求和，
                ' 若返回 [1,1,...] 会把预测值放大 modelCount 倍。
                Dim even As Double = 1.0R / modelCount

                For i As Integer = 0 To modelCount - 1
                    result(i) = even
                Next

                Return result
            End If

            If weights.Length <> modelCount Then
                Throw New ArgumentException(
                    $"the weight count ({weights.Length}) does not match the model count ({modelCount})!",
                    NameOf(weights))
            End If

            Dim total As Double = 0.0R

            For i As Integer = 0 To modelCount - 1
                Dim w As Double = weights(i)

                If Double.IsNaN(w) OrElse Double.IsInfinity(w) OrElse w < 0.0R Then
                    Throw New ArgumentException($"the weight at index {i} is invalid ({w})!", NameOf(weights))
                End If

                result(i) = w
                total += w
            Next

            If total <= 0.0R Then
                Throw New ArgumentException("the weights sum to zero!", NameOf(weights))
            End If

            For i As Integer = 0 To modelCount - 1
                result(i) /= total
            Next

            Return result
        End Function

        ''' <summary>按 <c>argmax</c> 把概率转成类别标签</summary>
        <Extension>
        Public Function PredictClass(classifier As IClassifier, x As Double()) As Double
            If classifier Is Nothing Then
                Throw New ArgumentNullException(NameOf(classifier))
            End If

            Dim probability As Double() = classifier.PredictProbability(x)
            Dim classes As Double() = classifier.Classes

            If probability Is Nothing OrElse probability.Length <> classes.Length Then
                Throw New InvalidOperationException(
                    $"the classifier '{classifier.GetType().FullName}' returned {If(probability Is Nothing, "nothing", CStr(probability.Length))} " &
                    $"probability value(s) for {classes.Length} class(es)!")
            End If

            Return classes(ArgMax(probability))
        End Function

    End Module
End Namespace
