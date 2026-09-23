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

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' 位图与原始像素字节流之间的转换辅助函数。
''' </summary>
''' <remarks>
''' 本命名空间下的 <see cref="Bitmap"/> 是 Core 项目提供的跨平台内存位图
''' （<c>Microsoft.VisualBasic.Imaging.Bitmap</c>），与 GDI+ 的 <c>System.Drawing.Bitmap</c>
''' 并不是同一个类型。这里统一通过 <see cref="BitmapBuffer"/> 的像素枚举访问数据，
''' 因此不需要任何 GDI+ 依赖，也保证与旧版本写入的未压缩画面逐字节一致。
''' </remarks>
Friend NotInheritable Class PixelData

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 读取一帧位图的像素数据，返回 top-down、行优先的 BGRA 字节流（每像素 4 字节）。
    ''' </summary>
    Public Shared Function ToBgraBytes(bitmap As Bitmap) As Byte()
        Dim buffer As BitmapBuffer = bitmap.MemoryBuffer
        Dim bytes As Byte() = New Byte(buffer.Width * buffer.Height * 4 - 1) {}
        Dim i As Integer = 0

        For Each pixel As Color In buffer.AsEnumerable
            bytes(i) = pixel.B
            bytes(i + 1) = pixel.G
            bytes(i + 2) = pixel.R
            bytes(i + 3) = pixel.A
            i += 4
        Next

        Return bytes
    End Function

    ''' <summary>
    ''' 从 RGBA 字节流（每像素 4 字节，r/g/b/a 顺序）创建位图。
    ''' </summary>
    Public Shared Function FromRgbaBytes(rgba As Byte(), width As Integer, height As Integer) As Bitmap
        If rgba Is Nothing Then
            Throw New ArgumentNullException(NameOf(rgba))
        End If
        If rgba.Length < width * height * 4 Then
            Throw New ArgumentException($"frame data is too small for a {width}x{height} image: {rgba.Length} bytes", NameOf(rgba))
        End If

        Dim pixels As Color() = New Color(width * height - 1) {}

        For i As Integer = 0 To pixels.Length - 1
            Dim p As Integer = i * 4
            ' 输入顺序为 r/g/b/a
            pixels(i) = Color.FromArgb(rgba(p + 3), rgba(p), rgba(p + 1), rgba(p + 2))
        Next

        Return FromColors(pixels, width, height)
    End Function

    ''' <summary>
    ''' 从按行优先展开的 <see cref="Color"/> 序列创建位图。
    ''' </summary>
    Public Shared Function FromColors(pixels As Color(), width As Integer, height As Integer) As Bitmap
        If pixels Is Nothing Then
            Throw New ArgumentNullException(NameOf(pixels))
        End If
        If pixels.Length < width * height Then
            Throw New ArgumentException($"frame data is too small for a {width}x{height} image: {pixels.Length} pixels", NameOf(pixels))
        End If

        Dim buffer As New BitmapBuffer(pixels, New Size(width, height))

        Return New Bitmap(buffer)
    End Function

End Class
