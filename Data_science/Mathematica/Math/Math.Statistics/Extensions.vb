#Region "Microsoft.VisualBasic::100b8def2d5a92777d764cf107e22ede, Data_science\Mathematica\Math\Math.Statistics\Extensions.vb"

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

    '   Total Lines: 52
    '    Code Lines: 27
    ' Comment Lines: 17
    '   Blank Lines: 8
    '     File Size: 1.54 KB


    ' Module Extensions
    ' 
    '     Function: CI, CI68, CI95, CI99, SD
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports std = System.Math

Public Module Extensions

    ' 95％置信区间上限=均数+1.96×标准差
    ' 95％置信区间下限=均数-1.96×标准差

    '       Z
    ' 80%   1.282
    ' 85%   1.440
    ' 90%   1.645
    ' 95%   1.960
    ' 99%   2.576
    ' 99.5%	2.807
    ' 99.9%	3.291

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
        Dim lower = m - factor * sd / std.Sqrt(n)
        Dim upper = m + factor * sd / std.Sqrt(n)
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
