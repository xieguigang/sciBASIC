#Region "Microsoft.VisualBasic::cf8253323cfbf97c2ded28c572c4a1f3, Data_science\DataMining\BinaryTree\AffinityPropagation\Graph.vb"

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

    '   Total Lines: 31
    '    Code Lines: 22
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 930 B


    '     Class Graph
    ' 
    '         Properties: VerticesCount
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace AffinityPropagation

    Public Class Graph

        Public ReadOnly Property VerticesCount As Integer

        Public SimMatrixElementsCount As Integer

        Public outEdges As Edge()()
        Public inEdges As Edge()()
        Public Edges As Edge()

        Public Sub New(vertices As Integer)
            VerticesCount = If(vertices < 0, 0, vertices)
            SimMatrixElementsCount = (VerticesCount - 1) * VerticesCount + VerticesCount

            outEdges = New Edge(VerticesCount - 1)() {}
            inEdges = New Edge(VerticesCount - 1)() {}
            Edges = New Edge(SimMatrixElementsCount - 1) {}

            Dim i = 0

            While i < VerticesCount
                outEdges(i) = New Edge(VerticesCount - 1) {}
                inEdges(i) = New Edge(VerticesCount - 1) {}
                i += 1
            End While
        End Sub

    End Class
End Namespace
