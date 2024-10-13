#Region "Microsoft.VisualBasic::febcb740e8ce1219e32e43dc28055fc6, mime\application%pdf\PdfFileWriter\PDF\PdfImage.vb"

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

    '   Total Lines: 1164
    '    Code Lines: 574 (49.31%)
    ' Comment Lines: 334 (28.69%)
    '    - Xml Docs: 38.02%
    ' 
    '   Blank Lines: 256 (21.99%)
    '     File Size: 40.29 KB


    ' Enum SaveImageAs
    ' 
    '     BWImage, GrayImage, IndexedImage, Jpeg
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class PdfImage
    ' 
    '     Properties: GrayToBWCutoff, HeightPix, ImageQuality, Resolution, WidthPix
    ' 
    '     Constructor: (+6 Overloads) Sub New
    ' 
    '     Function: GetEncoderInfo, (+2 Overloads) ImageSize, (+2 Overloads) ImageSizePosition, LoadImageFromFile, PictureToBWImage
    '               PictureToGrayImage, PictureToIndexedImage
    ' 
    '     Sub: BooleanToBWImage, ConstructorHelper, ConvertImageToBitmap, Dispose, ImageRectangle
    '          ImageSizeInPixels, (+5 Overloads) LoadImage, PictureToJpeg, WriteObjectToPdfFile
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfImage
'	PDF Image resource.
'
'	Uzi Granot
'	Version: 1.0
'	Date: April 1, 2013
'	Copyright (C) 2013-2019 Uzi Granot. All Rights Reserved
'
'	PdfFileWriter C# class library and TestPdfFileWriter test/demo
'  application are free software.
'	They is distributed under the Code Project Open License (CPOL).
'	The document PdfFileWriterReadmeAndLicense.pdf contained within
'	the distribution specify the license agreement and other
'	conditions and notes. You must read this document and agree
'	with the conditions specified in order to use this software.
'
'	For version history please refer to PdfDocument.cs
'
'

Imports System.IO
Imports System.Runtime.InteropServices
Imports i32 = Microsoft.VisualBasic.Language.i32
Imports std = System.Math
Imports System.Drawing


#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

