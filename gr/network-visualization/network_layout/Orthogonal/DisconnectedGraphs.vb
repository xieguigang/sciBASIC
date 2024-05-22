#Region "Microsoft.VisualBasic::877ecb4f90e41ecfa0249870246f9737, gr\network-visualization\network_layout\Orthogonal\DisconnectedGraphs.vb"

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

    '   Total Lines: 122
    '    Code Lines: 92 (75.41%)
    ' Comment Lines: 12 (9.84%)
    '    - Xml Docs: 25.00%
    ' 
    '   Blank Lines: 18 (14.75%)
    '     File Size: 5.05 KB


    '     Class DisconnectedGraphs
    ' 
    '         Function: findDisconnectedGraphs, mergeDisconnectedEmbeddingsSideBySide, subgraph
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ListExtensions

Namespace Orthogonal


    ' 
    ' 	 * To change this license header, choose License Headers in Project Properties.
    ' 	 * To change this template file, choose Tools | Templates
    ' 	 * and open the template in the editor.
    ' 	 

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class DisconnectedGraphs
        Public Shared Function findDisconnectedGraphs(graph As Integer()()) As IList(Of IList(Of Integer))
            Dim n = graph.Length
            Dim graphs As IList(Of IList(Of Integer)) = New List(Of IList(Of Integer))()
            Dim closedAccum As IList(Of Integer) = New List(Of Integer)()
            Do
                Dim open As List(Of Integer) = New List(Of Integer)()
                Dim closed As List(Of Integer) = New List(Of Integer)()

                For i = 0 To n - 1
                    If Not closedAccum.Contains(i) Then
                        open.Add(i)
                        Exit For
                    End If
                Next

                While open.Count > 0
                    Dim current = open.PopAt(0)
                    For i = 0 To n - 1
                        If graph(current)(i) <> 0 OrElse graph(i)(current) <> 0 Then
                            If Not closed.Contains(i) AndAlso Not open.Contains(i) Then
                                open.Add(i)
                            End If
                        End If
                    Next
                    closed.Add(current)
                End While
                If closed.Count > 0 Then
                    graphs.Add(closed)
                End If
                closed.Sort()
                CType(closedAccum, List(Of Integer)).AddRange(closed)
            Loop While closedAccum.Count < n

            Return graphs
        End Function


        Public Shared Function subgraph(graph As Integer()(), vertexIndexes As IList(Of Integer)) As Integer()()
            Dim n2 = vertexIndexes.Count

            Dim graph2 = RectangularArray.Matrix(Of Integer)(n2, n2)
            For i2 = 0 To n2 - 1
                Dim i = vertexIndexes(i2)
                For j2 = 0 To n2 - 1
                    Dim j = vertexIndexes(j2)
                    graph2(i2)(j2) = graph(i)(j)
                Next
            Next
            Return graph2
        End Function

        Public Shared Function mergeDisconnectedEmbeddingsSideBySide(disconnectedEmbeddings As IList(Of OrthographicEmbeddingResult),
                                                                     vertexIndexes As IList(Of IList(Of Integer)),
                                                                     separation As Double) As OrthographicEmbeddingResult
            If disconnectedEmbeddings.Count = 1 Then
                Return disconnectedEmbeddings(0)
            Else
                Dim embeddingSizes = New Integer(disconnectedEmbeddings.Count - 1) {}
                Dim n = 0

                For i = 0 To disconnectedEmbeddings.Count - 1
                    If Not disconnectedEmbeddings(i) Is Nothing Then
                        embeddingSizes(i) = disconnectedEmbeddings(i).nodeIndexes.Length
                        n += embeddingSizes(i)
                    End If
                Next

                Dim aggregated As OrthographicEmbeddingResult = New OrthographicEmbeddingResult(n)

                Dim startIndex = 0
                Dim startX As Double = 0
                Dim nextStartX As Double = 0
                For i = 0 To disconnectedEmbeddings.Count - 1
                    Dim er As OrthographicEmbeddingResult = disconnectedEmbeddings(i)
                    Dim vi = vertexIndexes(i)
                    For j = 0 To embeddingSizes(i) - 1
                        ' node indexes:
                        If er.nodeIndexes(j) >= 0 Then
                            aggregated.nodeIndexes(startIndex + j) = vi(er.nodeIndexes(j))
                        Else
                            aggregated.nodeIndexes(startIndex + j) = -1
                        End If

                        ' coordinates:
                        If er.x(j) + startX > nextStartX Then
                            nextStartX = er.x(j) + startX
                        End If
                        aggregated.x(startIndex + j) = startX + er.x(j)
                        aggregated.y(startIndex + j) = er.y(j)

                        ' edges:
                        For k = 0 To embeddingSizes(i) - 1
                            aggregated.edges(startIndex + j)(startIndex + k) = er.edges(j)(k)
                        Next
                    Next
                    startX = nextStartX + separation
                    startIndex += embeddingSizes(i)
                Next

                Return aggregated
            End If
        End Function
    End Class

End Namespace
