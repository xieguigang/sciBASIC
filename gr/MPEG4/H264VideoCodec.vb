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
''' 一个宏块在解码器中留下的邻接状态：后续宏块推导上下文时要用到。
''' </summary>
''' <remarks>
''' 三样东西必须与解码器完全一致，否则 CABAC 上下文会跑偏导致码流无法解码：
''' 
''' + <c>cbp</c>：完整编码块模式，其中 bit8 表示亮度 DC 块是否有系数、bit6/bit7 表示色度 Cb/Cr 的 DC 块是否有系数
'''   （ffmpeg 的 <c>get_cabac_cbf_ctx</c> 直接用 <c>left_cbp</c>/<c>top_cbp</c> 的这几位推导 DC 块的 coded_block_flag 上下文）
''' + <c>lumaAcNnz</c> / <c>chromaAcNnz</c>：每个 4x4 块的非零系数个数，AC 块只用它的"是否大于 0"
''' </remarks>
Friend Class H264MbInfo

    ''' <summary>完整 cbp（含三个 DC 标志位）</summary>
    Friend Property cbp As Integer

    ''' <summary>16 个亮度 AC 4x4 块的非零系数个数，按空间顺序 by*4+bx</summary>
    Friend ReadOnly Property lumaAcNnz As Integer() = New Integer(15) {}

    ''' <summary>两个色度平面各 4 个 AC 4x4 块的非零系数个数</summary>
    Friend ReadOnly Property chromaAcNnz As Integer()() = {New Integer(3) {}, New Integer(3) {}}

End Class

