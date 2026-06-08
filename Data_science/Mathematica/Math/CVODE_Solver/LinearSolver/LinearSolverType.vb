#Region "Microsoft.VisualBasic::77159f27e1464043aba3c88e04fcaf0b, Data_science\Mathematica\Math\CVODE_Solver\LinearSolver\LinearSolverType.vb"

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

    '   Total Lines: 18
    '    Code Lines: 5 (27.78%)
    ' Comment Lines: 12 (66.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (5.56%)
    '     File Size: 360 B


    ' Enum LinearSolverType
    ' 
    '     Band, Dense, Diagonal
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region


''' <summary>
''' 线性求解器类型枚举
''' </summary>
Public Enum LinearSolverType
    ''' <summary>
    ''' 稠密矩阵直接求解（LU分解）
    ''' </summary>
    Dense
    ''' <summary>
    ''' 带状矩阵求解
    ''' </summary>
    Band
    ''' <summary>
    ''' 对角矩阵求解
    ''' </summary>
    Diagonal
End Enum
