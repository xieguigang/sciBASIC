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

Imports Microsoft.VisualBasic.Imaging.AVIMedia
Imports Color = System.Drawing.Color
Imports CoreBitmap = Microsoft.VisualBasic.Imaging.Bitmap

''' <summary>
''' MP4 中的一条视频轨，形状对齐 <c>Microsoft.VisualBasic.Imaging.AVIMedia.AVIStream</c>。
''' </summary>
''' <remarks>
''' 三条帧入口与 AVI 侧保持一致：
''' 
''' + <see cref="addFrame(CoreBitmap)"/>：跨平台内存位图
''' + <see cref="addFrame(Color())"/>：按行优先展开的颜色数组
''' + <see cref="addRGBFrame(Byte())"/>：扁平的 (r, g, b, a) 字节数组
''' 
''' 逐帧编码后立即落盘到临时文件，只在内存中保留每个采样几字节的索引，
''' 因此导出长时间动画时内存占用不随帧数增长。
''' </remarks>
Public Class MP4Stream
    Implements IDisposable

    Public Property fps As Integer
    Public Property width As Integer
    Public Property height As Integer

    ''' <summary>画质（0 - 100），映射为编码器的量化参数</summary>
    Public ReadOnly Property quality As Integer

    Friend ReadOnly Property codec As H264VideoCodec
    Friend ReadOnly Property samples As MP4SampleStore

    ''' <summary>
    ''' 每个采样的合成时间偏移（<c>ctts</c>）：显示时间 − 解码时间，单位为帧。
    ''' </summary>
    ''' <remarks>
    ''' 采样按<b>解码顺序</b>追加（B 帧需要此顺序才能引用未来的锚点），
    ''' 而播放顺序由本列表还原——封装层把它写成 <c>ctts</c>。
    ''' 全部为 0 时（只含 I/P 帧）封装层不会写出该 box。
    ''' </remarks>
    Friend ReadOnly Property compositionOffsets As New List(Of Integer)

    ''' <summary>
    ''' 创建一条 H.264 视频轨
    ''' </summary>
    ''' <param name="fps">帧率</param>
    ''' <param name="width">画面宽度</param>
    ''' <param name="height">画面高度</param>
    ''' <param name="quality">画质（0 - 100），越大越清晰、文件越大</param>
    Sub New(fps%, width%, height%, Optional quality% = 90)
        If width <= 0 OrElse height <= 0 Then
            Throw New ArgumentException($"invalid frame size: {width} x {height}")
        End If

        Me.fps = fps
        Me.width = width
        Me.height = height
        Me.quality = If(quality < 0, 0, If(quality > 100, 100, quality))
        Me.codec = New H264VideoCodec(width, height, fps, Me.quality)
        Me.samples = New MP4SampleStore($"mp4_frames_{fps}fps")
    End Sub

    ''' <summary>
    ''' 追加一幅位图帧
    ''' </summary>
    Public Sub addFrame(image As CoreBitmap)
        If image Is Nothing Then
            Throw New ArgumentNullException(NameOf(image))
        End If

        Dim bgra As Byte() = PixelData.ToBgraBytes(image)

        Call addBgraFrame(bgra, image.MemoryBuffer.Width)
    End Sub

    ''' <summary>
    ''' 追加一帧按行优先展开的像素
    ''' </summary>
    Public Sub addFrame(imagePixels As Color())
        If imagePixels Is Nothing Then
            Throw New ArgumentNullException(NameOf(imagePixels))
        End If

        Using bitmap As CoreBitmap = PixelData.FromColors(imagePixels, width, height)
            Call addFrame(bitmap)
        End Using
    End Sub

    ''' <summary>
    ''' 追加一帧扁平的 (r, g, b, a) 像素字节
    ''' </summary>
    Public Sub addRGBFrame(imgData As Byte())
        If imgData Is Nothing Then
            Throw New ArgumentNullException(NameOf(imgData))
        End If

        ' 编码器内部统一使用 BGRA（与位图的内存布局一致），这里做一次通道交换
        Dim bgra As Byte() = New Byte(imgData.Length - 1) {}

        For i As Integer = 0 To bgra.Length - 1 Step 4
            bgra(i) = imgData(i + 2)
            bgra(i + 1) = imgData(i + 1)
            bgra(i + 2) = imgData(i)
            bgra(i + 3) = If(i + 3 < imgData.Length, imgData(i + 3), CByte(255))
        Next

        Call addBgraFrame(bgra, width)
    End Sub

    Private Sub addBgraFrame(bgra As Byte(), pixelWidth As Integer)
        Dim required As Integer = width * height * 4

        If pixelWidth < width OrElse bgra.Length < required Then
            Throw New ArgumentException(
                $"the frame data ({bgra.Length} bytes, row width {pixelWidth}) does not match the canvas size ({width} x {height}) of this mp4 video stream!")
        End If

        ' 一次 addFrame 可能产出 0 - 2 个采样：B 帧要等「未来锚点」到达后才随锚点一起按解码顺序输出
        Call appendEncoded(codec.encodeFrame(bgra, pixelWidth))
    End Sub

    ''' <summary>
    ''' 输出缓冲中剩余的采样。
    ''' </summary>
    ''' <remarks>
    ''' 由 <see cref="MP4Encoder.WriteBuffer"/> 在封装前调用：总帧数为奇数时最后一帧是
    ''' 「没有未来锚点的 B」，编码器会把它改按 P 帧编出，因此这里仍会有采样产出。
    ''' </remarks>
    Friend Sub flush()
        Call appendEncoded(codec.flush())
    End Sub

    Private Sub appendEncoded(frames As List(Of H264EncodedFrame))
        For Each frame As H264EncodedFrame In frames
            ' 首帧为 IDR（关键帧）
            Call samples.add(frame.data, isKeyFrame:=(samples.count = 0))
            ' 采样按解码顺序追加，显示顺序由 ctts 还原
            Call compositionOffsets.Add(frame.compositionOffset)
        Next
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Call samples.Dispose()
    End Sub

End Class
