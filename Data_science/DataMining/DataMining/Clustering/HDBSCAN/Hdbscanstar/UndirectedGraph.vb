#Region "Microsoft.VisualBasic::4d190819505c38c0b541e1df742ef408, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Hdbscanstar\UndirectedGraph.vb"

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

    '   Total Lines: 175
    '    Code Lines: 108
    ' Comment Lines: 36
    '   Blank Lines: 31
    '     File Size: 7.44 KB


    '     Class UndirectedGraph
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetEdgeListForVertex, GetEdgeWeightAtIndex, GetFirstVertexAtIndex, GetNumEdges, GetNumVertices
    '                   GetSecondVertexAtIndex, Partition, SelectPivotIndex
    ' 
    '         Sub: QuicksortByEdgeWeight, SwapEdges
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic

Namespace HDBSCAN.Hdbscanstar
    ''' <summary>
    ''' An undirected graph, with weights assigned to each edge.
    ''' Vertices in the graph are 0 indexed.
    ''' </summary>
    Public Class UndirectedGraph
        Private ReadOnly _numVertices As Integer
        Private ReadOnly _verticesA As Integer()
        Private ReadOnly _verticesB As Integer()
        Private ReadOnly _edgeWeights As Double()
        Private ReadOnly _edges As List(Of Integer)()

        ''' <summary>
        ''' Constructs a new UndirectedGraph, including creating an edge list for each vertex from the 
        ''' vertex arrays.  For an index i, verticesA[i] and verticesB[i] share an edge with weight
        ''' edgeWeights[i].
        ''' </summary>
        ''' <param name="numVertices">The number of vertices in the graph (indexed 0 to numVertices-1)</param>
        ''' <param name="verticesA">An array of vertices corresponding to the array of edges</param>
        ''' <param name="verticesB">An array of vertices corresponding to the array of edges</param>
        ''' <param name="edgeWeights">An array of edges corresponding to the arrays of vertices</param>
        Public Sub New(numVertices As Integer, verticesA As Integer(), verticesB As Integer(), edgeWeights As Double())
            _numVertices = numVertices
            _verticesA = verticesA
            _verticesB = verticesB
            _edgeWeights = edgeWeights
            _edges = New List(Of Integer)(numVertices - 1) {}

            For i = 0 To _edges.Length - 1
                _edges(i) = New List(Of Integer)(1 + edgeWeights.Length / numVertices)
            Next

            For i = 0 To edgeWeights.Length - 1
                Dim vertexOne = _verticesA(i)
                Dim vertexTwo = _verticesB(i)

                _edges(vertexOne).Add(vertexTwo)

                If vertexOne <> vertexTwo Then _edges(vertexTwo).Add(vertexOne)
            Next
        End Sub

        ''' <summary>
        ''' Quicksorts the graph by edge weight in descending order.
        ''' This quicksort implementation is iterative and in-place.
        ''' </summary>
        Public Sub QuicksortByEdgeWeight()
            If _edgeWeights.Length <= 1 Then Return

            Dim startIndexStack = New Integer(_edgeWeights.Length / 2 - 1) {}
            Dim endIndexStack = New Integer(_edgeWeights.Length / 2 - 1) {}

            startIndexStack(0) = 0
            endIndexStack(0) = _edgeWeights.Length - 1

            Dim stackTop = 0

            While stackTop >= 0
                Dim startIndex = startIndexStack(stackTop)
                Dim endIndex = endIndexStack(stackTop)
                stackTop -= 1

                Dim pivotIndex = SelectPivotIndex(startIndex, endIndex)
                pivotIndex = Partition(startIndex, endIndex, pivotIndex)

                If pivotIndex > startIndex + 1 Then
                    startIndexStack(stackTop + 1) = startIndex
                    endIndexStack(stackTop + 1) = pivotIndex - 1
                    stackTop += 1
                End If

                If pivotIndex < endIndex - 1 Then
                    startIndexStack(stackTop + 1) = pivotIndex + 1
                    endIndexStack(stackTop + 1) = endIndex
                    stackTop += 1
                End If
            End While
        End Sub

        ''' <summary>
        ''' Returns a pivot index by finding the median of edge weights between the startIndex, endIndex,
        ''' and middle.
        ''' </summary>
        ''' <param name="startIndex">The lowest index from which the pivot index should come</param>
        ''' <param name="endIndex">The highest index from which the pivot index should come</param>
        ''' <returns>A pivot index</returns>
        Private Function SelectPivotIndex(startIndex As Integer, endIndex As Integer) As Integer
            If startIndex - endIndex <= 1 Then Return startIndex

            Dim first = _edgeWeights(startIndex)
            Dim middle = _edgeWeights(startIndex + (endIndex - startIndex) / 2)
            Dim last = _edgeWeights(endIndex)

            If first <= middle Then
                If middle <= last Then Return startIndex + (endIndex - startIndex) / 2

                If last >= first Then Return endIndex

                Return startIndex
            End If

            If first <= last Then Return startIndex

            If last >= middle Then Return endIndex

            Return startIndex + (endIndex - startIndex) / 2
        End Function

        ''' <summary>
        ''' Partitions the array in the interval [startIndex, endIndex] around the value at pivotIndex.
        ''' </summary>
        ''' <param name="startIndex">The lowest index to  partition</param>
        ''' <param name="endIndex">The highest index to partition</param>
        ''' <param name="pivotIndex">The index of the edge weight to partition around</param>
        ''' <returns>The index position of the pivot edge weight after the partition</returns>
        Private Function Partition(startIndex As Integer, endIndex As Integer, pivotIndex As Integer) As Integer
            Dim pivotValue = _edgeWeights(pivotIndex)
            SwapEdges(pivotIndex, endIndex)
            Dim lowIndex = startIndex
            For i = startIndex To endIndex - 1
                If _edgeWeights(i) < pivotValue Then
                    SwapEdges(i, lowIndex)
                    lowIndex += 1
                End If
            Next
            SwapEdges(lowIndex, endIndex)
            Return lowIndex
        End Function

        ''' <summary>
        ''' Swaps the vertices and edge weights between two index locations in the graph.
        ''' </summary>
        ''' <param name="indexOne">The first index location</param>
        ''' <param name="indexTwo">The second index location</param>
        Private Sub SwapEdges(indexOne As Integer, indexTwo As Integer)
            If indexOne = indexTwo Then Return

            Dim tempVertexA = _verticesA(indexOne)
            Dim tempVertexB = _verticesB(indexOne)
            Dim tempEdgeDistance = _edgeWeights(indexOne)
            _verticesA(indexOne) = _verticesA(indexTwo)
            _verticesB(indexOne) = _verticesB(indexTwo)
            _edgeWeights(indexOne) = _edgeWeights(indexTwo)
            _verticesA(indexTwo) = tempVertexA
            _verticesB(indexTwo) = tempVertexB
            _edgeWeights(indexTwo) = tempEdgeDistance
        End Sub

        Public Function GetNumVertices() As Integer
            Return _numVertices
        End Function

        Public Function GetNumEdges() As Integer
            Return _edgeWeights.Length
        End Function

        Public Function GetFirstVertexAtIndex(index As Integer) As Integer
            Return _verticesA(index)
        End Function

        Public Function GetSecondVertexAtIndex(index As Integer) As Integer
            Return _verticesB(index)
        End Function

        Public Function GetEdgeWeightAtIndex(index As Integer) As Double
            Return _edgeWeights(index)
        End Function

        Public Function GetEdgeListForVertex(vertex As Integer) As List(Of Integer)
            Return _edges(vertex)
        End Function
    End Class
End Namespace
