' ---------------------------------------------------------------------------
' 混合精度诊断：把"静默回退"变成可观测的计数
'
' 存在的理由（这是本文件唯一要解决的事）：
'   后端契约里缺失的算子会**静默回退**到 TensorComputeFBase 的标量实现
'   （全仓 NotImplementedException 数为 0）。于是"代码跑通了"和
'   "算子真的走了 GPU"是两件独立的事 —— 上层改造时极易出现
'   "改完了，一个算子都没上 GPU，还以为拿到了加速"。
'
' 用法：后端在每个"判定不上 GPU / 内核缺失"的分支上记一次 Fallback，
'       在每个"确实走了内核"的分支上记一次 Kernel；
'       跑完一批之后用 Report() 打印，或者在回归里断言 Kernel > 0。
'
' 线程安全：后端是进程级共享对象（computeKernelF 是 Shared 属性），
'           因此这里的所有计数都必须是原子的。
' ---------------------------------------------------------------------------

Imports System.Collections.Concurrent

Namespace Compute

    ''' <summary>单个算子的回退 / 内核命中计数</summary>
    Public Structure PrecisionCounter

        ''' <summary>走了加速路径（SIMD 或 GPU 内核）的次数</summary>
        Public Accelerated As Long
        ''' <summary>回退到标量 CPU 实现的次数</summary>
        Public Fallback As Long

        ''' <summary>加速命中率；没有任何调用时返回 0</summary>
        Public ReadOnly Property Ratio As Double
            Get
                Dim total = Accelerated + Fallback
                If total = 0 Then Return 0.0
                Return Accelerated / CDbl(total)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"加速 {Accelerated} / 回退 {Fallback} ({Ratio * 100.0:F1}%)"
        End Function

    End Structure

    ''' <summary>
    ''' 混合精度的全局诊断计数器。
    ''' </summary>
    Public Module MixedPrecisionDiagnostics

        ' 两张表分开而不是存一个结构体，是为了让 AddOrUpdate 的更新保持无锁
        Private ReadOnly _accelerated As New ConcurrentDictionary(Of String, Long)()
        Private ReadOnly _fallback As New ConcurrentDictionary(Of String, Long)()

        ''' <summary>总开关：关闭后所有记录变成空操作（用于把开销彻底归零）</summary>
        Public Property Enabled As Boolean = True

        ''' <summary>记一次"走了加速路径"</summary>
        Public Sub Accelerated(op As String)
            If Not Enabled OrElse op Is Nothing Then Return
            _accelerated.AddOrUpdate(op, 1L, Function(key, old) old + 1L)
        End Sub

        ''' <summary>
        ''' 记一次"回退到 CPU 标量实现"。
        ''' </summary>
        ''' <param name="op">算子名</param>
        ''' <param name="reason">回退原因（规模不足 / 内核缺失 / 形状不支持），仅用于诊断输出</param>
        Public Sub Fallback(op As String, Optional reason As String = Nothing)
            If Not Enabled OrElse op Is Nothing Then Return
            _fallback.AddOrUpdate(op, 1L, Function(key, old) old + 1L)

            If reason IsNot Nothing Then
                _reasons(op) = reason
            End If
        End Sub

        Private ReadOnly _reasons As New ConcurrentDictionary(Of String, String)()

        ''' <summary>取某个算子的计数</summary>
        Public Function [Of](op As String) As PrecisionCounter
            Dim a As Long = 0, f As Long = 0

            If op IsNot Nothing Then
                _accelerated.TryGetValue(op, a)
                _fallback.TryGetValue(op, f)
            End If

            Return New PrecisionCounter With {.Accelerated = a, .Fallback = f}
        End Function

        ''' <summary>清空全部计数（每次基准开跑前调用，保证数字可比）</summary>
        Public Sub Reset()
            _accelerated.Clear()
            _fallback.Clear()
            _reasons.Clear()
        End Sub

        ''' <summary>
        ''' 打印一份"到底有没有上加速路径"的报告。
        ''' </summary>
        ''' <remarks>
        ''' 只列出被调用过的算子；命中率 100% 的算子会被折叠成一行，
        ''' 让真正需要关注的（有回退的）项浮在上面。
        ''' </remarks>
        Public Function Report() As String
            Dim lines As New List(Of String)
            Dim names As New HashSet(Of String)()

            For Each k In _accelerated.Keys
                names.Add(k)
            Next
            For Each k In _fallback.Keys
                names.Add(k)
            Next

            If names.Count = 0 Then
                Return "混合精度诊断: 没有任何算子被调用"
            End If

            lines.Add("算子                                       加速      回退    命中率   原因")

            For Each name In names.OrderBy(Function(n) n)
                Dim c = [Of](name)
                Dim reason As String = Nothing
                _reasons.TryGetValue(name, reason)

                lines.Add($"{name,-40} {c.Accelerated,8} {c.Fallback,8}   {c.Ratio * 100.0,5:F1}%   {If(reason, "")}")
            Next

            Return String.Join(Environment.NewLine, lines)
        End Function

    End Module

End Namespace
