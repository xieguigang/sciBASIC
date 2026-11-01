Imports Microsoft.VisualBasic.Math.SIMD
Imports sysmath = System.Math

Namespace train

    ''' <summary>
    ''' SIMD 优化策略开关与阈值。
    '''
    ''' 所有「是否走 SIMD / 是否并行」的判定都收敛到这里，方便在服务器上按 NUMA 与核数调优，
    ''' 也方便在回归里一键退回纯标量实现做数值对照。
    ''' </summary>
    ''' <remarks>
    ''' 环境变量：<c>XGB_SIMD=0</c> 可全局关闭 SIMD（等价于 <see cref="UseSimd"/> = False），
    ''' <c>XGB_SIMD_FASTEXP=0</c> 可关闭多项式近似 exp（改用 <see cref="System.Math.Exp(Double)"/>）。
    ''' </remarks>
    Public NotInheritable Class SimdOptions

        Private Shared _useSimd As Boolean = readSwitch("XGB_SIMD", True)
        Private Shared _fastExp As Boolean = readSwitch("XGB_SIMD_FASTEXP", True)
        Private Shared _minVectorLength As Integer = 8
        Private Shared _minParallelLength As Integer = 4096

        Private Sub New()
        End Sub

        Private Shared Function readSwitch(name As String, defaultValue As Boolean) As Boolean
            Dim text As String = Environment.GetEnvironmentVariable(name)

            If String.IsNullOrEmpty(text) Then
                Return defaultValue
            End If

            Select Case text.Trim().ToLowerInvariant()
                Case "0", "false", "off", "no"
                    Return False
                Case Else
                    Return True
            End Select
        End Function

        ''' <summary>
        ''' 是否允许使用 SIMD 与并行算子（默认 True）。
        '''
        ''' 关闭后所有热循环退回与改造前数值一致的标量实现。
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property UseSimd As Boolean
            Get
                Return _useSimd
            End Get
            Set(value As Boolean)
                _useSimd = value
            End Set
        End Property

        ''' <summary>
        ''' 是否允许使用多项式近似的 <c>exp</c>（<see cref="SimdMath.ExpInPlace(Double(), Integer)"/>）。
        '''
        ''' 关闭后 sigmoid 仍走向量化的加减乘，但指数部分使用 <see cref="System.Math.Exp(Double)"/>。
        ''' 精度门槛见测试 <c>SimdPrecision</c>。
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property FastExp As Boolean
            Get
                Return _fastExp
            End Get
            Set(value As Boolean)
                _fastExp = value
            End Set
        End Property

        ''' <summary>
        ''' 低于该长度直接走标量：小数组的向量化装载/尾部处理开销大于收益。
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property MinVectorLength As Integer
            Get
                Return _minVectorLength
            End Get
            Set(value As Integer)
                _minVectorLength = sysmath.Max(value, 1)
            End Set
        End Property

        ''' <summary>
        ''' 低于该长度不并行（与 <see cref="SimdParallel"/> 的 <c>minLength</c> 对齐）。
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property MinParallelLength As Integer
            Get
                Return _minParallelLength
            End Get
            Set(value As Integer)
                _minParallelLength = sysmath.Max(value, 1)
            End Set
        End Property

        ''' <summary>
        ''' SIMD 是否真正生效（开关打开且硬件/运行时支持）。
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property IsActive As Boolean
            Get
                Return _useSimd AndAlso SIMDEnvironment.IsEnabled
            End Get
        End Property

        ''' <summary>
        ''' 是否启用多项式近似 exp（要求 SIMD 生效）。
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property UseFastExp As Boolean
            Get
                Return IsActive AndAlso _fastExp
            End Get
        End Property

        ''' <summary>
        ''' 归一化线程数：<c>&lt;= 0</c> 表示取 <see cref="Environment.ProcessorCount"/>。
        ''' </summary>
        ''' <param name="numThread"></param>
        ''' <returns></returns>
        Public Shared Function Parallelism(numThread As Integer) As Integer
            If numThread > 0 Then
                Return numThread
            End If

            Return sysmath.Max(Environment.ProcessorCount, 1)
        End Function

        ''' <summary>
        ''' 一行可观测描述，用于训练日志确认优化是否真的启用。
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function Describe() As String
            Dim width As Integer = System.Numerics.Vector(Of Double).Count
            Dim state As String = If(IsActive, "SIMD=on", "SIMD=off")

            If IsActive Then
                state &= If(SIMDEnvironment.IsFmaSupported, ",fma=on", ",fma=off")
                state &= If(SIMDEnvironment.IsAvx2Supported, ",avx2=on", ",avx2=off")
                state &= $",vecWidth={width}"
            End If

            state &= If(UseFastExp, ",fastExp=on", ",fastExp=off")

            Return $"{state},minVec={_minVectorLength},minParallel={_minParallelLength},cpus={Environment.ProcessorCount}"
        End Function
    End Class
End Namespace
