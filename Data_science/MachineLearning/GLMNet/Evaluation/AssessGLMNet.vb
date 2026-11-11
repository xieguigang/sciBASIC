Imports Microsoft.VisualBasic.Data
Imports std = System.Math

''' <summary>
''' <c>assess.glmnet</c> / <c>roc.glmnet</c> / <c>confusion.glmnet</c> 的等价能力。
''' </summary>
''' <remarks>
''' 所有指标都按整条 lambda 路径逐点计算，返回"指标名 → 曲线"的映射，
''' 便于直接导出为 <c>NumericTable</c> 供绘图。
''' </remarks>
Public Module AssessGLMNet

    ''' <summary>
    ''' 在测试集上评估一条 glmnet 路径，返回各指标随 lambda 变化的曲线。
    ''' </summary>
    Public Function Assess(fit As GlmnetFit, newx As Double()(), newy As Double()()) As Dictionary(Of String, Double())
        If fit Is Nothing Then Throw New ArgumentNullException(NameOf(fit))
        If newx Is Nothing Then Throw New ArgumentNullException(NameOf(newx))
        If newy Is Nothing Then Throw New ArgumentNullException(NameOf(newy))

        Dim steps As Integer = fit.Count
        Dim n As Integer = newx.Length
        Dim k As Integer = fit.nresponses
        Dim predictions As Double()()() = New Double(steps - 1)()() {}

        For s As Integer = 0 To steps - 1
            predictions(s) = fit.LinearPredictor(newx, fit.GetCoefficients(s))
        Next

        Dim truth As New GlmnetData(newx, newy, New GlmnetOptions(), fit.Family)
        Dim solver As IGlmnetFamily = FamilyRegistry.Create(fit.Family)
        Dim result As New Dictionary(Of String, Double())()

        Select Case fit.Family
            Case FamilyKind.Binomial
                result("deviance") = solver.Measure(predictions, truth, CVMeasure.Deviance)
                result("class") = solver.Measure(predictions, truth, CVMeasure.Class)
                result("auc") = solver.Measure(predictions, truth, CVMeasure.Auc)
                result("mse") = solver.Measure(predictions, truth, CVMeasure.Mse)
                result("mae") = solver.Measure(predictions, truth, CVMeasure.Mae)
            Case FamilyKind.Multinomial
                result("deviance") = solver.Measure(predictions, truth, CVMeasure.Deviance)
                result("class") = solver.Measure(predictions, truth, CVMeasure.Class)
                result("mse") = solver.Measure(predictions, truth, CVMeasure.Mse)
                result("mae") = solver.Measure(predictions, truth, CVMeasure.Mae)
            Case FamilyKind.Cox
                result("deviance") = solver.Measure(predictions, truth, CVMeasure.Deviance)
                result("C") = solver.Measure(predictions, truth, CVMeasure.CIndex)
            Case FamilyKind.MGaussian
                result("mse") = solver.Measure(predictions, truth, CVMeasure.Mse)
                result("mae") = solver.Measure(predictions, truth, CVMeasure.Mae)
            Case Else
                result("mse") = solver.Measure(predictions, truth, CVMeasure.Mse)
                result("mae") = solver.Measure(predictions, truth, CVMeasure.Mae)
                result("deviance") = solver.Measure(predictions, truth, CVMeasure.Deviance)
        End Select

        result("lambda") = CType(fit.lambda.Clone(), Double())
        Return result
    End Function

    ''' <summary>
    ''' ROC 曲线：返回按阈值降序排列的 <c>[FPR, TPR]</c> 点列。
    ''' </summary>
    Public Function Roc(scores As Double(), labels As Double()) As Double()()
        Dim n As Integer = scores.Length
        Dim order As Integer() = New Integer(n - 1) {}
        For i As Integer = 0 To n - 1
            order(i) = i
        Next
        Call Array.Sort(CType(scores.Clone(), Double()), order)
        Array.Reverse(order)

        Dim positives As Integer = 0
        Dim negatives As Integer = 0
        For i As Integer = 0 To n - 1
            If labels(i) > 0.0R Then positives += 1 Else negatives += 1
        Next
        If positives = 0 OrElse negatives = 0 Then
            Return New Double()() {New Double() {0.0R, 0.0R}, New Double() {1.0R, 1.0R}}
        End If

        Dim points As New List(Of Double())() From {New Double() {0.0R, 0.0R}}
        Dim tp As Integer = 0
        Dim fp As Integer = 0

        For Each i As Integer In order
            If labels(i) > 0.0R Then
                tp += 1
            Else
                fp += 1
            End If
            points.Add(New Double() {CDbl(fp) / CDbl(negatives), CDbl(tp) / CDbl(positives)})
        Next

        Return points.ToArray()
    End Function

    ''' <summary>多分类 / 二分类的混淆矩阵，行 = 真实类别、列 = 预测类别。</summary>
    Public Function ConfusionMatrix(predicted As Integer(), actual As Integer(), numberOfClasses As Integer) As Integer()()
        Dim matrix As Integer()() = New Integer(numberOfClasses - 1)() {}
        For r As Integer = 0 To numberOfClasses - 1
            matrix(r) = New Integer(numberOfClasses - 1) {}
        Next
        For i As Integer = 0 To predicted.Length - 1
            Dim a As Integer = actual(i)
            Dim p As Integer = predicted(i)
            If a >= 0 AndAlso a < numberOfClasses AndAlso p >= 0 AndAlso p < numberOfClasses Then
                matrix(a)(p) += 1
            End If
        Next
        Return matrix
    End Function

    ''' <summary>把评估曲线导出为二维表，每行一个 lambda。</summary>
    Public Function toNumericTable(curves As Dictionary(Of String, Double())) As NumericTable
        Dim keys As String() = curves.Keys.ToArray()
        Dim steps As Integer = curves("lambda").Length
        Dim matrix As Double()() = New Double(steps - 1)() {}
        Dim rowNames As String() = New String(steps - 1) {}

        For i As Integer = 0 To steps - 1
            Dim row As Double() = New Double(keys.Length - 1) {}
            For c As Integer = 0 To keys.Length - 1
                row(c) = curves(keys(c))(i)
            Next
            matrix(i) = row
            rowNames(i) = $"s{i}"
        Next

        Return New NumericTable(matrix, rowNames, keys)
    End Function

End Module
