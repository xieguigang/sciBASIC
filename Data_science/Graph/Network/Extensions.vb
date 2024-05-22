#Region "Microsoft.VisualBasic::3abf5cf05a99d8f49916027f3c7895f0, Data_science\Graph\Network\Extensions.vb"

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

    '   Total Lines: 95
    '    Code Lines: 57 (60.00%)
    ' Comment Lines: 23 (24.21%)
    '    - Xml Docs: 73.91%
    ' 
    '   Blank Lines: 15 (15.79%)
    '     File Size: 4.47 KB


    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) ComputeDegreeData, EndPoints, IteratesSubNetworks
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
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
        Public Function EndPoints(Of Node As {New, Network.Node},
                                      Edge As {New, Network.Edge(Of Node)})(network As NetworkGraph(Of Node, Edge)) As (input As Node(), output As Node())
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
        ''' <param name="edgeCut">
        ''' all of the edge weight less than this 
        ''' cutff value will be ignored.
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function IteratesSubNetworks(Of Node As {New, Network.Node},
                                               U As {New, Edge(Of Node)},
                                               Graph As {New, NetworkGraph(Of Node, U)})(
                                               network As NetworkGraph(Of Node, U),
                                               Optional singleNodeAsGraph As Boolean = False,
                                               Optional edgeCut As Double = -1) As IEnumerable(Of Graph)

            Return New SubNetworkComponents(Of Node, U, Graph)(network, singleNodeAsGraph, edgeCut)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ComputeDegreeData(Of T As {New, Network.Node},
                                              Edge As {New, Network.Edge(Of T)})(edges As IEnumerable(Of Edge)) As DegreeData

            Return ComputeDegreeData(edges, Function(l) l.U.label, Function(l) l.V.label)
        End Function

        Public Function ComputeDegreeData(Of Edge)(edges As IEnumerable(Of Edge),
                                                   U As Func(Of Edge, String),
                                                   V As Func(Of Edge, String)) As DegreeData

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

            Return New DegreeData With {.[In] = [in], .Out = out}
        End Function
    End Module
End Namespace
