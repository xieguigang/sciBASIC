Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

''' <summary>
''' 推理回归：用 <c>resources/model</c> 下的真实 XGBoost 模型文件，
''' 与 <c>resources/expectation</c> 下的期望输出逐值比对。
'''
''' 覆盖 gbtree / gblinear / 多个 XGBoost 文件版本 / 二分类与多分类变换。
''' </summary>
Public Module PredictorRegression

    Private Class CaseDef
        Public Model As String
        Public Data As String
        Public Expect As String
    End Class

    Private ReadOnly cases As CaseDef() = {
        New CaseDef With {.Model = "gbtree\v47\binary-logistic.model", .Data = "agaricus.txt.0.test", .Expect = "gbtree\v47\binary-logistic"},
        New CaseDef With {.Model = "gbtree\v47\binary-logitraw.model", .Data = "agaricus.txt.0.test", .Expect = "gbtree\v47\binary-logitraw"},
        New CaseDef With {.Model = "gbtree\v47\multi-softmax.model", .Data = "agaricus.txt.0.test", .Expect = "gbtree\v47\multi-softmax"},
        New CaseDef With {.Model = "gbtree\v47\multi-softprob.model", .Data = "agaricus.txt.0.test", .Expect = "gbtree\v47\multi-softprob"},
        New CaseDef With {.Model = "gbtree\v40\binary-logistic.model", .Data = "agaricus.txt.0.test", .Expect = "gbtree\v40\binary-logistic"},
        New CaseDef With {.Model = "gbtree\v40\multi-softprob.model", .Data = "agaricus.txt.0.test", .Expect = "gbtree\v40\multi-softprob"},
        New CaseDef With {.Model = "gblinear\v47\binary-logistic.model", .Data = "agaricus.txt.0.test", .Expect = "gblinear\v47\binary-logistic"},
        New CaseDef With {.Model = "gblinear\v47\multi-softprob.model", .Data = "agaricus.txt.0.test", .Expect = "gblinear\v47\multi-softprob"},
        New CaseDef With {.Model = "gblinear\v40\binary-logistic.model", .Data = "agaricus.txt.0.test", .Expect = "gblinear\v40\binary-logistic"},
        New CaseDef With {.Model = "gbtree\v47\sms-spam.model", .Data = "sms-spam.test", .Expect = "gbtree\v47\sms-spam"}
    }

    Private Const TOLERANCE As Double = 0.00001

    Public Sub Run()
        Console.WriteLine("[推理回归] 开始")

        For Each c As CaseDef In cases
            Call checkCase(c)
        Next

        Console.WriteLine("[推理回归] 结束")
    End Sub

    Private Sub checkCase(c As CaseDef)
        Dim name As String = c.Expect.Replace("\"c, "/"c)
        Dim modelFile As String = TestSupport.ResourcePath("model\" & c.Model)
        Dim dataFile As String = TestSupport.ResourcePath("data\" & c.Data)
        Dim expectBase As String = TestSupport.ResourcePath("expectation\" & c.Expect)

        If Not File.Exists(modelFile) OrElse Not File.Exists(dataFile) Then
            Call DebugAudit.Mark($"skip:{name}", "模型或数据文件缺失")
            Console.WriteLine($"  [跳过] {name}")
            Return
        End If

        Dim predictor = TestSupport.LoadPredictor(modelFile)
        Dim matrix = TestSupport.LoadLibSvm(dataFile)

        Call DebugAudit.Mark($"model:{name}", $"rows={matrix.size}")

        ' 1) 变换后的预测值
        Dim predictFile As String = expectBase & ".predict"

        If File.Exists(predictFile) Then
            Dim expected = TestSupport.ReadExpectation(predictFile)
            Dim actual As Double()() = New Double(matrix.size - 1)() {}

            For i As Integer = 0 To matrix.size - 1
                actual(i) = predictor.predict(matrix.matrix(i))
            Next

            Call report($"{name}:predict", expected, actual, TOLERANCE)
        End If

        ' 2) margin（未变换）
        Dim marginFile As String = expectBase & ".margin"

        If File.Exists(marginFile) Then
            Dim expected = TestSupport.ReadExpectation(marginFile)
            Dim actual As Double()() = New Double(matrix.size - 1)() {}

            For i As Integer = 0 To matrix.size - 1
                actual(i) = predictor.predict(matrix.matrix(i), True)
            Next

            Call report($"{name}:margin", expected, actual, TOLERANCE)
        End If

        ' 3) 叶子索引
        Dim leafFile As String = expectBase & ".leaf"

        If File.Exists(leafFile) Then
            Dim expected = TestSupport.ReadExpectation(leafFile)
            Dim actual As Double()() = New Double(matrix.size - 1)() {}

            For i As Integer = 0 To matrix.size - 1
                Dim leaves As Integer() = predictor.predictLeaf(matrix.matrix(i))
                Dim values As Double() = New Double(leaves.Length - 1) {}

                For j As Integer = 0 To leaves.Length - 1
                    values(j) = leaves(j)
                Next

                actual(i) = values
            Next

            Call report($"{name}:leaf", expected, actual, 0.5)
        End If
    End Sub

    Private Sub report(label As String, expected As Double()(), actual As Double()(), tolerance As Double)
        Dim result = TestSupport.Compare(expected, actual, tolerance)
        Dim text As String = $"maxDiff={result.maxDiff:G6}, outliers={result.outliers}/{result.compared}"

        Call DebugAudit.Mark(label, text)
        Console.WriteLine($"  {label}: {text}")

        Call DebugAudit.Check(result.outliers = 0,
            $"{label}: {result.outliers} 个输出超出容差 {tolerance:G3}（最大偏差 {result.maxDiff:G6}）")
    End Sub
End Module
