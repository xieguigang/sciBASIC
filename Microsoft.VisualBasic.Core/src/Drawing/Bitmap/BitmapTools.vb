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

            ' 确保裁剪区域在源图像范围内[4,5](@ref)
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

            Return newBuffer
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