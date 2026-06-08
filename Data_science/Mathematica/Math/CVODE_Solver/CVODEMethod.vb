#Region "Microsoft.VisualBasic::419422fa2ea4b2ff7f0a30038690da5e, Data_science\Mathematica\Math\CVODE_Solver\CVODEMethod.vb"

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

    '   Total Lines: 16
    '    Code Lines: 4 (25.00%)
    ' Comment Lines: 11 (68.75%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (6.25%)
    '     File Size: 348 B


    ' Enum CVODEMethod
    ' 
    '     Adams, BDF
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region


''' <summary>
''' 求解方法枚举
''' </summary>
Public Enum CVODEMethod
    ''' <summary>
    ''' Adams-Bashforth-Moulton预测-校正方法
    ''' 适用于非刚性问题
    ''' </summary>
    Adams
    ''' <summary>
    ''' 后向微分公式（BDF）方法
    ''' 适用于刚性问题
    ''' </summary>
    BDF
End Enum
