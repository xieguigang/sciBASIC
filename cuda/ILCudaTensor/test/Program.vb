#Region "Microsoft.VisualBasic::3ad420f467caa014e094af76d59b565d, cuda\ILCudaTensor\test\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 492
    '    Code Lines: 325 (66.06%)
    ' Comment Lines: 75 (15.24%)
    '    - Xml Docs: 25.33%
    ' 
    '   Blank Lines: 92 (18.70%)
    '     File Size: 24.50 KB


    ' Module Program
    ' 
    '     Function: BuildRandomCsr, MaxDiff, NetShapeProbe, RelErr
    ' 
    '     Sub: Check, Main
    '     Class ScalarProbe
    ' 
    '         Properties: Name
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Diagnostics
Imports tf = Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports tfMath = Microsoft.VisualBasic.MachineLearning.TensorFlow.Math
Imports tfnn = Microsoft.VisualBasic.MachineLearning.TensorFlow.nn
Imports tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports std = System.Math

' ---------------------------------------------------------------------------
' ILCudaTensor 的 demo 测试
'
' 同一段负载分别在 SIMD CPU 与 CUDA GPU（全 double）后端上执行，
' 逐项断言数值与 CPU 双精度参考一致，并输出性能参考。
'
' 关键点：下面的业务代码里**完全没有出现任何后端相关调用**——
' 只使用 Tensor / Math / nn 的公开 API，通过 CudaTensor.Register() 即可无感切换。
'
' 退出码：0 = 全部通过，1 = 有断言失败，2 = GPU 不可用。
' ---------------------------------------------------------------------------

