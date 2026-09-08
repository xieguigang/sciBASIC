#include "Microsoft.VisualBasic.Computing.ILCuda.dll"
#include "Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.dll"

imports System.Reflection
imports System.Text
imports System.Text.RegularExpressions
imports Microsoft.VisualBasic.Computing.ILCuda.Runtime
imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL

' ============================================================================
'  IL -> CUDA 教程: 用脚本里定义的 VB.NET 数学函数, 在 GPU 上算矩阵行间的
'  皮尔逊相关系数矩阵 与 欧氏距离矩阵
'
'  运行:
'      vbs.exe tutorials\VBS\cuda.vb
'
'  流水线:
'      VB.NET 函数 -> IL 字节码 -> AST(MethodSyntax) -> .cu 源码
'                 -> NVRTC 即时编译 -> cuLaunchKernel -> 结果矩阵
'
'  三条内核映射约定(详见 cuda\ILCuda\README.md):
'      1) 形参里有 i        -> 一维内核, i = blockIdx.x * blockDim.x + threadIdx.x
'      2) 形参里有 i 和 j   -> 二维内核, i 取行(blockIdx.y), j 取列(blockIdx.x)
'      3) 纯标量, 不做数组取元素 -> 逐元素自动包裹, 每个标量参数都变成按下标读取的数组
'
'  注意: 由于 VBS 引擎会把顶层的 Function 重写为匿名函数, 拿不到 Shared 的
'  MethodInfo, 所以待反编译的目标函数一律写进下面的 Public Class 里 —— 类型块
'  会被原样搬到生成的 Module 中, 可以正常反射。
' ============================================================================

' ---------------------------------------------------------------------------
'  第 1 节: 待反编译的目标函数
'
'  下面全部是普通的 VB.NET 写法, 不含任何 CUDA 概念。IL2Cuda 会在运行时把它们
'  反编译成表达式树, 再发射成 .cu。
'
'  设第 i 行 sum_i = Σx, sq_i = Σx², dot_ij = Σx_ik·x_jk, n = 列数:
'      mean_i  = sum_i / n
'      var_i   = sq_i - n * mean_i²
'      corr_ij = (dot_ij - n * mean_i * mean_j) / sqrt(var_i * var_j)
'      dist_ij = sqrt(sq_i + sq_j - 2 * dot_ij)
'
'  对角线在数学上恒为 corr=1 / dist=0, 这里直接返回, 避免单精度下两个相近
'  大数相减把舍入误差放大(见 README 的数值注意点)。
' ---------------------------------------------------------------------------

