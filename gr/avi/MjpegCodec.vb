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
Imports System.Runtime.InteropServices

' MJPEG 的 JPEG 编码使用项目已经引用的 GDI+（System.Drawing.Common）。
' 由于本命名空间下存在同名的跨平台位图类型与 Encoder 类型，这里通过别名词根进行限定调用。
Imports GdiDrawing = System.Drawing
Imports GdiImaging = System.Drawing.Imaging
Imports std = System.Math

''' <summary>
''' Motion JPEG 编码器（fourCC <c>'MJPG'</c>）：每一帧都是一张标准的 JPEG 图片。
''' </summary>
''' <remarks>
''' 帧间不共享信息，因此任意一帧都可以独立解码，容错性最好；压缩比则由 JPEG 画质参数控制。
''' 视频流中的 MJPEG 帧必须是"裸"的 JPEG 字节流，这里直接使用 GDI+ 自带的 JPEG 编码器，
''' 因此不引入任何新的依赖。
''' 
''' 注意：JPEG 编码步骤需要 GDI+，所以 <see cref="AviCodec.MJPEG"/> 只在 Windows 上可用；
''' 未压缩的 <see cref="AviCodec.DIB"/> 与 MPEG-4 两条链路都是纯托管的。
''' </remarks>
Public Class MjpegCodec : Inherits AviVideoCodec

    ''' <summary>
    ''' JPEG 画质，取值范围 0 - 100，数值越小压缩比越高、画面损失越大。
    ''' </summary>
    Public Property Quality As Integer

    ReadOnly jpeg As GdiImaging.ImageCodecInfo

    Sub New(fps As Integer, width As Integer, height As Integer, Optional quality As Integer = AviVideoCodecs.DefaultQuality)
        MyBase.New(fps, width, height)

        Me.Quality = std.Max(0, std.Min(100, quality))
        Me.jpeg = getJpegEncoder()
    End Sub

    Public Overrides ReadOnly Property FourCC As String
        Get
            Return "MJPG"
        End Get
    End Property

    Public Overrides ReadOnly Property BitCount As Short
        Get
            Return 24
        End Get
    End Property

    ''' <summary>
    ''' JPEG 帧通常远小于未压缩画面，这里给出一个保守上界以免读取端缓冲不足。
    ''' </summary>
    Public Overrides ReadOnly Property SuggestedBufferSize As Integer
        Get
            Return width * height * 3
        End Get
    End Property

    Public Overrides Function Encode(bitmap As Bitmap) As Byte()
        Call validateFrameSize(bitmap)

        Dim bgra As Byte() = PixelData.ToBgraBytes(bitmap)

        Using image As New GdiDrawing.Bitmap(width, height, GdiImaging.PixelFormat.Format32bppArgb)
            Dim data As GdiImaging.BitmapData = image.LockBits(New GdiDrawing.Rectangle(0, 0, width, height),
                                                               GdiImaging.ImageLockMode.WriteOnly,
                                                               GdiImaging.PixelFormat.Format32bppArgb)

            Try
                ' 跨平台位图的像素缓冲就是 top-down 的 BGRA，和 GDI+ 的 32bppArgb 内存布局完全一致
                Call Marshal.Copy(bgra, 0, data.Scan0, bgra.Length)
            Finally
                Call image.UnlockBits(data)
            End Try

            Using ms As New MemoryStream
                Using parameters As New GdiImaging.EncoderParameters(1)
                    parameters.Param(0) = New GdiImaging.EncoderParameter(GdiImaging.Encoder.Quality, CLng(Quality))
                    Call image.Save(ms, jpeg, parameters)
                End Using

                Return ms.ToArray
            End Using
        End Using
    End Function

    Private Shared Function getJpegEncoder() As GdiImaging.ImageCodecInfo
        For Each codec As GdiImaging.ImageCodecInfo In GdiImaging.ImageCodecInfo.GetImageEncoders()
            If codec.FormatID = GdiImaging.ImageFormat.Jpeg.Guid Then
                Return codec
            End If
        Next

        Throw New NotSupportedException("no JPEG encoder was found in the current GDI+ installation!")
    End Function

End Class
