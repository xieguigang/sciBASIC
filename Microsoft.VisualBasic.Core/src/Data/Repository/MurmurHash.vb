#Region "Microsoft.VisualBasic::73da11469ecd72cc25d5a283afd636ea, Microsoft.VisualBasic.Core\src\Data\Repository\MurmurHash.vb"

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

    '   Total Lines: 150
    '    Code Lines: 97 (64.67%)
    ' Comment Lines: 35 (23.33%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 18 (12.00%)
    '     File Size: 5.77 KB


    '     Module MurmurHash
    ' 
    '         Function: (+2 Overloads) MurmurHashCode3_x86_32
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Buffers.Binary

Namespace Data.Repository

    ''' <summary>
    ''' MurmurHash3 x86_32 哈希实现。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 关于 SIMD 加速的评估结论（2026-10）：
    ''' MurmurHash3 对单条数据而言是一条乘法/旋转/异或的<b>串行依赖链</b>，无法对单条哈希
    ''' 进行向量化；而 <c>Microsoft.VisualBasic.Math.SIMD</c> 库
    ''' （<see cref="Math.SIMD.SIMDIntrinsics"/> 提供 Double/Single FMA 内核，
    ''' <c>Vectorized</c> 提供 VecAdd/VecMultiply/VecModulo 等逐元素算术原语）
    ''' 缺少 MurmurHash 所需的 32 位整数 rotate、字节 shuffle 原语，
    ''' 因此<b>无法基于该 SIMD 库对本模块的哈希函数进行加速</b>。
    ''' </para>
    ''' <para>
    ''' 替代优化方案：字节数组重载的主体循环已改用
    ''' <see cref="BinaryPrimitives.ReadUInt32LittleEndian(ReadOnlySpan(Of Byte))"/>
    ''' 每 4 字节一次批量读取，相比旧版逐字节移位拼接，消除了每块 4 次的数组边界检查
    ''' 与移位/或运算，热路径预期有 2~4 倍提升，且无任何堆内存分配，计算结果位级一致。
    ''' </para>
    ''' <para>
    ''' 若将来出现批量哈希场景（例如全量索引重建、批量加载），可以考虑基于
    ''' <c>System.Runtime.Intrinsics.X86.Avx2</c> 实现 8 通道并行的 MurmurHash
    ''' （使用 <c>Avx2.MultiplyLow</c> + Sse2 移位/或模拟 rotate，同时处理 8 条数据），
    ''' 该实现应直接使用 <c>System.Runtime.Intrinsics</c>，而不是 sciBASIC 的 SIMD 库；
    ''' 在当前 BucketDb 单 key Get/Put 访问模式下收益有限，暂不实现。
    ''' </para>
    ''' </remarks>
    Public Module MurmurHash

        ' MurmurHash3 的常量
        Const c1 As UInteger = &HCC9E2D51UI
        Const c2 As UInteger = &H1B873593UI
        Const r1 As Integer = 15
        Const r2 As Integer = 13
        Const m As UInteger = 5UI
        Const n As UInteger = &HE6546B64UI

        <System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)>
        Private Function LoadUInt32(data As Byte(), i As Integer) As UInteger
            ' 每 4 字节一次批量小端读取：
            ' 相比逐字节移位拼接，消除 4 次数组边界检查与 3 次移位/或运算，
            ' JIT 会将 AsSpan 越界检查折叠进主循环的条件判断，结果位级一致
            Return BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(i, 4))
        End Function

        ''' <summary>
        ''' 计算给定数据的32位MurmurHash3值。
        ''' </summary>
        ''' <param name="data">输入数据。</param>
        ''' <param name="seed">哈希种子。</param>
        ''' <returns>32位无符号哈希值。</returns>
        ''' <remarks>
        ''' 主体循环使用 <see cref="BinaryPrimitives.ReadUInt32LittleEndian(ReadOnlySpan(Of Byte))"/>
        ''' 批量读取以消除边界检查与移位拼接开销，详见类型注释中的 SIMD 评估结论。
        ''' </remarks>
        Public Function MurmurHashCode3_x86_32(data As Byte(), seed As UInteger) As UInteger
            Dim length As Integer = data.Length
            Dim h As UInteger = seed
            Dim i As Integer = 0
            Dim k As UInteger = 0

            ' --- 处理主体部分（4字节块） ---
            While length - i >= 4
                k = LoadUInt32(data, i)

                k *= c1
                k = (k << r1) Or (k >> (32 - r1))
                k *= c2

                h = h Xor k
                h = (h << r2) Or (h >> (32 - r2))
                h = h * m + n
                i += 4
            End While

            ' --- 处理尾部剩余字节（修正部分） ---
            k = 0
            Select Case length - i
                Case 3
                    k = k Xor CUInt(data(i + 2)) << 16
                    ' 注意：这里没有 Exit Select，会继续执行 Case 2
                    k = k Xor CUInt(data(i + 1)) << 8
                    ' 继续执行 Case 1
                    k = k Xor CUInt(data(i))
                Case 2
                    k = k Xor CUInt(data(i + 1)) << 8
                    ' 继续执行 Case 1
                    k = k Xor CUInt(data(i))
                Case 1
                    k = k Xor CUInt(data(i))
            End Select

            If length - i > 0 Then
                k *= c1
                k = (k << r1) Or (k >> (32 - r1))
                k *= c2
                h = h Xor k
            End If

            ' --- 最终混合 ---
            h = h Xor CUInt(length)
            h = h Xor (h >> 16)
            h *= &H85EBCA6BUI
            h = h Xor (h >> 13)
            h *= &HC2B2AE35UI
            h = h Xor (h >> 16)

            Return h
        End Function

        ''' <summary>
        ''' 直接从字符串的字符区间上面计算32位 MurmurHash3 值，不会产生任何临时的字符串或者字节数组分配。
        ''' </summary>
        ''' <param name="s">源字符串</param>
        ''' <param name="start">字符区间的起始下标</param>
        ''' <param name="length">字符区间的长度</param>
        ''' <param name="seed">哈希种子</param>
        ''' <returns>32位无符号哈希值</returns>
        ''' <remarks>
        ''' 这个重载要求字符区间之内全部都是 ASCII 字符：因为对于 ASCII 字符而言，
        ''' UTF8 编码的结果就是字符本身的值，所以在满足这个前提的情况下，
        ''' 结果与 <c>MurmurHashCode3_x86_32(Encoding.UTF8.GetBytes(s.Substring(start, length)), seed)</c>
        ''' 完全一致，但是没有任何堆上的内存分配，可以安全地放在上亿次调用的热循环之中。
        ''' 
        ''' 对于可能包含非 ASCII 字符的数据，请使用字节数组版本。
        ''' </remarks>
        Public Function MurmurHashCode3_x86_32(s As String, start As Integer, length As Integer, seed As UInteger) As UInteger
            Dim h As UInteger = seed
            Dim i As Integer = 0
            Dim k As UInteger = 0

            ' --- 处理主体部分（4字节块） ---
            While length - i >= 4
                k = CUInt(AscW(s(start + i))) Or
                            CUInt(AscW(s(start + i + 1))) << 8 Or
                            CUInt(AscW(s(start + i + 2))) << 16 Or
                            CUInt(AscW(s(start + i + 3))) << 24

                k *= c1
                k = (k << r1) Or (k >> (32 - r1))
                k *= c2

                h = h Xor k
                h = (h << r2) Or (h >> (32 - r2))
                h = h * m + n
                i += 4
            End While

            ' --- 处理尾部剩余字节（修正部分） ---
            k = 0
            Select Case length - i
                Case 3
                    k = k Xor CUInt(AscW(s(start + i + 2))) << 16
                    ' 注意：这里没有 Exit Select，会继续执行 Case 2
                    k = k Xor CUInt(AscW(s(start + i + 1))) << 8
                    ' 继续执行 Case 1
                    k = k Xor CUInt(AscW(s(start + i)))
                Case 2
                    k = k Xor CUInt(AscW(s(start + i + 1))) << 8
                    ' 继续执行 Case 1
                    k = k Xor CUInt(AscW(s(start + i)))
                Case 1
                    k = k Xor CUInt(AscW(s(start + i)))
            End Select

            If length - i > 0 Then
                k *= c1
                k = (k << r1) Or (k >> (32 - r1))
                k *= c2
                h = h Xor k
            End If

            ' --- 最终混合 ---
            h = h Xor CUInt(length)
            h = h Xor (h >> 16)
            h *= &H85EBCA6BUI
            h = h Xor (h >> 13)
            h *= &HC2B2AE35UI
            h = h Xor (h >> 16)

            Return h
        End Function
    End Module
End Namespace
