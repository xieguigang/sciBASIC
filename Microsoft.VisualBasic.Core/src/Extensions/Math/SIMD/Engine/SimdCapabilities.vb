#Region "Microsoft.VisualBasic::58321379684a81a22593f171d54a7f4a, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Engine\SimdCapabilities.vb"

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

    '   Total Lines: 120
    '    Code Lines: 46 (38.33%)
    ' Comment Lines: 60 (50.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (11.67%)
    '     File Size: 5.32 KB


    '     Class SimdCapabilities
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BuildDescription, IsVectorizable, VectorSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.InteropServices
Imports System.Runtime.Intrinsics.Arm
Imports System.Runtime.Intrinsics.X86

Namespace Math.SIMD

    ''' <summary>
    ''' 当前处理器/运行时的 SIMD 能力探测结果。所有的探测值都在类型初始化阶段
    ''' 计算一次并被缓存下来，之后的查询都是常量时间的字段读取。
    ''' </summary>
    ''' <remarks>
    ''' 这个类型只做能力**读取**，不做任何分派决策；决定实际计算路径的策略集中在
    ''' <see cref="SIMDEnvironment.IsEnabled"/> 与各个内核实现之中。
    ''' </remarks>
    Public NotInheritable Class SimdCapabilities

        ''' <summary>
        ''' <see cref="Vector.IsHardwareAccelerated"/>，既 <see cref="System.Numerics.Vector(Of T)"/>
        ''' 的运算是否由硬件加速。在 JIT 无法使用硬件向量时会返回 False（此时
        ''' <see cref="System.Numerics.Vector(Of T)"/> 会退化为软件实现，通常比纯标量更慢）。
        ''' </summary>
        Public Shared ReadOnly IsHardwareAccelerated As Boolean = Vector.IsHardwareAccelerated

        ''' <summary>
        ''' 当前进程的处理器架构，例如 ``X64`` / ``Arm64``。
        ''' </summary>
        Public Shared ReadOnly Architecture As String = RuntimeInformation.ProcessArchitecture.ToString()

        ''' <summary>
        ''' SSE2 支持（x86/x64 上的向量化基线）。
        ''' </summary>
        Public Shared ReadOnly IsSse2 As Boolean = Sse2.IsSupported
        ''' <summary>
        ''' SSE4.2 支持。
        ''' </summary>
        Public Shared ReadOnly IsSse42 As Boolean = Sse42.IsSupported
        ''' <summary>
        ''' AVX 支持（256 位浮点向量）。
        ''' </summary>
        Public Shared ReadOnly IsAvx As Boolean = Avx.IsSupported
        ''' <summary>
        ''' AVX2 才具有整数类型的 256 位理论吞吐；对 <see cref="Double"/> 而言
        ''' 与 <see cref="IsAvx"/> 等价。
        ''' </summary>
        Public Shared ReadOnly IsAvx2 As Boolean = Avx2.IsSupported
        ''' <summary>
        ''' FMA(Fused Multiply-Add)融合乘加指令支持。开启之后
        ''' ``a * b + c`` 可以在一条指令内完成，是点积/平方和类运算的主要加速点。
        ''' </summary>
        Public Shared ReadOnly IsFma As Boolean = Fma.IsSupported

        ''' <summary>
        ''' ARM Advanced SIMD(NEON)支持。
        ''' </summary>
        Public Shared ReadOnly IsAdvSimd As Boolean = AdvSimd.IsSupported

        ''' <summary>
        ''' <c>Vector(Of Double).Count</c>，即一个向量寄存器可以容纳的 <see cref="Double"/> 个数。
        ''' </summary>
        Public Shared ReadOnly countDouble As Integer = Vector(Of Double).Count
        ''' <summary>
        ''' <c>Vector(Of Single).Count</c>
        ''' </summary>
        Public Shared ReadOnly countFloat As Integer = Vector(Of Single).Count
        ''' <summary>
        ''' <c>Vector(Of Integer).Count</c>
        ''' </summary>
        Public Shared ReadOnly countInteger As Integer = Vector(Of Integer).Count
        ''' <summary>
        ''' <c>Vector(Of Long).Count</c>
        ''' </summary>
        Public Shared ReadOnly countLong As Integer = Vector(Of Long).Count
        ''' <summary>
        ''' <c>Vector(Of Short).Count</c>
        ''' </summary>
        Public Shared ReadOnly countShort As Integer = Vector(Of Short).Count

        ''' <summary>
        ''' 人类可读的能力描述，主要用于基准测试与诊断日志输出。
        ''' </summary>
        Public Shared ReadOnly Description As String = BuildDescription()

        Private Sub New()
        End Sub

        Private Shared Function BuildDescription() As String
            Dim flags As New List(Of String)

            If IsHardwareAccelerated Then
                Call flags.Add("hardware-accelerated")
            Else
                Call flags.Add("software-fallback")
            End If
            If IsSse2 Then Call flags.Add("SSE2")
            If IsSse42 Then Call flags.Add("SSE4.2")
            If IsAvx Then Call flags.Add("AVX")
            If IsAvx2 Then Call flags.Add("AVX2")
            If IsFma Then Call flags.Add("FMA")
            If IsAdvSimd Then Call flags.Add("AdvSIMD/NEON")

            Return $"{Architecture} [{String.Join(", ", flags)}], vector width: " &
                $"f64={countDouble}, f32={countFloat}, i32={countInteger}, i64={countLong}, i16={countShort}"
        End Function

        ''' <summary>
        ''' 针对类型 <typeparamref name="T"/> 的一个向量寄存器可以容纳的元素个数。
        ''' </summary>
        Public Shared Function VectorSize(Of T As Structure)() As Integer
            Return Vector(Of T).Count
        End Function

        ''' <summary>
        ''' 针对类型 <typeparamref name="T"/> 是否值得走向量化路径（宽度大于 1 且硬件已加速）。
        ''' </summary>
        Public Shared Function IsVectorizable(Of T As Structure)() As Boolean
            Return IsHardwareAccelerated AndAlso Vector(Of T).Count > 1
        End Function
    End Class
End Namespace
