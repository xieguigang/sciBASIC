#Region "Microsoft.VisualBasic::a39325e2fff870a383054c66ec7c14f2, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\SIMD.vb"

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

    '   Total Lines: 65
    '    Code Lines: 23 (35.38%)
    ' Comment Lines: 34 (52.31%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (12.31%)
    '     File Size: 2.30 KB


    '     Enum SIMDConfiguration
    ' 
    '         auto, disable, enable, legacy
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class SIMDEnvironment
    ' 
    '         Properties: config
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics

Namespace Math.SIMD

    ''' <summary>
    ''' SIMD 计算模式的配置枚举。
    ''' </summary>
    ''' <remarks>
    ''' 在 v10 之后，运行时会**自动**探测处理器能力并分派到最佳计算路径，
    ''' 因此除 <see cref="disable"/> 之外的成员都只是为了让旧代码可以继续编译
    ''' 而保留的历史取值（它们会被当作 <see cref="auto"/> 处理）。
    ''' </remarks>
    Public Enum SIMDConfiguration
        ''' <summary>
        ''' 强制使用纯标量路径。这是唯一的“逃生口”开关，用于
        ''' 性能对照测试或者在遇到精度/平台问题时回退。
        ''' </summary>
        disable
        ''' <summary>
        ''' 【兼容保留】历史取值：使用 ``System.Runtime.Intrinsics.X86`` 的 SIMD 支持。
        ''' 现在等价于 <see cref="auto"/>。
        ''' </summary>
        enable
        ''' <summary>
        ''' 【兼容保留】历史取值：使用 ``System.Numerics.Vector`` 的 SIMD 支持。
        ''' 现在等价于 <see cref="auto"/>。
        ''' </summary>
        legacy

        ''' <summary>
        ''' 根据处理器能力自动选择向量化路径（默认值）。
        ''' </summary>
        auto
    End Enum

    ''' <summary>
    ''' SIMD(Single Instruction Multiple Data)即单指令流多数据流，
    ''' 是一种采用一个控制器来控制多个处理器，同时对一组数据（又称“数据
    ''' 向量”）中的每一个分别执行相同的操作从而实现空间上的并行性的技术。
    ''' 简单来说就是一个指令能够同时处理多个数据。
    ''' </summary>
    ''' <remarks>
    ''' 逐元素内核（<see cref="SimdEngine"/> / <see cref="SimdMath"/>）不会对
    ''' 输入数组做长度一致性检查，默认调用方已经保证这一点；而
    ''' 归约与点积（<see cref="SimdReduce"/>）会显式校验长度并抛出
    ''' <see cref="ArgumentException"/>。
    ''' </remarks>
    Public NotInheritable Class SIMDEnvironment

        Private Shared _config As SIMDConfiguration = SIMDConfiguration.auto

        ''' <summary>
        ''' 【兼容保留】SIMD 计算模式配置。
        ''' </summary>
        ''' <remarks>
        ''' 为了保持与旧代码的二进制/源码兼容，这个属性仍然存在且可读写；
        ''' 但只有 <see cref="SIMDConfiguration.disable"/> 会影响实际分派结果
        ''' （强制走纯标量路径），其余取值统一按自动探测处理。
        ''' </remarks>
        Public Shared Property config As SIMDConfiguration
            Get
                Return _config
            End Get
            Set(value As SIMDConfiguration)
                _config = value
            End Set
        End Property

        ''' <summary>
        ''' 当前的 SIMD 是否启用。等价于 <c>config &lt;&gt; disable</c> 并且
        ''' 硬件确实支持向量加速。
        ''' </summary>
        Public Shared ReadOnly Property IsEnabled As Boolean
            Get
                Return _config <> SIMDConfiguration.disable AndAlso
                    SimdCapabilities.IsHardwareAccelerated
            End Get
        End Property

        ''' <summary>
        ''' <c>Vector(Of Double).Count</c>
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
        ''' 委托到 <see cref="SimdCapabilities.IsHardwareAccelerated"/>。
        ''' </summary>
        Public Shared ReadOnly Property IsHardwareAccelerated As Boolean
            Get
                Return SimdCapabilities.IsHardwareAccelerated
            End Get
        End Property

        ''' <summary>
        ''' 委托到 <see cref="SimdCapabilities.IsFma"/>。
        ''' </summary>
        Public Shared ReadOnly Property IsFmaSupported As Boolean
            Get
                Return SimdCapabilities.IsFma
            End Get
        End Property

        ''' <summary>
        ''' 委托到 <see cref="SimdCapabilities.IsAvx2"/>。
        ''' </summary>
        Public Shared ReadOnly Property IsAvx2Supported As Boolean
            Get
                Return SimdCapabilities.IsAvx2
            End Get
        End Property

        ''' <summary>
        ''' 委托到 <see cref="SimdCapabilities.IsAdvSimd"/>。
        ''' </summary>
        Public Shared ReadOnly Property IsAdvSimdSupported As Boolean
            Get
                Return SimdCapabilities.IsAdvSimd
            End Get
        End Property

        ''' <summary>
        ''' 当前处理器的能力描述文本。
        ''' </summary>
        Public Shared ReadOnly Property Description As String
            Get
                Return SimdCapabilities.Description
            End Get
        End Property

        Private Sub New()
        End Sub
    End Class
End Namespace
