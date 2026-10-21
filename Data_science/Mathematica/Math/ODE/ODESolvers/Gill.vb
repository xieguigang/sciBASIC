#Region "Microsoft.VisualBasic::2cc84e93ec96ca24b27252f950db86b5, Data_science\Mathematica\Math\ODE\ODESolvers\Gill.vb"

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
    '    Code Lines: 24 (70.59%)
    ' Comment Lines: 4 (11.76%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 6 (17.65%)
    '     File Size: 1.23 KB


    ' Module ODESolver
    ' 
    '     Function: Gill
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

Partial Module ODESolver

    ''' <summary>
    ''' 把根号2算出来，不在循环体内反复执行根号2，减少计算负担
    ''' </summary>
    ReadOnly sqr2# = std.Sqrt(2.0)

    ''' <summary>
    ''' Gill 四阶方法，与经典 RK4 同阶，但是通过特殊选取的系数将浮点舍入误差
    ''' 的累积降到最低，适合长时程积分。
    ''' 
    ''' 标准的 Gill 系数如下：
    ''' 
    ''' ```
    ''' k1 = f(t, y)
    ''' k2 = f(t + h/2, y + h/2 * k1)
    ''' k3 = f(t + h/2, y + h * (((sqr2 - 1) / 2) * k1 + ((2 - sqr2) / 2) * k2))
    ''' k4 = f(t + h,   y + h * (-sqr2 / 2 * k2 + (1 + sqr2 / 2) * k3))
    ''' y  = y + h/6 * (k1 + (2 - sqr2) * k2 + (2 + sqr2) * k3 + k4)
    ''' ```
    ''' </summary>
    ''' <param name="ode"></param>
    ''' <param name="N">积分步数，返回的结果将会含有 N + 1 个采样点(包含积分区间两端的端点)</param>
    ''' <param name="t0"></param>
    ''' <param name="tt"></param>
    <Extension>
    Public Function Gill(ode As ODE, N As Integer, t0 As Double, tt As Double) As ODEOutput
        Dim f As df = GetDerivative(ode)
        Dim out As ODEOutput = ode.Allocate(N, t0, tt)
        Dim vector#() = out.Y.vector
        Dim h# = (tt - t0) / N   ' 步长
        Dim y As Double = ode.y0, t As Double = t0    ' 积分初值
        Dim k1 As Double, k2 As Double, k3 As Double, k4 As Double

        ' Gill 系数常量
        Dim a31 As Double = 0.5 * (sqr2 - 1.0)          ' k3 的 k1 系数
        Dim a32 As Double = 0.5 * (2.0 - sqr2)          ' k3 的 k2 系数
        Dim a42 As Double = -0.5 * sqr2                 ' k4 的 k2 系数
        Dim a43 As Double = 1.0 + 0.5 * sqr2            ' k4 的 k3 系数
        Dim b2 As Double = 2.0 - sqr2                   ' 终值的 k2 权重
        Dim b3 As Double = 2.0 + sqr2                   ' 终值的 k3 权重
        Dim halfH As Double = 0.5 * h
        Dim h6 As Double = h / 6.0

        ' 因为已经设置了y0了，所以在这里都是从1开始的
        For i% = 1 To N
            k1 = f(t, y)
            k2 = f(t + halfH, y + halfH * k1)
            k3 = f(t + halfH, y + h * (a31 * k1 + a32 * k2))
            k4 = f(t + h, y + h * (a42 * k2 + a43 * k3))

            y = y + h6 * (k1 + b2 * k2 + b3 * k3 + k4)
            t = t0 + h * i
            vector(i) = y
        Next

        Return out
    End Function
End Module
