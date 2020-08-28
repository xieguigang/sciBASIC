#Region "Microsoft.VisualBasic::71be6a1d9635f9bce17a8fb89ef5d45c, Data_science\Mathematica\SignalProcessing\SignalProcessing\Alignment.vb"

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

    ' Module Alignment
    ' 
    '     Function: Normalize, Similarity
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports stdNum = System.Math

Public Module Alignment

    Public Function Similarity(ref As GeneralSignal, sample As GeneralSignal, Optional steps# = 0.25) As Double
        Dim polyref = Resampler.CreateSampler(ref)
        Dim polySample = Resampler.CreateSampler(sample)
        Dim x As Double() = ref.Measures.JoinIterates(sample.Measures).ToArray
        Dim xmin As Double = x.Min
        Dim xmax As Double = x.Max
        Dim resample As Double() = seq(xmin, xmax, steps).ToArray
        Dim signalref As Vector = resample.Select(Function(xi) polyref.GetIntensity(xi)).AsVector
        Dim signalsample As Vector = resample.Select(Function(xi) polySample.GetIntensity(xi)).AsVector
        Dim variants As Vector = Vector.Abs((signalref - signalsample) / signalref) _
            .Where(Function(vi) Not vi.IsNaNImaginary) _
            .Select(Function(vi) If(vi > 1, 1, vi)) _
            .AsVector
        Dim score As Double = 1 - variants.Average

        Return score
    End Function

    <Extension>
    Public Function Normalize(signal As GeneralSignal) As GeneralSignal
        If signal.Strength.Any(Function(vi) vi < 0) Then
            Dim minY As Double = stdNum.Abs(signal.Strength.Min)
            Dim norm As Double() = signal.Strength _
                .Select(Function(v) v + minY) _
                .ToArray

            signal.Strength = norm
        End If

        Return signal
    End Function
End Module
