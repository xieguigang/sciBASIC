#Region "Microsoft.VisualBasic::5896147eedba267bd7bdec57b3253a59, physics\Engine\Joints3D\IConstraint3D.vb"

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

    '   Total Lines: 14
    '    Code Lines: 6 (42.86%)
    ' Comment Lines: 5 (35.71%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (21.43%)
    '     File Size: 554 B


    '     Interface IConstraint3D
    ' 
    '         Sub: SolvePosition, SolveVelocity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Joints3D

    ''' <summary>
    ''' 3D 约束（关节）统一接口，与 2D 的 <see cref="Joints.IConstraint"/> 完全同构。
    ''' </summary>
    Public Interface IConstraint3D

        ''' <summary>速度层求解：在不改变位置的前提下消除违反约束的相对速度。</summary>
        Sub SolveVelocity(dt As Double)

        ''' <summary>位置层求解（软修正）：直接微调位置/姿态以消除累积漂移。</summary>
        Sub SolvePosition(dt As Double)
    End Interface
End Namespace

