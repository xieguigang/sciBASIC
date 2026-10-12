#Region "Microsoft.VisualBasic::f6477e1fdc357bc527da19cfb8f95ef5, gr\physics\Joints\IConstraint.vb"

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

    '   Total Lines: 19
    '    Code Lines: 7 (36.84%)
    ' Comment Lines: 8 (42.11%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 4 (21.05%)
    '     File Size: 667 B


    '     Interface IConstraint
    ' 
    '         Sub: SolvePosition, SolveVelocity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 约束/关节统一接口。

Imports Microsoft.VisualBasic.Imaging.Physics

Namespace Joints

    ''' <summary>
    ''' 约束（关节）统一接口。在物理世界的速度迭代阶段调用 <see cref="SolveVelocity"/>，
    ''' 在位置阶段调用 <see cref="SolvePosition"/> 做穿透/漂移修正。
    ''' </summary>
    Public Interface IConstraint
        ''' <summary>在速度求解迭代中施加冲量</summary>
        Sub SolveVelocity(dt As Double)

        ''' <summary>位置修正（消除约束漂移）</summary>
        Sub SolvePosition(dt As Double)
    End Interface
End Namespace
