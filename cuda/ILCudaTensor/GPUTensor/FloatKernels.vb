#Region "Microsoft.VisualBasic::793af5372f78a4c7dd22264c65349b4c, cuda\ILCudaTensor\GPUTensor\FloatKernels.vb"

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

    '   Total Lines: 252
    '    Code Lines: 172 (68.25%)
    ' Comment Lines: 37 (14.68%)
    '    - Xml Docs: 43.24%
    ' 
    '   Blank Lines: 43 (17.06%)
    '     File Size: 10.42 KB


    '     Module FloatKernels
    ' 
    '         Function: FAbs, FAdd, FAddScalar, FClip, FDiv
    '                   FDivScalar, FExp, FLog, FMax, FMin
    '                   FMul, FNeg, FRecip, FRelu, FScale
    '                   FSigmoid, FSqrt, FSquare, FSub, FTanh
    ' 
    '     Module FloatKernelRegistry
    ' 
    '         Properties: Failures, Registry
    ' 
    '         Function: Available, KernelMethod
    ' 
    '         Sub: EnsureRegistered
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' P2（单精度版）：用 IL2Cuda 自动生成的 float32 逐元素内核
'
' 与双精度栈的 DoubleKernels.vb 完全同构：
'   VB.NET 源码 -> IL -> 反编译为 AST -> 发射 CUDA C -> 注入 NVRTC 编译单元
' 下面 FloatKernels 里的方法**只用于被 IL2Cuda 反编译**，不会被托管代码直接调用。
'
' 之所以可以整份照搬双精度栈的做法：ILCuda 的 CudaTypeMap 已经完整支持单精度 ——
'   * Single -> float（Double -> double）
'   * System.Math 的静态方法按 PrefersSingle() 自动加 f 后缀：
'     sqrtf / expf / logf / fabsf / fminf / fmaxf / tanhf ...
' 因此这些生成内核天然是 float32 语义，不会隐式双精度化。
'
' 内核映射约定（详见 ILCuda 的 README）：
'   * 纯标量公式（没有数组取元素）    -> 逐元素包裹：标量参数变成按下标读取的数组
'   * 带 i 形参                        -> 一维内核，i = blockIdx.x * blockDim.x + threadIdx.x
'   * 带 i / j 形参                    -> 二维内核
'
' 内核名统一以 il_tensorf_ 开头（双精度栈是 il_tensor_），避免两条栈在同一
' 编译单元里撞名 —— 两条栈的内核会同时被注入同一个 NVRTC 程序。
' ---------------------------------------------------------------------------

Imports System.Reflection
Imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
Imports std = System.Math

