#Region "Microsoft.VisualBasic::6c7a65e4a824fa9a33739f564d7e3221, gr\Microsoft.VisualBasic.Imaging\Filters\TabulateClipScaler.vb"

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

'   Total Lines: 126
'    Code Lines: 96 (76.19%)
' Comment Lines: 11 (8.73%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 19 (15.08%)
'     File Size: 5.27 KB


'     Module TabulateClipScaler
' 
'         Function: AdjustGrayscale, ClipScale, GlobalTileScales
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

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
        ''' <returns>
        ''' grayscale image data set
        ''' </returns>
        <Extension>
        Public Iterator Function GlobalTileScales(tiles As IEnumerable(Of BitmapBuffer),
                                                  Optional wr As Single = 0.3,
                                                  Optional wg As Single = 0.59,
                                                  Optional wb As Single = 0.11) As IEnumerable(Of BitmapBuffer)

            Dim pull As BitmapBuffer() = tiles.ToArray
            Dim heatmap As New BucketSet(Of Integer)

            Call "processing the global grayscale image...".info

            ' convert color pixel to grayscale pixel
            For Each tile As BitmapBuffer In TqdmWrapper.Wrap(pull)
                Call heatmap.Add(From pixel As Color
                                 In tile.GetPixelsAll
                                 Select BitmapScale.GrayScale(
                                     pixel.R, pixel.G, pixel.B,
                                     wr, wg, wb)
                                 )
            Next

            Call "create global heatmap bins".info
            
            Dim hist As IntegerTagged(Of Integer())() = heatmap.ForEachBucket _
                .AsParallel _
                .Select(Function(tile)
                            ' 255/5 = 51
                            ' split into 6 bins
                            Return tile.GroupBy(Function(b) CInt(b / 20)).ToArray
                        End Function) _
                .ToArray _
                .IteratesALL _
                .GroupBy(Function(a) a.Key) _
                .Select(Function(a) New IntegerTagged(Of Integer())(a.Key, a.IteratesALL.ToArray)) _
                .OrderBy(Function(a) a.Tag) _
                .ToArray
            Dim maxN As Integer = which.Max(hist.Select(Function(a) a.Value.Length))
            Dim resample As New DoubleRange(hist.TakeBags(maxN, nbags:=5).IteratesALL)
            Dim i As i32 = 0
            Dim bin As Double() = resample.MinMax

            For Each grayscale As Integer() In TqdmWrapper.Wrap(heatmap.ForEachBucket.ToArray)            
                Dim tile As BitmapBuffer = pull(++i)
                Dim scales As Byte() = grayscale.AsDouble.ClipScale(bin).ToArray
                Dim raster As Color(,) = scales _
                    .Select(Function(si) Color.FromArgb(si, si, si)) _
                    .Split(tile.Width) _
                    .ToMatrix

                Yield New BitmapBuffer(raster, tile.Size)

                Erase scales
                Erase raster
            Next
        End Function

        <Extension>
        Private Iterator Function TakeBags(hist As IntegerTagged(Of Integer())(), maxN As Integer, nbags As Integer) As IEnumerable(Of Integer())
            If maxN <= nbags Then
                For i As Integer = 0 To std.min(nbags, hist.Length) - 1
                    Yield hist(i).Value
                Next
            ElseIf maxN >= hist.Length - nbags Then
                For i As Integer = hist.Length - 1 To std.Max(hist.Length - nbags, 0) Step -1
                    Yield hist(i).Value
                Next
            Else
                Dim start As Integer = maxN - nbags \ 2
                Dim ends As Integer = maxN + nbags \ 2

                For i As Integer = std.Max(0, start) To std.Min(hist.Length - 1, ends)
                    Yield hist(i).Value
                Next
            End If
        End Function

    End Module
End Namespace
