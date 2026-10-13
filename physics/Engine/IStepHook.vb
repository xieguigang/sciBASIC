#Region "Microsoft.VisualBasic::381860b88455ca3ed7cedb732df8147e, physics\Engine\IStepHook.vb"

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
    '    Code Lines: 3 (16.67%)
    ' Comment Lines: 14 (77.78%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (5.56%)
    '     File Size: 879 B


    ' Interface IStepHook
    ' 
    '     Sub: BeforeSubstep
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' 子步钩子：需要在<strong>每个子步</strong>都重新施力的外部控制器实现该接口。
''' </summary>
''' <remarks>
''' <see cref="PhysicsWorld3D.StepSub"/> 会在每个子步的末尾调用
''' <see cref="RigidBody3D.ClearForces"/>，因此在 <c>Step</c> 之前施加一次的力
''' 只会在第一个子步生效，等效强度被稀释成 <c>1/Substeps</c>。
''' 像平衡控制器、步态马达这类"每帧持续施加"的力必须通过本钩子在每个子步补充，
''' 而冲量（<see cref="RigidBody3D.ApplyImpulse"/>）直接改速度，不受影响。
''' </remarks>
Public Interface IStepHook

    ''' <summary>
    ''' 在施加重力之前、马达扭矩之后被调用。
    ''' </summary>
    ''' <param name="dt">当前子步长（秒）。</param>
    Sub BeforeSubstep(dt As Double)
End Interface

