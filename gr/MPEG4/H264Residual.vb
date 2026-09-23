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
''' H.264 残差变换系数块的 CABAC 编码（coded_block_flag + 显著性图 + 幅值 + 符号）。
''' </summary>
''' <remarks>
''' 逐字对照 ffmpeg libavcodec/h264_cabac.c 的解码流程镜像实现：
''' 
''' + <c>coded_block_flag</c>：上下文由调用方按邻居算好后传入（见 <see cref="H264VideoCodec"/>）
''' + 显著性图 <c>significant_coeff_flag</c> / <c>last_significant_coeff_flag</c>（h264_cabac.c:1669-1704）：
'''   对扫描序 <c>i = 0 .. maxCoeff-2</c> 逐位编码，<b>扫描序最后一个位置恒为显著、不编 sig/last</b>
''' + 幅值 <c>coeff_abs_level_minus1</c>（h264_cabac.c:1722-1763）：
'''   前缀 unary（上限到 <c>coeff_abs = 15</c>）用上下文，<c>&gt;= 15</c> 转 bypass 后缀
'''   <c>coeff_abs = 14 + 2^j + (j 位二进制)</c>，其中 <c>j</c> 为后置 unary 的 1 的个数（上限 23）
''' + 符号：1 个 bypass 位，位为 1 表示非负（h264_cabac.c 的 <c>get_cabac_bypass_sign(CC, -coeff_abs)</c>）
''' 
''' 三张偏移表按帧宏块取第 0 行、cat 0..4 依次为：
''' sig <c>105,120,134,149,152</c>、last <c>166,181,195,210,213</c>、abs <c>227,237,247,257,266</c>。
''' </remarks>
Friend NotInheritable Class H264Residual

    ''' <summary>
    ''' coded_block_flag 的上下文基址（cat 0..4：luma DC / luma AC / luma 4x4 / chroma DC / chroma AC）
    ''' </summary>
    Friend Shared ReadOnly cbfBaseCtx As Integer() = {85, 89, 93, 97, 101}

    ''' <summary>significant_coeff_flag 的上下文基址（帧宏块）</summary>
    Friend Shared ReadOnly significantBase As Integer() = {105, 120, 134, 149, 152}

    ''' <summary>last_significant_coeff_flag 的上下文基址（帧宏块）</summary>
    Friend Shared ReadOnly lastBase As Integer() = {166, 181, 195, 210, 213}

    ''' <summary>coeff_abs_level_minus1 的上下文基址</summary>
    Friend Shared ReadOnly absLevelBase As Integer() = {227, 237, 247, 257, 266}

    ''' <summary>cat 0 - 4 对应的最大系数个数：luma DC 16、luma AC 15、luma 4x4 16、chroma DC 4、chroma AC 15</summary>
    Friend Shared ReadOnly maxCoeffs As Integer() = {16, 15, 16, 4, 15}

    Private Shared ReadOnly level1Ctx As Integer() = {1, 2, 3, 4, 0, 0, 0, 0}
    Private Shared ReadOnly levelGt1Ctx As Integer() = {5, 5, 5, 5, 6, 7, 8, 9}
    Private Shared ReadOnly levelTransAfterOne As Integer() = {1, 2, 3, 3, 4, 5, 6, 7}
    Private Shared ReadOnly levelTransAfterGtOne As Integer() = {4, 4, 4, 4, 5, 6, 7, 7}

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 4x4 块的扫描顺序（同时用于 luma DC 块）
    ''' </summary>
    Friend Shared ReadOnly zigZag As Integer() = H264Transform.zigZag

    ''' <summary>
    ''' luma AC / chroma AC 使用的扫描顺序（跳过 DC 位置）
    ''' </summary>
    Friend Shared ReadOnly acScan As Integer() = {
        H264Transform.zigZag(1), H264Transform.zigZag(2), H264Transform.zigZag(3),
        H264Transform.zigZag(4), H264Transform.zigZag(5), H264Transform.zigZag(6),
        H264Transform.zigZag(7), H264Transform.zigZag(8), H264Transform.zigZag(9),
        H264Transform.zigZag(10), H264Transform.zigZag(11), H264Transform.zigZag(12),
        H264Transform.zigZag(13), H264Transform.zigZag(14), H264Transform.zigZag(15)
    }

    ''' <summary>
    ''' 色度 DC 的扫描顺序：逻辑上的 (0,0), (1,0), (0,1), (1,1)
    ''' </summary>
    Friend Shared ReadOnly chromaDcScan As Integer() = {0, 1, 2, 3}

    ''' <summary>
    ''' 写入一个系数块，返回该块的非零系数个数（即解码器的 nnz）。
    ''' </summary>
    ''' <param name="cabac">算术编码器</param>
    ''' <param name="levels">按光栅顺序存放的量化系数，长度至少 16</param>
    ''' <param name="cat">0 - 4 的块类别，决定三类上下文基址</param>
    ''' <param name="scan">扫描顺序表：scan(i) 给出扫描序 i 对应的光栅下标</param>
    ''' <param name="cbfCtx">coded_block_flag 的上下文（由邻居信息算出）</param>
    Friend Shared Function writeBlock(cabac As H264Cabac,
                                      levels As Integer(),
                                      cat As Integer,
                                      scan As Integer(),
                                      cbfCtx As Integer) As Integer

        Dim maxCoeff As Integer = maxCoeffs(cat)

        ' 统计非零系数以及最后一个非零系数在扫描序中的位置
        Dim lastScan As Integer = -1
        Dim coeffCount As Integer = 0

        For i As Integer = 0 To maxCoeff - 1
            If levels(scan(i)) <> 0 Then
                lastScan = i
                coeffCount += 1
            End If
        Next

        Call cabac.encodeBin(cbfCtx, If(coeffCount > 0, 1, 0))

        If coeffCount = 0 Then
            Return 0
        End If

        ' 注意：局部变量不要与同名的上下文基址表重名，否则会在初始化表达式里自我遮蔽
        Dim sigBaseOffset As Integer = significantBase(cat)
        Dim lastBaseOffset As Integer = lastBase(cat)

        ' 显著性图：扫描序最后一个位置（maxCoeff-1）恒为显著，不需要编码任何位
        For i As Integer = 0 To maxCoeff - 2
            Dim significant As Boolean = levels(scan(i)) <> 0

            Call cabac.encodeBin(sigBaseOffset + i, If(significant, 1, 0))

            If significant Then
                Dim isLast As Boolean = (i = lastScan)

                Call cabac.encodeBin(lastBaseOffset + i, If(isLast, 1, 0))

                If isLast Then Exit For
            End If
        Next

        ' 幅值与符号：按扫描序<b>倒序</b>编码
        Dim nodeCtx As Integer = 0
        Dim absBase As Integer = absLevelBase(cat)

        For i As Integer = lastScan To 0 Step -1
            Dim level As Integer = levels(scan(i))

            If level = 0 Then Continue For

            Dim magnitude As Integer = Math.Abs(level)
            Dim ctx As Integer = absBase + level1Ctx(nodeCtx)

            If magnitude = 1 Then
                Call cabac.encodeBin(ctx, 0)
                nodeCtx = levelTransAfterOne(nodeCtx)
            Else
                Call cabac.encodeBin(ctx, 1)

                ' 注意：level > 1 的上下文要用<b>更新前</b>的 node_ctx 计算
                ctx = absBase + levelGt1Ctx(nodeCtx)
                nodeCtx = levelTransAfterGtOne(nodeCtx)

                Dim prefixTop As Integer = Math.Min(magnitude, 15)

                For a As Integer = 3 To prefixTop
                    Call cabac.encodeBin(ctx, 1)
                Next

                If magnitude < 15 Then
                    Call cabac.encodeBin(ctx, 0)
                Else
                    Call writeCoeffSuffix(cabac, magnitude - 14)
                End If
            End If

            Call cabac.encodeBypassSign(level)
        Next

        Return coeffCount
    End Function

    ''' <summary>
    ''' 幅值 &gt;= 15 时的 bypass 后缀：<c>coeff_abs = 14 + 2^j + (j 位二进制)</c>
    ''' </summary>
    Private Shared Sub writeCoeffSuffix(cabac As H264Cabac, suffix As Integer)
        Dim j As Integer = 0

        While (1 << (j + 1)) <= suffix
            j += 1
        End While

        For k As Integer = 1 To j
            Call cabac.encodeBypass(1)
        Next

        Call cabac.encodeBypass(0)

        For k As Integer = j - 1 To 0 Step -1
            Call cabac.encodeBypass((suffix >> k) And 1)
        Next
    End Sub

End Class
