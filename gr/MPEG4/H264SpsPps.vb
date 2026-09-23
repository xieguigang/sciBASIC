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
''' H.264 序列参数集(SPS)与图像参数集(PPS)的序列化。
''' </summary>
''' <remarks>
''' 字段顺序与位宽严格对照 ffmpeg 的解析实现 libavcodec/h264_ps.c
''' （<c>ff_h264_decode_seq_parameter_set</c> @284、<c>ff_h264_decode_picture_parameter_set</c> @698）。
''' 
''' 本实现固定使用：
''' 
''' + <c>profile_idc = 77</c>（Main profile，因此 <b>不写</b> High profile 专属的
'''   chroma_format_idc / bit_depth_* / scaling matrix 字段）
''' + <c>profile_compatibility = 0x40</c>（constraint_set1_flag = 1）。
'''   这一位有实际作用：ffmpeg 的 <c>more_rbsp_data_in_pps()</c>（h264_ps.c:677）对 66/77/88
'''   只要该字节低 3 位非 0 就不会去解析 PPS 扩展段，从而避免把 rbsp 停止位误读成
'''   transform_8x8_mode_flag。
''' + 4:2:0 / 8bit / 逐行 / 无 VUI（帧率信息由 MP4 容器的 stts 承载）
''' </remarks>
Friend NotInheritable Class H264SpsPps

    ''' <summary>Main profile</summary>
    Friend Const PROFILE_IDC As Integer = 77

    ''' <summary>constraint_set1_flag = 1，其余约束位与 reserved_zero_2bits 均为 0</summary>
    Friend Const PROFILE_COMPATIBILITY As Integer = &H40

    ''' <summary>log2_max_frame_num_minus4 = 4 → frame_num 为 u(8)</summary>
    Friend Const LOG2_MAX_FRAME_NUM_MINUS4 As Integer = 4

    ''' <summary>pic_order_cnt_type = 0 时，log2_max_pic_order_cnt_lsb_minus4 = 4 → POC LSB 为 u(8)</summary>
    Friend Const LOG2_MAX_POC_LSB_MINUS4 As Integer = 4

    Friend Const MAX_FRAME_NUM_BITS As Integer = LOG2_MAX_FRAME_NUM_MINUS4 + 4

    Friend Const POC_LSB_BITS As Integer = LOG2_MAX_POC_LSB_MINUS4 + 4

    ''' <summary>编码后的宏块列数</summary>
    Friend ReadOnly Property widthInMbs As Integer

    ''' <summary>编码后的宏块行数</summary>
    Friend ReadOnly Property heightInMapUnits As Integer

    ''' <summary>显示宽度（可能小于编码宽度，多出来的部分靠 frame_cropping 裁掉）</summary>
    Friend ReadOnly Property width As Integer

    ''' <summary>显示高度</summary>
    Friend ReadOnly Property height As Integer

    ''' <summary>SPS 的原始字节（含 NAL 头，Annex-B 起始码）</summary>
    Friend ReadOnly Property sps As Byte()

    ''' <summary>PPS 的原始字节（含 NAL 头，Annex-B 起始码）</summary>
    Friend ReadOnly Property pps As Byte()

    ''' <summary>SPS/PPS 的 RBSP 净荷（不含起始码与 NAL 头），用于写进 MP4 的 avcC</summary>
    Friend ReadOnly Property spsRbsp As Byte()

    Friend ReadOnly Property ppsRbsp As Byte()

    Sub New(width As Integer, height As Integer, Optional maxNumRefFrames As Integer = 2, Optional levelIdc As Integer = 0)
        Me.width = width
        Me.height = height
        ' 编码尺寸必须是 16 的整数倍；色度 4:2:0 的裁剪量需要能被 2 整除，所以必要时多补一个宏块
        Me.widthInMbs = (width + 15) \ 16
        Me.heightInMapUnits = (height + 15) \ 16
        If ((Me.widthInMbs * 16 - width) Mod 2) <> 0 Then Me.widthInMbs += 1
        If ((Me.heightInMapUnits * 16 - height) Mod 2) <> 0 Then Me.heightInMapUnits += 1

        If levelIdc <= 0 Then levelIdc = getLevelIdc(width, height)

        Me.spsRbsp = buildSps(maxNumRefFrames, levelIdc)
        Me.ppsRbsp = buildPps()
        Me.sps = BitStreamWriter.nalUnit(Me.spsRbsp, 3, 7)
        Me.pps = BitStreamWriter.nalUnit(Me.ppsRbsp, 3, 8)
    End Sub

    ''' <summary>裁剪后的显示宽度（像素）</summary>
    Friend ReadOnly Property codedWidth As Integer
        Get
            Return widthInMbs * 16
        End Get
    End Property

    ''' <summary>裁剪后的显示高度（像素）</summary>
    Friend ReadOnly Property codedHeight As Integer
        Get
            Return heightInMapUnits * 16
        End Get
    End Property

    ''' <summary>
    ''' 依据分辨率给出一个够用的 level_idc（Level 3.0 = 30、3.1 = 31、4.0 = 40）。
    ''' </summary>
    Private Shared Function getLevelIdc(width As Integer, height As Integer) As Integer
        Dim mbs As Integer = ((width + 15) \ 16) * ((height + 15) \ 16)

        If mbs <= 396 Then Return 30       ' 到 352x288
        If mbs <= 1620 Then Return 31      ' 到 720x576
        Return 40                          ' 到 1920x1080
    End Function

    Private Function buildSps(maxNumRefFrames As Integer, levelIdc As Integer) As Byte()
        Dim w As New BitStreamWriter(64)

        Call w.writeBits(PROFILE_IDC, 8)
        Call w.writeBits(PROFILE_COMPATIBILITY, 8)
        Call w.writeBits(levelIdc, 8)
        Call w.writeUe(0)                                       ' seq_parameter_set_id
        ' profile_idc = 77：整块 High profile 字段省略
        Call w.writeUe(LOG2_MAX_FRAME_NUM_MINUS4)                ' log2_max_frame_num_minus4
        Call w.writeUe(0)                                       ' pic_order_cnt_type = 0
        Call w.writeUe(LOG2_MAX_POC_LSB_MINUS4)                  ' log2_max_pic_order_cnt_lsb_minus4
        Call w.writeUe(maxNumRefFrames)                          ' max_num_ref_frames
        Call w.writeBit(0)                                      ' gaps_in_frame_num_value_allowed_flag
        Call w.writeUe(widthInMbs - 1)                           ' pic_width_in_mbs_minus1
        Call w.writeUe(heightInMapUnits - 1)                     ' pic_height_in_map_units_minus1
        Call w.writeBit(1)                                      ' frame_mbs_only_flag
        Call w.writeBit(1)                                      ' direct_8x8_inference_flag

        Dim cropRight As Integer = (codedWidth - width) \ 2
        Dim cropBottom As Integer = (codedHeight - height) \ 2

        If cropRight > 0 OrElse cropBottom > 0 Then
            Call w.writeBit(1)                                  ' frame_cropping_flag
            Call w.writeUe(0)                                   ' crop_left
            Call w.writeUe(cropRight)                           ' crop_right
            Call w.writeUe(0)                                   ' crop_top
            Call w.writeUe(cropBottom)                          ' crop_bottom
        Else
            Call w.writeBit(0)                                  ' frame_cropping_flag
        End If

        Call w.writeBit(0)                                      ' vui_parameters_present_flag
        Call w.writeRbspTrailingBits()

        Return w.toArray
    End Function

    Private Function buildPps() As Byte()
        Dim w As New BitStreamWriter(16)

        Call w.writeUe(0)                       ' pic_parameter_set_id
        Call w.writeUe(0)                       ' seq_parameter_set_id
        Call w.writeBit(1)                      ' entropy_coding_mode_flag = 1（CABAC）
        Call w.writeBit(0)                      ' bottom_field_pic_order_in_frame_present_flag
        Call w.writeUe(0)                       ' num_slice_groups_minus1
        Call w.writeUe(0)                       ' num_ref_idx_l0_default_active_minus1
        Call w.writeUe(0)                       ' num_ref_idx_l1_default_active_minus1
        Call w.writeBit(0)                      ' weighted_pred_flag
        Call w.writeBits(0, 2)                  ' weighted_bipred_idc
        Call w.writeSe(0)                       ' pic_init_qp_minus26
        Call w.writeSe(0)                       ' pic_init_qs_minus26
        Call w.writeSe(0)                       ' chroma_qp_index_offset
        Call w.writeBit(1)                      ' deblocking_filter_control_present_flag
        Call w.writeBit(0)                      ' constrained_intra_pred_flag
        Call w.writeBit(0)                      ' redundant_pic_cnt_present_flag
        Call w.writeRbspTrailingBits()

        Return w.toArray
    End Function

End Class
