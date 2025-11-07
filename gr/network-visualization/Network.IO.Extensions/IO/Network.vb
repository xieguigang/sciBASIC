#Region "Microsoft.VisualBasic::0c743ea783cb6fed433036c92b290897, gr\network-visualization\Network.IO.Extensions\IO\Network.vb"

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

'   Total Lines: 227
'    Code Lines: 142 (62.56%)
' Comment Lines: 54 (23.79%)
'    - Xml Docs: 96.30%
' 
'   Blank Lines: 31 (13.66%)
'     File Size: 8.20 KB


'     Class Network
' 
'         Properties: edges, IsEmpty, meta, nodes
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: GetNode, HaveNode
' 
'         Sub: RemoveDuplicated, RemoveSelfLoop, RemovesIsolatedNodes
' 
'         Operators: (+4 Overloads) -, (+2 Overloads) ^, (+4 Overloads) +, <=, >=
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace FileStream.Generic

    ''' <summary>
    ''' The network csv data information with specific type of the datamodel
    ''' </summary>
    ''' <typeparam name="T_Node"></typeparam>
    ''' <typeparam name="T_Edge"></typeparam>
    ''' <remarks></remarks>
    Public Class Network(Of T_Node As Node, T_Edge As NetworkEdge)
        Implements IKeyValuePairObject(Of T_Node(), T_Edge())

        Public Property meta As MetaData

        Public Property nodes As T_Node() Implements IKeyValuePairObject(Of T_Node(), T_Edge()).Key
            Get
                If __nodes Is Nothing Then
                    __nodes = New Dictionary(Of T_Node)
                End If
                Return __nodes.Values.ToArray
            End Get
            Set(value As T_Node())
                If value Is Nothing Then
                    __nodes = New Dictionary(Of T_Node)
                Else
                    __nodes = value.ToDictionary(replaceOnDuplicate:=True)
                End If
            End Set
        End Property

        Public Property edges As T_Edge() Implements IKeyValuePairObject(Of T_Node(), T_Edge()).Value
            Get
                If __edges Is Nothing Then
                    __edges = New List(Of T_Edge)
                End If
                Return __edges.ToArray
            End Get
            Set(value As T_Edge())
                If value Is Nothing Then
                    __edges = New List(Of T_Edge)
                Else
                    __edges = value.AsList
                End If
            End Set
        End Property

        ''' <summary>
        ''' 判断这个网络模型之中是否是没有任何数据
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return __nodes.IsNullOrEmpty AndAlso __edges.IsNullOrEmpty
            End Get
        End Property

        Sub New()
            __nodes = New Dictionary(Of T_Node)
            __edges = New List(Of T_Edge)
            _meta = New MetaData With {
                .create_time = Now.ToString,
                .creators = {Environment.UserName}
            }
        End Sub

        Dim __nodes As Dictionary(Of T_Node)
        Dim __edges As List(Of T_Edge)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function HaveNode(id$) As Boolean
            Return __nodes.ContainsKey(id)
        End Function

        ''' <summary>
        ''' 移除的重复的边
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub RemoveDuplicated()
            Dim LQuery As T_Edge() = edges _
                .GroupBy(Function(ed) ed.GetNullDirectedGuid(True)) _
                .Select(Function(g) g.First) _
                .ToArray

            edges = LQuery
        End Sub

        ''' <summary>
        ''' 移除自身与自身的边
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub RemoveSelfLoop()
            Dim LQuery = LinqAPI.Exec(Of T_Edge) _
                                                 _
                () <= From x As T_Edge
                      In edges
                      Where Not x.selfLoop
                      Select x

            edges = LQuery
        End Sub

        Public Sub RemovesIsolatedNodes()
            Dim connectedNodes = edges.Select(Function(e) {e.fromNode, e.toNode}).IteratesALL.Distinct.Indexing

            nodes = nodes _
                .Where(Function(n) n.ID Like connectedNodes) _
                .ToArray
        End Sub

        Public Shared Operator +(net As Network(Of T_Node, T_Edge), x As T_Node) As Network(Of T_Node, T_Edge)
            Call net.__nodes.Add(x)
            Return net
        End Operator

        Public Shared Operator -(net As Network(Of T_Node, T_Edge), x As T_Node) As Network(Of T_Node, T_Edge)
            Call net.__nodes.Remove(x)
            Return net
        End Operator

        Public Shared Operator +(net As Network(Of T_Node, T_Edge), x As T_Edge) As Network(Of T_Node, T_Edge)
            Call net.__edges.Add(x)
            Return net
        End Operator

        Public Shared Operator -(net As Network(Of T_Node, T_Edge), x As T_Edge) As Network(Of T_Node, T_Edge)
            Call net.__edges.Remove(x)
            Return net
        End Operator

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="x">由于会调用ToArray，所以这里建议使用Iterator</param>
        ''' <returns></returns>
        Public Shared Operator +(net As Network(Of T_Node, T_Edge), x As IEnumerable(Of T_Node)) As Network(Of T_Node, T_Edge)
            Call net.__nodes.AddRange(x.ToArray)
            Return net
        End Operator

        Public Shared Operator -(net As Network(Of T_Node, T_Edge), lst As IEnumerable(Of T_Node)) As Network(Of T_Node, T_Edge)
            For Each x In lst
                Call net.__nodes.Remove(x)
            Next

            Return net
        End Operator

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="x">由于会调用ToArray，所以这里建议使用Iterator</param>
        ''' <returns></returns>
        Public Shared Operator +(net As Network(Of T_Node, T_Edge), x As IEnumerable(Of T_Edge)) As Network(Of T_Node, T_Edge)
            Call net.__edges.AddRange(x.ToArray)
            Return net
        End Operator

        Public Shared Operator -(net As Network(Of T_Node, T_Edge), lst As IEnumerable(Of T_Edge)) As Network(Of T_Node, T_Edge)
            For Each x In lst
                Call net.__edges.Remove(x)
            Next

            Return net
        End Operator

        ''' <summary>
        ''' Network contains node?
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Shared Operator ^(net As Network(Of T_Node, T_Edge), node As String) As Boolean
            Return net.__nodes.ContainsKey(node)
        End Operator

        ''' <summary>
        ''' Network contains node?
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Shared Operator ^(net As Network(Of T_Node, T_Edge), node As T_Node) As Boolean
            Return net ^ node.ID
        End Operator

        ''' <summary>
        ''' GET node
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator &(net As Network(Of T_Node, T_Edge), node As String) As T_Node
            If net.__nodes.ContainsKey(node) Then
                Return net.__nodes(node)
            Else
                Return Nothing
            End If
        End Operator

        ''' <summary>
        ''' Select nodes from the network based on the input identifers <paramref name="nodes"/>
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="nodes"></param>
        ''' <returns></returns>
        Public Shared Operator <=(net As Network(Of T_Node, T_Edge), nodes As IEnumerable(Of String)) As T_Node()
            Dim LQuery = (From sId As String In nodes Select net.__nodes(sId)).ToArray
            Return LQuery
        End Operator

        Public Shared Operator >=(net As Network(Of T_Node, T_Edge), nodes As IEnumerable(Of String)) As T_Node()
            Return net <= nodes
        End Operator

        Public Function GetNode(name As String) As T_Node
            Return Me & name
        End Function
    End Class
End Namespace
