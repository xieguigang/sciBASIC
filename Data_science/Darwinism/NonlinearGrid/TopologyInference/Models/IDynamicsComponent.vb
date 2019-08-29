#Region "Microsoft.VisualBasic::1ad5c79fc2409de496e89442662aa040, Data_science\Darwinism\NonlinearGrid\TopologyInference\Models\IDynamicsComponent.vb"

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

    ' Interface IDynamicsComponent
    ' 
    '     Properties: Width
    ' 
    '     Function: Evaluate
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

Public Interface IDynamicsComponent(Of T) : Inherits ICloneable(Of T)

    ''' <summary>
    ''' 获取的到这个动力学系统中的系统变量的个数
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property Width As Integer

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="X">系统变量/样本数据</param>
    ''' <returns></returns>
    Function Evaluate(X As Vector) As Double
End Interface
