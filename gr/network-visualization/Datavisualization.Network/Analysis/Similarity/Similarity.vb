#Region "Microsoft.VisualBasic::4d46b6290b7b235cde7f3ec13016f74e, gr\network-visualization\Datavisualization.Network\Analysis\Similarity\Similarity.vb"

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

    '   Total Lines: 111
    '    Code Lines: 75
    ' Comment Lines: 16
    '   Blank Lines: 20
    '     File Size: 4.36 KB


    '     Module Similarity
    ' 
    '         Function: AllNodeTypes, CosOfNodeTuple, GraphSimilarity, NodeSimilarity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis.SimilarityImpl
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Analysis

    Public Module Similarity

        Public Function GraphSimilarity(x As NetworkGraph, y As NetworkGraph,
                                        Optional cutoff# = 0.85,
                                        Optional classEquivalent As Func(Of String, String, Double) = Nothing,
                                        Optional topologyCos As Boolean = False) As Double

            ' JaccardIndex (intersects / union) -> highly similar / (dis-similar + highly similar)
            Dim similar%
            Dim top#
            Dim cos#

            If classEquivalent Is Nothing Then
                classEquivalent = Function(a, b) If(a <> b, 0, 1)
            End If

            ' 20191231
            ' X should always greater than Y in graph size
            ' or vertex count - similar may be negative value
            ' the negative value will cause union value smaller 
            ' than similar count, result an invalid cos similar
            ' value which is greater than 1
            If x.size.vertex > y.size.vertex Then
                Dim tmp = x

                x = y
                y = tmp
            End If

            For Each a As Node In x.vertex
                top = -99999

                For Each b As Node In y.vertex
                    cos = Similarity.NodeSimilarity(a, b, classEquivalent, topologyCos)

                    If cos > top Then
                        top = cos
                    End If
                Next

                If top >= cutoff Then
                    similar += 1
                End If
            Next

            Dim union As Integer = (similar + (x.vertex.Count - similar) + (y.vertex.Count - similar))
            Dim jaccardIndex As Double = similar / union

            Return jaccardIndex
        End Function

        ''' <summary>
        ''' Compare node similarity between two network graph
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Function NodeSimilarity(a As Node, b As Node,
                                       classEquivalent As Func(Of String, String, Double),
                                       Optional topologyCos As Boolean = True) As Double
            ' consider the node itself
            ' if the two node is not in same datatype, then returns not similar
            Dim class1 As String = Scripting.ToString(a.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE))
            Dim class2 As String = Scripting.ToString(b.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE))
            Dim score As Double = classEquivalent(class1, class2)

            If score = 0.0 Then
                Return 0
            Else
                Return CosOfNodeTuple(a, b, topologyCos) * score
            End If
        End Function

        Public Function CosOfNodeTuple(a As Node, b As Node, Optional topologyCos As Boolean = True) As Double
            Dim cos As Double = ClassConnectivity.MeasureCosValue(a, b)

            If topologyCos Then
                Return cos * GraphTopology.TopologyCos(a, b) * GraphTopology.VertexDistanceCos(a, b)
            Else
                Return cos
            End If
        End Function

        <Extension>
        Public Function AllNodeTypes(v As Node) As IEnumerable(Of String)
            If v.adjacencies Is Nothing Then
                ' 孤立节点
                Return {}
            End If

            Return v.adjacencies _
                .EnumerateAllEdges _
                .Select(Function(e)
                            If e.U Is v Then
                                Return e.V.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)
                            Else
                                Return e.U.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)
                            End If
                        End Function) _
                .Select(AddressOf Scripting.ToString)
        End Function

    End Module
End Namespace
