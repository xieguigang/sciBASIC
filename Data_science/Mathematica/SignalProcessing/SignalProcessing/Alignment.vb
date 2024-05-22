#Region "Microsoft.VisualBasic::abd7528f9c1272b43b521c3544efa8ba, Data_science\Mathematica\SignalProcessing\SignalProcessing\Alignment.vb"

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

    '   Total Lines: 83
    '    Code Lines: 64 (77.11%)
    ' Comment Lines: 9 (10.84%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 10 (12.05%)
    '     File Size: 3.37 KB


    ' Module Alignment
    ' 
    '     Function: Join, Normalize, Similarity
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions
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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="signals"></param>
    ''' <param name="steps#"></param>
    ''' <param name="maxgenerality">
    ''' max generality or max features when union these signal points
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function Join(signals As IEnumerable(Of GeneralSignal),
                         Optional steps# = 0.25,
                         Optional maxgenerality As Boolean = False) As GeneralSignal

        Dim resampling As Resampler() = signals.Select(Function(sig) Resampler.CreateSampler(sig, max_dx:=steps * 5)).ToArray
        Dim measures As Double() = resampling _
            .Select(Function(sample) sample.enumerateMeasures) _
            .IteratesALL _
            .ToArray
        Dim xmin As Double = measures.Min
        Dim xmax As Double = measures.Max
        Dim resample As Double() = seq(xmin, xmax, steps).ToArray
        Dim signalsample As Double() = resample _
            .Select(Function(xi)
                        Dim sigInto As Double() = resampling _
                            .Select(Function(a) a.GetIntensity(xi)) _
                            .ToArray

                        If maxgenerality Then
                            Return sigInto.Average
                        Else
                            Return sigInto.TabulateMode
                        End If
                    End Function) _
            .ToArray

        Return New GeneralSignal With {
            .Measures = resample,
            .Strength = signalsample,
            .description = If(maxgenerality, "max_generality", "max_features")
        }
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
