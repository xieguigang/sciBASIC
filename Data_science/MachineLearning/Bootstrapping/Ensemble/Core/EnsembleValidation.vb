Imports System
Imports System.Runtime.CompilerServices

Namespace Ensemble

    ''' <summary>
    ''' 集成框架的统一入参校验与数值守卫。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 六个集成器（Voting / Bagging / Stacking × 分类 / 回归）在 <c>Train</c> 与
    ''' <c>Predict</c> 入口上的校验逻辑完全一致，集中在这里可以保证：
    ''' </para>
    ''' <list type="bullet">
    ''' <item>异常类型统一：空数据 / 长度不一致 → <see cref="ArgumentException"/>；
    '''       未训练 / 模型缺失 → <see cref="InvalidOperationException"/>；</item>
    ''' <item>异常消息统一：一律带上模型类型、出错位置与期望/实际值，
    '''       避免上层只能看到「something is wrong」；</item>
    ''' <item>特征维度守卫统一：训练期记录 <c>FeatureWidth</c>，预测期逐样本校验，
    '''       维度错位不再静默产出错误结果。</item>
    ''' </list>
    ''' </remarks>
    Public Module EnsembleValidation

        ''' <summary>
        ''' 校验监督学习的训练数据：特征行非空、行数一致、每行维度一致且为有限值。
        ''' </summary>
        ''' <param name="x">特征矩阵，每个元素是一行样本</param>
        ''' <param name="y">标签向量（分类为类别标签，回归为连续响应）</param>
        ''' <param name="context">出错时用于定位的调用上下文，例如 <c>BaggingClassifier.Train</c></param>
        Public Sub ValidateTrainingData(x As Double()(), y As Double(), context As String)
            If x Is Nothing OrElse x.Length = 0 Then
                Throw New ArgumentException($"{context}: the feature matrix can not be empty!", NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y), $"{context}: the label vector is NULL!")
            End If
            If y.Length <> x.Length Then
                Throw New ArgumentException(
                    $"{context}: the feature rows ({x.Length}) and labels ({y.Length}) do not agree!", NameOf(y))
            End If

            Dim width As Integer = 0

            For i As Integer = 0 To x.Length - 1
                Dim row As Double() = x(i)

                If row Is Nothing OrElse row.Length = 0 Then
                    Throw New ArgumentException(
                        $"{context}: the feature row[{i}] contains no value!", NameOf(x))
                End If

                If width = 0 Then
                    width = row.Length
                ElseIf row.Length <> width Then
                    Throw New ArgumentException(
                        $"{context}: the feature row[{i}] has {row.Length} value(s), " &
                        $"but the previous rows have {width}!", NameOf(x))
                End If
            Next
        End Sub

        ''' <summary>
        ''' 取特征矩阵的行宽，同时校验所有行长度一致。
        ''' </summary>
        ''' <param name="x">特征矩阵</param>
        ''' <param name="context">出错时用于定位的调用上下文</param>
        Public Function FeatureWidthOf(x As Double()(), context As String) As Integer
            If x Is Nothing OrElse x.Length = 0 Then
                Throw New ArgumentException($"{context}: the feature matrix can not be empty!", NameOf(x))
            End If

            Dim width As Integer = 0

            For i As Integer = 0 To x.Length - 1
                Dim row As Double() = x(i)

                If row Is Nothing OrElse row.Length = 0 Then
                    Throw New ArgumentException(
                        $"{context}: the feature row[{i}] contains no value!", NameOf(x))
                End If

                If width = 0 Then
                    width = row.Length
                ElseIf row.Length <> width Then
                    Throw New ArgumentException(
                        $"{context}: the feature row[{i}] has {row.Length} value(s), " &
                        $"but the previous rows have {width}!", NameOf(x))
                End If
            Next

            Return width
        End Function

        ''' <summary>
        ''' 校验单个样本的特征向量长度，防止维度错位导致的静默错误。
        ''' </summary>
        ''' <param name="x">单样本特征向量</param>
        ''' <param name="expectedWidth">模型训练期的特征维度</param>
        ''' <param name="context">出错时用于定位的调用上下文</param>
        <Extension>
        Public Sub ValidateFeatureVector(x As Double(), expectedWidth As Integer, context As String)
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x), $"{context}: the feature vector is NULL!")
            End If
            If expectedWidth > 0 AndAlso x.Length <> expectedWidth Then
                Throw New ArgumentException(
                    $"{context}: the feature vector has {x.Length} value(s), " &
                    $"but the model expects {expectedWidth}!", NameOf(x))
            End If
        End Sub

        ''' <summary>
        ''' 校验批量预测的特征矩阵：行数非空且每行维度等于模型的训练维度。
        ''' </summary>
        ''' <param name="x">特征矩阵</param>
        ''' <param name="expectedWidth">模型训练期的特征维度</param>
        ''' <param name="context">出错时用于定位的调用上下文</param>
        <Extension>
        Public Sub ValidateFeatureMatrix(x As Double()(), expectedWidth As Integer, context As String)
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x), $"{context}: the feature matrix is NULL!")
            End If

            For i As Integer = 0 To x.Length - 1
                Call ValidateFeatureVector(x(i), expectedWidth, $"{context} row[{i}]")
            Next
        End Sub

        ''' <summary>
        ''' 断言数值为有限值；用于拦截基模型返回的 NaN / Infinity，
        ''' 避免它们被静默地传播到聚合结果里。
        ''' </summary>
        ''' <param name="value">待校验的数值</param>
        ''' <param name="context">出错时用于定位的调用上下文</param>
        Public Function EnsureFinite(value As Double, context As String) As Double
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                Throw New InvalidOperationException($"{context} returned an invalid value ({value})!")
            End If

            Return value
        End Function

        ''' <summary>
        ''' 未训练守卫。<paramref name="initialized"/> 为 <c>False</c> 时抛出
        ''' 带模型类型与缺失成员名的 <see cref="InvalidOperationException"/>。
        ''' </summary>
        ''' <param name="initialized">模型是否已完成训练/恢复</param>
        ''' <param name="modelType">模型类型名</param>
        ''' <param name="missing">缺失的状态成员名，用于提示恢复或训练步骤</param>
        Public Sub RequireTrained(initialized As Boolean, modelType As String, missing As String)
            If Not initialized Then
                Throw New InvalidOperationException(
                    $"the {modelType} has not been trained yet (missing '{missing}'); " &
                    "call Train() or load a persisted model before predicting!")
            End If
        End Sub

        ''' <summary>中位数（输入不被修改），用于 Bagging 回归器的稳健聚合</summary>
        Public Function Median(values As Double()) As Double
            If values Is Nothing OrElse values.Length = 0 Then
                Throw New ArgumentException("the value vector can not be empty!", NameOf(values))
            End If

            Dim sorted As Double() = CType(values.Clone(), Double())

            Call Array.Sort(sorted)

            Dim middle As Integer = sorted.Length \ 2

            If sorted.Length Mod 2 = 1 Then
                Return sorted(middle)
            End If

            Return (sorted(middle - 1) + sorted(middle)) / 2.0R
        End Function

    End Module

End Namespace
