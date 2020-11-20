#Region "Microsoft.VisualBasic::632bc8cf627486778c8aa40b8d0d0ec0, Data_science\Graph\Model\Tree\KdTree\Node.vb"

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

    '     Class Node
    ' 
    '         Properties: dimension, left, obj, parent, right
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace KdTree

    Public Class Node : Inherits Vertex

        Public Property obj As Object
        Public Property left As Node
        Public Property right As Node
        Public Property parent As Node
        Public Property dimension As Integer

        Sub New(obj As Object, dimension%, parent As Node)
            Me.obj = obj
            Me.dimension = dimension
            Me.parent = parent
        End Sub

    End Class
End Namespace
