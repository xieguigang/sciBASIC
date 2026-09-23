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

Imports System.Runtime.InteropServices

' 本命名空间的父命名空间（Microsoft.VisualBasic.Imaging）下存在一个同名的 Bitmap 类型，
' 因此这里统一使用显式别名，保证解析到 GDI+ 的类型
Imports Bitmap = System.Drawing.Bitmap
Imports BitmapData = System.Drawing.Imaging.BitmapData
Imports ImageLockMode = System.Drawing.Imaging.ImageLockMode
Imports PixelFormat = System.Drawing.Imaging.PixelFormat
Imports Graphics = System.Drawing.Graphics
Imports Point = System.Drawing.Point
Imports Rectangle = System.Drawing.Rectangle
Imports Color = System.Drawing.Color

''' <summary>
''' 位图与原始像素字节流之间的转换辅助函数。
''' </summary>
''' <remarks>
''' 所有函数统一使用 32bppArgb 作为交换格式：该格式在 GDI+ 下每一行的步长恒等于
''' <c>width * 4</c>，因此可以得到紧凑且无行填充的像素缓冲，正好满足 AVI 未压缩
''' DIB 帧的布局要求。
''' </remarks>
Friend NotInheritable Class PixelData

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 把位图读取为 top-down 的 BGRA 字节流（每像素 4 字节）。
    ''' </summary>
    Public Shared Function ToBgraBytes(bitmap As Bitmap) As Byte()
        Dim source As Bitmap = bitmap
        Dim allocated As Boolean = False

        If bitmap.PixelFormat <> PixelFormat.Format32bppArgb Then
            ' GDI+ 只允许在少数格式之间直接 LockBits，所以先统一转换到 32bppArgb
            source = New Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb)
            allocated = True

            Using g As Graphics = Graphics.FromImage(source)
                Call g.DrawImageUnscaled(bitmap, New Point)
            End Using
        End If

        Try
            Dim data As BitmapData = source.LockBits(New Rectangle(0, Scan0, source.Width, source.Height),
                                                     ImageLockMode.ReadOnly,
                                                     PixelFormat.Format32bppArgb)

            Try
                Dim bytes As Byte() = New Byte(source.Width * source.Height * 4 - 1) {}
                Dim line As Byte() = New Byte(data.Stride - 1) {}

                For y As Integer = 0 To source.Height - 1
                    Call Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), line, Scan0, line.Length)
                    Call Buffer.BlockCopy(line, Scan0, bytes, y * source.Width * 4, source.Width * 4)
                Next

                Return bytes
            Finally
                Call source.UnlockBits(data)
            End Try
        Finally
            If allocated Then Call source.Dispose()
        End Try
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

        Dim bitmap As New Bitmap(width, height, PixelFormat.Format32bppArgb)
        Dim data As BitmapData = bitmap.LockBits(New Rectangle(0, Scan0, width, height),
                                                 ImageLockMode.WriteOnly,
                                                 PixelFormat.Format32bppArgb)

        Try
            Dim line As Byte() = New Byte(width * 4 - 1) {}

            For y As Integer = 0 To height - 1
                Dim offset As Integer = y * width * 4

                For x As Integer = 0 To width - 1
                    Dim p As Integer = offset + x * 4
                    Dim i As Integer = x * 4

                    ' 内存中的像素顺序为 BGRA，而输入数据为 RGBA
                    line(i) = rgba(p + 2)
                    line(i + 1) = rgba(p + 1)
                    line(i + 2) = rgba(p)
                    line(i + 3) = rgba(p + 3)
                Next

                Call Marshal.Copy(line, Scan0, IntPtr.Add(data.Scan0, y * data.Stride), line.Length)
            Next
        Finally
            Call bitmap.UnlockBits(data)
        End Try

        Return bitmap
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

        Dim bitmap As New Bitmap(width, height, PixelFormat.Format32bppArgb)
        Dim data As BitmapData = bitmap.LockBits(New Rectangle(0, Scan0, width, height),
                                                 ImageLockMode.WriteOnly,
                                                 PixelFormat.Format32bppArgb)

        Try
            Dim line As Byte() = New Byte(width * 4 - 1) {}

            For y As Integer = 0 To height - 1
                For x As Integer = 0 To width - 1
                    Dim pixel As Color = pixels(y * width + x)
                    Dim i As Integer = x * 4

                    line(i) = pixel.B
                    line(i + 1) = pixel.G
                    line(i + 2) = pixel.R
                    line(i + 3) = pixel.A
                Next

                Call Marshal.Copy(line, Scan0, IntPtr.Add(data.Scan0, y * data.Stride), line.Length)
            Next
        Finally
            Call bitmap.UnlockBits(data)
        End Try

        Return bitmap
    End Function

End Class
