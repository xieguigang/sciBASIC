Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Imaging

Namespace Gradiant3D

	Public Class BitmapCreator
		Private Width As Integer, Height As Integer

		Private Pixels As Byte()

		Private Stride As Integer


		Public Sub New(width__1 As Integer, height__2 As Integer)
			Width = width__1
			Height = height__2
			Pixels = New Byte(width__1 * height__2 * 4 - 1) {}
			Stride = width__1 * 4
		End Sub

		Public Sub GetPixel(x As Integer, y As Integer, ByRef red As Byte, ByRef green As Byte, ByRef blue As Byte, ByRef alpha As Byte)
			Dim index As Integer = y * Stride + x * 4
            blue = Pixels(index.MoveNext)
            green = Pixels(index.MoveNext)
            red = Pixels(index.MoveNext)
            alpha = Pixels(index)
        End Sub

        Public Function GetBlue(x As Integer, y As Integer) As Byte
            Return Pixels(y * Stride + x * 4)
        End Function

        Public Function GetGreen(x As Integer, y As Integer) As Byte
            Return Pixels(y * Stride + x * 4 + 1)
        End Function

        Public Function GetRed(x As Integer, y As Integer) As Byte
            Return Pixels(y * Stride + x * 4 + 2)
        End Function

        Public Function GetAlpha(x As Integer, y As Integer) As Byte
            Return Pixels(y * Stride + x * 4 + 3)
        End Function

        Public Sub SetPixel(x As Integer, y As Integer, red As Byte, green As Byte, blue As Byte, alpha As Byte)
            Dim index As Integer = y * Stride + x * 4
            Pixels(index.MoveNext) = blue
            Pixels(index.MoveNext) = green
            Pixels(index.MoveNext) = red
            Pixels(index.MoveNext) = alpha
        End Sub

        Public Sub SetBlue(x As Integer, y As Integer, blue As Byte)
            Pixels(y * Stride + x * 4) = blue
        End Sub

        Public Sub SetGreen(x As Integer, y As Integer, green As Byte)
            Pixels(y * Stride + x * 4 + 1) = green
        End Sub

        Public Sub SetRed(x As Integer, y As Integer, red As Byte)
            Pixels(y * Stride + x * 4 + 2) = red
        End Sub

        Public Sub SetAlpha(x As Integer, y As Integer, alpha As Byte)
            Pixels(y * Stride + x * 4 + 3) = alpha
        End Sub

        Public Sub SetColor(red As Byte, green As Byte, blue As Byte, alpha As Byte)
            Dim bytecount As Integer = Width * Height * 4
            Dim index As Integer = 0
            While index < bytecount
                Pixels(index.MoveNext) = blue
                Pixels(index.MoveNext) = green
                Pixels(index.MoveNext) = red
                Pixels(index.MoveNext) = alpha
            End While
		End Sub

		Public Sub SetColor(red As Byte, green As Byte, blue As Byte)
			SetColor(red, green, blue, 255)
		End Sub

		Public Function MakeBitmap(dpiX As Double, dpiY As Double) As WriteableBitmap
			Dim wbitmap As New WriteableBitmap(Width, Height, dpiX, dpiY, PixelFormats.Bgra32, Nothing)

			Dim rect As New Int32Rect(0, 0, Width, Height)
			wbitmap.WritePixels(rect, Pixels, Stride, 0)

			Return wbitmap
		End Function
	End Class
End Namespace
