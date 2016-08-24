#Region "Microsoft.VisualBasic::7af2ec82340ec24a8c2d0e7060dd6767, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\HeatMap3D\BitmapCreator.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

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