Public Class PearsonMetrics

    ' 行求和, 对应约定 1: 形参 i 为线程索引 -> 一维内核
    Public Shared Function RowSum(x As Single(), cols As Integer, i As Integer) As Single
        Dim sum As Single = 0.0F

        For k As Integer = 0 To cols - 1
            sum += x(i * cols + k)
        Next

        Return sum
    End Function

    ' 行平方和, 同样是一维内核
    Public Shared Function RowSumSq(x As Single(), cols As Integer, i As Integer) As Single
        Dim sumSq As Single = 0.0F

        For k As Integer = 0 To cols - 1
            Dim v As Single = x(i * cols + k)
            sumSq += v * v
        Next

        Return sumSq
    End Function

    ' 两行的点积, 对应约定 2: 同时有 i 和 j -> 二维内核
    Public Shared Function GramDot(x As Single(), cols As Integer, i As Integer, j As Integer) As Single
        Dim acc As Single = 0.0F

        For k As Integer = 0 To cols - 1
            acc += x(i * cols + k) * x(j * cols + k)
        Next

        Return acc
    End Function

    ' 皮尔逊相关单元: guard clause + if/else 菱形 + MathF 调用
    Public Shared Function CorrelationCell(dot As Single(), rowSum As Single(), rowSumSq As Single(),
                                           rows As Integer, cols As Integer,
                                           i As Integer, j As Integer) As Single
        If i = j Then
            Return 1.0F
        End If

        Dim n As Single = CSng(cols)
        Dim meanI As Single = rowSum(i) / n
        Dim meanJ As Single = rowSum(j) / n
        Dim varI As Single = rowSumSq(i) - n * meanI * meanI
        Dim varJ As Single = rowSumSq(j) - n * meanJ * meanJ
        Dim d As Single = dot(i * rows + j)
        Dim cov As Single = d - n * meanI * meanJ
        Dim denom As Single = MathF.Sqrt(MathF.Max(varI, 0.0F)) * MathF.Sqrt(MathF.Max(varJ, 0.0F))
        Dim c As Single

        If denom > 1.0E-12F Then
            c = cov / denom
        Else
            c = 0.0F
        End If

        Return MathF.Min(1.0F, MathF.Max(-1.0F, c))
    End Function

    ' 欧氏距离单元: guard clause + MathF.Sqrt
    Public Shared Function DistanceCell(dot As Single(), rowSumSq As Single(),
                                        rows As Integer, i As Integer, j As Integer) As Single
        If i = j Then
            Return 0.0F
        End If

        Dim d As Single = dot(i * rows + j)
        Dim d2 As Single = MathF.Max(rowSumSq(i) + rowSumSq(j) - 2.0F * d, 0.0F)

        Return MathF.Sqrt(d2)
    End Function

    ' 纯标量截断, 对应约定 3: 没有 i/j, 也没有数组取元素 -> 逐元素自动包裹
    Public Shared Function PearsonClamp(cov As Single, denom As Single) As Single
        Dim c As Single

        If denom > 1.0E-12F Then
            c = cov / denom
        Else
            c = 0.0F
        End If

        Return MathF.Min(1.0F, MathF.Max(-1.0F, c))
    End Function
End Class

' ---------------------------------------------------------------------------
'  第 2 节: 教程用的辅助代码(目标方法清单 / CPU 参考实现 / 排版 / 自检)
' ---------------------------------------------------------------------------

