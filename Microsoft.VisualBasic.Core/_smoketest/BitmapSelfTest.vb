Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.BitmapImage.FileStream

Namespace BitmapSelfTest

    ''' <summary>
    ''' The self test of the bmp image file reader and writer: the 1/4/8/16/24/32 
    ''' bits per pixel image, the color palette, the row padding, the top down 
    ''' DIB and the row order.
    ''' </summary>
    Module BitmapSelfTest

        Private failures As Integer = 0
        Private dir As String = IO.Path.Combine(IO.Path.GetTempPath(), "bmp_self_test")

        ''' <summary>
        ''' run all of the bmp image file checks
        ''' </summary>
        ''' <returns>the number of the failed checks.</returns>
        Function Run() As Integer
            failures = 0

            If Not IO.Directory.Exists(dir) Then
                Call IO.Directory.CreateDirectory(dir)
            End If

            Call Console.WriteLine()
            Call Console.WriteLine("-- bmp image file --")

            Call TestTrueColor()
            Call TestIndexedColor()
            Call TestSixteenBits()
            Call TestTopDown()
            Call TestRowOrder()
            Call TestRandomAccessReader()

            Return failures
        End Function

        ''' <summary>
        ''' the 24bpp and the 32bpp image: the file layout, the row padding and 
        ''' the save/load round trip
        ''' </summary>
        Private Sub TestTrueColor()
            Call Section("the 24bpp and the 32bpp image")

            ' 7 pixels of a 24bpp row is 21 bytes, so that the row padding is required
            Dim w As Integer = 7
            Dim h As Integer = 5

            For Each bits As BitsPerPixelEnum In New BitsPerPixelEnum() {BitsPerPixelEnum.RGB24, BitsPerPixelEnum.RGBA32}
                Dim channels As Integer = If(bits = BitsPerPixelEnum.RGB24, 3, 4)
                Dim pixels As Byte() = MakePixels(w, h, channels)
                Dim file As String = IO.Path.Combine(dir, $"{CInt(bits)}bpp.bmp")

                Call BitmapFileHelper.SaveBitmapToFile(file, New Bitmap(w, h, pixels, bits))

                Dim raw As Byte() = IO.File.ReadAllBytes(file)
                Dim bytesPerRow As Integer = Bitmap.RequiredBytesPerRow(w, bits)
                Dim infoHeaderSize As Integer = BitmapFileHeader.InfoHeaderSize(bits, 0)
                Dim offset As Integer = 14 + infoHeaderSize

                Call Check(raw(0) = AscW("B"c) AndAlso raw(1) = AscW("M"c), $"the {CInt(bits)}bpp file has the BM signature")
                Call Check(BitConverter.ToInt32(raw, 10) = offset, $"the {CInt(bits)}bpp pixel data offset = {BitConverter.ToInt32(raw, 10)}")
                Call Check(raw.Length = offset + bytesPerRow * h, $"the {CInt(bits)}bpp file size = {raw.Length} bytes")
                Call Check(BitConverter.ToInt32(raw, 18) = w AndAlso BitConverter.ToInt32(raw, 22) = h, $"the {CInt(bits)}bpp image size = {w}x{h}")

                Dim image As Bitmap = BitmapFileHelper.ReadFileAsBitmap(file)

                Call Check(image.Width = w AndAlso image.Height = h AndAlso image.BitsPerPixelEnum = bits,
                           $"the {CInt(bits)}bpp image is loaded as {image.Width}x{image.Height} {CInt(image.BitsPerPixelEnum)}bpp")
                Call Check(SameBytes(pixels, image.PixelData), $"the {CInt(bits)}bpp pixel data is identical")

                ' the file that is saved back from the loaded image should be 
                ' the same as the source file
                Dim again As String = IO.Path.Combine(dir, $"{CInt(bits)}bpp.again.bmp")

                Call BitmapFileHelper.SaveBitmapToFile(again, image)
                Call Check(SameBytes(raw, IO.File.ReadAllBytes(again)), $"the {CInt(bits)}bpp file survives the save/load round trip")
            Next

            ' the 24bpp image of 4 pixels requires no row padding at all
            Dim unpad As Byte() = MakePixels(4, 3, 3)
            Dim unpadFile As String = IO.Path.Combine(dir, "24bpp_nopad.bmp")

            Call BitmapFileHelper.SaveBitmapToFile(unpadFile, New Bitmap(4, 3, unpad, BitsPerPixelEnum.RGB24))
            Call Check(IO.File.ReadAllBytes(unpadFile).Length = 54 + 12 * 3, "the 24bpp file of 4 pixels has no row padding")
            Call Check(SameBytes(unpad, BitmapFileHelper.ReadFileAsBitmap(unpadFile).PixelData), "the unpadded 24bpp pixel data is identical")
        End Sub

        ''' <summary>
        ''' the 1/4/8 bits per pixel indexed image and its color palette
        ''' </summary>
        Private Sub TestIndexedColor()
            Call Section("the indexed image with the color palette")

            Dim w As Integer = 16
            Dim h As Integer = 8

            For Each bits As BitsPerPixelEnum In New BitsPerPixelEnum() {BitsPerPixelEnum.Eight, BitsPerPixelEnum.Four, BitsPerPixelEnum.Monochrome}
                Dim entries As Integer = Bitmap.PaletteEntries(bits)
                Dim packed As Byte() = MakeIndexed(bits, w, h)
                Dim file As String = IO.Path.Combine(dir, $"{CInt(bits)}bpp_indexed.bmp")
                Dim source As New Bitmap(w, h, packed, bits)

                Call Check(source.Stride = Bitmap.UnpaddedRowSize(w, bits), $"the {CInt(bits)}bpp scan line = {source.Stride} bytes")
                Call Check(source.Palette.Length = entries * 4, $"the {CInt(bits)}bpp palette = {source.Palette.Length} bytes")

                Call BitmapFileHelper.SaveBitmapToFile(file, source)

                Dim raw As Byte() = IO.File.ReadAllBytes(file)
                Dim bytesPerRow As Integer = Bitmap.RequiredBytesPerRow(w, bits)

                Call Check(BitConverter.ToInt32(raw, 10) = 14 + 40 + entries * 4, $"the {CInt(bits)}bpp pixel data is stored after the palette")
                Call Check(raw.Length = 14 + 40 + entries * 4 + bytesPerRow * h, $"the {CInt(bits)}bpp file size = {raw.Length} bytes")

                Dim image As Bitmap = BitmapFileHelper.ReadFileAsBitmap(file)

                Call Check(image.BitsPerPixelEnum = bits, $"the {CInt(bits)}bpp image is loaded as {CInt(image.BitsPerPixelEnum)}bpp")
                Call Check(SameBytes(packed, image.PixelData), $"the {CInt(bits)}bpp index data is identical")
                Call Check(SameBytes(source.Palette, image.Palette), $"the {CInt(bits)}bpp color palette is identical")
                Call Check(SameBytes(raw, IO.File.ReadAllBytes(SaveAgain(image, $"{CInt(bits)}bpp_indexed.again.bmp"))),
                           $"the {CInt(bits)}bpp file survives the save/load round trip")

                ' the indexed image could be expanded into the rgb color image
                Dim rgb As Bitmap = BitmapFileHelper.ReadFileAsBitmap(file, expandToRgb24:=True)

                Call Check(rgb.BitsPerPixelEnum = BitsPerPixelEnum.RGB24, $"the {CInt(bits)}bpp image is expanded as {CInt(rgb.BitsPerPixelEnum)}bpp")

                Dim gray As Integer = GrayOf(ReadIndex(packed, bits, 3, 2), entries)
                Dim at As Integer = (2 * w + 3) * 3

                Call Check(rgb.PixelData(at) = CByte(gray) AndAlso
                           rgb.PixelData(at + 1) = CByte(gray) AndAlso
                           rgb.PixelData(at + 2) = CByte(gray),
                           $"the {CInt(bits)}bpp pixel (3,2) is expanded into the gray scale {gray}")
            Next
        End Sub

        ''' <summary>
        ''' the 16bpp RGB555 image
        ''' </summary>
        Private Sub TestSixteenBits()
            Call Section("the 16bpp image")

            Dim w As Integer = 5
            Dim h As Integer = 4
            Dim pixels As Byte() = New Byte(w * h * 2 - 1) {}

            For y As Integer = 0 To h - 1
                For x As Integer = 0 To w - 1
                    Dim v As Integer = ((x * 7 + y * 13) * 257) And &HFFFF
                    Dim at As Integer = (y * w + x) * 2

                    pixels(at) = CByte(v And &HFF)
                    pixels(at + 1) = CByte((v >> 8) And &HFF)
                Next
            Next

            Dim file As String = IO.Path.Combine(dir, "16bpp.bmp")
            Dim image As New Bitmap(w, h, pixels, BitsPerPixelEnum.RBG16)

            ' the 16bpp image is stored in the standard 40 bytes BITMAPINFOHEADER
            Call Check(BitmapFileHeader.InfoHeaderSize(BitsPerPixelEnum.RBG16, 0) = 40, "the 16bpp DIB header is the 40 bytes BITMAPINFOHEADER")

            Call BitmapFileHelper.SaveBitmapToFile(file, image)

            Dim raw As Byte() = IO.File.ReadAllBytes(file)
            Dim back As Bitmap = BitmapFileHelper.ReadFileAsBitmap(file)

            Call Check(BitConverter.ToInt32(raw, 10) = 54, $"the 16bpp pixel data offset = {BitConverter.ToInt32(raw, 10)}")
            Call Check(back.BitsPerPixelEnum = BitsPerPixelEnum.RBG16, "the 16bpp image is loaded as 16bpp")
            Call Check(SameBytes(pixels, back.PixelData), "the 16bpp pixel data is identical")

            Dim rgb As Bitmap = BitmapFileHelper.ReadFileAsBitmap(file, expandToRgb24:=True)
            Dim v1 As Integer = (pixels(0) Or (CInt(pixels(1)) << 8))

            Call Check(rgb.PixelData(0) = CByte(((v1 >> 10) And &H1F) * 255 \ 31), $"the 16bpp pixel is expanded into the rgb {rgb.PixelData(0)}")
        End Sub

        ''' <summary>
        ''' a negative image height of the DIB header means that the pixel rows 
        ''' are stored from the top to the bottom
        ''' </summary>
        Private Sub TestTopDown()
            Call Section("the top down DIB")

            Dim w As Integer = 4
            Dim h As Integer = 3
            Dim pixels As Byte() = MakePixels(w, h, 3)
            Dim file As String = SavePixels("24bpp_4x3.bmp", pixels, w, h, BitsPerPixelEnum.RGB24)
            Dim topDownFile As String = IO.Path.Combine(dir, "24bpp_topdown.bmp")
            Dim raw As Byte() = IO.File.ReadAllBytes(file)

            ' flip the sign of the image height of the DIB header
            Dim negativeHeight As Integer = -BitConverter.ToInt32(raw, 22)

            raw(22) = CByte(negativeHeight And &HFF)
            raw(23) = CByte((negativeHeight >> 8) And &HFF)
            raw(24) = CByte((negativeHeight >> 16) And &HFF)
            raw(25) = CByte((negativeHeight >> 24) And &HFF)

            Call IO.File.WriteAllBytes(topDownFile, raw)

            Dim bottomUp As Bitmap = BitmapFileHelper.ReadFileAsBitmap(file)
            Dim topDown As Bitmap = BitmapFileHelper.ReadFileAsBitmap(topDownFile)

            Call Check(topDown.Height = h, $"the negative image height {BitConverter.ToInt32(raw, 22)} is loaded as {topDown.Height}")

            ' the top down rows are normalized into the bottom up row order
            Dim reversed As Byte() = New Byte(pixels.Length - 1) {}
            Dim rowSize As Integer = w * 3

            For row As Integer = 0 To h - 1
                Call System.Buffer.BlockCopy(src:=pixels,
                                             srcOffset:=row * rowSize,
                                             dst:=reversed,
                                             dstOffset:=(h - 1 - row) * rowSize,
                                             count:=rowSize)
            Next

            Call Check(SameBytes(reversed, topDown.PixelData), "the top down DIB is normalized into the bottom up row order")
            Call Check(Not SameBytes(bottomUp.PixelData, topDown.PixelData), "the top down DIB is not loaded as a bottom up one")
        End Sub

        ''' <summary>
        ''' the flipRows option and the <see cref="Bitmap.PixelDataFliped"/> 
        ''' property were ignored by the old implementation
        ''' </summary>
        Private Sub TestRowOrder()
            Call Section("the row order")

            Dim w As Integer = 4
            Dim h As Integer = 3
            Dim pixels As Byte() = MakePixels(w, h, 3)
            Dim file As String = SavePixels("24bpp_4x3.bmp", pixels, w, h, BitsPerPixelEnum.RGB24)
            Dim raw As Byte() = IO.File.ReadAllBytes(file)
            Dim image As Bitmap = BitmapFileHelper.ReadFileAsBitmap(file)
            Dim flipped As Bitmap = BitmapFileHelper.ReadFileAsBitmap(file, flipRows:=True)

            Call Check(Not SameBytes(image.PixelData, flipped.PixelData), "the flipRows option is applied on the loaded pixel data")
            Call Check(SameBytes(flipped.PixelData, image.PixelDataFliped), "the flipRows result is the same as the PixelDataFliped")
            Call Check(SameBytes(image.PixelData, flipped.PixelDataFliped), "the row reversing is reversible")

            ' the flipped rows could be saved as it is and loaded back
            Dim flippedFile As String = IO.Path.Combine(dir, "24bpp_flipped.bmp")

            Call BitmapFileHelper.SaveBitmapToFile(flippedFile, image, flipRows:=True)
            Call Check(SameBytes(flipped.PixelData, BitmapFileHelper.ReadFileAsBitmap(flippedFile).PixelData), "the flipped rows are saved and loaded back")
            Call Check(Not SameBytes(raw, IO.File.ReadAllBytes(flippedFile)), "the flipped file is different from the source file")
        End Sub

        ''' <summary>
        ''' the random access reader of the huge scientific image data
        ''' </summary>
        Private Sub TestRandomAccessReader()
            Call Section("the random access bitmap reader")

            Dim w As Integer = 4
            Dim h As Integer = 3
            Dim file As String = SavePixels("24bpp_4x3.bmp", MakePixels(w, h, 3), w, h, BitsPerPixelEnum.RGB24)

            Using reader As New BitmapReader(file)
                Call Check(reader.ImageWidth = w AndAlso reader.ImageHeight = h, $"the reader sees the {reader.ImageWidth}x{reader.ImageHeight} image")
                Call Check(reader.RowSize = 12, $"the reader row size = {reader.RowSize} bytes")

                ' the pixel rows are stored from the bottom to the top, so that 
                ' the visual row 0 is the last row of the file
                Dim c As Drawing.Color = reader.GetPixelColor(0, 0)
                Dim r As Integer = (0 * 7 + 2 * 13 + 2 * 29) Mod 256
                Dim g As Integer = (0 * 7 + 2 * 13 + 1 * 29) Mod 256
                Dim b As Integer = (0 * 7 + 2 * 13 + 0 * 29) Mod 256

                Call Check(c.R = r AndAlso c.G = g AndAlso c.B = b, $"the pixel (0,0) = RGB({c.R},{c.G},{c.B}), expected RGB({r},{g},{b})")
            End Using

            ' an indexed image is read through the color palette
            Dim indexed As String = IO.Path.Combine(dir, "8bpp_indexed.bmp")

            Using reader As New BitmapReader(indexed)
                Call Check(reader.BitsPerPixel = BitsPerPixel.Eight, $"the reader sees the {CInt(reader.BitsPerPixel)}bpp image")
                Call Check(reader.PaletteEntrySize = 4, $"the palette entry = {reader.PaletteEntrySize} bytes")
                Call Check(reader.PaletteOffset = 54, $"the palette is stored at {reader.PaletteOffset}")

                Dim c As Drawing.Color = reader.GetPixelColor(0, 0)
                Dim index As Integer = (0 * 17 + 7 * 31) And &HFF

                Call Check(c.R = index AndAlso c.G = index AndAlso c.B = index, $"the indexed pixel (0,0) = gray {c.R}, expected {index}")
            End Using
        End Sub

        Private Function SavePixels(name As String, pixels As Byte(), w As Integer, h As Integer, bits As BitsPerPixelEnum) As String
            Dim file As String = IO.Path.Combine(dir, name)

            Call BitmapFileHelper.SaveBitmapToFile(file, New Bitmap(w, h, pixels, bits))

            Return file
        End Function

        Private Function SaveAgain(image As Bitmap, name As String) As String
            Dim file As String = IO.Path.Combine(dir, name)

            Call BitmapFileHelper.SaveBitmapToFile(file, image)

            Return file
        End Function

        ''' <summary>
        ''' (x*7 + y*13 + c*29) Mod 256, the pixel value function of the test image
        ''' </summary>
        Private Function MakePixels(w As Integer, h As Integer, channels As Integer) As Byte()
            Dim buffer As Byte() = New Byte(w * h * channels - 1) {}

            For y As Integer = 0 To h - 1
                For x As Integer = 0 To w - 1
                    For c As Integer = 0 To channels - 1
                        buffer((y * w + x) * channels + c) = CByte((x * 7 + y * 13 + c * 29) Mod 256)
                    Next
                Next
            Next

            Return buffer
        End Function

        ''' <summary>
        ''' the bit packed index data of the indexed image
        ''' </summary>
        Private Function MakeIndexed(bits As BitsPerPixelEnum, w As Integer, h As Integer) As Byte()
            Dim stride As Integer = Bitmap.UnpaddedRowSize(w, bits)
            Dim buffer As Byte() = New Byte(stride * h - 1) {}

            For y As Integer = 0 To h - 1
                For x As Integer = 0 To w - 1
                    Dim v As Integer = (x * 17 + y * 31) And &HFF
                    Dim at As Integer = y * stride

                    Select Case bits
                        Case BitsPerPixelEnum.Eight
                            buffer(at + x) = CByte(v)
                        Case BitsPerPixelEnum.Four
                            Dim index As Integer = (v >> 4) And &HF
                            Dim p As Integer = at + x \ 2

                            If (x Mod 2) = 0 Then
                                buffer(p) = CByte(buffer(p) Or (index << 4))
                            Else
                                buffer(p) = CByte(buffer(p) Or index)
                            End If
                        Case Else
                            Dim index As Integer = (v >> 7) And &H1
                            Dim p As Integer = at + x \ 8

                            buffer(p) = CByte(buffer(p) Or (index << (7 - (x Mod 8))))
                    End Select
                Next
            Next

            Return buffer
        End Function

        Private Function ReadIndex(packed As Byte(), bits As BitsPerPixelEnum, x As Integer, y As Integer) As Integer
            Dim w As Integer = 16
            Dim stride As Integer = Bitmap.UnpaddedRowSize(w, bits)
            Dim at As Integer = y * stride

            Select Case bits
                Case BitsPerPixelEnum.Eight : Return packed(at + x)
                Case BitsPerPixelEnum.Four
                    Dim b As Integer = packed(at + x \ 2)

                    Return If((x Mod 2) = 0, (b >> 4) And &HF, b And &HF)
                Case Else
                    Return (CInt(packed(at + x \ 8)) >> (7 - (x Mod 8))) And &H1
            End Select
        End Function

        Private Function GrayOf(index As Integer, entries As Integer) As Integer
            Return CInt(System.Math.Round(index * 255.0 / (entries - 1)))
        End Function

        Private Function SameBytes(a As Byte(), b As Byte()) As Boolean
            If a Is Nothing OrElse b Is Nothing Then
                Return a Is Nothing AndAlso b Is Nothing
            End If
            If a.Length <> b.Length Then
                Return False
            End If

            For i As Integer = 0 To a.Length - 1
                If a(i) <> b(i) Then
                    Return False
                End If
            Next

            Return True
        End Function

        Private Sub Section(title As String)
            Call Console.WriteLine($"   * {title}")
        End Sub

        Private Sub Check(condition As Boolean, message As String)
            If condition Then
                Call Console.WriteLine($"   [ok]   {message}")
            Else
                failures += 1
                Call Console.WriteLine($"   [FAIL] {message}")
            End If
        End Sub

    End Module

End Namespace
