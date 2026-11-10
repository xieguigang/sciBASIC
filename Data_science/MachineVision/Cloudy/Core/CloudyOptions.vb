Imports std = System.Math

Namespace Core

    ''' <summary>
    ''' 全局计算选项：并行策略、随机种子、内存预算与诊断输出。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 点云算法的耗时与内存高度依赖数据规模，同一套参数在 1 万点和 1 亿点上的最优取值完全不同。
    ''' 这个类型把这些“策略性”参数从算法实现里抽出来：
    ''' 算法本身只描述数学，调用方（流水线/测试/桌面端）决定并行度与预算。
    ''' </para>
    ''' <para>
    ''' <b>默认值的选择依据</b>：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>
    ''' <see cref="ParallelThreshold"/> = 4096：低于这个行数时线程池调度（约 10~30 微秒）
    ''' 会盖过实际计算；4096 行 × 每行 30 邻居的法向估计刚好是并行开始有收益的临界点。
    ''' </description></item>
    ''' <item><description>
    ''' <see cref="Seed"/> 固定：RANSAC、4PCS、随机采样都是随机算法，
    ''' 固定的默认种子保证“同样的输入两次运行结果一致”，这是工业检测可追溯性的基本要求。
    ''' 需要多次独立采样时用 <see cref="CreateRandom(Integer)"/> 派生不同种子。
    ''' </description></item>
    ''' <item><description>
    ''' <see cref="MemoryBudgetMB"/> = 4096：TSDF/泊松这类体素算法需要按预算决定分辨率，
    ''' 否则一个 2mm 分辨率的车间体积会直接吃掉几十 GB。
    ''' </description></item>
    ''' </list>
    ''' <para>
    ''' <b>线程安全</b>：实例本身可变（调用方按需调整），但一旦传入算法就应视为只读；
    ''' 并行算法只读取这些字段，不会修改它们。
    ''' </para>
    ''' </remarks>
    Public Class CloudyOptions

        ''' <summary>
        ''' 进程级默认选项。修改它会全局生效，测试中请使用 <see cref="Clone"/> 派生独立实例。
        ''' </summary>
        Public Shared Property [Default] As CloudyOptions = New CloudyOptions()

        ''' <summary>
        ''' 是否启用多线程（默认 <c>True</c>）。关闭后所有算法退化为单线程，便于基准对照与调试。
        ''' </summary>
        Public Property EnableParallel As Boolean = True

        ''' <summary>
        ''' 触发并行的最小“行数”（点是行、面片也是行）。默认 4096。
        ''' </summary>
        Public Property ParallelThreshold As Integer = 4096

        ''' <summary>
        ''' 最大并行度；<c>0</c> 表示使用 <see cref="Environment.ProcessorCount"/>。
        ''' </summary>
        Public Property MaxDegreeOfParallelism As Integer = 0

        ''' <summary>
        ''' 默认随机种子（RANSAC、采样、ESF 成对采样等）。
        ''' </summary>
        Public Property Seed As Integer = 20240101

        ''' <summary>
        ''' 体素/网格类算法的内存预算（MB）。超过预算时应降低分辨率而不是抛内存不足。
        ''' </summary>
        Public Property MemoryBudgetMB As Integer = 4096

        ''' <summary>
        ''' 诊断信息接收器（可选）。为空时算法不产生任何日志输出。
        ''' </summary>
        ''' <remarks>
        ''' 刻意使用回调而不是直接写控制台：库不应该决定日志格式与去向，
        ''' 而且热循环里若忘记关闭输出会造成严重的性能塌陷。
        ''' 约定只在“每次迭代/每个阶段”写一条，不在逐点循环里写。
        ''' </remarks>
        Public Property DiagnosticSink As Action(Of String) = Nothing

        ''' <summary>
        ''' 实际使用的线程数。
        ''' </summary>
        Public ReadOnly Property Workers As Integer
            Get
                If MaxDegreeOfParallelism > 0 Then Return MaxDegreeOfParallelism

                Return std.Max(1, Environment.ProcessorCount)
            End Get
        End Property

        ''' <summary>
        ''' 校验选项合法性（缩放到合理区间，而不是抛异常）。
        ''' </summary>
        ''' <remarks>
        ''' 这些参数是“性能旋钮”，不是业务约束；用户传入 0 或负数时按默认值处理
        ''' 比抛异常更符合工程直觉（尤其是在脚本/交互式环境里）。
        ''' </remarks>
        Public Sub Validate()
            If ParallelThreshold < 1 Then ParallelThreshold = 1
            If MaxDegreeOfParallelism < 0 Then MaxDegreeOfParallelism = 0
            If MemoryBudgetMB < 64 Then MemoryBudgetMB = 64
        End Sub

        ''' <summary>
        ''' 派生一个独立副本（修改副本不会影响原实例）。
        ''' </summary>
        Public Function Clone() As CloudyOptions
            Return New CloudyOptions With {
                .EnableParallel = EnableParallel,
                .ParallelThreshold = ParallelThreshold,
                .MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                .Seed = Seed,
                .MemoryBudgetMB = MemoryBudgetMB,
                .DiagnosticSink = DiagnosticSink
            }
        End Function

        ''' <summary>
        ''' 派生一个带偏移的随机数发生器（用于需要多次独立采样的算法）。
        ''' </summary>
        Public Function CreateRandom(Optional offset As Integer = 0) As Random
            Return New Random(Seed + offset)
        End Function

        ''' <summary>
        ''' 输出一条诊断信息（未设置 <see cref="DiagnosticSink"/> 时是空操作）。
        ''' </summary>
        Public Sub Report(message As String)
            Dim sink As Action(Of String) = DiagnosticSink

            If sink IsNot Nothing Then Call sink(message)
        End Sub

        ''' <summary>
        ''' 按“外层分块 + 块内串行”的策略执行区间任务。
        ''' </summary>
        ''' <param name="length">行数（点/面片/体素层）</param>
        ''' <param name="body">回调参数为半开区间 <c>[start, ends)</c></param>
        ''' <param name="minLength">
        ''' 触发并行的最小规模；<c>-1</c> 表示使用 <see cref="ParallelThreshold"/>。
        ''' </param>
        ''' <remarks>
        ''' <para>
        ''' 这是整个库唯一的并行入口。统一在这里分块有三个好处：
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description>不会出现嵌套并行（内层 SIMD 内核永远单线程），避免线程池风暴。</description></item>
        ''' <item><description>块远大于缓存行（数千行），避免伪共享与任务调度开销。</description></item>
        ''' <item><description>每个块只写自己的输出下标区间，天然无锁。</description></item>
        ''' </list>
        ''' <para>
        ''' 分块数取 <c>Workers × 4</c>：略多于线程数可以让 CPU 在负载不均时自动补齐
        ''' （点云在空间上分布不均，各块的实际工作量常有 2~3 倍的差异）。
        ''' </para>
        ''' </remarks>
        Public Sub ForRange(length As Integer, body As Action(Of Integer, Integer), Optional minLength As Integer = -1)
            If body Is Nothing Then Throw New ArgumentNullException(NameOf(body))

            If length <= 0 Then Return

            Dim threshold As Integer = If(minLength > 0, minLength, ParallelThreshold)

            If Not EnableParallel OrElse length < threshold * 2 Then
                Call body(0, length)

                Return
            End If

            Dim chunk As Integer = std.Max(threshold, length \ (Workers * 4))
            Dim chunks As Integer = (length + chunk - 1) \ chunk
            Dim options As New System.Threading.Tasks.ParallelOptions With {
                .MaxDegreeOfParallelism = Workers
            }

            System.Threading.Tasks.Parallel.For(0, chunks, options,
                Sub(c)
                    Dim start As Integer = c * chunk
                    Dim stopAt As Integer = std.Min(start + chunk, length)

                    If start < stopAt Then Call body(start, stopAt)
                End Sub)
        End Sub

        Public Overrides Function ToString() As String
            Return $"parallel={EnableParallel}, workers={Workers}, threshold={ParallelThreshold}, seed={Seed}, budget={MemoryBudgetMB}MB"
        End Function
    End Class
End Namespace
