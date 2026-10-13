#Region "Microsoft.VisualBasic::27a5b8f7b49ffd684cad32d25c67758c, physics\Particles\Grid\IContainer3D.vb"

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
    '    Code Lines: 5 (33.33%)
    ' Comment Lines: 7 (46.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (20.00%)
    '     File Size: 451 B


    ' Interface IContainer3D
    ' 
    '     Properties: BoxSize, Entity
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Engine.Physics

''' <summary>
''' the 3D volume container of the particles(the 3D counterpart of the 2D
''' <see cref="IContainer(Of T)"/>).
''' </summary>
Public Interface IContainer3D(Of T)

    ReadOnly Property Entity As IReadOnlyCollection(Of T)
    ''' <summary>
    ''' the size(x/y/z extent) of the box volume that holds the fluid.
    ''' </summary>
    Property BoxSize As Vector3

End Interface

