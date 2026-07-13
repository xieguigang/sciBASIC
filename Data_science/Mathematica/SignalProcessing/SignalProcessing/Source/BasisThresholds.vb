#Region "Microsoft.VisualBasic::25e9841c195edbd0c02e606a46d89ad9, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\BasisThresholds.vb"

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

    '   Total Lines: 56
    '    Code Lines: 22 (39.29%)
    ' Comment Lines: 26 (46.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (14.29%)
    '     File Size: 2.06 KB


    '     Class Tanh
    ' 
    '         Function: Shape
    ' 
    '     Class ReLU
    ' 
    '         Properties: Cut
    ' 
    '         Function: Shape
    ' 
    '     Class [Step]
    ' 
    '         Function: Shape
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Source.Generators

    ''' <summary>
    ''' hyperbolic tangent: Amp * tanh((x - Center) / Scale) + Offset.
    ''' </summary>
    ''' <remarks>
    ''' Like sigmoid but in [-1, 1] and steeper around zero. Use it instead of
    ''' sigmoid when the signal must switch symmetrically between positive and
    ''' negative states.
    ''' </remarks>
    Public Class Tanh : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            Return std.Tanh(u)
        End Function
    End Class

    ''' <summary>
    ''' rectified linear unit: Amp * max(0, (x - Center) / Scale - Cut) + Offset.
    ''' </summary>
    ''' <remarks>
    ''' Models a "threshold trigger": the output only grows linearly once the
    ''' (shifted) input exceeds the <see cref="Cut"/> threshold.
    ''' </remarks>
    Public Class ReLU : Inherits BasisFunction

        ''' <summary>the threshold offset c (in normalized units).</summary>
        Public Property Cut As Double = 0.0

        Protected Overrides Function Shape(u As Double) As Double
            Return std.Max(0, u - Cut)
        End Function
    End Class

    ''' <summary>
    ''' step function: the limit of a sigmoid. 0 before <see cref="BasisFunction.Center"/>,
    ''' 1 afterwards, with a transition sharpness controlled by <see cref="BasisFunction.Scale"/>.
    ''' </summary>
    ''' <remarks>
    ''' Models an instantaneous state change (fuse blowing, threshold alarm).
    ''' When <see cref="BasisFunction.Scale"/> &gt; 0 it uses a logistic sigmoid
    ''' for a smooth-but-sharp transition; when Scale = 0 it becomes an exact
    ''' Heaviside step.
    ''' </remarks>
    Public Class [Step] : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            If Scale <= 0 Then
                Return If(u >= 0, 1.0, 0.0)
            End If
            Return 1 / (1 + std.Exp(-u))
        End Function
    End Class
End Namespace
