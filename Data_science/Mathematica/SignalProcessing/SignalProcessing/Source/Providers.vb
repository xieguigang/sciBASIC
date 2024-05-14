#Region "Microsoft.VisualBasic::869b5c6e372cba1fb5eb17fbdf4ee9e5, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\Providers.vb"

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

    '   Total Lines: 61
    '    Code Lines: 46
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 2.22 KB


    '     Class OneSignal
    ' 
    '         Function: calculate
    ' 
    '     Class RectangularSignal
    ' 
    '         Function: calculate
    ' 
    '     Class SawtoothSignal
    ' 
    '         Function: calculate
    ' 
    '     Class SinusSignal
    ' 
    '         Function: calculate
    ' 
    '     Class SiSignal
    ' 
    '         Function: calculate
    ' 
    '     Class TriangularSignal
    ' 
    '         Function: calculate
    ' 
    '     Class ZeroSignal
    ' 
    '         Function: calculate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Source

    Public Class OneSignal : Inherits Signal

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq As Double, phase As Double) As Double
            Return 1
        End Function
    End Class

    Public Class RectangularSignal : Inherits Signal

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq As Double, phase As Double) As Double
            Return std.Sign(std.Sin(freq + phase))
        End Function
    End Class

    Public Class SawtoothSignal : Inherits Signal

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq As Double, phase As Double) As Double
            Return 2 * ((freq + phase) / (2 * std.PI) - std.Floor(0.5 + (freq + phase) / (2 * std.PI)))
        End Function
    End Class

    Public Class SinusSignal : Inherits Signal

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq As Double, phase As Double) As Double
            Return std.Sin(freq + phase)
        End Function
    End Class

    Public Class SiSignal : Inherits Signal

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq As Double, phase As Double) As Double
            Return std.Sin(2 * std.PI * (freq - phase)) / (2 * std.PI * (freq - phase))
        End Function
    End Class

    Public Class TriangularSignal : Inherits Signal

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq As Double, phase As Double) As Double
            Return 2 * std.Abs(2 * ((freq + phase) / (2 * std.PI) - std.Floor((freq + phase) / (2 * std.PI) + 0.5))) - 1
        End Function
    End Class

    Public Class ZeroSignal : Inherits Signal

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq As Double, phase As Double) As Double
            Return 0
        End Function
    End Class
End Namespace
