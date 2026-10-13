' ---------------------------------------------------------------------------
' 混合精度策略：把"什么规模下单精度 + GPU 才划算"集中在一处
'
' 为什么要有这个模块：
'   如果每个改造点各自判断"这个矩阵够不够大"，阈值就会散落在四个项目里，
'   调参时改一处漏三处，而且没人说得清"为什么这里选了 FP32"。
'   这里把**规模估算**与**决策**收成两个纯函数，上层只问结果。
'
' 决策的两个输入：
'   1. 算术强度 —— GEMM 是 O(m·k·n) 而访存是 O(m·k + k·n)，规模越大越划算；
'      池化几乎不做浮点运算，是纯带宽受限，规模要求要高一档。
'   2. 数值容忍度 —— 见 PrecisionClass。这一条与规模无关，是**语义**属性：
'      归一化统计量 / ODE 积分 / 代理梯度无论如何都不能降精度。
'
' 阈值本身是"经验起点"而不是真理：本文件里的常量应当用 MixedPrecisionBench
' 的 -bench 模式在目标机器上量一遍之后再定。
' ---------------------------------------------------------------------------

Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' 算子的数值敏感度分类。
    ''' </summary>
    ''' <remarks>
    ''' 这是混合精度里唯一"不可协商"的维度：它描述的是**语义**而不是性能。
    ''' 标成 MustDouble 的算子，即便规模再大也不允许走单精度。
    ''' </remarks>
    Public Enum PrecisionClass

        ''' <summary>算力密集、数值容忍度高：GEMM / Conv / SpMM</summary>
        ComputeBound = 0
        ''' <summary>带宽受限、收益主要来自并行与向量化：打包 / 重排 / 逐元素</summary>
        MemoryBound = 1
        ''' <summary>数值敏感，必须保持双精度：归一化统计量 / ODE / 代理梯度 / 损失 / 优化器</summary>
        MustDouble = 2
        ''' <summary>未分类：按规模启发式决定</summary>
        Unclassified = 3

    End Enum

    ''' <summary>一次混合精度决策的结果</summary>
    Public Structure PrecisionDecision

        ''' <summary>是否建议走单精度路径</summary>
        Public UseSingle As Boolean
        ''' <summary>决策依据（用于诊断输出，避免出现"说不清为什么"的改造）</summary>
        Public Reason As String

        Public Sub New(useSingle As Boolean, reason As String)
            Me.UseSingle = useSingle
            Me.Reason = reason
        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(UseSingle, "Single", "Double")}（{Reason}）"
        End Function

    End Structure

    ''' <summary>
    ''' 混合精度的集中策略。
    ''' </summary>
    Public Module MixedPrecisionPolicy

        ' ---------------- 规模阈值（经验起点，需用 -bench 在目标机器上校准）----------------

        ''' <summary>GEMM 的 m·k·n 达到该值时才建议走单精度加速路径</summary>
        Public Property GemmOpsThreshold As Long = 262144L
        ''' <summary>卷积的 N·OH·OW·OutC·KH·KW·C 达到该值时才建议上加速路径</summary>
        Public Property ConvOpsThreshold As Long = 1048576L
        ''' <summary>CSR 的非零元个数达到该值时才建议走加速 SpMM</summary>
        Public Property SpmmNnzThreshold As Long = 65536L
        ''' <summary>逐元素算子达到该元素数时才值得搬 GPU（低于此值往返开销倒挂）</summary>
        Public Property ElementwiseThreshold As Integer = 65536

        ''' <summary>
        ''' 总开关。关闭后所有决策一律返回 <c>Double</c>，
        ''' 用于"改完了但效果不达预期"时的整体回退。
        ''' </summary>
        Public Property Enabled As Boolean = True

        ' ---------------- 规模估算 ----------------

        ''' <summary>GEMM 的浮点运算量 m·k·n（每个输出元素 k 次乘加）</summary>
        Public Function GemmOps(m As Integer, k As Integer, n As Integer) As Long
            Return CLng(m) * CLng(k) * CLng(n)
        End Function

        ''' <summary>卷积的浮点运算量（与输出尺寸 × 感受野成正比）</summary>
        Public Function ConvOps(batch As Integer, outH As Integer, outW As Integer,
                                outC As Integer, kernelH As Integer, kernelW As Integer,
                                inC As Integer) As Long
            Return CLng(batch) * outH * outW * outC * kernelH * kernelW * inC
        End Function

        ' ---------------- 决策 ----------------

        ''' <summary>
        ''' 按"算子类别 + 规模"给出精度建议。
        ''' </summary>
        ''' <param name="category">算子的数值敏感度分类</param>
        ''' <param name="ops">规模估算值（GEMM 用 m·k·n，卷积用 ConvOps，其余用元素数）</param>
        ''' <param name="opName">算子名，用于诊断计数</param>
        Public Function Decide(category As PrecisionClass, ops As Long, Optional opName As String = Nothing) As PrecisionDecision
            ' 数值敏感类没有商量余地：规模再大也不能降精度
            If category = PrecisionClass.MustDouble Then
                Call Note(opName, False, "数值敏感类")
                Return New PrecisionDecision(False, "该算子属于数值敏感类，必须保持双精度")
            End If

            If Not Enabled Then
                Call Note(opName, False, "总开关关闭")
                Return New PrecisionDecision(False, "混合精度总开关已关闭")
            End If

            Dim threshold As Long

            Select Case category
                Case PrecisionClass.ComputeBound
                    threshold = GemmOpsThreshold
                Case PrecisionClass.MemoryBound
                    threshold = ElementwiseThreshold
                Case Else
                    threshold = ElementwiseThreshold
            End Select

            Dim useSingle = ops >= threshold

            Call Note(opName, useSingle, If(useSingle, "规模达标", $"规模 {ops} < 阈值 {threshold}"))

            Return New PrecisionDecision(useSingle,
                If(useSingle,
                   $"规模 {ops} 达到阈值 {threshold}",
                   $"规模 {ops} 低于阈值 {threshold}，降精度不划算"))
        End Function

        ''' <summary>把决策记进诊断计数器（把"静默回退"变成可观测事件）</summary>
        Private Sub Note(opName As String, accelerated As Boolean, reason As String)
            If opName Is Nothing Then Return

            If accelerated Then
                MixedPrecisionDiagnostics.Accelerated(opName)
            Else
                MixedPrecisionDiagnostics.Fallback(opName, reason)
            End If
        End Sub

        ''' <summary>
        ''' 双精度 -> 单精度的受控转换：非有限值一律保留而不是变成 NaN/Inf 的静默传播源。
        ''' </summary>
        ''' <remarks>
        ''' 单精度下 <c>Double</c> 的大值会溢出成 <c>Infinity</c>，
        ''' 而 Inf 一旦进入动量/二阶矩就会永久粘住（见 train.cu 的自愈逻辑）。
        ''' 与其等它在下游变 NaN，不如在这里按 ±Single.MaxValue 夹住并计数。
        ''' </remarks>
        Public Function ToSingleChecked(value As Double) As Single
            If Double.IsNaN(value) Then Return 0.0F

            If value > Single.MaxValue Then
                MixedPrecisionDiagnostics.Fallback("ToSingle", "上溢夹断")
                Return Single.MaxValue
            End If

            If value < Single.MinValue Then
                MixedPrecisionDiagnostics.Fallback("ToSingle", "下溢夹断")
                Return Single.MinValue
            End If

            Return CSng(value)
        End Function

        ''' <summary>
        ''' 一个张量是否可以安全降到单精度：存在非有限值时返回 <c>False</c>。
        ''' </summary>
        Public Function IsSafeForSingle(values As Double()) As Boolean
            If values Is Nothing Then Return False

            For Each v In values
                If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then Return False
                If v > Single.MaxValue OrElse v < Single.MinValue Then Return False
            Next

            Return True
        End Function

    End Module

End Namespace
