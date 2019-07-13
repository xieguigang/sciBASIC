#Region "Microsoft.VisualBasic::b96e26df5b3c28ce33d71ea1609e2b69, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\Providers.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
            Return Math.Sign(Math.Sin(freq + phase))
        End Function
    End Class

    Public Class SawtoothSignal : Inherits Signal

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq As Double, phase As Double) As Double
            Return 2 * ((freq + phase) / (2 * Math.PI) - Math.Floor(0.5 + (freq + phase) / (2 * Math.PI)))
        End Function
    End Class

    Public Class SinusSignal : Inherits Signal

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq As Double, phase As Double) As Double
            Return Math.Sin(freq + phase)
        End Function
    End Class

    Public Class SiSignal : Inherits Signal

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq As Double, phase As Double) As Double
            Return Math.Sin(2 * Math.PI * (freq - phase)) / (2 * Math.PI * (freq - phase))
        End Function
    End Class

    Public Class TriangularSignal : Inherits Signal

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq As Double, phase As Double) As Double
            Return 2 * Math.Abs(2 * ((freq + phase) / (2 * Math.PI) - Math.Floor((freq + phase) / (2 * Math.PI) + 0.5))) - 1
        End Function
    End Class

    Public Class ZeroSignal : Inherits Signal

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq As Double, phase As Double) As Double
            Return 0
        End Function
    End Class
End Namespace
