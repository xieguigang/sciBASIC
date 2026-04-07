#Region "Microsoft.VisualBasic::72ad94159a1a4bc1bac04bf993e62d1c, Data_science\MachineLearning\LNN\ODESolver.vb"

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

    '   Total Lines: 126
    '    Code Lines: 53 (42.06%)
    ' Comment Lines: 50 (39.68%)
    '    - Xml Docs: 66.00%
    ' 
    '   Blank Lines: 23 (18.25%)
    '     File Size: 4.86 KB


    ' Module ODESolver
    ' 
    ' 
    '     Delegate Function
    ' 
    '         Function: AdaptiveRK45Step, EulerStep, HeunStep, RK4Step
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 常微分方程求解器
''' 用于数值积分液态神经网络的微分方程
''' </summary>
Public Module ODESolver

    ''' <summary>
    ''' 常微分方程的右端函数委托类型
    ''' </summary>
    ''' <param name="state">当前状态</param>
    ''' <param name="input">外部输入</param>
    ''' <param name="time">当前时间</param>
    ''' <returns>状态导数 dx/dt</returns>
    Public Delegate Function ODEFunction(state As Tensor, input As Tensor, time As Double) As Tensor

    ''' <summary>
    ''' 欧拉法求解ODE（一阶精度，计算快速）
    ''' 适用于实时应用和快速原型开发
    ''' </summary>
    ''' <param name="odeFunc">ODE右端函数</param>
    ''' <param name="initialState">初始状态</param>
    ''' <param name="input">外部输入</param>
    ''' <param name="t0">起始时间</param>
    ''' <param name="dt">时间步长</param>
    ''' <returns>下一时刻的状态</returns>
    Public Function EulerStep(odeFunc As ODEFunction, initialState As Tensor, input As Tensor, t0 As Double, dt As Double) As Tensor
        ' 计算导数: dx/dt = f(x, u, t)
        Dim derivative = odeFunc(initialState, input, t0)

        ' 欧拉更新: x(t+dt) = x(t) + dt * dx/dt
        Dim result = initialState + derivative * CSng(dt)

        Return result
    End Function

    ''' <summary>
    ''' 改进欧拉法（Heun方法，二阶精度）
    ''' 比欧拉法更精确，计算量适中
    ''' </summary>
    Public Function HeunStep(odeFunc As ODEFunction, initialState As Tensor, input As Tensor, t0 As Double, dt As Double) As Tensor
        ' 第一步：预测
        Dim k1 = odeFunc(initialState, input, t0)
        Dim predicted = initialState + k1 * CSng(dt)

        ' 第二步：校正
        Dim k2 = odeFunc(predicted, input, t0 + dt)

        ' 平均斜率
        Dim avgSlope = (k1 + k2) * 0.5F

        ' 最终更新
        Dim result = initialState + avgSlope * CSng(dt)

        Return result
    End Function

    ''' <summary>
    ''' 四阶龙格-库塔法（四阶精度）
    ''' 高精度求解器，适用于精确模拟
    ''' </summary>
    Public Function RK4Step(odeFunc As ODEFunction, initialState As Tensor, input As Tensor, t0 As Double, dt As Double) As Tensor
        Dim dtSng = CSng(dt)

        ' k1 = f(x, u, t)
        Dim k1 = odeFunc(initialState, input, t0)

        ' k2 = f(x + dt/2 * k1, u, t + dt/2)
        Dim state2 = initialState + k1 * (dtSng * 0.5F)
        Dim k2 = odeFunc(state2, input, t0 + dt * 0.5)

        ' k3 = f(x + dt/2 * k2, u, t + dt/2)
        Dim state3 = initialState + k2 * (dtSng * 0.5F)
        Dim k3 = odeFunc(state3, input, t0 + dt * 0.5)

        ' k4 = f(x + dt * k3, u, t + dt)
        Dim state4 = initialState + k3 * dtSng
        Dim k4 = odeFunc(state4, input, t0 + dt)

        ' 加权平均: x(t+dt) = x(t) + dt/6 * (k1 + 2*k2 + 2*k3 + k4)
        Dim weightedSum = k1 + k2 * 2.0F + k3 * 2.0F + k4
        Dim result = initialState + weightedSum * (dtSng / 6.0F)

        Return result
    End Function

    ''' <summary>
    ''' 自适应步长RK45求解器（Dormand-Prince方法简化版）
    ''' 根据误差估计自动调整步长
    ''' </summary>
    Public Function AdaptiveRK45Step(odeFunc As ODEFunction, initialState As Tensor, input As Tensor,
                                      t0 As Double, ByRef dt As Double, tolerance As Double) As Tensor
        Dim safetyFactor = 0.9
        Dim minDt = 0.0001
        Dim maxDt = 0.1

        ' 使用当前步长计算RK4解
        Dim rk4Result = RK4Step(odeFunc, initialState, input, t0, dt)

        ' 使用半步长计算两次（更高精度）
        Dim halfDt = dt / 2.0
        Dim tempState = RK4Step(odeFunc, initialState, input, t0, halfDt)
        Dim rk4ResultHalf = RK4Step(odeFunc, tempState, input, t0 + halfDt, halfDt)

        ' 估计误差
        Dim [error] As Double = 0.0
        For i = 0 To initialState.Length - 1
            Dim diff = std.Abs(rk4Result(i) - rk4ResultHalf(i))
            [error] = std.Max([error], diff)
        Next

        ' 调整步长
        If [error] < tolerance Then
            ' 接受结果，可能增大步长
            dt = std.Min(maxDt, dt * safetyFactor * std.Pow(tolerance / ([error] + 0.0000000001), 0.2))
            Return rk4ResultHalf
        Else
            ' 拒绝结果，减小步长重试
            dt = std.Max(minDt, dt * safetyFactor * std.Pow(tolerance / ([error] + 0.0000000001), 0.25))
            Return AdaptiveRK45Step(odeFunc, initialState, input, t0, dt, tolerance)
        End If
    End Function

End Module
