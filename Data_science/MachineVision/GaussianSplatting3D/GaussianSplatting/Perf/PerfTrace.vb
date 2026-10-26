' ---------------------------------------------------------------------------
' 分阶段性能计时
'
' 只在 PerfTrace.Enabled = True 时记录。计时代码本身用 timeStamp 差值实现，
' 不构造 Stopwatch 对象，因此在默认（关闭）状态下只剩一次布尔判断，
' 不会污染热路径的分支预测。
'
' 为什么要分阶段：高斯溅射的耗时分布会随高斯数量、屏幕分辨率、可见高斯
' 比例剧烈变化（投影 / 分箱 / 光栅化 / 反向 / 优化器），只给一个"总耗时"
' 无法判断下一步该优化谁。
' ---------------------------------------------------------------------------

Imports System.Text

''' <summary>
''' 计时的阶段划分。
''' </summary>
Public Enum PerfPhase

        ''' <summary>逐高斯屏幕投影（相机变换 / 协方差 / conic / sigmoid）</summary>
        Projection = 0
        ''' <summary>可见性剔除与深度排序</summary>
        Culling = 1
        ''' <summary>tile 分箱（计数 / 前缀和 / 填充）</summary>
        Binning = 2
        ''' <summary>前向像素合成</summary>
        Rasterize = 3
        ''' <summary>解析反向传播</summary>
        Backward = 4
        ''' <summary>损失计算</summary>
        Loss = 5
        ''' <summary>Adam 参数更新</summary>
        Optimize = 6
        ''' <summary>自适应密度控制</summary>
        Densify = 7
        ''' <summary>模型 fp32 镜像同步</summary>
        Sync = 8

    End Enum

    ''' <summary>
    ''' 进程级的分阶段计时器（线程安全累计）。
    ''' </summary>
    Public NotInheritable Class PerfTrace

        Private Const PhaseCount As Integer = 9

        Private Shared ReadOnly _ticks As Long() = New Long(PhaseCount - 1) {}
        Private Shared ReadOnly _calls As Integer() = New Integer(PhaseCount - 1) {}

        ''' <summary>总开关。关闭时所有测量方法直接返回、不做任何记录。</summary>
        Public Shared Property Enabled As Boolean = False

        ''' <summary>开始一次测量。仅在 <see cref="Enabled"/> 为 True 时需要（否则返回 0）。</summary>
        Public Shared Function Begin() As Long
            Return If(Enabled, System.Diagnostics.Stopwatch.GetTimestamp(), 0L)
        End Function

        ''' <summary>结束一次测量并累计到指定阶段。</summary>
        Public Shared Sub Done(phase As PerfPhase, start As Long)
            If Not Enabled OrElse start = 0 Then Return

            Dim elapsed = System.Diagnostics.Stopwatch.GetTimestamp() - start
            System.Threading.Interlocked.Add(_ticks(CInt(phase)), elapsed)
            System.Threading.Interlocked.Increment(_calls(CInt(phase)))
        End Sub

        ''' <summary>测量一个有返回值的操作。</summary>
        Public Shared Function Time(Of T)(phase As PerfPhase, body As Func(Of T)) As T
            If Not Enabled Then Return body()

            Dim t0 = System.Diagnostics.Stopwatch.GetTimestamp()
            Try
                Return body()
            Finally
                Done(phase, t0)
            End Try
        End Function

        ''' <summary>测量一个无返回值的操作。</summary>
        Public Shared Sub Time(phase As PerfPhase, body As Action)
            If Not Enabled Then
                body()
                Return
            End If

            Dim t0 = System.Diagnostics.Stopwatch.GetTimestamp()
            Try
                body()
            Finally
                Done(phase, t0)
            End Try
        End Sub

        Public Shared Sub Reset()
            Array.Clear(_ticks, 0, _ticks.Length)
            Array.Clear(_calls, 0, _calls.Length)
        End Sub

        ''' <summary>某阶段的累计耗时（毫秒）。</summary>
        Public Shared Function ElapsedMs(phase As PerfPhase) As Double
            Return _ticks(CInt(phase)) * 1000.0 / System.Diagnostics.Stopwatch.Frequency
        End Function

        ''' <summary>某阶段被记录的次数。</summary>
        Public Shared Function Calls(phase As PerfPhase) As Integer
            Return _calls(CInt(phase))
        End Function

        ''' <summary>格式化的分阶段报告（用于 bench 输出）。</summary>
        Public Shared Function Report() As String
            Dim sb As New StringBuilder()
            Dim total = 0.0

            For i = 0 To PhaseCount - 1
                total += ElapsedMs(CType(i, PerfPhase))
            Next

            sb.AppendLine($"{"phase",-14}{"ms",12}{"calls",10}{"share",9}")

            For i = 0 To PhaseCount - 1
                Dim ms = ElapsedMs(CType(i, PerfPhase))
                Dim share = If(total > 0, ms / total, 0)

                sb.AppendLine($"{CType(i, PerfPhase).ToString().ToLowerInvariant(),-14}" &
                              $"{ms,12:F3}{_calls(i),10}{share,9:P1}")
            Next

            sb.AppendLine($"{"total",-14}{total,12:F3}")

            Return sb.ToString()
        End Function

End Class
