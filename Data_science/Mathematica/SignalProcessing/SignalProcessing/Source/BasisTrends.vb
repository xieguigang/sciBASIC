#Region "Microsoft.VisualBasic::418f9e19c45ed5e02862fec5b41b8410, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\BasisTrends.vb"

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

    '   Total Lines: 70
    '    Code Lines: 31 (44.29%)
    ' Comment Lines: 28 (40.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (15.71%)
    '     File Size: 2.41 KB


    '     Class Linear
    ' 
    '         Function: Shape
    ' 
    '     Class Exponential
    ' 
    '         Properties: Rate
    ' 
    '         Function: Shape
    ' 
    '     Class Logarithm
    ' 
    '         Function: Shape
    ' 
    '     Class Power
    ' 
    '         Properties: Exponent
    ' 
    '         Function: Shape
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Source.Generators

    ''' <summary>
    ''' linear trend: Amp * (x - Center) / Scale + Offset.
    ''' </summary>
    ''' <remarks>The simplest constant growth / decline.</remarks>
    Public Class Linear : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            Return u
        End Function
    End Class

    ''' <summary>
    ''' exponential growth / decay: Amp * e^(alpha (x - Center) / Scale) + Offset.
    ''' </summary>
    ''' <remarks>
    ''' With a positive <see cref="Rate"/> it models unbounded population growth,
    ''' radioactive decay or chemical concentration; with a negative rate it decays.
    ''' </remarks>
    Public Class Exponential : Inherits BasisFunction

        ''' <summary>the growth / decay rate (alpha).</summary>
        Public Property Rate As Double = 1.0

        Protected Overrides Function Shape(u As Double) As Double
            Return std.Exp(Rate * u)
        End Function
    End Class

    ''' <summary>
    ''' logarithmic trend: Amp * ln((x - Center) / Scale) + Offset.
    ''' </summary>
    ''' <remarks>
    ''' Models diminishing returns, learning curves or fast-then-flat evolution.
    ''' Returns <see cref="BasisFunction.Offset"/> for x &lt;= Center to avoid NaN.
    ''' </remarks>
    Public Class Logarithm : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            If u <= 0 Then
                Return 0.0
            End If
            Return std.Log(u)
        End Function
    End Class

    ''' <summary>
    ''' power-law trend: Amp * ((x - Center) / Scale)^alpha + Offset.
    ''' </summary>
    ''' <remarks>
    ''' 0 &lt; alpha &lt; 1 behaves like a log curve; alpha &gt; 1 behaves like
    ''' exponential growth. Extremely common in nature (fluid flow, fractals).
    ''' For fractional alpha the domain is restricted to x &gt;= Center.
    ''' </remarks>
    Public Class Power : Inherits BasisFunction

        ''' <summary>the exponent (alpha).</summary>
        Public Property Exponent As Double = 1.0

        Protected Overrides Function Shape(u As Double) As Double
            If u <= 0 Then
                Return 0.0
            End If
            Return std.Pow(u, Exponent)
        End Function
    End Class
End Namespace
