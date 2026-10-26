Imports System.IO
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.BitmapImage.Jpeg
Imports Microsoft.VisualBasic.Imaging.BitmapImage.Gif
Imports Microsoft.VisualBasic.Serialization

''' <summary>
''' the dependency free jpeg / gif codec self test: encode -> decode round trip,
''' the default image loader magic number dispatch and the bitmap save routing.
''' </summary>
Module JpegGifSelfTest

    Private failures As Integer = 0

    Function Run() As Integer
        failures = 0

        Call Console.WriteLine()
        Call Console.WriteLine("-- jpeg / gif image codec --")

        Call JpegRoundTrip()
        Call GifStaticRoundTrip()
        Call GifAnimatedRoundTrip()
        Call DefaultLoaderDispatch()
        Call BitmapSaveRouting()

        Return failures
    End Function

    Private Sub Check(condition As Boolean, message As String)
        If condition Then
            Call Console.WriteLine($"   [ok]   {message}")
        Else
            failures += 1
            Call Console.WriteLine($"   [FAIL] {message}")
        End If
    End Sub

    ''' <summary>
    ''' build a 3 channels (rgb -> bgr) bitmap buffer from a per pixel colour function.
    ''' </summary>
    Private Function BuildRgb(w As Integer, h As Integer, fn As Func(Of Integer, Integer, Integer())) As BitmapBuffer
        Dim buf(w * h * 3 - 1) As Byte

        For y As Integer = 0 To h - 1
            For x As Integer = 0 To w - 1
                Dim c As Integer() = fn(x, y)
                Dim i As Integer = (y * w + x) * 3
                buf(i) = CByte(c(2))      ' b
                buf(i + 1) = CByte(c(1))  ' g
                buf(i + 2) = CByte(c(0))  ' r
            Next
        Next

        Return New BitmapBuffer(buf, New Size(w, h), 3)
    End Function

    Private Sub JpegRoundTrip()
        Dim w As Integer = 32
        Dim h As Integer = 32

        ' a smooth gradient keeps the lossy error small
        Dim src As BitmapBuffer = BuildRgb(w, h, Function(x, y)
                                                 Return New Integer() {
                                                     CInt(255 * x / (w - 1)),
                                                     CInt(255 * y / (h - 1)),
                                                     CInt(255 * (x + y) / (2 * (w - 1)))
                                                 }
                                             End Function)

        Using ms As New MemoryStream()
            Call JpegWriter.Save(ms, src, 90)
            Dim jpg As Byte() = ms.ToArray()

            Call Check(jpg.Length > 0, $"jpeg encoded size > 0 (got {jpg.Length} bytes)")

            Dim dec As JpegImage = JpegImage.Decode(jpg)

            Call Check(dec.Width = w AndAlso dec.Height = h, $"jpeg decode size matches ({dec.Width}x{dec.Height})")

            Dim db As BitmapBuffer = dec.GetMemoryBitmap()
            Dim a As Byte() = src.RawBuffer
            Dim b As Byte() = db.RawBuffer

            Dim maxDiff As Integer = 0
            Dim sumDiff As Long = 0

            For i As Integer = 0 To a.Length - 1
                Dim d As Integer = Math.Abs(CInt(a(i)) - CInt(b(i)))
                If d > maxDiff Then maxDiff = d
                sumDiff += d
            Next

            Call Check(maxDiff < 64, $"jpeg max per-pixel diff < 64 (got {maxDiff})")
            Call Check((sumDiff \ a.Length) < 24, $"jpeg average per-channel diff < 24 (got {sumDiff \ a.Length})")
        End Using
    End Sub

    Private Sub GifStaticRoundTrip()
        ' isolated lzw round trip
        Dim testIdx As Byte() = {0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 3, 3, 3, 2, 2, 1, 1, 0, 0, 1, 2, 3, 1, 1, 1, 2, 2, 2, 3, 3}
        Using ms As New MemoryStream()
            Call GifLzwEncoder.Encode(testIdx, ms, 2)
            Dim enc As Byte() = ms.ToArray()
            Using ms2 As New MemoryStream(enc)
                Dim decStream As New MemoryStream()
                Call GifLzwDecoder.Decode(ms2, decStream, 2)
                Dim dec As Byte() = decStream.ToArray()
                Dim ds As String = "", es As String = ""
                For i As Integer = 0 To testIdx.Length - 1
                    If i < dec.Length Then ds &= dec(i).ToString() & " "
                Next
                For i As Integer = 0 To testIdx.Length - 1 : es &= testIdx(i).ToString() & " " : Next
                Call Console.WriteLine($"   [dbg] enc={enc.Length}B dec={dec.Length}B")
                Call Console.WriteLine($"   [dbg] exp: {es}")
                Call Console.WriteLine($"   [dbg] got: {ds}")
                Dim same As Boolean = (dec.Length = testIdx.Length)
                If same Then
                    For i As Integer = 0 To testIdx.Length - 1
                        If dec(i) <> testIdx(i) Then same = False : Exit For
                    Next
                End If
                Call Check(same, $"isolated gif lzw round-trip (dec len={dec.Length}, exp={testIdx.Length})")
            End Using
        End Using

        Dim w As Integer = 16
        Dim h As Integer = 16
        Dim palette As Integer()() = {
            New Integer() {255, 0, 0},
            New Integer() {0, 255, 0},
            New Integer() {0, 0, 255},
            New Integer() {255, 255, 255}
        }

        Dim src As BitmapBuffer = BuildRgb(w, h, Function(x, y) palette((x + y) Mod 4))

        Using ms As New MemoryStream()
            Call GifWriter.Save(ms, src)
            Dim gif As Byte() = ms.ToArray()

            Call System.IO.File.WriteAllBytes("gif_debug.gif", gif)
            Dim head As String = ""
            For i As Integer = 0 To Math.Min(60, gif.Length) - 1
                head &= gif(i).ToString("X2") & " "
            Next
            Call Console.WriteLine($"   [dbg] gif len={gif.Length} minCodeSize@35={gif(35)} firstLen@36={gif(36)} :: {head}")

            Call Check(gif.Length > 0, $"gif encoded size > 0 (got {gif.Length} bytes)")

            Dim dec As GifImage = GifImage.Decode(gif)

            Call Check(dec.Width = w AndAlso dec.Height = h, $"gif size matches ({dec.Width}x{dec.Height})")
            Call Check(dec.FrameCount = 1, $"gif single frame (got {dec.FrameCount})")

            Dim a As Byte() = src.RawBuffer
            Dim b As Byte() = dec.GetMemoryBitmap().RawBuffer
            Dim same As Boolean = (a.Length = b.Length)

            If same Then
                For i As Integer = 0 To a.Length - 1 Step 3
                    If a(i) <> b(i) OrElse a(i + 1) <> b(i + 1) OrElse a(i + 2) <> b(i + 2) Then
                        same = False
                        Exit For
                    End If
                Next
            End If

            Call Check(same, "gif static pixels exact round-trip")
        End Using
    End Sub

    Private Sub GifAnimatedRoundTrip()
        Dim w As Integer = 8
        Dim h As Integer = 8
        Dim palette As Integer()() = {
            New Integer() {255, 0, 0},
            New Integer() {0, 255, 0},
            New Integer() {0, 0, 255},
            New Integer() {255, 255, 255}
        }

        Dim frames As New List(Of BitmapBuffer)()

        For k As Integer = 0 To 2
            frames.Add(BuildRgb(w, h, Function(x, y) palette((x + y + k) Mod 4)))
        Next

        Dim delays As Integer() = {10, 20, 30}

        Using ms As New MemoryStream()
            Call GifWriter.Save(ms, frames, 0, delays, Nothing)
            Dim gif As Byte() = ms.ToArray()

            Dim dec As GifImage = GifImage.Decode(gif)

            Call Check(dec.FrameCount = 3, $"animated frame count = 3 (got {dec.FrameCount})")
            Call Check(dec.LoopCount = 0, $"loop count = 0 (got {dec.LoopCount})")
            Call Check(dec.Delays.Length = 3 AndAlso dec.Delays(0) = 10 AndAlso dec.Delays(1) = 20 AndAlso dec.Delays(2) = 30, "per frame delays preserved")

            Dim a As Byte() = frames(0).RawBuffer
            Dim b As Byte() = dec.Frame(0).RawBuffer
            Dim same As Boolean = (a.Length = b.Length)

            If same Then
                For i As Integer = 0 To a.Length - 1 Step 3
                    If a(i) <> b(i) OrElse a(i + 1) <> b(i + 1) OrElse a(i + 2) <> b(i + 2) Then
                        same = False
                        Exit For
                    End If
                Next
            End If

            Call Check(same, "animated first frame exact round-trip")
        End Using
    End Sub

    Private Sub DefaultLoaderDispatch()
        Dim w As Integer = 16
        Dim h As Integer = 16
        Dim palette As Integer()() = {
            New Integer() {255, 0, 0},
            New Integer() {0, 255, 0},
            New Integer() {0, 0, 255},
            New Integer() {255, 255, 255}
        }
        Dim src As BitmapBuffer = BuildRgb(w, h, Function(x, y) palette((x + y) Mod 4))

        ' gif dispatch
        Using ms As New MemoryStream(GifWriter.Encode(New List(Of BitmapBuffer) From {src}, 0))
            Dim img As Image = Image.FromStream(ms)
            Call Check(img IsNot Nothing AndAlso img.Width = w AndAlso img.Height = h, "Image.FromStream(gif) decodes")
            Call Check(TypeOf img Is GifImage, "Image.FromStream(gif) returns GifImage")
        End Using

        ' jpeg dispatch
        Using tmp As New MemoryStream()
            Call JpegWriter.Save(tmp, src, 90)
            Using ms As New MemoryStream(tmp.ToArray())
                Dim img As Image = Image.FromStream(ms)
                Call Check(img IsNot Nothing AndAlso img.Width = w AndAlso img.Height = h, "Image.FromStream(jpeg) decodes")
                Call Check(TypeOf img Is JpegImage, "Image.FromStream(jpeg) returns JpegImage")
            End Using
        End Using
    End Sub

    Private Sub BitmapSaveRouting()
        Dim w As Integer = 16
        Dim h As Integer = 16
        Dim src As BitmapBuffer = BuildRgb(w, h, Function(x, y) New Integer() {(x * 16) Mod 256, (y * 16) Mod 256, ((x + y) * 8) Mod 256})
        Dim bmp As New Bitmap(src)

        ' jpeg
        Using ms As New MemoryStream()
            Call bmp.Save(ms, ImageFormats.Jpeg)
            Dim jpg As Byte() = ms.ToArray()
            Call Check(jpg.Length > 2 AndAlso jpg(0) = &HFF AndAlso jpg(1) = &HD8, "Bitmap.Save(Jpeg) emits a valid jpeg stream")
        End Using

        ' gif
        Using ms As New MemoryStream()
            Call bmp.Save(ms, ImageFormats.Gif)
            Dim gif As Byte() = ms.ToArray()
            Call Check(gif.Length > 6 AndAlso gif(0) = AscW("G"c) AndAlso gif(1) = AscW("I"c) AndAlso gif(2) = AscW("F"c), "Bitmap.Save(Gif) emits a valid gif header")
        End Using

        ' png
        Using ms As New MemoryStream()
            Call bmp.Save(ms, ImageFormats.Png)
            Dim png As Byte() = ms.ToArray()
            Call Check(png.Length > 8 AndAlso png(0) = &H89 AndAlso png(1) = &H50 AndAlso png(2) = &H4E AndAlso png(3) = &H47, "Bitmap.Save(Png) emits a valid png header")
        End Using
    End Sub
End Module
