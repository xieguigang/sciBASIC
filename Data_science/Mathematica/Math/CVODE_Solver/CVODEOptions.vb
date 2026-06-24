#Region "Microsoft.VisualBasic::fd1f2fa38f68998d2ecf97a6ee3c86e4, Data_science\Mathematica\Math\CVODE_Solver\CVODEOptions.vb"

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

    '   Total Lines: 74
    '    Code Lines: 16 (21.62%)
    ' Comment Lines: 45 (60.81%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (17.57%)
    '     File Size: 1.92 KB


    ' Class CVODEOptions
    ' 
    '     Properties: AbsoluteTolerance, InitialStep, JacobianUpdateFrequency, MaxGrowthFactor, MaxNewtonIterations
    '                 MaxOrder, MaxStep, MaxSteps, MinReductionFactor, MinStep
    '                 NewtonConvergenceFactor, RelativeTolerance, SafetyFactor, UseUserJacobian
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' CVODE求解器配置选项
''' </summary>
Public Class CVODEOptions
    ''' <summary>
    ''' 相对误差容差
    ''' </summary>
    Public Property RelativeTolerance As Double = 0.0001

    ''' <summary>
    ''' 绝对误差容差（标量或向量）
    ''' </summary>
    Public Property AbsoluteTolerance As Double = 0.00000001

    ''' <summary>
    ''' 最大阶数（Adams: 1-12, BDF: 1-5）
    ''' </summary>
    Public Property MaxOrder As Integer = 5

    ''' <summary>
    ''' 最大步数
    ''' </summary>
    Public Property MaxSteps As Integer = 10000

    ''' <summary>
    ''' 最大步长（0表示自动）
    ''' </summary>
    Public Property MaxStep As Double = 0.0

    ''' <summary>
    ''' 最小步长（0表示自动）
    ''' </summary>
    Public Property MinStep As Double = 0.0

    ''' <summary>
    ''' 初始步长（0表示自动）
    ''' </summary>
    Public Property InitialStep As Double = 0.0

    ''' <summary>
    ''' Newton迭代最大次数
    ''' </summary>
    Public Property MaxNewtonIterations As Integer = 4

    ''' <summary>
    ''' Newton迭代收敛因子
    ''' </summary>
    Public Property NewtonConvergenceFactor As Double = 0.1

    ''' <summary>
    ''' 步长增长因子上限
    ''' </summary>
    Public Property MaxGrowthFactor As Double = 10.0

    ''' <summary>
    ''' 步长缩减因子下限
    ''' </summary>
    Public Property MinReductionFactor As Double = 0.2

    ''' <summary>
    ''' 安全因子
    ''' </summary>
    Public Property SafetyFactor As Double = 0.9

    ''' <summary>
    ''' 是否使用用户提供的Jacobian
    ''' </summary>
    Public Property UseUserJacobian As Boolean = False

    ''' <summary>
    ''' Jacobian更新策略
    ''' </summary>
    Public Property JacobianUpdateFrequency As Integer = 20
End Class
