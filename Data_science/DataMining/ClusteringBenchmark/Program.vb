Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Math.SIMD

''' <summary>
''' 聚类算法性能基准 / 数值对拍的命令行入口。
''' </summary>
Public Module Program

    Private Const HelpText As String =
"ClusteringBenchmark —— 聚类算法 SIMD + Parallel 性能基准

用法:
  ClusteringBenchmark [--bench] [--quick] [--full] [--save <file.md>]
  ClusteringBenchmark --parity

选项:
  --bench     运行性能基准（默认行为），同时跑 SIMD+Parallel 与纯标量串行两条路径
  --quick     小规模数据（用于冒烟/CI）
  --full      额外包含谱聚类（O(n^3) 特征分解，较慢）
  --save      把对比表格写成 Markdown 文件
  --parity    只运行数值对拍（距离/选择/成对矩阵/聚类一致性）
  --help      显示本帮助

说明:
  '纯标量串行' 基线通过 SIMDEnvironment.config = disable 与 SimdParallel.Enable = False 得到，
  因此表中的加速比反映的是 'SIMD + 分块并行' 相对 '标量 for 循环' 的综合收益。"

    Public Function Main(args As String()) As Integer
        If args.Any(Function(a) a = "--help" OrElse a = "-h" OrElse a = "/?") Then
            Call Console.WriteLine(HelpText)

            Return 0
        End If

        If args.Any(Function(a) a = "--parity") Then
            Return ParityChecks.Run()
        End If

        Dim quick As Boolean = args.Any(Function(a) a = "--quick")
        Dim full As Boolean = args.Any(Function(a) a = "--full")
        Dim savePath As String = GetOption(args, "--save")

        ' 关掉进度条与调试输出，避免污染测量结果（Tqdm 直接写 Console.Out）
        Call Microsoft.VisualBasic.ApplicationServices.App.SetVariable("tqdm", "FALSE")
        Microsoft.VisualBasic.VBDebugger.Mute = True

        Call Console.WriteLine($"ClusteringBenchmark  |  mode={(If(quick, "quick", "default"))}  |  {SIMDEnvironment.Description}")
        Call Console.WriteLine($"SIMD enabled: {SIMDEnvironment.IsEnabled}, parallel: {SimdParallel.Enable}, CPUs: {Environment.ProcessorCount}")

        ' 预热：让两条路径的 JIT 成本尽量对称
        Call WarmUp()

        SIMDEnvironment.config = SIMDConfiguration.auto
        SimdParallel.Enable = True

        Call Console.WriteLine()
        Call Console.WriteLine("running: SIMD + Parallel ...")

        Dim fast As BenchCases.CaseResult() = BenchCases.RunAll(quick, full)

        SIMDEnvironment.config = SIMDConfiguration.disable
        SimdParallel.Enable = False

        Call Console.WriteLine("running: scalar + serial (baseline) ...")

        Dim baseline As BenchCases.CaseResult() = BenchCases.RunAll(quick, full)

        SIMDEnvironment.config = SIMDConfiguration.auto
        SimdParallel.Enable = True

        Dim markdown As String = Report.BuildMarkdown(fast, baseline, "Clustering SIMD + Parallel benchmark")

        Call Console.WriteLine()
        Call Console.Write(markdown)

        If Not String.IsNullOrEmpty(savePath) Then
            Call File.WriteAllText(savePath, markdown, New UTF8Encoding(False))
            Call Console.WriteLine($"saved: {savePath}")
        End If

        Return 0
    End Function

    Private Function GetOption(args As String(), name As String) As String
        For i As Integer = 0 To args.Length - 1
            If args(i) = name AndAlso i + 1 < args.Length Then
                Return args(i + 1)
            End If
        Next

        Return ""
    End Function

    Private Sub WarmUp()
        Dim a As Double() = New Double(63) {}
        Dim b As Double() = New Double(63) {}

        For i As Integer = 0 To a.Length - 1
            a(i) = i * 0.5
            b(i) = i * 0.25
        Next

        Dim sink As Double = 0

        For i As Integer = 0 To 20000
            sink += SimdReduce.DistanceSquared(a, b)
        Next

        Dim values As Double() = New Double(999) {}

        For i As Integer = 0 To values.Length - 1
            values(i) = (i * 37) Mod 101
        Next

        For i As Integer = 0 To 200
            Call SimdSelect.TopKIndices(values, 8)
        Next

        If sink = Double.MinValue Then Console.WriteLine(sink)
    End Sub
End Module