Module Program

    Private _passed As Integer
    Private _failed As Integer

    Private Sub Check(name As String, ok As Boolean, detail As String)
        If ok Then
            _passed += 1
        Else
            _failed += 1
        End If

        Dim status As String = If(ok, "PASS", "FAIL")
        Console.WriteLine($"    [{status}] {name,-30} {detail}")
    End Sub

    Private Function MaxDiff(x As Double(), y As Double()) As Double
        Dim m As Double = 0

        For i As Integer = 0 To x.Length - 1
            m = std.Max(m, std.Abs(x(i) - y(i)))
        Next

        Return m
    End Function

    Private Function RelErr(got As Double, expected As Double) As Double
        Dim d = std.Abs(got - expected)
        Return If(std.Abs(expected) > 0.0, d / std.Abs(expected), d)
    End Function

    ''' <summary>
    ''' 只使用 <see cref="tfCompute.TensorComputeBase"/> 之中的标量实现，不覆盖任何算子。
    ''' </summary>
    ''' <remarks>
    ''' 用于给做过并行/向量化优化的后端(例如 <see cref="tfCompute.SIMDTensor"/> 的卷积)做逐位对照：
    ''' 优化实现必须与标量实现在**同样的求和顺序**下得到完全相同的结果。
    ''' </remarks>
    Private Class ScalarProbe
        Inherits tfCompute.TensorComputeBase

        Public Overrides ReadOnly Property Name As String = "scalar-probe"
    End Class

    ''' <summary>
    ''' 用与 MNIST CNN 实际网络**完全相同的规模**（N=1、padding=2、inC=32、outC=64）跑一组算子。
    ''' </summary>
    ''' <remarks>
    ''' 小规模用例通过并不代表真实网络也正确：这里专门覆盖
    ''' (a) batch = 1，(b) padding = 2，(c) 较大的 inC/outC，
    ''' 用于排查"小规模测试通过但真实网络不一致"的问题。
    ''' </remarks>
    Private Function NetShapeProbe() As tf.Tensor()
        Dim x = tf.Tensor.Random({1, 14, 14, 32}, seed:=81)
        Dim w = tf.Tensor.Random({5, 5, 32, 64}, seed:=82)
        Dim b = tf.Tensor.Random({64}, seed:=83)
        Dim grad = tf.Tensor.Random({1, 14, 14, 64}, seed:=84)

        Dim poolX = tf.Tensor.Random({1, 28, 28, 32}, seed:=85)
        Dim poolGrad = tf.Tensor.Random({1, 14, 14, 32}, seed:=86)
        Dim poolIdx As tf.Tensor = Nothing

        Return {
            tf.Tensor.computeKernel.Conv2D(x, w, b, 1, 2),
            tf.Tensor.computeKernel.Conv2DBackwardInput(grad, w, New Integer() {1, 14, 14, 32}, 1, 2),
            tf.Tensor.computeKernel.Conv2DBackwardFilter(grad, x, New Integer() {5, 5, 32, 64}, 1, 2),
            tf.Tensor.computeKernel.Conv2DBackwardBias(grad),
            tf.Tensor.computeKernel.MaxPool2D(poolX, 2, 2, 0, poolIdx),
            tf.Tensor.computeKernel.MaxPool2DBackward(poolGrad, poolIdx, New Integer() {1, 28, 28, 32})
        }
    End Function

    ''' <summary>
    ''' 构造一个 CSR 稀疏矩阵：每行固定 fanIn 条边，列索引随机（允许重复与乱序，
    ''' 用于同时检验重复边的累加与乱序行的处理）。
    ''' </summary>
    Private Function BuildRandomCsr(rng As Random, rows As Integer, columns As Integer,
                                    fanIn As Integer) As tfCompute.SparseCsr
        Dim nnz = rows * fanIn
        Dim rowPtr(rows) As Integer
        Dim colIdx(nnz - 1) As Integer
        Dim values(nnz - 1) As Double

        For r As Integer = 0 To rows - 1
            rowPtr(r) = r * fanIn
            For k As Integer = 0 To fanIn - 1
                Dim idx = r * fanIn + k
                colIdx(idx) = rng.Next(columns)
                values(idx) = 0.1 + rng.NextDouble()
            Next
        Next

        rowPtr(rows) = nnz
        Return New tfCompute.SparseCsr(rows, columns, rowPtr, colIdx, values)
    End Function

    Sub Main(args As String())
        Console.WriteLine("=== ILCudaTensor demo test：SIMD CPU vs CUDA GPU（全 double）===")
        Console.WriteLine($"默认后端 : {tf.Tensor.computeKernel.Name}")
        Console.WriteLine($"SIMD 能力: {Microsoft.VisualBasic.Math.SIMD.SIMDEnvironment.Description}")
        Console.WriteLine()

        tfCompute.SIMDTensor.Register()

        ' ---------------- 输入 ----------------
        Const n As Integer = 8192
        Dim x = tf.Tensor.Random({n}, seed:=7)
        Dim y = tf.Tensor.Random({n}, seed:=11)

        Dim mat = tf.Tensor.Random({256, 128}, seed:=3)

        Const dimSize As Integer = 512
        Dim a = tf.Tensor.Random({dimSize, dimSize}, seed:=42)
        Dim b = tf.Tensor.Random({dimSize, dimSize}, seed:=2024)

        Const outer As Integer = 512
        Const classes As Integer = 64
        Dim logits = tf.Tensor.Random({outer, classes}, seed:=99)

        ' 大张量：用于验证两段式全局归约的并行度与精度
        Const bigN As Integer = 400000000

        Dim big = tf.Tensor.Random({bigN}, seed:=1234)

        ' ---------------- 1) SIMD CPU 参考 ----------------
        Console.WriteLine(">> 1) SIMD CPU 参考（双精度）")

        Dim ewCpu = tfMath.sqrt(tfMath.sigmoid(tfMath.tanh(tfMath.exp(x) + y)) * 2.0F)
        Dim trCpu = mat.Transpose()
        Dim matCpu = tfMath.reduce_sum(a * b)
        Dim smCpu = tfnn.softmax(logits)
        Dim lsCpu = tfnn.log_softmax(logits)
        Dim rsCpu = tfMath.reduce_sum(logits, axis:=1)
        Dim amCpu = tfMath.argmax(logits, axis:=1)

        ' Heaviside 阶跃算子: ReLU 系激活函数反向传播所需的 (x > 0) 掩码
        Dim hvCpu = tf.Tensor.computeKernel.Heaviside(x)

        ' ---- SIMD 后端的卷积算子 vs 标量参考实现（必须逐位一致）----
        ' 注意：规模必须超过 SIMDTensor.ConvParallelThreshold(200000)，否则会回退标量路径而测不到目标代码
        Dim probe As New ScalarProbe()

        Dim bigX = tf.Tensor.Random({2, 16, 16, 8}, seed:=71)
        Dim bigW = tf.Tensor.Random({3, 3, 8, 16}, seed:=72)
        Dim bigBias = tf.Tensor.Random({16}, seed:=73)
        Dim bigGrad = tf.Tensor.Random({2, 16, 16, 16}, seed:=74)

        Dim simdConv = tf.Tensor.computeKernel.Conv2D(bigX, bigW, bigBias, 1, 1)
        Dim simdBwInput = tf.Tensor.computeKernel.Conv2DBackwardInput(
            bigGrad, bigW, New Integer() {2, 16, 16, 8}, 1, 1)
        Dim simdBwFilter = tf.Tensor.computeKernel.Conv2DBackwardFilter(
            bigGrad, bigX, New Integer() {3, 3, 8, 16}, 1, 1)

        Dim scalarConv = probe.Conv2D(bigX, bigW, bigBias, 1, 1)
        Dim scalarBwInput = probe.Conv2DBackwardInput(bigGrad, bigW, New Integer() {2, 16, 16, 8}, 1, 1)
        Dim scalarBwFilter = probe.Conv2DBackwardFilter(bigGrad, bigX, New Integer() {3, 3, 8, 16}, 1, 1)

        Dim simdConvErr = MaxDiff(simdConv.Data, scalarConv.Data)
        Dim simdBwInputErr = MaxDiff(simdBwInput.Data, scalarBwInput.Data)
        Dim simdBwFilterErr = MaxDiff(simdBwFilter.Data, scalarBwFilter.Data)

        Check("SIMD conv2d 前向 == 标量", simdConvErr = 0.0, $"maxdiff={simdConvErr:E3}")
        Check("SIMD conv2d 反向-输入 == 标量", simdBwInputErr = 0.0, $"maxdiff={simdBwInputErr:E3}")
        Check("SIMD conv2d 反向-卷积核 == 标量", simdBwFilterErr = 0.0, $"maxdiff={simdBwFilterErr:E3}")

        ' ---- 卷积 / 池化的 CPU 参考（走当前后端 = SIMDTensor）----
        Dim convIn = tf.Tensor.Random({2, 8, 8, 3}, seed:=5)
        Dim convW = tf.Tensor.Random({3, 3, 3, 4}, seed:=6)
        Dim convB = tf.Tensor.Random({4}, seed:=7)
        Dim poolIn = tf.Tensor.Random({2, 8, 8, 4}, seed:=8)
        Dim convGrad = tf.Tensor.Random({2, 8, 8, 4}, seed:=12)
        Dim poolGrad = tf.Tensor.Random({2, 4, 4, 4}, seed:=13)

        Dim convCpu = tf.Tensor.computeKernel.Conv2D(convIn, convW, convB, 1, 1)
        Dim poolIdxCpu As tf.Tensor = Nothing
        Dim poolCpu = tf.Tensor.computeKernel.MaxPool2D(poolIn, 2, 2, 0, poolIdxCpu)
        Dim convBwdInCpu = tf.Tensor.computeKernel.Conv2DBackwardInput(convGrad, convW, convIn.Shape, 1, 1)
        Dim convBwdWCpu = tf.Tensor.computeKernel.Conv2DBackwardFilter(convGrad, convIn, convW.Shape, 1, 1)
        Dim convBwdBCpu = tf.Tensor.computeKernel.Conv2DBackwardBias(convGrad)
        Dim poolBwdCpu = tf.Tensor.computeKernel.MaxPool2DBackward(poolGrad, poolIdxCpu, poolIn.Shape)

        ' ---- 真实网络规模的探针（N=1, padding=2, inC=32, outC=64）----
        Dim netProbeCpu = NetShapeProbe()

        ' ---- 稀疏 SpMM 的 CPU 参考（ScalarProbe 纯主机实现）----
        Dim csrSmall = BuildRandomCsr(New Random(101), 16, 32, 4)
        Dim denseSmall = tf.Tensor.Random({3, 16}, 0.0, 1.0, seed:=41)
        Dim spmmSmallCpu = probe.SpMM(csrSmall, denseSmall)

        Dim csrBig = BuildRandomCsr(New Random(202), 4096, 4096, 32)
        Dim denseBig = tf.Tensor.Random({2, 4096}, 0.0, 1.0, seed:=42)
        Dim spmmBigCpu = probe.SpMM(csrBig, denseBig)

        Dim sw = Stopwatch.StartNew()
        Dim bigSumCpu = tfMath.reduce_sum(big).Data(0)
        Dim bigMaxCpu = tfMath.reduce_max(big).Data(0)
        Dim bigMinCpu = tfMath.reduce_min(big).Data(0)
        sw.Stop()
        Dim cpuBigMs = sw.Elapsed.TotalMilliseconds

        Console.WriteLine($"    大张量全局 sum/max/min {bigN:N0} 元素  用时 {cpuBigMs,8:F3} ms")
        Console.WriteLine()

        ' ---------------- 2) 注册 GPU ----------------
        Console.WriteLine(">> 2) 注册 CUDA GPU 后端")

        Dim opts As New ILCudaRuntime.EngineOptions()

        If Not gpu.CudaTensor.Register(opts) Then
            Console.WriteLine($"    注册失败: {gpu.CudaTensor.LastError}")

            For Each line In opts.Diagnostics
                Console.WriteLine($"      {line}")
            Next

            Environment.ExitCode = 2
            Return
        End If

        Console.WriteLine($"    当前后端 = {tf.Tensor.computeKernel.Name}")

        If gpu.CudaTensor.KernelFailures.Count = 0 Then
            Console.WriteLine("    IL2Cuda 双精度内核：全部翻译并注册成功")
        Else
            For Each f In gpu.CudaTensor.KernelFailures
                Console.WriteLine($"    [内核翻译失败] {f.Key} -> {f.Value}")
            Next
        End If

        Console.WriteLine()

        Dim ewGpu = tfMath.sqrt(tfMath.sigmoid(tfMath.tanh(tfMath.exp(x) + y)) * 2.0F)
        Dim trGpu = mat.Transpose()
        Dim matGpu = tfMath.reduce_sum(a * b)
        Dim smGpu = tfnn.softmax(logits)
        Dim lsGpu = tfnn.log_softmax(logits)
        Dim rsGpu = tfMath.reduce_sum(logits, axis:=1)
        Dim amGpu = tfMath.argmax(logits, axis:=1)

        ' Heaviside 阶跃算子（GPU）
        Dim hvGpu = tf.Tensor.computeKernel.Heaviside(x)

        ' ---- 真实网络规模的探针（GPU）----
        Dim netProbeGpu = NetShapeProbe()
        Dim netProbeNames = {
            "net-shape conv2d 前向",
            "net-shape conv2d 反向-输入",
            "net-shape conv2d 反向-卷积核",
            "net-shape conv2d 反向-偏置",
            "net-shape maxpool2d 前向",
            "net-shape maxpool2d 反向"
        }

        For i As Integer = 0 To netProbeNames.Length - 1
            Dim netDiff = MaxDiff(netProbeCpu(i).Data, netProbeGpu(i).Data)
            ' 卷积反向在 GPU 上是用 atomicAdd 散射累加的，归约顺序与 CPU 的顺次累加不同，
            ' 因此这里容许舍入级差异；真正的回归判据是"不能出现结构性偏差"。
            Check(netProbeNames(i) & " == CPU", netDiff < 1.0E-12, $"maxdiff={netDiff:E3}")
        Next

        ' ---- 显存缓存失效验证：模拟 DataBlock.addImageData 的"就地改写输入数组" ----
        ' CNN 的输入图像块是**复用的数组**，每个样本由 addImageData 就地覆盖写入，
        ' 期间不会调用 InvalidateAllDeviceCaches。若设备端缓存没跟着失效，
        ' 同一个批次内后续样本就会一直复用第一个样本的输入 —— 这会静默污染训练。
        Dim staleImg = tf.Tensor.Random({1, 28, 28, 1}, seed:=95)
        Dim staleW = tf.Tensor.Random({5, 5, 1, 32}, seed:=96)
        Dim staleGrad = tf.Tensor.Random({1, 28, 28, 32}, seed:=97)

        Call tf.Tensor.computeKernel.Conv2DBackwardFilter(
            staleGrad, staleImg, New Integer() {5, 5, 1, 32}, 1, 2)

        ' 就地改写主机数组。
        ' 契约：任何"绕过 Tensor 索引器"的就地写入之后，必须调用 MarkHostModified() 声明失效，
        ' 否则设备端会继续复用旧副本（CNN 的 DataBlock 就是这样在 AddImageData/SetValues 等
        ' 写入路径里统一调用它的）。
        For i As Integer = 0 To staleImg.Data.Length - 1
            staleImg.Data(i) = -staleImg.Data(i)
        Next

        staleImg.MarkHostModified()

        Dim staleResult = tf.Tensor.computeKernel.Conv2DBackwardFilter(
            staleGrad, staleImg, New Integer() {5, 5, 1, 32}, 1, 2)

        Dim savedKernel = tf.Tensor.computeKernel

        tf.Tensor.computeKernel = tfCompute.SIMDTensor.Default
        Dim staleExpect = New ScalarProbe().Conv2DBackwardFilter(
            staleGrad, staleImg, New Integer() {5, 5, 1, 32}, 1, 2)
        tf.Tensor.computeKernel = savedKernel

        ' 判据：若设备端仍复用旧副本，误差会是 O(1) 的结构性偏差（修复前实测 6.9e+01）；
        ' 只要降到舍入量级就说明缓存已经正确失效。
        Dim staleErr = MaxDiff(staleResult.Data, staleExpect.Data)
        Check("就地改写输入后设备缓存已同步", staleErr < 1.0E-12, $"maxdiff={staleErr:E3}")

        sw.Restart()
        Dim bigSumGpu = tfMath.reduce_sum(big).Data(0)
        Dim bigMaxGpu = tfMath.reduce_max(big).Data(0)
        Dim bigMinGpu = tfMath.reduce_min(big).Data(0)
        sw.Stop()
        Dim gpuBigMs = sw.Elapsed.TotalMilliseconds

        ' ---- 卷积 / 池化的 GPU 路径 ----
        ' 注意：这些张量的元素数（2*8*8*3 = 384）远小于默认阈值 MinGpuElements=4096，
        ' 若不调低阈值就会静默回退 CPU，使断言"假通过"。这里临时下调阈值，
        ' 保证确实执行了 GPU 内核。
        Dim savedMinGpu = gpu.CudaTensor.MinGpuElements
        gpu.CudaTensor.MinGpuElements = 1

        Dim convGpu = tf.Tensor.computeKernel.Conv2D(convIn, convW, convB, 1, 1)
        Dim poolIdxGpu As tf.Tensor = Nothing
        Dim poolGpu = tf.Tensor.computeKernel.MaxPool2D(poolIn, 2, 2, 0, poolIdxGpu)
        Dim convBwdInGpu = tf.Tensor.computeKernel.Conv2DBackwardInput(convGrad, convW, convIn.Shape, 1, 1)
        Dim convBwdWGpu = tf.Tensor.computeKernel.Conv2DBackwardFilter(convGrad, convIn, convW.Shape, 1, 1)
        Dim convBwdBGpu = tf.Tensor.computeKernel.Conv2DBackwardBias(convGrad)
        Dim poolBwdGpu = tf.Tensor.computeKernel.MaxPool2DBackward(poolGrad, poolIdxGpu, poolIn.Shape)

        ' ---- 设备端缓存失效验证 ----
        ' 模拟"绕过 Tensor 直接就地改写底层数组"（CNN 训练器的做法）：
        ' 若不调用 InvalidateAllDeviceCaches，GPU 会继续使用显存里的旧副本。
        Dim mutableData As Double() = {1.0, 2.0, 3.0, 4.0}
        Dim mutable = tf.Tensor.Wrap(mutableData, 4)
        Dim sumBefore = tf.Tensor.computeKernel.SumAll(mutable)   ' 触发一次上传
        mutableData(0) = 100.0                                    ' 就地改写，张量自身无从感知
        tf.Tensor.InvalidateAllDeviceCaches()
        Dim sumAfter = tf.Tensor.computeKernel.SumAll(mutable)

        gpu.CudaTensor.MinGpuElements = savedMinGpu

        ' ---------------- 3) 正确性断言 ----------------
        Console.WriteLine(">> 3) 正确性检查（以 CPU 双精度为基准）")

        Dim ewErr = MaxDiff(ewCpu.Data, ewGpu.Data)
        Dim trErr = MaxDiff(trCpu.Data, trGpu.Data)
        Dim smErr = MaxDiff(smCpu.Data, smGpu.Data)
        Dim lsErr = MaxDiff(lsCpu.Data, lsGpu.Data)
        Dim rsErr = MaxDiff(rsCpu.Data, rsGpu.Data)
        Dim amErr = MaxDiff(amCpu.Data, amGpu.Data)
        Dim hvErr = MaxDiff(hvCpu.Data, hvGpu.Data)
        Dim gemmErr = RelErr(matGpu.Data(0), matCpu.Data(0))
        Dim sumErr = RelErr(bigSumGpu, bigSumCpu)
        Dim maxErr = RelErr(bigMaxGpu, bigMaxCpu)
        Dim minErr = RelErr(bigMinGpu, bigMinCpu)

        Check("P2 逐元素链", ewErr < 1.0E-12, $"maxdiff={ewErr:E3}")
        Check("P2 转置", trErr = 0.0, $"maxdiff={trErr:E3}")
        Check("P3 softmax", smErr < 1.0E-14, $"maxdiff={smErr:E3}")
        Check("P3 log_softmax", lsErr < 1.0E-13, $"maxdiff={lsErr:E3}")
        Check("P3 末轴求和", rsErr < 1.0E-12, $"maxdiff={rsErr:E3}")
        Check("P3 末轴 argmax", amErr = 0.0, $"maxdiff={amErr:E3}")

        ' Heaviside: 既要求 CPU/GPU 逐位一致, 也要求取值语义正确(恰好是正元素的个数)
        Dim expectOnes = x.Data.Count(Function(v) v > 0.0)
        Dim actualOnes = hvCpu.Data.Sum()
        Check("P2 Heaviside 阶跃", hvErr = 0.0 AndAlso Math.Abs(actualOnes - expectOnes) < 0.5,
              $"maxdiff={hvErr:E3} ones={actualOnes}/{expectOnes}")
        Check("double GEMM", gemmErr < 1.0E-13, $"relerr={gemmErr:E3}")
        Check("两段式全局 sum", sumErr < 1.0E-13, $"relerr={sumErr:E3}")
        Check("两段式全局 max", maxErr < 1.0E-15, $"relerr={maxErr:E3}")
        Check("两段式全局 min", minErr < 1.0E-15, $"relerr={minErr:E3}")

        ' ---- 卷积 / 池化（Kernels\conv.cu / pool.cu）----
        Dim convErr = MaxDiff(convCpu.Data, convGpu.Data)
        Dim poolErr = MaxDiff(poolCpu.Data, poolGpu.Data)
        Dim poolIdxErr = MaxDiff(poolIdxCpu.Data, poolIdxGpu.Data)
        Dim convBwdInErr = MaxDiff(convBwdInCpu.Data, convBwdInGpu.Data)
        Dim convBwdWErr = MaxDiff(convBwdWCpu.Data, convBwdWGpu.Data)
        Dim convBwdBErr = MaxDiff(convBwdBCpu.Data, convBwdBGpu.Data)
        Dim poolBwdErr = MaxDiff(poolBwdCpu.Data, poolBwdGpu.Data)

        Check("P4 conv2d 前向", convErr < 1.0E-12, $"maxdiff={convErr:E3}")
        Check("P4 maxpool2d 前向", poolErr = 0.0, $"maxdiff={poolErr:E3}")
        Check("P4 maxpool2d argmax", poolIdxErr = 0.0, $"maxdiff={poolIdxErr:E3}")
        Check("P4 conv2d 反向-输入", convBwdInErr < 1.0E-12, $"maxdiff={convBwdInErr:E3}")
        Check("P4 conv2d 反向-卷积核", convBwdWErr < 1.0E-12, $"maxdiff={convBwdWErr:E3}")
        Check("P4 conv2d 反向-偏置", convBwdBErr < 1.0E-12, $"maxdiff={convBwdBErr:E3}")
        Check("P4 maxpool2d 反向", poolBwdErr < 1.0E-15, $"maxdiff={poolBwdErr:E3}")

        ' 缓存失效：就地改写 + InvalidateAllDeviceCaches 之后，GPU 必须看到新值
        Check("P4 设备端缓存失效",
              std.Abs(sumBefore - 10.0) < 1.0E-12 AndAlso std.Abs(sumAfter - 109.0) < 1.0E-12,
              $"before={sumBefore}, after={sumAfter}")

        ' ---- 稀疏 SpMM（Kernels\spmm.cu）----
        ' 小规模：临时下调 MinSparseNnz 以确实命中 GPU 内核（否则会静默回退 CPU 造成“假通过”）
        Dim savedMinNnz = gpu.CudaTensor.MinSparseNnz
        gpu.CudaTensor.MinSparseNnz = 1
        Dim spmmSmallGpu = tf.Tensor.computeKernel.SpMM(csrSmall, denseSmall)
        gpu.CudaTensor.MinSparseNnz = savedMinNnz

        ' 大规模：nnz 超过默认阈值，自然走 GPU
        Dim spmmBigGpu = tf.Tensor.computeKernel.SpMM(csrBig, denseBig)

        Dim spmmSmallErr = MaxDiff(spmmSmallCpu.Data, spmmSmallGpu.Data)
        Dim spmmBigErr = MaxDiff(spmmBigCpu.Data, spmmBigGpu.Data)

        ' 输出走 atomicAdd 累加，求和顺序与 CPU 的顺次累加不同 → 只容许舍入级差异
        Check("SpMM 稀疏(小规模/强制GPU) == CPU", spmmSmallErr < 1.0E-12, $"maxdiff={spmmSmallErr:E3}")
        Check("SpMM 稀疏(nnz 超过阈值) == CPU", spmmBigErr < 1.0E-12, $"maxdiff={spmmBigErr:E3}")

        ' ---- 稀疏权重就地修改 + MarkModified：GPU 必须取到新权重 ----
        Dim csrMut = BuildRandomCsr(New Random(303), 16, 16, 4)
        Dim denseMut = tf.Tensor.Random({2, 16}, 0.0, 1.0, seed:=43)

        gpu.CudaTensor.MinSparseNnz = 1
        Dim mutBefore = tf.Tensor.computeKernel.SpMM(csrMut, denseMut)   ' 先让 CSR 进入显存缓存
        For i As Integer = 0 To csrMut.Values.Length - 1
            csrMut.Values(i) *= 2.0
        Next
        csrMut.MarkModified()
        Dim mutAfter = tf.Tensor.computeKernel.SpMM(csrMut, denseMut)
        gpu.CudaTensor.MinSparseNnz = savedMinNnz

        Dim mutRef = probe.SpMM(csrMut, denseMut)
        Dim mutErr = MaxDiff(mutRef.Data, mutAfter.Data)
        ' 若 MarkModified 未生效，设备端会复用旧权重，误差将是 O(1) 的结构性偏差
        Check("SparseCsr.MarkModified 后 GPU 权重已同步", mutErr < 1.0E-12, $"maxdiff={mutErr:E3}")

        Dim rowSums = tfMath.reduce_sum(smGpu, axis:=1)
        Dim rowErr As Double = 0
        For i As Integer = 0 To rowSums.Length - 1
            rowErr = std.Max(rowErr, std.Abs(rowSums.Data(i) - 1.0))
        Next
        Check("softmax 行和 = 1", rowErr < 1.0E-14, $"maxerr={rowErr:E3}")

        ' ---- 融合 LIF 单步（Kernels\lif.cu）：设备常驻状态 + 就地更新 ----
        ' 这是脉冲网络的核心算子：把「稀疏输入 → 泄漏积分 → 阈值触发 → 复位 → 计数累加」
        ' 压成一次调用，状态张量钉在显存里跨时间步复用（每步零主机往返）。
        ' 判据：与 CPU 标量实现<b>逐位一致</b>——脉冲计数是整数，任何舍入差异都会放大成计数偏差。
        Const lifN As Integer = 4096
        Const lifSteps As Integer = 8
        Const lifBeta As Double = 0.9
        Const lifThreshold As Double = 1.0

        Dim lifCsr = BuildRandomCsr(New Random(505), lifN, lifN, 32)
        Dim lifExt(lifSteps - 1) As tf.Tensor

        For t As Integer = 0 To lifSteps - 1
            lifExt(t) = tf.Tensor.Random({1, lifN}, -0.2, 0.6, seed:=600 + t)
        Next

        ' ---- CPU 参考（标量实现，与逐算子路径逐比特等价）----
        Dim cpuH = New tf.Tensor(1, lifN)
        Dim cpuS = New tf.Tensor(1, lifN)
        Dim cpuCounts = New tf.Tensor(1, lifN)
        Dim cpuPrev = New tf.Tensor(1, lifN)
        Dim cpuAllFused As Boolean = True

        For t As Integer = 0 To lifSteps - 1
            cpuAllFused = cpuAllFused AndAlso probe.LifStep(
                lifCsr, cpuPrev, lifExt(t), cpuH, cpuS, cpuCounts, lifBeta, lifThreshold, False)

            ' 交换双缓冲：本步的输出脉冲成为下一步的递归输入（零拷贝）
            Dim swapS = cpuPrev

            cpuPrev = cpuS
            cpuS = swapS
        Next

        Check("LifStep CPU 标量实现可用", cpuAllFused, $"counts.sum={cpuCounts.Data.Sum():F0}")

        ' ---- GPU 双精度档：h / s / counts 全部钉成双精度常驻 ----
        Dim cuda = gpu.CudaTensor.Current
        Dim gpuH = New tf.Tensor(1, lifN)
        Dim gpuS = New tf.Tensor(1, lifN)
        Dim gpuCounts = New tf.Tensor(1, lifN)
        Dim gpuPrev = New tf.Tensor(1, lifN)

        Call cuda.PinDevice64(gpuH, "lif.H", zeroFill:=False)
        Call cuda.PinDevice64(gpuS, "lif.S", zeroFill:=False)
        Call cuda.PinDevice64(gpuCounts, "lif.counts", zeroFill:=True)
        Call cuda.PinDevice64(gpuPrev, "lif.S_prev", zeroFill:=False)

        Dim fusedSteps As Integer = 0

        For t As Integer = 0 To lifSteps - 1
            If tf.Tensor.computeKernel.LifStep(lifCsr, gpuPrev, lifExt(t), gpuH, gpuS, gpuCounts,
                                               lifBeta, lifThreshold, False) Then
                fusedSteps += 1
            End If

            Dim swapS = gpuPrev

            gpuPrev = gpuS
            gpuS = swapS
        Next

        ' 设备为主副本：读主机内容之前必须显式同步
        Call cuda.SyncFromDevice(gpuH)
        Call cuda.SyncFromDevice(gpuPrev)
        Call cuda.SyncFromDevice(gpuCounts)

        Dim lifHErr = MaxDiff(cpuH.Data, gpuH.Data)
        Dim lifSErr = MaxDiff(cpuPrev.Data, gpuPrev.Data)
        Dim lifCountsErr = MaxDiff(cpuCounts.Data, gpuCounts.Data)

        Check("融合 LIF 确实走了 GPU 内核", fusedSteps = lifSteps, $"fused={fusedSteps}/{lifSteps}")
        Check("融合 LIF 膜电位 == CPU", lifHErr = 0.0, $"maxdiff={lifHErr:E3}")
        Check("融合 LIF 脉冲 == CPU", lifSErr = 0.0, $"maxdiff={lifSErr:E3}")
        Check("融合 LIF 脉冲计数 == CPU", lifCountsErr = 0.0, $"maxdiff={lifCountsErr:E3}")

        ' ---- 单精度档（最快档）：h / counts 钉单精度，脉冲仍双精度 ----
        Dim f32H = New tf.Tensor(1, lifN)
        Dim f32S = New tf.Tensor(1, lifN)
        Dim f32Counts = New tf.Tensor(1, lifN)
        Dim f32Prev = New tf.Tensor(1, lifN)

        Call cuda.PinDevice64(f32S, "lif.f32.S", zeroFill:=False)
        Call cuda.PinDevice64(f32Prev, "lif.f32.S_prev", zeroFill:=False)
        Call cuda.PinDevice(f32H, "lif.f32.H", zeroFill:=False)
        Call cuda.PinDevice(f32Counts, "lif.f32.counts", zeroFill:=True)

        Dim f32FusedSteps As Integer = 0

        For t As Integer = 0 To lifSteps - 1
            If tf.Tensor.computeKernel.LifStep(lifCsr, f32Prev, lifExt(t), f32H, f32S, f32Counts,
                                               lifBeta, lifThreshold, False) Then
                f32FusedSteps += 1
            End If

            Dim swapS = f32Prev

            f32Prev = f32S
            f32S = swapS
        Next

        Call cuda.SyncFromDevice(f32H)
        Call cuda.SyncFromDevice(f32Prev)
        Call cuda.SyncFromDevice(f32Counts)

        Dim f32HErr = MaxDiff(cpuH.Data, f32H.Data)
        Dim f32CountsErr = MaxDiff(cpuCounts.Data, f32Counts.Data)
        Dim f32DiffNeurons As Integer = 0

        For i As Integer = 0 To f32Counts.Length - 1
            If f32Counts.Data(i) <> cpuCounts.Data(i) Then f32DiffNeurons += 1
        Next

        Check("融合 LIF 单精度档确实走了 GPU 内核", f32FusedSteps = lifSteps, $"fused={f32FusedSteps}/{lifSteps}")
        ' 单精度档是"最快档"：这里如实报告偏差，不作硬断言（精度受控档是上面的双精度通道）
        Console.WriteLine($"    单精度档参考：膜电位 maxdiff={f32HErr:E3}  计数 maxdiff={f32CountsErr:F0}  " &
                          $"计数不同神经元={f32DiffNeurons}/{lifN}")

        ' 释放常驻缓冲（不释放会一直占着显存，直到后端 Dispose）
        For Each t In {gpuH, gpuS, gpuCounts, gpuPrev, f32H, f32S, f32Counts, f32Prev}
            Call cuda.UnpinDevice(t)
        Next

        Check("常驻缓冲已全部释放", cuda.IsDevicePinned(gpuH) = False AndAlso cuda.PinnedDeviceBytes = 0,
              $"pinned={cuda.PinnedDeviceBytes} bytes")

        ' ---------------- 4) 性能参考 ----------------
        Console.WriteLine()
        Console.WriteLine(">> 4) 性能参考")
        Console.WriteLine($"    大张量全局归约 {bigN:N0} 元素   SIMD {cpuBigMs,8:F3} ms   CUDA {gpuBigMs,8:F3} ms")

        If gpuBigMs > 0 Then
            Console.WriteLine($"    加速比: {cpuBigMs / gpuBigMs:F2} x")
        End If

        ' ---- 稀疏 SpMM 性能参考（SNN 加载连接组的核心算子）----
        Dim csrPerf = BuildRandomCsr(New Random(404), 20000, 20000, 64)
        Dim densePerf = tf.Tensor.Random({1, 20000}, 0.0, 1.0, seed:=44)

        Dim swSparse As Stopwatch = Stopwatch.StartNew()
        Dim perfCpu = probe.SpMM(csrPerf, densePerf)
        swSparse.Stop()
        Dim cpuSparseMs = swSparse.Elapsed.TotalMilliseconds

        gpu.CudaTensor.MinSparseNnz = 1
        ' 预热一次：把 CSR 与 dense 上传到显存（后续调用复用驻留缓冲）
        Call tf.Tensor.computeKernel.SpMM(csrPerf, densePerf)
        swSparse.Restart()
        Dim perfGpu = tf.Tensor.computeKernel.SpMM(csrPerf, densePerf)
        swSparse.Stop()
        Dim gpuSparseMs = swSparse.Elapsed.TotalMilliseconds
        gpu.CudaTensor.MinSparseNnz = savedMinNnz

        Dim perfErr = MaxDiff(perfCpu.Data, perfGpu.Data)
        Console.WriteLine($"    稀疏 SpMM nnz={csrPerf.NonZeros:N0}   SIMD {cpuSparseMs,8:F3} ms   CUDA {gpuSparseMs,8:F3} ms   maxdiff={perfErr:E3}")

        If gpuSparseMs > 0 Then
            Console.WriteLine($"    稀疏 SpMM 加速比: {cpuSparseMs / gpuSparseMs:F2} x")
        End If

        ' ---- 融合 LIF 单步性能参考（脉冲网络的真实负载形态）----
        ' 每步：稀疏输入 + 泄漏积分 + 阈值触发 + 复位 + 计数累加；状态跨步复用。
        Const perfLifSteps As Integer = 30
        Dim perfCsr = BuildRandomCsr(New Random(606), 20000, 20000, 64)
        Dim perfExt(perfLifSteps - 1) As tf.Tensor

        For t As Integer = 0 To perfLifSteps - 1
            perfExt(t) = tf.Tensor.Random({1, 20000}, -0.2, 0.6, seed:=700 + t)
        Next

        ' CPU：标量融合实现
        Dim perfCpuH = New tf.Tensor(1, 20000)
        Dim perfCpuS = New tf.Tensor(1, 20000)
        Dim perfCpuCounts = New tf.Tensor(1, 20000)
        Dim perfCpuPrev = New tf.Tensor(1, 20000)

        Dim swLif As Stopwatch = Stopwatch.StartNew()

        For t As Integer = 0 To perfLifSteps - 1
            Call probe.LifStep(perfCsr, perfCpuPrev, perfExt(t), perfCpuH, perfCpuS, perfCpuCounts,
                               0.9, 1.0, False)

            Dim swapS = perfCpuPrev

            perfCpuPrev = perfCpuS
            perfCpuS = swapS
        Next

        swLif.Stop()
        Dim cpuLifMs = swLif.Elapsed.TotalMilliseconds

        ' GPU：设备常驻 + 融合内核（每步零往返，只有外部电流需要上传）
        Dim perfGpuH = New tf.Tensor(1, 20000)
        Dim perfGpuS = New tf.Tensor(1, 20000)
        Dim perfGpuCounts = New tf.Tensor(1, 20000)
        Dim perfGpuPrev = New tf.Tensor(1, 20000)

        Call cuda.PinDevice64(perfGpuH, "perf.H", zeroFill:=False)
        Call cuda.PinDevice64(perfGpuS, "perf.S", zeroFill:=False)
        Call cuda.PinDevice64(perfGpuCounts, "perf.counts", zeroFill:=True)
        Call cuda.PinDevice64(perfGpuPrev, "perf.S_prev", zeroFill:=False)

        ' 预热：让 CSR 与状态先上传一次（正式计时只反映稳态）
        Call tf.Tensor.computeKernel.LifStep(perfCsr, perfGpuPrev, perfExt(0), perfGpuH, perfGpuS,
                                            perfGpuCounts, 0.9, 1.0, False)

        swLif.Restart()

        For t As Integer = 0 To perfLifSteps - 1
            Call tf.Tensor.computeKernel.LifStep(perfCsr, perfGpuPrev, perfExt(t), perfGpuH, perfGpuS,
                                                 perfGpuCounts, 0.9, 1.0, False)

            Dim swapS = perfGpuPrev

            perfGpuPrev = perfGpuS
            perfGpuS = swapS
        Next

        swLif.Stop()
        Dim gpuLifMs = swLif.Elapsed.TotalMilliseconds

        Call cuda.SyncFromDevice(perfGpuPrev)
        Call cuda.SyncFromDevice(perfGpuCounts)

        Dim perfLifErr = MaxDiff(perfCpuPrev.Data, perfGpuPrev.Data)
        Dim perfLifCountsErr = MaxDiff(perfCpuCounts.Data, perfGpuCounts.Data)

        For Each t In {perfGpuH, perfGpuS, perfGpuCounts, perfGpuPrev}
            Call cuda.UnpinDevice(t)
        Next

        Console.WriteLine($"    融合 LIF {perfLifSteps} 步  N=20,000 nnz={perfCsr.NonZeros:N0}   " &
                          $"CPU {cpuLifMs,8:F3} ms   CUDA {gpuLifMs,8:F3} ms   " &
                          $"maxdiff={perfLifErr:E3}/{perfLifCountsErr:E3}")

        If gpuLifMs > 0 Then
            Console.WriteLine($"    融合 LIF 加速比: {cpuLifMs / gpuLifMs:F2} x")
        End If

        ' ---------------- 收尾 ----------------
        gpu.CudaTensor.Unregister()

        Console.WriteLine()
        Console.WriteLine($"== 测试结果: {_passed} 通过 / {_failed} 失败 ==")

        Environment.ExitCode = If(_failed = 0, 0, 1)
    End Sub

End Module
