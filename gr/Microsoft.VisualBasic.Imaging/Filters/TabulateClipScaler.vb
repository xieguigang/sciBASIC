Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Scripting.Runtime

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

        ''' <summary>
        ''' A help function for better processing of the dzi image
        ''' </summary>
        ''' <param name="tiles"></param>
        ''' <param name="wr"></param>
        ''' <param name="wg"></param>
        ''' <param name="wb"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function GlobalTileScales(tiles As IEnumerable(Of BitmapBuffer),
                                                  Optional wr As Single = 0.3,
                                                  Optional wg As Single = 0.59,
                                                  Optional wb As Single = 0.11) As IEnumerable(Of BitmapBuffer)

            Dim pull As BitmapBuffer() = tiles.ToArray
            Dim heatmap As New BucketSet(Of Integer)

            For Each tile As BitmapBuffer In pull
                Call heatmap.Add(From pixel As Color
                                 In tile.GetPixelsAll
                                 Select BitmapScale.GrayScale(
                                     pixel.R, pixel.G, pixel.B,
                                     wr, wg, wb)
                                 )
            Next

            Dim hist = heatmap.GroupBy(Function(b) CInt(b / 20)).OrderBy(Function(a) a.Key).ToArray
            Dim maxN As Integer = which.Max(hist.Select(Function(a) a.Count))
            Dim resample As DoubleRange

            If maxN = 0 Then
                resample = New DoubleRange(vector:=hist(Scan0).AsList + hist(1).AsEnumerable)
            ElseIf maxN = hist.Length - 1 Then
                resample = New DoubleRange(vector:=hist(hist.Length - 1).AsList + hist(hist.Length - 2).AsEnumerable)
            Else
                resample = New DoubleRange(vector:=hist(maxN - 1).AsList + hist(maxN).AsEnumerable + hist(maxN + 1).AsEnumerable)
            End If

            Dim i As Integer = 0
            Dim bin As Double() = resample.MinMax

            For Each grayscale As Integer() In heatmap.ForEachBucket
                Dim tile As BitmapBuffer = pull(i)
                Dim scales As Byte() = grayscale.AsDouble.ClipScale(bin).ToArray
                Dim raster As Color(,) = scales _
                    .Select(Function(si) Color.FromArgb(si, si, si)) _
                    .Split(tile.Width) _
                    .ToMatrix

                Yield New BitmapBuffer(raster, tile.Size)
            Next
        End Function

    End Module
End Namespace