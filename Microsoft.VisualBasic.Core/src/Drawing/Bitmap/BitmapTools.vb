#Region "Microsoft.VisualBasic::6c8befff0fb96c113421c9f7df31eaa3, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\BitmapTools.vb"

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

    '   Total Lines: 70
    '    Code Lines: 40 (57.14%)
    ' Comment Lines: 16 (22.86%)
    '    - Xml Docs: 56.25%
    ' 
    '   Blank Lines: 14 (20.00%)
    '     File Size: 3.08 KB


    '     Class BitmapTools
    ' 
    '         Function: CropBitmapBuffer, GetValidCropRectangle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports std = System.Math

Namespace Imaging.BitmapImage

    Public Class BitmapTools

        ''' <summary>
        ''' 裁剪BitmapBuffer的指定区域
        ''' </summary>
        ''' <param name="sourceBuffer">源BitmapBuffer</param>
        ''' <param name="cropRect">裁剪区域</param>
        ''' <returns>裁剪后的新BitmapBuffer</returns>
        Public Shared Function CropBitmapBuffer(sourceBuffer As BitmapBuffer, cropRect As Rectangle) As BitmapBuffer
            ' 验证输入参数
            If sourceBuffer Is Nothing Then
                Throw New ArgumentNullException("sourceBuffer")
            End If

            If sourceBuffer.RawBuffer Is Nothing OrElse sourceBuffer.RawBuffer.Length = 0 Then
                Throw New ArgumentException("源缓冲区为空")
            End If

            If cropRect.Width <= 0 OrElse cropRect.Height <= 0 Then
                Throw New ArgumentException("裁剪区域尺寸无效")
            End If

            ' 确保裁剪区域在源图像范围内
            cropRect = GetValidCropRectangle(cropRect, sourceBuffer.Width, sourceBuffer.Height)

            If cropRect.Width <= 0 OrElse cropRect.Height <= 0 Then
                Throw New ArgumentException("裁剪区域超出图像范围")
            End If

            ' 创建新的BitmapBuffer来存储裁剪结果
            Dim newWidth As Integer = cropRect.Width
            Dim newHeight As Integer = cropRect.Height
            Dim newBuffer As Byte() = New Byte(newWidth * newHeight * 4 - 1) {}

            ' 计算源图像和目标图像的行字节数
            Dim sourceStride As Integer = sourceBuffer.Width * 4
            Dim destStride As Integer = newWidth * 4

            ' 执行像素数据复制
            For y As Integer = 0 To newHeight - 1
                Dim sourceY As Integer = cropRect.Y + y
                ' 计算源和目标行的起始索引
                Dim sourceStartIndex As Integer = sourceY * sourceStride + cropRect.X * 4
                Dim destStartIndex As Integer = y * destStride

                ' 复制整行数据
                Call Array.Copy(sourceBuffer.RawBuffer, sourceStartIndex, newBuffer, destStartIndex, destStride)
            Next

            Return New BitmapBuffer(newBuffer, New Size(newWidth, newHeight), channel:=4)
        End Function

        ''' <summary>
        ''' 确保裁剪矩形在图像有效范围内
        ''' </summary>
        Private Shared Function GetValidCropRectangle(rect As Rectangle, maxWidth As Integer, maxHeight As Integer) As Rectangle
            Dim x As Integer = std.Max(0, std.Min(rect.X, maxWidth - 1))
            Dim y As Integer = std.Max(0, std.Min(rect.Y, maxHeight - 1))
            Dim width As Integer = std.Min(rect.Width, maxWidth - x)
            Dim height As Integer = std.Min(rect.Height, maxHeight - y)

            Return New Rectangle(x, y, width, height)
        End Function
    End Class
End Namespace
