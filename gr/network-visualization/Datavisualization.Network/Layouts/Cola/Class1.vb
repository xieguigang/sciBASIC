#Region "Microsoft.VisualBasic::719c6bf3a315c28946227601f3c07a9a, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Class1.vb"

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

    '     Class Leaf
    ' 
    ' 
    ' 
    '     Class ProjectionGroup
    ' 
    ' 
    ' 
    '     Class [Event]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports number = System.Double

Namespace Layouts.Cola

    Public Class Leaf
        Public bounds As Rectangle2D
        Public variable As Variable
    End Class

    Public Class ProjectionGroup
        Public bounds As Rectangle2D
        Public padding As Number
        Public stiffness As Number
        Public leaves As Leaf()
        Public groups As ProjectionGroup()
        Public minVar As Variable
        Public maxVar As Variable
    End Class

    Public Class [Event]

        Public isOpen As Boolean
        Public v As Node
        Public pos As number

        Sub New(isOpen As Boolean, v As Node, pos As number)
            Me.isOpen = isOpen
            Me.v = v
            Me.pos = pos
        End Sub

    End Class
End Namespace
