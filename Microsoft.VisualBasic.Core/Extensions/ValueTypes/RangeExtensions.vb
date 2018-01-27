Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges

Namespace ValueTypes

    Public Module RangeExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Percentage(range As DoubleRange, value#) As Double
            Return (value - range.Min) / range.Length
        End Function
    End Module
End Namespace