''' <summary>
''' Save image as enumeration
''' </summary>
Public Enum SaveImageAs
    ''' <summary>
    ''' Jpeg format (default)
    ''' </summary>
    Jpeg

    ''' <summary>
    ''' PDF indexed bitmap format
    ''' </summary>
    IndexedImage

    ''' <summary>
    ''' convert to gray image
    ''' </summary>
    GrayImage

    ''' <summary>
    ''' Black and white format from bool array
    ''' </summary>
    BWImage
End Enum

''' <summary>
''' PDF Image class
''' </summary>
''' <remarks>
''' <para>
''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#ImageSupport">2.4 Image Support</a>
''' </para>
''' <para>
''' <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#DrawImage">For example of drawing image see 3.9. Draw Image and Clip it</a>
''' </para>
''' </remarks>
Public Class PdfImage
    Inherits PdfObject
    Implements IDisposable

    ''' <summary>
    ''' Gets image height in pixels
    ''' </summary>
    Private _WidthPix As Integer, _HeightPix As Integer
    ''' <summary>
    ''' Save image as
    ''' </summary>
    Public SaveAs As SaveImageAs = SaveImageAs.Jpeg

    ''' <summary>
    ''' Crop image rectangle (image pixels)
    ''' </summary>
    Public CropRect As Rectangle = System.Drawing.Rectangle.Empty

    ''' <summary>
    ''' Crop image rectangle (percent of image size)
    ''' </summary>
    Public CropPercent As RectangleF = RectangleF.Empty

    ''' <summary>
    ''' Reverse black and white (SaveImageAs.BWImage)
    ''' </summary>
    Public ReverseBW As Boolean = False

    ''' <summary>
    ''' Layer control
    ''' </summary>
    Public LayerControl As PdfLayer = Nothing

    ''' <summary>
    ''' Set output resolution 
    ''' </summary>
    Public Overridable Property Resolution As Double
        Get
            Return _Resolution
        End Get
        Set(value As Double)
            If value < 0 Then Throw New ApplicationException("Resolution must be greater than zero, or zero for default")

            ' save resolution
            _Resolution = value
        End Set
    End Property
    ''' <summary>
    ''' Image resolution
    ''' </summary>
    Protected _Resolution As Double = 0

    ''' <summary>
    ''' Default Jpeg image quality
    ''' </summary>
    Public Const DefaultQuality As Integer = -1

    ''' <summary>
    ''' Gets or sets Jpeg image quality
    ''' </summary>
    Public Property ImageQuality As Integer
        Get
            Return _ImageQuality
        End Get
        Set(value As Integer)
            ' set image quality
            If value <> DefaultQuality AndAlso (value < 0 OrElse value > 100) Then Throw New ApplicationException("PdfImageControl.ImageQuality must be DefaultQuality or 0 to 100")
            _ImageQuality = value
            Return
        End Set
    End Property

    Friend _ImageQuality As Integer = DefaultQuality

    ''' <summary>
    ''' Gray to BW cutoff level
    ''' </summary>
    Public Property GrayToBWCutoff As Integer
        Get
            Return _GrayToBWCutoff
        End Get
        Set(value As Integer)
            If value < 1 OrElse value > 99 Then Throw New ApplicationException("PdfImageControl.GrayToBWCutoff must be 1 to 99 (default is 50)")
            _GrayToBWCutoff = value
        End Set
    End Property

    Friend _GrayToBWCutoff As Integer = 50

    Public Property WidthPix As Integer
        Get
            Return _WidthPix
        End Get
        Friend Set(value As Integer)
            _WidthPix = value
        End Set
    End Property    ' in pixels

    Public Property HeightPix As Integer
        Get
            Return _HeightPix
        End Get
        Friend Set(value As Integer)
            _HeightPix = value
        End Set
    End Property    ' in pixels

    Friend ImageRect As Rectangle

    ''' <summary>
    ''' Gets image width in pixels
    ''' </summary>
    Friend Picture As Bitmap
    Friend DisposePicture As Boolean
    Friend DisposeImage As Boolean
    Friend BWImage As Boolean(,)
    Friend OneBitMask As Byte() = {&H80, &H40, &H20, &H10, 8, 4, 2, 1}

    ''' <summary>
    ''' PdfImage constructor
    ''' </summary>
    ''' <param name="Document">PdfDocument</param>
    Public Sub New(Document As PdfDocument)
        MyBase.New(Document, ObjectType.Stream, "/XObject")
        ' set subtype to /Image
        Dictionary.Add("/Subtype", "/Image")

        ' create resource code
        ResourceCode = Document.GenerateResourceNumber("X"c)
        Return
    End Sub

    ''' <summary>
    ''' Load image from file
    ''' </summary>
    ''' <param name="ImageFileName">Image file name</param>
    Public Sub LoadImage(ImageFileName As String)
        LoadImage(LoadImageFromFile(ImageFileName))
        Return
    End Sub

    ''' <summary>
    ''' Load image from Image derived class (Bitmap)
    ''' </summary>
    ''' <param name="Image">Image derived class</param>
    Public Sub LoadImage(Image As Image)
        ' image rectangle
        ImageRectangle(Image)

        ' image size in pixels
        ImageSizeInPixels(Image)

        ' convert the image to bitmap
        ConvertImageToBitmap(Image)

        ' write to output file
        WriteObjectToPdfFile()

        ' exit
        Return
    End Sub

    ''' <summary>
    ''' Load image from black and white bool matrix
    ''' </summary>
    ''' <param name="BWImage">BW bool matrix</param>
    Public Sub LoadImage(BWImage As Boolean(,))
        ' image dimensions
        WidthPix = BWImage.GetUpperBound(0) + 1
        HeightPix = BWImage.GetUpperBound(1) + 1

        ' image represented as two dimension boolean array
        Me.BWImage = BWImage

        ' set save as to BWImage
        SaveAs = SaveImageAs.BWImage
        Return
    End Sub

    ''' <summary>
    ''' Load image fro Pdf417Encoder
    ''' </summary>
    ''' <param name="Pdf417Encoder">Pdf417 encoder</param>
    Public Sub LoadImage(Pdf417Encoder As Pdf417Encoder)
        ' barcode width and height
        WidthPix = Pdf417Encoder.ImageWidth
        HeightPix = Pdf417Encoder.ImageHeight

        ' black and white barcode image
        BWImage = Pdf417Encoder.ConvertBarcodeMatrixToPixels()

        ' set save as to BWImage
        SaveAs = SaveImageAs.BWImage
        ReverseBW = True

        ' write to output file
        WriteObjectToPdfFile()

        ' exit
        Return
    End Sub

    ''' <summary>
    ''' Load image from QRCode encoder
    ''' </summary>
    ''' <param name="QREncoder">QRCode encoder</param>
    Public Sub LoadImage(QREncoder As QREncoder)
        ' barcode width and height
        WidthPix = QREncoder.QRCodeImageDimension
        HeightPix = WidthPix

        ' black and white barcode image
        BWImage = QREncoder.ConvertQRCodeMatrixToPixels()

        ' set save as to BWImage
        SaveAs = SaveImageAs.BWImage
        ReverseBW = True

        ' write to output file
        WriteObjectToPdfFile()

        ' exit
        Return
    End Sub


    ' Load image from disk file into Image class

    Friend Function LoadImageFromFile(ImageFileName As String) As Image
        ' test exitance
        If Not File.Exists(ImageFileName) Then Throw New ApplicationException("Image file " & ImageFileName & " does not exist")

        ' get file length
        Dim FI As FileInfo = New FileInfo(ImageFileName)
        Dim ImageFileLength = FI.Length
        If ImageFileLength >= Integer.MaxValue Then Throw New ApplicationException("Image file " & ImageFileName & " too long")

        ' load the image file
        Dim Image As Image

        Try
            ' file is metafile format
            If ImageFileName.EndsWith(".emf", StringComparison.OrdinalIgnoreCase) OrElse ImageFileName.EndsWith(".wmf", StringComparison.OrdinalIgnoreCase) Then

                ' all other image formats
                Image = New Metafile(ImageFileName)
            Else
                Image = New Bitmap(ImageFileName)
            End If

            ' not image file
        Catch __unusedArgumentException1__ As ArgumentException
            Throw New ApplicationException("Invalid image file: " & ImageFileName)
        End Try

        ' set dispose image flag
        DisposeImage = True

        ' return
        Return Image
    End Function


    ' Create Image rectangle
    ' some images have origin not at top left corner

    Friend Sub ImageRectangle(Image As Image)
        ' image rectangle
        ImageRect = New Rectangle(0, 0, Image.Width, Image.Height)

        ' some images have origin not at top left corner
        Dim Unit = GraphicsUnit.Pixel
        Dim ImageBounds = Image.GetBounds(Unit)

        If ImageBounds.X <> 0.0 OrElse ImageBounds.Y <> 0.0 Then
            ' set origin
            If Unit = GraphicsUnit.Pixel Then
                ImageRect.X = CInt(ImageBounds.X)
                ImageRect.Y = CInt(ImageBounds.Y)
            Else
                ImageRect.X = CInt(ImageBounds.X * Image.Width / ImageBounds.Width)
                ImageRect.Y = CInt(ImageBounds.Y * Image.Height / ImageBounds.Height)
            End If
        End If

        Return
    End Sub


    ' Set image size in pixels
    ' If crop is active adjust image size to crop rectangle


    Friend Sub ImageSizeInPixels(Image As Image)
        ' crop rectangle is given in percent width or height
        If CropRect.IsEmpty AndAlso Not CropPercent.IsEmpty Then
            CropRect = New Rectangle(0.01 * Image.Width * CropPercent.X + 0.5, 0.01 * Image.Height * CropPercent.Y + 0.5, 0.01 * Image.Width * CropPercent.Width + 0.5, 0.01 * Image.Height * CropPercent.Height + 0.5)
        End If

        ' no crop
        If CropRect.IsEmpty Then
            ' get image width and height in pixels
            WidthPix = Image.Width
            HeightPix = Image.Height
            Return
        End If

        ' crop
        ' adjust origin
        If ImageRect.X <> 0 OrElse ImageRect.Y <> 0 Then
            CropRect.X += ImageRect.X
            CropRect.Y += ImageRect.Y
        End If

        ' crop rectangle must be contained within image rectangle
        If Not ImageRect.Contains(CropRect) Then Throw New ApplicationException("PdfImage: Crop rectangle must be contained within image rectangle")

        ' change image size to crop size
        WidthPix = CropRect.Width
        HeightPix = CropRect.Height

        ' replace image rectangle with crop rectangle
        ImageRect = CropRect
        Return
    End Sub


    ' Convert image to bitmap


    Friend Sub ConvertImageToBitmap(Image As Image)
        ' destination rectangle
        Dim DestRect As Rectangle = New Rectangle(0, 0, WidthPix, HeightPix)

        ' resolution pixels per inch
        Dim HorizontalResolution As Double = Image.HorizontalResolution
        Dim VerticalResolution As Double = Image.VerticalResolution

        ' adjust resolution if it is not zero or greater than exising resolution
        If _Resolution <> 0 Then
            ' image resolution
            Dim ImageResolution = 0.5 * (HorizontalResolution + VerticalResolution)

            ' requested resolution is less than image
            If _Resolution < ImageResolution Then
                ' change in resolution 
                Dim Factor = _Resolution / ImageResolution

                ' convert to pixels based on requested resolution
                Dim NewWidthPix As Integer = WidthPix * Factor + 0.5
                Dim NewHeightPix As Integer = HeightPix * Factor + 0.5

                ' new size in pixels is must be smaller than image size or cropped image size
                If NewWidthPix < WidthPix AndAlso NewHeightPix < HeightPix Then
                    ' new image size in pixels
                    WidthPix = NewWidthPix
                    HeightPix = NewHeightPix
                    DestRect.Width = NewWidthPix
                    DestRect.Height = NewHeightPix

                    ' adjust resolution
                    HorizontalResolution *= Factor
                    VerticalResolution *= Factor
                Else
                    _Resolution = 0
                End If
            Else
                _Resolution = 0
            End If
        End If

        ' Assume we will need to dispose the Picture Bitmap
        DisposePicture = True

        ' image is Bitmap (not Metafile)
        If Image.GetType() Is GetType(Bitmap) Then
            ' no crop
            If CropRect.IsEmpty Then
                ' image is bitmap, no crop, no change in resolution
                If _Resolution = 0 Then
                    Picture = CType(Image, Bitmap)
                    DisposePicture = DisposeImage

                    ' image is bitmap, no crop, change to resolution
                    DisposeImage = False
                Else
                    ' load bitmap into smaller bitmap
                    Picture = New Bitmap(Image, WidthPix, HeightPix)

                    ' crop image
                End If
            Else
                ' create bitmap
                Picture = New Bitmap(WidthPix, HeightPix)

                ' create graphics object fill with white
                Dim GR = Graphics.FromImage(Picture)

                ' draw the image into the bitmap
                GR.DrawImage(Image, DestRect, ImageRect, GraphicsUnit.Pixel)

                ' dispose of the graphics object
                GR.Dispose()

                ' image is Metafile (not Bitmap)
            End If
        Else
            ' create bitmap
            Picture = New Bitmap(WidthPix, HeightPix)

            ' create graphics object fill with white
            Dim GR = Graphics.FromImage(Picture)
            GR.Clear(Color.White)

            'GR.CompositingQuality = CompositingQuality.HighSpeed;
            'GR.InterpolationMode = InterpolationMode.Low;
            'GR.SmoothingMode = SmoothingMode.None;

            ' draw the image into the bitmap
            GR.DrawImage(Image, DestRect, ImageRect, GraphicsUnit.Pixel)

            ' dispose of the graphics object
            GR.Dispose()
        End If

        ' dispose image
        If DisposeImage Then Image.Dispose()

        ' set resolution
        Picture.SetResolution(HorizontalResolution, VerticalResolution)
        Return
    End Sub


    ' Write object to PDF file

    Friend Overrides Sub WriteObjectToPdfFile()
        ' add items to dictionary
        Dictionary.AddInteger("/Width", WidthPix)
        Dictionary.AddInteger("/Height", HeightPix)

        ' layer control
        If LayerControl IsNot Nothing Then Dictionary.AddIndirectReference("/OC", LayerControl)

        ' switch based on save as method
        Select Case SaveAs
            Case SaveImageAs.Jpeg
_Select0_CasePdfFileWriter_SaveImageAs_Jpeg:
                PictureToJpeg()
            Case SaveImageAs.IndexedImage

                If Not PictureToIndexedImage() Then
                    GoTo _Select0_CasePdfFileWriter_SaveImageAs_Jpeg
                End If

            Case SaveImageAs.GrayImage

                If Not PictureToGrayImage() Then
                    GoTo _Select0_CasePdfFileWriter_SaveImageAs_Jpeg
                End If

            Case SaveImageAs.BWImage

                If Picture IsNot Nothing Then
                    If Not PictureToBWImage() Then
                        GoTo _Select0_CasePdfFileWriter_SaveImageAs_Jpeg
                    End If
                Else
                    BooleanToBWImage()
                End If
        End Select

        ' dispose picture
        Dispose()

        ' debug
        If Document.Debug Then ObjectValueArray = Document.TextToByteArray("*** IMAGE PLACE HOLDER ***")

        ' write stream
        MyBase.WriteObjectToPdfFile()
        Return
    End Sub


    ' Convert .net bitmap image to PDF indexed bitmap image


    Friend Sub PictureToJpeg()
        ' create memory stream
        Dim MS As MemoryStream = New MemoryStream()

        ' image quality is default
        If ImageQuality = DefaultQuality Then
            ' save in jpeg format with 75 quality

            ' save image with defined quality
            Picture.Save(MS, ImageFormat.Jpeg)
        Else
            ' build EncoderParameter object for image quality
            Dim EncoderParameters As EncoderParameters = New EncoderParameters(1)
            EncoderParameters.Param(0) = New EncoderParameter(Encoder.Quality, ImageQuality)

            ' save in jpeg format with specified quality
            Picture.Save(MS, GetEncoderInfo("image/jpeg"), EncoderParameters)
        End If

        ' image byte array
        ObjectValueArray = MS.GetBuffer()

        ' close and dispose memory stream
        MS.Close()
        MS = Nothing

        ' no deflate compression
        NoCompression = True

        ' image dictionary
        Dictionary.Add("/Filter", "/DCTDecode")
        Dictionary.Add("/ColorSpace", "/DeviceRGB")
        Dictionary.Add("/BitsPerComponent", "8")
        Return
    End Sub


    ' Convert .net bitmap image to PDF indexed bitmap image


    Friend Function PictureToIndexedImage() As Boolean
        ' if Picture Bitmap cannot be converted to RGB array, return with false
        Dim PictureData As BitmapData

        Try
            ' lock picture and get array of R G B bytes
            PictureData = Picture.LockBits(New Rectangle(0, 0, WidthPix, HeightPix), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb)
        Catch
            Return False
        End Try

        ' frame width in bytes
        Dim FrameWidth = std.Abs(PictureData.Stride)

        ' number of unused bytes at the end of the frame
        Dim PicDelta = FrameWidth - 3 * WidthPix

        ' allocate byte array for picture bytes
        Dim PictureBytes = New Byte(FrameWidth * HeightPix - 1) {}

        ' pointer to start of data in unmanaged memory
        Dim Scan0 = PictureData.Scan0

        ' copy RGB bytes from picture to local array
        Marshal.Copy(Scan0, PictureBytes, 0, PictureBytes.Length)

        ' unlock picture
        Picture.UnlockBits(PictureData)

        ' create indexed color array
        Dim ColorArray As List(Of Integer) = New List(Of Integer)()
        Dim PicPtr = 0

        For Y = 0 To HeightPix - 1

            For X = 0 To WidthPix - 1
                If ColorArray.Count = 256 Then Return False
                ' color order is blue, green and red
                Dim Pixel As Integer = PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) Or PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) << 8 Or PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) << 16
                Dim Index = ColorArray.BinarySearch(Pixel)
                If Index >= 0 Then Continue For
                ColorArray.Insert(Not Index, Pixel)
            Next

            PicPtr += PicDelta
        Next

        Dim BitPerComponent As Integer

        ' create stream for 1 or 2 colors
        If ColorArray.Count <= 2 Then
            ' bits per component
            BitPerComponent = 1

            ' each row must be multiple of bytes
            Dim WidthBytes As Integer = (WidthPix + 7) / 8
            Dim ObjDelta = If((WidthPix And 7) = 0, 0, 1)

            ' creale empty object value array
            ObjectValueArray = New Byte(WidthPix * WidthBytes - 1) {}


            ' convert picture in rgb to color index
            PicPtr = 0
            Dim ObjPtr = 0

            For Y = 0 To HeightPix - 1

                For X = 0 To WidthPix - 1
                    Dim Pixel As Integer = PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) Or PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) << 8 Or PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) << 16
                    Dim Index = ColorArray.BinarySearch(Pixel)
                    If Index <> 0 Then ObjectValueArray(ObjPtr) = ObjectValueArray(ObjPtr) Or OneBitMask(X And 7)
                    If (X And 7) = 7 Then ObjPtr += 1
                Next

                PicPtr += PicDelta
                ObjPtr += ObjDelta
            Next

            ' create stream for 3 to 4 colors
        ElseIf ColorArray.Count <= 4 Then
            ' bits per component
            BitPerComponent = 2

            ' each row must be multiple of bytes
            Dim WidthBytes As Integer = (WidthPix + 3) / 4
            Dim ObjDelta = If((WidthPix And 3) = 0, 0, 1)

            ' creale empty object value array
            ObjectValueArray = New Byte(WidthBytes * HeightPix - 1) {}

            ' convert picture in rgb to color index
            PicPtr = 0
            Dim ObjPtr = 0

            For Y = 0 To HeightPix - 1
                Dim Shift = 6

                For X = 0 To WidthPix - 1
                    Dim Pixel As Integer = PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) Or PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) << 8 Or PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) << 16
                    Dim Index = ColorArray.BinarySearch(Pixel)
                    ObjectValueArray(ObjPtr) = ObjectValueArray(ObjPtr) Or CByte(Index << Shift)
                    Shift -= 2

                    If Shift < 0 Then
                        Shift = 6
                        ObjPtr += 1
                    End If
                Next

                PicPtr += PicDelta
                ObjPtr += ObjDelta
            Next

            ' create stream for 5 or 16 colors
        ElseIf ColorArray.Count <= 16 Then
            ' bits per component
            BitPerComponent = 4

            ' each row must be multiple of bytes
            Dim WidthBytes As Integer = (WidthPix + 1) \ 2
            Dim ObjDelta = WidthPix And 1

            ' creale empty object value array
            ' ObjectValueArray = New Byte(WidthBytes * HeightPix - 1) {}
            ObjectValueArray = New Byte(WidthBytes * HeightPix - 1) {}

            ' convert picture in rgb to color index
            PicPtr = 0
            Dim ObjPtr = 0

            For Y = 0 To HeightPix - 1

                For X = 0 To WidthPix - 1
                    Dim Pixel As Integer = PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) Or PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) << 8 Or PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) << 16
                    Dim Index = ColorArray.BinarySearch(Pixel)

                    If (X And 1) = 0 Then
                        ObjectValueArray(ObjPtr) = CByte(Index << 4)
                    Else
                        ObjectValueArray(std.Min(Threading.Interlocked.Increment(ObjPtr), ObjPtr - 1)) = ObjectValueArray(std.Min(Threading.Interlocked.Increment(ObjPtr), ObjPtr - 1)) Or CByte(Index)
                    End If
                Next

                PicPtr += PicDelta
                ObjPtr += ObjDelta

                ' create stream for 17 to 256 colors
            Next
        Else
            ' 8 bits per component
            BitPerComponent = 8

            ' allocate one byte per pixel array
            ObjectValueArray = New Byte(WidthPix * HeightPix - 1) {}

            ' convert picture in rgb to color index
            PicPtr = 0
            Dim ObjPtr = 0

            For Y = 0 To HeightPix - 1

                For X = 0 To WidthPix - 1
                    Dim Pixel As Integer = PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) Or PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) << 8 Or PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) << 16
                    ObjectValueArray(std.Min(Threading.Interlocked.Increment(ObjPtr), ObjPtr - 1)) = CByte(ColorArray.BinarySearch(Pixel))
                Next

                PicPtr += PicDelta
            Next
        End If

        ' convert color array from int to byte
        Dim ColorByteArray = New Byte(ColorArray.Count * 3 - 1) {}
        Dim ColorPtr As i32 = 0

        For Index = 0 To ColorArray.Count - 1
            ColorByteArray(++ColorPtr) = CByte(ColorArray(Index) >> 16)
            ColorByteArray(++ColorPtr) = CByte(ColorArray(Index) >> 8)
            ColorByteArray(++ColorPtr) = CByte(ColorArray(Index))
        Next

        ' encryption is active. PDF string must be encrypted
        If Document.Encryption IsNot Nothing Then ColorByteArray = Document.Encryption.EncryptByteArray(ObjectNumber, ColorByteArray)

        ' convert byte array to PDF string format
        Dim ColorStr = Document.ByteArrayToPdfString(ColorByteArray)

        ' add items to dictionary
        Dictionary.AddFormat("/ColorSpace", "[/Indexed /DeviceRGB {0} {1}]", ColorArray.Count - 1, ColorStr)    ' R G B
        Dictionary.AddInteger("/BitsPerComponent", BitPerComponent) ' 1 2 4 8 
        Return True
    End Function


    ' Convert .net bitmap image to PDF indexed bitmap image


    Friend Function PictureToGrayImage() As Boolean
        ' if Picture Bitmap cannot be converted to RGB array, return with false
        Dim PictureData As BitmapData

        Try
            ' lock picture and get array of Blue green and Red bytes
            PictureData = Picture.LockBits(New Rectangle(0, 0, WidthPix, HeightPix), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb)
        Catch
            Return False
        End Try

        ' frame width in bytes
        Dim FrameWidth = std.Abs(PictureData.Stride)

        ' number of unused bytes at the end of the frame
        Dim PicDelta = FrameWidth - 3 * WidthPix

        ' allocate byte array for picture bytes
        Dim PictureBytes = New Byte(FrameWidth * HeightPix - 1) {}

        ' pointer to start of data in unmanaged memory
        Dim Scan0 = PictureData.Scan0

        ' copy RGB bytes from picture to local array
        Marshal.Copy(Scan0, PictureBytes, 0, PictureBytes.Length)

        ' unlock picture
        Picture.UnlockBits(PictureData)

        ' allocate one byte per pixel array
        ObjectValueArray = New Byte(WidthPix * HeightPix - 1) {}

        ' convert picture in rgb to shades of gray
        Dim PicPtr = 0
        Dim ObjPtr = 0

        For Y = 0 To HeightPix - 1

            For X = 0 To WidthPix - 1
                ' bytes are in blue green red order
                Dim Pixel As Integer = (11 * PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) + 59 * PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) + 30 * PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) + 50) / 100
                ObjectValueArray(std.Min(Threading.Interlocked.Increment(ObjPtr), ObjPtr - 1)) = CByte(Pixel)
            Next

            PicPtr += PicDelta
        Next

        ' add items to dictionary
        Dictionary.Add("/ColorSpace", "/DeviceGray")
        Dictionary.Add("/BitsPerComponent", "8")
        If ReverseBW Then Dictionary.Add("/Decode", "[1 0]")
        Return True
    End Function


    ' Convert .net bitmap image to PDF indexed bitmap image


    Friend Function PictureToBWImage() As Boolean
        ' if Picture Bitmap cannot be converted to RGB array, return with false
        Dim PictureData As BitmapData

        Try
            ' lock picture and get array of Blue green and Red bytes
            PictureData = Picture.LockBits(New Rectangle(0, 0, WidthPix, HeightPix), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb)
        Catch
            Return False
        End Try

        ' frame width in bytes
        Dim FrameWidth = std.Abs(PictureData.Stride)

        ' number of unused bytes at the end of the frame
        Dim PicDelta = FrameWidth - 3 * WidthPix

        ' allocate byte array for picture bytes
        Dim PictureBytes = New Byte(FrameWidth * HeightPix - 1) {}

        ' pointer to start of data in unmanaged memory
        Dim Scan0 = PictureData.Scan0

        ' copy RGB bytes from picture to local array
        Marshal.Copy(Scan0, PictureBytes, 0, PictureBytes.Length)

        ' unlock picture
        Picture.UnlockBits(PictureData)

        ' each row must be multiple of bytes
        Dim WidthBytes As Integer = (WidthPix + 7) / 8

        ' creale empty object value array
        ObjectValueArray = New Byte(HeightPix * WidthBytes - 1) {}

        ' QRCode matrix to PDF bitmap
        Dim PicPtr = 0
        Dim RowPtr = 0
        Dim Cutoff = 255 * _GrayToBWCutoff

        For Row = 0 To HeightPix - 1

            For Col = 0 To WidthPix - 1
                If 11 * PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) + 59 * PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) + 30 * PictureBytes(std.Min(Threading.Interlocked.Increment(PicPtr), PicPtr - 1)) >= Cutoff Then ObjectValueArray(RowPtr + (Col >> 3)) = ObjectValueArray(RowPtr + (Col >> 3)) Or CByte(1 << 7 - (Col And 7))
            Next

            PicPtr += PicDelta
            RowPtr += WidthBytes
        Next

        ' add items to dictionary
        Dictionary.Add("/ColorSpace", "/DeviceGray")
        Dictionary.Add("/BitsPerComponent", "1")
        If ReverseBW Then Dictionary.Add("/Decode", "[1 0]")
        Return True
    End Function


    ' Convert .net bitmap image to PDF indexed bitmap image

    Friend Sub BooleanToBWImage()
        ' each row must be multiple of bytes
        Dim WidthBytes As Integer = (WidthPix + 7) / 8

        ' creale empty object value array
        ObjectValueArray = New Byte(HeightPix * WidthBytes - 1) {}

        ' QRCode matrix to PDF bitmap
        Dim RowPtr = 0

        For Row = 0 To HeightPix - 1

            For Col = 0 To WidthPix - 1
                If BWImage(Row, Col) Then ObjectValueArray(RowPtr + (Col >> 3)) = ObjectValueArray(RowPtr + (Col >> 3)) Or CByte(1 << 7 - (Col And 7))
            Next

            RowPtr += WidthBytes
        Next

        ' add items to dictionary
        Dictionary.Add("/ColorSpace", "/DeviceGray")
        Dictionary.Add("/BitsPerComponent", "1")
        If ReverseBW Then Dictionary.Add("/Decode", "[1 0]")
        Return
    End Sub


    ' Write object to PDF file

    Private Function GetEncoderInfo(mimeType As String) As ImageCodecInfo
        Dim EncoderArray As ImageCodecInfo() = ImageCodecInfo.GetImageEncoders()

        For Each Encoder In EncoderArray
            If Equals(Encoder.MimeType, mimeType) Then Return Encoder
        Next

        Throw New ApplicationException("GetEncoderInfo: image/jpeg encoder does not exist")
    End Function


    ''' <summary>
    ''' Calculates image size to preserve aspect ratio.
    ''' </summary>
    ''' <param name="InputSize">Image display area.</param>
    ''' <returns>Adjusted image display area.</returns>
    ''' <remarks>
    ''' Calculates best fit to preserve aspect ratio.
    ''' </remarks>

    Public Function ImageSize(InputSize As SizeD) As SizeD
        Return ImageSizePos.ImageSize(WidthPix, HeightPix, InputSize.Width, InputSize.Height)
    End Function


    ''' <summary>
    ''' Calculates image size to preserve aspect ratio.
    ''' </summary>
    ''' <param name="Width">Image display width.</param>
    ''' <param name="Height">Image display height.</param>
    ''' <returns>Adjusted image display area.</returns>
    ''' <remarks>
    ''' Calculates best fit to preserve aspect ratio.
    ''' </remarks>

    Public Function ImageSize(Width As Double, Height As Double) As SizeD
        Return ImageSizePos.ImageSize(WidthPix, HeightPix, Width, Height)
    End Function


    ''' <summary>
    ''' Calculates image size to preserve aspect ratio and sets position.
    ''' </summary>
    ''' <param name="InputSize">Image display area</param>
    ''' <param name="Alignment">Content alignment</param>
    ''' <returns>Adjusted image size and position within area.</returns>
    ''' <remarks>
    ''' Calculates best fit to preserve aspect ratio and adjust
    ''' position according to content alignment argument.
    ''' </remarks>

    Public Function ImageSizePosition(InputSize As SizeD, Alignment As ContentAlignment) As PdfRectangle
        Return ImageArea(WidthPix, HeightPix, 0.0, 0.0, InputSize.Width, InputSize.Height, Alignment)
    End Function


    ''' <summary>
    ''' Calculates image size to preserve aspect ratio and sets position.
    ''' </summary>
    ''' <param name="Width">Image display width</param>
    ''' <param name="Height">Image display height</param>
    ''' <param name="Alignment">Content alignment</param>
    ''' <returns>Adjusted image size and position within area.</returns>
    ''' <remarks>
    ''' Calculates best fit to preserve aspect ratio and adjust
    ''' position according to content alignment argument.
    ''' </remarks>

    Public Function ImageSizePosition(Width As Double, Height As Double, Alignment As ContentAlignment) As PdfRectangle
        Return ImageArea(WidthPix, HeightPix, 0.0, 0.0, Width, Height, Alignment)
    End Function


    ''' <summary>
    ''' Dispose unmanaged resources
    ''' </summary>

    Public Sub Dispose() Implements IDisposable.Dispose
        ' release bitmap
        If DisposePicture AndAlso Picture IsNot Nothing Then
            Picture.Dispose()
            Picture = Nothing
        End If

        ' exit
        Return
    End Sub

    Private Const ObsoleteError As Boolean = False
    Private Const ObsoleteMsg As String = "This PdfImage constructor is obsolete. See latest documentation."

    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public Sub New(Document As PdfDocument, ImageFileName As String, Optional ImageControl As PdfImageControl = Nothing)
        Me.New(Document)
        ConstructorHelper(LoadImageFromFile(ImageFileName), ImageControl)
        Return
    End Sub

    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public Sub New(Document As PdfDocument, Image As Image, Optional ImageControl As PdfImageControl = Nothing)
        Me.New(Document)
        ConstructorHelper(Image, ImageControl)
        Return
    End Sub

    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public Sub New(Document As PdfDocument, BWImage As Boolean(,), Optional ImageControl As PdfImageControl = Nothing)
        Me.New(Document)
        ' image dimensions
        WidthPix = BWImage.GetUpperBound(0) + 1
        HeightPix = BWImage.GetUpperBound(1) + 1

        ' image represented as two dimension boolean array
        Me.BWImage = BWImage

        ' default image control
        If ImageControl IsNot Nothing Then
            CropRect = ImageControl.CropRect
            CropPercent = ImageControl.CropPercent
            ReverseBW = ImageControl.ReverseBW
            GrayToBWCutoff = ImageControl.GrayToBWCutoff
            Resolution = ImageControl.Resolution
            ImageQuality = ImageControl.ImageQuality
            SaveAs = ImageControl.SaveAs
        End If

        SaveAs = SaveImageAs.BWImage

        ' write image stream to pdf file
        WriteObjectToPdfFile()
        Return
    End Sub

    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public Sub New(Document As PdfDocument, Pdf417Encoder As Pdf417Encoder)
        Me.New(Document)
        ' barcode width and height
        WidthPix = Pdf417Encoder.ImageWidth
        HeightPix = Pdf417Encoder.ImageHeight

        ' black and white barcode image
        BWImage = Pdf417Encoder.ConvertBarcodeMatrixToPixels()

        ' image control for PDF417 code
        ReverseBW = True
        SaveAs = SaveImageAs.BWImage

        ' write image stream to pdf file
        WriteObjectToPdfFile()
        Return
    End Sub

    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Public Sub New(Document As PdfDocument, QREncoder As QREncoder)
        Me.New(Document)
        ' barcode width and height
        WidthPix = QREncoder.QRCodeImageDimension
        HeightPix = QREncoder.QRCodeImageDimension

        ' black and white barcode image
        BWImage = QREncoder.ConvertQRCodeMatrixToPixels()

        ' image control for PDF417 code
        ReverseBW = True
        SaveAs = SaveImageAs.BWImage

        ' write image stream to pdf file
        WriteObjectToPdfFile()
        Return
    End Sub

    <Obsolete(ObsoleteMsg, ObsoleteError)>
    Private Sub ConstructorHelper(Image As Image, ImageControl As PdfImageControl)
        ' image control
        If ImageControl Is Nothing Then ImageControl = New PdfImageControl()
        CropRect = ImageControl.CropRect
        CropPercent = ImageControl.CropPercent
        ReverseBW = ImageControl.ReverseBW
        GrayToBWCutoff = ImageControl.GrayToBWCutoff
        Resolution = ImageControl.Resolution
        ImageQuality = ImageControl.ImageQuality
        SaveAs = ImageControl.SaveAs

        ' image rectangle
        ImageRectangle(Image)

        ' image size in pixels
        ImageSizeInPixels(Image)

        ' convert the image to bitmap
        ConvertImageToBitmap(Image)

        ' write to output file
        WriteObjectToPdfFile()

        ' exit
        Return
    End Sub
End Class
