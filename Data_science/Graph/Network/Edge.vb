#Region "Microsoft.VisualBasic::6fb4e0a7df136478bb06f391265b1dd0, Data_science\Graph\Network\Edge.vb"

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

'   Total Lines: 30
'    Code Lines: 10 (33.33%)
' Comment Lines: 15 (50.00%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 5 (16.67%)
'     File Size: 734 B


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

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq

Namespace Network

    ''' <summary>
    ''' interaction edge is a tuple of two node vertex object
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Edge(Of T As Node) : Inherits GraphTheory.Edge(Of T)

        Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' an edge link: [u, v]
    ''' </summary>
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

    Public Interface IndexGraph(Of T As IndexEdge)

        Property Nodes As String()
        Property Edges As T()

    End Interface

    Public Module IndexGraphExtensions

        Public Function FromNetwork(Of E As SparseGraph.IInteraction, T As IndexEdge, G As {New, IndexGraph(Of T)})(network As IEnumerable(Of E), edge As Func(Of Integer, Integer, T)) As G
            Dim graph = network.SafeQuery.ToArray
            Dim nodes As Index(Of String) = graph _
                .SelectMany(Iterator Function(i) As IEnumerable(Of String)
                                Yield i.source
                                Yield i.target
                            End Function) _
                .Distinct _
                .OrderBy(Function(id) id) _
                .Indexing
            Dim edges As T() = New T(graph.Length - 1) {}
            Dim j As E

            For i As Integer = 0 To graph.Length - 1
                j = graph(i)
                edges(i) = edge(nodes(j.source), nodes(j.target))
            Next

            Return New G With {
                .Edges = edges,
                .Nodes = nodes.Objects
            }
        End Function
    End Module
End Namespace
