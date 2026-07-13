#Region "Microsoft.VisualBasic::7acc90d9a257c50a4bdad2c8a7553149, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\BasisOscillations.vb"

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

    '   Total Lines: 92
    '    Code Lines: 40 (43.48%)
    ' Comment Lines: 38 (41.30%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (15.22%)
    '     File Size: 3.52 KB


    '     Class Sine
    ' 
    '         Function: Shape
    ' 
    '     Class Cosine
    ' 
    '         Function: Shape
    ' 
    '     Class Square
    ' 
    '         Function: Shape
    ' 
    '     Class Triangle
    ' 
    '         Function: Shape
    ' 
    '     Class Sawtooth
    ' 
    '         Function: Shape
    ' 
    '     Class DampedSine
    ' 
    '         Properties: Damping
    ' 
    '         Function: Shape
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Source.Generators

    ''' <summary>
    ''' sine wave: Amp * sin(2π (x - Center) / Scale) + Offset
    ''' </summary>
    ''' <remarks>
    ''' <see cref="BasisFunction.Scale"/> is interpreted as the period (wavelength)
    ''' of the wave. Combine several sine waves of different periods and phases to
    ''' fit almost any periodic signal (Fourier principle).
    ''' </remarks>
    Public Class Sine : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            Return std.Sin(2 * std.PI * u)
        End Function
    End Class

    ''' <summary>
    ''' cosine wave: Amp * cos(2π (x - Center) / Scale) + Offset
    ''' </summary>
    ''' <remarks><see cref="BasisFunction.Scale"/> is the period.</remarks>
    Public Class Cosine : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            Return std.Cos(2 * std.PI * u)
        End Function
    End Class

    ''' <summary>
    ''' square wave: switches between +Amp and -Amp with a period of <see cref="BasisFunction.Scale"/>.
    ''' </summary>
    ''' <remarks>
    ''' Useful for modeling digital / on-off signals, switching states
    ''' (e.g. awake / asleep) or pulse trains.
    ''' </remarks>
    Public Class Square : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            Dim phase = u - std.Floor(u)
            Return If(phase < 0.5, 1.0, -1.0)
        End Function
    End Class

    ''' <summary>
    ''' triangle wave: linear rise and fall, period = <see cref="BasisFunction.Scale"/>.
    ''' </summary>
    ''' <remarks>Sharper than a sine; common for uniform mechanical motion or sweeps.</remarks>
    Public Class Triangle : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            Dim phase = u - std.Floor(u)
            Return If(phase < 0.5, 4 * phase - 1, 3 - 4 * phase)
        End Function
    End Class

    ''' <summary>
    ''' sawtooth wave: rises linearly then drops instantly, period = <see cref="BasisFunction.Scale"/>.
    ''' </summary>
    ''' <remarks>Models capacitor charge-discharge, single sweeps or gradually
    ''' accumulating-then-bursting physical processes.</remarks>
    Public Class Sawtooth : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            Dim phase = u - std.Floor(u)
            Return 2 * phase - 1
        End Function
    End Class

    ''' <summary>
    ''' damped sine wave: Amp * e^(-alpha (x - Center)) * sin(2π (x - Center) / Scale) + Offset.
    ''' </summary>
    ''' <remarks>
    ''' Extremely important for simulating mechanical vibration after a hit,
    ''' RLC circuit response, seismic waves and any decaying oscillation.
    ''' <see cref="BasisFunction.Scale"/> is the period; <see cref="Damping"/>
    ''' (alpha) controls how fast the oscillation decays.
    ''' </remarks>
    Public Class DampedSine : Inherits BasisFunction

        ''' <summary>the damping coefficient (alpha) of the exponential envelope.</summary>
        Public Property Damping As Double = 1.0

        Protected Overrides Function Shape(u As Double) As Double
            If u < 0 Then
                Return 0.0
            End If
            Return std.Exp(-Damping * u) * std.Sin(2 * std.PI * u)
        End Function
    End Class
End Namespace