Namespace GPUTensor

    ''' <summary>
    ''' 待反编译为 CUDA 的单精度（float32）逐元素函数（本身不会被托管调用）。
    ''' </summary>
    Public Module FloatKernels

#Region "逐元素 - 二元（纯标量：两个输入数组）"

        <CudaKernel(FloatKernelRegistry.EwAdd)>
        Public Function FAdd(a As Single, b As Single) As Single
            Return a + b
        End Function

        <CudaKernel(FloatKernelRegistry.EwSub)>
        Public Function FSub(a As Single, b As Single) As Single
            Return a - b
        End Function

        <CudaKernel(FloatKernelRegistry.EwMul)>
        Public Function FMul(a As Single, b As Single) As Single
            Return a * b
        End Function

        <CudaKernel(FloatKernelRegistry.EwDiv)>
        Public Function FDiv(a As Single, b As Single) As Single
            Return a / b
        End Function

        <CudaKernel(FloatKernelRegistry.EwMax)>
        Public Function FMax(a As Single, b As Single) As Single
            Return std.Max(a, b)
        End Function

        <CudaKernel(FloatKernelRegistry.EwMin)>
        Public Function FMin(a As Single, b As Single) As Single
            Return std.Min(a, b)
        End Function

#End Region

#Region "逐元素 - 一元（纯标量）"

        <CudaKernel(FloatKernelRegistry.EwExp)>
        Public Function FExp(x As Single) As Single
            Return std.Exp(x)
        End Function

        <CudaKernel(FloatKernelRegistry.EwLog)>
        Public Function FLog(x As Single) As Single
            Return std.Log(x)
        End Function

        <CudaKernel(FloatKernelRegistry.EwSqrt)>
        Public Function FSqrt(x As Single) As Single
            Return std.Sqrt(x)
        End Function

        <CudaKernel(FloatKernelRegistry.EwAbs)>
        Public Function FAbs(x As Single) As Single
            Return std.Abs(x)
        End Function

        <CudaKernel(FloatKernelRegistry.EwNeg)>
        Public Function FNeg(x As Single) As Single
            Return -x
        End Function

        <CudaKernel(FloatKernelRegistry.EwRecip)>
        Public Function FRecip(x As Single) As Single
            Return 1.0F / x
        End Function

        <CudaKernel(FloatKernelRegistry.EwSquare)>
        Public Function FSquare(x As Single) As Single
            Return x * x
        End Function

        <CudaKernel(FloatKernelRegistry.EwTanh)>
        Public Function FTanh(x As Single) As Single
            Return std.Tanh(x)
        End Function

        <CudaKernel(FloatKernelRegistry.EwSigmoid)>
        Public Function FSigmoid(x As Single) As Single
            Return 1.0F / (1.0F + std.Exp(-x))
        End Function

        <CudaKernel(FloatKernelRegistry.EwRelu)>
        Public Function FRelu(x As Single) As Single
            Return std.Max(0.0F, x)
        End Function

#End Region

#Region "逐元素 - 标量参数（x 数组 + 运行时标量）"

        <CudaKernel(FloatKernelRegistry.EwScale)>
        Public Function FScale(x As Single(), alpha As Single, i As Integer) As Single
            Return alpha * x(i)
        End Function

        <CudaKernel(FloatKernelRegistry.EwAddScalar)>
        Public Function FAddScalar(x As Single(), s As Single, i As Integer) As Single
            Return x(i) + s
        End Function

        <CudaKernel(FloatKernelRegistry.EwDivScalar)>
        Public Function FDivScalar(x As Single(), s As Single, i As Integer) As Single
            Return x(i) / s
        End Function

        <CudaKernel(FloatKernelRegistry.EwClip)>
        Public Function FClip(x As Single(), low As Single, high As Single, i As Integer) As Single
            Return std.Min(std.Max(x(i), low), high)
        End Function

#End Region

    End Module

    ''' <summary>
    ''' 把 <see cref="FloatKernels"/> 里的方法逐个翻译成 CUDA 内核并注册到编译单元。
    ''' 必须在 <c>CudaEngine.TryCreate</c> 之前调用。
    ''' </summary>
    ''' <remarks>
    ''' 与 <see cref="DoubleKernelRegistry"/> 的三段式结构保持一致：
    ''' 内核名常量 -> <see cref="EnsureRegistered"/>（幂等、异常逐个记入
    ''' <see cref="Failures"/>）-> <see cref="Available"/>（算子据此决定是否回退 CPU）。
    ''' </remarks>
    Friend Module FloatKernelRegistry

        Public Const EwAdd As String = "il_tensorf_ew_add"
        Public Const EwSub As String = "il_tensorf_ew_sub"
        Public Const EwMul As String = "il_tensorf_ew_mul"
        Public Const EwDiv As String = "il_tensorf_ew_div"
        Public Const EwMax As String = "il_tensorf_ew_max"
        Public Const EwMin As String = "il_tensorf_ew_min"
        Public Const EwExp As String = "il_tensorf_ew_exp"
        Public Const EwLog As String = "il_tensorf_ew_log"
        Public Const EwSqrt As String = "il_tensorf_ew_sqrt"
        Public Const EwAbs As String = "il_tensorf_ew_abs"
        Public Const EwNeg As String = "il_tensorf_ew_neg"
        Public Const EwRecip As String = "il_tensorf_ew_recip"
        Public Const EwSquare As String = "il_tensorf_ew_square"
        Public Const EwTanh As String = "il_tensorf_ew_tanh"
        Public Const EwSigmoid As String = "il_tensorf_ew_sigmoid"
        Public Const EwRelu As String = "il_tensorf_ew_relu"
        Public Const EwScale As String = "il_tensorf_ew_scale"
        Public Const EwAddScalar As String = "il_tensorf_ew_add_scalar"
        Public Const EwDivScalar As String = "il_tensorf_ew_div_scalar"
        Public Const EwClip As String = "il_tensorf_ew_clip"

        Private ReadOnly _sync As New Object()
        Private ReadOnly _failures As New Dictionary(Of String, String)
        Private _registered As Boolean

        ''' <summary>翻译失败的"方法名 -> 失败原因"</summary>
        Public ReadOnly Property Failures As IReadOnlyDictionary(Of String, String)
            Get
                Return _failures
            End Get
        End Property

        ''' <summary>该内核是否可用（已成功翻译并注册）</summary>
        Public Function Available(kernelName As String) As Boolean
            EnsureRegistered()
            Return Not _failures.ContainsKey(kernelName)
        End Function

        ''' <summary>逐个翻译并注册全部单精度内核（幂等）</summary>
        Public Sub EnsureRegistered()
            If _registered Then Return

            SyncLock _sync
                If _registered Then Return

                For Each pair In Registry
                    Try
                        Dim kernel = IlCudaTranslator.Translate(KernelMethod(pair.Key), pair.Value)
                        kernel.Register()
                    Catch ex As Exception
                        _failures(pair.Value) = $"{pair.Key}: {ex.Message}"
                    End Try
                Next

                _registered = True
            End SyncLock
        End Sub

        Private ReadOnly Property Registry As Dictionary(Of String, String)
            Get
                Return New Dictionary(Of String, String) From {
                    {NameOf(FloatKernels.FAdd), EwAdd},
                    {NameOf(FloatKernels.FSub), EwSub},
                    {NameOf(FloatKernels.FMul), EwMul},
                    {NameOf(FloatKernels.FDiv), EwDiv},
                    {NameOf(FloatKernels.FMax), EwMax},
                    {NameOf(FloatKernels.FMin), EwMin},
                    {NameOf(FloatKernels.FExp), EwExp},
                    {NameOf(FloatKernels.FLog), EwLog},
                    {NameOf(FloatKernels.FSqrt), EwSqrt},
                    {NameOf(FloatKernels.FAbs), EwAbs},
                    {NameOf(FloatKernels.FNeg), EwNeg},
                    {NameOf(FloatKernels.FRecip), EwRecip},
                    {NameOf(FloatKernels.FSquare), EwSquare},
                    {NameOf(FloatKernels.FTanh), EwTanh},
                    {NameOf(FloatKernels.FSigmoid), EwSigmoid},
                    {NameOf(FloatKernels.FRelu), EwRelu},
                    {NameOf(FloatKernels.FScale), EwScale},
                    {NameOf(FloatKernels.FAddScalar), EwAddScalar},
                    {NameOf(FloatKernels.FDivScalar), EwDivScalar},
                    {NameOf(FloatKernels.FClip), EwClip}
                }
            End Get
        End Property

        ''' <summary>翻译用：按方法名取 <see cref="FloatKernels"/> 上的静态方法</summary>
        Friend Function KernelMethod(methodName As String) As MethodInfo
            Return GetType(FloatKernels).GetMethod(
                methodName, BindingFlags.Public Or BindingFlags.Static Or BindingFlags.NonPublic)
        End Function

    End Module

End Namespace
