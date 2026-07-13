#Region "Microsoft.VisualBasic::d2e3f4a5b6c7d80910203e4f5a6b7c8d, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\BasisPulses.vb"

    ' Author:
    ' 
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed

    ' The pulse & local-feature basis functions:
    ' Gaussian, Lorentz, DoubleExp, Ricker and RectPulse.

#End Region

Imports GaussianFunc = Microsoft.VisualBasic.Math.Distributions.Gaussian
Imports std = System.Math

Namespace Source.Generators

    ''' <summary>
    ''' Gaussian (normal) peak: a smooth localized bump.
    ''' </summary>
    ''' <remarks>
    ''' Reuses <see cref="GaussianFunc"/> from the math library.
    ''' Center = mu, Scale = sigma, Amp = height.
    ''' </remarks>
    Public Class Gaussian : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            ' GaussianFunc is defined as A * exp(-(x-mu)^2 / (2 sigma^2))
            ' here x = Center + Scale * u, mu = Center => (x-mu) = Scale * u
            ' so the shape with height 1 is exp(-u^2 / 2)
            Return std.Exp(-(u * u) / 2)
        End Function
    End Class

    ''' <summary>
    ''' Lorentzian / Cauchy peak: similar to a Gaussian but with much heavier
    ''' (slower decaying) tails.
    ''' </summary>
    ''' <remarks>
    ''' Formula: 1 / (1 + ((x - Center) / Scale)^2). Commonly used in
    ''' spectroscopy and for modeling long-tail突发事件.
    ''' </remarks>
    Public Class Lorentz : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            Return 1 / (1 + u * u)
        End Function
    End Class

    ''' <summary>
    ''' double-sided exponential: e^(-|x - Center| / Scale).
    ''' </summary>
    ''' <remarks>
    ''' Forms a sharp peak at the center and decays exponentially to both
    ''' sides; models instant impact energy release.
    ''' </remarks>
    Public Class DoubleExp : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            Return std.Exp(-std.Abs(u))
        End Function
    End Class

    ''' <summary>
    ''' Ricker wavelet / Mexican hat: a positive lobe flanked by two negative
    ''' side-lobes.
    ''' </summary>
    ''' <remarks>
    ''' A classic local-feature kernel in signal processing, radar and seismic
    ''' exploration. Reuses <see cref="MathUtils.MexicanHatWavelet"/>.
    ''' </remarks>
    Public Class Ricker : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            ' MexicanHatWavelet expects (x, center, sigma); sigma here = Scale
            ' so (x - center) / sigma = u
            Return MathUtils.MexicanHatWavelet(Center + Scale * u, Center, Scale)
        End Function
    End Class

    ''' <summary>
    ''' rectangular pulse: constant Amp inside [Center, Center + Width], 0 elsewhere.
    ''' </summary>
    ''' <remarks>Models a device turning on and off over a fixed interval.</remarks>
    Public Class RectPulse : Inherits BasisFunction

        ''' <summary>the duration of the pulse (in the same unit as x).</summary>
        Public Property Width As Double = 1.0

        Protected Overrides Function Shape(u As Double) As Double
            Dim x = Center + Scale * u
            If x >= Center AndAlso x <= Center + Width Then
                Return 1.0
            Else
                Return 0.0
            End If
        End Function
    End Class
End Namespace
