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
''' H.264 的 slice_type 取值（写入码流的是该值，读到时大于 4 表示整幅图像统一类型）。
''' </summary>
Friend Enum H264SliceType
    P = 0
    B = 1
    I = 2
End Enum

''' <summary>
''' slice header 的序列化。
''' </summary>
''' <remarks>
''' 字段顺序对照 ffmpeg 的 libavcodec/h264_slice.c <c>h264_slice_header_parse</c> @1728。
''' 本实现固定：单参考帧、CABAC、无 weighted prediction、无 FMO、逐行、
''' <c>disable_deblocking_filter_idc = 1</c>（关闭去环滤波）。
''' 
''' 注意两点容易写错的地方：
''' 
''' + <c>frame_num</c> 与 <c>pic_order_cnt_lsb</c> 是 u(v) 定长字段，宽度分别由 SPS 的
'''   log2_max_frame_num / log2_max_pic_order_cnt_lsb_minus4 决定，<b>不能</b>写成 ue(v)；
''' + CABAC 的 slice data 之前要补 <c>cabac_alignment_one_bit</c>（值为 1）对齐到字节边界。
''' </remarks>
Friend NotInheritable Class H264SliceHeader

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 写出一个切片头；返回的位流已经完成 CABAC 前的字节对齐，可以直接接着写宏块数据。
    ''' </summary>
    ''' <param name="sliceType">切片类型</param>
    ''' <param name="isIdr">是否为 IDR（决定 NAL 类型与 dec_ref_pic_marking 分支）</param>
    ''' <param name="frameNum">frame_num，宽度为 8 位</param>
    ''' <param name="picOrderCntLsb">pic_order_cnt_lsb，宽度为 8 位</param>
    ''' <param name="idrPicId">IDR 图像的 idr_pic_id</param>
    ''' <param name="sliceQpDelta">slice_qp_delta（相对 PPS 的 pic_init_qp_minus26）</param>
    ''' <param name="cabacInitIdc">非 I 切片的 cabac_init_idc，取 0 - 2</param>
    ''' <param name="outputStream">已经写好的位流写入器</param>
    Friend Shared Sub write(w As BitStreamWriter,
                            sliceType As H264SliceType,
                            isIdr As Boolean,
                            frameNum As Integer,
                            picOrderCntLsb As Integer,
                            Optional idrPicId As Integer = 0,
                            Optional sliceQpDelta As Integer = 0,
                            Optional cabacInitIdc As Integer = 0,
                            Optional numRefIdxOverride As Boolean = False,
                            Optional numRefIdxL0Minus1 As Integer = 0,
                            Optional numRefIdxL1Minus1 As Integer = 0)

        Call w.writeUe(0)                                               ' first_mb_in_slice
        Call w.writeUe(CInt(sliceType))                                 ' slice_type
        Call w.writeUe(0)                                               ' pic_parameter_set_id
        Call w.writeBits(frameNum, H264SpsPps.MAX_FRAME_NUM_BITS)       ' frame_num（定长 u(v)）

        ' frame_mbs_only_flag = 1 → 不写 field_pic_flag / bottom_field_flag

        If isIdr Then
            Call w.writeUe(idrPicId)                                    ' idr_pic_id
        End If

        ' pic_order_cnt_type = 0
        Call w.writeBits(picOrderCntLsb, H264SpsPps.POC_LSB_BITS)       ' pic_order_cnt_lsb（定长 u(v)）
        ' bottom_field_pic_order_in_frame_present_flag = 0 → 不写 delta_pic_order_cnt_bottom

        If sliceType = H264SliceType.B Then
            Call w.writeBit(1)                                          ' direct_spatial_mv_pred_flag
        End If

        If sliceType <> H264SliceType.I Then
            ' B 切片需要「未来锚点 + 过去锚点」两条列表各自可见，而 PPS 的默认活动参考数是 1，
            ' 故 B 切片用 override 把它抬到 2；P 切片保持默认（写 0），位流与既有已验证结果逐位一致。
            Call w.writeBit(If(numRefIdxOverride, 1, 0))                ' num_ref_idx_active_override_flag

            If numRefIdxOverride Then
                Call w.writeUe(numRefIdxL0Minus1)                       ' num_ref_idx_l0_active_minus1
                If sliceType = H264SliceType.B Then
                    Call w.writeUe(numRefIdxL1Minus1)                   ' num_ref_idx_l1_active_minus1
                End If
            End If

            ' 不做参考重排序：B 切片的列表 1 默认与列表 0 同序（8.2.4.2.4），
            ' 即两者都是 [较新参考, 较旧参考]，用 ref_idx 0 / 1 即可分别取到未来与过去锚点。
            Call w.writeBit(0)                                          ' ref_pic_list_modification_flag_l0
            If sliceType = H264SliceType.B Then
                Call w.writeBit(0)                                      ' ref_pic_list_modification_flag_l1
            End If
        End If

        ' dec_ref_pic_marking（nal_ref_idc != 0 时存在）
        If isIdr Then
            Call w.writeBit(0)                                          ' no_output_of_prior_pics_flag
            Call w.writeBit(0)                                          ' long_term_reference_flag
        Else
            Call w.writeBit(0)                                          ' adaptive_ref_pic_marking_mode_flag
        End If

        If sliceType <> H264SliceType.I Then
            Call w.writeUe(Math.Max(0, Math.Min(2, cabacInitIdc)))      ' cabac_init_idc
        End If

        Call w.writeSe(sliceQpDelta)                                    ' slice_qp_delta
        Call w.writeUe(1)                                               ' disable_deblocking_filter_idc = 1
        ' idc = 1 → 不写 slice_alpha_c0_offset_div2 / slice_beta_offset_div2

        Call w.alignWithOnes()                                          ' cabac_alignment_one_bit
    End Sub

End Class
