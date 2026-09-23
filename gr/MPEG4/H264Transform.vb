' Author:
' 
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

''' <summary>
''' H.264 的 4x4 整数变换、量化与反量化，以及 I_16x16 亮度 DC / 色度 DC 的 Hadamard 变换。
''' </summary>
''' <remarks>
''' 变换矩阵为 <c>[1 1 1 1; 2 1 -1 -2; 1 -1 -1 1; 1 -2 2 -1]</c>（规范只规定逆变换，
''' 正向变换按同一矩阵的蝶形来实现）。
''' 
''' 量化使用规范 Table 8-4 的 <c>normAdjust4x4</c>（按位置分三类：
''' 奇数行奇数列、奇数行偶数列或偶数行奇数列、偶数行偶数列），
''' 正反两向严格配对：
''' 
''' + 正向：<c>level = (|W| * normAdjust + f) >> (15 + qp \ 6)</c>，其中 intra 的
'''   <c>f = (1 &lt;&lt; qbits) \ 3</c>
''' + 反向：<c>d = ((level * normAdjust) &lt;&lt; (qp \ 6)) &gt;&gt; 4</c>，并按规范钳制到 ±32768
''' 
''' 反向量化只在编码器内部的重建帧上使用（生成后续帧的参考画面）。
''' </remarks>
Friend NotInheritable Class H264Transform

    ''' <summary>4x4 的之字形扫描顺序</summary>
    Friend Shared ReadOnly zigZag As Integer() = {
        0, 1, 4, 8, 5, 2, 3, 6,
        9, 12, 13, 10, 7, 11, 14, 15
    }

    ''' <summary>
    ''' normAdjust4x4[qp Mod 6][位置类别]（规范 Table 8-4）
    ''' 类别 0：行列下标同奇偶；类别 1：行列下标都偏奇；类别 2：其余
    ''' </summary>
    Private Shared ReadOnly normAdjust As Integer()() = {
        New Integer() {13107, 5243, 8066},
        New Integer() {11916, 4660, 7490},
        New Integer() {10082, 4194, 6554},
        New Integer() {9362, 3647, 5825},
        New Integer() {8192, 3355, 5243},
        New Integer() {7282, 2893, 4559}
    }

    ''' <summary>
    ''' 位置类别：行、列下标之和为偶数为类别 0，同为奇数为类别 1，其余为类别 2
    ''' </summary>
    Private Shared ReadOnly positionClass As Integer() = buildPositionClass()

    Private Shared Function buildPositionClass() As Integer()
        Dim table As Integer() = New Integer(15) {}

        For i As Integer = 0 To 3
            For j As Integer = 0 To 3
                Dim parity As Integer = (i And 1) + (j And 1)

                table(i * 4 + j) = If(parity = 0, 0, If(parity = 2, 1, 2))
            Next
        Next

        Return table
    End Function

#Region "4x4 变换"

    ''' <summary>
    ''' 4x4 正向变换（不含归一化，输出为变换系数 W）
    ''' </summary>
    Friend Shared Sub forwardTransform(src As Integer(), dst As Integer())
        Dim tmp As Integer() = tmpBuffer

        ' 先做行变换
        For i As Integer = 0 To 3
            Dim o As Integer = i * 4
            Dim t0 As Integer = src(o) + src(o + 3)
            Dim t1 As Integer = src(o + 1) + src(o + 2)
            Dim t2 As Integer = src(o + 1) - src(o + 2)
            Dim t3 As Integer = src(o) - src(o + 3)

            tmp(o) = t0 + t1
            tmp(o + 1) = 2 * t3 + t2
            tmp(o + 2) = t0 - t1
            tmp(o + 3) = t3 - 2 * t2
        Next

        ' 再做列变换
        For i As Integer = 0 To 3
            Dim t0 As Integer = tmp(i) + tmp(12 + i)
            Dim t1 As Integer = tmp(4 + i) + tmp(8 + i)
            Dim t2 As Integer = tmp(4 + i) - tmp(8 + i)
            Dim t3 As Integer = tmp(i) - tmp(12 + i)

            dst(i) = t0 + t1
            dst(4 + i) = 2 * t3 + t2
            dst(8 + i) = t0 - t1
            dst(12 + i) = t3 - 2 * t2
        Next
    End Sub

    ''' <summary>
    ''' 4x4 逆向变换（输入的系数已经完成反量化，输出残差样点）
    ''' </summary>
    Friend Shared Sub inverseTransform(src As Integer(), dst As Integer())
        Dim tmp As Integer() = tmpBuffer

        For i As Integer = 0 To 3
            Dim o As Integer = i * 4
            Dim t0 As Integer = src(o) + src(o + 2)
            Dim t1 As Integer = src(o) - src(o + 2)
            Dim t2 As Integer = (src(o + 1) >> 1) - src(o + 3)

            tmp(o) = t0 + src(o + 1) + t2
            tmp(o + 1) = t0 - src(o + 1) - t2
            tmp(o + 2) = t1 + src(o + 3) + (src(o + 1) >> 1)
            tmp(o + 3) = t1 - src(o + 3) - (src(o + 1) >> 1)
        Next

        For i As Integer = 0 To 3
            Dim t0 As Integer = tmp(i) + tmp(8 + i)
            Dim t1 As Integer = tmp(i) - tmp(8 + i)
            Dim t2 As Integer = (tmp(4 + i) >> 1) - tmp(12 + i)

            dst(i) = (t0 + tmp(4 + i) + t2 + 32) >> 6
            dst(4 + i) = (t0 - tmp(4 + i) - t2 + 32) >> 6
            dst(8 + i) = (t1 + tmp(12 + i) + (tmp(4 + i) >> 1) + 32) >> 6
            dst(12 + i) = (t1 - tmp(12 + i) - (tmp(4 + i) >> 1) + 32) >> 6
        Next
    End Sub

    ''' <summary>
    ''' 量化：把变换系数 W 量化为 level
    ''' </summary>
    ''' <param name="coeff">变换系数（长度 16）</param>
    ''' <param name="levels">量化输出（长度 16）</param>
    ''' <param name="qp">量化参数</param>
    ''' <param name="intra">是否按帧内编码的舍入偏移</param>
    ''' <summary>
    ''' 反量化基数表（ffmpeg <c>ff_h264_dequant4_coeff_init</c>，h264data.c:152），
    ''' 行按 <c>qp Mod 6</c>、列按位置类别索引
    ''' </summary>
    Private Shared ReadOnly dequantInit As Integer()() = {
        New Integer() {10, 13, 16},
        New Integer() {11, 14, 18},
        New Integer() {13, 16, 20},
        New Integer() {14, 18, 23},
        New Integer() {16, 20, 25},
        New Integer() {18, 23, 29}
    }

    ''' <summary>
    ''' 4x4 块内的位置类别，与 ffmpeg 的 <c>(x &amp; 1) + ((x &gt;&gt; 2) &amp; 1)</c> 完全一致
    ''' （x 为块内光栅下标；该表达式对行列转置对称，因此 ffmpeg 存储时做的转置不影响取到的类别）
    ''' </summary>
    Friend Shared Function quantClass(i As Integer) As Integer
        Return (i And 1) + ((i >> 2) And 1)
    End Function

    ''' <summary>
    ''' 亮度 DC 的 Hadamard 输出位置到宏块内 4x4 块序号的映射
    ''' （ffmpeg <c>h264_mb.c:712</c> 的 <c>dc_mapping</c>，恰为一组对换，故自逆）
    ''' </summary>
    Friend Shared ReadOnly lumaDcMapping As Integer() = {
        0, 1, 4, 5, 2, 3, 6, 7,
        8, 9, 12, 13, 10, 11, 14, 15
    }

    ''' <summary>
    ''' 解码器使用的反量化系数：<c>level * qmul</c> 即为逆变换（带 <c>&gt;&gt; 6</c>）的输入
    ''' </summary>
    Friend Shared Function qmul(qp As Integer, i As Integer) As Integer
        Return dequantInit(qp Mod 6)(quantClass(i)) << (qp \ 6 + 2)
    End Function

    ''' <summary>
    ''' 量化：正向变换与解码器逆变换的精确逆运算
    ''' </summary>
    ''' <remarks>
    ''' 解码器对每个系数执行 <c>d = level * qmul</c>，随后逆变换整体 <c>&gt;&gt; 6</c>。
    ''' 由于 4x4 正向变换对平坦块的 DC 增益为 16，要让逆变换还原出残差就需要
    ''' <c>level * qmul = 64 * W</c>，即 <c>level = 4 * W / qmul</c>；
    ''' 这里用 <c>scale = 4 * 2^qbits / qmul</c> 与常量舍入偏移 <c>f</c> 实现该除法。
    ''' </remarks>
    Friend Shared Sub quantize(coeff As Integer(), levels As Integer(), qp As Integer, Optional intra As Boolean = True)
        Dim qbits As Integer = 15 + (qp \ 6)
        Dim f As Integer = If(intra, (1 << qbits) \ 3, (1 << qbits) \ 6)
        Dim numerator As Integer = 4 << qbits

        For i As Integer = 0 To 15
            Dim c As Integer = coeff(i)
            Dim magnitude As Integer = If(c < 0, -c, c)
            Dim scale As Integer = numerator \ qmul(qp, i)
            Dim level As Integer = (magnitude * scale + f) >> qbits

            levels(i) = If(c < 0, -level, level)
        Next
    End Sub

    ''' <summary>
    ''' 反量化：严格镜像解码器的 <c>d = level * qmul</c>（时长整数按 16 位钳制，与解码器一致）
    ''' </summary>
    Friend Shared Sub dequantize(levels As Integer(), coeff As Integer(), qp As Integer)
        For i As Integer = 0 To 15
            Dim v As Long = CLng(levels(i)) * qmul(qp, i)

            If v > 32767L Then v = 32767L
            If v < -32768L Then v = -32768L

            coeff(i) = CInt(v)
        Next
    End Sub

#End Region

#Region "DC 变换"

    ''' <summary>
    ''' I_16x16 亮度 DC 的 4x4 Hadamard 正向变换
    ''' </summary>
    Friend Shared Sub forwardLumaDcTransform(src As Integer(), dst As Integer())
        Dim tmp As Integer() = tmpBuffer

        For i As Integer = 0 To 3
            Dim o As Integer = i * 4
            Dim s0 As Integer = src(o) + src(o + 2)
            Dim s1 As Integer = src(o + 1) + src(o + 3)
            Dim d0 As Integer = src(o) - src(o + 2)
            Dim d1 As Integer = src(o + 1) - src(o + 3)

            tmp(o) = s0 + s1
            tmp(o + 1) = d0 + d1
            tmp(o + 2) = s0 - s1
            tmp(o + 3) = d0 - d1
        Next

        For i As Integer = 0 To 3
            Dim s0 As Integer = tmp(i) + tmp(8 + i)
            Dim s1 As Integer = tmp(4 + i) + tmp(12 + i)
            Dim d0 As Integer = tmp(i) - tmp(8 + i)
            Dim d1 As Integer = tmp(4 + i) - tmp(12 + i)

            dst(i) = s0 + s1
            dst(4 + i) = d0 + d1
            dst(8 + i) = s0 - s1
            dst(12 + i) = d0 - d1
        Next
    End Sub

    ''' <summary>
    ''' I_16x16 亮度 DC 的 4x4 Hadamard 逆向变换（输出的样点为 DC 值，后续按 (x+32)&gt;&gt;6 还原）
    ''' </summary>
    Friend Shared Sub inverseLumaDcTransform(src As Integer(), dst As Integer())
        Dim tmp As Integer() = tmpBuffer

        For i As Integer = 0 To 3
            Dim o As Integer = i * 4
            Dim s0 As Integer = src(o) + src(o + 2)
            Dim s1 As Integer = src(o + 1) + src(o + 3)
            Dim d0 As Integer = src(o) - src(o + 2)
            Dim d1 As Integer = src(o + 1) - src(o + 3)

            tmp(o) = s0 + s1
            tmp(o + 1) = d0 + d1
            tmp(o + 2) = s0 - s1
            tmp(o + 3) = d0 - d1
        Next

        For i As Integer = 0 To 3
            Dim s0 As Integer = tmp(i) + tmp(8 + i)
            Dim s1 As Integer = tmp(4 + i) + tmp(12 + i)
            Dim d0 As Integer = tmp(i) - tmp(8 + i)
            Dim d1 As Integer = tmp(4 + i) - tmp(12 + i)

            ' 镜像解码器 ff_h264_luma_dc_dequant_idct：Hadamard【不做归一化】，
            ' 随后 (x + 128) >> 8 落在"×64 域"上（该值再经逆变换的 >> 6 得到残差 DC），
            ' 并按 dc_mapping 把结果分发到宏块内的 4x4 块。
            dst(lumaDcMapping(i)) = (s0 + s1 + 128) >> 8
            dst(lumaDcMapping(4 + i)) = (d0 + d1 + 128) >> 8
            dst(lumaDcMapping(8 + i)) = (s0 - s1 + 128) >> 8
            dst(lumaDcMapping(12 + i)) = (d0 - d1 + 128) >> 8
        Next
    End Sub

    ''' <summary>
    ''' 色度 DC 的 2x2 Hadamard 正向变换
    ''' </summary>
    Friend Shared Sub forwardChromaDcTransform(a As Integer, b As Integer, c As Integer, d As Integer, dst As Integer())
        dst(0) = a + b + c + d
        dst(1) = a - b + c - d
        dst(2) = a + b - c - d
        dst(3) = a - b - c + d
    End Sub

    ''' <summary>
    ''' 色度 DC 的 2x2 Hadamard 逆向变换
    ''' </summary>
    Friend Shared Sub inverseChromaDcTransform(src As Integer(), dst As Integer())
        Dim a As Integer = src(0)
        Dim b As Integer = src(1)
        Dim c As Integer = src(2)
        Dim d As Integer = src(3)

        ' 镜像解码器 ff_h264_chroma_dc_dequant_idct：2x2 Hadamard【不归一化】，随后 >> 7
        dst(0) = (a + b + c + d) >> 7
        dst(1) = (a - b + c - d) >> 7
        dst(2) = (a + b - c - d) >> 7
        dst(3) = (a - b - c + d) >> 7
    End Sub

#End Region

    ''' <summary>
    ''' DC 系数（Hadamard 的输出）的量化：所有位置统一使用 (0,0) 位置的标度
    ''' </summary>
    Friend Shared Sub quantizeDc(coeff As Integer(), levels As Integer(), count As Integer, qp As Integer, intra As Boolean)
        ' 解码器对 DC 的逆变换是【不归一化】的 Hadamard 后接 (x*qmul + 128) >> 8（亮度）
        ' 或 (x*qmul) >> 7（色度），最终经 >> 6 得到残差 DC。由此反解出前向应有的倍数：
        '   亮度：levels = 16 * H(每块 DC 的量化值)
        '   色度：levels = 32 * X(每块 DC 的量化值)
        ' 这两个倍数与 qmul 无关，因此 DC 通路在第二级是精确无损的（量化误差只来自第一级）。
        Dim k As Integer = If(count = 16, 16, 32)

        For i As Integer = 0 To count - 1
            levels(i) = coeff(i) * k
        Next
    End Sub

    ''' <summary>
    ''' DC 系数的反量化：与解码器一致，乘以 DC 位置（类别 0）的 <c>qmul</c>
    ''' </summary>
    Friend Shared Sub dequantizeDc(levels As Integer(), coeff As Integer(), count As Integer, qp As Integer)
        Dim scale As Integer = qmul(qp, 0)

        ' 注意：这里【不能】做 16 位钳位。解码器的 DC 通路（ff_h264_luma_dc_dequant_idct /
        ' ff_h264_chroma_dc_dequant_idct）全程用 int 计算，只有 level 本身存成 int16；
        ' 而 AC 通路才是 int16_t block[i] = src[i] * qmul[i]（因此 AC 侧保留钳位）。
        For i As Integer = 0 To count - 1
            coeff(i) = CInt(CLng(levels(i)) * scale)
        Next
    End Sub

    ' 变换使用的临时缓冲（每帧复用，避免逐块分配）
    <ThreadStatic>
    Private Shared tmpStorage As Integer()

    Private Shared ReadOnly Property tmpBuffer As Integer()
        Get
            If tmpStorage Is Nothing Then tmpStorage = New Integer(15) {}
            Return tmpStorage
        End Get
    End Property

End Class
