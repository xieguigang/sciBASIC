Imports System.Drawing
Imports System.Drawing.Imaging

Namespace Image2ASCII

    ''' <summary>
    ''' Program that converts images to ASCII art images
    ''' 
    ''' > https://github.com/juangallostra/Image2ASCII
    ''' </summary>
    Public Module HelperMethods

        Public Function Convert2ASCII(BW_Image As Image, characters As List(Of WeightedChar), ByRef ResultText As List(Of List(Of String))) As Image
            '
            '             * ALGORITHM:
            '             * 
            '             *  1- Get target Image size (w=Width,h=Height)
            '             *  2- Create Result Image with white background and size W = w*character_image_width
            '             *                                                        H = h*character_image_height
            '             *  3- Create empty string to hold the text   
            '             *  
            '             *  4- for (int j=0;j=target_Image_Height;j++) --> ALL ROWS 
            '             *       5- Create row text string
            '             *       for (int i=0;i=target_Image_Width;i++) --> ALL COLUMNS
            '             *          6- Get target pixel weight
            '             *          7- Get closest weight from character list
            '             *          8- Paste character image in position w = i*character_image_width
            '             *                                               h = j*character_image_height
            '             *            ¡¡ Be careful with coordinate system when placing Images !!
            '             *          9- Add (string)character to row text string
            '             *       10- Add row text string to text holding string
            '             *  11 - return resulting Image & Text
            '             

            Dim ResultImage As Image = New Bitmap(BW_Image.Width * characters(0).CharacterImage.Width, BW_Image.Height * characters(0).CharacterImage.Height)
            Dim drawing As Graphics = Graphics.FromImage(ResultImage)
            drawing.Clear(Color.White)
            ResultText = New List(Of List(Of String))() From {
            }
            Dim BlackAndWhite As Bitmap = DirectCast(BW_Image, Bitmap)

            For j As Integer = 0 To BW_Image.Height - 1
                ' ROW
                Dim RowText As New List(Of String)() From {
                }
                For i As Integer = 0 To BW_Image.Width - 1
                    ' COLUMN
                    Dim pixel As Color = BlackAndWhite.GetPixel(i, j)
                    Dim targetvalue As Double = (pixel.R + pixel.G + pixel.B) \ 3

                    Dim closestchar As WeightedChar = characters.Where(Function(t) Math.Abs(t.Weight - targetvalue) = characters.Min(Function(e) Math.Abs(e.Weight - targetvalue))).FirstOrDefault()
                    RowText.Add(closestchar.Character)
                    drawing.DrawImage(closestchar.CharacterImage, i * closestchar.CharacterImage.Width, j * closestchar.CharacterImage.Height)
                Next
                ResultText.Add(RowText)
            Next
            drawing.Dispose()
            Return DirectCast(ResultImage, Image)
        End Function

        Public Function Convert2ASCIIColor(ResizedImage_O As Image, characters As List(Of WeightedChar), ImageText As List(Of List(Of String))) As Image
            '
            '             * ALGORITHM
            '             * 1- Create result image with white background
            '             *  2- for (int j=0;j=target_Image_Height;j++) --> ALL ROWS 
            '             *       for (int i=0;i=target_Image_Width;i++) --> ALL COLUMNS
            '             *          6- Get target pixel color, get target character from string
            '             *          7- Create Image with the correct size, color and character
            '             *          8- Paste character image in position w = i*character_image_width
            '             *                                               h = j*character_image_height
            '             *            ¡¡ Be careful with coordinate system when placing Images !!
            '             *  11 - return resulting Image
            '             



            ' Needed variables for iteration

            ' Result Image and graphics object
            Dim ResultImage As Image = New Bitmap(ResizedImage_O.Width * characters(0).CharacterImage.Width, ResizedImage_O.Height * characters(0).CharacterImage.Height)
            Dim drawing As Graphics = Graphics.FromImage(ResultImage)
            drawing.Clear(Color.White)
            ' ResizedImage Bitmap data and byte-encoded pixel lenght
            Dim bmp = DirectCast(ResizedImage_O, Bitmap)
            Dim bitmapdata = bmp.LockBits(New Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
            Dim PixelSize As Integer = 4
            ' character image size
            Dim width As Integer = characters(0).CharacterImage.Width
            Dim height As Integer = characters(0).CharacterImage.Height



            ' foreach pixel in image
            For j As Integer = 0 To ResizedImage_O.Height - 1
                Dim destPixels As Pointer(Of Byte) = CType(bitmapdata.Scan0, Pointer(Of Byte)) + (j * bitmapdata.Stride)

                For i As Integer = 0 To ResizedImage_O.Width - 1
                    ' get pixel color
                    Dim B = CInt(destPixels(i * PixelSize))
                    ' B
                    Dim G = CInt(destPixels(i * PixelSize + 1))
                    ' G
                    Dim R = CInt(destPixels(i * PixelSize + 2))
                    ' R
                    ' get character
                    Dim character = ImageText(j)(i)
                    ' create char image
                    Dim charimage = DrawText(character, Color.FromArgb(R, G, B), Color.White, New SizeF(width, height))
                    ' paste char image 

                    drawing.DrawImage(charimage, i * charimage.Width, j * charimage.Height)
                Next
            Next
            bmp.UnlockBits(bitmapdata)
            drawing.Dispose()
            Return DirectCast(ResultImage, Image)
        End Function

        Public Sub AdjustContrast(bmp As Bitmap, contrast As Double)
            If True Then
                Dim contrast_lookup As Byte() = New Byte(255) {}
                Dim newValue As Double = 0
                Dim c As Double = (100.0 + contrast) / 100.0

                c *= c

                For i As Integer = 0 To 255
                    newValue = CDbl(i)
                    newValue /= 255.0
                    newValue -= 0.5
                    newValue *= c
                    newValue += 0.5
                    newValue *= 255

                    If newValue < 0 Then
                        newValue = 0
                    End If
                    If newValue > 255 Then
                        newValue = 255
                    End If
                    contrast_lookup(i) = CByte(Math.Truncate(newValue))
                Next

                Dim bitmapdata = bmp.LockBits(New Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)

                Dim PixelSize As Integer = 4

                For y As Integer = 0 To bitmapdata.Height - 1
                    Dim destPixels As Pointer(Of Byte) = CType(bitmapdata.Scan0, Pointer(Of Byte)) + (y * bitmapdata.Stride)
                    For x As Integer = 0 To bitmapdata.Width - 1
                        destPixels(x * PixelSize) = contrast_lookup(destPixels(x * PixelSize))
                        ' B
                        destPixels(x * PixelSize + 1) = contrast_lookup(destPixels(x * PixelSize + 1))
                        ' G
                        ' R
                        'destPixels[x * PixelSize + 3] = contrast_lookup[destPixels[x * PixelSize + 3]]; //A
                        destPixels(x * PixelSize + 2) = contrast_lookup(destPixels(x * PixelSize + 2))
                    Next
                Next
                bmp.UnlockBits(bitmapdata)
            End If
        End Sub

        Public Function DrawText(text As String, textColor As Color, backColor As Color, WidthAndHeight As SizeF) As Image

            ' Get char width for insertion point calculation purposes
            Dim dummy_img As Image = New Bitmap(1, 1)
            Dim dummy_drawing As Graphics = Graphics.FromImage(dummy_img)
            Dim textSize As SizeF = dummy_drawing.MeasureString(text, SystemFonts.DefaultFont)
            dummy_img.Dispose()
            dummy_drawing.Dispose()

            ' Create a dummy bitmap just to get a graphics object
            Dim img As Image = New Bitmap(1, 1)
            Dim drawing As Graphics = Graphics.FromImage(img)

            ' Free up resources taken by the dummy image and old graphics object
            img.Dispose()
            drawing.Dispose()

            ' Create a new image of the right size
            img = New Bitmap(CInt(Math.Truncate(WidthAndHeight.Width)), CInt(Math.Truncate(WidthAndHeight.Height)))
            ' Get a graphics object
            drawing = Graphics.FromImage(img)

            ' Paint the background
            drawing.Clear(backColor)

            ' Create a brush for the text
            Dim textBrush As Brush = New SolidBrush(textColor)

            drawing.DrawString(text, SystemFonts.DefaultFont, textBrush, (WidthAndHeight.Width - textSize.Width) / 2, 0)
            ' El punto de inserción del carácter se puede afinar más (Trial & Error)
            drawing.Save()

            textBrush.Dispose()
            drawing.Dispose()

            Return img
        End Function
    End Module
End Namespace

