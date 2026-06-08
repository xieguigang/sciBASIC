#Region "Microsoft.VisualBasic::208541be200f5fa2ef5cf00f9c2edbb8, Data_science\Mathematica\Math\CVODE_Solver\RootFindingResult.vb"

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

    '   Total Lines: 24
    '    Code Lines: 6 (25.00%)
    ' Comment Lines: 15 (62.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (12.50%)
    '     File Size: 513 B


    ' Class RootFindingResult
    ' 
    '     Properties: Found, RootIndex, RootTime, State
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' 根查找结果
''' </summary>
Public Class RootFindingResult
    ''' <summary>
    ''' 是否找到根
    ''' </summary>
    Public Property Found As Boolean

    ''' <summary>
    ''' 根所在的时间
    ''' </summary>
    Public Property RootTime As Double

    ''' <summary>
    ''' 根的索引
    ''' </summary>
    Public Property RootIndex As Integer

    ''' <summary>
    ''' 根处的状态
    ''' </summary>
    Public Property State As NVector
End Class
