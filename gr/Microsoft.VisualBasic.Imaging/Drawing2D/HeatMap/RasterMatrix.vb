Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Drawing2D.HeatMap

    Public Class RasterMatrix : Implements IRasterGrayscaleHeatmap

        Dim m As GeneralMatrix

        Sub New(m As GeneralMatrix)
            Me.m = m
        End Sub

        Public Iterator Function GetRasterPixels() As IEnumerable(Of Pixel) Implements IRasterGrayscaleHeatmap.GetRasterPixels
            Dim y As Integer = 0

            For Each row As Vector In m.RowVectors
                Dim v = row.Array

                For x As Integer = 0 To v.Length - 1
                    Yield New PixelData With {
                        .Scale = v(x),
                        .X = x,
                        .Y = y
                    }
                Next

                y += 1
            Next
        End Function
    End Class
End Namespace