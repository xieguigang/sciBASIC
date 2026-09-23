#Region "Microsoft.VisualBasic::f92085c39e956f3ad65c4c31c48bfa03, gr\avi\AVIStream.vb"

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

    '   Total Lines: 148
    '    Code Lines: 106 (71.62%)
    ' Comment Lines: 18 (12.16%)
    '    - Xml Docs: 94.44%
    ' 
    '   Blank Lines: 24 (16.22%)
    '     File Size: 5.79 KB


    ' Class AVIStream
    ' 
    '     Properties: fps, frames, height, width
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: writeDataBuffer, writeHeaderBuffer
    ' 
    '     Sub: (+2 Overloads) addFrame, addRGBFrame
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Linq

' 本命名空间下的 Bitmap 是 Core 提供的跨平台内存位图，Color 则来自 System.Drawing.Primitives
Imports Color = System.Drawing.Color

Public Class AVIStream

    Public Property fps As Integer
    Public Property width As Short
    Public Property height As Short
    Public Property frames As New List(Of FrameStream)

    ''' <summary>
    ''' 本视频流所使用的压缩方式。
    ''' </summary>
    Public ReadOnly Property codecType As AviCodec

    ''' <summary>
    ''' 本视频流所使用的编解码器实例，帧数据与流头部描述都由它产生。
    ''' </summary>
    Public ReadOnly Property codec As AviVideoCodec

    ReadOnly temp$

    ''' <summary>
    ''' 创建一个视频流，默认使用 <see cref="AviCodec.MJPEG"/> 压缩。
    ''' </summary>
    Sub New(fps%, width As Short, height As Short)
        Call Me.New(fps, width, height, AviCodec.MJPEG)
    End Sub

    ''' <summary>
    ''' 创建一个指定压缩方式的视频流。
    ''' </summary>
    ''' <param name="fps">帧率</param>
    ''' <param name="width">画面宽度</param>
    ''' <param name="height">画面高度</param>
    ''' <param name="codec">
    ''' 压缩方式；传入 <see cref="AviCodec.DIB"/> 可以继续得到与旧版本一致的未压缩输出。
    ''' </param>
    ''' <param name="quality">压缩画质（0 - 100）</param>
    Sub New(fps%, width As Short, height As Short, codec As AviCodec, Optional quality% = AviVideoCodecs.DefaultQuality)
        Me.fps = fps
        Me.width = width
        Me.height = height
        Me.codecType = codec
        Me.codec = AviVideoCodecs.Create(codec, fps, CInt(width), CInt(height), quality)
        Me.temp = TempFileSystem.GetAppSysTempFile(".avi_frames", App.PID, prefix:=GetHashCode.ToHexString)
    End Sub

    ''' <summary>
    ''' 把一个位图帧添加到帧列表末尾。
    ''' </summary>
    Public Sub addFrame(image As Bitmap)
        Dim payload As Byte() = codec.Encode(image)

        If frames.Count = 0 Then
            payload = prepend(payload, codec.CodecPrivate)
        End If

        frames.Add(New FrameStream(temp, payload))
    End Sub

    ''' <summary>
    ''' 把一帧按行优先展开的像素添加到帧列表末尾。
    ''' </summary>
    Public Sub addFrame(imagePixels As Color())
        Using bitmap As Bitmap = PixelData.FromColors(imagePixels, width, height)
            Call addFrame(bitmap)
        End Using
    End Sub

    ''' <summary>
    ''' Adds a frame-array to the frame list.
    ''' </summary>
    ''' <param name="imgData">
    ''' the data of an image; a flat array containing ``(r, g, b, a)`` values.
    ''' </param>
    Public Sub addRGBFrame(imgData As Byte())
        Using bitmap As Bitmap = PixelData.FromRgbaBytes(imgData, width, height)
            Call addFrame(bitmap)
        End Using
    End Sub

    ''' <summary>
    ''' 把编解码器私有数据（例如 MPEG-4 的 VOL 头）拼接到首个视频帧之前。
    ''' </summary>
    Private Shared Function prepend(payload As Byte(), header As Byte()) As Byte()
        If header Is Nothing OrElse header.Length = 0 Then
            Return payload
        End If

        Dim buf As Byte() = New Byte(header.Length + payload.Length - 1) {}

        Call Buffer.BlockCopy(header, Scan0, buf, Scan0, header.Length)
        Call Buffer.BlockCopy(payload, Scan0, buf, header.Length, payload.Length)

        Return buf
    End Function

    ''' <summary>
    ''' Writes the avi header to a buffer.
    ''' </summary>
    ''' <param name="idx">the stream index</param>
    ''' <param name="dataOffset">the offset of the stream data from the beginning of the file</param>
    ''' <returns></returns>
    Public Function writeHeaderBuffer(stream As UInt8Array, idx%, dataOffset As Long) As Integer
        Dim hexIdx = idx.ToHexString.TrimStart("0"c) & "db"

        If hexIdx = "db" Then hexIdx = "0" & hexIdx
        If hexIdx.Length = 3 Then hexIdx = "0" & hexIdx

        stream.writeString(0, "LIST")
        stream.writeInt(4, 148 + frames.Count * 4 * 2)
        stream.writeString(8, "strl")
        stream.writeString(12, "strh")
        stream.writeInt(16, 56)
        stream.writeString(20, "vids")    ' fourCC
        stream.writeString(24, codec.FourCC) ' 视频的压缩方式
        stream.writeInt(28, 0)            ' Flags
        stream.writeShort(32, 1)          ' Priority
        stream.writeShort(34, 0)          ' Language
        stream.writeInt(36, 0)            ' Initial frames
        stream.writeInt(40, 1)            ' Scale
        stream.writeInt(44, fps)          ' Rate
        stream.writeInt(48, 0)            ' Startdelay
        stream.writeInt(52, frames.Count) ' Length
        stream.writeInt(56, codec.SuggestedBufferSize) ' suggested buffer size
        stream.writeInt(60, -1)       ' quality
        stream.writeInt(64, 0)        ' sampleSize
        stream.writeShort(68, 0)      ' Rect left
        stream.writeShort(70, 0)      ' Rect top
        stream.writeShort(72, width)  ' Rect width
        stream.writeShort(74, height) ' Rect height

        stream.writeString(76, "strf")
        stream.writeInt(80, 40)
        stream.writeInt(84, 40)      ' struct size
        stream.writeInt(88, width)   ' width
        stream.writeInt(92, CInt(height) * codec.HeightSign) ' height：仅未压缩 DIB 使用负值表示 top-down
        stream.writeShort(96, 1)     ' planes
        stream.writeShort(98, codec.BitCount) ' bits per pixel
        stream.writeInt(100, AviVideoCodecs.FourCC(codec.FourCC)) ' compression
        stream.writeInt(104, codec.SuggestedBufferSize) ' image size
        stream.writeInt(108, 0)      ' x pixels per meter
        stream.writeInt(112, 0)      ' y pixels per meter
        stream.writeInt(116, 0)      ' colortable used
        stream.writeInt(120, 0)      ' colortable important

        stream.writeString(124, "indx")
        stream.writeInt(128, 24 + frames.Count * 4 * 2) ' size
        stream.writeShort(132, 2)                       ' LongsPerEntry
        stream.writeBytes(134, {0, &H1})                ' indexSubType + indexType
        stream.writeInt(136, frames.Count)              ' numIndexEntries
        stream.writeString(140, hexIdx)                 ' chunkID
        stream.writeLong(144, dataOffset)               ' data offset
        stream.writeInt(152, 0)                         ' reserved

        Dim offset As Long = 0

        For i As Integer = 0 To Me.frames.Count - 1            ' index entries
            ' 原先这里是writeInt，但是大文件溢出了
            stream.writeLong(156 + i * 8, offset)              ' offset
            stream.writeInt(160 + i * 8, frames(i).length + 8) ' size

            ' 偏移量必须包含 word 对齐的填充字节，否则压缩帧（长度常为奇数）会导致索引错位
            offset += Me.frames(i).chunkSize
        Next

        Return 156 + Me.frames.Count * 4 * 2
    End Function

    ''' <summary>
    ''' Writes the frame data of a stream to the buffer.
    ''' </summary>
    ''' <param name="idx">the stream index</param>
    ''' <returns></returns>
    Public Function writeDataBuffer(buf As UInt8Array, idx As Integer) As Long
        Dim len& = 0
        Dim hexIdx = idx.ToHexString.TrimStart("0"c) & "db"

        If hexIdx = "db" Then hexIdx = "0" & hexIdx
        If hexIdx.Length = 3 Then hexIdx = "0" & hexIdx

        For i As Integer = 0 To Me.frames.Count - 1
            buf.writeString(len, hexIdx)
            buf.writeInt(len + 4, frames(i).length)
            buf.writeBytes(len + 8, frames(i))

            len += frames(i).length + 8

            ' RIFF 要求 chunk 按 word 边界对齐：奇数长度的帧数据需要补一个填充字节
            If frames(i).length Mod 2 = 1 Then
                buf.writeBytes(len, New Byte() {0})
                len += 1
            End If
        Next

        Return len
    End Function
End Class
