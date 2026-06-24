#Region "Microsoft.VisualBasic::7e720efb775b2c7d38f939d820334348, Data_science\Mathematica\Math\CVODE_Solver\CVODEMethod.vb"

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

    '   Total Lines: 15
    '    Code Lines: 4 (26.67%)
    ' Comment Lines: 11 (73.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 346 B


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
