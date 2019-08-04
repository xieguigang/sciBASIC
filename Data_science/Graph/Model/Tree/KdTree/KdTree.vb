#Region "Microsoft.VisualBasic::ad89000b9f382eec97cff3a64a305bc5, Data_science\Graph\Model\Tree\KdTree\KdTree.vb"

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

    '     Class KdTree
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: buildTree
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace KdTree

    Public Class KdTree

        Dim dimensions As Integer()
        Dim points As Object()
        Dim metric As Object

        Sub New(points As Object(), metric As Object, dimensions As Integer())
            Me.points = points
            Me.metric = metric
            Me.dimensions = dimensions
        End Sub

        Private Function buildTree(points As Object(), depth As Integer, parent As Node) As Node
            Dim [dim] = depth Mod dimensions.Length
            Dim median As Double
            Dim node As Node

            If points.Length = 0 Then
                Return Nothing
            ElseIf points.Length = 1 Then
                Return New Node(points(Scan0), [dim], parent)
            End If

            'points.Sort(Function(a, b)
            '                Return a(dimensions([dim])) - b(dimensions([dim]))
            '            End Function)
        End Function
    End Class
End Namespace