Public Class TutorialKit

    ''' 全部待反编译的目标方法
    Public Shared Function Targets() As MethodInfo()
        Dim t As Type = GetType(PearsonMetrics)

        Return New MethodInfo() {
            t.GetMethod("RowSum"),
            t.GetMethod("RowSumSq"),
            t.GetMethod("GramDot"),
            t.GetMethod("CorrelationCell"),
            t.GetMethod("DistanceCell"),
            t.GetMethod("PearsonClamp")
        }
    End Function

    ''' 造一个可复现的随机矩阵(行主序)
    Public Shared Function MakeMatrix(rows As Integer, cols As Integer, seed As Integer) As Single()
        Dim rnd As New Random(seed)
        Dim data(rows * cols - 1) As Single

        For i As Integer = 0 To data.Length - 1
            data(i) = CSng(rnd.NextDouble() * 2.0 - 1.0)
        Next

        Return data
    End Function

    ''' 为解释求值自检造一组下标合法、数值不退化的样例参数
    Public Shared Function SampleArgs(m As MethodInfo) As Object()
        Dim ps As ParameterInfo() = m.GetParameters()
        Dim args(ps.Length - 1) As Object

        For i As Integer = 0 To ps.Length - 1
            Dim p As ParameterInfo = ps(i)
            Dim lower As String = p.Name.ToLowerInvariant()

            If p.ParameterType.IsArray Then
                Dim data(255) As Single

                For k As Integer = 0 To 255
                    data(k) = CSng((k Mod 17) - 8) * 0.5F
                Next

                args(i) = data
            ElseIf p.ParameterType = GetType(Single) Then
                args(i) = 1.5F
            ElseIf p.ParameterType = GetType(Integer) Then
                Select Case lower
                    Case "cols" : args(i) = 8
                    Case "rows" : args(i) = 16
                    Case "i" : args(i) = 2
                    Case "j" : args(i) = 5
                    Case Else : args(i) = 3
                End Select
            End If
        Next

        Return args
    End Function

    ''' CPU 参考实现: 皮尔逊相关矩阵(双精度两遍算法)
    Public Shared Function CpuCorrelation(data As Single(), rows As Integer, cols As Integer) As Double()
        Dim n As Double = cols
        Dim mean(rows - 1) As Double
        Dim sd(rows - 1) As Double

        For i As Integer = 0 To rows - 1
            Dim s As Double = 0
            Dim sq As Double = 0

            For k As Integer = 0 To cols - 1
                Dim v As Double = data(i * cols + k)
                s += v
                sq += v * v
            Next

            mean(i) = s / n
            sd(i) = System.Math.Sqrt(System.Math.Max(sq - n * mean(i) * mean(i), 0.0))
        Next

        Dim out(rows * rows - 1) As Double

        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To rows - 1
                Dim d As Double = 0

                For k As Integer = 0 To cols - 1
                    d += data(i * cols + k) * data(j * cols + k)
                Next

                Dim cov As Double = d - n * mean(i) * mean(j)
                Dim den As Double = sd(i) * sd(j)
                Dim c As Double = If(den > 0.0, cov / den, 0.0)

                out(i * rows + j) = If(i = j, 1.0, System.Math.Min(1.0, System.Math.Max(-1.0, c)))
            Next
        Next

        Return out
    End Function

    ''' CPU 参考实现: 欧氏距离矩阵
    Public Shared Function CpuDistance(data As Single(), rows As Integer, cols As Integer) As Double()
        Dim sq(rows - 1) As Double

        For i As Integer = 0 To rows - 1
            Dim s As Double = 0

            For k As Integer = 0 To cols - 1
                Dim v As Double = data(i * cols + k)
                s += v * v
            Next

            sq(i) = s
        Next

        Dim out(rows * rows - 1) As Double

        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To rows - 1
                Dim d As Double = 0

                For k As Integer = 0 To cols - 1
                    d += data(i * cols + k) * data(j * cols + k)
                Next

                out(i * rows + j) = If(i = j, 0.0,
                    System.Math.Sqrt(System.Math.Max(sq(i) + sq(j) - 2.0 * d, 0.0)))
            Next
        Next

        Return out
    End Function

    ''' GPU 结果与 CPU 参考值之间的最大绝对误差
    Public Shared Function MaxError(got As Single(), expect As Double()) As Double
        Dim maxDiff As Double = 0

        For i As Integer = 0 To got.Length - 1
            Dim diff As Double = System.Math.Abs(CDbl(got(i)) - expect(i))

            If diff > maxDiff Then
                maxDiff = diff
            End If
        Next

        Return maxDiff
    End Function

    ''' GPU 结果与同为单精度的参考值之间的最大绝对误差
    Public Shared Function MaxErrorSingle(got As Single(), expect As Single()) As Double
        Dim maxDiff As Double = 0

        For i As Integer = 0 To got.Length - 1
            Dim diff As Double = System.Math.Abs(CDbl(got(i)) - CDbl(expect(i)))

            If diff > maxDiff Then
                maxDiff = diff
            End If
        Next

        Return maxDiff
    End Function

    ''' 双精度参考值转单精度, 便于复用同一套预览排版
    Public Shared Function ToSingle(source As Double()) As Single()
        Dim out(source.Length - 1) As Single

        For i As Integer = 0 To source.Length - 1
            out(i) = CSng(source(i))
        Next

        Return out
    End Function

    ''' 分节标题
    Public Shared Sub PrintTitle(text As String)
        Call Console.WriteLine()
        Call Console.WriteLine(New String("="c, 74))
        Call Console.WriteLine("  " & text)
        Call Console.WriteLine(New String("="c, 74))
    End Sub

    ''' 缩进打印一段多行文本(伪代码 / CUDA 源码)
    Public Shared Sub PrintBlock(text As String, indent As String)
        Dim lines As String() = Regex.Split(text, "\r\n|\r|\n")

        For Each line As String In lines
            Call Console.WriteLine(indent & line)
        Next
    End Sub

    ''' 打印结果矩阵的左上角 n x n
    Public Shared Sub PrintMatrix(title As String, m As Single(), rows As Integer, n As Integer)
        Call Console.WriteLine("  " & title)

        For i As Integer = 0 To n - 1
            Dim line As New StringBuilder()

            For j As Integer = 0 To n - 1
                Call line.Append(m(i * rows + j).ToString("F4").PadLeft(10))
            Next

            Call Console.WriteLine("    " & line.ToString())
        Next
    End Sub
End Class

' ---------------------------------------------------------------------------
'  第 3 节: 主流程
' ---------------------------------------------------------------------------

dim rows As Integer = 1024
dim cols As Integer = 1024
dim seed As Integer = 42
dim preview As Integer = 6
dim allOk As Boolean = True

call TutorialKit.PrintTitle("IL -> CUDA 教程: 皮尔逊相关矩阵 与 欧氏距离矩阵")
call console.WriteLine("  输入规模: " & rows & " 行 x " & cols & " 列, 随机种子 " & seed)

' ---------- 造输入矩阵 ----------
dim data As Single() = TutorialKit.MakeMatrix(rows, cols, seed)

' ---------- 第 1 步: IL -> AST -> .cu ----------
dim kernels As New Dictionary(Of String, IlCudaKernel)(StringComparer.Ordinal)

call TutorialKit.PrintTitle("第 1 步: 把 VB.NET 函数反编译为 AST, 再发射成 .cu")

for each m As MethodInfo In TutorialKit.Targets()
    dim errMsg As String = Nothing
    dim kernel As IlCudaKernel = Nothing

    try
        kernel = IlCudaTranslator.Translate(m)
    catch ex As Exception
        errMsg = ex.GetBaseException().Message
    end try

    if errMsg IsNot Nothing Then
        call console.WriteLine("  " & m.Name & " 反编译失败: " & errMsg)
        allOk = False
    else
        call kernels.Add(m.Name, kernel)

        call console.WriteLine()
        call console.WriteLine("  ---- " & m.Name & " ----")
        call console.WriteLine("  索引模式  : " & kernel.IndexMode.ToString())
        call console.WriteLine("  设备函数  : " & kernel.DeviceFunctionName)
        call console.WriteLine("  内核      : " & kernel.Name)
        call console.WriteLine()
        call console.WriteLine("  还原出的伪代码:")
        call TutorialKit.PrintBlock(SyntaxWriter.WriteMethod(kernel.Syntax), "    ")
        call console.WriteLine()
        call console.WriteLine("  生成的 CUDA 源码:")
        call TutorialKit.PrintBlock(kernel.Source, "    ")

        ' 解释求值自检: 把同一棵 AST 交给 CPU 解释器执行, 与直接调用原方法比对
        dim sampleArgs As Object() = TutorialKit.SampleArgs(m)
        dim expected As Object = m.Invoke(Nothing, sampleArgs)
        dim actual As Object = New AstInterpreter(kernel.Syntax).Invoke(sampleArgs)
        dim diff As Double = System.Math.Abs(System.Convert.ToDouble(expected) - System.Convert.ToDouble(actual))
        dim tol As Double = 1.0E-5 * System.Math.Max(1.0, System.Math.Abs(System.Convert.ToDouble(expected)))
        dim pass As Boolean = diff <= tol

        call console.WriteLine()
        call console.WriteLine("  解释求值自检: 原方法=" & expected & ", AST=" & actual &
                               ", 差=" & diff.ToString("E3") & "  " & (if(pass, "OK", "FAIL")))

        if Not pass Then
            allOk = False
        end if
    end if
next

' ---------- 第 2 步: 把生成的 .cu 注册进 KernelSources ----------
call TutorialKit.PrintTitle("第 2 步: 注册内核源码(必须在创建引擎之前)")

for each k As IlCudaKernel In kernels.Values
    call k.Register()
    call console.WriteLine("  已注册 " & k.ToString())
next

' ---------- 第 3 步: NVRTC 编译 + 启动内核 ----------
call TutorialKit.PrintTitle("第 3 步: 在 GPU 上计算相关矩阵与距离矩阵")

dim opts As New EngineOptions With {.DeviceOrdinal = 0}
dim engine As CudaEngine = CudaEngine.TryCreate(opts)
dim corrGPU As Single() = Nothing
dim distGPU As Single() = Nothing
dim gpuOk As Boolean = False

if Not allOk Then
    call console.WriteLine("  第 1 步存在失败项, 跳过 GPU 计算。")
elseif engine Is Nothing Then
    call console.WriteLine("  GPU 不可用: " & opts.ErrorMessage)

    dim report = CudaEnvironment.Probe()

    for each s As FixSuggestion In CudaEnvironment.Suggest(report)
        call console.WriteLine("  建议: " & s.ToString())
        call console.WriteLine("        " & s.Detail)
    next

    call console.WriteLine()
    call console.WriteLine("  >> 已回退 CPU 参考实现, 下面只给出 CPU 结果。")
else
    using engine
        dim cells As Integer = rows * rows

        call console.WriteLine("  设备      : " & engine.Device.Name)
        call console.WriteLine("  内核镜像  : " & engine.Image.ToString())

        using bufX As New DeviceBuffer(Of Single)(data.Length), _
              bufSum As New DeviceBuffer(Of Single)(rows), _
              bufSumSq As New DeviceBuffer(Of Single)(rows), _
              bufDot As New DeviceBuffer(Of Single)(cells), _
              bufCorr As New DeviceBuffer(Of Single)(cells), _
              bufDist As New DeviceBuffer(Of Single)(cells)

            call bufX.Write(data)

            ' 1) 行统计量: 一维内核, 一个线程负责一行
            call kernels("RowSum").Launch(engine, LaunchPlanner.For1D(rows, 256), bufX, cols, bufSum, rows)
            call kernels("RowSumSq").Launch(engine, LaunchPlanner.For1D(rows, 256), bufX, cols, bufSumSq, rows)

            ' 2) Gram 点积: 二维内核
            call kernels("GramDot").Launch(engine, LaunchPlanner.For2D(rows, rows, 16, 16), bufX, cols, bufDot, rows, rows)

            ' 3) 由点积与行统计量还原相关矩阵与距离矩阵
            call kernels("CorrelationCell").Launch(engine, LaunchPlanner.For2D(rows, rows, 16, 16),
                                                   bufDot, bufSum, bufSumSq, rows, cols, bufCorr, rows, rows)
            call kernels("DistanceCell").Launch(engine, LaunchPlanner.For2D(rows, rows, 16, 16),
                                                bufDot, bufSumSq, rows, bufDist, rows, rows)

            call engine.Synchronize()

            corrGPU = bufCorr.Read()
            distGPU = bufDist.Read()
            gpuOk = True

            ' 4) 约定 3 的演示: 纯标量函数被自动包裹成逐元素内核
            dim hostDot As Single() = bufDot.Read()
            dim hostSum As Single() = bufSum.Read()
            dim hostSumSq As Single() = bufSumSq.Read()
            dim n1 As Single = CSng(cols)
            dim cov(cells - 1) As Single
            dim denom(cells - 1) As Single
            dim clampRef(cells - 1) As Single

            for i As Integer = 0 To rows - 1
                dim meanI As Single = hostSum(i) / n1
                dim varI As Single = hostSumSq(i) - n1 * meanI * meanI
                dim sdI As Single = MathF.Sqrt(MathF.Max(varI, 0.0F))

                for j As Integer = 0 To rows - 1
                    dim meanJ As Single = hostSum(j) / n1
                    dim varJ As Single = hostSumSq(j) - n1 * meanJ * meanJ
                    dim idx As Integer = i * rows + j

                    cov(idx) = hostDot(idx) - n1 * meanI * meanJ
                    denom(idx) = sdI * MathF.Sqrt(MathF.Max(varJ, 0.0F))
                    clampRef(idx) = PearsonMetrics.PearsonClamp(cov(idx), denom(idx))
                next
            next

            using bufCov As New DeviceBuffer(Of Single)(cells), _
                  bufDenom As New DeviceBuffer(Of Single)(cells), _
                  bufClamp As New DeviceBuffer(Of Single)(cells)

                call bufCov.Write(cov)
                call bufDenom.Write(denom)

                call kernels("PearsonClamp").Launch(engine, LaunchPlanner.For1D(cells, 256),
                                                    bufCov, bufDenom, bufClamp, cells)
                call engine.Synchronize()

                dim clampErr As Double = TutorialKit.MaxErrorSingle(bufClamp.Read(), clampRef)
                dim clampPass As Boolean = clampErr <= 1.0E-5

                call console.WriteLine()
                call console.WriteLine("  PearsonClamp(逐元素自动包裹) 最大绝对误差 = " &
                                       clampErr.ToString("E3") & "  " & (if(clampPass, "OK", "FAIL")))

                if Not clampPass Then
                    allOk = False
                end if
            end using
        end using
    end using
end if

' ---------- 第 4 步: 与 CPU 参考实现比对 ----------
call TutorialKit.PrintTitle("第 4 步: CPU 参考实现比对 + 结果预览")

dim refCorr As Double() = TutorialKit.CpuCorrelation(data, rows, cols)
dim refDist As Double() = TutorialKit.CpuDistance(data, rows, cols)

if gpuOk Then
    dim eCorr As Double = TutorialKit.MaxError(corrGPU, refCorr)
    dim eDist As Double = TutorialKit.MaxError(distGPU, refDist)
    dim corrPass As Boolean = eCorr <= 1.0E-3
    dim distPass As Boolean = eDist <= 1.0E-2

    call console.WriteLine("  皮尔逊相关矩阵 最大绝对误差 = " & eCorr.ToString("E3") & "  " & (if(corrPass, "OK", "FAIL")))
    call console.WriteLine("  欧氏距离矩阵   最大绝对误差 = " & eDist.ToString("E3") & "  " & (if(distPass, "OK", "FAIL")))

    if (Not corrPass) OrElse (Not distPass) Then
        allOk = False
    end if

    call console.WriteLine()
    call TutorialKit.PrintMatrix("皮尔逊相关矩阵(GPU, 左上 " & preview & " x " & preview & "):", corrGPU, rows, preview)
    call console.WriteLine()
    call TutorialKit.PrintMatrix("欧氏距离矩阵(GPU, 左上 " & preview & " x " & preview & "):", distGPU, rows, preview)
else
    call console.WriteLine("  GPU 结果不可用, 下面是 CPU 参考实现的结果。")
    call console.WriteLine()
    call TutorialKit.PrintMatrix("皮尔逊相关矩阵(CPU, 左上 " & preview & " x " & preview & "):", TutorialKit.ToSingle(refCorr), rows, preview)
    call console.WriteLine()
    call TutorialKit.PrintMatrix("欧氏距离矩阵(CPU, 左上 " & preview & " x " & preview & "):", TutorialKit.ToSingle(refDist), rows, preview)
end if

call console.WriteLine()

if allOk Then
    call console.WriteLine("教程全部步骤通过。")
else
    call console.WriteLine("存在失败项, 请查看上面的输出。")
end if
