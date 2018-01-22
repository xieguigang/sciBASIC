Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges

Public Module Extensions

    ' 95％置信区间上限=均数+1.96×标准差
    ' 95％置信区间下限=均数-1.96×标准差

    ''' <summary>
    ''' 从95%的置信区间推断出可能的SD值
    ''' </summary>
    ''' <param name="m#"></param>
    ''' <param name="up#"></param>
    ''' <param name="down#"></param>
    ''' <returns></returns>
    Public Function SD(m#, up#, down#) As Double
        up = (up - m) / 1.96
        down = -(down - m) / 1.96
        Return (up + down) / 2
    End Function

    Public Function CI(m#, factor#, sd#, n%) As DoubleRange
        Dim lower = m - factor * sd / Math.Sqrt(n)
        Dim upper = m + factor * sd / Math.Sqrt(n)
        Return New DoubleRange(lower, upper)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function CI99(m#, sd#, n%) As DoubleRange
        Return CI(m, 2.58, sd, n)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function CI95(m#, sd#, n%) As DoubleRange
        Return CI(m, 1.96, sd, n)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function CI68(m#, sd#, n%) As DoubleRange
        Return CI(m, 1, sd, n)
    End Function
End Module
