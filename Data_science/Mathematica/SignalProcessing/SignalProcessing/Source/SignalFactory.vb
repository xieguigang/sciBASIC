#Region "Microsoft.VisualBasic::d8e9f00112233445566778899001122, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\SignalFactory.vb"

    ' Author:
    ' 
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed

    ' Convenience factory for basis functions and composite presets
    ' (ECG, mechanical vibration, meteorological temperature).

#End Region

Imports std = System.Math

Namespace Source.Generators

    ''' <summary>
    ''' fluent parameter helpers for any <see cref="BasisFunction"/>.
    ''' </summary>
    ''' <remarks>
    ''' These return the same object so calls can be chained, e.g.
    ''' <c>New Sine().Amplified(2).Scaled(5).At(1)</c>.
    ''' </remarks>
    Public Module BasisExtensions

        <System.Runtime.CompilerServices.Extension>
        Public Function Amplified(Of T As BasisFunction)(f As T, amp As Double) As T
            f.Amp = amp
            Return f
        End Function

        <System.Runtime.CompilerServices.Extension>
        Public Function At(Of T As BasisFunction)(f As T, center As Double) As T
            f.Center = center
            Return f
        End Function

        <System.Runtime.CompilerServices.Extension>
        Public Function Scaled(Of T As BasisFunction)(f As T, scale As Double) As T
            f.Scale = scale
            Return f
        End Function

        <System.Runtime.CompilerServices.Extension>
        Public Function Shifted(Of T As BasisFunction)(f As T, offset As Double) As T
            f.Offset = offset
            Return f
        End Function
    End Module

    ''' <summary>
    ''' factory methods to build each basis function with the four universal
    ''' parameters in a single call.
    ''' </summary>
    Public Module Basis

        Public Function Sine(amp#, center#, scale#, Optional offset# = 0) As Sine
            Return New Sine With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function Cosine(amp#, center#, scale#, Optional offset# = 0) As Cosine
            Return New Cosine With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function Square(amp#, center#, scale#, Optional offset# = 0) As Square
            Return New Square With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function Triangle(amp#, center#, scale#, Optional offset# = 0) As Triangle
            Return New Triangle With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function Sawtooth(amp#, center#, scale#, Optional offset# = 0) As Sawtooth
            Return New Sawtooth With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function DampedSine(amp#, center#, period#, damping#, Optional offset# = 0) As DampedSine
            Return New DampedSine With {.Amp = amp, .Center = center, .Scale = period, .Damping = damping, .Offset = offset}
        End Function

        Public Function Gaussian(amp#, center#, sigma#, Optional offset# = 0) As Gaussian
            Return New Gaussian With {.Amp = amp, .Center = center, .Scale = sigma, .Offset = offset}
        End Function

        Public Function Lorentz(amp#, center#, scale#, Optional offset# = 0) As Lorentz
            Return New Lorentz With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function DoubleExp(amp#, center#, scale#, Optional offset# = 0) As DoubleExp
            Return New DoubleExp With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function Ricker(amp#, center#, scale#, Optional offset# = 0) As Ricker
            Return New Ricker With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function RectPulse(amp#, center#, width#, Optional scale# = 1, Optional offset# = 0) As RectPulse
            Return New RectPulse With {.Amp = amp, .Center = center, .Width = width, .Scale = scale, .Offset = offset}
        End Function

        Public Function Linear(amp#, center#, scale#, Optional offset# = 0) As Linear
            Return New Linear With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function Exponential(amp#, center#, rate#, Optional scale# = 1, Optional offset# = 0) As Exponential
            Return New Exponential With {.Amp = amp, .Center = center, .Rate = rate, .Scale = scale, .Offset = offset}
        End Function

        Public Function Logarithm(amp#, center#, scale#, Optional offset# = 0) As Logarithm
            Return New Logarithm With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function Power(amp#, center#, exponent#, Optional scale# = 1, Optional offset# = 0) As Power
            Return New Power With {.Amp = amp, .Center = center, .Exponent = exponent, .Scale = scale, .Offset = offset}
        End Function

        Public Function Tanh(amp#, center#, scale#, Optional offset# = 0) As Tanh
            Return New Tanh With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function ReLU(amp#, center#, Optional cut# = 0, Optional scale# = 1, Optional offset# = 0) As ReLU
            Return New ReLU With {.Amp = amp, .Center = center, .Cut = cut, .Scale = scale, .Offset = offset}
        End Function

        Public Function [Step](amp#, center#, Optional scale# = 0, Optional offset# = 0) As [Step]
            Return New [Step] With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function LogNormal(amp#, center#, sigma#, Optional offset# = 0) As LogNormal
            Return New LogNormal With {.Amp = amp, .Center = center, .Scale = sigma, .Offset = offset}
        End Function

        Public Function Sinc(amp#, center#, scale#, Optional offset# = 0) As Sinc
            Return New Sinc With {.Amp = amp, .Center = center, .Scale = scale, .Offset = offset}
        End Function

        Public Function Gompertz(amp#, center#, rate#, Optional scale# = 1, Optional offset# = 0) As Gompertz
            Return New Gompertz With {.Amp = amp, .Center = center, .Rate = rate, .Scale = scale, .Offset = offset}
        End Function

        Public Function GaussianNoise(stddev#, Optional offset# = 0) As GaussianNoise
            Return New GaussianNoise With {.Amp = stddev, .Offset = offset}
        End Function

        Public Function UniformNoise(width#, Optional offset# = 0) As UniformNoise
            Return New UniformNoise With {.Amp = width, .Offset = offset}
        End Function
    End Module

    ''' <summary>
    ''' ready-to-use composite presets that demonstrate how basis functions
    ''' combine into realistic signals.
    ''' </summary>
    Public Module Presets

        ''' <summary>
        ''' a single-lead ECG-like beat synthesized from Gaussian bumps
        ''' (P wave, Q, R spike, S, T wave) plus a small baseline wander and noise.
        ''' </summary>
        ''' <param name="period">the RR interval (beat length) in seconds.</param>
        ''' <param name="noise">standard deviation of the measurement noise.</param>
        ''' <returns>a <see cref="SignalGenerator"/> producing one beat shape</returns>
        Public Function ECG(Optional period# = 1.0, Optional noise# = 0.02) As SignalGenerator
            Dim p = period
            Dim gen = New SignalGenerator()

            ' P wave
            gen.Add(Basis.Gaussian(0.12, 0.15 * p, 0.03 * p))
            ' Q dip
            gen.Add(Basis.Gaussian(-0.1, 0.25 * p, 0.012 * p))
            ' R spike
            gen.Add(Basis.Gaussian(1.0, 0.28 * p, 0.012 * p))
            ' S dip
            gen.Add(Basis.Gaussian(-0.18, 0.31 * p, 0.015 * p))
            ' T wave
            gen.Add(Basis.Gaussian(0.25, 0.45 * p, 0.05 * p))
            ' baseline wander + noise
            gen.Add(Basis.Sine(0.03, 0, 4 * p))
            gen.Add(Basis.GaussianNoise(noise))

            Return gen
        End Function

        ''' <summary>
        ''' mechanical vibration: a damped sinusoid (impulse response) convolved
        ''' with a train of impact impulses, plus measurement noise.
        ''' </summary>
        ''' <param name="duration">total time in seconds.</param>
        ''' <param name="noise">standard deviation of the measurement noise.</param>
        Public Function Vibration(Optional duration# = 10.0, Optional noise# = 0.05) As SignalGenerator
            Dim impacts = New SignalGenerator()

            For i As Integer = 1 To CInt(duration)
                impacts.Add(Basis.RectPulse(1.0, i, 0.02))
            Next

            Dim impulseResponse = Basis.DampedSine(1.0, 0, 0.2, 6.0)

            Dim conv = New Convolution(impacts, impulseResponse, 0, duration, 2000)

            Return New SignalGenerator() _
                .Add(conv) _
                .Add(Basis.GaussianNoise(noise))
        End Function

        ''' <summary>
        ''' daily temperature-like curve: a slow seasonal sinusoid, a gentle
        ''' linear warming trend, diurnal variation and weather noise.
        ''' </summary>
        ''' <param name="days">number of days to simulate.</param>
        Public Function Weather(Optional days# = 365.0) As SignalGenerator
            Return New SignalGenerator() _
                .Add(Basis.Sine(amp:=10, center:=0, scale:=days, offset:=15)) _
                .Add(Basis.Sine(amp:=5, center:=0, scale:=1, offset:=0)) _
                .Add(Basis.Linear(amp:=0.005, center:=0, scale:=1)) _
                .Add(Basis.GaussianNoise(0.5))
        End Function
    End Module
End Namespace
