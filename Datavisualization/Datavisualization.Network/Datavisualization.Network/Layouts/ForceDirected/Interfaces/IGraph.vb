'! 
'@file IGraph.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief Graph Interface
'@version 1.0
'
'@section LICENSE
'
'The MIT License (MIT)
'
'Copyright (c) 2013 Woong Gyu La <juhgiyo@gmail.com>
'
'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:
'
'The above copyright notice and this permission notice shall be included in
'all copies or substantial portions of the Software.
'
'THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
'THE SOFTWARE.
'
'@section DESCRIPTION
'
'An Interface for the Graph.
'
'

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph

Namespace Layouts.Interfaces

    Public Interface IGraph
        Sub Clear()
        Function AddNode(iNode As Node) As Node
        Function AddEdge(iEdge As Edge) As Edge
        Sub CreateNodes(iDataList As List(Of NodeData))
        Sub CreateNodes(iNameList As List(Of String))
        Sub CreateEdges(iDataList As List(Of Triple(Of String, String, EdgeData)))
        Sub CreateEdges(iDataList As List(Of KeyValuePair(Of String, String)))
        Function CreateNode(data As NodeData) As Node
        Function CreateNode(name As String) As Node
        Function CreateEdge(iSource As Node, iTarget As Node, Optional iData As EdgeData = Nothing) As Edge
        Function CreateEdge(iSource As String, iTarget As String, Optional iData As EdgeData = Nothing) As Edge
        Function GetEdges(iNode1 As Node, iNode2 As Node) As List(Of Edge)
        Sub RemoveNode(iNode As Node)
        Sub DetachNode(iNode As Node)
        Sub RemoveEdge(iEdge As Edge)
        Sub Merge(iMergeGraph As NetworkGraph)
        Sub FilterNodes(match As Predicate(Of Node))
        Sub FilterEdges(match As Predicate(Of Edge))
        Sub AddGraphListener(iListener As IGraphEventListener)

        ReadOnly Property nodes() As List(Of Node)

        ReadOnly Property edges() As List(Of Edge)
    End Interface
    Public Interface IGraphEventListener
        Sub GraphChanged()
    End Interface
End Namespace