#Region "Microsoft.VisualBasic::0c7bf02345689d988d411df4fe3a26b4, Data_science\Mathematica\Math\Math\Distributions\TrIQ.vb"

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

    '   Total Lines: 146
    '    Code Lines: 57 (39.04%)
    ' Comment Lines: 71 (48.63%)
    '    - Xml Docs: 91.55%
    ' 
    '   Blank Lines: 18 (12.33%)
    '     File Size: 5.45 KB


    '     Module TrIQ
    ' 
    '         Function: CutThreshold, DiscreteLevels, FindThreshold, GetTrIQRange
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions

Namespace Distributions

    ''' <summary>
    ''' Contrast optimization of mass spectrometry imaging (MSI) data visualization by threshold intensity quantization (TrIQ)
    ''' </summary>
    ''' <remarks>
    ''' works based on the <see cref="ECDF"/>.
    ''' </remarks>
    Public Module TrIQ

        ''' <summary>
        ''' trim the head intensity data by a given cutoff threshold 
        ''' which is evaluated via the TrIQ algorithm.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="q"></param>
        ''' <param name="N"></param>
        ''' <param name="eps"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CutThreshold(data As IEnumerable(Of Double), q As Double,
                                     Optional N As Integer = 100,
                                     Optional eps As Double = 0.1) As IEnumerable(Of Double)

            Dim v As Double() = data.ToArray
            Dim cut As Double = v.FindThreshold(q, N, eps)

            Return v _
                .Select(Function(xi)
                            If xi > cut Then
                                Return cut
                            Else
                                Return xi
                            End If
                        End Function)
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

            Return New ECDF(data, N).FindThreshold(q, eps)
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
