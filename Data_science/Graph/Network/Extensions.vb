#Region "Microsoft.VisualBasic::0bbf7cb166a7de8b369d54ee8a3958c0, Data_science\Graph\Network\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) ComputeDegreeData, EndPoints, IteratesSubNetworks
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language

Namespace Network

    ''' <summary>
    ''' Graph network api module extensions
    ''' </summary>
    <HideModuleName> Public Module Extensions

        ''' <summary>
        ''' 查找出网络模型之中可能的网络端点
        ''' </summary>
        ''' <param name="network"></param>
        ''' <returns></returns>
        <Extension>
        Public Function EndPoints(Of Node As {New, Network.Node}, Edge As {New, Network.Edge(Of Node)})(network As NetworkGraph(Of Node, Edge)) As (input As Node(), output As Node())
            Dim inputs As New List(Of Node)(network.vertex)
            Dim output As New List(Of Node)(inputs)
            Dim removes = Sub(ByRef list As List(Of Node), getNode As Func(Of Edge, Node))
                              For Each link As Edge In network
                                  Dim n = getNode(link)

                                  If list.IndexOf(n) > -1 Then
                                      Call list.Remove(n)
                                  End If
                              Next
                          End Sub

            ' 对于一个网络端点而言
            ' 对于输入端,则该节点在网络中找不到任何一个指向该输入端的链接
            ' 所以, 如果是target(output)就removes掉, 列表中剩余的节点就都是输入端了
            Call removes(inputs, Function(e) e.V)

            ' 对于一个网络端点而言
            ' 对于输出端,则该节点在网络中找不到任何一个从该节点指出的链接
            ' 所以, 如果是source(inputs)就removes掉, 列表中的剩余节点就都是输出端了
            Call removes(output, Function(e) e.U)  ' 

            Return (inputs.ToArray, output.ToArray)
        End Function

        ''' <summary>
        ''' 枚举出所输入的网络数据模型之中的所有互不相连的子网络
        ''' </summary>
        ''' <param name="network"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function IteratesSubNetworks(Of Node As {New, Network.Node}, U As {New, Network.Edge(Of Node)}, Graph As {New, NetworkGraph(Of Node, U)})(network As NetworkGraph(Of Node, U), Optional singleNodeAsGraph As Boolean = False) As IEnumerable(Of Graph)
            Dim edges As List(Of U) = network.edges.Values.AsList
            Dim popFirstEdge = Function(n As Node) As U
                                   Return edges _
                                      .Where(Function(e) e.U Is n OrElse e.V Is n) _
                                      .FirstOrDefault
                               End Function
            Dim populatedNodes As New List(Of Node)

            Do While edges > 0
                Dim subnetwork As New Graph
                Dim edge As U = edges.First
                Dim list As New List(Of Node)

                Call list.Add(edge.U)
                Call list.Add(edge.V)

                Do While list > 0
                    ' U和V是由edge带进来的，可能会产生重复
                    subnetwork.AddVertex(edge.U)
                    subnetwork.AddVertex(edge.V)
                    subnetwork.AddEdge(edge.U, edge.V)
                    populatedNodes.Add(edge.U)
                    populatedNodes.Add(edge.V)
                    edges.Remove(edge)

                    If -1 = list.IndexOf(edge.U) Then
                        Call list.Add(edge.U)
                    End If
                    If -1 = list.IndexOf(edge.V) Then
                        Call list.Add(edge.V)
                    End If

                    edge = Nothing

                    Do While edge Is Nothing AndAlso list > 0
                        edge = popFirstEdge(list.First)

                        If edge Is Nothing Then
                            ' 当前的这个节点已经没有相连的边了，移除这个节点
                            Call list.RemoveAt(Scan0)
                        End If
                    Loop
                Loop

                Yield subnetwork
            Loop

            If singleNodeAsGraph Then
                Dim removedIndex As Index(Of Node) = populatedNodes.Distinct.Indexing
                Dim [single] As New Graph

                For Each v As Node In network.vertex.Where(Function(n) removedIndex(n) = -1)
                    [single] = New Graph
                    [single].AddVertex(v)

                    Yield [single]
                Next
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ComputeDegreeData(Of T As {New, Network.Node}, Edge As {New, Network.Edge(Of T)})(edges As IEnumerable(Of Edge)) As ([in] As Dictionary(Of String, Integer), out As Dictionary(Of String, Integer))
            Return ComputeDegreeData(edges, Function(l) l.U.label, Function(l) l.V.label)
        End Function

        Public Function ComputeDegreeData(Of Edge)(edges As IEnumerable(Of Edge),
                                                   U As Func(Of Edge, String),
                                                   V As Func(Of Edge, String)) As ([in] As Dictionary(Of String, Integer), out As Dictionary(Of String, Integer))

            Dim [in] As New Dictionary(Of String, Integer)
            Dim out As New Dictionary(Of String, Integer)
            Dim count = Sub(node$, ByRef table As Dictionary(Of String, Integer))
                            If table.ContainsKey(node) Then
                                table(node) += 1
                            Else
                                table.Add(node, 1)
                            End If
                        End Sub
            Dim countIn = Sub(node$) Call count(node, [in])
            Dim countOut = Sub(node$) Call count(node, out)

            For Each link As Edge In edges
                Call countIn(U(link))
                Call countOut(V(link))
            Next

            Return ([in], out)
        End Function
    End Module
End Namespace
