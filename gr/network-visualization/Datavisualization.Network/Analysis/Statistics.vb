#Region "Microsoft.VisualBasic::76e06fefc868df8b28813e9bd9e20224, gr\network-visualization\Datavisualization.Network\Analysis\Statistics.vb"

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

    '     Module Statistics
    ' 
    '         Function: ComputeDegreeData, ComputeNodeDegrees, Sum
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract
Imports Microsoft.VisualBasic.Linq
Imports names = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic.NamesOf

Namespace Analysis

    Public Module Statistics

        <Extension>
        Public Function ComputeDegreeData(Of T As IInteraction)(edges As IEnumerable(Of T)) As ([in] As Dictionary(Of String, Integer), out As Dictionary(Of String, Integer))
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

            For Each edge As T In edges
                Call countIn(edge.target)
                Call countOut(edge.source)
            Next

            Return ([in], out)
        End Function

        <Extension>
        Public Function Sum(degrees As ([in] As Dictionary(Of String, Integer), out As Dictionary(Of String, Integer))) As Dictionary(Of String, Integer)
            Dim degreeValue As New Dictionary(Of String, Integer)(degrees.in)

            For Each node In degrees.out
                degreeValue(node.Key) += degreeValue(node.Key) + node.Value
            Next

            Return degreeValue
        End Function

        ''' <summary>
        ''' 这个函数计算网络的节点的degree，然后将degree数据写入节点的同时，通过字典返回给用户
        ''' </summary>
        ''' <param name="net"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ComputeNodeDegrees(ByRef net As NetworkGraph) As Dictionary(Of String, Integer)
            Dim connectNodes = net _
                .graphEdges _
                .Select(Function(link) {link.U.Label, link.V.Label}) _
                .IteratesALL _
                .GroupBy(Function(id) id) _
                .ToDictionary(Function(ID) ID.Key,
                              Function(list)
                                  Return list.Count
                              End Function)
            Dim d%

            With net.graphEdges.ComputeDegreeData
                For Each node In net.vertex

                    If Not connectNodes.ContainsKey(node.Label) Then
                        ' 这个节点是孤立的节点，度为零
                        node.data.Add(names.REFLECTION_ID_MAPPING_DEGREE, 0)
                        node.data.Add(names.REFLECTION_ID_MAPPING_DEGREE_IN, 0)
                        node.data.Add(names.REFLECTION_ID_MAPPING_DEGREE_OUT, 0)

                    Else
                        d = connectNodes(node.Label)
                        node.data.Add(names.REFLECTION_ID_MAPPING_DEGREE, d)

                        If .in.ContainsKey(node.Label) Then
                            d = .in(node.Label)
                            node.data.Add(names.REFLECTION_ID_MAPPING_DEGREE_IN, d)
                        End If
                        If .out.ContainsKey(node.Label) Then
                            d = .out(node.Label)
                            node.data.Add(names.REFLECTION_ID_MAPPING_DEGREE_OUT, d)
                        End If
                    End If
                Next
            End With

            Return connectNodes
        End Function
    End Module
End Namespace
