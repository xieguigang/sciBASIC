#Region "Microsoft.VisualBasic::ff851bf44d1c24f945185361ef05254a, cuda\ILCudaTensor\GPUTensor\DoubleKernels.vb"

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

    '   Total Lines: 298
    '    Code Lines: 216 (72.48%)
    ' Comment Lines: 29 (9.73%)
    '    - Xml Docs: 51.72%
    ' 
    '   Blank Lines: 53 (17.79%)
    '     File Size: 12.10 KB


    '     Module DoubleKernels
    ' 
    '         Function: DAbs, DAdd, DAddScalar, DClip, DCos
    '                   DDiv, DDivScalar, DElu, DExp, DGelu
    '                   DLeakyRelu, DLog, DMul, DNeg, DPow
    '                   DRecip, DRelu, DScale, DSigmoid, DSin
    '                   DSqrt, DSquare, DSub, DSwish, DTanh
    '                   DTranspose
    ' 
    '     Module DoubleKernelRegistry
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
' P2：用 IL2Cuda 自动生成的双精度逐元素内核
'
' 下面 DoubleKernels 里的方法**只用于被 IL2Cuda 反编译**，不会被托管代码直接调用：
'   VB.NET 源码 -> IL -> 反编译为 AST -> 发射 CUDA C -> 注入 NVRTC 编译单元
'
' 与 ILCuda 自带内核（全部是 float）相比，这些生成内核是 double 精度，
' 因此 Tensor 的逐元素运算不再需要 Double -> Single 的降精度桥接。
'
' 内核映射约定（详见 ILCuda 的 README）：
'   * 纯标量公式（没有数组取元素）    -> 逐元素包裹：标量参数变成按下标读取的数组
'   * 带 i 形参                        -> 一维内核，i = blockIdx.x * blockDim.x + threadIdx.x
'   * 带 i / j 形参                    -> 二维内核
' ---------------------------------------------------------------------------

Imports System.Reflection
Imports Microsoft.VisualBasic.Computing.ILCuda.IL2Cuda
Imports std = System.Math

