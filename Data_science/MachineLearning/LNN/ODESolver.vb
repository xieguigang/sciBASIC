#Region "Microsoft.VisualBasic::e9c245e1cc91f9fd145048d7adce9a03, Data_science\MachineLearning\LNN\ODESolver.vb"

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

    '   Total Lines: 245
    '    Code Lines: 116 (47.35%)
    ' Comment Lines: 84 (34.29%)
    '    - Xml Docs: 78.57%
    ' 
    '   Blank Lines: 45 (18.37%)
    '     File Size: 9.14 KB


    ' Module LNNMath
    ' 
    '     Function: Mul, Scale
    ' 
    '     Sub: AddInPlace
    ' 
    ' Module ODESolver
    ' 
    ' 
    '     Delegate Function
    ' 
    '         Function: AdaptiveRK45Step, EulerStep, HeunStep, RK4Step, ScaleAdd
    '     Class ODEStages
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' LNN 内部使用的张量数值工具（全程 Double 精度）
''' </summary>
Friend Module LNNMath

    ''' <summary>返回 a 的逐元素缩放副本</summary>
    Public Function Scale(a As Tensor, factor As Double) As Tensor
        Dim r = New Tensor(a.Shape)

        For i = 0 To a.Length - 1
            r(i) = a(i) * factor
        Next

        Return r
    End Function

    ''' <summary>把 src 累加到 target 之上：target += factor * src</summary>
    Public Sub AddInPlace(target As Tensor, src As Tensor, Optional factor As Double = 1.0)
        For i = 0 To target.Length - 1
            target(i) += src(i) * factor
        Next
    End Sub

    ''' <summary>
    ''' 逐元素乘（Hadamard 积）
    ''' </summary>
    ''' <remarks>
    ''' Tensor 的 <c>*</c> 运算符对两个张量而言是矩阵乘法，这里提供明确的逐元素版本。
    ''' 不检查形状，以便 1-D 与 (1,n) 的行向量形式可以混用（扁平长度相同即可）。
    ''' </remarks>
    Public Function Mul(a As Tensor, b As Tensor) As Tensor
        Dim r = New Tensor(a.Shape)

        For i = 0 To a.Length - 1
            r(i) = a(i) * b(i)
        Next

        Return r
    End Function

