' ---------------------------------------------------------------------------
' 性能基准：-bench
'
' 目的不是为了"证明 GPU 更快"，而是为了把**交叉点**量出来。
'
'   单精度 + CUDA 相对双精度 SIMD 的收益不是一个常数：
'     * 规模小时，显存往返（每个未钉住的算子各一次 H2D + 一次 D2H）就是净亏损；
'     * 规模大时，FP32 的吞吐优势（消费级显卡 FP64 通常只有 FP32 的 1/64）
'       和更高的内存带宽才真正兑现。
'   改造之前必须先知道每条算子的交叉点在哪，否则"统统改成 tensorF"只会更慢。
'
' 用法：
'   MixedPrecisionBench -bench              双精度 SIMD vs 单精度 SIMD
'   MixedPrecisionBench -bench -gpu         再追加一列单精度 CUDA
' ---------------------------------------------------------------------------

Imports System.Diagnostics
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports std = System.Math

Namespace MixedPrecisionBench

    Module Bench

        ''' <summary>一条基准条目</summary>
        Private Structure Row
            Public Op As String
            Public Shape As String
            Public DoubleMs As Double
            Public SingleCpuMs As Double
            Public SingleGpuMs As Double
            ''' <summary>
            ''' 单精度路径的计时动作。后端是进程级共享的，
            ''' 因此同一个委托在"切到 SIMD"与"切到 CUDA"之后再跑一次即可，
            ''' 不需要为 GPU 另写一份。
            ''' </summary>
            Public SingleAction As Action
        End Structure

        ''' <summary>每个条目至少重复这么多次，避免单次抖动的干扰</summary>
        Private Const MinIterations As Integer = 5

        Friend Sub Run(useGpu As Boolean)
            Dim rows As New List(Of Row)

            ' ---- GEMM：算力密集，是混合精度的主战场 ----
            For Each n In New Integer() {128, 256, 512, 1024}
                rows.Add(GemmRow(n))
            Next

            ' ---- 卷积 ----
            rows.Add(ConvRow(batch:=8, h:=64, w:=64, inC:=16, outC:=32))
            rows.Add(ConvRow(batch:=16, h:=128, w:=128, inC:=32, outC:=64))

            ' ---- softmax：访存密集 + 需要三次遍历 ----
            rows.Add(SoftmaxRow(rows:=256, cols:=512))
            rows.Add(SoftmaxRow(rows:=1024, cols:=1024))

            ' ---- 稀疏传播：脉冲网络 / 图卷积的主算力 ----
            rows.Add(SpmmRow(rows:=4096, cols:=4096, nnz:=200000, batch:=8))

            If useGpu Then
                rows = WithGpu(rows)
            End If

            Console.WriteLine()
            Console.WriteLine("================ 性能基准（毫秒 / 次，越小越好）================")
            Console.WriteLine()
            Console.WriteLine(Pad("算子", 16) & Pad("形状", 26) &
                              Pad("Double+SIMD", 14) & Pad("Single+SIMD", 14) &
                              Pad("Single+CUDA", 14) & "相对收益")
            Console.WriteLine()

            For Each r In rows
                Dim best = std.Min(If(r.SingleCpuMs > 0, r.SingleCpuMs, Double.MaxValue),
                                   If(r.SingleGpuMs > 0, r.SingleGpuMs, Double.MaxValue))
                Dim speedup = If(best > 0 AndAlso r.DoubleMs > 0, r.DoubleMs / best, 0.0)

                Console.WriteLine(Pad(r.Op, 16) & Pad(r.Shape, 26) &
                                  Pad(Fmt(r.DoubleMs), 14) & Pad(Fmt(r.SingleCpuMs), 14) &
                                  Pad(Fmt(r.SingleGpuMs), 14) & speedup.ToString("F2") & "x")
            Next

            Console.WriteLine()
            Console.WriteLine("  相对收益 = Double+SIMD / min(Single+SIMD, Single+CUDA)；1.00x 表示没有变化。")
            Console.WriteLine()
            Console.WriteLine(Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute.MixedPrecisionDiagnostics.Report())
            Console.WriteLine()
        End Sub

        Private Function Fmt(ms As Double) As String
            If ms <= 0 Then Return "-"
            Return ms.ToString("F3")
        End Function

        ''' <summary>等宽左对齐（中文字符按两个西文宽度计，避免表格错位）</summary>
        Private Function Pad(text As String, width As Integer) As String
            If text Is Nothing Then text = ""

            Dim shown = 0
            For Each ch In text
                shown += If(AscW(ch) > &H2E80, 2, 1)
            Next

            If shown >= width Then Return text & " "
            Return text & New String(" "c, width - shown)
        End Function

        ' ---------------- 计时 ----------------

        ''' <summary>
        ''' 计时：先预热一次排除 JIT 与首次显存分配，
        ''' 然后重复到累计耗时超过 250 ms（或达到次数上限）再取平均。
        ''' </summary>
        Private Function Measure(action As Action) As Double
            Call action()

            Dim sw As New Stopwatch()
            Dim iterations = 0
            Dim elapsed As Double = 0

            Do
                sw.Restart()
                Call action()
                sw.Stop()

                elapsed += sw.Elapsed.TotalMilliseconds
                iterations += 1
            Loop While elapsed < 250.0 AndAlso iterations < 50

            If iterations = 0 Then Return 0.0
            Return elapsed / iterations
        End Function

        ' ---------------- 各算子 ----------------

        Private Function GemmRow(n As Integer) As Row
            Dim a = Randoms(n * n, seed:=n, scale:=1.0)
            Dim b = Randoms(n * n, seed:=n + 1, scale:=1.0)

            Dim dbl = Measure(Sub()
                                  Tensor.computeKernel.MatMul(New Tensor(a, n, n), New Tensor(b, n, n))
                              End Sub)

            Dim sgl = Measure(Sub()
                                  TensorF.computeKernelF.MatMul(TensorF.FromDoubles(a, New Integer() {n, n}),
                                                                TensorF.FromDoubles(b, New Integer() {n, n}))
                              End Sub)

            Dim singleAction As Action = Sub()
                                             TensorF.computeKernelF.MatMul(
                                                 TensorF.FromDoubles(a, New Integer() {n, n}),
                                                 TensorF.FromDoubles(b, New Integer() {n, n}))
                                         End Sub

            Return New Row With {.Op = "MatMul", .Shape = $"{n}x{n} * {n}x{n}",
                .DoubleMs = dbl, .SingleCpuMs = sgl, .SingleAction = singleAction}
        End Function

        Private Function ConvRow(batch As Integer, h As Integer, w As Integer,
                                 inC As Integer, outC As Integer) As Row
            Dim kH = 3, kW = 3
            Dim x = Randoms(batch * h * w * inC, seed:=h, scale:=1.0)
            Dim f = Randoms(kH * kW * inC * outC, seed:=h + 1, scale:=0.5)

            Dim dbl = Measure(Sub()
                                  Tensor.computeKernel.Conv2D(
                                      New Tensor(x, batch, h, w, inC),
                                      New Tensor(f, kH, kW, inC, outC), Nothing, 1, 1)
                              End Sub)

            Dim sgl = Measure(Sub()
                                  TensorF.computeKernelF.Conv2D(
                                      TensorF.FromDoubles(x, New Integer() {batch, h, w, inC}),
                                      TensorF.FromDoubles(f, New Integer() {kH, kW, inC, outC}), Nothing, 1, 1)
                              End Sub)

            Dim singleAction As Action = Sub()
                                             TensorF.computeKernelF.Conv2D(
                                                 TensorF.FromDoubles(x, New Integer() {batch, h, w, inC}),
                                                 TensorF.FromDoubles(f, New Integer() {kH, kW, inC, outC}),
                                                 Nothing, 1, 1)
                                         End Sub

            Return New Row With {
                .Op = "Conv2D",
                .Shape = $"{batch}x{h}x{w}x{inC} -> {outC}",
                .DoubleMs = dbl, .SingleCpuMs = sgl, .SingleAction = singleAction}
        End Function

        Private Function SoftmaxRow(rows As Integer, cols As Integer) As Row
            Dim x = Randoms(rows * cols, seed:=rows, scale:=8.0)

            ' 张量预先构造好：计时的应当只是算子本身，
            ' 而不是每次迭代都重做一遍"数据准备"（真实训练里输入是现成的）。
            Dim xT = New Tensor(x, rows, cols)
            Dim xF = TensorF.FromDoubles(x, New Integer() {rows, cols})

            Dim dbl = Measure(Sub()
                                  Tensor.computeKernel.Softmax(xT, -1)
                              End Sub)

            Dim sgl = Measure(Sub()
                                  TensorF.computeKernelF.Softmax(xF, -1)
                              End Sub)

            Dim singleAction As Action = Sub()
                                             TensorF.computeKernelF.Softmax(xF, -1)
                                         End Sub

            Return New Row With {.Op = "Softmax", .Shape = $"{rows}x{cols}",
                .DoubleMs = dbl, .SingleCpuMs = sgl, .SingleAction = singleAction}
        End Function

        Private Function SpmmRow(rows As Integer, cols As Integer, nnz As Integer, batch As Integer) As Row
            Dim csr = BuildCsr(rows, cols, nnz, seed:=nnz)
            Dim x = Randoms(batch * rows, seed:=nnz + 1, scale:=1.0)

            For i = 0 To x.Length - 1
                If (i Mod 5) <> 0 Then x(i) = 0.0
            Next

            Dim dbl = Measure(Sub()
                                  Tensor.computeKernel.SpMM(csr, New Tensor(x, batch, rows))
                              End Sub)

            Dim sgl = Measure(Sub()
                                  TensorF.computeKernelF.SpMM(csr, TensorF.FromDoubles(x, New Integer() {batch, rows}))
                              End Sub)

            Dim singleAction As Action = Sub()
                                             TensorF.computeKernelF.SpMM(
                                                 csr, TensorF.FromDoubles(x, New Integer() {batch, rows}))
                                         End Sub

            Return New Row With {
                .Op = "SpMM",
                .Shape = $"{batch}x{rows} * nnz={nnz}",
                .DoubleMs = dbl, .SingleCpuMs = sgl, .SingleAction = singleAction}
        End Function

        ''' <summary>
        ''' 把单精度栈切到 CUDA，再把同一个委托重跑一遍。
        ''' </summary>
        ''' <remarks>
        ''' 后端是进程级共享属性，因此"换后端"等价于"同一个调用换个实现"，
        ''' 不需要为 GPU 另写一份计时代码 —— 这样也保证两条列测的是同一件事。
        ''' </remarks>
        Private Function WithGpu(rows As List(Of Row)) As List(Of Row)
            If Not gpu.CudaTensorF.Register() Then
                Console.WriteLine("CUDA 不可用，跳过 Single+CUDA 列")
                Return rows
            End If

            Dim out As New List(Of Row)

            For Each r In rows
                r.SingleGpuMs = Measure(r.SingleAction)
                out.Add(r)
            Next

            Return out
        End Function

    End Module

End Namespace
