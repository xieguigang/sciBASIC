#Region "Microsoft.VisualBasic::5abb5b209986a8c9c104bdea1eb6c4a9, gr\network-visualization\Network.IO.Extensions\graphology.vb"

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

    '   Total Lines: 39
    '    Code Lines: 27 (69.23%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (30.77%)
    '     File Size: 899 B


    '     Class node
    ' 
    '         Properties: color, id, label, size, x
    '                     y
    ' 
    '     Class edge
    ' 
    '         Properties: color, id, size, source, target
    ' 
    '     Class graph
    ' 
    '         Properties: edges, nodes
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace graphology

    Public Class node

        Public Property id As String
        Public Property label As String
        Public Property x As Double
        Public Property y As Double
        Public Property size As Double
        Public Property color As String

    End Class

    Public Class edge

        Public Property id As String
        Public Property source As String
        Public Property target As String
        Public Property size As Double
        Public Property color As String

    End Class

    Public Class graph

        Public Property nodes As node()
        Public Property edges As edge()

        Sub New()
        End Sub

        Sub New(nodes As IEnumerable(Of node), edges As IEnumerable(Of edge))
            _nodes = nodes.ToArray
            _edges = edges.ToArray
        End Sub

    End Class

End Namespace

