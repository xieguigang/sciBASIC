Imports std = System.Math

Namespace Imaging.BitmapImage

	Public Module BitmapResizer

		Public Function ResizeImage(srcImage(,) As Byte, srcWidth As Integer, srcHeight As Integer, dstWidth As Integer, dstHeight As Integer) As Byte(,)
			Dim dstImage(dstHeight - 1, dstWidth - 1) As Byte
			Dim scaleX As Double = CDbl(srcWidth) / dstWidth
			Dim scaleY As Double = CDbl(srcHeight) / dstHeight

			For dstY As Integer = 0 To dstHeight - 1
				For dstX As Integer = 0 To dstWidth - 1
					' Calculate the corresponding source coordinates
					Dim srcX As Double = (dstX + 0.5) * scaleX - 0.5
					Dim srcY As Double = (dstY + 0.5) * scaleY - 0.5
					Dim x1 As Integer = CInt(std.Truncate(srcX))
					Dim y1 As Integer = CInt(std.Truncate(srcY))
					Dim x2 As Integer = If(x1 < srcWidth - 1, x1 + 1, x1)
					Dim y2 As Integer = If(y1 < srcHeight - 1, y1 + 1, y1)

					' Calculate the weights for interpolation
					Dim xWeight As Double = srcX - x1
					Dim yWeight As Double = srcY - y1

					' Bilinear interpolation
					Dim value As Byte = CByte(std.Truncate((1 - xWeight) * (1 - yWeight) * srcImage(y1, x1) + xWeight * (1 - yWeight) * srcImage(y1, x2) + (1 - xWeight) * yWeight * srcImage(y2, x1) + xWeight * yWeight * srcImage(y2, x2)))

					dstImage(dstY, dstX) = value
				Next dstX
			Next dstY

			Return dstImage
		End Function

		Public Function GetPixelValue(srcImage(,) As Byte, x As Integer, y As Integer, width As Integer, height As Integer, method As EdgeHandlingMethod) As Byte
			Select Case method
				Case EdgeHandlingMethod.Clamp
					x = std.Max(0, std.Min(width - 1, x))
					y = std.Max(0, std.Min(height - 1, y))
				Case EdgeHandlingMethod.Mirror
					If x < 0 Then
						x = -x
					End If
					If x >= width Then
						x = width - 1 - (x - width + 1)
					End If
					If y < 0 Then
						y = -y
					End If
					If y >= height Then
						y = height - 1 - (y - height + 1)
					End If
				Case EdgeHandlingMethod.Fill
					If x < 0 OrElse x >= width OrElse y < 0 OrElse y >= height Then
						Return 0 ' or any other fill color
					End If
				Case EdgeHandlingMethod.Repeat
					x = x Mod width
					If x < 0 Then
						x += width
					End If
					y = y Mod height
					If y < 0 Then
						y += height
					End If
			End Select
			Return srcImage(y, x)
		End Function

		Public Enum EdgeHandlingMethod
			Clamp
			Mirror
			Fill
			Repeat
		End Enum
	End Module
End Namespace