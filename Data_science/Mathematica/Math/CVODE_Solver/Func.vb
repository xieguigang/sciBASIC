#Region "Microsoft.VisualBasic::47f3d9d66d372f5293fdefd838cc475d, Data_science\Mathematica\Math\CVODE_Solver\Func.vb"

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

    '   Total Lines: 27
    '    Code Lines: 3 (11.11%)
    ' Comment Lines: 21 (77.78%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (11.11%)
    '     File Size: 1015 B


    ' Delegate Sub
    ' 
    ' 
    ' Delegate Sub
    ' 
    ' 
    ' Delegate Sub
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region


''' <summary>
''' 右端函数委托类型
''' 定义常微分方程组 dy/dt = f(t, y)
''' </summary>
''' <param name="t">当前时间</param>
''' <param name="y">当前状态向量</param>
''' <param name="ydot">导数向量（输出）</param>
Public Delegate Sub RHSFunction(t As Double, y As NVector, ydot As NVector)

''' <summary>
''' Jacobian矩阵计算委托类型
''' 计算Jacobian矩阵 J = ∂f/∂y
''' </summary>
''' <param name="t">当前时间</param>
''' <param name="y">当前状态向量</param>
''' <param name="fy">当前导数向量</param>
''' <param name="J">Jacobian矩阵（输出）</param>
Public Delegate Sub JacobianFunction(t As Double, y As NVector, fy As NVector, J As DenseMatrix)

''' <summary>
''' 根函数委托类型
''' </summary>
''' <param name="t">当前时间</param>
''' <param name="y">当前状态</param>
''' <param name="g">根函数值数组（输出）</param>
Public Delegate Sub RootFunction(t As Double, y As NVector, g As Double())
