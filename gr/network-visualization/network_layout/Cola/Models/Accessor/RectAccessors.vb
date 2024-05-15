#Region "Microsoft.VisualBasic::904bd442daa0b7c7a219d3f4720333c9, gr\network-visualization\network_layout\Cola\Models\Accessor\RectAccessors.vb"

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
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 582 B


    '     Class RectAccessors
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Imaging.LayoutModel

Namespace Cola

    Public Class RectAccessors
        Public getCentre As Func(Of Rectangle2D, Double)
        Public getOpen As Func(Of Rectangle2D, Double)
        Public getClose As Func(Of Rectangle2D, Double)
        Public getSize As Func(Of Rectangle2D, Double)
        Public makeRect As Func(Of Double, Double, Double, Double, Rectangle2D)
        Public findNeighbours As Action(Of Node, RBTree(Of Integer, Node))
    End Class
End Namespace
