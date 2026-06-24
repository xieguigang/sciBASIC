#Region "Microsoft.VisualBasic::860343060b1d5d2bc1e3ca9a9a4fc00a, Data_science\Mathematica\Math\CVODE_Solver\CVODEStatus.vb"

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

    '   Total Lines: 65
    '    Code Lines: 17 (26.15%)
    ' Comment Lines: 48 (73.85%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 1.38 KB


    ' Enum CVODEStatus
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' 求解器返回状态
''' </summary>
Public Enum CVODEStatus
    ''' <summary>
    ''' 成功
    ''' </summary>
    Success = 0
    ''' <summary>
    ''' 达到停止时间
    ''' </summary>
    TStopReturn = 1
    ''' <summary>
    ''' 根找到
    ''' </summary>
    RootReturn = 2
    ''' <summary>
    ''' 函数未定义
    ''' </summary>
    FuncUndefined = -1
    ''' <summary>
    ''' 步长过小
    ''' </summary>
    StepTooSmall = -2
    ''' <summary>
    ''' 测试失败
    ''' </summary>
    TestFail = -3
    ''' <summary>
    ''' 求解失败
    ''' </summary>
    SolveFailed = -4
    ''' <summary>
    ''' 收敛失败
    ''' </summary>
    ConvFail = -5
    ''' <summary>
    ''' 线性求解器初始化失败
    ''' </summary>
    LinearSolverInitFail = -6
    ''' <summary>
    ''' 线性求解失败
    ''' </summary>
    LinearSolveFail = -7
    ''' <summary>
    ''' 右端函数重复失败
    ''' </summary>
    RHSFuncFail = -8
    ''' <summary>
    ''' 首步失败
    ''' </summary>
    FirstStepFail = -9
    ''' <summary>
    ''' 步数超限
    ''' </summary>
    TooManySteps = -10
    ''' <summary>
    ''' 内存错误
    ''' </summary>
    MemoryError = -11
    ''' <summary>
    ''' 参数错误
    ''' </summary>
    BadInput = -12
End Enum
