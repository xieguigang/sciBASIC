#Region "Microsoft.VisualBasic::ee65924ddfde92acb04bac7168f1d9ac, gr\avi\test\Module1.vb"

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

    ' 注意：本示例针对 net10.0 版本的 Microsoft.VisualBasic.Imaging.AVIMedia 程序集，
    ' 需要将 test.vbproj 的 TargetFramework 升级到 net10.0 并引用 ..\AVI.NET5.vbproj 之后才能编译运行。

    ' /********************************************************************************/

    ' Summaries:

    '     Module Module1

    '         Sub: Main, writeVideo, requestCodec

    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.AVIMedia

Module Module1

    Sub Main()
        Dim frame As Bitmap = "E:\ba3a4a086e061d95dfc331c47bf40ad163d9ca04.jpg".LoadImage
        Dim fps As Integer = 24
        Dim frames As Integer = 120

        ' 默认压缩方式为 MJPEG：每一帧都是一张标准 JPEG 图片
        Call writeVideo(frame, fps, frames, AviCodec.MJPEG, "X:\mjpeg.avi", quality:=85)

        ' 未压缩回退：与旧版本的输出完全一致，但是文件体积最大
        Call writeVideo(frame, fps, frames, AviCodec.DIB, "X:\uncompressed.avi")

        Pause()
    End Sub

    ''' <summary>
    ''' 按指定的压缩方式把一幅测试画面重复写入一个 avi 文件
    ''' </summary>
    ''' <param name="codec">
    ''' 压缩方式：<see cref="AviCodec.MJPEG"/>（默认）/ <see cref="AviCodec.DIB"/>。
    ''' 另外还有 <see cref="AviCodec.DivX"/> 与 <see cref="AviCodec.XviD"/> 预留的 MPEG-4 标识，
    ''' 但它们目前还没有对应的编码器实现。
    ''' </param>
    ''' <param name="quality">压缩画质（0 - 100），仅对 MJPEG 生效</param>
    Private Sub writeVideo(frame As Bitmap, fps As Integer, frames As Integer,
                           codec As AviCodec, path As String,
                           Optional quality As Integer = AviVideoCodecs.DefaultQuality)

        Dim codecs As AviCodec = requestCodec(codec)
        Dim avi As New Encoder(New Settings With {.width = frame.Width, .height = frame.Height})
        Dim stream As New AVIStream(fps, frame.Width, frame.Height, codecs, quality)

        For i As Integer = 0 To frames
            Call stream.addFrame(frame)
        Next

        Call avi.streams.Add(stream)
        Call avi.WriteBuffer(path)
    End Sub

    ''' <summary>
    ''' 校验压缩方式是否已经实现：MPEG-4（DivX/XviD）目前还没有编码器可供使用
    ''' </summary>
    Private Function requestCodec(codec As AviCodec) As AviCodec
        Select Case codec
            Case AviCodec.MJPEG, AviCodec.DIB
                Return codec
            Case Else
                Call $"the selected avi codec '{codec}' is not implemented yet, use MJPEG instead.".Warning
                Return AviCodec.MJPEG
        End Select
    End Function

End Module
