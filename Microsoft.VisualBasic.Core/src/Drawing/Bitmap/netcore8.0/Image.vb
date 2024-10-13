#Region "Microsoft.VisualBasic::be3082c5af594011cb1f1eb6ac3b785e, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\netcore8.0\Image.vb"

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

    '   Total Lines: 267
    '    Code Lines: 124 (46.44%)
    ' Comment Lines: 88 (32.96%)
    '    - Xml Docs: 42.05%
    ' 
    '   Blank Lines: 55 (20.60%)
    '     File Size: 10.14 KB


    '     Class Image
    ' 
    '         Properties: Height, Width
    ' 
    '         Function: FromStream
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    '     Class Bitmap
    ' 
    '         Properties: MemoryBuffer, Size
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: Clone, ConvertToBitmapStream, GetPixel, Resize
    ' 
    '         Sub: Save, SetPixel
    ' 
    '     Enum PixelFormat
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
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.BitmapImage.StreamWriter

Namespace Imaging

#If NET8_0_OR_GREATER Then

    ''' <summary>
    ''' the abstract image data model, example as gdi+ raster image bitmap, svg image, pdf image, etc
    ''' </summary>
    Public MustInherit Class Image : Implements IDisposable

        Private disposedValue As Boolean

        ''' <summary>
        ''' the size of the image
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property Size As Size

        ''' <summary>
        ''' the width of the image <see cref="Size"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Width As Integer
            Get
                Return Size.Width
            End Get
        End Property

        ''' <summary>
        ''' the height of the image <see cref="Size"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Height As Integer
            Get
                Return Size.Height
            End Get
        End Property

        Public MustOverride Sub Save(s As Stream, format As ImageFormats)

        ''' <summary>
        ''' Convert current image object as bitmap file data
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' function for make bitmap object constructor
        ''' </remarks>
        Protected Friend MustOverride Function ConvertToBitmapStream() As MemoryStream

        Public Shared Function FromStream(s As Stream) As Bitmap
            Return New Bitmap(New BitmapReader(s).LoadMemory)
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class

    ''' <summary>
    ''' the gdi+ raster image data in memory
    ''' </summary>
    Public Class Bitmap : Inherits Image

        Public Overrides ReadOnly Property Size As Size
            Get
                Return MemoryBuffer.Size
            End Get
        End Property

        Public ReadOnly Property MemoryBuffer As BitmapBuffer

        Sub New(data As BitmapBuffer)
            MemoryBuffer = data
        End Sub

        Sub New(copy As Image)
            Dim buf As MemoryStream = copy.ConvertToBitmapStream()
            Dim reader As New BitmapReader(buf)

            MemoryBuffer = reader.LoadMemory
        End Sub

        Sub New(size As Size)
            Call Me.New(size.Width, size.Height)
        End Sub

        Sub New(width As Integer, height As Integer, Optional format As PixelFormat = PixelFormat.Format32bppArgb)
            Dim channels As Integer = If(format = PixelFormat.Format32bppArgb, 4, 3)
            Dim buffer As Byte() = New Byte(width * height * channels - 1) {}

            ' create empty bitmap data
            MemoryBuffer = New BitmapBuffer(buffer, New Size(width, height), channels)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetPixel(x As Integer, y As Integer, pixel As Color)
            Call MemoryBuffer.SetPixel(x, y, pixel)
        End Sub

        Public Function Resize(newWidth As Integer, newHeight As Integer) As Bitmap
            Dim pixels = MemoryBuffer.GetARGB
            pixels = BitmapResizer.ResizeImage(pixels, MemoryBuffer.Width, MemoryBuffer.Height, newWidth, newHeight)
            ' construct bitmap data based on pixels matrix
            Dim sizedBitmap As New BitmapBuffer(pixels, New Size(newWidth, newHeight))
            Dim bitmap As New Bitmap(sizedBitmap)
            Return bitmap
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPixel(X As Integer, Y As Integer) As Color
            Return MemoryBuffer.GetPixel(X, Y)
        End Function

        ''' <summary>
        ''' Save current bitmap object into a specific file
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="format"></param>
        Public Overrides Sub Save(s As Stream, format As ImageFormats)
            Dim writer As New BitmapWriter(Width, Height)
            Dim pixel As Color

            For x As Integer = 0 To Width - 1
                For y As Integer = 0 To Height - 1
                    pixel = MemoryBuffer.GetPixel(x, y)
                    writer.SetPixel(x, y, pixel.R, pixel.G, pixel.B)
                Next
            Next

            Call writer.SaveColorImage(s)
            Call s.Flush()
        End Sub

        Public Function Clone() As Object
            Return New Bitmap(MemoryBuffer)
        End Function

        Protected Friend Overrides Function ConvertToBitmapStream() As MemoryStream
            Dim ms As New MemoryStream
            Call Save(ms, ImageFormats.Bmp)
            Call ms.Seek(Scan0, SeekOrigin.Begin)
            Return ms
        End Function
    End Class

    ''' <summary>
    ''' Specifies the format of the color data for each pixel in the image.
    ''' </summary>
    Public Enum PixelFormat

        '     The pixel data contains color-indexed values, which means the values are an index
        '     to colors in the system color table, as opposed to individual color values.
        Indexed = 65536

        '     The pixel data contains GDI colors.
        Gdi = 131072

        '     The pixel data contains alpha values that are Not premultiplied.
        Alpha = 262144

        '     The pixel format contains premultiplied alpha values.
        PAlpha = 524288

        '     Reserved.
        Extended = 1048576

        '     The default pixel format of 32 bits per pixel. The format specifies 24-bit color
        '     depth And an 8-bit alpha channel.
        Canonical = 2097152

        '     The pixel format Is undefined.
        Undefined = 0

        '     No pixel format Is specified.
        DontCare = 0

        '     Specifies that the pixel format Is 1 bit per pixel And that it uses indexed color.
        '     The color table therefore has two colors in it.
        Format1bppIndexed = 196865

        '     Specifies that the format Is 4 bits per pixel, indexed.
        Format4bppIndexed = 197634

        '     Specifies that the format Is 8 bits per pixel, indexed. The color table therefore
        '     has 256 colors in it.
        Format8bppIndexed = 198659

        '     The pixel format Is 16 bits per pixel. The color information specifies 65536
        '     shades of gray.
        Format16bppGrayScale = 1052676

        '     Specifies that the format Is 16 bits per pixel; 5 bits each are used for the
        '     red, green, And blue components. The remaining bit Is Not used.
        Format16bppRgb555 = 135173

        '     Specifies that the format Is 16 bits per pixel; 5 bits are used for the red component,
        '     6 bits are used for the green component, And 5 bits are used for the blue component.
        Format16bppRgb565 = 135174

        '     The pixel format Is 16 bits per pixel. The color information specifies 32,768
        '     shades of color, of which 5 bits are red, 5 bits are green, 5 bits are blue,
        '     And 1 bit Is alpha.
        Format16bppArgb1555 = 397319

        '     Specifies that the format Is 24 bits per pixel; 8 bits each are used for the
        '     red, green, And blue components.
        Format24bppRgb = 137224

        '     Specifies that the format Is 32 bits per pixel; 8 bits each are used for the
        '     red, green, And blue components. The remaining 8 bits are Not used.
        Format32bppRgb = 139273

        ''' <summary>
        ''' Specifies that the format Is 32 bits per pixel; 8 bits each are used for the
        ''' alpha, red, green, And blue components.
        ''' </summary>
        Format32bppArgb = 2498570

        '     Specifies that the format Is 32 bits per pixel; 8 bits each are used for the
        '     alpha, red, green, And blue components. The red, green, And blue components are
        '     premultiplied, according to the alpha component.
        Format32bppPArgb = 925707

        '     Specifies that the format Is 48 bits per pixel; 16 bits each are used for the
        '     red, green, And blue components.
        Format48bppRgb = 1060876

        '     Specifies that the format Is 64 bits per pixel; 16 bits each are used for the
        '     alpha, red, green, And blue components.
        Format64bppArgb = 3424269

        '     Specifies that the format Is 64 bits per pixel; 16 bits each are used for the
        '     alpha, red, green, And blue components. The red, green, And blue components are
        '     premultiplied according to the alpha component.
        Format64bppPArgb = 1851406

        '     The maximum value for this enumeration.
        Max = 15
    End Enum
#End If
End Namespace
