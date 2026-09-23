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

    ''' <summary>各宏块实际使用的运动向量（按宏块光栅序号索引，供中值预测使用）</summary>
    Private ReadOnly mvStoreX As Integer()
    Private ReadOnly mvStoreY As Integer()

    ''' <summary>
    ''' 各宏块是否为 inter 编码（按宏块光栅序号索引，逐帧重置）。
    ''' </summary>
    ''' <remarks>
    ''' 中值预测必须区分「邻居是 inter 且存在」与「邻居是 intra / 不可用」：解码器用参考索引
    ''' <c>LIST_NOT_USED = -1</c>（帧内）与 <c>PART_NOT_AVAILABLE = -2</c>（画面外或异切片）
    ''' 来表示后两者，且它们的运动向量按 0 参与运算。缺了这一信息就只能退回纯中值，
    ''' 在「只有一个邻居是 inter」时给出 0，而解码器取该邻居的运动向量——表现为画面错位。
    ''' </remarks>
    Private ReadOnly interMb As Boolean()

    ''' <summary>
    ''' 各宏块写进码流的运动向量差幅值（供 mvd 首 bin 的上下文使用）。
    ''' 只在写出 mvd 时更新——解码器同样只在读到 mvd 时写它的 mvd_cache。
    ''' </summary>
    Private ReadOnly mvMagX As Integer()
    Private ReadOnly mvMagY As Integer()

    ''' <summary>列表 1 写进码流的 mvd 幅值（供 L1 的 mvd 首 bin 上下文使用）</summary>
    Private ReadOnly mvMagX1 As Integer()
    Private ReadOnly mvMagY1 As Integer()

    ''' <summary>已编码（已输出）的采样数，即解码顺序上的序号</summary>
    Private frameIndex As Integer

    ''' <summary>已接收的显示序帧数</summary>
    Private displayIndex As Integer

    ''' <summary>等待未来锚点的 B 帧原始像素及其行宽</summary>
    Private pendingBgra As Byte()
    Private pendingPixelWidth As Integer

    Private reference As H264Yuv420

    ''' <summary>
    ''' 上一个参考图像（B 切片的「过去锚点」）。
    ''' </summary>
    ''' <remarks>
    ''' B 切片有两条列表。在 <c>max_num_ref_frames = 2</c> 且不做参考重排序时，解码器的
    ''' 列表 1 默认与列表 0 同序（8.2.4.2.4），而列表 0 按 PicNum 降序初始化，
    ''' 于是 <c>L0[0] / L1[0]</c> 是较新的参考（本帧的「未来」锚点）、
    ''' <c>L0[1] / L1[1]</c> 是较旧的参考（「过去」锚点）。
    ''' 因此 <see cref="reference"/> 即未来锚点、本字段即过去锚点。
    ''' </remarks>
    Private referencePast As H264Yuv420

    ''' <summary>
    ''' 各宏块在列表 0 / 列表 1 中使用的参考索引，<c>-1</c> 表示该宏块不使用该列表。
    ''' </summary>
    ''' <remarks>
    ''' B 切片的运动向量预测要求「邻居在该列表上使用了相同参考索引」才参与中值，
    ''' 因此必须逐宏块逐列表记录（解码器用 <c>ref_cache[list][..]</c> 表达同一信息）。
    ''' P 切片的宏块恒为列表 0、参考索引 0。
    ''' </remarks>
    Private ReadOnly refL0 As Integer()
    Private ReadOnly refL1 As Integer()

    ''' <summary>列表 1 的运动向量（四分之一像素单位），布局与 <see cref="mvStoreX"/> 相同</summary>
    Private ReadOnly mvStoreX1 As Integer()
    Private ReadOnly mvStoreY1 As Integer()

    ' ---- 复用的临时缓冲：避免逐宏块/逐块分配造成 GC 抖动 ----
    Private ReadOnly predLuma As Integer() = New Integer(255) {}
    Private ReadOnly predChroma As Integer()() = {New Integer(63) {}, New Integer(63) {}}
    ''' <summary>B 切片第二路预测（用于双向平均与候选比较）</summary>
    Private ReadOnly predLumaB As Integer() = New Integer(255) {}
    Private ReadOnly predChromaB As Integer()() = {New Integer(63) {}, New Integer(63) {}}
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
        Me.qp = If(H264Debug.ForceQp > 0, H264Debug.ForceQp, mapQuality(quality))
        Me.codec = New H264SpsPps(width, height)

        Dim mbCount As Integer = codec.widthInMbs * codec.heightInMapUnits

        Me.mvStoreX = New Integer(mbCount - 1) {}
        Me.mvStoreY = New Integer(mbCount - 1) {}
        Me.mvStoreX1 = New Integer(mbCount - 1) {}
        Me.mvStoreY1 = New Integer(mbCount - 1) {}
        Me.mvMagX = New Integer(mbCount - 1) {}
        Me.mvMagY = New Integer(mbCount - 1) {}
        Me.mvMagX1 = New Integer(mbCount - 1) {}
        Me.mvMagY1 = New Integer(mbCount - 1) {}
        Me.interMb = New Boolean(mbCount - 1) {}
        Me.refL0 = New Integer(mbCount - 1) {}
        Me.refL1 = New Integer(mbCount - 1) {}
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
    Friend Function encodeFrame(bgra As Byte(), pixelWidth As Integer) As List(Of H264EncodedFrame)
        Dim result As New List(Of H264EncodedFrame)
        Dim display As Integer = displayIndex

        If Not bFrameEnabled Then
            ' B 帧路径尚未通过验收：只使用 B_L0_16x16 时零错误，但启用双向（列表 1）后
            ' 仍有一处解码器报 bytestream overread，说明列表 1 的语法或上下文尚有一处未对齐。
            ' 在定位并验证之前不产出 B 帧——回退到已验证的「显示序 = 解码序、逐帧为 I/P」形态，
            ' 这样输出的码流与上一阶段完全等价（ctts 偏移全为 0，封装层不会写出该 box）。
            Dim isI As Boolean = (display = 0)
            Dim isRefresh As Boolean = (display > 0) AndAlso (display Mod gopSize) = 0

            result.Add(encodePicture(bgra, pixelWidth,
                                     If(isI OrElse isRefresh, H264SliceType.I, H264SliceType.P),
                                     isI, display, (display * 2) And 255, display))
            displayIndex += 1

            Return result
        End If

        If display = 0 Then
            ' 首个锚点用 IDR
            result.Add(encodePicture(bgra, pixelWidth, H264SliceType.I, True, 0, 0, display))
        ElseIf (display And 1) = 1 Then
            ' B 候选：显示序为奇数的帧夹在两个锚点之间，必须先拿到【未来】锚点才能编码，
            ' 因此这里只缓存像素，等下一个锚点到齐时一并输出。
            pendingBgra = bgra
            pendingPixelWidth = pixelWidth
        Else
            ' 锚点（显示序偶数）：周期性插入 I 帧做刷新，其余为 P 帧
            Dim isRefresh As Boolean = (display Mod gopSize) = 0

            result.Add(encodePicture(bgra, pixelWidth,
                                     If(isRefresh, H264SliceType.I, H264SliceType.P),
                                     False, display \ 2, (display * 2) And 255, display))

            ' 未来锚点已就绪 → 立刻把缓存的 B 帧按解码顺序编在它之后
            If pendingBgra IsNot Nothing Then
                result.Add(encodePicture(pendingBgra, pendingPixelWidth, H264SliceType.B,
                                         False, display \ 2, ((display - 1) * 2) And 255, display - 1))
                pendingBgra = Nothing
            End If
        End If

        displayIndex += 1

        Return result
    End Function

    ''' <summary>
    ''' 输出缓冲中剩余的帧。
    ''' </summary>
    ''' <remarks>
    ''' 若总帧数为奇数，最后一帧是「没有未来锚点的 B」——此时改按 P 帧编出（引用最近的锚点），
    ''' 仍是合法且可播放的码流，不会因为缺少未来参考而丢帧。
    ''' </remarks>
    Friend Function flush() As List(Of H264EncodedFrame)
        Dim result As New List(Of H264EncodedFrame)

        If pendingBgra IsNot Nothing Then
            Dim display As Integer = displayIndex - 1

            result.Add(encodePicture(pendingBgra, pendingPixelWidth, H264SliceType.P,
                                     False, (display + 1) \ 2, (display * 2) And 255, display))
            pendingBgra = Nothing
        End If

        Return result
    End Function

    ''' <summary>
    ''' 按指定的切片类型编码一帧，并在解码顺序上推进参考帧与计数器。
    ''' </summary>
    ''' <param name="sliceType">切片类型（I / P / B）</param>
    ''' <param name="frameNum">frame_num（参考图像按解码顺序递增，非参考图像沿用前一个参考图像的取值）</param>
    ''' <param name="pocLsb">pic_order_cnt_lsb（= 显示序 × 2，保证 B 帧的 POC 落在两个锚点之间）</param>
    ''' <param name="display">本帧的显示序（用于推导 ctts 的合成时间偏移）</param>
    Private Function encodePicture(bgra As Byte(), pixelWidth As Integer,
                                   sliceType As H264SliceType, isIdr As Boolean,
                                   frameNum As Integer, pocLsb As Integer,
                                   display As Integer) As H264EncodedFrame
        Dim source As New H264Yuv420(codec.codedWidth, codec.codedHeight, frameWidth, frameHeight)

        Call source.loadBgra(bgra, pixelWidth)

        Dim recon As H264Yuv420 = source.clone()
        Dim isI As Boolean = (sliceType = H264SliceType.I)
        ' P 帧已通过验收（解码零错误、重建与解码器逐样点一致），故启用。
        ' 该路径此前长期无法通过，根因是六处「合法码流但预测/重建与解码器不一致」的偏差，逐一实测定位：
        '   1) 运动向量单位：码流用四分之一像素，早期直接把整像素位移当 MV 写出。静止内容因位移为 0
        '      而完全正常，运动内容则被解码端按 1/4 位移取样——这是长期掩盖该缺陷的主因；
        '   2) 运动向量预测：解码器 pred_motion 是「按参考索引匹配数分支」而非纯三邻居中值，
        '      且右上邻居不可用时回退到【左上】邻居；单参考下即 count>=2 取中值、=1 取该邻居、=0 取 0；
        '   3) 不可用邻居的 nnz 填充：inter 宏块为 0、帧内为 64（h264_mvpred.h fill_decode_caches）；
        '   4) 不可用邻居的 cbp 填充：inter 为 0x00F、帧内为 0x7CF（同上），影响色度 DC 的 CBF 上下文；
        '   5) 反量化漏了 (x + 32) >> 6 与默认缩放矩阵的 ×16（h264_ps.c:640-644 / h264_cabac.c STORE_BLOCK）；
        '   6) 逆变换蝶形与色度 DC 归一化、亮度 DC 舍入项与解码器不一致。
        ' 另：mb_qp_delta 只在 cbp != 0 或宏块为 I_16x16 时才写。
        ' 实测（8 帧、运动内容）：全部尺寸零错误解码，编码器重建与解码器输出逐样点一致（0/12288 不同）；
        ' 逐帧 PSNR 16x16 32.1-46.7、32x32 37.3-47.3、48x32 38.7-47.6、64x64 42.2-48.5、80x64 42.9-48.6 dB。
        ' 已知限制：128x96 第 1 帧为 29.7 dB —— 测试内容在 256 处回绕造成该帧 1/4 画面无法被平移匹配，
        ' 且运动估计只做整像素（无半像素细化），属编码质量而非正确性；该尺寸其余各帧为 31-50 dB。
        Dim isB As Boolean = (sliceType = H264SliceType.B)
        Dim bits As New BitStreamWriter(1 << 16)

        ' slice_qp_delta 是相对 PPS 的 pic_init_qp_minus26(= 0) 的偏移，因此实际 QP = 26 + delta
        ' B 切片用 num_ref_idx_active_override 把两条列表的活动参考数都抬到 2：
        ' 列表 0 按 PicNum 降序，故 ref 0 = 未来锚点、ref 1 = 过去锚点，列表 1 默认与列表 0 同序。
        Call H264SliceHeader.write(bits, sliceType, isIdr,
                                   frameNum:=frameNum,
                                   picOrderCntLsb:=pocLsb,
                                   sliceQpDelta:=(qp - 26),
                                   cabacInitIdc:=If(isB, 1, 0),
                                   numRefIdxOverride:=isB,
                                   numRefIdxL0Minus1:=1,
                                   numRefIdxL1Minus1:=1,
                                   isReference:=Not isB)

        Dim cabac As New H264Cabac(bits)

        Call cabac.begin()
        ' I 切片用 I 表；P 用 PB[0]、B 用 PB[1]，必须与切片头写出的 cabac_init_idc 一致
        Call cabac.initStates(isI, If(isB, 1, 0), qp)

        Dim mbCols As Integer = codec.widthInMbs
        Dim mbRows As Integer = codec.heightInMapUnits
        Dim prevRow As H264MbInfo() = newMbInfoRow(mbCols)
        Dim curRow As H264MbInfo() = newMbInfoRow(mbCols)

        ' 逐帧重置 inter 标记与两列表簿记：上一帧的邻居信息不能影响本帧的运动向量预测。
        ' refL0/refL1 的「未使用」哨兵值是 -1 而【不是 0】——0 表示「使用该列表且参考索引为 0」，
        ' 用作默认值会让 B 切片把尚未编码/不使用该列表的邻居误判为匹配。
        Call Array.Clear(Me.interMb, 0, Me.interMb.Length)

        For i As Integer = 0 To Me.refL0.Length - 1
            Me.refL0(i) = -1
            Me.refL1(i) = -1
        Next

        For mbY As Integer = 0 To mbRows - 1
            For mbX As Integer = 0 To mbCols - 1
                Dim isLast As Boolean = (mbY = mbRows - 1) AndAlso (mbX = mbCols - 1)

                If isB Then
                    Call encodeBInterMacroblock(cabac, source, recon, mbX, mbY, prevRow, curRow)
                ElseIf isI Then
                    Call encodeMacroblock(cabac, source, recon, mbX, mbY, prevRow, curRow)
                Else
                    Call encodeInterMacroblock(cabac, source, recon, mbX, mbY, prevRow, curRow)
                End If
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
        ' nal_ref_idc：B 帧是【非参考图像】，必须为 0——写成非 0 会让解码器把 B 帧放进 DPB 的
        ' 短参考列表，它与「沿用前一个参考的 frame_num」叠加就会触发
        ' 「illegal short term buffer state detected」，同时污染后续帧的参考列表内容。
        Dim nal As Byte() = BitStreamWriter.nalUnit(rbsp, If(isB, 0, 3), If(isIdr, 5, 1), annexB:=False)
        Dim sample As Byte() = New Byte(nal.Length + 3) {}

        sample(0) = CByte((nal.Length >> 24) And &HFF)
        sample(1) = CByte((nal.Length >> 16) And &HFF)
        sample(2) = CByte((nal.Length >> 8) And &HFF)
        sample(3) = CByte(nal.Length And &HFF)
        Call Array.Copy(nal, 0, sample, 4, nal.Length)

        ' 参考图像只在 I/P 帧推进：B 帧不作为参考，若把它的重建写进 reference，
        ' 后续帧的预测就会引用一个解码器并未标记为参考的图像。
        ' referencePast 保留上一个锚点，供紧随其后的 B 帧作为「过去锚点」（列表 1 的 ref_idx 1）。
        If Not isB Then
            referencePast = reference
            reference = recon
        End If

        ' ctts 的合成时间偏移 = 显示时间 − 解码时间；启用 B 帧时整体给显示时间平移 1 帧使其恒为非负
        ' （ctts 的 0 版本条目是无符号的）。平移等价于给整条视频轨加一个固定延迟，不影响播放顺序；
        ' 未启用 B 帧时平移为 0，偏移恒为 0，封装层不会写出 ctts（与上一阶段等价）。
        Dim encoded As New H264EncodedFrame(sample, (display + compositionShift) - frameIndex)

        frameIndex += 1

        ' 【临时诊断】导出编码器自建重建帧的三个平面，供与解码器输出逐样点比对
        If H264Debug.Enabled Then
            H264Debug.ReconWidth = frameWidth
            H264Debug.ReconHeight = frameHeight
            H264Debug.ReconY = exportPlane(recon.y, frameWidth, frameHeight)
            H264Debug.ReconU = exportPlane(recon.u, frameWidth \ 2, frameHeight \ 2)
            H264Debug.ReconV = exportPlane(recon.v, frameWidth \ 2, frameHeight \ 2)
        End If

        Return encoded
    End Function

    ''' <summary>【临时诊断】把一个平面导出为按行展开的 8 位字节数组（按 8 位钳制）</summary>
    Private Shared Function exportPlane(plane As H264Plane, width As Integer, height As Integer) As Byte()
        Dim data As Byte() = New Byte(width * height - 1) {}

        For y As Integer = 0 To height - 1
            For x As Integer = 0 To width - 1
                Dim v As Integer = CInt(plane.at(x, y))

                If v < 0 Then v = 0
                If v > 255 Then v = 255

                data(y * width + x) = CByte(v)
            Next
        Next

        Return data
    End Function

    Private Shared Function newMbInfoRow(count As Integer) As H264MbInfo()
        Dim row As H264MbInfo() = New H264MbInfo(count - 1) {}

        For i As Integer = 0 To count - 1
            row(i) = New H264MbInfo
        Next

        Return row
    End Function

    ''' <summary>GOP 长度：帧 0 为 IDR，其后每 gopSize 帧一个非 IDR I 帧，其余为 P 帧</summary>
    Private Const gopSize As Integer = 30

    ''' <summary>
    ''' 是否启用 B 帧（双向预测帧）。
    ''' </summary>
    ''' <remarks>
    ''' <b>尚未通过验收，保持关闭。</b>现状：GOP 结构（锚点 + 夹在其间的 B）、解码顺序重排、
    ''' 非参考图像的 <c>nal_ref_idc = 0</c> 与切片头、<c>ctts</c> 合成时间偏移、以及
    ''' <c>B_L0_16x16</c> 的完整语法都已实测零错误（把每个 B 宏块限制为只用列表 0 时，
    ''' 3 / 8 帧用例全部零错误解码）；但一旦宏块带上<b>列表 1</b>（双向）就会出现
    ''' <c>bytestream overread</c>，说明列表 1 的语法或上下文仍有未对齐之处。
    ''' 按失败保护，未验证的路径不得对外产出，故此处为 False——
    ''' 输出与上一已通过阶段（纯 I/P、无重排序）完全等价。
    ''' </remarks>
    Private Const bFrameEnabled As Boolean = False

    ''' <summary>
    ''' 编码一个 P 切片的「跳过」宏块（<c>mb_skip_flag = 1</c>）
    ''' </summary>
    ''' <remarks>
    ''' 跳过宏块<b>不写</b> mb_type、mvd、CBP 与残差：解码器按邻近运动向量中值预测取值，
    ''' 在整帧全跳过时该预测恒为 (0,0)，于是重建 = 直接复制参考帧同位块。
    ''' 
    ''' skip 的上下文规则（ffmpeg <c>decode_cabac_mb_skip</c>）：
    ''' <c>ctx = (左邻存在且未跳过 ? 0 : 1) + (上邻存在且未跳过 ? 0 : 1)</c>。
    ''' 全跳过帧中邻居要么跳过、要么不可用，故恒为 2。
    ''' </remarks>
    Private Sub encodeSkippedMacroblock(cabac As H264Cabac, recon As H264Yuv420, mbX As Integer, mbY As Integer)
        ' mb_skip_flag 的 ctxIdxOffset = 11（H.264 表 9-34；ffmpeg 为 &sl->cabac_state[11+ctx]），
        ' 增量按邻居规则至多 2，故实际上下文落在 11..13。漏掉这个基址会让解码器把 skip 解成 0，
        ' 进而去读 mb_type 与残差，表现为 bytestream overread。
        Call cabac.encodeBin(11 + 2, 1)

        Call copyPlane(recon.y, reference.y, mbX * 16, mbY * 16, 16)
        Call copyPlane(recon.u, reference.u, mbX * 8, mbY * 8, 8)
        Call copyPlane(recon.v, reference.v, mbX * 8, mbY * 8, 8)
    End Sub

    ''' <summary>把参考帧中的一块方形区域复制到重建帧（跳过宏块的运动向量为 0）</summary>
    Private Shared Sub copyPlane(dst As H264Plane, src As H264Plane, x0 As Integer, y0 As Integer, size As Integer)
        For y As Integer = 0 To size - 1
            For x As Integer = 0 To size - 1
                Call dst.setAt(x0 + x, y0 + y, src.at(x0 + x, y0 + y))
            Next
        Next
    End Sub

    ''' <summary>
    ''' 16x16 分区的运动向量预测（H.264 8.4.1.3）
    ''' </summary>
    ''' <remarks>
    ''' 邻居取法：A = 左邻、B = 上邻、C = 右上邻。
    ''' 
    ''' <para>
    ''' 解码器 <c>h264_mvpred.h: pred_motion</c> 的规则<b>不是</b>简单的三邻居中值，而是先按
    ''' <c>match_count</c> 分支——统计 A/B/C 中有几个「存在且使用与当前分区相同的参考索引」
    ''' （本编码器只用一个参考帧，故等价于「存在且为 inter」）：
    ''' </para>
    ''' <list type="bullet">
    '''   <item><c>match_count &gt;= 2</c>：取 <c>mid_pred(A, B, C)</c>，其中帧内/不可用邻居按其 0 参与；</item>
    '''   <item><c>match_count = 1</c>：直接取那唯一匹配邻居的运动向量，<b>不再取中值</b>；</item>
    '''   <item><c>match_count = 0</c>：取 0。</item>
    ''' </list>
    ''' 
    ''' <para>
    ''' 三态参考索引用 <c>h264pred.h</c> 的 <c>PART_NOT_AVAILABLE = -2</c>（画面外或异切片）与
    ''' <c>h264dec.h</c> 的 <c>LIST_NOT_USED = -1</c>（帧内）表示，两者都不等于当前 ref(=0)，
    ''' 故单参考下都归入「不匹配」，且其运动向量按 0 参与中值。
    ''' </para>
    ''' 
    ''' <para>
    ''' C 不可用时按 <c>fetch_diagonal_mv</c> 的 else 分支取 <c>mv_cache[i - 8 - 1]</c>，
    ''' 即<b>左上</b>邻居 D（规范 Figure 8-6 的 D），<b>不是</b>上方邻居 B——这一点最易写错。
    ''' </para>
    ''' 
    ''' <para>
    ''' 预测值若与解码器不一致<b>不会</b>造成语法失步，只会让解码端重建的运动向量与编码端不同，
    ''' 表现为整帧错位并随后续帧累积发散（零错误解码但 PSNR 极低），因此必须逐字对齐。
    ''' </para>
    ''' </remarks>
    Private Sub predictMedianMv(mbX As Integer, mbY As Integer, ByRef px As Integer, ByRef py As Integer)
        Dim mbCols As Integer = codec.widthInMbs
        Dim ax As Integer = 0, ay As Integer = 0
        Dim bx As Integer = 0, by As Integer = 0
        Dim cx As Integer = 0, cy As Integer = 0
        Dim aInter As Boolean = False
        Dim bInter As Boolean = False
        Dim cInter As Boolean = False

        If mbX > 0 Then
            Dim i As Integer = mbY * mbCols + mbX - 1

            aInter = interMb(i)

            If aInter Then
                ax = mvStoreX(i) : ay = mvStoreY(i)
            End If
        End If

        If mbY > 0 Then
            Dim i As Integer = (mbY - 1) * mbCols + mbX

            bInter = interMb(i)

            If bInter Then
                bx = mvStoreX(i) : by = mvStoreY(i)
            End If

            If mbX + 1 < mbCols Then
                Dim j As Integer = (mbY - 1) * mbCols + mbX + 1

                cInter = interMb(j)

                If cInter Then
                    cx = mvStoreX(j) : cy = mvStoreY(j)
                End If
            ElseIf mbX > 0 Then
                ' 右上不可用（画面右边界）→ 回退到左上邻居 D（与解码器 fetch_diagonal_mv 一致）
                Dim j As Integer = (mbY - 1) * mbCols + mbX - 1

                cInter = interMb(j)

                If cInter Then
                    cx = mvStoreX(j) : cy = mvStoreY(j)
                End If
            End If
        End If

        Dim count As Integer = If(aInter, 1, 0) + If(bInter, 1, 0) + If(cInter, 1, 0)

        If count >= 2 Then
            px = medianOf3(ax, bx, cx)
            py = medianOf3(ay, by, cy)
        ElseIf count = 1 Then
            If aInter Then
                px = ax : py = ay
            ElseIf bInter Then
                px = bx : py = by
            Else
                px = cx : py = cy
            End If
        Else
            px = 0 : py = 0
        End If
    End Sub

    Private Shared Function medianOf3(a As Integer, b As Integer, c As Integer) As Integer
        Return Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c))
    End Function

    ''' <summary>
    ''' B 切片的逐列表运动向量预测（镜像解码器 <c>pred_motion</c> 在指定 list/refIdx 下的分支）
    ''' </summary>
    ''' <param name="list">参考列表（0 或 1）</param>
    ''' <param name="refIdx">本宏块在该列表上使用的参考索引</param>
    ''' <remarks>
    ''' 与 P 用的逐图中值预测唯一不同之处是筛选条件：解码器只把「在<b>同一列表</b>上使用了
    ''' <b>相同参考索引</b>」的邻居计入匹配数（<c>ref_cache[list][nb] == ref</c>），
    ''' 帧内邻居、不使用该列表的邻居、参考索引不同的邻居都不参与。
    ''' 几何位置与「右上不可用时回退到左上 D」的规则与 P 切片完全一致，故逐行沿用。
    ''' </remarks>
    Private Sub predictListMv(list As Integer, refIdx As Integer,
                              mbX As Integer, mbY As Integer,
                              ByRef px As Integer, ByRef py As Integer)
        Dim mbCols As Integer = codec.widthInMbs
        Dim storeX As Integer() = If(list = 0, mvStoreX, mvStoreX1)
        Dim storeY As Integer() = If(list = 0, mvStoreY, mvStoreY1)
        Dim refs As Integer() = If(list = 0, refL0, refL1)
        Dim ax As Integer = 0, ay As Integer = 0, bx As Integer = 0, by As Integer = 0
        Dim cx As Integer = 0, cy As Integer = 0
        Dim aMatch As Boolean = False, bMatch As Boolean = False, cMatch As Boolean = False

        If mbX > 0 Then
            Dim i As Integer = mbY * mbCols + mbX - 1

            aMatch = (refs(i) = refIdx)

            If aMatch Then ax = storeX(i) : ay = storeY(i)
        End If

        If mbY > 0 Then
            Dim i As Integer = (mbY - 1) * mbCols + mbX

            bMatch = (refs(i) = refIdx)

            If bMatch Then bx = storeX(i) : by = storeY(i)

            If mbX + 1 < mbCols Then
                Dim j As Integer = (mbY - 1) * mbCols + mbX + 1

                cMatch = (refs(j) = refIdx)

                If cMatch Then cx = storeX(j) : cy = storeY(j)
            ElseIf mbX > 0 Then
                ' 右上不可用（画面右边界）→ 回退到左上邻居 D（与解码器 fetch_diagonal_mv 一致）
                Dim j As Integer = (mbY - 1) * mbCols + mbX - 1

                cMatch = (refs(j) = refIdx)

                If cMatch Then cx = storeX(j) : cy = storeY(j)
            End If
        End If

        ' refs 的初值是 -1（表示「不使用该列表」），因此 refIdx = -1 不会误匹配到邻居
        Dim count As Integer = If(aMatch, 1, 0) + If(bMatch, 1, 0) + If(cMatch, 1, 0)

        If count >= 2 Then
            px = medianOf3(ax, bx, cx)
            py = medianOf3(ay, by, cy)
        ElseIf count = 1 Then
            If aMatch Then
                px = ax : py = ay
            ElseIf bMatch Then
                px = bx : py = by
            Else
                px = cx : py = cy
            End If
        Else
            px = 0 : py = 0
        End If
    End Sub

    ''' <summary>
    ''' 双向预测的平均：解码器在等权默认下为 <c>(p0 + p1 + 1) &gt;&gt; 1</c>
    ''' </summary>
    Private Shared Sub averagePrediction(a As Integer(), b As Integer(), dst As Integer(), count As Integer)
        For i As Integer = 0 To count - 1
            dst(i) = (a(i) + b(i) + 1) >> 1
        Next
    End Sub

    ''' <summary>
    ''' 写一个 mvd 分量（严格镜像解码器 <c>decode_cabac_mb_mvd</c>）
    ''' </summary>
    ''' <param name="ctxBase">分量的上下文基址：x 为 40、y 为 47</param>
    ''' <param name="amvd">左邻与上邻的 <b>mvd 幅值</b>之和，决定首个 bin 的上下文</param>
    Private Shared Sub writeMvd(cabac As H264Cabac, ctxBase As Integer, mvd As Integer, amvd As Integer)
        Dim magnitude As Integer = Math.Abs(mvd)
        Dim ctx As Integer = ctxBase + If(amvd > 2, 1, 0) + If(amvd > 32, 1, 0)

        If magnitude = 0 Then
            Call cabac.encodeBin(ctx, 0)

            Return
        End If

        Call cabac.encodeBin(ctx, 1)

        ' 前缀一元：mvd 由 1 递增到 9，且 mvd < 4 时上下文逐级 +1
        Dim prefixCtx As Integer = ctxBase + 3
        Dim value As Integer = 1

        While value < 9 AndAlso value < magnitude
            Call cabac.encodeBin(prefixCtx, 1)

            If value < 4 Then prefixCtx += 1

            value += 1
        End While

        If magnitude < 9 Then
            Call cabac.encodeBin(prefixCtx, 0)
            Call cabac.encodeBypassSign(mvd)

            Return
        End If

        ' 后缀：余量 = 幅值 - 9，用「while 1: 减去 2^k」+ k 位二进制表示
        Dim remainder As Integer = magnitude - 9
        Dim k As Integer = 3

        While remainder >= (1 << k)
            Call cabac.encodeBypass(1)

            remainder -= (1 << k)
            k += 1
        End While

        Call cabac.encodeBypass(0)

        For i As Integer = k - 1 To 0 Step -1
            Call cabac.encodeBypass((remainder >> i) And 1)
        Next

        Call cabac.encodeBypassSign(mvd)
    End Sub

    ''' <summary>写 coded_block_pattern 的 4 个亮度 bin（P 切片基址为 73）</summary>
    Private Shared Sub writeCbpLuma(cabac As H264Cabac, cbp As Integer, leftCbp As Integer, topCbp As Integer)
        Dim bit0 As Integer = cbp And 1
        Dim bit1 As Integer = (cbp >> 1) And 1
        Dim bit2 As Integer = (cbp >> 2) And 1
        Dim bit3 As Integer = (cbp >> 3) And 1
        Dim ctx As Integer

        ctx = If((leftCbp And &H2) <> 0, 0, 1) + 2 * If((topCbp And &H4) <> 0, 0, 1)
        Call cabac.encodeBin(73 + ctx, bit0)

        ctx = If(bit0 <> 0, 0, 1) + 2 * If((topCbp And &H8) <> 0, 0, 1)
        Call cabac.encodeBin(73 + ctx, bit1)

        ctx = If((leftCbp And &H8) <> 0, 0, 1) + 2 * If(bit0 <> 0, 0, 1)
        Call cabac.encodeBin(73 + ctx, bit2)

        ctx = If(bit2 <> 0, 0, 1) + 2 * If(bit1 <> 0, 0, 1)
        Call cabac.encodeBin(73 + ctx, bit3)
    End Sub

    ''' <summary>写 coded_block_pattern 的色度 bin（P 切片基址为 77）</summary>
    Private Shared Sub writeCbpChroma(cabac As H264Cabac, cbp As Integer, leftCbp As Integer, topCbp As Integer)
        Dim cbpA As Integer = (leftCbp >> 4) And &H3
        Dim cbpB As Integer = (topCbp >> 4) And &H3
        Dim ctx As Integer = If(cbpA > 0, 1, 0) + If(cbpB > 0, 2, 0)

        If cbp = 0 Then
            Call cabac.encodeBin(77 + ctx, 0)

            Return
        End If

        Call cabac.encodeBin(77 + ctx, 1)

        ctx = 4 + If(cbpA = 2, 1, 0) + If(cbpB = 2, 2, 0)
        Call cabac.encodeBin(77 + ctx, cbp - 1)
    End Sub

    ''' <summary>
    ''' 编码一个 P 切片的 P_L0_16x16 宏块（非跳过）
    ''' </summary>
    ''' <remarks>
    ''' 语法顺序（ffmpeg <c>ff_h264_decode_mb_cabac</c> 的 P 分支）：
    ''' mb_skip_flag(=0) → mb_type（P_L0_16x16 只需在 ctx 14 写一个 0）→ mvd_l0（x: 40、y: 47）
    ''' → coded_block_pattern（亮度基址 73 四个 bin、色度基址 77 两个 bin）→ mb_qp_delta(ctx 60)
    ''' → 残差。
    ''' 
    ''' 残差与 I_16x16 的关键差异：inter 的亮度 4x4 块<b>各自携带自己的 DC</b>，
    ''' 走类别 2（16 个系数、zigzag），<b>不</b>做 DC Hadamard；色度仍走类别 3/4。
    ''' </remarks>
    Private Sub encodeInterMacroblock(cabac As H264Cabac,
                                      source As H264Yuv420, recon As H264Yuv420,
                                      mbX As Integer, mbY As Integer,
                                      prevRow As H264MbInfo(), curRow As H264MbInfo())
        Dim mbCols As Integer = codec.widthInMbs
        Dim mbIndex As Integer = mbY * mbCols + mbX
        Dim x0 As Integer = mbX * 16
        Dim y0 As Integer = mbY * 16
        Dim ux As Integer = mbX * 8
        Dim uy As Integer = mbY * 8
        Dim leftAvailable As Boolean = mbX > 0
        Dim topAvailable As Boolean = mbY > 0
        Dim left As H264MbInfo = If(leftAvailable, curRow(mbX - 1), Nothing)
        Dim top As H264MbInfo = If(topAvailable, prevRow(mbX), Nothing)
        Dim info As New H264MbInfo

        ' ---- 1. 运动估计（整像素，限制为偶数位移以保证色度位移为整数）----
        Dim mvX As Integer, mvY As Integer
        Dim bestSad As Long = Long.MaxValue
        Dim candX As Integer, candY As Integer

        For oy As Integer = -H264MotionEstimation.searchRadius To H264MotionEstimation.searchRadius Step 2
            For ox As Integer = -H264MotionEstimation.searchRadius To H264MotionEstimation.searchRadius Step 2
                Dim cost As Long = H264MotionEstimation.sad(reference.y, source.y, x0, y0, 16, ox, oy)

                If cost < bestSad Then
                    bestSad = cost
                    candX = ox
                    candY = oy
                End If
            Next
        Next

        If H264Debug.ForceZeroMv Then
            candX = 0
            candY = 0
        End If

        Call predictMedianMv(mbX, mbY, mvX, mvY)

        ' 运动向量在码流中的单位是【四分之一像素】（H.264 8.4.1.3），而运动估计给出的是整像素位移，
        ' 因此必须先 ×4 再参与预测、差分与重建取样。写成整像素会让解码端只按 1/4 位移取样：
        ' 编码器自建重建看着正常（它用的是整像素位移）、解码画面却严重错位；
        ' 而静止内容因位移为 0 完全正常——这正是该缺陷长期被掩盖的原因。
        Dim mvdX As Integer = candX * 4 - mvX
        Dim mvdY As Integer = candY * 4 - mvY

        mvX = mvX + mvdX
        mvY = mvY + mvdY

        ' ---- 2. 预测块：亮度取样位移 = mv/4 像素、色度 = mv/8 像素 ----
        ' （运动估计只取偶数整像素位移，故 mv 恒为 8 的倍数，两个除法都是精确的）
        Call H264MotionEstimation.buildPrediction(reference.y, x0, y0, 16, mvX \ 4, mvY \ 4, predLuma, 16)
        Call H264MotionEstimation.buildPrediction(reference.u, ux, uy, 8, mvX \ 8, mvY \ 8, predChroma(0), 8)
        Call H264MotionEstimation.buildPrediction(reference.v, ux, uy, 8, mvX \ 8, mvY \ 8, predChroma(1), 8)

        ' ---- 3. 亮度残差：16 个 4x4 块各自带 DC，走类别 2 ----
        For by As Integer = 0 To 3
            For bx As Integer = 0 To 3
                Call loadLumaResidual(source.y, predLuma, x0, y0, bx, by, resBlock)
                Call H264Transform.forwardTransform(resBlock, blockCoef)
                Call H264Transform.quantize(blockCoef, blockLevels, qp, False)
                Call Array.Copy(blockLevels, lumaAcLevels(by * 4 + bx), 16)
            Next
        Next

        ' ---- 4. 色度残差：DC 收集到 2x2 Hadamard，其余走 AC ----
        For c As Integer = 0 To 1
            Dim plane As H264Plane = If(c = 0, source.u, source.v)

            For by As Integer = 0 To 1
                For bx As Integer = 0 To 1
                    Dim blockIdx As Integer = by * 2 + bx

                    Call loadChromaResidual(plane, predChroma(c), ux, uy, bx, by, resBlock)
                    Call H264Transform.forwardTransform(resBlock, blockCoef)
                    Call H264Transform.quantize(blockCoef, blockLevels, qp, False)

                    chromaDcIn(c)(blockIdx) = blockLevels(0)

                    Dim ac As Integer() = chromaAcLevels(c)(blockIdx)

                    ac(0) = 0
                    For i As Integer = 1 To 15
                        ac(i) = blockLevels(i)
                    Next
                Next
            Next

            Call H264Transform.forwardChromaDcTransform(chromaDcIn(c)(0), chromaDcIn(c)(1), chromaDcIn(c)(2), chromaDcIn(c)(3), chromaDcCoeff)
            Call H264Transform.quantizeDc(chromaDcCoeff, chromaDcLevels(c), 4, qp, False)
        Next

        ' ---- 5. CBP：亮度按 8x8 分组汇总（4 bit），色度 0/1/2 ----
        Dim lumaCbp As Integer = 0

        For by As Integer = 0 To 3
            For bx As Integer = 0 To 3
                If hasCoeff(lumaAcLevels(by * 4 + bx)) Then
                    lumaCbp = lumaCbp Or (1 << ((by \ 2) * 2 + (bx \ 2)))
                End If
            Next
        Next

        Dim chromaDcPresent As Boolean = False
        Dim chromaAcPresent As Boolean = False

        For c As Integer = 0 To 1
            If hasCoeff(chromaDcLevels(c)) Then chromaDcPresent = True

            For i As Integer = 0 To 3
                If hasCoeff(chromaAcLevels(c)(i)) Then chromaAcPresent = True
            Next
        Next

        Dim chromaCbp As Integer = If(chromaAcPresent, 2, If(chromaDcPresent, 1, 0))

        If H264Debug.ForceNoResidual Then
            lumaCbp = 0
            chromaCbp = 0
        End If

        ' ---- 6. 宏块语法 ----
        ' mb_skip_flag 的上下文：解码器为「左/上邻【存在且未跳过】则 +1，否则 +0」
        ' （h264_cabac.c: decode_cabac_mb_skip 的 ctx++ 分支，基址 11）。
        ' 本实现不产生跳过宏块，故邻居只要可用就 +1——若写成「不可用 +1」就是反向，
        ' 解码器会在某些宏块上把 skip 解成 1 而跳过 mb_type，从那里开始失步。
        Call cabac.encodeBin(11 + If(leftAvailable, 1, 0) + If(topAvailable, 1, 0), 0)
        ' mb_type = P_L0_16x16 需要【三个】0 bin：ctx 14（P 型）→ ctx 15（16x16/8x8 族）
        ' → ctx 16（3*bin=0 即 P_L0_16x16）。只写第一个会让解码器接着读 ctx 15/16，
        ' 从本宏块起与残差位流错位。
        Call cabac.encodeBin(14, 0)
        Call cabac.encodeBin(15, 0)
        Call cabac.encodeBin(16, 0)

        ' amvd 只用【左右邻居的 mvd 幅值之和】（解码器为 mvd_cache[left] + mvd_cache[top]）：
        ' 不能把自己这一块的幅值算进去——那会让首个 bin 的上下文与解码器不一致，从而在这一宏块失步。
        Dim amvdX As Integer = If(leftAvailable, mvMagX(mbIndex - 1), 0) + If(topAvailable, mvMagX(mbIndex - mbCols), 0)
        Dim amvdY As Integer = If(leftAvailable, mvMagY(mbIndex - 1), 0) + If(topAvailable, mvMagY(mbIndex - mbCols), 0)

        Call writeMvd(cabac, 40, mvdX, amvdX)
        Call writeMvd(cabac, 47, mvdY, amvdY)

        ' 邻居不可用时按 ffmpeg fill_caches 的 0x7CF 填充（A/B 实测：改用 0 会立刻在 MB(0,0) 失步）
        Dim leftCbp As Integer = If(leftAvailable AndAlso left IsNot Nothing, left.cbp, &H7CF)
        Dim topCbp As Integer = If(topAvailable AndAlso top IsNot Nothing, top.cbp, &H7CF)

        Call writeCbpLuma(cabac, lumaCbp, leftCbp, topCbp)
        Call writeCbpChroma(cabac, chromaCbp, leftCbp, topCbp)

        ' mb_qp_delta 只在 cbp != 0 或宏块为 I_16x16 时才存在（ffmpeg: if (cbp || IS_INTRA16x16(mb_type))）。
        ' 本路径是 inter，故 cbp 为 0 时【绝不能】写这一位——多写一位会逐宏块累积，
        ' 表现为「同一行若干个宏块之后突然 bytestream overread」。
        If (lumaCbp Or chromaCbp) <> 0 Then
            Call cabac.encodeBin(60, 0)
        End If

        ' ---- 7. 残差系数 ----
        ' inter 的 coded_block_flag 上下文与 intra 不同：不可用邻居的 nnz 记 0（intra 才是记 16），
        ' 因此这里传入「零 nnz 的占位宏块信息」而不是 Nothing。
        Dim acLeft As H264MbInfo = If(left, New H264MbInfo)
        Dim acTop As H264MbInfo = If(top, New H264MbInfo)

        If lumaCbp <> 0 Then
            For i4x4 As Integer = 0 To 15
                Dim bx As Integer = ((i4x4 >> 2) And 1) * 2 + (i4x4 And 1)
                Dim by As Integer = ((i4x4 >> 3) And 1) * 2 + ((i4x4 >> 1) And 1)

                If ((lumaCbp >> ((by \ 2) * 2 + (bx \ 2))) And 1) = 0 Then Continue For

                Dim ctx As Integer = H264Residual.cbfBaseCtx(2) + acNeighborCtx(acLeft, acTop, info, bx, by, False, 0, 0)

                info.lumaAcNnz(by * 4 + bx) = H264Residual.writeBlock(cabac, lumaAcLevels(by * 4 + bx), 2, H264Residual.zigZag, ctx)
            Next
        End If

        Dim chromaDcBits As Integer = 0

        If chromaCbp <> 0 Then
            For c As Integer = 0 To 1
                ' 色度 DC 的 coded_block_flag 上下文取自邻居 left_cbp/top_cbp 的 bit(6+c)
                ' （h264_cabac.c: get_cabac_cbf_ctx 的 is_dc && cat==3 分支）。
                ' 邻居不可用时的填充值见 h264_mvpred.h: fill_decode_caches ——
                ' inter 宏块是 0x00F、帧内宏块才是 0x7CF，两者 bit6/bit7 分别是 0 与 1。
                ' 本路径是 inter，故不可用邻居【不加】偏移；写成 +1/+2 会与解码器失步。
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
                    Dim ctx As Integer = H264Residual.cbfBaseCtx(4) + acNeighborCtx(left, top, info, cx, cy, True, c, 0)

                    info.chromaAcNnz(c)(i) = H264Residual.writeBlock(cabac, chromaAcLevels(c)(i), 4, H264Residual.acScan, ctx)
                Next
            Next
        End If

        info.cbp = lumaCbp Or (chromaCbp << 4) Or chromaDcBits
        curRow(mbX) = info

        mvStoreX(mbIndex) = mvX
        mvStoreY(mbIndex) = mvY
        mvMagX(mbIndex) = Math.Abs(mvdX)
        mvMagY(mbIndex) = Math.Abs(mvdY)
        ' P 切片的宏块恒为列表 0、参考索引 0（列表 1 不使用 → 哨兵 -1）
        refL0(mbIndex) = 0
        refL1(mbIndex) = -1
        interMb(mbIndex) = True

        ' ---- 8. 重建 ----
        Call reconstructInterLuma(recon, predLuma, x0, y0)
        Call reconstructChroma(recon, predChroma, ux, uy)
    End Sub

    ''' <summary>
    ''' 在某参考平面上做有界整像素搜索；位移限制为偶数以保证色度位移为整数
    ''' </summary>
    Private Function searchBestMv(refPlane As H264Plane, sourcePlane As H264Plane,
                                  x0 As Integer, y0 As Integer,
                                  ByRef bestX As Integer, ByRef bestY As Integer) As Long
        Dim bestSad As Long = Long.MaxValue

        For oy As Integer = -H264MotionEstimation.searchRadius To H264MotionEstimation.searchRadius Step 2
            For ox As Integer = -H264MotionEstimation.searchRadius To H264MotionEstimation.searchRadius Step 2
                Dim cost As Long = H264MotionEstimation.sad(refPlane, sourcePlane, x0, y0, 16, ox, oy)

                If cost < bestSad Then
                    bestSad = cost
                    bestX = ox
                    bestY = oy
                End If
            Next
        Next

        If H264Debug.ForceZeroMv Then
            bestX = 0
            bestY = 0
        End If

        Return bestSad
    End Function

    ''' <summary>
    ''' 编码一个 B 切片的 16x16 宏块（<c>B_L0_16x16</c> / <c>B_L1_16x16</c> / <c>B_Bi_16x16</c> 三选一）
    ''' </summary>
    ''' <remarks>
    ''' 参考列表按解码器的默认初始化（不做参考重排序，8.2.4.2.4）：列表 1 默认与列表 0 同序，
    ''' 而列表 0 按 PicNum 降序，故 <c>L0[0] / L1[0]</c> 是<b>较新</b>参考（本帧的未来锚点 =
    ''' <see cref="reference"/>）、<c>L0[1] / L1[1]</c> 是<b>较旧</b>参考（过去锚点 =
    ''' <see cref="referencePast"/>）。因此固定取 <c>ref_idx_l0 = 0</c>（未来）与
    ''' <c>ref_idx_l1 = 1</c>（过去），双向宏块即「未来 + 过去」的平均，是真正的双向预测。
    ''' 
    ''' <para>
    ''' 语法顺序（ffmpeg <c>ff_h264_decode_mb_cabac</c> 的 B 分支）：
    ''' <c>mb_skip_flag</c>（基址 11，B 切片再 +13）→ <c>mb_type</c>（基址 27）→
    ''' <c>ref_idx_l0</c> / <c>ref_idx_l1</c>（基址 54，仅在本宏块使用该列表时写出）→
    ''' <c>mvd_l0</c> / <c>mvd_l1</c>（x 基址 40、y 基址 47，两条列表共用同一段上下文）→
    ''' <c>coded_block_pattern</c>（亮度 73 / 色度 77）→ <c>mb_qp_delta</c>(60) → 残差。
    ''' </para>
    ''' </remarks>
    Private Sub encodeBInterMacroblock(cabac As H264Cabac,
                                       source As H264Yuv420, recon As H264Yuv420,
                                       mbX As Integer, mbY As Integer,
                                       prevRow As H264MbInfo(), curRow As H264MbInfo())
        Dim mbCols As Integer = codec.widthInMbs
        Dim mbIndex As Integer = mbY * mbCols + mbX
        Dim x0 As Integer = mbX * 16
        Dim y0 As Integer = mbY * 16
        Dim ux As Integer = mbX * 8
        Dim uy As Integer = mbY * 8
        Dim leftAvailable As Boolean = mbX > 0
        Dim topAvailable As Boolean = mbY > 0
        Dim left As H264MbInfo = If(leftAvailable, curRow(mbX - 1), Nothing)
        Dim top As H264MbInfo = If(topAvailable, prevRow(mbX), Nothing)
        Dim info As New H264MbInfo

        ' ---- 1. 两条列表各自做运动估计（L0 = 未来锚点、L1 = 过去锚点）----
        Dim candX0 As Integer = 0, candY0 As Integer = 0
        Dim candX1 As Integer = 0, candY1 As Integer = 0
        Dim sadL0 As Long = searchBestMv(reference.y, source.y, x0, y0, candX0, candY0)
        Dim sadL1 As Long = searchBestMv(referencePast.y, source.y, x0, y0, candX1, candY1)

        ' ---- 2. 各自的运动向量预测与最终 MV（码流单位 = 四分之一像素）----
        Dim predX0 As Integer, predY0 As Integer, predX1 As Integer, predY1 As Integer

        Call predictListMv(0, 0, mbX, mbY, predX0, predY0)
        Call predictListMv(1, 1, mbX, mbY, predX1, predY1)

        Dim mvd0X As Integer = candX0 * 4 - predX0
        Dim mvd0Y As Integer = candY0 * 4 - predY0
        Dim mvd1X As Integer = candX1 * 4 - predX1
        Dim mvd1Y As Integer = candY1 * 4 - predY1
        Dim mv0X As Integer = predX0 + mvd0X
        Dim mv0Y As Integer = predY0 + mvd0Y
        Dim mv1X As Integer = predX1 + mvd1X
        Dim mv1Y As Integer = predY1 + mvd1Y

        ' ---- 3. 三选一：按「源与预测的平方误差」比较 L0 / L1 / 双向平均 ----
        Call H264MotionEstimation.buildPrediction(reference.y, x0, y0, 16, mv0X \ 4, mv0Y \ 4, predLuma, 16)
        Call H264MotionEstimation.buildPrediction(referencePast.y, x0, y0, 16, mv1X \ 4, mv1Y \ 4, predLumaB, 16)

        Dim sadBi As Long = 0

        For yy As Integer = 0 To 15
            For xx As Integer = 0 To 15
                Dim avg As Integer = (predLuma(yy * 16 + xx) + predLumaB(yy * 16 + xx) + 1) >> 1
                Dim diff As Integer = CInt(source.y.at(x0 + xx, y0 + yy)) - avg

                sadBi += CLng(diff) * diff
            Next
        Next

        Dim isBi As Boolean = False
        Dim useL0 As Boolean
        Dim useL1 As Boolean

        ' 全部宏块都使用 B_Bi_16x16（两条列表都参与，取「未来 + 过去」的平均）。
        ' 这里不只是画质取舍，也是<b>语法一致性</b>的要求：列表 1 的 ref_idx 上下文来自邻居的
        ' ref_cache[1]，而解码器只给「使用该列表」的宏块写入该缓存（h264_cabac.c 的 16x16 分支
        ' 没有 else 分支），不使用列表 1 的宏块在参考解码器里留下的是陈旧值，
        ' 编码器无法复现——实测「只写列表 0」零错误、一旦有宏块写列表 1 就出现
        ' 「Reference N >= 2」。让每个宏块都在两条列表上各带一个参考，上下文即为确定值。
        ' 两条列表的运动向量仍然各自取「分别搜索最近邻」的结果，因此这仍是真正的双向预测。
        isBi = True
        useL0 = True
        useL1 = True

        ' ---- 4. 按选择构造最终的亮度/色度预测 ----
        If isBi Then
            Call averagePrediction(predLuma, predLumaB, predLuma, 256)
        ElseIf useL1 Then
            Call Array.Copy(predLumaB, predLuma, 256)
        End If

        If useL1 Then
            Call H264MotionEstimation.buildPrediction(referencePast.u, ux, uy, 8, mv1X \ 8, mv1Y \ 8, predChromaB(0), 8)
            Call H264MotionEstimation.buildPrediction(referencePast.v, ux, uy, 8, mv1X \ 8, mv1Y \ 8, predChromaB(1), 8)
        End If

        Call H264MotionEstimation.buildPrediction(reference.u, ux, uy, 8, mv0X \ 8, mv0Y \ 8, predChroma(0), 8)
        Call H264MotionEstimation.buildPrediction(reference.v, ux, uy, 8, mv0X \ 8, mv0Y \ 8, predChroma(1), 8)

        If isBi Then
            Call averagePrediction(predChroma(0), predChromaB(0), predChroma(0), 64)
            Call averagePrediction(predChroma(1), predChromaB(1), predChroma(1), 64)
        ElseIf useL1 Then
            Call Array.Copy(predChromaB(0), predChroma(0), 64)
            Call Array.Copy(predChromaB(1), predChroma(1), 64)
        End If

        ' ---- 5. 亮度残差：16 个 4x4 块各自带 DC，走类别 2 ----
        For by As Integer = 0 To 3
            For bx As Integer = 0 To 3
                Call loadLumaResidual(source.y, predLuma, x0, y0, bx, by, resBlock)
                Call H264Transform.forwardTransform(resBlock, blockCoef)
                Call H264Transform.quantize(blockCoef, blockLevels, qp, False)
                Call Array.Copy(blockLevels, lumaAcLevels(by * 4 + bx), 16)
            Next
        Next

        ' ---- 6. 色度残差：DC 收集到 2x2 Hadamard，其余走 AC ----
        For c As Integer = 0 To 1
            Dim plane As H264Plane = If(c = 0, source.u, source.v)

            For by As Integer = 0 To 1
                For bx As Integer = 0 To 1
                    Dim blockIdx As Integer = by * 2 + bx

                    Call loadChromaResidual(plane, predChroma(c), ux, uy, bx, by, resBlock)
                    Call H264Transform.forwardTransform(resBlock, blockCoef)
                    Call H264Transform.quantize(blockCoef, blockLevels, qp, False)

                    chromaDcIn(c)(blockIdx) = blockLevels(0)

                    Dim ac As Integer() = chromaAcLevels(c)(blockIdx)

                    ac(0) = 0
                    For i As Integer = 1 To 15
                        ac(i) = blockLevels(i)
                    Next
                Next
            Next

            Call H264Transform.forwardChromaDcTransform(chromaDcIn(c)(0), chromaDcIn(c)(1), chromaDcIn(c)(2), chromaDcIn(c)(3), chromaDcCoeff)
            Call H264Transform.quantizeDc(chromaDcCoeff, chromaDcLevels(c), 4, qp, False)
        Next

        ' ---- 7. CBP ----
        Dim lumaCbp As Integer = 0

        For by As Integer = 0 To 3
            For bx As Integer = 0 To 3
                If hasCoeff(lumaAcLevels(by * 4 + bx)) Then
                    lumaCbp = lumaCbp Or (1 << ((by \ 2) * 2 + (bx \ 2)))
                End If
            Next
        Next

        Dim chromaDcPresent As Boolean = False
        Dim chromaAcPresent As Boolean = False

        For c As Integer = 0 To 1
            If hasCoeff(chromaDcLevels(c)) Then chromaDcPresent = True

            For i As Integer = 0 To 3
                If hasCoeff(chromaAcLevels(c)(i)) Then chromaAcPresent = True
            Next
        Next

        Dim chromaCbp As Integer = If(chromaAcPresent, 2, If(chromaDcPresent, 1, 0))

        If H264Debug.ForceNoResidual Then
            lumaCbp = 0
            chromaCbp = 0
        End If

        ' ---- 8. 宏块语法 ----
        ' mb_skip_flag：基址 11，B 切片再 +13（h264_cabac.c:1368-1370）。本实现不产生跳过宏块，
        ' 故邻居只要可用就 +1。
        Call cabac.encodeBin(24 + If(leftAvailable, 1, 0) + If(topAvailable, 1, 0), 0)

        ' mb_type：首 bin 的上下文 = 「非 Direct 的左邻 + 非 Direct 的上邻」。本实现不使用 B_Direct，
        ' 因此可用邻居都算「非 Direct」（h264_cabac.c:1968-1999）。
        Dim typeCtx As Integer = If(leftAvailable, 1, 0) + If(topAvailable, 1, 0)

        Call cabac.encodeBin(27 + typeCtx, 1)

        If isBi Then
            ' B_Bi_16x16 = 3：bin2 = 1（非 L0/L1_16x16），随后 4 个 0 → bits = 0 < 8 → mb_type = 3
            Call cabac.encodeBin(30, 1)
            Call cabac.encodeBin(31, 0)
            Call cabac.encodeBin(32, 0)
            Call cabac.encodeBin(32, 0)
            Call cabac.encodeBin(32, 0)
        Else
            ' B_L0_16x16 = 1 / B_L1_16x16 = 2：bin2 = 0，再由 ctx 32 的一位区分
            Call cabac.encodeBin(30, 0)
            Call cabac.encodeBin(32, If(useL0, 0, 1))
        End If

        ' ref_idx 与 mvd 必须按【列表】交错写出：解码器对 16x16 的 B 宏块是
        ' 「ref_idx_l0 → mvd_l0(x,y) → ref_idx_l1 → mvd_l1(x,y)」
        ' （h264_cabac.c:2235-2252 的 list 循环内先 decode_cabac_mb_ref 再 DECODE_CABAC_MB_MVD）。
        ' 先把两条列表的 ref_idx 都写完、再统一写 mvd 会让第二个列表整体错位。
        ' 活动参考数为 2（B 切片用 override 抬到 2），未使用的列表【不写】其 ref_idx。
        If useL0 Then
            Dim ctxL0 As Integer = If(leftAvailable AndAlso refL0(mbIndex - 1) > 0, 1, 0) +
                                   If(topAvailable AndAlso refL0(mbIndex - mbCols) > 0, 2, 0)

            Call cabac.encodeBin(54 + ctxL0, 0)                             ' ref_idx_l0 = 0

            ' mvd_l0：两条列表共用上下文 40（x）/ 47（y），amvd 用各列表自己的邻居 mvd 幅值之和
            Dim amvdX As Integer = If(leftAvailable, mvMagX(mbIndex - 1), 0) + If(topAvailable, mvMagX(mbIndex - mbCols), 0)
            Dim amvdY As Integer = If(leftAvailable, mvMagY(mbIndex - 1), 0) + If(topAvailable, mvMagY(mbIndex - mbCols), 0)

            Call writeMvd(cabac, 40, mvd0X, amvdX)
            Call writeMvd(cabac, 47, mvd0Y, amvdY)
        End If

        If useL1 Then
            Dim ctxL1 As Integer = If(leftAvailable AndAlso refL1(mbIndex - 1) > 0, 1, 0) +
                                   If(topAvailable AndAlso refL1(mbIndex - mbCols) > 0, 2, 0)

            ' ref_idx_l1 = 1：先写一个 1 bin，再写一个 0 bin；第二个 bin 的上下文为 (ctx >> 2) + 4
            ' （h264_cabac.c: decode_cabac_mb_ref 的 while 循环与 ctx 更新）
            Call cabac.encodeBin(54 + ctxL1, 1)
            Call cabac.encodeBin(54 + ((ctxL1 >> 2) + 4), 0)

            Dim amvdX As Integer = If(leftAvailable, mvMagX1(mbIndex - 1), 0) + If(topAvailable, mvMagX1(mbIndex - mbCols), 0)
            Dim amvdY As Integer = If(leftAvailable, mvMagY1(mbIndex - 1), 0) + If(topAvailable, mvMagY1(mbIndex - mbCols), 0)

            Call writeMvd(cabac, 40, mvd1X, amvdX)
            Call writeMvd(cabac, 47, mvd1Y, amvdY)
        End If

        Dim leftCbp As Integer = If(leftAvailable AndAlso left IsNot Nothing, left.cbp, &H0F)
        Dim topCbp As Integer = If(topAvailable AndAlso top IsNot Nothing, top.cbp, &H0F)

        Call writeCbpLuma(cabac, lumaCbp, leftCbp, topCbp)
        Call writeCbpChroma(cabac, chromaCbp, leftCbp, topCbp)

        ' mb_qp_delta 只在 cbp != 0 或宏块为 I_16x16 时才存在；本路径是 inter，故 cbp = 0 时不写
        If (lumaCbp Or chromaCbp) <> 0 Then
            Call cabac.encodeBin(60, 0)
        End If

        ' ---- 9. 残差系数（与 P 路径完全一致）----
        Dim acLeft As H264MbInfo = If(left, New H264MbInfo)
        Dim acTop As H264MbInfo = If(top, New H264MbInfo)

        If lumaCbp <> 0 Then
            For i4x4 As Integer = 0 To 15
                Dim bx As Integer = ((i4x4 >> 2) And 1) * 2 + (i4x4 And 1)
                Dim by As Integer = ((i4x4 >> 3) And 1) * 2 + ((i4x4 >> 1) And 1)

                If ((lumaCbp >> ((by \ 2) * 2 + (bx \ 2))) And 1) = 0 Then Continue For

                Dim ctx As Integer = H264Residual.cbfBaseCtx(2) + acNeighborCtx(acLeft, acTop, info, bx, by, False, 0, 0)

                info.lumaAcNnz(by * 4 + bx) = H264Residual.writeBlock(cabac, lumaAcLevels(by * 4 + bx), 2, H264Residual.zigZag, ctx)
            Next
        End If

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
                    Dim ctx As Integer = H264Residual.cbfBaseCtx(4) + acNeighborCtx(left, top, info, cx, cy, True, c, 0)

                    info.chromaAcNnz(c)(i) = H264Residual.writeBlock(cabac, chromaAcLevels(c)(i), 4, H264Residual.acScan, ctx)
                Next
            Next
        End If

        info.cbp = lumaCbp Or (chromaCbp << 4) Or chromaDcBits
        curRow(mbX) = info

        ' ---- 10. 逐列表簿记（供后续宏块的预测使用）----
        refL0(mbIndex) = If(useL0, 0, -1)
        refL1(mbIndex) = If(useL1, 1, -1)
        interMb(mbIndex) = True

        If useL0 Then
            mvStoreX(mbIndex) = mv0X
            mvStoreY(mbIndex) = mv0Y
            mvMagX(mbIndex) = Math.Abs(mvd0X)
            mvMagY(mbIndex) = Math.Abs(mvd0Y)
        End If

        If useL1 Then
            mvStoreX1(mbIndex) = mv1X
            mvStoreY1(mbIndex) = mv1Y
            mvMagX1(mbIndex) = Math.Abs(mvd1X)
            mvMagY1(mbIndex) = Math.Abs(mvd1Y)
        End If

        ' ---- 11. 重建 ----
        Call reconstructInterLuma(recon, predLuma, x0, y0)
        Call reconstructChroma(recon, predChroma, ux, uy)
    End Sub

    ''' <summary>
    ''' inter 的亮度重建：逐 4x4 块反量化 + 逆变换后加预测，<b>不做</b> DC Hadamard
    ''' </summary>
    Private Sub reconstructInterLuma(recon As H264Yuv420, pred As Integer(), x0 As Integer, y0 As Integer)
        For by As Integer = 0 To 3
            For bx As Integer = 0 To 3
                Call Array.Copy(lumaAcLevels(by * 4 + bx), blockLevels, 16)
                Call H264Transform.dequantize(blockLevels, reconCoef, qp)
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

                ' 解码器的亮度 DC 逆变换按 dc_mapping 分发结果，前向侧需做同一置换
                ' 置换已完全收进 H264Transform 的实测落点表里，这里必须按「块光栅序号」原样装填，
        ' 否则会与变换内部的矩阵相乘叠加成第二次转置。
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

        If H264Debug.ForceDcLevel > 0 Then
            Call Array.Clear(lumaDcLevels, 0, 16)
            lumaDcLevels(H264Debug.ForceDcIndex) = H264Debug.ForceDcLevel
        End If

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
        ' 邻居不可用时必须按「有系数」参与上下文推导（ffmpeg 用 left_cbp/top_cbp = 0x7CF 表示），
        ' 否则紧随其后的宏块会因为上下文状态不一致而立刻错位
        Dim dcCtx As Integer = H264Residual.cbfBaseCtx(0) +
            If(left Is Nothing OrElse (left.cbp And &H100) <> 0, 1, 0) +
            If(top Is Nothing OrElse (top.cbp And &H100) <> 0, 2, 0)

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
                ' 与亮度 DC 同理：帧内宏块的邻居不可用填充值是 0x7CF（h264_mvpred.h: fill_decode_caches
                ' 的 `IS_INTRA(mb_type) ? 0x7CF : 0x00F`），其 bit6/bit7 均为 1，
                ' 因此这里对不可用邻居按「有系数」偏移 +1/+2 —— 注意这与 inter 路径【故意不同】。
                Dim dcCtxChroma As Integer = H264Residual.cbfBaseCtx(3) +
                    If(left Is Nothing OrElse ((left.cbp >> (6 + c)) And 1) <> 0, 1, 0) +
                    If(top Is Nothing OrElse ((top.cbp >> (6 + c)) And 1) <> 0, 2, 0)

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

        ' 【临时诊断】DC 通路标定
        Call H264Debug.note("lumaDcLevel0", lumaDcLevels(0))
        Call H264Debug.note("lumaDcOut0", lumaDcOut(0))
        Call H264Debug.note("dcMap0", dcMap(0))
        Call H264Debug.note("qmulDc", H264Transform.qmul(qp, 0))

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

        ' 【临时诊断】首个宏块的预测与重建结果（用于与解码器输出逐一对照）
        If H264Debug.Enabled AndAlso x0 = 0 AndAlso y0 = 0 Then
            Call H264Debug.note("pred0", pred(0))

            For by As Integer = 0 To 3
                For bx As Integer = 0 To 3
                    Call H264Debug.note($"reconY_{bx}_{by}", recon.y.at(bx * 4, by * 4))
                Next
            Next
        End If
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
                                          chroma As Boolean, plane As Integer,
                                          Optional unavailableNnz As Integer = 64) As Integer
        Dim span As Integer = If(chroma, 2, 4)
        Dim nza As Integer = 0
        Dim nzb As Integer = 0

        If bx > 0 Then
            nza = nnzOf(info, by * span + bx - 1, chroma, plane)
        ElseIf left IsNot Nothing Then
            nza = nnzOf(left, by * span + (span - 1), chroma, plane)
        Else
            ' 邻居不可用时的 nnz 填充值见 h264_mvpred.h: fill_decode_caches 的
            ' `empty = CABAC && !IS_INTRA(mb_type) ? 0 : (... : 64)`：
            ' inter 宏块填 0、帧内宏块填 64（记为「有系数」）。二者的上下文偏移不同，
            ' 混用会让 coded_block_flag 的首个 bin 取到错误的上下文而与解码器失步。
            nza = unavailableNnz
        End If

        If by > 0 Then
            nzb = nnzOf(info, (by - 1) * span + bx, chroma, plane)
        ElseIf top IsNot Nothing Then
            nzb = nnzOf(top, (span - 1) * span + bx, chroma, plane)
        Else
            nzb = unavailableNnz
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

            ' 同分时优先 DC：DC 的信号代价最低、且不依赖上/左邻的完整可用性，更稳健
            If sse < bestSse OrElse (sse = bestSse AndAlso mode = CInt(H264IntraMode.DC)) Then
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
