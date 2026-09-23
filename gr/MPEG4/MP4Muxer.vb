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

Imports System.IO

''' <summary>
''' 一个编码后的 MP4 采样（AVCC 长度前缀形式的 NAL 序列）。
''' </summary>
Friend Structure H264Sample
    ''' <summary>采样在样本数据文件中的起始偏移</summary>
    Friend offset As Long
    ''' <summary>采样的字节长度</summary>
    Friend length As Integer
    ''' <summary>是否为随机访问点（I 帧）</summary>
    Friend isKeyFrame As Boolean
End Structure

''' <summary>
''' 把编码后的采样落盘到临时文件，并记录尺寸索引。
''' </summary>
''' <remarks>
''' 与 AVI 侧的帧存储思路一致：图像数据始终留在磁盘上，内存里只保留每个采样几字节的索引，
''' 因此导出长时间动画时内存占用不随帧数增长。
''' </remarks>
Friend Class MP4SampleStore
    Implements IDisposable

    Private ReadOnly tempPath As String
    Private ReadOnly writer As FileStream
    Private ReadOnly items As New List(Of H264Sample)
    Private total As Long

    Friend Sub New(Optional hint As String = "mp4_samples")
        ' 注意：这里必须写全称 System.IO.Path，否则会被同名的 tempPath 字段遮蔽
        Me.tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{hint}_{Guid.NewGuid().ToString("N")}.tmp")
        Me.writer = New FileStream(Me.tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
    End Sub

    Friend ReadOnly Property samples As List(Of H264Sample)
        Get
            Return items
        End Get
    End Property

    Friend ReadOnly Property count As Integer
        Get
            Return items.Count
        End Get
    End Property

    Friend ReadOnly Property totalSize As Long
        Get
            Return total
        End Get
    End Property

    Friend Sub add(data As Byte(), isKeyFrame As Boolean)
        If data Is Nothing OrElse data.Length = 0 Then
            Throw New ArgumentException("an encoded sample should not be empty!", NameOf(data))
        End If

        Call writer.Write(data, 0, data.Length)
        items.Add(New H264Sample With {
            .offset = total,
            .length = data.Length,
            .isKeyFrame = isKeyFrame
        })
        total += data.Length
    End Sub

    ''' <summary>
    ''' 把全部采样按顺序拷贝到目标流
    ''' </summary>
    Friend Sub copyTo(out As Stream)
        Call writer.Flush()
        Call writer.Seek(0, SeekOrigin.Begin)

        Dim buffer As Byte() = New Byte(64 * 1024 - 1) {}
        Dim remain As Long = total

        While remain > 0
            Dim n As Integer = writer.Read(buffer, 0, CInt(Math.Min(buffer.Length, remain)))
            If n <= 0 Then Exit While

            Call out.Write(buffer, 0, n)
            remain -= n
        End While
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Call writer.Dispose()
        Try
            If File.Exists(tempPath) Then Call File.Delete(tempPath)
        Catch ex As Exception
            ' 临时文件删除失败不影响导出结果
        End Try
    End Sub
End Class

''' <summary>
''' 写出容器时一条视频轨所需的信息：帧率、画布尺寸、SPS/PPS 与采样索引。
''' </summary>
Friend Class MP4TrackSource

    Friend ReadOnly Property fps As Integer
    Friend ReadOnly Property width As Integer
    Friend ReadOnly Property height As Integer

    ''' <summary>该轨的 SPS/PPS（avcC 与首帧私有数据都取自这里）</summary>
    Friend ReadOnly Property codec As H264SpsPps

    ''' <summary>该轨的采样索引（图像数据在临时文件中）</summary>
    Friend ReadOnly Property samples As MP4SampleStore

    ''' <summary>
    ''' 每个采样的合成时间偏移（ctts），Nothing 表示不需要 ctts（例如只有 I/P 帧时）
    ''' </summary>
    Friend Property compositionOffsets As List(Of Integer)

    Friend Sub New(fps As Integer, width As Integer, height As Integer, codec As H264SpsPps, samples As MP4SampleStore)
        Me.fps = fps
        Me.width = width
        Me.height = height
        Me.codec = codec
        Me.samples = samples
    End Sub

End Class

