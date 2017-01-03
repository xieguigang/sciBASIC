Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Public Module ImageDataExtensions

    <Extension>
    Public Function PointZProvider(img As Image, Optional convert As Func(Of Color, Integer) = Nothing) As Func(Of Double, Double, Double)
        Dim bitmap As hBitmap = hBitmap.FromImage(img)

        If convert Is Nothing Then
            convert = AddressOf ColorToDecimal
        End If

        Return Function(x, y)
                   Return convert(bitmap.GetPixel(x, y))
               End Function
    End Function

    <Extension>
    Public Function SurfaceProvider(img As Image) As Func(Of Double, Double, (z#, color As Double))
        Dim bitmap As hBitmap = hBitmap.FromImage(img)

        Return Function(x, y) As (Z#, Color As Double)
                   Dim c As Color = bitmap.GetPixel(x, y)
                   Dim b# = c.GetBrightness
                   Dim h# = c.ColorToDecimal

                   Return (Z:=b, Color:=h)
               End Function
    End Function

    <Extension>
    Public Function Image2DMap(img As Image, Optional steps% = 5) As Bitmap
        Dim color = img.PointZProvider
        Dim xrange As DoubleRange = $"0 -> {img.Width}"
        Dim yrange As DoubleRange = $"0 -> {img.Height}"

        Return ScatterHeatmap.Plot(
            color, xrange, yrange,
            xsteps:=steps, ysteps:=steps)
    End Function

    <Extension>
    Public Function Image3DMap(img As Image, camera As Camera, Optional steps% = 5) As Bitmap
        Dim Z = img.SurfaceProvider
        Dim xrange As DoubleRange = $"0 -> {img.Width}"
        Dim yrange As DoubleRange = $"0 -> {img.Height}"

        Return Plot3D.ScatterHeatmap.Plot(
            Z, xrange, yrange,
            camera,
            xn:=img.Width / steps,
            yn:=img.Height / steps)
    End Function
End Module
