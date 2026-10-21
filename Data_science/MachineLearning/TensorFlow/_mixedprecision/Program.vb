' ---------------------------------------------------------------------------
' 混合精度重构的验证装置
'
' 用途：把同一个算子分别在「双精度栈 tensor」与「单精度栈 tensorF」上跑一遍，
'       比较两者的数值差异，确保新增到 ITensorComputeF 的算子在语义上与
'       双精度栈严格对齐（这是后面所有上层改造的安全网）。
'
' 为什么必须有这个东西：
'   * 缺失算子在后端里是**静默 CPU 兜底**（全仓没有 NotImplementedException），
'     单看"跑通了"完全无法判断这个算子是否真的被正确实现；
'   * 混合精度改造的风险不在性能而在"数值漂移"，漂移必须被量化而不是被感知。
'
' 用法：
'   MixedPrecisionBench           运行双精度 / 单精度对拍，全部通过返回 0
'   MixedPrecisionBench -v        额外打印每个算子的实际最大误差
' ---------------------------------------------------------------------------

Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports std = System.Math

Namespace MixedPrecisionBench

    Module Program

        ''' <summary>一次比对的结果</summary>
        Friend Structure CaseResult
            Friend Name As String
            Friend Passed As Boolean
            Friend Detail As String

            Friend Sub New(name As String, passed As Boolean, detail As String)
                Me.Name = name
                Me.Passed = passed
                Me.Detail = detail
            End Sub
        End Structure

        ''' <summary>是否打印每个算子的实际误差（便于量产之前先看数量级）</summary>
        Friend Verbose As Boolean = False

        Function Main(args As String()) As Integer
            Verbose = args IsNot Nothing AndAlso args.Contains("-v")

            ' -gpu：把单精度栈切到 CUDA 后端，跑同一批用例。
            ' 此时参与比较的是「双精度 SIMD（参考）」 vs 「单精度 CUDA（被测）」。
            Dim useGpu = args IsNot Nothing AndAlso args.Contains("-gpu")
            Dim gpuReady = False

            If useGpu Then
                gpuReady = SetupGpuBackend(args.Contains("-small"))
            End If

            If args IsNot Nothing AndAlso args.Contains("-bench") Then
                Call Bench.Run(useGpu)
                Return 0
            End If

            Dim results As New List(Of CaseResult)

            Call OperatorParity.RunAll(results)

            Dim failed = results.Where(Function(r) Not r.Passed).ToList()

            Console.WriteLine()
            Console.WriteLine("================ 混合精度双/单精度算子对拍 ================")
            Console.WriteLine($"  参考: tensor  (Double, {Tensor.computeKernel.Name})")
            Console.WriteLine($"  被测: tensorF (Single, {TensorF.computeKernelF.Name})")

            If useGpu Then
                Console.WriteLine($"  GPU:  {If(gpuReady, "已注册并把阈值压到 0（强制走内核）", "不可用，本次退化为纯 CPU 对拍")}")
            End If

            Console.WriteLine()

            For Each r In results
                Console.WriteLine($"  [{If(r.Passed, "PASS", "FAIL")}] {r.Name,-34} {r.Detail}")
            Next

            Console.WriteLine()
            Console.WriteLine($"  合计 {results.Count} 项，通过 {results.Count - failed.Count} 项，失败 {failed.Count} 项")
            Console.WriteLine()

            If failed.Count > 0 Then Return 1
            Return 0
        End Function

        ''' <summary>
        ''' 注册单精度 CUDA 后端。
        ''' </summary>
        ''' <param name="forceSmall">
        ''' 把所有规模阈值压到 0，让每个算子都<b>必须</b>走内核路径。
        ''' 默认的用例尺寸是为了跑得快而设的，普遍低于 GPU 阈值；
        ''' 不压阈值的话这套用例会全部静默回退 CPU，
        ''' 而"跑通了"完全不能证明内核写对了 —— 这正是本次改造最需要防的那种假象。
        ''' </param>
        Friend Function SetupGpuBackend(forceSmall As Boolean) As Boolean
            '  -small 之外还允许用 -all-gpu 显式表达同一语义
            Dim ok As Boolean

            Try
                ok = gpu.CudaTensorF.Register()
            Catch ex As Exception
                Console.WriteLine($"CUDA 后端注册异常: {ex.Message}")
                Return False
            End Try

            If Not ok Then
                Console.WriteLine($"CUDA 后端不可用: {gpu.CudaTensorF.LastError}")
                Return False
            End If

            If forceSmall Then
                gpu.CudaTensorF.MinGpuElementsF = 0
                gpu.CudaTensorF.MinGemmElementsF = 0
                gpu.CudaTensorF.MinConvElementsF = 0
                gpu.CudaTensorF.MinPoolElementsF = 0
            End If

            Console.WriteLine(gpu.CudaTensorF.Current.DescribeKernels())
            Console.WriteLine()

            Return True
        End Function

        ' ---------------- 工具 ----------------

        ''' <summary>用固定种子生成 [-scale, scale] 均匀分布的数组，保证两次构建的数据一致。</summary>
        Friend Function Randoms(n As Integer, seed As Integer, Optional scale As Double = 1.0) As Double()
            Dim rnd As New Random(seed)
            Dim a(n - 1) As Double

            For i = 0 To n - 1
                a(i) = (rnd.NextDouble() * 2.0 - 1.0) * scale
            Next

            Return a
        End Function

        ''' <summary>把双精度数组做成同形的单精度张量。</summary>
        Friend Function AsF(data As Double(), ParamArray shape As Integer()) As TensorF
            Return TensorF.FromDoubles(data, shape)
        End Function

        ''' <summary>双精度参考张量</summary>
        Friend Function AsD(data As Double(), ParamArray shape As Integer()) As Tensor
            Return New Tensor(data, shape)
        End Function

        ''' <summary>逐元素最大绝对差</summary>
        Friend Function MaxAbsDiff(ref As Double(), got As Single()) As Double
            Dim m As Double = 0

            For i = 0 To ref.Length - 1
                Dim d = std.Abs(ref(i) - CDbl(got(i)))
                If d > m Then m = d
            Next

            Return m
        End Function

        ''' <summary>参考值的最大绝对值（用于把"绝对容差"换算成相对于量级的容差）</summary>
        Friend Function MaxAbs(data As Double()) As Double
            Dim m As Double = 0
            For Each v In data
                Dim a = std.Abs(v)
                If a > m Then m = a
            Next
            Return m
        End Function

        ''' <summary>
        ''' 比对两个张量。容差按参考值的量级放大：<c>tol = relTol · max|ref|</c>，
        ''' 并给一个 1e-6 的下限，避免全零张量退化成绝对相等判定。
        ''' </summary>
        Friend Function Compare(name As String, reference As Tensor, candidate As TensorF,
                                Optional relTol As Double = 0.00001) As CaseResult
            If reference.Shape.Length <> candidate.Shape.Length Then
                Return New CaseResult(name, False, $"秩不一致 {reference.Shape.Length} vs {candidate.Shape.Length}")
            End If

            For i = 0 To reference.Shape.Length - 1
                If reference.Shape(i) <> candidate.Shape(i) Then
                    Return New CaseResult(name, False,
                        $"形状不一致 [{String.Join(",", reference.Shape)}] vs [{String.Join(",", candidate.Shape)}]")
                End If
            Next

            Dim diff = MaxAbsDiff(reference.Data, candidate.Data)
            Dim scale = std.Max(MaxAbs(reference.Data), 1.0)
            Dim tol = std.Max(relTol * scale, 0.000001)
            Dim passed = diff <= tol

            Return New CaseResult(name, passed, $"maxAbsDiff={diff:E3} tol={tol:E3} shape=[{String.Join(",", reference.Shape)}]")
        End Function

        ''' <summary>比对整数值张量（argmax / argmin 的下标必须逐位相等）</summary>
        Friend Function CompareExact(name As String, reference As Tensor, candidate As TensorF) As CaseResult
            If reference.Length <> candidate.Length Then
                Return New CaseResult(name, False, $"长度不一致 {reference.Length} vs {candidate.Length}")
            End If

            For i = 0 To reference.Length - 1
                If CInt(reference.Data(i)) <> CInt(candidate.Data(i)) Then
                    Return New CaseResult(name, False, $"第 {i} 个元素 {reference.Data(i)} vs {candidate.Data(i)}")
                End If
            Next

            Return New CaseResult(name, True, $"逐位相等 (n={reference.Length})")
        End Function

        ''' <summary>标量比对</summary>
        Friend Function CompareScalar(name As String, reference As Double, candidate As Double,
                                      Optional relTol As Double = 0.00001) As CaseResult
            Dim diff = std.Abs(reference - candidate)
            Dim tol = std.Max(relTol * std.Max(std.Abs(reference), 1.0), 0.000001)
            Return New CaseResult(name, diff <= tol, $"ref={reference:E6} got={candidate:E6} diff={diff:E3} tol={tol:E3}")
        End Function

    End Module

End Namespace
