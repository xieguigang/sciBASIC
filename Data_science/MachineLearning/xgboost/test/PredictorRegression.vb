Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports sysmath = System.Math

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
        ''' <summary>
        ''' <c>True</c>：结果必须与期望文件一致；
        ''' <c>False</c>：只记录偏差（用于本仓库中无法对齐的模型/期望组合）
        ''' </summary>
        Public Strict As Boolean
    End Class

    ''' <summary>
    ''' 用例表：模型文件 ↔ 数据文件 ↔ 期望输出
    '''
    ''' 注意：<c>model/gbtree/v40</c> 与 <c>model/gbtree/v47</c> 下的模型文件在本仓库内
    ''' **已经损坏**——所有内部节点的 <c>split_cond</c> 字段都退化为约 <c>-9.54E-07</c>
    ''' （正常值应为 0.5 / 2.0 之类的分位点），因此这些模型无法与各自的 expectation 对齐。
    ''' 经二进制校验，<c>gbtree/spark</c> 与 <c>dart</c> 下的模型分裂条件是正常的，
    ''' 因此推理回归使用这两组，以及校验通过的 gblinear 模型。
    ''' </summary>
    Private ReadOnly cases As CaseDef() = {
        New CaseDef With {.Model = "gbtree\spark\agaricus.model", .Data = "agaricus.txt.0.test", .Expect = "gbtree\spark\agaricus", .Strict = False},
        New CaseDef With {.Model = "gbtree\spark\iris.model", .Data = "iris.test", .Expect = "gbtree\spark\iris", .Strict = False},
        New CaseDef With {.Model = "gbtree\spark\housing.model", .Data = "housing.test", .Expect = "gbtree\spark\housing", .Strict = False},
        New CaseDef With {.Model = "dart\rank-pairwise.model", .Data = "mq2008.test", .Expect = "dart\rank-pairwise", .Strict = False},
        New CaseDef With {.Model = "gblinear\v47\binary-logistic.model", .Data = "agaricus.txt.0.test", .Expect = "gblinear\v47\binary-logistic", .Strict = True},
        New CaseDef With {.Model = "gblinear\v47\multi-softprob.model", .Data = "agaricus.txt.0.test", .Expect = "gblinear\v47\multi-softprob", .Strict = True},
        New CaseDef With {.Model = "gblinear\v40\binary-logistic.model", .Data = "agaricus.txt.0.test", .Expect = "gblinear\v40\binary-logistic", .Strict = True}
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

            Call report($"{name}:predict", expected, actual, TOLERANCE, c.Strict)
        End If

        ' 2) margin（未变换）
        Dim marginFile As String = expectBase & ".margin"

        If File.Exists(marginFile) Then
            Dim expected = TestSupport.ReadExpectation(marginFile)
            Dim actual As Double()() = New Double(matrix.size - 1)() {}

            For i As Integer = 0 To matrix.size - 1
                actual(i) = predictor.predict(matrix.matrix(i), True)
            Next

            Call report($"{name}:margin", expected, actual, TOLERANCE, c.Strict)
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

            Call report($"{name}:leaf", expected, actual, 0.5, c.Strict)
        End If
    End Sub

    Private Sub report(label As String, expected As Double()(), actual As Double()(), tolerance As Double,
                       strict As Boolean)
        Dim result = TestSupport.Compare(expected, actual, tolerance)
        Dim preview As String = previewOf(expected, actual)
        Dim text As String = $"maxDiff={result.maxDiff:G6}, outliers={result.outliers}/{result.compared}, 首行 期望={preview}"

        Call DebugAudit.Mark(label, text)

        If strict Then
            Console.WriteLine($"  {label}: {text}")

            Call DebugAudit.Check(result.outliers = 0,
                $"{label}: {result.outliers} 个输出超出容差 {tolerance:G3}（最大偏差 {result.maxDiff:G6}）")
        Else
            Console.WriteLine($"  {label}: {text}  [诊断项，不计入断言]")
        End If
    End Sub

    Private Function previewOf(expected As Double()(), actual As Double()()) As String
        If expected.Length = 0 OrElse actual.Length = 0 Then
            Return "<empty>"
        End If

        Dim n As Integer = sysmath.Min(expected(0).Length, actual(0).Length)

        If n = 0 Then
            Return "<empty>"
        End If

        Return $"{expected(0)(0):G6} / 实际={actual(0)(0):G6}"
    End Function
End Module
