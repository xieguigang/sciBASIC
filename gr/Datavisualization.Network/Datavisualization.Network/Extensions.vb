#Region "Microsoft.VisualBasic::ea27b60c8ce2a290ac3e5d558873db4a, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language

Namespace FindPath

    Public Module Extensions

        ''' <summary>
        ''' 查找出网络模型之中可能的网络端点
        ''' </summary>
        ''' <param name="network"></param>
        ''' <returns></returns>
        <Extension>
        Public Function EndPoints(network As NetworkGraph) As (input As Node(), output As Node())
            Dim inputs As New List(Of Node)(network.nodes)
            Dim output As New List(Of Node)(network.nodes)
            Dim removes = Sub(ByRef list As List(Of Node), getNode As Func(Of Edge, Node))
                              For Each edge As Edge In network.edges
                                  Dim node = getNode(edge)

                                  If list.IndexOf(node) > -1 Then
                                      Call list.Remove(node)
                                  End If
                              Next
                          End Sub

            Call removes(inputs, Function(edge) edge.V)  ' 如果是target(output)就removes掉
            Call removes(output, Function(edge) edge.U)  ' 如果是source(inputs)就removes掉

            Return (inputs, output)
        End Function

        ''' <summary>
        ''' 枚举出所输入的网络数据模型之中的所有互不相连的子网络
        ''' </summary>
        ''' <param name="network"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function IteratesSubNetworks(network As NetworkGraph) As IEnumerable(Of NetworkGraph)
            Dim popEdge = Function(node As Node) As Edge
                              Return network _
                                  .edges _
                                  .Where(Function(e) e.U Is node OrElse e.V Is node) _
                                  .FirstOrDefault
                          End Function

            Do While network.edges > 0
                Dim subnetwork As New NetworkGraph
                Dim edge As Edge = network.edges.First
                Dim list As New List(Of Node)

                Call list.Add(edge.U)
                Call list.Add(edge.V)

                Do While list > 0
                    subnetwork.AddNode(edge.U)
                    subnetwork.AddNode(edge.V)
                    subnetwork.AddEdge(edge)
                    network.edges.Remove(edge)

                    If -1 = list.IndexOf(edge.U) Then
                        Call list.Add(edge.U)
                    End If
                    If -1 = list.IndexOf(edge.V) Then
                        Call list.Add(edge.V)
                    End If

                    edge = Nothing

                    Do While edge Is Nothing AndAlso list > 0
                        edge = popEdge(list.First)

                        If edge Is Nothing Then
                            ' 当前的这个节点已经没有相连的边了，移除这个节点
                            Call list.RemoveAt(Scan0)
                        End If
                    Loop
                Loop

                Yield subnetwork
            Loop
        End Function
    End Module
End Namespace