Namespace GPUTensor

    ''' <summary>
    ''' 待反编译为 CUDA 的双精度逐元素函数（本身不会被托管调用）。
    ''' </summary>
    Public Module DoubleKernels

#Region "逐元素 - 二元（纯标量：两个输入数组）"

        <CudaKernel(DoubleKernelRegistry.EwAdd)>
        Public Function DAdd(a As Double, b As Double) As Double
            Return a + b
        End Function

        <CudaKernel(DoubleKernelRegistry.EwSub)>
        Public Function DSub(a As Double, b As Double) As Double
            Return a - b
        End Function

        <CudaKernel(DoubleKernelRegistry.EwMul)>
        Public Function DMul(a As Double, b As Double) As Double
            Return a * b
        End Function

        <CudaKernel(DoubleKernelRegistry.EwDiv)>
        Public Function DDiv(a As Double, b As Double) As Double
            Return a / b
        End Function

#End Region

#Region "逐元素 - 一元（纯标量）"

        <CudaKernel(DoubleKernelRegistry.EwExp)>
        Public Function DExp(x As Double) As Double
            Return std.Exp(x)
        End Function

        <CudaKernel(DoubleKernelRegistry.EwLog)>
        Public Function DLog(x As Double) As Double
            Return std.Log(x)
        End Function

        <CudaKernel(DoubleKernelRegistry.EwSqrt)>
        Public Function DSqrt(x As Double) As Double
            Return std.Sqrt(x)
        End Function

        <CudaKernel(DoubleKernelRegistry.EwAbs)>
        Public Function DAbs(x As Double) As Double
            Return std.Abs(x)
        End Function

        <CudaKernel(DoubleKernelRegistry.EwNeg)>
        Public Function DNeg(x As Double) As Double
            Return -x
        End Function

        <CudaKernel(DoubleKernelRegistry.EwRecip)>
        Public Function DRecip(x As Double) As Double
            Return 1.0 / x
        End Function

        <CudaKernel(DoubleKernelRegistry.EwSquare)>
        Public Function DSquare(x As Double) As Double
            Return x * x
        End Function

        <CudaKernel(DoubleKernelRegistry.EwTanh)>
        Public Function DTanh(x As Double) As Double
            Return std.Tanh(x)
        End Function

        <CudaKernel(DoubleKernelRegistry.EwSigmoid)>
        Public Function DSigmoid(x As Double) As Double
            Return 1.0 / (1.0 + std.Exp(-x))
        End Function

        <CudaKernel(DoubleKernelRegistry.EwSin)>
        Public Function DSin(x As Double) As Double
            Return std.Sin(x)
        End Function

        <CudaKernel(DoubleKernelRegistry.EwCos)>
        Public Function DCos(x As Double) As Double
            Return std.Cos(x)
        End Function

        <CudaKernel(DoubleKernelRegistry.EwRelu)>
        Public Function DRelu(x As Double) As Double
            Return std.Max(0.0, x)
        End Function

        <CudaKernel(DoubleKernelRegistry.EwGelu)>
        Public Function DGelu(x As Double) As Double
            Return 0.5 * x * (1.0 + std.Tanh(std.Sqrt(2.0 / std.PI) * (x + 0.044715 * x * x * x)))
        End Function

        <CudaKernel(DoubleKernelRegistry.EwSwish)>
        Public Function DSwish(x As Double) As Double
            Return x / (1.0 + std.Exp(-x))
        End Function

#End Region

#Region "逐元素 - 标量参数（x 数组 + 运行时标量）"

        <CudaKernel(DoubleKernelRegistry.EwScale)>
        Public Function DScale(x As Double(), alpha As Double, i As Integer) As Double
            Return alpha * x(i)
        End Function

        <CudaKernel(DoubleKernelRegistry.EwAddScalar)>
        Public Function DAddScalar(x As Double(), s As Double, i As Integer) As Double
            Return x(i) + s
        End Function

        <CudaKernel(DoubleKernelRegistry.EwDivScalar)>
        Public Function DDivScalar(x As Double(), s As Double, i As Integer) As Double
            Return x(i) / s
        End Function

        <CudaKernel(DoubleKernelRegistry.EwPow)>
        Public Function DPow(x As Double(), exponent As Double, i As Integer) As Double
            Return std.Pow(x(i), exponent)
        End Function

        <CudaKernel(DoubleKernelRegistry.EwClip)>
        Public Function DClip(x As Double(), low As Double, high As Double, i As Integer) As Double
            Return std.Min(std.Max(x(i), low), high)
        End Function

        <CudaKernel(DoubleKernelRegistry.EwLeakyRelu)>
        Public Function DLeakyRelu(x As Double(), alpha As Double, i As Integer) As Double
            If x(i) >= 0.0 Then
                Return x(i)
            End If

            Return alpha * x(i)
        End Function

        <CudaKernel(DoubleKernelRegistry.EwElu)>
        Public Function DElu(x As Double(), alpha As Double, i As Integer) As Double
            If x(i) >= 0.0 Then
                Return x(i)
            End If

            Return alpha * (std.Exp(x(i)) - 1.0)
        End Function

#End Region

#Region "矩阵 - 转置（二维内核）"

        ''' <summary>
        ''' 转置：输出 (C, R) 的第 (i, j) 个元素 = 输入 (R, C) 的第 (j, i) 个元素。
        ''' 内核以二维 grid 启动，il_nRows = C、il_nCols = R。
        ''' </summary>
        <CudaKernel(DoubleKernelRegistry.Transpose, CudaIndexMode.Grid2D)>
        Public Function DTranspose(x As Double(), cols As Integer, i As Integer, j As Integer) As Double
            Return x(j * cols + i)
        End Function

#End Region

    End Module

    ''' <summary>
    ''' 把 <see cref="DoubleKernels"/> 里的方法逐个翻译成 CUDA 内核并注册到编译单元。
    ''' 必须在 <c>CudaEngine.TryCreate</c> 之前调用。
    ''' </summary>
    Friend Module DoubleKernelRegistry

        Public Const EwAdd As String = "il_tensor_ew_add"
        Public Const EwSub As String = "il_tensor_ew_sub"
        Public Const EwMul As String = "il_tensor_ew_mul"
        Public Const EwDiv As String = "il_tensor_ew_div"
        Public Const EwExp As String = "il_tensor_ew_exp"
        Public Const EwLog As String = "il_tensor_ew_log"
        Public Const EwSqrt As String = "il_tensor_ew_sqrt"
        Public Const EwAbs As String = "il_tensor_ew_abs"
        Public Const EwNeg As String = "il_tensor_ew_neg"
        Public Const EwRecip As String = "il_tensor_ew_recip"
        Public Const EwSquare As String = "il_tensor_ew_square"
        Public Const EwTanh As String = "il_tensor_ew_tanh"
        Public Const EwSigmoid As String = "il_tensor_ew_sigmoid"
        Public Const EwSin As String = "il_tensor_ew_sin"
        Public Const EwCos As String = "il_tensor_ew_cos"
        Public Const EwRelu As String = "il_tensor_ew_relu"
        Public Const EwGelu As String = "il_tensor_ew_gelu"
        Public Const EwSwish As String = "il_tensor_ew_swish"
        Public Const EwScale As String = "il_tensor_ew_scale"
        Public Const EwAddScalar As String = "il_tensor_ew_add_scalar"
        Public Const EwDivScalar As String = "il_tensor_ew_div_scalar"
        Public Const EwPow As String = "il_tensor_ew_pow"
        Public Const EwClip As String = "il_tensor_ew_clip"
        Public Const EwLeakyRelu As String = "il_tensor_ew_leaky_relu"
        Public Const EwElu As String = "il_tensor_ew_elu"
        Public Const Transpose As String = "il_tensor_transpose"

        Private ReadOnly _sync As New Object()
        Private ReadOnly _failures As New Dictionary(Of String, String)
        Private _registered As Boolean

        ''' <summary>翻译失败的“方法名 -> 失败原因”</summary>
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

        ''' <summary>逐个翻译并注册全部双精度内核（幂等）</summary>
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
                    {NameOf(DoubleKernels.DAdd), EwAdd},
                    {NameOf(DoubleKernels.DSub), EwSub},
                    {NameOf(DoubleKernels.DMul), EwMul},
                    {NameOf(DoubleKernels.DDiv), EwDiv},
                    {NameOf(DoubleKernels.DExp), EwExp},
                    {NameOf(DoubleKernels.DLog), EwLog},
                    {NameOf(DoubleKernels.DSqrt), EwSqrt},
                    {NameOf(DoubleKernels.DAbs), EwAbs},
                    {NameOf(DoubleKernels.DNeg), EwNeg},
                    {NameOf(DoubleKernels.DRecip), EwRecip},
                    {NameOf(DoubleKernels.DSquare), EwSquare},
                    {NameOf(DoubleKernels.DTanh), EwTanh},
                    {NameOf(DoubleKernels.DSigmoid), EwSigmoid},
                    {NameOf(DoubleKernels.DSin), EwSin},
                    {NameOf(DoubleKernels.DCos), EwCos},
                    {NameOf(DoubleKernels.DRelu), EwRelu},
                    {NameOf(DoubleKernels.DGelu), EwGelu},
                    {NameOf(DoubleKernels.DSwish), EwSwish},
                    {NameOf(DoubleKernels.DScale), EwScale},
                    {NameOf(DoubleKernels.DAddScalar), EwAddScalar},
                    {NameOf(DoubleKernels.DDivScalar), EwDivScalar},
                    {NameOf(DoubleKernels.DPow), EwPow},
                    {NameOf(DoubleKernels.DClip), EwClip},
                    {NameOf(DoubleKernels.DLeakyRelu), EwLeakyRelu},
                    {NameOf(DoubleKernels.DElu), EwElu},
                    {NameOf(DoubleKernels.DTranspose), Transpose}
                }
            End Get
        End Property

        ''' <summary>翻译用：按方法名取 <see cref="DoubleKernels"/> 上的静态方法</summary>
        Friend Function KernelMethod(methodName As String) As MethodInfo
            Return GetType(DoubleKernels).GetMethod(
                methodName, BindingFlags.Public Or BindingFlags.Static Or BindingFlags.NonPublic)
        End Function

    End Module

End Namespace

