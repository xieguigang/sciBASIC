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

Imports Bitmap = System.Drawing.Bitmap
Imports ImageCodecInfo = System.Drawing.Imaging.ImageCodecInfo
Imports ImageFormat = System.Drawing.Imaging.ImageFormat
Imports EncoderParameter = System.Drawing.Imaging.EncoderParameter
Imports EncoderParameters = System.Drawing.Imaging.EncoderParameters

' 本命名空间下也存在名为 Encoder 的类型，且 Microsoft.VisualBasic.Math 会覆盖 System.Math
Imports std = System.Math

''' <summary>
''' Motion JPEG 编码器（fourCC <c>'MJPG'</c>）：每一帧都是一张标准的 JPEG 图片。
''' </summary>
''' <remarks>
''' 帧间不共享信息，因此任意一帧都可以独立解码，容错性最好；压缩比则由 JPEG 画质参数控制。
''' 视频流中的 MJPEG 帧必须是"裸" JPEG 字节流（JFIF/EXIF 头都可以），这里直接使用
''' <see cref="System.Drawing.Common"/> 自带的 JPEG 编码器，所以不引入新的依赖。
''' </remarks>
Public Class MjpegCodec : Inherits AviVideoCodec

    ''' <summary>
    ''' JPEG 画质，取值范围 0 - 100，数值越小压缩比越高。
    ''' </summary>
    Public Property Quality As Integer

    ReadOnly jpeg As ImageCodecInfo

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
        Using ms As New MemoryStream
            Using parameters As New EncoderParameters(1)
                ' 注意：这里必须写全称，本命名空间下也存在一个名为 Encoder 的类型
                parameters.Param(0) = New EncoderParameter(System.Drawing.Imaging.Encoder.Quality, CLng(Quality))
                Call bitmap.Save(ms, jpeg, parameters)
            End Using

            Return ms.ToArray
        End Using
    End Function

    Private Shared Function getJpegEncoder() As ImageCodecInfo
        For Each codec As ImageCodecInfo In ImageCodecInfo.GetImageEncoders()
            If codec.FormatID = ImageFormat.Jpeg.Guid Then
                Return codec
            End If
        Next

        Throw New NotSupportedException("no JPEG encoder was found in the current GDI+ installation!")
    End Function

End Class
