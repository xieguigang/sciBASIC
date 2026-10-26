#Region "Microsoft.VisualBasic::5b6c7d8e9f0a1b2c3d4e5f60718293a4, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Png\PngImage.vb"

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

    '     Class PngImage
    ' 
    '         Properties: BitDepth, Channels, ColorType, GrayInAlpha, Height,
    '                     IdatBytes, Interlace, IsBlank, IsEmpty, Pixels, RawBytes,
    '                     Rgb, Width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Gray, ToRaster, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Imaging.BitmapImage.Png

    ''' <summary>
    ''' The raw raster image data that is decoded from a png image file, the 
    ''' pixel data is always expanded into the 8 bits per channel row major 
    ''' buffer, the row padding and the scan line filter are already removed.
    ''' </summary>
    Public Class PngImage

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
        ''' the number of the color channels of the <see cref="Pixels"/> buffer:
        ''' 1 = gray, 2 = gray + alpha, 3 = rgb, 4 = rgba.
        ''' </summary>
        ''' <returns></returns>
        Public Property Channels As Integer

        ''' <summary>
        ''' the row major raw pixel data, the row padding is already removed
        ''' </summary>
        ''' <returns></returns>
        Public Property Pixels As Byte()

        ''' <summary>
        ''' the bit depth and the color type of the source png file
        ''' </summary>
        ''' <returns></returns>
        Public Property BitDepth As Integer
        ''' <summary>
        ''' 0 = gray, 2 = rgb, 3 = the palette index, 4 = gray + alpha, 6 = rgba
        ''' </summary>
        ''' <returns></returns>
        Public Property ColorType As Integer

        ''' <summary>
        ''' the interlace method of the source png file: 0 = none, 1 = Adam7.
        ''' </summary>
        ''' <returns></returns>
        Public Property Interlace As Integer

        ''' <summary>
        ''' the byte length of the compressed IDAT section and the inflated raw 
        ''' pixel section
        ''' </summary>
        ''' <returns></returns>
        Public Property IdatBytes As Integer
        Public Property RawBytes As Integer

        ''' <summary>
        ''' true when the raw pixel section is fully transparent black
        ''' </summary>
        ''' <returns></returns>
        Public Property IsEmpty As Boolean
            Get
                If Not emptyKnown Then
                    isEmptyBuffer = IsZeroBuffer(Pixels)
                    emptyKnown = True
                End If

                Return isEmptyBuffer
            End Get
            Set(value As Boolean)
                isEmptyBuffer = value
                emptyKnown = True
            End Set
        End Property

        ''' <summary>
        ''' the gray scale intensity of some reference image is stored on the 
        ''' alpha channel with the rgb color all filled with zero (this is the 
        ''' output of the ``Color.FromArgb(alpha, 0, 0, 0)`` based gray scale 
        ''' renderer), in this case the intensity value is evaluated from the 
        ''' alpha channel just like the image is composited on a white canvas.
        ''' </summary>
        ''' <remarks>
        ''' this flag is detected from the decoded pixel buffer automatically 
        ''' when it was not set by the caller.
        ''' </remarks>
        ''' <returns></returns>
        Public Property GrayInAlpha As Boolean
            Get
                If Not grayInAlphaKnown Then
                    grayInAlphaBuffer = IsBlackRgb(Pixels, Channels)
                    grayInAlphaKnown = True
                End If

                Return grayInAlphaBuffer
            End Get
            Set(value As Boolean)
                grayInAlphaBuffer = value
                grayInAlphaKnown = True
            End Set
        End Property

        Private isEmptyBuffer As Boolean
        Private emptyKnown As Boolean
        Private grayInAlphaBuffer As Boolean
        Private grayInAlphaKnown As Boolean
        Private rgbBuffer As Byte()

        ''' <summary>
        ''' true when all of the decoded pixel values are zero
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsBlank As Boolean
            Get
                Return IsEmpty
            End Get
        End Property

        ''' <summary>
        ''' get the 3 channels rgb color buffer of current image, the gray 
        ''' scale image is expanded into the rgb color image and the alpha 
        ''' channel is dropped.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Rgb As Byte()
            Get
                If rgbBuffer Is Nothing Then
                    rgbBuffer = ExpandRgb()
                End If

                Return rgbBuffer
            End Get
        End Property

        ''' <summary>
        ''' the gray scale intensity matrix of current image, the values are 
        ''' scaled into the range [0,1]
        ''' </summary>
        ''' <returns></returns>
        Public Function Gray() As Double()()
            If Pixels Is Nothing OrElse Width <= 0 OrElse Height <= 0 Then
                Return New Double()() {}
            End If

            Dim matrix As Double()() = New Double(Height - 1)() {}
            Dim useAlpha As Boolean = GrayInAlpha

            For y As Integer = 0 To Height - 1
                Dim row As Double() = New Double(Width - 1) {}
                Dim p As Long = CLng(y) * Width * Channels

                For x As Integer = 0 To Width - 1
                    Dim offset As Integer = CInt(p)

                    Select Case Channels
                        Case 1
                            row(x) = Pixels(offset) / 255.0
                        Case 2
                            row(x) = If(useAlpha, 1.0 - Pixels(offset + 1) / 255.0, Pixels(offset) / 255.0)
                        Case Else
                            If useAlpha Then
                                row(x) = 1.0 - Pixels(offset + 3) / 255.0
                            Else
                                row(x) = (0.299 * Pixels(offset) +
                                          0.587 * Pixels(offset + 1) +
                                          0.114 * Pixels(offset + 2)) / 255.0
                            End If
                    End Select

                    p += Channels
                Next

                matrix(y) = row
            Next

            Return matrix
        End Function

        ''' <summary>
        ''' convert current raw raster image into the normalized pixel buffer
        ''' </summary>
        ''' <param name="toGray">convert the rgb color into the gray scale intensity or not.</param>
        ''' <returns>
        ''' a <see cref="RasterImage"/> object, the pixel values are normalized 
        ''' into the range [0,1].
        ''' </returns>
        Public Function ToRaster(Optional toGray As Boolean = True) As RasterImage
            Dim outChannels As Integer = If(toGray, 1, 3)
            Dim n As Long = CLng(Width) * CLng(Height)

            If Pixels Is Nothing OrElse n <= 0 Then
                Return New RasterImage With {
                    .Width = Width,
                    .Height = Height,
                    .Channels = outChannels,
                    .Pixels = New Double() {}
                }
            End If

            Dim buffer As Double() = New Double(CInt(n * outChannels) - 1) {}

            For p As Integer = 0 To CInt(n) - 1
                Dim source As Integer = CInt(CLng(p) * Channels)
                Dim r As Double, g As Double, b As Double

                If Channels <= 2 Then
                    r = Pixels(source) / 255.0
                    g = r
                    b = r
                Else
                    r = Pixels(source) / 255.0
                    g = Pixels(source + 1) / 255.0
                    b = Pixels(source + 2) / 255.0
                End If

                If toGray Then
                    buffer(p) = 0.299 * r + 0.587 * g + 0.114 * b
                Else
                    buffer(p * 3) = r
                    buffer(p * 3 + 1) = g
                    buffer(p * 3 + 2) = b
                End If
            Next

            Return New RasterImage With {
                .Width = Width,
                .Height = Height,
                .Channels = outChannels,
                .Pixels = buffer
            }
        End Function

        ''' <summary>
        ''' the single line description of current png image object
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"{Width}x{Height}x{Channels} (bit depth {BitDepth}, color type {ColorType})"
        End Function

        ''' <summary>
        ''' checks that the rgb color of almost all of the pixels are black
        ''' </summary>
        Private Function IsBlackRgb(pixels As Byte(), channels As Integer) As Boolean
            If pixels Is Nothing OrElse channels < 4 Then
                Return False
            End If

            Dim black As Integer = 0
            Dim total As Integer = 0

            For p As Integer = 0 To pixels.Length - channels Step channels
                If pixels(p) = 0 AndAlso pixels(p + 1) = 0 AndAlso pixels(p + 2) = 0 Then
                    black += 1
                End If

                total += 1
            Next

            Return total > 0 AndAlso black / total > 0.9
        End Function

        Private Function IsZeroBuffer(pixels As Byte()) As Boolean
            If pixels Is Nothing Then
                Return True
            End If

            For Each b As Byte In pixels
                If b <> 0 Then
                    Return False
                End If
            Next

            Return True
        End Function

        Private Function ExpandRgb() As Byte()
            If Pixels Is Nothing OrElse Width <= 0 OrElse Height <= 0 Then
                Return New Byte() {}
            End If

            Dim n As Integer = CInt(CLng(Width) * CLng(Height))

            If Channels = 3 Then
                Return DirectCast(Pixels.Clone(), Byte())
            End If

            Dim buffer As Byte() = New Byte(CInt(CLng(n) * 3) - 1) {}

            For p As Integer = 0 To n - 1
                Dim source As Integer = CInt(CLng(p) * Channels)
                Dim target As Integer = p * 3

                If Channels = 1 OrElse Channels = 2 Then
                    Dim g As Byte = Pixels(source)

                    buffer(target) = g
                    buffer(target + 1) = g
                    buffer(target + 2) = g
                Else
                    buffer(target) = Pixels(source)
                    buffer(target + 1) = Pixels(source + 1)
                    buffer(target + 2) = Pixels(source + 2)
                End If
            Next

            Return buffer
        End Function

    End Class

End Namespace
