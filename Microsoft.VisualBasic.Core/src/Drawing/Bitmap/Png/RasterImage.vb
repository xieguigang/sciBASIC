#Region "Microsoft.VisualBasic::3ac4b5c6d7e8f90a1b2c3d4e5f60718293, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Png\RasterImage.vb"

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

    '     Class RasterImage
    ' 
    '         Properties: Channels, Height, IsBlank, Pixels, Width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Imaging.BitmapImage.Png

    ''' <summary>
    ''' A raster image that is decoded from the png file, the pixel values are 
    ''' normalized into the range [0,1], so that the image could be feed into 
    ''' the numeric model directly.
    ''' </summary>
    Public Class RasterImage

        ''' <summary>
        ''' the image width, i.e. the number of the pixels of a scan line.
        ''' </summary>
        ''' <returns></returns>
        Public Property Width As Integer
        ''' <summary>
        ''' the image height, i.e. the number of the scan lines.
        ''' </summary>
        ''' <returns></returns>
        Public Property Height As Integer
        ''' <summary>
        ''' the number of the color channels: 1 = the gray scale, 3 = the rgb color.
        ''' </summary>
        ''' <returns></returns>
        Public Property Channels As Integer

        ''' <summary>
        ''' the pixel data, the value of a pixel is laid out as 
        ''' <c>(y * Width + x) * Channels + c</c>, and every value is 
        ''' normalized into the range [0,1].
        ''' </summary>
        ''' <returns></returns>
        Public Property Pixels As Double()

        ''' <summary>
        ''' true when all of the pixel values are zero, this is the typical 
        ''' symptom of a failed image decoding.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsBlank As Boolean
            Get
                If Pixels Is Nothing Then
                    Return True
                End If

                For i As Integer = 0 To Pixels.Length - 1
                    If Pixels(i) > 0.0 Then
                        Return False
                    End If
                Next

                Return True
            End Get
        End Property

        ''' <summary>
        ''' the single line description of current raster image object
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"{Width}x{Height}x{Channels}"
        End Function

    End Class

End Namespace
