#Region "Microsoft.VisualBasic::35eea96e3adbb5fd72d68482545a1e08, Data_science\Graph\Network\NetworkGraph.vb"

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

'     Class NetworkGraph
' 
'         Constructor: (+2 Overloads) Sub New
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Network

    Public Class NetworkGraph(Of Node As {New, Network.Node}, Edge As {New, Network.Edge(Of Node)}) : Inherits Graph(Of Node, Edge, NetworkGraph(Of Node, Edge))

        Sub New()
        End Sub

        ''' <summary>
        ''' Network model copy
        ''' </summary>
        ''' <param name="nodes"></param>
        ''' <param name="edges"></param>
        Sub New(nodes As IEnumerable(Of Node), edges As IEnumerable(Of Edge))
            Me.vertices = New Dictionary(Of Node)(nodes)
            Me.edges = New Dictionary(Of Edge)(edges)
        End Sub
    End Class
End Namespace
