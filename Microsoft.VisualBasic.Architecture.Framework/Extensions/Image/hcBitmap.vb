Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices

Public Module hcBitmap

    <Extension>
    Public Function GetBinaryBitmap(res As Image, Optional style As BinarizationStyles = BinarizationStyles.Binary) As Bitmap
        Dim bmp As New Bitmap(DirectCast(res.Clone, Image))
        bmp.Binarization(style)
        Return bmp
    End Function

    Public Enum BinarizationStyles
        SparseGray = 3
        Binary = 4
    End Enum

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="curBitmap"></param>
    ''' <remarks>
    ''' http://www.codeproject.com/Articles/1094534/Image-Binarization-Using-Program-Languages
    ''' 
    ''' The .net Bitmap object keeps a reference to HBITMAP handle, Not to the underlying bitmap itself.
    ''' So, single pixel access call to <see cref="Bitmap.SetPixel"/>/<see cref="Bitmap.GetPixel"/> Or 
    ''' even retrieve Width/Height properties does something Like: 
    ''' lock handle In place-Get/Set value/unlock handle. It Is the most inefficient way To manipulate bitmaps In .NET. 
    ''' The author should read about <see cref="Bitmap.LockBits"/> first.
    ''' </remarks>
    <Extension> Public Sub Binarization(ByRef curBitmap As Bitmap, Optional style As BinarizationStyles = BinarizationStyles.Binary)
        Dim iR As Integer = 0 ' Red
        Dim iG As Integer = 0 ' Green
        Dim iB As Integer = 0 ' Blue

        ' Lock the bitmap's bits.  
        Dim rect As New Rectangle(0, 0, curBitmap.Width, curBitmap.Height)
        Dim bmpData As BitmapData =
            curBitmap.LockBits(rect, ImageLockMode.ReadWrite, curBitmap.PixelFormat)
        ' Get the address of the first line.
        Dim ptr As IntPtr = bmpData.Scan0
        ' Declare an array to hold the bytes of the bitmap.
        Dim bytes As Integer = Math.Abs(bmpData.Stride) * curBitmap.Height

        Using rgbValues As Marshal.Byte = New Marshal.Byte(ptr, bytes)
            Dim byts As Marshal.Byte = rgbValues

            ' Set every third value to 255. A 24bpp bitmap will binarization.  
            Do While Not rgbValues.NullEnd(3)
                ' Get the red channel
                iR = rgbValues(2)
                ' Get the green channel
                iG = rgbValues(1)
                ' Get the blue channel
                iB = rgbValues(0)
                ' If the gray value more than threshold and then set a white pixel.
                If (iR + iG + iB) / 3 > 100 Then
                    ' White pixel
                    byts(2) = 255
                    byts(1) = 255
                    byts(0) = 255
                Else
                    ' Black pixel
                    byts(2) = 0
                    byts(1) = 0
                    byts(0) = 0
                End If

                byts += style
            Loop
        End Using

        ' Unlock the bits.
        Call curBitmap.UnlockBits(bmpData)
    End Sub

    <Extension>
    Public Function ByteLength(rect As Rectangle) As Integer
        Dim width As Integer = rect.Width * 3
        Return width * rect.Height
    End Function

    <Extension>
    Public Iterator Function Colors(buffer As Byte()) As IEnumerable(Of Color)
        Dim byts As Byte() = New Byte(2) {}
        Dim iR As Byte
        Dim iG As Byte
        Dim iB As Byte

        For i As Integer = 0 To buffer.Length - 1
            iR = buffer(i + 2)
            iG = buffer(i + 1)
            iB = buffer(i + 0)

            Yield Color.FromArgb(CInt(iR), CInt(iG), CInt(iB))
        Next
    End Function
End Module

''' <summary>
''' 线程不安全的图片数据对象
''' </summary>
Public Class hBitmap : Inherits Marshal.Byte
    Implements IDisposable
    Implements IEnumerable(Of Color)

    ReadOnly __source As Bitmap
    ReadOnly __handle As BitmapData

    Protected Sub New(ptr As IntPtr,
                      byts As Integer,
                      raw As Bitmap,
                      handle As BitmapData)
        Call MyBase.New(ptr, byts)

        __source = raw
        __handle = handle

        Width = raw.Width
        Height = raw.Height
        Size = New Size(Width, Height)
    End Sub

    Public ReadOnly Property Width As Integer
    Public ReadOnly Property Height As Integer
    Public ReadOnly Property Size As Size

    Public Function GetImage() As Bitmap
        Return DirectCast(__source.Clone, Bitmap)
    End Function

    ''' <summary>
    ''' 返回第一个元素的位置
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns>B, G, R</returns>
    Public Function GetIndex(x As Integer, y As Integer) As Integer
        y = y * (Width * 3)
        x = x * 3
        Return x + y
    End Function

    ''' <summary>
    ''' Gets the color of the specified pixel in this System.Drawing.Bitmap.
    ''' </summary>
    ''' <param name="x">The x-coordinate of the pixel to retrieve.</param>
    ''' <param name="y">The y-coordinate of the pixel to retrieve.</param>
    ''' <returns>A System.Drawing.Color structure that represents the color of the specified pixel.</returns>
    Public Function GetPixel(x As Integer, y As Integer) As Color
        Dim i As Integer = GetIndex(x, y)
        Dim iR As Byte = __innerRaw(i + 2)
        Dim iG As Byte = __innerRaw(i + 1)
        Dim iB As Byte = __innerRaw(i + 0)

        Return Color.FromArgb(CInt(iR), CInt(iG), CInt(iB))
    End Function

    ''' <summary>
    ''' Sets the color of the specified pixel in this System.Drawing.Bitmap.(这个函数线程不安全)
    ''' </summary>
    ''' <param name="x">The x-coordinate of the pixel to set.</param>
    ''' <param name="y">The y-coordinate of the pixel to set.</param>
    ''' <param name="color">
    ''' A System.Drawing.Color structure that represents the color to assign to the specified
    ''' pixel.</param>
    Public Sub SetPixel(x As Integer, y As Integer, color As Color)
        Dim i As Integer = GetIndex(x, y)
        __innerRaw(i + 2) = color.R
        __innerRaw(i + 1) = color.G
        __innerRaw(i + 0) = color.B
    End Sub

    Public Shared Function FromImage(res As Image) As hBitmap
        Return hBitmap.FromBitmap(New Bitmap(res))
    End Function

    Public Shared Function FromBitmap(curBitmap As Bitmap) As hBitmap
        ' Lock the bitmap's bits.  
        Dim rect As New Rectangle(0, 0, curBitmap.Width, curBitmap.Height)
        Dim bmpData As BitmapData =
            curBitmap.LockBits(rect, ImageLockMode.ReadWrite, curBitmap.PixelFormat)
        ' Get the address of the first line.
        Dim ptr As IntPtr = bmpData.Scan0
        ' Declare an array to hold the bytes of the bitmap.
        Dim bytes As Integer = Math.Abs(bmpData.Stride) * curBitmap.Height

        Return New hBitmap(ptr, bytes, curBitmap, bmpData)
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        Call Write()
        Call __source.UnlockBits(__handle)
    End Sub

    Public Iterator Function GetEnumerator() As IEnumerator(Of Color) Implements IEnumerable(Of Color).GetEnumerator
        For Each x As Color In __innerRaw.Colors
            Yield x
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class