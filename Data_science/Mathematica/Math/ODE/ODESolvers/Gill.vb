#Region "Microsoft.VisualBasic::94aee2d3628307aab938ce8df3f96def, Data_science\Mathematica\Math\ODE\ODESolvers\Gill.vb"

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

    '   Total Lines: 34
    '    Code Lines: 24
    ' Comment Lines: 4
    '   Blank Lines: 6
    '     File Size: 1.24 KB


    ' Module ODESolver
    ' 
    '     Function: Gill
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Partial Module ODESolver

    ''' <summary>
    ''' 把根号2算出来，不在循环体内反复执行根号2，减少计算负担
    ''' </summary>
    ReadOnly sqr2# = stdNum.Sqrt(2.0)

    <Extension>
    Public Function Gill(ByRef ode As ODE, N As Integer, t0 As Double, tt As Double) As ODEOutput
        Dim df As df = ode.df
        Dim out As ODEOutput = ode.Allocate(N, t0, tt)
        Dim y As Double = ode.y0, t As Double = t0    ' 积分初值
        Dim k1 As Double, k2 As Double, k3 As Double, k4 As Double
        Dim h# = (tt - t0) / N   ' 步长
        Dim vector#() = out.Y.Vector

        ' 因为已经设置了y0了，所以在这里都是从1开始的
        For i% = 1 To N
            k1 = df(y, t)
            k2 = df(y + 0.5 * h * k1, t + 0.5 * h)
            k3 = df(y + 0.5 * (sqr2 - 1.0) * h * k1 + 0.5 * (1 - sqr2) * h * k2, t + 0.5 * h)
            k4 = df(y - sqr2 * 0.5 * h * k2 + (1 + sqr2) * h * k3, t + h)

            y = y + (k1 + k2 * (2 - sqr2) + k3 * (2 + sqr2) + k4) * h / 6.0
            t = t + h
            vector(i) = y
        Next

        Return out
    End Function
End Module
