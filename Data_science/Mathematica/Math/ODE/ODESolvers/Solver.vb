#Region "Microsoft.VisualBasic::18cdc478973a1234c0fec3e10d09b138, Data_science\Mathematica\Math\ODE\ODESolvers\Solver.vb"

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

    '   Total Lines: 107
    '    Code Lines: 57
    ' Comment Lines: 36
    '   Blank Lines: 14
    '     File Size: 3.83 KB


    ' Module ODESolver
    ' 
    ' 
    '     Delegate Function
    ' 
    '         Function: Allocate, Eluer, RK2, RK4
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text.Xml.Models

''' <summary>
''' Solving the Ordinary differential equation(ODE) by using trapezoidal method.(使用梯形法求解常微分方程)
''' </summary>
''' <remarks>http://www.oschina.net/code/snippet_76_4433</remarks>
Public Module ODESolver

    ''' <summary>
    ''' df函数指针，微分方程 ``df = f(x,y)``
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    Public Delegate Function df(x As Double, y As Double) As Double

    ''' <summary>
    ''' 欧拉法解微分方程，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0,y0)，ODE的结果会从x和y这两个数组指针返回
    ''' </summary>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' 
    <Extension>
    Public Function Eluer(ByRef df As ODE, n As Integer, a As Double, b As Double) As ODEOutput
        Dim h As Double = (b - a) / n
        Dim out As ODEOutput = df.Allocate(n, a, b)
        Dim x# = a, y#() = out.Y.Vector

        For i As Integer = 1 To n - 1
            y(i) = x + h * df(x, y(i - 1))
            x = a + h * i
        Next

        Return out
    End Function

    ''' <summary>
    ''' 二阶龙格库塔法解解微分方程，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0, y0)
    ''' 参考http://blog.sina.com.cn/s/blog_698c6a6f0100lp4x.html
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    <Extension>
    Public Function RK2(ByRef df As ODE, n As Integer, a As Double, b As Double) As ODEOutput
        Dim h As Double = (b - a) / n
        Dim k1 As Double, k2 As Double
        Dim out As ODEOutput = df.Allocate(n, a, b)
        Dim y#() = out.Y.Vector
        Dim x# = a

        For i As Integer = 1 To n - 1
            k1 = df(x, y(i - 1))
            k2 = df(x + h, y(i - 1) + h * k1)
            y(i) = y(i - 1) + h / 2 * (k1 + k2)
            x = a + h * i
        Next

        Return out
    End Function

    <Extension>
    Public Function Allocate(ByRef ode As ODE, n%, a#, b#) As ODEOutput
        Dim out As New ODEOutput

        out.X = New Sequence(a, b, n)
        out.Y = New NumericVector With {
            .Vector = New Double(n - 1) {}
        }
        out.Y.Vector(Scan0) = ode.y0

        Return out
    End Function

    ''' <summary>
    ''' 四阶龙格库塔法解解微分方程，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0, y0)
    ''' 参考http://blog.sina.com.cn/s/blog_698c6a6f0100lp4x.html 和维基百科
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="n">
    ''' 分辨率，越大越好，分辨率过低会出现误差过大的问题，分辨率越高，所需要的计算时间也越长
    ''' </param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' 
    <Extension>
    Public Function RK4(ByRef df As ODE, n As Integer, a As Double, b As Double) As ODEOutput
        Dim h As Double = (b - a) / n
        Dim out As ODEOutput = df.Allocate(n, a, b)
        Dim x = a, y#() = out.Y.Vector
        Dim k1, k2, k3, k4 As Double

        For i As Integer = 1 To n - 1
            k1 = df(x, y(i - 1))
            k2 = df(x + 0.5 * h, y(i - 1) + 0.5 * h * k1)
            k3 = df(x + 0.5 * h, y(i - 1) + 0.5 * h * k2)
            k4 = df(x + h, y(i - 1) + h * k3)
            y(i) = y(i - 1) + h / 6 * (k1 + 2 * k2 + 2 * k3 + k4)
            x = a + h * i
        Next

        Return out
    End Function
End Module
