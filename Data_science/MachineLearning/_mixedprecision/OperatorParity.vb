' ---------------------------------------------------------------------------
' 算子对拍用例
'
' 每个用例都遵循同一个模板：
'   1. 用固定种子生成同一份数据；
'   2. 分别喂给 Tensor.computeKernel（双精度）与 TensorF.computeKernelF（单精度）；
'   3. 比较两者的数值差异是否落在单精度的合理舍入范围内。
'
' 容差口径：参考值是双精度结果，而单精度本身的机器精度约 1.2e-7，
' 因此这里统一取 1e-5 的相对容差 —— 足以暴露"实现写错了"级别的错误，
' 又不会把正常的降精度噪声误报成失败。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports std = System.Math

Namespace MixedPrecisionBench

    Module OperatorParity

        Friend Sub RunAll(results As List(Of CaseResult))
            ' 每组用例独立兜异常：GPU 路径上一旦某个内核真的算错，
            ' 应当被记成一条失败项，而不是让整个套件崩掉看不到其余结果。
            Dim groups As New List(Of Action(Of List(Of CaseResult))) From {
                AddressOf MatMulCases,
                AddressOf TransposeCase,
                AddressOf ReductionCases,
                AddressOf SoftmaxCases,
                AddressOf SparseCases,
                AddressOf ConvCases,
                AddressOf PoolCases,
                AddressOf LossCases,
                AddressOf ResidencyCases.RunAll
            }

            For Each g In groups
                Try
                    Call g(results)
                Catch ex As Exception
                    results.Add(New CaseResult("用例组异常 " & g.Method.Name, False, ex.Message))
                End Try
            Next
        End Sub

        Private ReadOnly Property KD As ITensorCompute
            Get
                Return Tensor.computeKernel
            End Get
        End Property

        Private ReadOnly Property KF As ITensorComputeF
            Get
                Return TensorF.computeKernelF
            End Get
        End Property

        ' ---------------- 矩阵 ----------------

        Private Sub MatMulCases(results As List(Of CaseResult))
            ' 大矩阵：跨过 SIMDTensor 的 MatrixDotThreshold，走并行 / Vector 路径
            Dim a = Randoms(37 * 53, seed:=11, scale:=2.0)
            Dim b = Randoms(53 * 29, seed:=12, scale:=2.0)

            results.Add(Compare("MatMul 37x53 * 53x29",
                                KD.MatMul(AsD(a, 37, 53), AsD(b, 53, 29)),
                                KF.MatMul(AsF(a, 37, 53), AsF(b, 53, 29))))

            ' 小矩阵：低于阈值，走标量三重循环。这条路径正是 batch=1 的 GEMV 所处的区间
            Dim c = Randoms(4 * 3, seed:=13, scale:=1.5)
            Dim d = Randoms(3 * 5, seed:=14, scale:=1.5)

            results.Add(Compare("MatMul 4x3 * 3x5 (GEMV 区间)",
                                KD.MatMul(AsD(c, 4, 3), AsD(d, 3, 5)),
                                KF.MatMul(AsF(c, 4, 3), AsF(d, 3, 5))))
        End Sub

        Private Sub TransposeCase(results As List(Of CaseResult))
            Dim a = Randoms(23 * 41, seed:=21, scale:=3.0)

            results.Add(Compare("Transpose 23x41",
                                KD.Transpose(AsD(a, 23, 41)),
                                KF.Transpose(AsF(a, 23, 41))))
        End Sub

        ' ---------------- 归约 ----------------

        Private Sub ReductionCases(results As List(Of CaseResult))
            Dim data = Randoms(7 * 11 * 5, seed:=31, scale:=4.0)

            ' 中间轴：步长不为 1，是最容易写错的一段
            results.Add(Compare("Sum axis=1 keepdims=True",
                                KD.Sum(AsD(data, 7, 11, 5), 1, True),
                                KF.Sum(AsF(data, 7, 11, 5), 1, True)))

            results.Add(Compare("Sum axis=1 keepdims=False",
                                KD.Sum(AsD(data, 7, 11, 5), 1, False),
                                KF.Sum(AsF(data, 7, 11, 5), 1, False)))

            results.Add(Compare("Mean axis=0 keepdims=False",
                                KD.Mean(AsD(data, 7, 11, 5), 0, False),
                                KF.Mean(AsF(data, 7, 11, 5), 0, False)))

            results.Add(Compare("Max axis=2 keepdims=False",
                                KD.Max(AsD(data, 7, 11, 5), 2, False),
                                KF.Max(AsF(data, 7, 11, 5), 2, False)))

            results.Add(Compare("Min axis=1 keepdims=False",
                                KD.Min(AsD(data, 7, 11, 5), 1, False),
                                KF.Min(AsF(data, 7, 11, 5), 1, False)))

            ' 整体归约：出口刻意保留双精度
            results.Add(CompareScalar("SumAll", KD.SumAll(AsD(data, 7, 11, 5)), KF.SumAll(AsF(data, 7, 11, 5))))

            ' 下标必须逐位相等，否则 Slice / TopK / 池化反向会连锁出错
            results.Add(CompareExact("ArgMax axis=2",
                                     KD.ArgMax(AsD(data, 7, 11, 5), 2),
                                     KF.ArgMax(AsF(data, 7, 11, 5), 2)))

            results.Add(CompareExact("ArgMin axis=1",
                                     KD.ArgMin(AsD(data, 7, 11, 5), 1),
                                     KF.ArgMin(AsF(data, 7, 11, 5), 1)))
        End Sub

        ' ---------------- softmax 系列 ----------------

        Private Sub SoftmaxCases(results As List(Of CaseResult))
            ' 刻意放大 logits 的量级：不做减最大值的话这里会直接溢出成 NaN
            Dim logits = Randoms(6 * 13, seed:=41, scale:=25.0)

            results.Add(Compare("Softmax axis=-1 (大 logits)",
                                KD.Softmax(AsD(logits, 6, 13), -1),
                                KF.Softmax(AsF(logits, 6, 13), -1)))

            results.Add(Compare("LogSoftmax axis=-1 (大 logits)",
                                KD.LogSoftmax(AsD(logits, 6, 13), -1),
                                KF.LogSoftmax(AsF(logits, 6, 13), -1)))

            ' 三秩沿中间轴：考验 outerSize / innerSize 的分解是否正确
            results.Add(Compare("Softmax axis=1 (三秩)",
                                KD.Softmax(AsD(logits, 6, 13, 1), 1),
                                KF.Softmax(AsF(logits, 6, 13, 1), 1)))
        End Sub

        ' ---------------- 稀疏 ----------------

        Private Function BuildCsr(rows As Integer, cols As Integer, nnz As Integer, seed As Integer) As SparseCsr
            Dim rnd As New Random(seed)
            Dim counts(rows - 1) As Integer

            For i = 0 To nnz - 1
                counts(rnd.Next(rows)) += 1
            Next

            Dim rowPointers(rows) As Integer
            For r = 0 To rows - 1
                rowPointers(r + 1) = rowPointers(r) + counts(r)
            Next

            Dim colIdx(nnz - 1) As Integer
            Dim values(nnz - 1) As Double

            Dim cur(rows - 1) As Integer
            For i = 0 To nnz - 1
                Dim r As Integer
                Do
                    r = rnd.Next(rows)
                Loop While cur(r) >= counts(r)

                Dim p = rowPointers(r) + cur(r)
                cur(r) += 1
                colIdx(p) = rnd.Next(cols)
                values(p) = rnd.NextDouble() * 2.0 - 1.0
            Next

            Return New SparseCsr(rows, cols, rowPointers, colIdx, values)
        End Function

        Private Sub SparseCases(results As List(Of CaseResult))
            Dim csr = BuildCsr(rows:=29, cols:=17, nnz:=140, seed:=51)
            Dim x = Randoms(4 * 29, seed:=52, scale:=2.0)

            ' 脉冲输入里绝大多数是 0，这里同样保留稀疏性以覆盖"跳过零源"分支
            For i = 0 To x.Length - 1
                If (i Mod 7) <> 0 Then x(i) = 0.0
            Next

            results.Add(Compare("SpMM 4x29 * 29x17",
                                KD.SpMM(csr, AsD(x, 4, 29)),
                                KF.SpMM(csr, AsF(x, 4, 29))))

            ' LIF 融合步：递归连接必须是方阵 [Units, Units]，因此另建一份
            Dim units = 17
            Dim lifCsr = BuildCsr(rows:=units, cols:=units, nnz:=110, seed:=56)
            Dim batch = 3
            Dim sPrev = Randoms(batch * units, seed:=53, scale:=1.0)
            For i = 0 To sPrev.Length - 1
                sPrev(i) = If(sPrev(i) > 0.6, 1.0, 0.0)
            Next

            Dim h = Randoms(batch * units, seed:=54, scale:=1.0)
            Dim ext = Randoms(batch * units, seed:=55, scale:=0.5)

            Dim hD = AsD(h, batch, units), sD = New Tensor(batch, units), cntD = New Tensor(batch, units)
            Dim okD = KD.LifStep(lifCsr, AsD(sPrev, batch, units), AsD(ext, batch, units), hD, sD, cntD, 0.85, 0.9, True)

            Dim hF = AsF(h, batch, units), sF = New TensorF(batch, units), cntF = New TensorF(batch, units)
            Dim okF = KF.LifStep(lifCsr, AsF(sPrev, batch, units), AsF(ext, batch, units), hF, sF, cntF, 0.85, 0.9, True)

            results.Add(New CaseResult("LifStep 融合步完成标志", okD AndAlso okF, $"double={okD} float={okF}"))
            results.Add(Compare("LifStep 膜电位", hD, hF, relTol:=0.0001))
            results.Add(CompareExact("LifStep 脉冲", sD, sF))
            results.Add(CompareExact("LifStep 计数", cntD, cntF))
        End Sub

        ' ---------------- 卷积 ----------------

        Private Sub ConvCases(results As List(Of CaseResult))
            Dim batch = 2, inH = 6, inW = 7, inC = 3, outC = 4
            Dim kH = 3, kW = 3, stride = 1, padding = 1

            Dim x = Randoms(batch * inH * inW * inC, seed:=61, scale:=1.5)
            Dim w = Randoms(kH * kW * inC * outC, seed:=62, scale:=0.8)
            Dim bias = Randoms(outC, seed:=63, scale:=0.3)

            Dim yD = KD.Conv2D(AsD(x, batch, inH, inW, inC), AsD(w, kH, kW, inC, outC), AsD(bias, outC), stride, padding)
            Dim yF = KF.Conv2D(AsF(x, batch, inH, inW, inC), AsF(w, kH, kW, inC, outC), AsF(bias, outC), stride, padding)

            results.Add(Compare("Conv2D forward", yD, yF))

            ' 用同一份上游梯度做三个方向的反向，保证前向/反向形状约定自洽
            Dim gOut = Randoms(yD.Length, seed:=64, scale:=0.5)

            results.Add(Compare("Conv2DBackwardInput",
                                KD.Conv2DBackwardInput(AsD(gOut, yD.Shape(0), yD.Shape(1), yD.Shape(2), yD.Shape(3)),
                                                       AsD(w, kH, kW, inC, outC),
                                                       New Integer() {batch, inH, inW, inC}, stride, padding),
                                KF.Conv2DBackwardInput(AsF(gOut, yF.Shape(0), yF.Shape(1), yF.Shape(2), yF.Shape(3)),
                                                       AsF(w, kH, kW, inC, outC),
                                                       New Integer() {batch, inH, inW, inC}, stride, padding)))

            results.Add(Compare("Conv2DBackwardFilter",
                                KD.Conv2DBackwardFilter(AsD(gOut, yD.Shape(0), yD.Shape(1), yD.Shape(2), yD.Shape(3)),
                                                        AsD(x, batch, inH, inW, inC),
                                                        New Integer() {kH, kW, inC, outC}, stride, padding),
                                KF.Conv2DBackwardFilter(AsF(gOut, yF.Shape(0), yF.Shape(1), yF.Shape(2), yF.Shape(3)),
                                                        AsF(x, batch, inH, inW, inC),
                                                        New Integer() {kH, kW, inC, outC}, stride, padding)))

            results.Add(Compare("Conv2DBackwardBias",
                                KD.Conv2DBackwardBias(AsD(gOut, yD.Shape(0), yD.Shape(1), yD.Shape(2), yD.Shape(3))),
                                KF.Conv2DBackwardBias(AsF(gOut, yF.Shape(0), yF.Shape(1), yF.Shape(2), yF.Shape(3)))))
        End Sub

        ' ---------------- 池化 ----------------

        Private Sub PoolCases(results As List(Of CaseResult))
            Dim batch = 2, inH = 8, inW = 8, inC = 3
            Dim x = Randoms(batch * inH * inW * inC, seed:=71, scale:=3.0)

            Dim argMaxD As Tensor = Nothing
            Dim argMaxF As TensorF = Nothing

            Dim yD = KD.MaxPool2D(AsD(x, batch, inH, inW, inC), 2, 2, 0, argMaxD)
            Dim yF = KF.MaxPool2D(AsF(x, batch, inH, inW, inC), 2, 2, 0, argMaxF)

            results.Add(Compare("MaxPool2D forward", yD, yF))

            ' 下标错了的话，反向会把梯度回填到错误位置 —— 这里必须逐位相等
            results.Add(CompareExact("MaxPool2D argMax", argMaxD, argMaxF))

            Dim gOut = Randoms(yD.Length, seed:=72, scale:=1.0)

            results.Add(Compare("MaxPool2DBackward",
                                KD.MaxPool2DBackward(AsD(gOut, yD.Shape(0), yD.Shape(1), yD.Shape(2), yD.Shape(3)),
                                                     argMaxD, New Integer() {batch, inH, inW, inC}),
                                KF.MaxPool2DBackward(AsF(gOut, yF.Shape(0), yF.Shape(1), yF.Shape(2), yF.Shape(3)),
                                                     argMaxF, New Integer() {batch, inH, inW, inC})))
        End Sub

        ' ---------------- 损失 ----------------

        Private Sub LossCases(results As List(Of CaseResult))
            Dim rows = 6, vocab = 11
            Dim logits = Randoms(rows * vocab, seed:=81, scale:=6.0)

            Dim rnd As New Random(82)
            Dim targets(rows - 1) As Integer
            Dim mask(rows - 1) As Boolean

            For i = 0 To rows - 1
                targets(i) = rnd.Next(vocab)
                mask(i) = (i Mod 4) <> 0    ' 留出若干不计损失的位置，覆盖掩码分支
            Next

            Dim dLogitsD As Tensor = Nothing
            Dim dLogitsF As TensorF = Nothing

            Dim lossD = KD.MaskedCrossEntropy(AsD(logits, rows, vocab), targets, mask, dLogitsD)
            Dim lossF = KF.MaskedCrossEntropy(AsF(logits, rows, vocab), targets, mask, dLogitsF)

            results.Add(CompareScalar("MaskedCrossEntropy loss", lossD, lossF))
            results.Add(Compare("MaskedCrossEntropy dLogits", dLogitsD, dLogitsF))

            ' 掩码位置必须保持严格为 0：一点点漂移都会让 SFT 学到不该学的东西
            Dim leak As Double = 0
            For r = 0 To rows - 1
                If mask(r) Then Continue For
                For j = 0 To vocab - 1
                    leak = std.Max(leak, std.Abs(dLogitsF.Data(r * vocab + j)))
                Next
            Next
            results.Add(New CaseResult("MaskedCrossEntropy 掩码零泄漏", leak = 0.0, $"maxAbs={leak:E3}"))

            Dim pred = Randoms(24, seed:=83, scale:=2.0)
            Dim truth = Randoms(24, seed:=84, scale:=2.0)

            results.Add(Compare("MseLoss", KD.MseLoss(AsD(pred, 24), AsD(truth, 24)), KF.MseLoss(AsF(pred, 24), AsF(truth, 24))))
            results.Add(Compare("L2Loss", KD.L2Loss(AsD(pred, 24)), KF.L2Loss(AsF(pred, 24))))
            results.Add(Compare("HuberLoss delta=1", KD.HuberLoss(AsD(pred, 24), AsD(truth, 24), 1.0), KF.HuberLoss(AsF(pred, 24), AsF(truth, 24), 1.0F)))

            ' sigmoid 交叉熵内部按 max(x,0)-x*z+log(1+e^-|x|) 稳定化，取大 |x| 才能验证是否真的稳定
            Dim labels = Randoms(24, seed:=85, scale:=1.0)
            For i = 0 To labels.Length - 1
                labels(i) = If(labels(i) > 0, 1.0, 0.0)
            Next
            Dim big = Randoms(24, seed:=86, scale:=30.0)

            results.Add(Compare("SigmoidCrossEntropyWithLogits (|x|~30)",
                                KD.SigmoidCrossEntropyWithLogits(AsD(labels, 24), AsD(big, 24)),
                                KF.SigmoidCrossEntropyWithLogits(AsF(labels, 24), AsF(big, 24))))
        End Sub

    End Module

End Namespace
