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
    Friend Shared Sub quantize(coeff As Integer(), levels As Integer(), qp As Integer, Optional intra As Boolean = True)
        Dim qbits As Integer = 15 + (qp \ 6)
        Dim f As Integer = If(intra, (1 << qbits) \ 3, (1 << qbits) \ 6)
        Dim scale As Integer() = normAdjust(qp Mod 6)

        For i As Integer = 0 To 15
            Dim c As Integer = coeff(i)
            Dim magnitude As Integer = If(c < 0, -c, c)
            Dim level As Integer = (magnitude * scale(positionClass(i)) + f) >> qbits

            levels(i) = If(c < 0, -level, level)
        Next
    End Sub

    ''' <summary>
    ''' 反量化：把量化系数还原为变换系数（供重建帧使用）
    ''' </summary>
    ''' <remarks>
    ''' 逆标度表由正向表推导：<c>invScale = round(2^17 / normAdjust)</c>。
    ''' 推导依据是"正向变换的 DC 增益为 16、解码器逆变换的 DC 增益为 1/64"这一对关系：
    ''' 
    ''' 平坦块 <c>a</c> 经 4x4 正向变换得到 <c>W = 16a</c>，量化后
    ''' <c>level = 16a * normAdjust / 2^(15 + qp\6)</c>；反量化再乘以
    ''' <c>2^17 / normAdjust</c> 就恰好得到 <c>64a</c>，逆变换的 <c>&gt;&gt; 6</c> 后精确还原为 <c>a</c>。
    ''' 这样正向、反向与解码器（ffmpeg <c>ff_h264_idct_add</c>）三者严格自洽，重建不会产生漂移。
    ''' </remarks>
    Friend Shared Sub dequantize(levels As Integer(), coeff As Integer(), qp As Integer)
        Dim shift As Integer = qp \ 6
        Dim inverse As Integer() = inverseNormAdjust(qp Mod 6)

        For i As Integer = 0 To 15
            Dim v As Long = CLng(levels(i)) * inverse(positionClass(i))

            If shift > 0 Then v <<= shift

            If v > 32767L Then v = 32767L
            If v < -32768L Then v = -32768L

            coeff(i) = CInt(v)
        Next
    End Sub

    ''' <summary>
    ''' 由正向标度表推导出的逆向标度表：<c>round(2^17 / normAdjust)</c>
    ''' </summary>
    Private Shared ReadOnly inverseNormAdjust As Integer()() = buildInverseNormAdjust()

    Private Shared Function buildInverseNormAdjust() As Integer()()
        Dim table As Integer()() = New Integer(5)() {}

        For q As Integer = 0 To 5
            Dim row As Integer() = New Integer(2) {}

            For c As Integer = 0 To 2
                row(c) = (131072 + normAdjust(q)(c) \ 2) \ normAdjust(q)(c)
            Next

            table(q) = row
        Next

        Return table
    End Function

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

            ' 归一化 ÷16：Hadamard 的 DC 增益为 16，输出需要落到"4x4 逆变换所需的 DC 系数"这一量纲上
            dst(i) = (s0 + s1 + 8) >> 4
            dst(4 + i) = (d0 + d1 + 8) >> 4
            dst(8 + i) = (s0 - s1 + 8) >> 4
            dst(12 + i) = (d0 - d1 + 8) >> 4
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

        ' 归一化 ÷4：2x2 Hadamard 的 DC 增益为 4（与亮度 DC 的 16 不同），
        ' 归一化后同样落到"4x4 逆变换所需的 DC 系数"量纲上
        dst(0) = (a + b + c + d + 2) >> 2
        dst(1) = (a - b + c - d + 2) >> 2
        dst(2) = (a + b - c - d + 2) >> 2
        dst(3) = (a - b - c + d + 2) >> 2
    End Sub

#End Region

    ''' <summary>
    ''' DC 系数（Hadamard 的输出）的量化：所有位置统一使用 (0,0) 位置的标度
    ''' </summary>
    Friend Shared Sub quantizeDc(coeff As Integer(), levels As Integer(), count As Integer, qp As Integer, intra As Boolean)
        Dim qbits As Integer = 15 + (qp \ 6)
        Dim f As Integer = If(intra, (1 << qbits) \ 3, (1 << qbits) \ 6)
        Dim scale As Integer = normAdjust(qp Mod 6)(0)

        For i As Integer = 0 To count - 1
            Dim c As Integer = coeff(i)
            Dim magnitude As Integer = If(c < 0, -c, c)
            Dim level As Integer = (magnitude * scale + f) >> qbits

            levels(i) = If(c < 0, -level, level)
        Next
    End Sub

    ''' <summary>
    ''' DC 系数的反量化（供重建使用），与 <see cref="quantizeDc"/> 严格配对
    ''' </summary>
    Friend Shared Sub dequantizeDc(levels As Integer(), coeff As Integer(), count As Integer, qp As Integer)
        Dim shift As Integer = qp \ 6
        Dim inverse As Integer = inverseNormAdjust(qp Mod 6)(0)

        For i As Integer = 0 To count - 1
            Dim v As Long = CLng(levels(i)) * inverse

            If shift > 0 Then v <<= shift
            If v > 32767L Then v = 32767L
            If v < -32768L Then v = -32768L

            coeff(i) = CInt(v)
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
