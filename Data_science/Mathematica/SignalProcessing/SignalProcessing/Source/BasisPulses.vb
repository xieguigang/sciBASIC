#Region "Microsoft.VisualBasic::f20e183087d8d0bdc02c2bc8d40b8bfd, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\BasisPulses.vb"

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

    '   Total Lines: 87
    '    Code Lines: 35 (40.23%)
    ' Comment Lines: 40 (45.98%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 12 (13.79%)
    '     File Size: 3.20 KB


    '     Class Gaussian
    ' 
    '         Function: Shape
    ' 
    '     Class Lorentz
    ' 
    '         Function: Shape
    ' 
    '     Class DoubleExp
    ' 
    '         Function: Shape
    ' 
    '     Class Ricker
    ' 
    '         Function: Shape
    ' 
    '     Class RectPulse
    ' 
    '         Properties: Width
    ' 
    '         Function: Shape
    ' 
    ' 
    ' /********************************************************************************/

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