''' <summary>
''' H.264/AVC 视频编码器：把 RGBA 帧编码为 AVCC 格式的采样数据。
''' </summary>
''' <remarks>
''' 当前实现的范围（与计划的阶段 2 对应）：
''' 
''' + **仅 I 帧**，宏块统一使用 <b>I_16x16</b>（含 4 种亮度预测模式的选择）
''' + CABAC 熵编码、Main profile、4:2:0、8bit、关闭去环滤波
''' + 色度预测模式目前固定为 DC（0）——这样它的上下文恒为 0，规避 ffmpeg 对
'''   <c>chroma_pred_mode_table</c> 的特殊处理带来的歧义；后续版本再放开
''' + 亮度/色度 CBP 取最大值（15 / 2），即所有 AC 块都参与编码，压缩率不是最优但语法最简单
''' 
''' 关键实现要点（全部逐字对照 ffmpeg 解码器实现）：
''' 
''' + mb_type 的 bin 顺序：<c>首bin(3+ctx) → terminate → cbp_luma(6) → cbp_chroma(7) → chroma==2(8) → 模式高位(9) → 模式低位(10)</c>，
'''   其中 I_16x16 的 <c>mb_type = 1 + Intra16x16PredMode + 4*CodedBlockPatternChroma + 12*CodedBlockPatternLuma</c>
''' + I_16x16 的色度 CBP 与亮度 CBP <b>都隐含在 mb_type 里</b>，不能再编 CBPY/CBC 语法
''' + 重建帧既是后续宏块帧内预测的参考，也是后续 P 帧的参考，因此必须与解码器逐样点一致
''' </remarks>
Friend Class H264VideoCodec

    ''' <summary>亮度 CBP：I_16x16 下只能取 0 或 15，这里恒取 15</summary>
    Private Const LUMA_CBP As Integer = 15

    ''' <summary>色度 CBP：0 = 无、1 = 仅 DC、2 = DC + AC</summary>
    Private Const CHROMA_CBP As Integer = 2

    Friend ReadOnly Property codec As H264SpsPps

    Private ReadOnly frameWidth As Integer
    Private ReadOnly frameHeight As Integer
    Private ReadOnly qp As Integer

    Private frameIndex As Integer
    Private reference As H264Yuv420

    ' ---- 复用的临时缓冲：避免逐宏块/逐块分配造成 GC 抖动 ----
    Private ReadOnly predLuma As Integer() = New Integer(255) {}
    Private ReadOnly predChroma As Integer()() = {New Integer(63) {}, New Integer(63) {}}
    Private ReadOnly resBlock As Integer() = New Integer(15) {}
    Private ReadOnly blockCoef As Integer() = New Integer(15) {}
    Private ReadOnly blockLevels As Integer() = New Integer(15) {}
    Private ReadOnly reconCoef As Integer() = New Integer(15) {}
    Private ReadOnly reconRes As Integer() = New Integer(15) {}
    Private ReadOnly lumaAcLevels As Integer()() = newLumaAcLevels()
    Private ReadOnly chromaAcLevels As Integer()()() = newChromaAcLevels()
    Private ReadOnly lumaDcIn As Integer() = New Integer(15) {}
    Private ReadOnly lumaDcCoeff As Integer() = New Integer(15) {}
    Private ReadOnly lumaDcLevels As Integer() = New Integer(15) {}
    Private ReadOnly lumaDcOut As Integer() = New Integer(15) {}
    Private ReadOnly dcMap As Integer() = New Integer(15) {}
    Private ReadOnly chromaDcIn As Integer()() = {New Integer(3) {}, New Integer(3) {}}
    Private ReadOnly chromaDcCoeff As Integer() = New Integer(3) {}
    Private ReadOnly chromaDcLevels As Integer()() = {New Integer(3) {}, New Integer(3) {}}
    Private ReadOnly chromaDcOut As Integer() = New Integer(3) {}

    Private Shared Function newLumaAcLevels() As Integer()()
        Dim blocks As Integer()() = New Integer(15)() {}

        For i As Integer = 0 To 15
            blocks(i) = New Integer(15) {}
        Next

        Return blocks
    End Function

    Private Shared Function newChromaAcLevels() As Integer()()()
        Dim planes As Integer()()() = New Integer(1)()() {}

        For c As Integer = 0 To 1
            Dim blocks As Integer()() = New Integer(3)() {}

            For i As Integer = 0 To 3
                blocks(i) = New Integer(15) {}
            Next

            planes(c) = blocks
        Next

        Return planes
    End Function

    Friend Sub New(width As Integer, height As Integer, fps As Integer, quality As Integer)
        Me.frameWidth = width
        Me.frameHeight = height
        Me.qp = mapQuality(quality)
        Me.codec = New H264SpsPps(width, height)
    End Sub

    ''' <summary>
    ''' 画质（0 - 100）映射为量化参数。
    ''' </summary>
    ''' <remarks>
    ''' 结果被限制在 1 - 29：PPS 的 <c>chroma_qp_index_offset = 0</c> 时，
    ''' QP 不超过 29 可以让色度 QP 与亮度 QP 保持恒等映射，避免依赖色度 QP 映射表。
    ''' </remarks>
    Private Shared Function mapQuality(quality As Integer) As Integer
        Dim q As Integer = If(quality < 0, 0, If(quality > 100, 100, quality))
        Dim value As Integer = 1 + (100 - q) * 50 \ 100

        Return Math.Max(1, Math.Min(29, value))
    End Function

    ''' <summary>
    ''' 编码一帧，返回 AVCC 格式（4 字节大端长度前缀）的采样数据。
    ''' </summary>
    ''' <param name="bgra">扁平的 BGRA 像素字节</param>
    ''' <param name="pixelWidth">像素行的实际宽度（可能大于可见宽度）</param>
    Friend Function encodeFrame(bgra As Byte(), pixelWidth As Integer) As Byte()
        Dim source As New H264Yuv420(codec.codedWidth, codec.codedHeight, frameWidth, frameHeight)

        Call source.loadBgra(bgra, pixelWidth)

        Dim recon As H264Yuv420 = source.clone()
        Dim isIdr As Boolean = (frameIndex = 0)
        Dim bits As New BitStreamWriter(1 << 16)

        ' slice_qp_delta 是相对 PPS 的 pic_init_qp_minus26(= 0) 的偏移，因此实际 QP = 26 + delta
        Call H264SliceHeader.write(bits, H264SliceType.I, isIdr,
                                   frameNum:=(frameIndex And 255),
                                   picOrderCntLsb:=((frameIndex * 2) And 255),
                                   sliceQpDelta:=(qp - 26))

        Dim cabac As New H264Cabac(bits)

        Call cabac.begin()
        Call cabac.initStates(True, 0, qp)

        Dim mbCols As Integer = codec.widthInMbs
        Dim mbRows As Integer = codec.heightInMapUnits
        Dim prevRow As H264MbInfo() = newMbInfoRow(mbCols)
        Dim curRow As H264MbInfo() = newMbInfoRow(mbCols)

        For mbY As Integer = 0 To mbRows - 1
            For mbX As Integer = 0 To mbCols - 1
                Dim isLast As Boolean = (mbY = mbRows - 1) AndAlso (mbX = mbCols - 1)

                Call encodeMacroblock(cabac, source, recon, mbX, mbY, prevRow, curRow)
                ' 每个宏块之后都要写 end_of_slice_flag，只有最后一个宏块为 1（同时完成算术编码收尾）
                Call cabac.encodeTerminate(If(isLast, 1, 0))
            Next

            ' 行滚动：当前行成为下一宏块行的"上邻"
            Dim swap As H264MbInfo() = prevRow

            prevRow = curRow
            curRow = swap
        Next

        Dim rbsp As Byte() = bits.toArray
        ' 采样中的 NAL 不带起始码，只带 NAL 头（AVCC 封装）
        Dim nal As Byte() = BitStreamWriter.nalUnit(rbsp, 3, If(isIdr, 5, 1), annexB:=False)
        Dim sample As Byte() = New Byte(nal.Length + 3) {}

        sample(0) = CByte((nal.Length >> 24) And &HFF)
        sample(1) = CByte((nal.Length >> 16) And &HFF)
        sample(2) = CByte((nal.Length >> 8) And &HFF)
        sample(3) = CByte(nal.Length And &HFF)
        Call Array.Copy(nal, 0, sample, 4, nal.Length)

        frameIndex += 1
        reference = recon

        Return sample
    End Function

    Private Shared Function newMbInfoRow(count As Integer) As H264MbInfo()
        Dim row As H264MbInfo() = New H264MbInfo(count - 1) {}

        For i As Integer = 0 To count - 1
            row(i) = New H264MbInfo
        Next

        Return row
    End Function

    ''' <summary>
    ''' 编码一个 I_16x16 宏块：预测 → 残差 → 变换量化 → CABAC 语法 → 重建
    ''' </summary>
    Private Sub encodeMacroblock(cabac As H264Cabac,
                                 source As H264Yuv420, recon As H264Yuv420,
                                 mbX As Integer, mbY As Integer,
                                 prevRow As H264MbInfo(), curRow As H264MbInfo())

        Dim x0 As Integer = mbX * 16
        Dim y0 As Integer = mbY * 16
        Dim ux As Integer = mbX * 8
        Dim uy As Integer = mbY * 8
        Dim leftAvailable As Boolean = mbX > 0
        Dim topAvailable As Boolean = mbY > 0
        Dim left As H264MbInfo = If(leftAvailable, curRow(mbX - 1), Nothing)
        Dim top As H264MbInfo = If(topAvailable, prevRow(mbX), Nothing)
        Dim info As New H264MbInfo

        ' ---- 1. 亮度 I_16x16 预测模式 ----
        Dim predMode As Integer = selectLumaMode(source, recon, x0, y0, leftAvailable, topAvailable)

        Call H264IntraPrediction.predict16x16(recon.y, x0, y0, CType(predMode, H264IntraMode),
                                              leftAvailable, topAvailable, predLuma)

        ' ---- 2. 色度预测：固定 DC 模式 ----
        Call H264IntraPrediction.predict8x8(recon.u, ux, uy, H264IntraMode.DC, leftAvailable, topAvailable, predChroma(0))
        Call H264IntraPrediction.predict8x8(recon.v, ux, uy, H264IntraMode.DC, leftAvailable, topAvailable, predChroma(1))

        ' ---- 3. 亮度残差 → 4x4 变换 → 量化；DC 单独收集 ----
        For by As Integer = 0 To 3
            For bx As Integer = 0 To 3
                Dim blockIdx As Integer = by * 4 + bx

                Call loadLumaResidual(source.y, predLuma, x0, y0, bx, by, resBlock)
                Call H264Transform.forwardTransform(resBlock, blockCoef)
                Call H264Transform.quantize(blockCoef, blockLevels, qp, True)

                lumaDcIn(blockIdx) = blockLevels(0)

                Dim ac As Integer() = lumaAcLevels(blockIdx)

                ac(0) = 0
                For i As Integer = 1 To 15
                    ac(i) = blockLevels(i)
                Next
            Next
        Next

        ' ---- 4. 亮度 DC：4x4 Hadamard → 量化（统一用 (0,0) 标度）----
        Call H264Transform.forwardLumaDcTransform(lumaDcIn, lumaDcCoeff)
        Call H264Transform.quantizeDc(lumaDcCoeff, lumaDcLevels, 16, qp, True)

        ' ---- 5. 色度残差 → 变换量化 + 2x2 Hadamard ----
        For c As Integer = 0 To 1
            Dim plane As H264Plane = If(c = 0, source.u, source.v)

            For by As Integer = 0 To 1
                For bx As Integer = 0 To 1
                    Dim blockIdx As Integer = by * 2 + bx

                    Call loadChromaResidual(plane, predChroma(c), ux, uy, bx, by, resBlock)
                    Call H264Transform.forwardTransform(resBlock, blockCoef)
                    Call H264Transform.quantize(blockCoef, blockLevels, qp, True)

                    chromaDcIn(c)(blockIdx) = blockLevels(0)

                    Dim ac As Integer() = chromaAcLevels(c)(blockIdx)

                    ac(0) = 0
                    For i As Integer = 1 To 15
                        ac(i) = blockLevels(i)
                    Next
                Next
            Next

            Call H264Transform.forwardChromaDcTransform(chromaDcIn(c)(0), chromaDcIn(c)(1), chromaDcIn(c)(2), chromaDcIn(c)(3), chromaDcCoeff)
            Call H264Transform.quantizeDc(chromaDcCoeff, chromaDcLevels(c), 4, qp, True)
        Next

        ' ---- 6. 宏块语法 ----
        ' CodedBlockPattern 必须如实反映「是否存在非零系数」（H.264 7.4.5）：
        ' 声称某块有系数、而该块 coded_block_flag 又写 0 属于非法码流，
        ' 且会让后续宏块从本方 cbp 推导的上下文与解码器不一致。
        Dim lumaCbp As Integer = 0

        For i As Integer = 0 To 15
            If hasCoeff(lumaAcLevels(i)) Then
                lumaCbp = 15
                Exit For
            End If
        Next

        Dim chromaDcPresent As Boolean = False
        Dim chromaAcPresent As Boolean = False

        For c As Integer = 0 To 1
            If hasCoeff(chromaDcLevels(c)) Then chromaDcPresent = True

            For i As Integer = 0 To 3
                If hasCoeff(chromaAcLevels(c)(i)) Then chromaAcPresent = True
            Next
        Next

        ' I_16x16 的色度 CBP 只能取 0 / 1 / 2：解码器用 (cbp And &H30) 判色度 DC、用 (cbp And &H20) 判色度 AC
        Dim chromaCbp As Integer = If(chromaAcPresent, 2, If(chromaDcPresent, 1, 0))

        Call cabac.encodeBin(3 + If(leftAvailable, 1, 0) + If(topAvailable, 1, 0), 1) ' 不是 I_4x4
        Call cabac.encodeTerminate(0)                                               ' 不是 I_PCM
        Call cabac.encodeBin(6, If(lumaCbp <> 0, 1, 0))                             ' 亮度 CBP != 0
        ' 解码器只在「色度 CBP != 0」时才继续读下一位（h264_cabac.c:1329-1330）：
        '   if (get_cabac(&state[2])) { mb_type += 4 + 4 * get_cabac(&state[2+intra_slice]); }
        ' 因此色度 CBP 为 0 时绝对不能写出 bin8，否则后面所有语法元素都会错位。
        Call cabac.encodeBin(7, If(chromaCbp <> 0, 1, 0))                           ' 色度 CBP != 0

        If chromaCbp <> 0 Then
            Call cabac.encodeBin(8, If(chromaCbp = 2, 1, 0))                        ' 色度 CBP == 2
        End If
        ' 亮度预测模式隐含在 mb_type 的低 2 位里
        Call cabac.encodeBin(9, (predMode >> 1) And 1)
        Call cabac.encodeBin(10, predMode And 1)

        ' 色度预测模式：DC（0）只需一个 0 bin；其上下文恒为 0
        Call cabac.encodeBin(64, 0)

        ' mb_qp_delta = 0：首 bin 用 ctx 60（上一个宏块的差值也是 0）
        Call cabac.encodeBin(60, 0)

        ' ---- 7. 残差系数 ----
        Dim dcCtx As Integer = H264Residual.cbfBaseCtx(0) +
            If(left IsNot Nothing AndAlso (left.cbp And &H100) <> 0, 1, 0) +
            If(top IsNot Nothing AndAlso (top.cbp And &H100) <> 0, 2, 0)

        Dim lumaDcCount As Integer = H264Residual.writeBlock(cabac, lumaDcLevels, 0, H264Residual.zigZag, dcCtx)

        ' 亮度 CBP 为 0 时 16 个 AC 块整体不参与编码（解码器同样按 (cbp And 15) 跳过）
        If lumaCbp <> 0 Then
            For i4x4 As Integer = 0 To 15
                Dim bx As Integer = ((i4x4 >> 2) And 1) * 2 + (i4x4 And 1)
                Dim by As Integer = ((i4x4 >> 3) And 1) * 2 + ((i4x4 >> 1) And 1)
                Dim ctx As Integer = H264Residual.cbfBaseCtx(1) + acNeighborCtx(left, top, info, bx, by, False, 0)

                info.lumaAcNnz(by * 4 + bx) = H264Residual.writeBlock(cabac, lumaAcLevels(by * 4 + bx), 1, H264Residual.acScan, ctx)
            Next
        End If

        ' 色度块的顺序必须与解码器一致（ffmpeg h264_cabac.c:2467-2482）：
        ' 先两个平面的 DC（Cb、Cr），再两个平面的 AC（Cb 的 4 个、Cr 的 4 个）。
        ' 若写成「Cb DC -> Cb AC -> Cr DC -> Cr AC」就会与解码器错位。
        Dim chromaDcBits As Integer = 0

        If chromaCbp <> 0 Then
            For c As Integer = 0 To 1
                Dim dcCtxChroma As Integer = H264Residual.cbfBaseCtx(3) +
                    If(left IsNot Nothing AndAlso ((left.cbp >> (6 + c)) And 1) <> 0, 1, 0) +
                    If(top IsNot Nothing AndAlso ((top.cbp >> (6 + c)) And 1) <> 0, 2, 0)

                Dim count As Integer = H264Residual.writeBlock(cabac, chromaDcLevels(c), 3, H264Residual.chromaDcScan, dcCtxChroma)

                If count > 0 Then chromaDcBits = chromaDcBits Or (1 << (6 + c))
            Next
        End If

        If chromaCbp = 2 Then
            For c As Integer = 0 To 1
                For i As Integer = 0 To 3
                    Dim cx As Integer = i And 1
                    Dim cy As Integer = (i >> 1) And 1
                    Dim ctx As Integer = H264Residual.cbfBaseCtx(4) + acNeighborCtx(left, top, info, cx, cy, True, c)

                    info.chromaAcNnz(c)(i) = H264Residual.writeBlock(cabac, chromaAcLevels(c)(i), 4, H264Residual.acScan, ctx)
                Next
            Next
        End If

        info.cbp = lumaCbp Or (chromaCbp << 4) Or chromaDcBits Or If(lumaDcCount > 0, &H100, 0)
        curRow(mbX) = info

        ' ---- 8. 重建（供后续宏块预测与后续帧参考）----
        Call reconstructLuma(recon, predLuma, x0, y0)
        Call reconstructChroma(recon, predChroma, ux, uy)
    End Sub

    ''' <summary>
    ''' 判断一个系数块里是否存在非零量化系数（用于推导 CodedBlockPattern）
    ''' </summary>
    Private Shared Function hasCoeff(levels As Integer()) As Boolean
        For i As Integer = 0 To levels.Length - 1
            If levels(i) <> 0 Then Return True
        Next

        Return False
    End Function

    ''' <summary>
    ''' 亮度残差：原始样点减去预测
    ''' </summary>
    Private Shared Sub loadLumaResidual(plane As H264Plane, pred As Integer(),
                                        x0 As Integer, y0 As Integer, bx As Integer, by As Integer,
                                        res As Integer())
        For yy As Integer = 0 To 3
            For xx As Integer = 0 To 3
                res(yy * 4 + xx) = plane.at(x0 + bx * 4 + xx, y0 + by * 4 + yy) -
                    pred((by * 4 + yy) * 16 + bx * 4 + xx)
            Next
        Next
    End Sub

    ''' <summary>
    ''' 色度残差：原始样点减去预测
    ''' </summary>
    Private Shared Sub loadChromaResidual(plane As H264Plane, pred As Integer(),
                                          ux As Integer, uy As Integer, bx As Integer, by As Integer,
                                          res As Integer())
        For yy As Integer = 0 To 3
            For xx As Integer = 0 To 3
                res(yy * 4 + xx) = plane.at(ux + bx * 4 + xx, uy + by * 4 + yy) -
                    pred((by * 4 + yy) * 8 + bx * 4 + xx)
            Next
        Next
    End Sub

    ''' <summary>
    ''' 亮度重建：DC 逆 Hadamard → 逐 4x4 反量化 + 逆变换 + 加预测
    ''' </summary>
    Private Sub reconstructLuma(recon As H264Yuv420, pred As Integer(), x0 As Integer, y0 As Integer)
        Call H264Transform.dequantizeDc(lumaDcLevels, lumaDcOut, 16, qp)
        Call H264Transform.inverseLumaDcTransform(lumaDcOut, dcMap)

        For by As Integer = 0 To 3
            For bx As Integer = 0 To 3
                Dim blockIdx As Integer = by * 4 + bx
                Dim level As Integer() = lumaAcLevels(blockIdx)

                Call Array.Copy(level, blockLevels, 16)
                Call H264Transform.dequantize(blockLevels, reconCoef, qp)

                reconCoef(0) = dcMap(blockIdx)

                Call H264Transform.inverseTransform(reconCoef, reconRes)

                For yy As Integer = 0 To 3
                    For xx As Integer = 0 To 3
                        Call recon.y.setAt(x0 + bx * 4 + xx, y0 + by * 4 + yy,
                                           pred((by * 4 + yy) * 16 + bx * 4 + xx) + reconRes(yy * 4 + xx))
                    Next
                Next
            Next
        Next
    End Sub

    ''' <summary>
    ''' 色度重建：DC 逆 2x2 Hadamard → 逐 4x4 反量化 + 逆变换 + 加预测
    ''' </summary>
    Private Sub reconstructChroma(recon As H264Yuv420, pred As Integer()(), ux As Integer, uy As Integer)
        For c As Integer = 0 To 1
            Call H264Transform.dequantizeDc(chromaDcLevels(c), chromaDcCoeff, 4, qp)
            Call H264Transform.inverseChromaDcTransform(chromaDcCoeff, chromaDcOut)

            Dim plane As H264Plane = If(c = 0, recon.u, recon.v)

            For by As Integer = 0 To 1
                For bx As Integer = 0 To 1
                    Dim blockIdx As Integer = by * 2 + bx

                    Call Array.Copy(chromaAcLevels(c)(blockIdx), blockLevels, 16)
                    Call H264Transform.dequantize(blockLevels, reconCoef, qp)

                    reconCoef(0) = chromaDcOut(blockIdx)

                    Call H264Transform.inverseTransform(reconCoef, reconRes)

                    For yy As Integer = 0 To 3
                        For xx As Integer = 0 To 3
                            Call plane.setAt(ux + bx * 4 + xx, uy + by * 4 + yy,
                                             pred(c)((by * 4 + yy) * 8 + bx * 4 + xx) + reconRes(yy * 4 + xx))
                        Next
                    Next
                Next
            Next
        Next
    End Sub

    ''' <summary>
    ''' AC 块的 coded_block_flag 上下文：左邻与上邻的非零标志
    ''' </summary>
    ''' <param name="left">左邻宏块信息</param>
    ''' <param name="top">上邻宏块信息</param>
    ''' <param name="info">当前宏块信息（正在填写）</param>
    ''' <param name="bx">块在本宏块内的横向位置（亮度 0..3、色度 0..1）</param>
    ''' <param name="by">块在本宏块内的纵向位置</param>
    ''' <param name="chroma">是否为色度 AC 块</param>
    ''' <param name="plane">色度平面（0 = Cb、1 = Cr）</param>
    Private Shared Function acNeighborCtx(left As H264MbInfo, top As H264MbInfo,
                                          info As H264MbInfo, bx As Integer, by As Integer,
                                          chroma As Boolean, plane As Integer) As Integer
        Dim span As Integer = If(chroma, 2, 4)
        Dim nza As Integer = 0
        Dim nzb As Integer = 0

        If bx > 0 Then
            nza = nnzOf(info, by * span + bx - 1, chroma, plane)
        ElseIf left IsNot Nothing Then
            nza = nnzOf(left, by * span + (span - 1), chroma, plane)
        End If

        If by > 0 Then
            nzb = nnzOf(info, (by - 1) * span + bx, chroma, plane)
        ElseIf top IsNot Nothing Then
            nzb = nnzOf(top, (span - 1) * span + bx, chroma, plane)
        End If

        Return If(nza > 0, 1, 0) + If(nzb > 0, 2, 0)
    End Function

    Private Shared Function nnzOf(info As H264MbInfo, index As Integer, chroma As Boolean, plane As Integer) As Integer
        If info Is Nothing Then Return 0

        If chroma Then
            Return info.chromaAcNnz(plane)(index)
        Else
            Return info.lumaAcNnz(index)
        End If
    End Function

    ''' <summary>
    ''' 在合法的 I_16x16 预测模式里选残差平方和最小者
    ''' </summary>
    Private Function selectLumaMode(source As H264Yuv420, recon As H264Yuv420,
                                    x0 As Integer, y0 As Integer,
                                    leftAvailable As Boolean, topAvailable As Boolean) As Integer
        Dim allowed As Boolean() = H264IntraPrediction.allowedModes(leftAvailable, topAvailable)
        Dim tryPred As Integer() = selectBuffer
        Dim bestMode As Integer = CInt(H264IntraMode.DC)
        Dim bestSse As Long = Long.MaxValue

        For mode As Integer = 0 To 3
            If Not allowed(mode) Then Continue For

            Call H264IntraPrediction.predict16x16(recon.y, x0, y0, CType(mode, H264IntraMode),
                                                  leftAvailable, topAvailable, tryPred)

            Dim sse As Long = 0

            For y As Integer = 0 To 15
                For x As Integer = 0 To 15
                    Dim d As Integer = source.y.at(x0 + x, y0 + y) - tryPred(y * 16 + x)

                    sse += CLng(d) * d
                Next
            Next

            If sse < bestSse Then
                bestSse = sse
                bestMode = mode
            End If
        Next

        Return bestMode
    End Function

    Private ReadOnly selectBufferStorage As New Threading.ThreadLocal(Of Integer())

    Private ReadOnly Property selectBuffer As Integer()
        Get
            Dim buf As Integer() = selectBufferStorage.Value

            If buf Is Nothing Then
                buf = New Integer(255) {}
                selectBufferStorage.Value = buf
            End If

            Return buf
        End Get
    End Property

End Class
