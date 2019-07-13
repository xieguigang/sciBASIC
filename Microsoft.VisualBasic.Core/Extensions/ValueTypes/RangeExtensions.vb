#Region "Microsoft.VisualBasic::b1d0cec8228de1019b9c7e5945b6c7ef, Microsoft.VisualBasic.Core\Extensions\ValueTypes\RangeExtensions.vb"

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

    '     Module RangeExtensions
    ' 
    '         Function: Percentage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace ValueTypes

    Public Module RangeExtensions

        ''' <summary>
        ''' ```
        ''' d = <paramref name="value"/> - <see cref="DoubleRange.Min"/>
        ''' p% = d / <see cref="DoubleRange.Length"/> * 100%
        ''' ```
        ''' </summary>
        ''' <param name="range"></param>
        ''' <param name="value#"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Percentage(range As DoubleRange, value#) As Double
            Return If(value = range.Min, 0, (value - range.Min) / range.Length)
        End Function
    End Module
End Namespace
