' ------------------------------------------------------------------------
' 逐元素（elementwise）算子
'
' 每个算子都提供两种入口：
'   * 显存版：<c>Add(engine, a, b, out, stream)</c>  —— 直接操作 DeviceBuffer，零拷贝
'   * 主机版：<c>Add(engine, a(), b())</c>            —— 内部完成上传/回读，用起来最省事
' ------------------------------------------------------------------------

Imports Enigma.ILCuda.Runtime

Namespace Math

    ''' <summary>单精度向量的逐元素算子</summary>
    Public Module GpuElementwise

        ''' <summary>默认的一维 block 尺寸</summary>
        Public Const DefaultBlockSize As Integer = 256

        ' ------------------------------------------------------------------
        ' 二元算子：out = a OP b
        ' ------------------------------------------------------------------

        ''' <summary>out = a + b</summary>
        Public Sub Add(engine As CudaEngine, a As DeviceBuffer(Of Single), b As DeviceBuffer(Of Single),
                       out As DeviceBuffer(Of Single), Optional stream As CudaStream = Nothing)
            Binary(engine, KernelNames.EwAdd, a, b, out, stream)
        End Sub

        ''' <summary>out = a - b</summary>
        Public Sub Subtract(engine As CudaEngine, a As DeviceBuffer(Of Single), b As DeviceBuffer(Of Single),
                            out As DeviceBuffer(Of Single), Optional stream As CudaStream = Nothing)
            Binary(engine, KernelNames.EwSub, a, b, out, stream)
        End Sub

        ''' <summary>out = a * b</summary>
        Public Sub Multiply(engine As CudaEngine, a As DeviceBuffer(Of Single), b As DeviceBuffer(Of Single),
                            out As DeviceBuffer(Of Single), Optional stream As CudaStream = Nothing)
            Binary(engine, KernelNames.EwMul, a, b, out, stream)
        End Sub

        ''' <summary>out = a / b（分母为 0 的位置结果写 0）</summary>
        Public Sub Divide(engine As CudaEngine, a As DeviceBuffer(Of Single), b As DeviceBuffer(Of Single),
                          out As DeviceBuffer(Of Single), Optional stream As CudaStream = Nothing)
            Binary(engine, KernelNames.EwDiv, a, b, out, stream)
        End Sub

        Private Sub Binary(engine As CudaEngine, kernelName As String,
                           a As DeviceBuffer(Of Single), b As DeviceBuffer(Of Single),
                           out As DeviceBuffer(Of Single), stream As CudaStream)
            ValidateEngine(engine)
            RequireSameLength(NameOf(a), a, NameOf(b), b)
            RequireSameLength(NameOf(a), a, NameOf(out), out)

            Dim config = LaunchPlanner.For1D(a.Count, DefaultBlockSize)

            engine.GetKernel(kernelName).Launch(stream, config, a, b, out, a.Count)
        End Sub

        ' ------------------------------------------------------------------
        ' 带标量的算子
        ' ------------------------------------------------------------------

        ''' <summary>out = alpha * x</summary>
        Public Sub Scale(engine As CudaEngine, x As DeviceBuffer(Of Single), alpha As Single,
                         out As DeviceBuffer(Of Single), Optional stream As CudaStream = Nothing)
            ValidateEngine(engine)
            RequireSameLength(NameOf(x), x, NameOf(out), out)

            Dim config = LaunchPlanner.For1D(x.Count, DefaultBlockSize)

            engine.GetKernel(KernelNames.EwScale).Launch(stream, config, x, alpha, out, x.Count)
        End Sub

        ''' <summary>out = a + alpha * b</summary>
        Public Sub Axpy(engine As CudaEngine, a As DeviceBuffer(Of Single), b As DeviceBuffer(Of Single),
                        alpha As Single, out As DeviceBuffer(Of Single),
                        Optional stream As CudaStream = Nothing)
            ValidateEngine(engine)
            RequireSameLength(NameOf(a), a, NameOf(b), b)
            RequireSameLength(NameOf(a), a, NameOf(out), out)

            Dim config = LaunchPlanner.For1D(a.Count, DefaultBlockSize)

            engine.GetKernel(KernelNames.EwAxpy).Launch(stream, config, a, b, alpha, out, a.Count)
        End Sub

        ' ------------------------------------------------------------------
        ' 一元算子：out = f(x)
        ' ------------------------------------------------------------------

        ''' <summary>out = max(x, 0)</summary>
        Public Sub Relu(engine As CudaEngine, x As DeviceBuffer(Of Single), out As DeviceBuffer(Of Single),
                        Optional stream As CudaStream = Nothing)
            Unary(engine, KernelNames.EwRelu, x, out, stream)
        End Sub

        ''' <summary>out = exp(x)</summary>
        Public Sub Exp(engine As CudaEngine, x As DeviceBuffer(Of Single), out As DeviceBuffer(Of Single),
                       Optional stream As CudaStream = Nothing)
            Unary(engine, KernelNames.EwExp, x, out, stream)
        End Sub

        ''' <summary>out = ln(x)（x <= 0 时结果无意义，内核不做保护）</summary>
        Public Sub Log(engine As CudaEngine, x As DeviceBuffer(Of Single), out As DeviceBuffer(Of Single),
                       Optional stream As CudaStream = Nothing)
            Unary(engine, KernelNames.EwLog, x, out, stream)
        End Sub

        ''' <summary>out = sqrt(max(x, 0))</summary>
        Public Sub Sqrt(engine As CudaEngine, x As DeviceBuffer(Of Single), out As DeviceBuffer(Of Single),
                        Optional stream As CudaStream = Nothing)
            Unary(engine, KernelNames.EwSqrt, x, out, stream)
        End Sub

        ''' <summary>out = |x|</summary>
        Public Sub Abs(engine As CudaEngine, x As DeviceBuffer(Of Single), out As DeviceBuffer(Of Single),
                       Optional stream As CudaStream = Nothing)
            Unary(engine, KernelNames.EwAbs, x, out, stream)
        End Sub

        Private Sub Unary(engine As CudaEngine, kernelName As String,
                          x As DeviceBuffer(Of Single), out As DeviceBuffer(Of Single), stream As CudaStream)
            ValidateEngine(engine)
            RequireSameLength(NameOf(x), x, NameOf(out), out)

            Dim config = LaunchPlanner.For1D(x.Count, DefaultBlockSize)

            engine.GetKernel(kernelName).Launch(stream, config, x, out, x.Count)
        End Sub

        ' ------------------------------------------------------------------
        ' 主机数组便捷入口（内部完成上传 / 回读）
        ' ------------------------------------------------------------------

        ''' <summary>主机版：out = a + b</summary>
        Public Function Add(engine As CudaEngine, a As Single(), b As Single()) As Single()
            Return Run(engine, KernelNames.EwAdd, a, b)
        End Function

        ''' <summary>主机版：out = a - b</summary>
        Public Function Subtract(engine As CudaEngine, a As Single(), b As Single()) As Single()
            Return Run(engine, KernelNames.EwSub, a, b)
        End Function

        ''' <summary>主机版：out = a * b</summary>
        Public Function Multiply(engine As CudaEngine, a As Single(), b As Single()) As Single()
            Return Run(engine, KernelNames.EwMul, a, b)
        End Function

        ''' <summary>主机版：out = a / b</summary>
        Public Function Divide(engine As CudaEngine, a As Single(), b As Single()) As Single()
            Return Run(engine, KernelNames.EwDiv, a, b)
        End Function

        ''' <summary>主机版：out = f(x)</summary>
        Public Function Apply(engine As CudaEngine, op As UnaryOp, x As Single()) As Single()
            ValidateEngine(engine)
            If x Is Nothing OrElse x.Length = 0 Then Return Array.Empty(Of Single)()

            Dim kernelName = KernelOf(op)

            Using deviceX As New DeviceBuffer(Of Single)(x.Length),
                  deviceOut As New DeviceBuffer(Of Single)(x.Length)

                deviceX.Write(x)

                Dim config = LaunchPlanner.For1D(x.Length, DefaultBlockSize)
                engine.GetKernel(kernelName).Launch(config, deviceX, deviceOut, x.Length)

                Return deviceOut.Read()
            End Using
        End Function

        Private Function Run(engine As CudaEngine, kernelName As String, a As Single(), b As Single()) As Single()
            ValidateEngine(engine)

            If a Is Nothing OrElse b Is Nothing Then Throw New ArgumentNullException("输入数组不能为空")
            If a.Length <> b.Length Then Throw New ArgumentException("两个输入数组长度不一致")
            If a.Length = 0 Then Return Array.Empty(Of Single)()

            Using deviceA As New DeviceBuffer(Of Single)(a.Length),
                  deviceB As New DeviceBuffer(Of Single)(b.Length),
                  deviceOut As New DeviceBuffer(Of Single)(a.Length)

                deviceA.Write(a)
                deviceB.Write(b)

                Dim config = LaunchPlanner.For1D(a.Length, DefaultBlockSize)
                engine.GetKernel(kernelName).Launch(config, deviceA, deviceB, deviceOut, a.Length)

                Return deviceOut.Read()
            End Using
        End Function

        ' ------------------------------------------------------------------
        ' 校验
        ' ------------------------------------------------------------------

        Private Sub ValidateEngine(engine As CudaEngine)
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))
        End Sub

        Private Sub RequireSameLength(leftName As String, left As DeviceBuffer(Of Single),
                                      rightName As String, right As DeviceBuffer(Of Single))
            If left Is Nothing Then Throw New ArgumentNullException(leftName)
            If right Is Nothing Then Throw New ArgumentNullException(rightName)

            If left.Count <> right.Count Then
                Throw New ArgumentException(
                    $"{leftName} 长度 {left.Count} 与 {rightName} 长度 {right.Count} 不一致")
            End If
        End Sub

        Private Function KernelOf(op As UnaryOp) As String
            Select Case op
                Case UnaryOp.Relu
                    Return KernelNames.EwRelu
                Case UnaryOp.Exp
                    Return KernelNames.EwExp
                Case UnaryOp.Log
                    Return KernelNames.EwLog
                Case UnaryOp.Sqrt
                    Return KernelNames.EwSqrt
                Case Else
                    Return KernelNames.EwAbs
            End Select
        End Function
    End Module

    ''' <summary>一元算子的种类（供主机版 Apply 使用）</summary>
    Public Enum UnaryOp
        Relu
        Exp
        Log
        Sqrt
        Abs
    End Enum
End Namespace
