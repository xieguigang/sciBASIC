#Region "Microsoft.VisualBasic::b0a5667959487ce72f00fb58702efb07, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\BitmapResizer.vb"

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

    '   Total Lines: 224
    '    Code Lines: 166 (74.11%)
    ' Comment Lines: 23 (10.27%)
    '    - Xml Docs: 65.22%
    ' 
    '   Blank Lines: 35 (15.62%)
    '     File Size: 9.97 KB


    '     Module BitmapResizer
    ' 
    '         Function: BilinearInterpolation, ByteArrayToByteMatrix, ByteMatrixToByteArray, CubicInterpolate, CubicInterpolation
    '                   GetPixelValue, ResizeImage
    '         Enum EdgeHandlingMethod
    ' 
    '             Clamp, Fill, Mirror, Repeat
    ' 
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Imaging.BitmapImage

    Public Module BitmapResizer

        Public Function ByteArrayToByteMatrix(byteArray As Byte(), width As Integer, height As Integer) As Byte(,)
            Dim matrix = New Byte(width - 1, height * 4 - 1) {} ' 4 channels (ARGB)

            For Y As Integer = 0 To height - 1
                For X As Integer = 0 To width - 1
                    Dim index = (Y * width + X) * 4 ' Calculate index in the byte array
                    matrix(X, Y * 4) = byteArray(index)     ' Alpha
                    matrix(X, Y * 4 + 1) = byteArray(index + 1) ' Red
                    matrix(X, Y * 4 + 2) = byteArray(index + 2) ' Green
                    matrix(X, Y * 4 + 3) = byteArray(index + 3) ' Blue
                Next
            Next

            Return matrix
        End Function

        Public Function ByteMatrixToByteArray(byteMatrix As Byte(,)) As Byte()
            Dim width = byteMatrix.GetLength(0)
            Dim height = byteMatrix.GetLength(1)
            Dim channels = 4 ' 假设每个像素有4个通道（例如 ARGB）
            Dim byteArray = New Byte(width * height * channels - 1) {}

            For Y As Integer = 0 To height - 1
                For X As Integer = 0 To width - 1
                    For c = 0 To channels - 1
                        Dim index = Y * width * channels + X * channels + c
                        byteArray(index) = byteMatrix(X, Y)
                    Next
                Next
            Next

            Return byteArray
        End Function

        Public Function ResizeImage(ByRef srcImage(,) As Color, srcWidth As Integer, srcHeight As Integer, dstWidth As Integer, dstHeight As Integer) As Color(,)
            Dim dstImage(dstHeight - 1, dstWidth - 1) As Color
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
                    Dim y1x1 = srcImage(y1, x1)
                    Dim y1x2 = srcImage(y1, x2)
                    Dim y2x1 = srcImage(y2, x1)
                    Dim y2x2 = srcImage(y2, x2)

                    Dim A As Integer = CInt(std.Truncate((1 - xWeight) * (1 - yWeight) * y1x1.A + xWeight * (1 - yWeight) * y1x2.A + (1 - xWeight) * yWeight * y2x1.A + xWeight * yWeight * y2x2.A))
                    Dim R As Integer = CInt(std.Truncate((1 - xWeight) * (1 - yWeight) * y1x1.R + xWeight * (1 - yWeight) * y1x2.R + (1 - xWeight) * yWeight * y2x1.R + xWeight * yWeight * y2x2.R))
                    Dim G As Integer = CInt(std.Truncate((1 - xWeight) * (1 - yWeight) * y1x1.G + xWeight * (1 - yWeight) * y1x2.G + (1 - xWeight) * yWeight * y2x1.G + xWeight * yWeight * y2x2.G))
                    Dim B As Integer = CInt(std.Truncate((1 - xWeight) * (1 - yWeight) * y1x1.B + xWeight * (1 - yWeight) * y1x2.B + (1 - xWeight) * yWeight * y2x1.B + xWeight * yWeight * y2x2.B))

                    dstImage(dstY, dstX) = Color.FromArgb(A, R, G, B)
                Next
            Next

            Return dstImage
        End Function

        ''' <summary>
        ''' Cubic interpolate
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="scale"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function CubicInterpolation(input As Byte(,), scale As Double) As Byte(,)
            Dim originalWidth = input.GetLength(0)
            Dim originalHeight = input.GetLength(1)
            Dim newWidth As Integer = originalWidth * scale
            Dim newHeight As Integer = originalHeight * scale

            Dim output = New Byte(newWidth - 1, newHeight - 1) {}

            For X As Integer = 0 To newWidth - 1
                For Y As Integer = 0 To newHeight - 1
                    Dim newX = X / scale
                    Dim newY = Y / scale

                    Dim x0 As Integer = newX
                    Dim y0 As Integer = newY
                    Dim x1 = x0 + 1
                    Dim y1 = y0 + 1

                    Dim dx = newX - x0
                    Dim dy = newY - y0

                    Dim values = New Byte(3) {}
                    For i = 0 To 1
                        For j = 0 To 1
                            If x1 < originalWidth AndAlso y1 < originalHeight Then
                                values(i * 2 + j) = input(x1 + i, y1 + j)
                            Else
                                values(i * 2 + j) = 0 ' or use edge pixel value
                            End If
                        Next
                    Next

                    Dim interpolationX0 = CubicInterpolate(values(0), values(1), values(2), values(3), dx)
                    Dim interpolationX1 = CubicInterpolate(values(2), values(3), values(4), values(5), dx)

                    output(X, Y) = CByte(CubicInterpolate(interpolationX0, interpolationX1, 0, 0, dy))
                Next
            Next

            Return output
        End Function

        Private Function CubicInterpolate(v0 As Double, v1 As Double, v2 As Double, v3 As Double, x As Double) As Double
            Dim a0, a1, a2, a3, x2 As Double

            x2 = x * x
            a0 = v3 - v2 - v0 + v1
            a1 = v0 - v1 - a0
            a2 = v2 - v0
            a3 = v1

            Return a0 * x * x2 + a1 * x2 + a2 * x + a3
        End Function

        ''' <summary>
        ''' Bilinear interpolation
        ''' </summary>
        ''' <param name="srcImage"></param>
        ''' <param name="srcWidth"></param>
        ''' <param name="srcHeight"></param>
        ''' <param name="dstWidth"></param>
        ''' <param name="dstHeight"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function BilinearInterpolation(ByRef srcImage(,) As Byte, srcWidth As Integer, srcHeight As Integer, dstWidth As Integer, dstHeight As Integer) As Byte(,)
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
                Next
            Next

            Return dstImage
        End Function

        Public Function GetPixelValue(ByRef srcImage(,) As Byte, x As Integer, y As Integer, width As Integer, height As Integer, method As EdgeHandlingMethod) As Byte
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
