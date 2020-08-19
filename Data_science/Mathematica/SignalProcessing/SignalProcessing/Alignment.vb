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