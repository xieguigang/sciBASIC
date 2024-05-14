#Region "Microsoft.VisualBasic::65769e0a88dc1bf5100e87e916a720bd, Data_science\Visualization\Plots\ImageData.vb"

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

    '   Total Lines: 74
    '    Code Lines: 62
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 2.80 KB


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
    Public Function Image2DMap(img As Image,
                               Optional scaleName As String = "Jet",
                               Optional mapLevels% = 25,
                               Optional steps% = 1) As GraphicsData

        Dim color = img.PointZProvider
        Dim xrange As DoubleRange = DoubleRange.TryParse($"0 -> {img.Width}")
        Dim yrange As DoubleRange = DoubleRange.TryParse($"0 -> {img.Height}")

        Return Contour.HeatMap.Plot(
            color, xrange, yrange,
            xsteps:=steps, ysteps:=steps, unit:=1,
            colorMap:=scaleName,
            legendTitle:="GrayScale Heatmap"
        )
    End Function

    <Extension>
    Public Function Image3DMap(img As Image, camera As Camera, Optional steps% = 1) As GraphicsData
        Dim Z = img.SurfaceProvider
        Dim xrange As DoubleRange = DoubleRange.TryParse($"0 -> {img.Width}")
        Dim yrange As DoubleRange = DoubleRange.TryParse($"0 -> {img.Height}")

        Return Plot3D.ScatterHeatmap.Plot(
            Z, xrange, yrange,
            camera,
            xn:=img.Width / steps,
            yn:=img.Height / steps) ', dev:=Plot3D.Device.NewWindow)
    End Function
End Module
