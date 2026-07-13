#Region "Microsoft.VisualBasic::7d4ef0269ad060a76edfd53c9fd9969f, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\BasisNoise.vb"

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

    '   Total Lines: 36
    '    Code Lines: 17 (47.22%)
    ' Comment Lines: 13 (36.11%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 6 (16.67%)
    '     File Size: 1.38 KB


    '     Class GaussianNoise
    ' 
    '         Function: Shape, stdAbs
    ' 
    '     Class UniformNoise
    ' 
    '         Function: Shape
    ' 
    ' 
    ' /********************************************************************************/

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
