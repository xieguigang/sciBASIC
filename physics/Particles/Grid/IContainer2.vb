#Region "Microsoft.VisualBasic::1e741be778bae45ece5835d854da075b, physics\Particles\Grid\IContainer2.vb"

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

    '   Total Lines: 11
    '    Code Lines: 6 (54.55%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (45.45%)
    '     File Size: 249 B


    ' Interface IContainer
    ' 
    '     Properties: Entity, Height, Width
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.Imaging

Public Interface IContainer(Of T As Layout2D)

    ReadOnly Property Entity As IReadOnlyCollection(Of T)

    ReadOnly Property Width As Double
    ReadOnly Property Height As Double

End Interface