''' <summary>
''' MP4（ISO BMFF）容器封装：写出 ftyp / moov / mdat，并支持 faststart（moov 前置）。
''' </summary>
''' <remarks>
''' 采样数据格式为 AVC 的 <c>lengthSizeMinusOne = 3</c>，即每个 NAL 前面是 4 字节大端长度，
''' SPS/PPS 放在 <c>avcC</c> 中（关键帧采样里也会重复携带，便于任意位置起播）。
''' 
''' faststart 的 moov 尺寸与 stco 偏移互相依赖，这里采用与 ffmpeg
''' <c>compute_moov_size</c>（movenc.c:9042）相同的做法：先算出 moov 尺寸，再带着最终偏移重建一次。
''' </remarks>
Friend Class MP4Muxer
    Implements IDisposable

    Private Const MOVIE_TIMESCALE As Integer = 1000
    Private Const LANGUAGE_UND As Integer = &H55C4

    ''' <summary>是否把 moov 前置（faststart）</summary>
    Friend Property fastStart As Boolean = True

    ''' <summary>整体画布尺寸，用于 mvhd 之外的展示信息</summary>
    Friend ReadOnly Property width As Integer

    Friend ReadOnly Property height As Integer

    Sub New(width As Integer, height As Integer)
        Me.width = width
        Me.height = height
    End Sub

    ''' <summary>
    ''' 写出 MP4 文件
    ''' </summary>
    ''' <param name="path">目标文件路径</param>
    ''' <param name="tracks">各条视频轨（顺序即为 mdat 中的存放顺序）</param>
    Friend Sub writeFile(path As String, tracks As List(Of MP4TrackSource))
        If tracks Is Nothing OrElse tracks.Count = 0 Then
            Throw New InvalidOperationException("at least one video stream is required for writing an mp4 file!")
        End If

        For Each track As MP4TrackSource In tracks
            If track.samples.count = 0 Then
                Throw New InvalidOperationException(
                    $"the video stream (fps={track.fps}) has no frame, nothing to write!")
            End If
        Next

        Dim ftyp As Byte() = buildFtyp()
        Dim mdatHeaderSize As Long = If(totalSampleSize(tracks) + 8L > &HFFFFFFFFL, 16L, 8L)
        Dim offsets As Long() = New Long(tracks.Count - 1) {}
        Dim moov As Byte() = buildMoov(tracks, offsets)
        Dim moovSize As Long = moov.Length

        ' 采样在文件中的绝对偏移依赖最终 moov 尺寸，重建到稳定为止（最多几轮）
        For attempt As Integer = 1 To 4
            If fastStart Then
                Call computeOffsets(tracks, offsets, ftyp.Length + moovSize + mdatHeaderSize)
            Else
                Call computeOffsets(tracks, offsets, ftyp.Length + mdatHeaderSize)
            End If

            Dim rebuilt As Byte() = buildMoov(tracks, offsets)

            If rebuilt.Length = moovSize Then
                moov = rebuilt
                Exit For
            End If

            moov = rebuilt
            moovSize = rebuilt.Length
        Next

        Using out As New FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)
            Call out.Write(ftyp, 0, ftyp.Length)

            If fastStart Then
                Call out.Write(moov, 0, moov.Length)
                Call writeMdat(out, tracks)
            Else
                Call writeMdat(out, tracks)
                Call out.Write(moov, 0, moov.Length)
            End If

            Call out.Flush()
        End Using
    End Sub

    Private Shared Function totalSampleSize(tracks As List(Of MP4TrackSource)) As Long
        Dim total As Long = 0

        For Each track As MP4TrackSource In tracks
            total += track.samples.totalSize
        Next

        Return total
    End Function

    ''' <summary>
    ''' 计算每条轨第一个采样的绝对偏移（本实现每条轨只有一个 chunk）
    ''' </summary>
    Private Shared Sub computeOffsets(tracks As List(Of MP4TrackSource), offsets As Long(), firstSampleOffset As Long)
        Dim cursor As Long = firstSampleOffset

        For i As Integer = 0 To tracks.Count - 1
            offsets(i) = cursor
            cursor += tracks(i).samples.totalSize
        Next
    End Sub

    Private Shared Sub writeMdat(out As Stream, tracks As List(Of MP4TrackSource))
        Dim total As Long = totalSampleSize(tracks)
        Dim large As Boolean = total + 8L > &HFFFFFFFFL

        If large Then
            writeU32(out, 1L)                       ' size = 1 表示使用 64 位长度
            writeType(out, "mdat")
            writeU64(out, total + 16L)
        Else
            writeU32(out, total + 8L)
            writeType(out, "mdat")
        End If

        For Each track As MP4TrackSource In tracks
            track.samples.copyTo(out)
        Next
    End Sub

    ''' <summary>
    ''' 组装 moov：mvhd + 每条轨的 trak
    ''' </summary>
    Private Function buildMoov(tracks As List(Of MP4TrackSource), offsets As Long()) As Byte()
        Using w As New Mp4BoxWriter
            Call w.box("moov",
                       Sub(m)
                           Call writeMvhd(m, tracks)
                           For i As Integer = 0 To tracks.Count - 1
                               Call writeTrak(m, tracks(i), i + 1, offsets(i))
                           Next
                       End Sub)

            Return w.toArray
        End Using
    End Function

    ''' <summary>
    ''' ftyp：major_brand = isom，minor_version = 0x200，兼容 isom/iso2/avc1/mp41
    ''' </summary>
    Private Shared Function buildFtyp() As Byte()
        Using w As New Mp4BoxWriter
            Call w.box("ftyp",
                       Sub(m)
                           Call m.writeType("isom")
                           Call m.writeU32(&H200L)
                           Call m.writeType("isom")
                           Call m.writeType("iso2")
                           Call m.writeType("avc1")
                           Call m.writeType("mp41")
                       End Sub)

            Return w.toArray
        End Using
    End Function

    Private Shared Sub writeMvhd(w As Mp4BoxWriter, tracks As List(Of MP4TrackSource))
        Dim duration As Long = 0

        For Each track As MP4TrackSource In tracks
            Dim d As Long = CLng(track.samples.count) * MOVIE_TIMESCALE \ Math.Max(1, track.fps)
            If d > duration Then duration = d
        Next

        Call w.fullBox("mvhd", 0, 0,
                       Sub(m)
                           Call m.writeU32(0L)                      ' creation_time
                           Call m.writeU32(0L)                      ' modification_time
                           Call m.writeU32(MOVIE_TIMESCALE)
                           Call m.writeU32(duration)
                           Call m.writeU32(&H10000L)                ' rate = 1.0
                           Call m.writeU16(&H100)                   ' volume = 1.0
                           Call m.writeU16(0)
                           Call m.writeU32(0L)
                           Call m.writeU32(0L)
                           Call m.writeUnityMatrix()
                           Call m.writeZeros(24)                     ' pre_defined
                           Call m.writeU32(tracks.Count + 1)        ' next_track_ID
                       End Sub)
    End Sub

    Private Shared Sub writeTrak(w As Mp4BoxWriter, track As MP4TrackSource, trackId As Integer, chunkOffset As Long)
        Call w.box("trak",
                   Sub(t)
                       Call writeTkhd(t, track, trackId)
                       Call t.box("mdia",
                                  Sub(m)
                                      Call writeMdhd(m, track)
                                      Call writeHdlr(m)
                                      Call m.box("minf",
                                                 Sub(n)
                                                     Call n.fullBox("vmhd", 0, 1,
                                                                    Sub(v)
                                                                        Call v.writeU16(0)      ' graphicsmode
                                                                        Call v.writeU16(0)
                                                                        Call v.writeU16(0)
                                                                        Call v.writeU16(0)
                                                                    End Sub)
                                                     Call writeDinf(n)
                                                     Call writeStbl(n, track, chunkOffset)
                                                 End Sub)
                                  End Sub)
                   End Sub)
    End Sub

    Private Shared Sub writeTkhd(w As Mp4BoxWriter, track As MP4TrackSource, trackId As Integer)
        Dim duration As Long = CLng(track.samples.count) * MOVIE_TIMESCALE \ Math.Max(1, track.fps)

        Call w.fullBox("tkhd", 0, &H7,
                       Sub(m)
                           Call m.writeU32(0L)
                           Call m.writeU32(0L)
                           Call m.writeU32(trackId)
                           Call m.writeU32(0L)
                           Call m.writeU32(duration)
                           Call m.writeU32(0L)
                           Call m.writeU32(0L)
                           Call m.writeU16(0)               ' layer
                           Call m.writeU16(0)               ' alternate_group
                           Call m.writeU16(0)               ' volume（视频轨为 0）
                           Call m.writeU16(0)
                           Call m.writeUnityMatrix()
                           Call m.writeU32(CLng(track.width) << 16)
                           Call m.writeU32(CLng(track.height) << 16)
                       End Sub)
    End Sub

    Private Shared Sub writeMdhd(w As Mp4BoxWriter, track As MP4TrackSource)
        Call w.fullBox("mdhd", 0, 0,
                       Sub(m)
                           Call m.writeU32(0L)
                           Call m.writeU32(0L)
                           Call m.writeU32(Math.Max(1, track.fps))   ' 轨道时间基 = 帧率
                           Call m.writeU32(track.samples.count)       ' 每个采样 1 个时间单位
                           Call m.writeU16(LANGUAGE_UND)
                           Call m.writeU16(0)
                       End Sub)
    End Sub

    Private Shared Sub writeHdlr(w As Mp4BoxWriter)
        Call w.fullBox("hdlr", 0, 0,
                       Sub(m)
                           Call m.writeU32(0L)
                           Call m.writeType("vide")
                           Call m.writeZeros(12)
                           Call m.writeCString("VideoHandler")
                       End Sub)
    End Sub

    Private Shared Sub writeDinf(w As Mp4BoxWriter)
        Call w.box("dinf",
                   Sub(d)
                       Call d.fullBox("dref", 0, 0,
                                      Sub(r)
                                          Call r.writeU32(1L)                       ' entry_count
                                          Call r.fullBox("url ", 0, 1, Nothing)     ' 自包含
                                      End Sub)
                   End Sub)
    End Sub

    ''' <summary>
    ''' 采样表：stsd(avc1+avcC) / stts / stss / ctts / stsc / stsz / stco
    ''' </summary>
    Private Shared Sub writeStbl(w As Mp4BoxWriter, track As MP4TrackSource, chunkOffset As Long)
        Call w.box("stbl",
                   Sub(b)
                       Call writeStsd(b, track)
                       Call writeStts(b, track)
                       Call writeStss(b, track)
                       Call writeCtts(b, track)
                       Call writeStsc(b, track)
                       Call writeStsz(b, track)
                       Call writeStco(b, track, chunkOffset)
                   End Sub)
    End Sub

    Private Shared Sub writeStsd(w As Mp4BoxWriter, track As MP4TrackSource)
        Call w.fullBox("stsd", 0, 0,
                       Sub(s)
                           Call s.writeU32(1L)          ' entry_count

                           Call s.box("avc1",
                                      Sub(a)
                                          Call a.writeZeros(6)                  ' reserved
                                          Call a.writeU16(1)                    ' data_reference_index
                                          Call a.writeU16(0)                    ' pre_defined
                                          Call a.writeU16(0)                    ' reserved
                                          Call a.writeZeros(12)                 ' pre_defined[3]
                                          Call a.writeU16(track.width)
                                          Call a.writeU16(track.height)
                                          Call a.writeU32(&H480000L)            ' 72 dpi
                                          Call a.writeU32(&H480000L)
                                          Call a.writeU32(0L)
                                          Call a.writeU16(1)                    ' frame_count
                                          Call a.writeBytes(BuildCompressorName())
                                          Call a.writeU16(&H18)                 ' depth
                                          Call a.writeU16(&HFFFF)               ' pre_defined = -1
                                          Call writeAvcc(a, track)
                                      End Sub)
                       End Sub)
    End Sub

    Private Shared Function BuildCompressorName() As Byte()
        ' compassor name 固定 32 字节：1 字节长度 + 31 字节内容
        Dim name As Byte() = New Byte(31) {}
        Dim text As Byte() = System.Text.Encoding.ASCII.GetBytes("AVC Coding")

        name(0) = CByte(text.Length)
        Call Array.Copy(text, 0, name, 1, Math.Min(text.Length, 31))

        Return name
    End Function

    ''' <summary>
    ''' avcC：configurationVersion + profile/compat/level（直接抄 SPS 的字节）+ SPS/PPS 列表
    ''' </summary>
    Private Shared Sub writeAvcc(w As Mp4BoxWriter, track As MP4TrackSource)
        ' avcC 中的参数集必须是「含 NAL 头」的形态（第 0 字节是 NAL 头，第 1/2/3 字节依次是 profile/兼容位/level）
        Dim sps As Byte() = track.codec.spsPayload
        Dim pps As Byte() = track.codec.ppsPayload

        Call w.box("avcC",
                   Sub(a)
                       Call a.writeU8(1)                    ' configurationVersion
                       Call a.writeU8(sps(1))               ' AVCProfileIndication
                       Call a.writeU8(sps(2))               ' profile_compatibility
                       Call a.writeU8(sps(3))               ' AVCLevelIndication
                       Call a.writeU8(&HFF)                 ' 6 位保留 + lengthSizeMinusOne = 3
                       Call a.writeU8(&HE0 Or 1)            ' 3 位保留 + numOfSequenceParameterSets
                       Call a.writeU16(sps.Length)
                       Call a.writeBytes(sps)
                       Call a.writeU8(1)                    ' numOfPictureParameterSets
                       Call a.writeU16(pps.Length)
                       Call a.writeBytes(pps)
                   End Sub)
    End Sub

    Private Shared Sub writeStts(w As Mp4BoxWriter, track As MP4TrackSource)
        ' 恒定帧率：所有采样时长相同，只需一个条目
        Call w.fullBox("stts", 0, 0,
                       Sub(s)
                           Call s.writeU32(1L)
                           Call s.writeU32(track.samples.count)
                           Call s.writeU32(1L)
                       End Sub)
    End Sub

    Private Shared Sub writeStss(w As Mp4BoxWriter, track As MP4TrackSource)
        Dim keys As New List(Of Integer)

        For i As Integer = 0 To track.samples.samples.Count - 1
            If track.samples.samples(i).isKeyFrame Then keys.Add(i + 1)   ' 采样号是 1-based
        Next

        ' 全部都是关键帧时（只有 I 帧）不需要 stss
        If keys.Count = 0 OrElse keys.Count = track.samples.count Then Return

        Call w.fullBox("stss", 0, 0,
                       Sub(s)
                           Call s.writeU32(keys.Count)
                           For Each n As Integer In keys
                               Call s.writeU32(n)
                           Next
                       End Sub)
    End Sub

    Private Shared Sub writeCtts(w As Mp4BoxWriter, track As MP4TrackSource)
        If track.compositionOffsets Is Nothing Then Return

        Dim entries As List(Of Integer) = track.compositionOffsets

        If entries.Count <> track.samples.count Then Return
        If entries.TrueForAll(Function(v) v = entries(0)) AndAlso entries(0) = 0 Then Return

        Call w.fullBox("ctts", 0, 0,
                       Sub(s)
                           Dim runCount As Integer = 1
                           Dim runs As New List(Of Tuple(Of Integer, Integer))

                           For i As Integer = 1 To entries.Count - 1
                               If entries(i) = entries(i - 1) Then
                                   runCount += 1
                               Else
                                   runs.Add(New Tuple(Of Integer, Integer)(runCount, entries(i - 1)))
                                   runCount = 1
                               End If
                           Next

                           runs.Add(New Tuple(Of Integer, Integer)(runCount, entries(entries.Count - 1)))

                           Call s.writeU32(runs.Count)
                           For Each run In runs
                               Call s.writeU32(run.Item1)
                               Call s.writeU32(run.Item2 And &HFFFFFFFFL)
                           Next
                       End Sub)
    End Sub

    Private Shared Sub writeStsc(w As Mp4BoxWriter, track As MP4TrackSource)
        Call w.fullBox("stsc", 0, 0,
                       Sub(s)
                           Call s.writeU32(1L)
                           Call s.writeU32(1L)                        ' first_chunk
                           Call s.writeU32(track.samples.count)       ' samples_per_chunk
                           Call s.writeU32(1L)                        ' sample_description_index
                       End Sub)
    End Sub

    Private Shared Sub writeStsz(w As Mp4BoxWriter, track As MP4TrackSource)
        Call w.fullBox("stsz", 0, 0,
                       Sub(s)
                           Call s.writeU32(0L)                        ' sample_size = 0 表示变长
                           Call s.writeU32(track.samples.count)
                           For Each sample As H264Sample In track.samples.samples
                               Call s.writeU32(sample.length)
                           Next
                       End Sub)
    End Sub

    Private Shared Sub writeStco(w As Mp4BoxWriter, track As MP4TrackSource, chunkOffset As Long)
        Dim large As Boolean = chunkOffset > &HFFFFFFFFL

        Call w.fullBox(If(large, "co64", "stco"), 0, 0,
                       Sub(s)
                           Call s.writeU32(1L)
                           If large Then
                               Call s.writeU64(chunkOffset)
                           Else
                               Call s.writeU32(chunkOffset)
                           End If
                       End Sub)
    End Sub

    Private Shared Sub writeU32(out As Stream, value As Long)
        Call out.WriteByte(CByte((value >> 24) And &HFF))
        Call out.WriteByte(CByte((value >> 16) And &HFF))
        Call out.WriteByte(CByte((value >> 8) And &HFF))
        Call out.WriteByte(CByte(value And &HFF))
    End Sub

    Private Shared Sub writeU64(out As Stream, value As Long)
        Call writeU32(out, value >> 32)
        Call writeU32(out, value And &HFFFFFFFFL)
    End Sub

    Private Shared Sub writeType(out As Stream, value As String)
        Dim data As Byte() = System.Text.Encoding.ASCII.GetBytes(value)

        Call out.Write(data, 0, 4)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' 目前 muxer 不持有非托管资源；采样数据由 MP4TrackSource 自己回收
    End Sub

End Class
