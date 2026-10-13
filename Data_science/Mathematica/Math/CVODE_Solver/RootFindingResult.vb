#Region "Microsoft.VisualBasic::b3e341da7c9819f5c9e09a1cb18386cd, Data_science\Mathematica\Math\CVODE_Solver\RootFindingResult.vb"

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

    '   Total Lines: 26
    '    Code Lines: 7 (26.92%)
    ' Comment Lines: 15 (57.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (15.38%)
    '     File Size: 564 B


    ' Class RootFindingResult
    ' 
    '     Properties: Found, RootIndex, RootTime, State
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra

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
    Public Property State As Vector
End Class
