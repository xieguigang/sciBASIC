#Region "Microsoft.VisualBasic::041a8a99fe50079869dfe67d5d2b8bb1, gr\network-visualization\Datavisualization.Network\Graph\Model\Node.vb"

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

    '   Total Lines: 231
    '    Code Lines: 117 (50.65%)
    ' Comment Lines: 93 (40.26%)
    '    - Xml Docs: 51.61%
    ' 
    '   Blank Lines: 21 (9.09%)
    '     File Size: 8.53 KB


    '     Class Node
    ' 
    '         Properties: adjacencies, data, directedVertex, pinned, text
    '                     visited
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: adjacentTo, Clone, EnumerateAdjacencies, (+2 Overloads) Equals, GetHashCode
    '                   ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'! 
'@file Node.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief Node Interface
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
'An Interface for the Node Class.
'
'

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization

Namespace Graph

    ''' <summary>
    ''' <see cref="Node.Label"/> -> <see cref="INamedValue.Key"/>
    ''' </summary>
    Public Class Node : Inherits GraphTheory.Network.Node
        Implements INamedValue
        Implements IGraphValueContainer(Of NodeData)
        Implements ICloneable(Of Node)

        Public Property data As NodeData Implements IGraphValueContainer(Of NodeData).data

        ''' <summary>
        ''' Get all of the edge collection that connect to current node object
        ''' </summary>
        ''' <returns></returns>
        Public Property adjacencies As AdjacencySet(Of Edge)
        Public Property directedVertex As DirectedVertex

        ''' <summary>
        ''' 这个节点是被钉住的？在进行布局计算的时候，钉住的节点将不会更新位置
        ''' </summary>
        ''' <returns></returns>
        Public Property pinned As Boolean
        Public Property visited As Boolean

        Public ReadOnly Property text As String
            Get
                Return data.label
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>
        ''' returns nothing if the node meta <see cref="data"/> is nothing orelse 
        ''' node data contains no such given key name.
        ''' </returns>
        Default Public ReadOnly Property Metadata(name As String) As String
            Get
                If data Is Nothing Then
                    Return Nothing
                Else
                    Return data.ItemValue(name)
                End If
            End Get
        End Property

        ''' <summary>
        ''' 在这里是用的是unique id进行初始化，对于Display title则可以在<see cref="NodeData.label"/>属性上面设置
        ''' </summary>
        ''' <param name="iId"></param>
        ''' <param name="iData"></param>
        Public Sub New(iId As String, Optional iData As NodeData = Nothing)
            If iData IsNot Nothing Then
                data = iData.Clone
            End If

            label = iId
            pinned = False

            If iId IsNot Nothing Then
                adjacencies = New AdjacencySet(Of Edge)(iId)
            End If
        End Sub

        Sub New()
            Call Me.New(Nothing, Nothing)
        End Sub

        Public Overrides Function GetHashCode() As Integer
            Return label.GetHashCode()
        End Function

        ''' <summary>
        ''' 枚举出所有的与当前节点直接相邻接的节点列表
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function EnumerateAdjacencies() As IEnumerable(Of Node)
            For Each edge As Edge In adjacencies.EnumerateAllEdges
                If edge.U Is Me Then
                    Yield edge.V
                Else
                    Yield edge.U
                End If
            Next
        End Function

        ''' <summary>
        ''' Indicates if the node is adjacent to the node specified by id
        ''' </summary>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Function adjacentTo(node As Node) As Boolean
            Return node.label Like adjacencies
        End Function

        Public Overrides Function ToString() As String
            If Not data Is Nothing AndAlso Not data.label.StringEmpty Then
                Return $"{label} ({data.label})"
            Else
                Return label
            End If
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            ' If parameter is null return false.
            If obj Is Nothing Then
                Return False
            Else
                ' If parameter cannot be cast to Point return false.
                Return Equals(p:=TryCast(obj, Node))
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Equals(p As Node) As Boolean
            ' If parameter is null return false:
            If p Is Nothing Then
                Return False
            Else
                ' Return true if the fields match:
                Return (label = p.label)
            End If
        End Function

        ''' <summary>
        ''' check vertex node equivalent via the <see cref="Node.label"/> equivalent.
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Shared Operator =(a As Node, b As Node) As Boolean
            ' If one is null, but not both, return false.
            If a Is Nothing OrElse b Is Nothing Then
                Return False
            End If

            ' If both are null, or both are same instance, return true.
            If a Is b Then
                Return True
            Else
                Return a.Equals(p:=b)
            End If
        End Operator

        ''' <summary>
        ''' check vertex node un-equivalent via the <see cref="Node.label"/> un-equivalent
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(a As Node, b As Node) As Boolean
            Return Not (a = b)
        End Operator

        ''' <summary>
        ''' make data clone of current graph vertex node
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Clone() As Node Implements ICloneable(Of Node).Clone
            Return New Node With {
                .ID = ID,
                .label = label,
                .degree = degree,
                .pinned = pinned,
                .visited = visited,
                .adjacencies = If(adjacencies Is Nothing, New AdjacencySet(Of Edge)(), adjacencies.Clone),
                .directedVertex = New DirectedVertex(label),
                .data = New NodeData With {
                    .color = data.color,
                    .label = data.label,
                    .force = data.force,
                    .initialPostion = data.initialPostion,
                    .mass = data.mass,
                    .neighbours = data.neighbours,
                    .origID = data.origID,
                    .size = data.size.SafeQuery.ToArray,
                    .weights = data.weights.SafeQuery.ToArray,
                    .Properties = New Dictionary(Of String, String)(data.Properties),
                    .betweennessCentrality = data.betweennessCentrality
                }
            }
        End Function
    End Class
End Namespace
