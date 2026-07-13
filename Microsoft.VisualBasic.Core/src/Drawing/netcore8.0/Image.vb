#Region "Microsoft.VisualBasic::e67b5b91bf1c4d98ef22ae56528ecf8c, Microsoft.VisualBasic.Core\src\Drawing\netcore8.0\Image.vb"

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

    '   Total Lines: 327
    '    Code Lines: 165 (50.46%)
    ' Comment Lines: 100 (30.58%)
    '    - Xml Docs: 49.00%
    ' 
    '   Blank Lines: 62 (18.96%)
    '     File Size: 12.63 KB


    '     Class Image
    ' 
    '         Properties: Height, Width
    ' 
    '         Function: FromFile, FromStream
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    '     Interface IRasterMemory
    ' 
    '         Function: GetMemoryBuffer
    ' 
    '     Class Bitmap
    ' 
    '         Properties: MemoryBuffer, Size
    ' 
    '         Constructor: (+5 Overloads) Sub New
    ' 
    '         Function: Clone, ConvertToBitmapStream, GetMemoryBitmap, GetMemoryBuffer, GetPixel
    '                   LoadMemory, Resize, ToString
    ' 
    '         Sub: (+3 Overloads) Save, SetPixel
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
Imports Microsoft.VisualBasic.Imaging.BitmapImage.FileStream
Imports Microsoft.VisualBasic.Imaging.Driver
Imports MemoryBmp = Microsoft.VisualBasic.Imaging.BitmapImage.FileStream.Bitmap

Namespace Imaging

