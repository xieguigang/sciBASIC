#Region "Microsoft.VisualBasic::08003548382d3dd4f04514a3b6be1391, Data_science\Mathematica\Math\CVODE_Solver\CVODEBuilder.vb"

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

    '   Total Lines: 106
    '    Code Lines: 57 (53.77%)
    ' Comment Lines: 34 (32.08%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (14.15%)
    '     File Size: 2.95 KB


    ' Class CVODEBuilder
    ' 
    '     Function: Build, UseMethod, WithAbsoluteTolerance, WithInitialStep, WithJacobian
    '               WithMaxOrder, WithMaxStep, WithMaxSteps, WithRelativeTolerance, WithRHS
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra

''' <summary>
''' CVODE求解器构建器
''' 提供流式API创建求解器
''' </summary>
Public Class CVODEBuilder

    Private _method As CVODEMethod = CVODEMethod.BDF
    Private _rhsFunc As RHSFunction
    Private _jacobianFunc As JacobianFunction
    Private _dimension As Integer
    Private _options As New CVODEOptions()

    ''' <summary>
    ''' 设置求解方法
    ''' </summary>
    Public Function UseMethod(method As CVODEMethod) As CVODEBuilder
        _method = method
        Return Me
    End Function

    ''' <summary>
    ''' 设置右端函数
    ''' </summary>
    Public Function WithRHS(rhsFunc As RHSFunction, dimension As Integer) As CVODEBuilder
        _rhsFunc = rhsFunc
        _dimension = dimension
        Return Me
    End Function

    ''' <summary>
    ''' 设置Jacobian函数
    ''' </summary>
    Public Function WithJacobian(jacFunc As JacobianFunction) As CVODEBuilder
        _jacobianFunc = jacFunc
        Return Me
    End Function

    ''' <summary>
    ''' 设置相对误差容差
    ''' </summary>
    Public Function WithRelativeTolerance(rtol As Double) As CVODEBuilder
        _options.RelativeTolerance = rtol
        Return Me
    End Function

    ''' <summary>
    ''' 设置绝对误差容差
    ''' </summary>
    Public Function WithAbsoluteTolerance(atol As Double) As CVODEBuilder
        _options.AbsoluteTolerance = atol
        Return Me
    End Function

    ''' <summary>
    ''' 设置最大阶数
    ''' </summary>
    Public Function WithMaxOrder(maxOrder As Integer) As CVODEBuilder
        _options.MaxOrder = maxOrder
        Return Me
    End Function

    ''' <summary>
    ''' 设置最大步数
    ''' </summary>
    Public Function WithMaxSteps(maxSteps As Integer) As CVODEBuilder
        _options.MaxSteps = maxSteps
        Return Me
    End Function

    ''' <summary>
    ''' 设置最大步长
    ''' </summary>
    Public Function WithMaxStep(maxStep As Double) As CVODEBuilder
        _options.MaxStep = maxStep
        Return Me
    End Function

    ''' <summary>
    ''' 设置初始步长
    ''' </summary>
    Public Function WithInitialStep(h0 As Double) As CVODEBuilder
        _options.InitialStep = h0
        Return Me
    End Function

    ''' <summary>
    ''' 构建求解器
    ''' </summary>
    Public Function Build() As CVODESolver
        If _rhsFunc Is Nothing Then
            Throw New InvalidOperationException("必须设置右端函数")
        End If
        If _dimension <= 0 Then
            Throw New InvalidOperationException("必须设置问题维度")
        End If

        Dim solver As New CVODESolver(_method, _rhsFunc, _dimension, _options)

        If _jacobianFunc IsNot Nothing Then
            solver.SetJacobianFunction(_jacobianFunc)
        End If

        Return solver
    End Function

    ''' <summary>
    ''' 构建求解器并直接运行一次积分，结果统一输出为 <see cref="ODEsOut"/> 对象
    ''' （求解器在内部自动释放）
    ''' </summary>
    ''' <param name="t0">积分下界</param>
    ''' <param name="tEnd">积分上界</param>
    ''' <param name="y0">初始状态</param>
    ''' <param name="n">积分步数（输出 ``n + 1`` 个等距采样点）</param>
    ''' <param name="names">结果列的名称，缺省为 ``y0``, ``y1``, ...</param>
    ''' <returns></returns>
    Public Function Solve(t0 As Double, tEnd As Double, y0 As Vector, n As Integer,
                          Optional names As String() = Nothing) As ODEsOut
        Using solver As CVODESolver = Build()
            Return solver.Solve(t0, tEnd, y0, n, names)
        End Using
    End Function

End Class
