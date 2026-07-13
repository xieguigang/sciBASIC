#Region "Microsoft.VisualBasic::b6c7d8e9f0011223344556677889900, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\BasisNoise.vb"

    ' Author:
    ' 
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed

    ' The stochastic basis functions:
    ' GaussianNoise and UniformNoise.

#End Region

Imports RE = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Source.Generators

    ''' <summary>
    ''' Gaussian (white) noise: independent N(Offset, |Amp|) samples at every point.
    ''' </summary>
    ''' <remarks>
    ''' Use it to add realistic measurement noise, e.g.
    ''' Signal = Trend + Seasonality + GaussianNoise.
    ''' </remarks>
    Public Class GaussianNoise : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            ' Amp carries the standard deviation here
            Return RE.seeds.NextGaussian(0, stdAbs(Amp))
        End Function

        <System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)>
        Private Shared Function stdAbs(x As Double) As Double
            Return If(x < 0, -x, x)
        End Function
    End Class

    ''' <summary>
    ''' uniform noise: independent samples uniformly distributed in
    ''' [Offset, Offset + Amp] (or [Offset + Amp, Offset] if Amp &lt; 0).
    ''' </summary>
    ''' <remarks>Useful for quantization / rounding style noise.</remarks>
    Public Class UniformNoise : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            Return RE.seeds.NextDouble(0, Amp)
        End Function
    End Class
End Namespace
