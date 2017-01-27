#Region "Microsoft.VisualBasic::8fb853bd838ee77619be4a5bfdfbf843, ..\sciBASIC#\Data_science\Mathematical\Plots\g\AxisScalling.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges

Public Module AxisScalling

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="max#"></param>
    ''' <param name="parts%"></param>
    ''' <param name="min#"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' + 0-10
    ''' + 0-100
    ''' + 0-1000
    ''' + 0-1E30
    ''' + 0-1E-30
    ''' + 0-1
    ''' + 0-0.1
    ''' </remarks>
    Public Function GetAxisValues(max#, Optional parts% = 10, Optional min# = 0R) As Double()
        Dim d = max - min
        Dim steps = d / parts
        Dim pow% = Fix(Math.Log10(steps))
        Dim tick# = 10 ^ pow

        If parts * tick + min > max + tick Then
            tick = 5 * 10 ^ (pow - 1)
        End If

        Return GetAxisByTick(max, tick, min)
    End Function

    <Extension>
    Public Function GetAxisValues(range As DoubleRange, Optional parts% = 10) As Double()
        Return GetAxisValues(range.Max, parts, range.Min)
    End Function

    <Extension>
    Public Function GetAxisByTick(range As DoubleRange, tick#) As Double()
        Return GetAxisByTick(range.Max, tick, range.Min)
    End Function

    Public Function GetAxisByTick(max#, tick#, Optional min# = 0R) As Double()
        Dim l As New List(Of Double)

        For i As Double = min To max Step tick
            Call l.Add(i)
        Next

        Return l.ToArray
    End Function
End Module