#If NET8_0_OR_GREATER Or NETSTANDARD2_0_OR_GREATER Then

    ''' <summary>
    ''' the abstract image data model, example as gdi+ raster image bitmap, svg image, pdf image, etc
    ''' </summary>
    ''' <remarks>
    ''' the image model implements the interface <see cref="IRasterMemory"/>
    ''' </remarks>
    Public MustInherit Class Image : Implements IDisposable, IRasterMemory

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
        Public MustOverride Function GetMemoryBitmap() As BitmapBuffer Implements IRasterMemory.GetMemoryBuffer

        ''' <summary>
        ''' Load bitmap image from file stream
        ''' </summary>
        ''' <param name="s">only works for bitmap image file stream</param>
        ''' <returns></returns>
        Public Shared Function FromStream(s As Stream) As Image
            If DriverLoad.CheckRasterImageLoader Then
                Return DriverLoad.LoadFromStream(s)
            Else
                Return New Bitmap(New BitmapReader(s).LoadMemory)
            End If
        End Function

        Public Shared Function FromFile(path As String) As Image
            Using s As Stream = path.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                Return FromStream(s)
            End Using
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

    Public Interface IRasterMemory
        Function GetMemoryBuffer() As BitmapBuffer
    End Interface

    ''' <summary>
    ''' the gdi+ raster image data in memory
    ''' </summary>
    Public Class Bitmap : Inherits Image
        Implements IRasterMemory

        Public Overrides ReadOnly Property Size As Size
            Get
                Return MemoryBuffer.Size
            End Get
        End Property

        Public ReadOnly Property MemoryBuffer As BitmapBuffer

        Sub New(file As String)
            Call Me.New(LoadMemory(file))
        End Sub

        Sub New(data As BitmapBuffer)
            MemoryBuffer = data
        End Sub

        ''' <summary>
        ''' make memory data copy
        ''' </summary>
        ''' <param name="copy"></param>
        Sub New(copy As Image)
            With copy.GetMemoryBitmap
                MemoryBuffer = New BitmapBuffer(.RawBuffer.ToArray, .Size, .GetPixelChannels)
            End With
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

        Public Overrides Function ToString() As String
            If MemoryBuffer Is Nothing Then
                Return "null;"
            Else
                Return MemoryBuffer.ToString
            End If
        End Function

        Private Shared Function LoadMemory(file As String) As BitmapBuffer
            Using s As Stream = file.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                Return New BitmapReader(s).LoadMemory
            End Using
        End Function

        Public Overloads Sub Save(filename As String, format As ImageFormats)
            Using s As Stream = filename.Open(FileMode.OpenOrCreate, doClear:=True)
                Call Save(s, format)
            End Using
        End Sub

        ''' <summary>
        ''' Save current bitmap object into a specific file
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="format"></param>
        Public Overrides Sub Save(s As Stream, format As ImageFormats)
            Dim pixelFormat As BitsPerPixelEnum = If(MemoryBuffer.GetPixelChannels = 3, BitsPerPixelEnum.RGB24, BitsPerPixelEnum.RGBA32)
            Dim writer As New MemoryBmp(Width, Height, MemoryBuffer.RawBuffer, pixelFormat)

            Call writer.Save(s, flipped:=True)
            Call s.Flush()
        End Sub

        Public Overloads Sub Save(filename As String)
            Using s As Stream = filename.Open(FileMode.OpenOrCreate, doClear:=True)
                Call Save(s, Nothing)
            End Using
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

        Public Overrides Function GetMemoryBitmap() As BitmapBuffer
            Return MemoryBuffer
        End Function

        Public Function GetMemoryBuffer() As BitmapBuffer Implements IRasterMemory.GetMemoryBuffer
            Return MemoryBuffer
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

    ''' <summary>
    ''' Specifies the file format of the image. Not inheritable.
    ''' </summary>
    Public NotInheritable Class ImageFormat

        Private Sub New(guid As Guid)
            _GUID = guid
        End Sub

        Private ReadOnly _GUID As Guid

        Public ReadOnly Property Guid As Guid
            Get
                Return _GUID
            End Get
        End Property

        ''' <summary>
        ''' Gets the bitmap (BMP) image format.
        ''' </summary>
        Public Shared ReadOnly Property Bmp As New ImageFormat(New Guid("b96b3cab-0728-11d3-9d7b-0000f81ef32e"))

        ''' <summary>
        ''' Gets the Graphics Interchange Format (GIF) image format.
        ''' </summary>
        Public Shared ReadOnly Property Gif As New ImageFormat(New Guid("b96b3cb0-0728-11d3-9d7b-0000f81ef32e"))

        ''' <summary>
        ''' Gets the Joint Photographic Experts Group (JPEG) image format.
        ''' </summary>
        Public Shared ReadOnly Property Jpeg As New ImageFormat(New Guid("b96b3cae-0728-11d3-9d7b-0000f81ef32e"))

        ''' <summary>
        ''' Gets the W3C Portable Network Graphics (PNG) image format.
        ''' </summary>
        Public Shared ReadOnly Property Png As New ImageFormat(New Guid("b96b3caf-0728-11d3-9d7b-0000f81ef32e"))

        ''' <summary>
        ''' Gets the Tagged Image File Format (TIFF) image format.
        ''' </summary>
        Public Shared ReadOnly Property Tiff As New ImageFormat(New Guid("b96b3cb1-0728-11d3-9d7b-0000f81ef32e"))

        ''' <summary>
        ''' Gets the Windows metafile (WMF) image format.
        ''' </summary>
        Public Shared ReadOnly Property Wmf As New ImageFormat(New Guid("b96b3cad-0728-11d3-9d7b-0000f81ef32e"))

        ''' <summary>
        ''' Gets the enhanced metafile (EMF) image format.
        ''' </summary>
        Public Shared ReadOnly Property Emf As New ImageFormat(New Guid("b96b3cac-0728-11d3-9d7b-0000f81ef32e"))

        ''' <summary>
        ''' Gets the Exchangeable Image File (Exif) format.
        ''' </summary>
        Public Shared ReadOnly Property Exif As New ImageFormat(New Guid("b96b3cb2-0728-11d3-9d7b-0000f81ef32e"))

        ''' <summary>
        ''' Gets the Windows icon image format.
        ''' </summary>
        Public Shared ReadOnly Property Icon As New ImageFormat(New Guid("b96b3cb5-0728-11d3-9d7b-0000f81ef32e"))

        ''' <summary>
        ''' Gets the memory bitmap image format.
        ''' </summary>
        Public Shared ReadOnly Property MemoryBmp As New ImageFormat(New Guid("b96b3caa-0728-11d3-9d7b-0000f81ef32e"))

        Public Overrides Function ToString() As String
            Return _GUID.ToString
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If TypeOf obj Is ImageFormat Then
                Return _GUID.Equals(DirectCast(obj, ImageFormat).Guid)
            End If
            Return False
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return _GUID.GetHashCode
        End Function
    End Class

    ''' <summary>
    ''' Defines an array of colors that make up a color palette.
    ''' The colors are 32-bit ARGB colors. Not inheritable.
    ''' </summary>
    Public NotInheritable Class ColorPalette

        ''' <summary>
        ''' Gets an array of Color structures.
        ''' </summary>
        Public Property Entries As Color()

        ''' <summary>
        ''' Gets a value that specifies how to interpret the color information in the array of colors.
        ''' </summary>
        Public Property Flags As Integer

        Sub New()
        End Sub

        Sub New(entries As Color(), Optional flags As Integer = 0)
            _Entries = entries
            _Flags = flags
        End Sub
    End Class

    ''' <summary>
    ''' Defines a graphic metafile. A metafile contains records that describe a sequence 
    ''' of graphics operations that can be recorded and played back.
    ''' </summary>
    Public Class Metafile : Inherits Image

        Public Overrides ReadOnly Property Size As Size

        ''' <summary>
        ''' Gets the width, in pixels, of this Metafile.
        ''' </summary>
        Public ReadOnly Property HMetafile As IntPtr

        ''' <summary>
        ''' Gets the MetafileHeader associated with this Metafile.
        ''' </summary>
        Public ReadOnly Property MetafileHeader As MetafileHeaderData

        Sub New(filename As String)
            _Size = New Size(0, 0)
        End Sub

        Sub New(stream As Stream)
            _Size = New Size(0, 0)
        End Sub

        Sub New(hdc As IntPtr, frameRect As RectangleF, Optional frameUnit As MetafileFrameUnit = MetafileFrameUnit.GdiCompatible, Optional type As MetafileType = MetafileType.Memory)
            _Size = New Size(CInt(frameRect.Width), CInt(frameRect.Height))
        End Sub

        Sub New(hdc As IntPtr, frameRect As Rectangle, Optional frameUnit As MetafileFrameUnit = MetafileFrameUnit.GdiCompatible, Optional type As MetafileType = MetafileType.Memory)
            _Size = New Size(frameRect.Width, frameRect.Height)
        End Sub

        Sub New(stream As Stream, hdc As IntPtr, Optional frameRect As RectangleF = Nothing, Optional frameUnit As MetafileFrameUnit = MetafileFrameUnit.GdiCompatible, Optional type As MetafileType = MetafileType.Memory)
            _Size = New Size(CInt(frameRect.Width), CInt(frameRect.Height))
        End Sub

        Public Overrides Sub Save(s As Stream, format As ImageFormats)
        End Sub

        Protected Friend Overrides Function ConvertToBitmapStream() As MemoryStream
            Return New MemoryStream
        End Function

        Public Overrides Function GetMemoryBitmap() As BitmapBuffer
            Return Nothing
        End Function
    End Class

    ''' <summary>
    ''' Specifies the layout of the metafile format.
    ''' </summary>
    Public Enum MetafileType
        ''' <summary>
        ''' Specifies a metafile type that is not valid.
        ''' </summary>
        Invalid
        ''' <summary>
        ''' Specifies a WMF metafile.
        ''' </summary>
        Wmf
        ''' <summary>
        ''' Specifies a WMF Placeable metafile.
        ''' </summary>
        WmfPlaceable
        ''' <summary>
        ''' Specifies an EMF metafile.
        ''' </summary>
        Emf
        ''' <summary>
        ''' Specifies an EMF+ metafile.
        ''' </summary>
        EmfPlusOnly
        ''' <summary>
        ''' Specifies an EMF+ Dual metafile.
        ''' </summary>
        EmfPlusDual
        ''' <summary>
        ''' Specifies a memory metafile.
        ''' </summary>
        Memory
    End Enum

    ''' <summary>
    ''' Specifies a unit of measurement for the rectangle used to size and position a metafile.
    ''' </summary>
    Public Enum MetafileFrameUnit
        ''' <summary>
        ''' Specifies a pixel as the unit of measure.
        ''' </summary>
        Pixel
        ''' <summary>
        ''' Specifies a printer's point as the unit of measure.
        ''' </summary>
        Point
        ''' <summary>
        ''' Specifies an inch as the unit of measure.
        ''' </summary>
        Inch
        ''' <summary>
        ''' Specifies 1/300 of an inch as the unit of measure.
        ''' </summary>
        Document
        ''' <summary>
        ''' Specifies a millimeter as the unit of measure.
        ''' </summary>
        Millimeter
        ''' <summary>
        ''' Specifies .01 of a millimeter as the unit of measure. Supported by GDI.
        ''' </summary>
        GdiCompatible
    End Enum

    ''' <summary>
    ''' Contains attributes of an associated Metafile. Not inheritable.
    ''' </summary>
    Public Class MetafileHeaderData

        ''' <summary>
        ''' Gets the type of the associated Metafile.
        ''' </summary>
        Public ReadOnly Property Type As MetafileType

        ''' <summary>
        ''' Gets the size, in bytes, of the associated Metafile object.
        ''' </summary>
        Public ReadOnly Property MetafileSize As Integer

        ''' <summary>
        ''' Gets the version number of the metafile.
        ''' </summary>
        Public ReadOnly Property Version As Integer

        ''' <summary>
        ''' Gets the horizontal DPI of the metafile.
        ''' </summary>
        Public ReadOnly Property DpiX As Single

        ''' <summary>
        ''' Gets the vertical DPI of the metafile.
        ''' </summary>
        Public ReadOnly Property DpiY As Single

        ''' <summary>
        ''' Gets the bounding rectangle of the metafile.
        ''' </summary>
        Public ReadOnly Property Bounds As Rectangle

        ''' <summary>
        ''' Gets a value indicating whether the metafile is an EMF or EMF+.
        ''' </summary>
        Public ReadOnly Property IsEmfOrEmfPlus As Boolean

        ''' <summary>
        ''' Gets a value indicating whether the metafile is a WMF.
        ''' </summary>
        Public ReadOnly Property IsWmf As Boolean
    End Class
#End If
End Namespace
