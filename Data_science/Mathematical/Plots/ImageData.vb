Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging

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

    Public Function SurfaceProvider(img As Image) As Func(Of Double, Double, (z#, color As Double))
        Dim bitmap As hBitmap = hBitmap.FromImage(img)

        Return Function(x, y) As (Z#, Color As Double)
                   Dim c As Color = bitmap.GetPixel(x, y)
                   Dim b# = c.GetBrightness
                   Dim h# = c.ColorToDecimal

                   Return (Z:=b, Color:=h)
               End Function
    End Function
End Module
