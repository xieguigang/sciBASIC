﻿#Region "Microsoft.VisualBasic::585f212cba4963436ef4d6475a168109, Data_science\Graph\Network\Edge.vb"

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

    '   Total Lines: 27
    '    Code Lines: 10 (37.04%)
    ' Comment Lines: 12 (44.44%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (18.52%)
    '     File Size: 665 B


    '     Class Edge
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Interface IndexEdge
    ' 
    '         Properties: U, V
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Network

    ''' <summary>
    ''' interaction edge is a tuple of two node vertex object
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Edge(Of T As Node) : Inherits GraphTheory.Edge(Of T)

        Sub New()
        End Sub
    End Class

    Public Interface IndexEdge

        ''' <summary>
        ''' index of the vertex u source
        ''' </summary>
        ''' <returns></returns>
        Property U As Integer
        ''' <summary>
        ''' index of the vertex v target
        ''' </summary>
        ''' <returns></returns>
        Property V As Integer

    End Interface
End Namespace
