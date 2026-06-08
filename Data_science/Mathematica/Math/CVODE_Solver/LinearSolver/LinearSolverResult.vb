#Region "Microsoft.VisualBasic::aad8d978585990a89ab2cd3e36d723d6, Data_science\Mathematica\Math\CVODE_Solver\LinearSolver\LinearSolverResult.vb"

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

    '   Total Lines: 23
    '    Code Lines: 6 (26.09%)
    ' Comment Lines: 15 (65.22%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (8.70%)
    '     File Size: 443 B


    ' Enum LinearSolverResult
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region



''' <summary>
''' 线性求解器返回状态
''' </summary>
Public Enum LinearSolverResult
    ''' <summary>
    ''' 求解成功
    ''' </summary>
    Success = 0
    ''' <summary>
    ''' 矩阵奇异
    ''' </summary>
    SingularMatrix = -1
    ''' <summary>
    ''' 求解失败
    ''' </summary>
    SolveFailed = -2
    ''' <summary>
    ''' 内存分配失败
    ''' </summary>
    MemoryFail = -3
End Enum
