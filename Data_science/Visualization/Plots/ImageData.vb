#Region "Microsoft.VisualBasic::a5edf9acba440d4bd079aada56b8ee60, Data_science\Visualization\Plots\ImageData.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module ImageDataExtensions
    ' 
    '     Function: Image2DMap, Image3DMap, PointZProvider, SurfaceProvider
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver

Public Module ImageDataExtensions

    <Extension>
    Public Function PointZProvider(img As Image, Optional convert As Func(Of Color, Integer) = Nothing) As Func(Of Double, Double, Double)
        Dim bitmap As BitmapBuffer = BitmapBuffer.FromImage(img)

        If convert Is Nothing Then
            convert = AddressOf GrayScale
        End If

        Return Function(x, y)
                   If Not bitmap.OutOfRange(x, y) Then
                       Return convert(bitmap.GetPixel(x, y))
                   Else
                       Return -1
                   End If
               End Function
    End Function

    <Extension>
    Public Function SurfaceProvider(img As Image) As Func(Of Double, Double, (z#, color As Double))
        Dim bitmap As BitmapBuffer = BitmapBuffer.FromImage(img)

        Return Function(x, y) As (Z#, Color As Double)
                   If Not bitmap.OutOfRange(x, y) Then
                       Dim c As Color = bitmap.GetPixel(x, y)
                       Dim b# = c.GrayScale
                       Dim h# = c.ToArgb

                       Return (Z:=b, Color:=h)
                   Else
                       Return (0#, 0#)
                   End If
               End Function
    End Function

    <Extension>
    Public Function Image2DMap(img As Image, Optional steps% = 1) As GraphicsData
        Dim color = img.PointZProvider
        Dim xrange As DoubleRange = $"0 -> {img.Width}"
        Dim yrange As DoubleRange = $"0 -> {img.Height}"

        Return Contour.Plot(
            color, xrange, yrange,
            xsteps:=steps, ysteps:=steps, unit:=1, scale:=3,
            colorMap:="Jet",
            legendTitle:="GrayScale Heatmap")
    End Function

    <Extension>
    Public Function Image3DMap(img As Image, camera As Camera, Optional steps% = 1) As GraphicsData
        Dim Z = img.SurfaceProvider
        Dim xrange As DoubleRange = $"0 -> {img.Width}"
        Dim yrange As DoubleRange = $"0 -> {img.Height}"

        Return Plot3D.ScatterHeatmap.Plot(
            Z, xrange, yrange,
            camera,
            xn:=img.Width / steps,
            yn:=img.Height / steps) ', dev:=Plot3D.Device.NewWindow)
    End Function
End Module