End Module

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
    ''' 一次数值积分内各阶斜率 f(·) 的求值点记录
    ''' </summary>
    ''' <remarks>
    ''' 反向模式自动微分（BPTT）需要按前向的图结构回放，因此这里只登记
    ''' 被求值过的状态点；具体的中间激活值在反向时重新计算，避免占用额外内存。
    ''' </remarks>
    Public Class ODEStages

        ''' <summary>k1 的求值点，即步首状态 x(t)</summary>
        Public s1 As Tensor

        ''' <summary>k2 的求值点</summary>
        Public s2 As Tensor

        ''' <summary>k3 的求值点</summary>
        Public s3 As Tensor

        ''' <summary>k4 的求值点</summary>
        Public s4 As Tensor

        ''' <summary>Heun 方法的预测点</summary>
        Public p As Tensor

    End Class

    ''' <summary>
    ''' 以全 Double 精度计算 a + b * factor
    ''' </summary>
    ''' <remarks>
    ''' 早期实现使用 <c>CSng(dt)</c> 以及 Tensor 与 Single 的乘法运算符，
    ''' 会把状态压缩到单精度，长序列上产生明显的累积误差；这里统一走 Double。
    ''' </remarks>
    Private Function ScaleAdd(a As Tensor, b As Tensor, factor As Double) As Tensor
        Dim r = New Tensor(a.Shape)

        For i = 0 To a.Length - 1
            r(i) = a(i) + b(i) * factor
        Next

        Return r
    End Function

    ''' <summary>
    ''' 欧拉法求解ODE（一阶精度，计算快速）
    ''' 适用于实时应用和快速原型开发
    ''' </summary>
    ''' <param name="odeFunc">ODE右端函数</param>
    ''' <param name="initialState">初始状态</param>
    ''' <param name="input">外部输入</param>
    ''' <param name="t0">起始时间</param>
    ''' <param name="dt">时间步长</param>
    ''' <param name="stages">可选：登记本次积分的求值点，供反向传播使用</param>
    ''' <returns>下一时刻的状态</returns>
    Public Function EulerStep(odeFunc As ODEFunction, initialState As Tensor, input As Tensor,
                              t0 As Double, dt As Double, Optional stages As ODEStages = Nothing) As Tensor
        ' 计算导数: dx/dt = f(x, u, t)
        Dim derivative = odeFunc(initialState, input, t0)

        If stages IsNot Nothing Then
            stages.s1 = initialState
        End If

        ' 欧拉更新: x(t+dt) = x(t) + dt * dx/dt
        Return ScaleAdd(initialState, derivative, dt)
    End Function

    ''' <summary>
    ''' 改进欧拉法（Heun方法，二阶精度）
    ''' 比欧拉法更精确，计算量适中
    ''' </summary>
    Public Function HeunStep(odeFunc As ODEFunction, initialState As Tensor, input As Tensor,
                             t0 As Double, dt As Double, Optional stages As ODEStages = Nothing) As Tensor
        ' 第一步：预测
        Dim k1 = odeFunc(initialState, input, t0)
        Dim predicted = ScaleAdd(initialState, k1, dt)

        ' 第二步：校正
        Dim k2 = odeFunc(predicted, input, t0 + dt)

        ' 平均斜率
        Dim avgSlope = New Tensor(initialState.Shape)
        For i = 0 To initialState.Length - 1
            avgSlope(i) = (k1(i) + k2(i)) * 0.5
        Next

        If stages IsNot Nothing Then
            stages.s1 = initialState
            stages.p = predicted
        End If

        ' 最终更新
        Return ScaleAdd(initialState, avgSlope, dt)
    End Function

    ''' <summary>
    ''' 四阶龙格-库塔法（四阶精度）
    ''' 高精度求解器，适用于精确模拟
    ''' </summary>
    Public Function RK4Step(odeFunc As ODEFunction, initialState As Tensor, input As Tensor,
                            t0 As Double, dt As Double, Optional stages As ODEStages = Nothing) As Tensor
        ' k1 = f(x, u, t)
        Dim k1 = odeFunc(initialState, input, t0)

        ' k2 = f(x + dt/2 * k1, u, t + dt/2)
        Dim state2 = ScaleAdd(initialState, k1, dt * 0.5)
        Dim k2 = odeFunc(state2, input, t0 + dt * 0.5)

        ' k3 = f(x + dt/2 * k2, u, t + dt/2)
        Dim state3 = ScaleAdd(initialState, k2, dt * 0.5)
        Dim k3 = odeFunc(state3, input, t0 + dt * 0.5)

        ' k4 = f(x + dt * k3, u, t + dt)
        Dim state4 = ScaleAdd(initialState, k3, dt)
        Dim k4 = odeFunc(state4, input, t0 + dt)

        ' 加权平均: x(t+dt) = x(t) + dt/6 * (k1 + 2*k2 + 2*k3 + k4)
        Dim weightedSum = New Tensor(initialState.Shape)
        For i = 0 To initialState.Length - 1
            weightedSum(i) = k1(i) + 2.0 * k2(i) + 2.0 * k3(i) + k4(i)
        Next

        If stages IsNot Nothing Then
            stages.s1 = initialState
            stages.s2 = state2
            stages.s3 = state3
            stages.s4 = state4
        End If

        Return ScaleAdd(initialState, weightedSum, dt / 6.0)
    End Function

    ''' <summary>
    ''' 自适应步长RK45求解器（Dormand-Prince方法简化版）
    ''' 根据误差估计自动调整步长
    ''' </summary>
    ''' <param name="maxRetries">拒绝步长的最大重试次数，防止在 stiff 系统上无限递归导致栈溢出</param>
    Public Function AdaptiveRK45Step(odeFunc As ODEFunction, initialState As Tensor, input As Tensor,
                                      t0 As Double, ByRef dt As Double, tolerance As Double,
                                      Optional maxRetries As Integer = 32) As Tensor
        Dim safetyFactor = 0.9
        Dim minDt = 0.0001
        Dim maxDt = 0.1
        Dim result As Tensor = Nothing

        For retry = 0 To maxRetries
            ' 使用当前步长计算RK4解
            Dim fullStep = RK4Step(odeFunc, initialState, input, t0, dt)

            ' 使用半步长计算两次（更高精度）
            Dim halfDt = dt / 2.0
            Dim tempState = RK4Step(odeFunc, initialState, input, t0, halfDt)
            Dim twoHalfStep = RK4Step(odeFunc, tempState, input, t0 + halfDt, halfDt)

            ' 估计误差
            Dim [error] As Double = 0.0

            For i = 0 To initialState.Length - 1
                [error] = std.Max([error], std.Abs(fullStep(i) - twoHalfStep(i)))
            Next

            If [error] < tolerance Then
                ' 接受结果，可能增大步长
                dt = std.Min(maxDt, dt * safetyFactor * std.Pow(tolerance / ([error] + 0.0000000001), 0.2))
                result = twoHalfStep
                Exit For
            ElseIf dt <= minDt Then
                ' 已经退到最小步长，无法再通过细化步长降低误差，直接接受避免死循环
                result = twoHalfStep
                Exit For
            Else
                ' 拒绝结果，减小步长重试
                dt = std.Max(minDt, dt * safetyFactor * std.Pow(tolerance / ([error] + 0.0000000001), 0.25))
            End If
        Next

        If result Is Nothing Then
            ' 理论上不可达：循环至少会在 dt 收缩到 minDt 时收敛
            result = RK4Step(odeFunc, initialState, input, t0, dt)
        End If

        Return result
    End Function

End Module
