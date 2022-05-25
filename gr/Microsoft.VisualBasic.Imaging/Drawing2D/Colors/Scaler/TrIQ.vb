#Region "Microsoft.VisualBasic::c333a1b747acdeb1a376bf27cbdc3ad5, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Scaler\TrIQ.vb"

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

    '   Total Lines: 177
    '    Code Lines: 77
    ' Comment Lines: 76
    '   Blank Lines: 24
    '     File Size: 6.37 KB


    '     Module TrIQ
    ' 
    '         Function: CDF, DiscreteLevels, (+2 Overloads) FindThreshold, GetTrIQRange
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports stdNum = System.Math

Namespace Drawing2D.Colors.Scaler

    ''' <summary>
    ''' Contrast optimization of mass spectrometry imaging (MSI) data visualization by threshold intensity quantization (TrIQ)
    ''' </summary>
    Public Module TrIQ

        ''' <summary>
        ''' T computation involves the cumulative distributive
        ''' function p(k)(CDF), defined As
        ''' 
        ''' ```
        ''' q ~ p(k) = sum(h(i)) / N
        ''' ```
        '''
        ''' h(i) stands For the i bin's frequency within an image 
        ''' histogram, N is the image’s pixel count. Given a target 
        ''' probability q it Is possible to find the bin k whose 
        ''' CDF closely resembles q. Then, the upper limit Of the 
        ''' bin k In h will be used As the threshold value T.
        ''' </summary>
        ''' <param name="bin"></param>
        ''' <param name="N"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CDF(bin As IEnumerable(Of DataBinBox(Of Double)), N As Integer) As Double
            Dim sumHk As Double = Aggregate hi As DataBinBox(Of Double) In bin Into Sum(hi.Count)
            Dim p As Double = sumHk / N

            Return p
        End Function

        ''' <summary>
        ''' The Threshold Intensity Quantization addresses this issue by setting a new upper limit T
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="q"></param>
        ''' <param name="N"></param>
        ''' <param name="eps"></param>
        ''' <returns>the upper bound raw value of the threshold</returns>
        <Extension>
        Public Function FindThreshold(data As IEnumerable(Of Double), q As Double,
                                      Optional N As Integer = 100,
                                      Optional eps As Double = 0.1) As Double

            Return CutBins _
                .FixedWidthBins(data, N, Function(x) x) _
                .FindThreshold(q, eps)
        End Function

        ''' <summary>
        ''' get the best value range for level scaler via TrIQ algorithm. 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="q"></param>
        ''' <param name="N"></param>
        ''' <param name="eps"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetTrIQRange(data As IEnumerable(Of Double), q As Double,
                                     Optional N As Integer = 100,
                                     Optional eps As Double = 0.1) As DoubleRange

            Dim raw As Double() = data.SafeQuery.ToArray

            If raw.Length = 0 Then
                Return New DoubleRange(0, 0)
            End If

            Dim max As Double = raw.FindThreshold(q, N, eps)
            Dim range As New DoubleRange(raw.Min, max)

            Return range
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="q"></param>
        ''' <param name="eps"></param>
        ''' <returns>
        ''' the upper bound raw value of the threshold
        ''' </returns>
        <Extension>
        Public Function FindThreshold(data As IEnumerable(Of DataBinBox(Of Double)), q As Double, Optional eps As Double = 0.1) As Double
            Dim sample As DataBinBox(Of Double)() = data.OrderBy(Function(b) b.Boundary.Min).ToArray
            Dim N As Integer = Aggregate point As DataBinBox(Of Double)
                               In sample
                               Into Sum(point.Count)
            Dim minK As Integer = 1
            Dim minD As Double = Double.MaxValue

            If sample.Length = 0 Then
                Return 0
            End If

            For k As Integer = 1 To sample.Length - 1
                Dim cdf As Double = sample.Take(k).CDF(N)

                If cdf > q Then
                    Exit For
                End If

                Dim d As Double = stdNum.Abs(cdf - q)

                If d <= eps Then
                    Return sample(k).Boundary.Min
                ElseIf d < minD Then
                    minD = d
                    minK = k
                End If
            Next

            Return sample(minK - 1).Boundary.Max
        End Function

        ''' <summary>
        ''' Quantization is a process for mapping a range 
        ''' of analog intensity values To a Single discrete 
        ''' value, known As a gray level.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="n"></param>
        ''' <param name="T"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Quantization is a process For mapping a range of 
        ''' analog intensity values To a Single discrete value, 
        ''' known As a gray level. Zero-memory Is a widely 
        ''' used quantization method. The zero-memory quantizer
        ''' computes equally spaced intensity bins Of width w:
        ''' 
        ''' ```
        ''' w = (max(f) - min(f)) / n
        ''' ```
        ''' 
        ''' where n represents the number Of discrete values, 
        ''' usually 256; min(f) And max(f) operators provide 
        ''' minimum And maximum intensity values. Quantization 
        ''' Is based On a comparison with the transition levels 
        ''' tk:
        ''' 
        ''' ```
        ''' tk = w + min(f), 2w + min(f), ..., nw + min(f)
        ''' ```
        ''' 
        ''' Finally, the discrete mapped value Q Is obtained:
        ''' 
        ''' ```
        ''' Q(f(x, y)) = {
        '''     
        '''     0,  f(x,y) &lt;= t1
        '''     k,  tk &lt; f(x,y) &lt; tk+1
        ''' }
        ''' ```
        ''' </remarks>
        <Extension>
        Public Function DiscreteLevels(data As IEnumerable(Of Double),
                                       Optional n As Integer = 30,
                                       Optional T As Double? = Nothing) As IEnumerable(Of Integer)

            Dim f As Double() = data.ToArray
            Dim minf As Double = f.Min

            If T Is Nothing Then
                T = f.Max
            End If

            Dim levelRange As New DoubleRange(0, n)
            Dim scaler As New DoubleRange(minf, T)

            Return From w As Double
                   In f
                   Let q As Double = If(w >= T, n - 1, scaler.ScaleMapping(w, levelRange))
                   Select CInt(q)
        End Function

    End Module
End Namespace
