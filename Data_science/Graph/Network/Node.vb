﻿#Region "Microsoft.VisualBasic::9508b735362dc18dd9b06988b7d1d3c8, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Graph//Network/Node.vb"

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
    '    Code Lines: 5
    ' Comment Lines: 7
    '   Blank Lines: 3
    '     File Size: 370 B


    '     Class Node
    ' 
    '         Properties: degree
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Network

    ''' <summary>
    ''' A network node model
    ''' </summary>
    Public Class Node : Inherits Vertex

        ''' <summary>
        ''' Node connection counts: [point_to_this_node, point_from_this_node]
        ''' </summary>
        ''' <returns></returns>
        Public Property degree As (In%, Out%)

    End Class
End Namespace
