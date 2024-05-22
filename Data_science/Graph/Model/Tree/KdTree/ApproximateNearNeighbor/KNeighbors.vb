#Region "Microsoft.VisualBasic::abf039bdc3e1df882b2a2b84c3b2f4f8, Data_science\Graph\Model\Tree\KdTree\ApproximateNearNeighbor\KNeighbors.vb"

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

    '   Total Lines: 22
    '    Code Lines: 12 (54.55%)
    ' Comment Lines: 6 (27.27%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (18.18%)
    '     File Size: 618 B


    '     Structure KNeighbors
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace KdTree.ApproximateNearNeighbor

    ''' <summary>
    ''' k neighbors of a item row
    ''' </summary>
    Public Structure KNeighbors

        ''' <summary>
        ''' the score cutoff maybe applied, so the neighbors size may smaller than the given k
        ''' </summary>
        Dim size As Integer
        Dim indices As Integer()
        Dim weights As Double()

        Sub New(size As Integer, indices As Integer(), weights As Double())
            Me.size = size
            Me.indices = indices
            Me.weights = weights
        End Sub

    End Structure
End Namespace
