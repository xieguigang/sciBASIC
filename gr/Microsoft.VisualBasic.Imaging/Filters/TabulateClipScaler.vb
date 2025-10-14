Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Math.Distributions

Namespace Filters

    ''' <summary>
    ''' processing of the grayscale/heatmap scale image
    ''' </summary>
    Public Module TabulateClipScaler

        <Extension>
        Public Iterator Function ClipScale(grayscale As Double(), tabulate_bin As Double()) As IEnumerable(Of Byte)
            Dim min As Double = tabulate_bin(0)
            Dim max As Double = tabulate_bin(1)
            Dim scale As New DoubleRange(tabulate_bin)
            Dim bytes As New DoubleRange(0, 255)

            For Each pixel As Byte In grayscale
                If pixel < min Then
                    pixel = min
                ElseIf pixel > max Then
                    pixel = max
                End If

                pixel = scale.ScaleMapping(pixel, bytes)

                Yield CByte(pixel)
            Next
        End Function

        <Extension>
        Public Function AdjustGrayscale(grayscale As BitmapBuffer,
                                        Optional wr As Single = 0.3,
                                        Optional wg As Single = 0.59,
                                        Optional wb As Single = 0.11) As BitmapBuffer

            Dim heatmap As Double() = grayscale _
                .GetPixelsAll _
                .Select(Function(c) CDbl(BitmapScale.GrayScale(c.R, c.G, c.B, wr, wg, wb))) _
                .ToArray
            Dim bin As Double() = heatmap.TabulateBin(topBin:=False)
            Dim scales As Byte() = heatmap.ClipScale(bin).ToArray
            Dim raster As Color(,) = scales _
                .Select(Function(si) Color.FromArgb(si, si, si)) _
                .Split(grayscale.Width) _
                .ToMatrix

            Return New BitmapBuffer(raster, grayscale.Size)
        End Function

    End Module
End Namespace