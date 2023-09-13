Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Linq

Namespace Filters

    Public Class ImageStrech

        ''' <summary>
        ''' [x => [y => pixel]]
        ''' </summary>
        ReadOnly matrix As Dictionary(Of Integer, Dictionary(Of Integer, Pixel))
        ''' <summary>
        ''' the original image size
        ''' </summary>
        ReadOnly dims As Rectangle

        Sub New(img As IRasterGrayscaleHeatmap)
            matrix = img.GetRasterPixels _
                .GroupBy(Function(p) p.X) _
                .ToDictionary(Function(c) c.Key,
                              Function(c)
                                  Return c.ToDictionary(Function(p) p.Y)
                              End Function)
            dims = New Polygon2D(pull).GetDimension
        End Sub

        Private Function pull() As IEnumerable(Of Pixel)
            Return matrix.Values.Select(Function(c) c.Values).IteratesALL
        End Function

        ''' <summary>
        ''' Resize and strech the current image matrix data
        ''' </summary>
        ''' <param name="newSize"></param>
        ''' <returns></returns>
        Public Iterator Function Resize(newSize As Size) As IEnumerable(Of Pixel)
            Dim originalSize As Size = dims.Size
            Dim ratioX As Double = newSize.Width / originalSize.Width
            Dim ratioY As Double = newSize.Height / originalSize.Height
        End Function
    End Class
End Namespace