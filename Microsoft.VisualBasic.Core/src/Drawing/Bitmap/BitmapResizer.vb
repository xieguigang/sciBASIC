#Region "Microsoft.VisualBasic::1cc4fe0615e2e44df8ead3ef7cc00762, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\BitmapResizer.vb"

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

    '   Total Lines: 116
    '    Code Lines: 93 (80.17%)
    ' Comment Lines: 6 (5.17%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (14.66%)
    '     File Size: 5.67 KB


    '     Module BitmapResizer
    ' 
    '         Function: GetPixelValue, (+2 Overloads) ResizeImage
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
Imports std = System.Math

Namespace Imaging.BitmapImage

    Public Module BitmapResizer

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

        Public Function ResizeImage(ByRef srcImage(,) As Byte, srcWidth As Integer, srcHeight As Integer, dstWidth As Integer, dstHeight As Integer) As Byte(,)
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
