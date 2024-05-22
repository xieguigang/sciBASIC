#Region "Microsoft.VisualBasic::f83090b249f368ecc6c293035e6a5586, gr\avi\AVIMainHeader.vb"

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

    '   Total Lines: 111
    '    Code Lines: 53 (47.75%)
    ' Comment Lines: 48 (43.24%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (9.01%)
    '     File Size: 3.74 KB


    ' Class AVIMainHeader
    ' 
    '     Properties: cb, dwFlags, dwHeight, dwInitialFrames, dwMaxBytesPerSec
    '                 dwMicroSecPerFrame, dwPaddingGranularity, dwReserved, dwStreams, dwSuggestedBufferSize
    '                 dwTotalFrames, dwWidth
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Write
    ' 
    ' /********************************************************************************/

#End Region

Public Class AVIMainHeader

    Public Const Magic As String = "avih"

    ''' <summary>
    ''' 本数据结构的大小，不包括最初的8个字节（fcc和cb两个域）
    ''' </summary>
    ''' <returns></returns>
    Public Property cb As Integer = 56
    ''' <summary>
    ''' 视频帧间隔时间（以毫秒为单位）
    ''' </summary>
    ''' <returns></returns>
    Public Property dwMicroSecPerFrame As Integer = 66665
    ''' <summary>
    ''' 这个AVI文件的最大数据率
    ''' </summary>
    ''' <returns></returns>
    Public Property dwMaxBytesPerSec As Integer = 0
    ''' <summary>
    ''' 数据填充的粒度
    ''' </summary>
    ''' <returns></returns>
    Public Property dwPaddingGranularity As Integer = 2
    ''' <summary>
    ''' AVI文件的全局标记，比如是否含有索引块等
    ''' </summary>
    ''' <returns></returns>
    Public Property dwFlags As Integer = 0
    ''' <summary>
    ''' 总帧数
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property dwTotalFrames As Integer
        Get
            Return encoder.streams.Select(Function(i) i.frames.Count).Sum
        End Get
    End Property

    ''' <summary>
    ''' 为交互格式指定初始帧数（非交互格式应该指定为0）
    ''' </summary>
    ''' <returns></returns>
    Public Property dwInitialFrames As Integer = 0
    ''' <summary>
    ''' 本文件包含的流的个数
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property dwStreams As Integer
        Get
            Return encoder.streams.Count
        End Get
    End Property

    ''' <summary>
    ''' 建议读取本文件的缓存大小（应能容纳最大的块）
    ''' </summary>
    ''' <returns></returns>
    Public Property dwSuggestedBufferSize As Integer = 0
    ''' <summary>
    ''' 视频图像的宽（以像素为单位）
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property dwWidth As Integer
        Get
            Return encoder.settings.width
        End Get
    End Property

    ''' <summary>
    ''' 视频图像的高（以像素为单位）
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property dwHeight As Integer
        Get
            Return encoder.settings.height
        End Get
    End Property

    ''' <summary>
    ''' 保留的4个零
    ''' </summary>
    ''' <returns></returns>
    Public Property dwReserved As Integer()

    Dim encoder As Encoder

    Sub New(encoder As Encoder)
        Me.encoder = encoder
    End Sub

    Public Sub Write(buffer As UInt8Array)
        buffer.writeString(24, Magic)              ' avih chunk
        buffer.writeInt(28, cb)                    ' avih size

        buffer.writeInt(32, dwMicroSecPerFrame)
        buffer.writeInt(36, dwMaxBytesPerSec)      ' MaxBytesPerSec
        buffer.writeInt(40, dwPaddingGranularity)  ' Padding (In bytes)
        buffer.writeInt(44, dwFlags)               ' Flags
        buffer.writeInt(48, dwTotalFrames)         ' Total Frames
        buffer.writeInt(52, dwInitialFrames)       ' Initial Frames
        buffer.writeInt(56, dwStreams)             ' Total Streams
        buffer.writeInt(60, dwSuggestedBufferSize) ' Suggested Buffer size
        buffer.writeInt(64, dwWidth)               ' pixel width
        buffer.writeInt(68, dwHeight)              ' pixel height
        buffer.writeInt(72, 0)                     '; Reserved int[4]
        buffer.writeInt(76, 0)                     ';
        buffer.writeInt(80, 0)                     ';
        buffer.writeInt(84, 0)                     ';
    End Sub
End Class
