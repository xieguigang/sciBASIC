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
    '    Code Lines: 57 (53.27%)
    ' Comment Lines: 36 (33.64%)
    '    - Xml Docs: 94.44%
    ' 
    '   Blank Lines: 14 (13.08%)
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
''' Solving the Ordinary differential equation(ODE) by using the explicit Runge-Kutta
''' family methods (Euler / RK2 / RK4 / Gill) and the adaptive step size Dormand-Prince
''' RK45 method.(使用显式龙格库塔族方法求解常微分方程)
''' </summary>
Public Module ODESolver

    ''' <summary>
    ''' df函数指针，微分方程 ``df = f(x,y)``
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    Public Delegate Function df(x As Double, y As Double) As Double

    ''' <summary>
    ''' Get the derivative function pointer from the given <see cref="ODE"/> model,
    ''' throws a descriptive error when the model is not initialized correctly.
    ''' </summary>
    ''' <param name="ode"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Function GetDerivative(ode As ODE) As df
        Dim f As df = If(ode Is Nothing, Nothing, ode.df)

        If f Is Nothing Then
            Throw New InvalidOperationException($"The derivative function pointer ``df`` of the ODE model '{If(ode?.ID, "(null)")}' is not initialized!")
        End If

        Return f
    End Function

    ''' <summary>
    ''' 欧拉法解微分方程，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0,y0)，ODE的结果会从x和y这两个数组指针返回
    ''' </summary>
    ''' <param name="ode"></param>
    ''' <param name="n">
    ''' 积分步数，返回的结果将会含有 n + 1 个采样点(包含积分区间两端的端点)
    ''' </param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' 
    <Extension>
    Public Function Eluer(ode As ODE, n As Integer, a As Double, b As Double) As ODEOutput
        Dim f As df = GetDerivative(ode)
        Dim h As Double = (b - a) / n
        Dim out As ODEOutput = ode.Allocate(n, a, b)
        Dim y#() = out.Y.vector
        Dim x# = a

        ' y(i) = y(i - 1) + h * f(x, y(i - 1))，默认的初值 y(0) = y0 已经在 Allocate 之中完成赋值
        For i As Integer = 1 To n
            y(i) = y(i - 1) + h * f(x, y(i - 1))
            x = a + h * i
        Next

        Return out
    End Function

    ''' <summary>
    ''' 二阶龙格库塔法解解微分方程(改进欧拉法/Heun方法)，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0, y0)
    ''' </summary>
    ''' <param name="ode"></param>
    ''' <param name="n">
    ''' 积分步数，返回的结果将会含有 n + 1 个采样点(包含积分区间两端的端点)
    ''' </param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    <Extension>
    Public Function RK2(ode As ODE, n As Integer, a As Double, b As Double) As ODEOutput
        Dim f As df = GetDerivative(ode)
        Dim h As Double = (b - a) / n
        Dim out As ODEOutput = ode.Allocate(n, a, b)
        Dim y#() = out.Y.vector
        Dim x# = a
        Dim y0 As Double, k1 As Double, k2 As Double

        For i As Integer = 1 To n
            y0 = y(i - 1)
            k1 = f(x, y0)
            k2 = f(x + h, y0 + h * k1)
            y(i) = y0 + h / 2 * (k1 + k2)
            x = a + h * i
        Next

        Return out
    End Function

    ''' <summary>
    ''' 申请ODE积分结果的存储空间，结果向量含有 ``n + 1`` 个采样点，并且已经写入了初值 ``y0``
    ''' </summary>
    ''' <param name="ode"></param>
    ''' <param name="n">积分步数</param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    <Extension>
    Public Function Allocate(ode As ODE, n As Integer, a As Double, b As Double) As ODEOutput
        If ode Is Nothing Then
            Throw New ArgumentNullException(NameOf(ode))
        End If

        Dim out As New ODEOutput With {
            .X = New ODESequence(a, b, n),
            .Y = New NumericVector With {
                .vector = New Double(n) {}
            }
        }

        out.Y.vector(0) = ode.y0

        Return out
    End Function

    ''' <summary>
    ''' 四阶龙格库塔法解解微分方程(经典RK4)，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0, y0)
    ''' </summary>
    ''' <param name="ode"></param>
    ''' <param name="n">
    ''' 分辨率，越大越好，分辨率过低会出现误差过大的问题，分辨率越高，所需要的计算时间也越长
    ''' </param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' 
    <Extension>
    Public Function RK4(ode As ODE, n As Integer, a As Double, b As Double) As ODEOutput
        Dim f As df = GetDerivative(ode)
        Dim h As Double = (b - a) / n
        Dim out As ODEOutput = ode.Allocate(n, a, b)
        Dim y#() = out.Y.vector
        Dim x# = a
        Dim y0 As Double, k1 As Double, k2 As Double, k3 As Double, k4 As Double
        Dim halfH As Double = 0.5 * h
        Dim h6 As Double = h / 6

        For i As Integer = 1 To n
            y0 = y(i - 1)
            k1 = f(x, y0)
            k2 = f(x + halfH, y0 + halfH * k1)
            k3 = f(x + halfH, y0 + halfH * k2)
            k4 = f(x + h, y0 + h * k3)
            y(i) = y0 + h6 * (k1 + 2 * k2 + 2 * k3 + k4)
            x = a + h * i
        Next

        Return out
    End Function
End Module
