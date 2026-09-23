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
''' AVI 视频流的压缩方式。
''' </summary>
Public Enum AviCodec
    ''' <summary>
    ''' 未压缩的 32 位 DIB（fourCC <c>'DIB '</c>），即改造前的原始输出方式，体积最大。
    ''' </summary>
    DIB = 0
    ''' <summary>
    ''' Motion JPEG（fourCC <c>'MJPG'</c>），默认压缩方式：每帧为一张标准 JPEG 图片。
    ''' </summary>
    MJPEG = 1
    ''' <summary>
    ''' MPEG-4 Part 2 ASP，使用 DivX 标识（fourCC <c>'DIVX'</c>）。
    ''' </summary>
    DivX = 2
    ''' <summary>
    ''' MPEG-4 Part 2 ASP，使用 XviD 标识（fourCC <c>'XVID'</c>）。
    ''' </summary>
    XviD = 3
End Enum

''' <summary>
''' <see cref="AviCodec"/> 到具体编解码器实例的工厂。
''' </summary>
Public NotInheritable Class AviVideoCodecs

    ''' <summary>
    ''' 默认压缩画质（0 - 100）。
    ''' </summary>
    Public Const DefaultQuality As Integer = 90

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 为指定的压缩方式创建编解码器。
    ''' </summary>
    ''' <param name="type">压缩方式</param>
    ''' <param name="fps">流帧率</param>
    ''' <param name="width">画面宽度</param>
    ''' <param name="height">画面高度</param>
    ''' <param name="quality">压缩画质（0 - 100）</param>
    Public Shared Function Create(type As AviCodec,
                                  fps As Integer,
                                  width As Integer,
                                  height As Integer,
                                  Optional quality As Integer = DefaultQuality) As AviVideoCodec

        Select Case type
            Case AviCodec.DIB
                Return New DibCodec(fps, width, height)
            Case AviCodec.MJPEG
                Return New MjpegCodec(fps, width, height, quality)
            Case Else
                Throw New ArgumentException($"unsupported avi video codec: {type}", NameOf(type))
        End Select
    End Function

    ''' <summary>
    ''' 把四字符编码转换为 <c>biCompression</c> 所需要的小端 32 位整数。
    ''' </summary>
    Public Shared Function FourCC(value As String) As Integer
        If String.IsNullOrEmpty(value) OrElse value.Length <> 4 Then
            Throw New ArgumentException($"a fourCC value should be exactly 4 characters, but got: '{value}'", NameOf(value))
        End If

        Return Asc(value.Chars(0)) Or
            (Asc(value.Chars(1)) << 8) Or
            (Asc(value.Chars(2)) << 16) Or
            (Asc(value.Chars(3)) << 24)
    End Function

End Class
