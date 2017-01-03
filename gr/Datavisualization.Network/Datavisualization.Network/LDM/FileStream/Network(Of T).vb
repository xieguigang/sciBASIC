#Region "Microsoft.VisualBasic::9831235236a11c0484c4dca3f3f2443f, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\LDM\FileStream\Network(Of T).vb"

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

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.csv.Extensions
Imports Microsoft.VisualBasic.Language

Namespace FileStream

    ''' <summary>
    ''' The network csv data information with specific type of the datamodel
    ''' </summary>
    ''' <typeparam name="T_Node"></typeparam>
    ''' <typeparam name="T_Edge"></typeparam>
    ''' <remarks></remarks>
    Public Class Network(Of T_Node As Node, T_Edge As NetworkEdge) : Inherits Language.UnixBash.File
        Implements IKeyValuePairObject(Of T_Node(), T_Edge())
        Implements ISaveHandle

        Public Property Nodes As T_Node() Implements IKeyValuePairObject(Of T_Node(), T_Edge()).Identifier
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
                    __nodes = value.ToDictionary
                End If
            End Set
        End Property
        Public Property Edges As T_Edge() Implements IKeyValuePairObject(Of T_Node(), T_Edge()).Value
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
                    __edges = value.ToList
                End If
            End Set
        End Property

        Sub New()
            __nodes = New Dictionary(Of T_Node)
            __edges = New List(Of T_Edge)
        End Sub

        Dim __nodes As Dictionary(Of T_Node)
        Dim __edges As List(Of T_Edge)

        Public Function HaveNode(id$) As Boolean
            Return __nodes.ContainsKey(id)
        End Function

        ''' <summary>
        ''' 移除的重复的边
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub RemoveDuplicated()
            Dim LQuery As T_Edge() =
                Edges _
                .GroupBy(Function(ed) ed.GetNullDirectedGuid(True)) _
                .Select(Function(g) g.First) _
                .ToArray

            Edges = LQuery
        End Sub

        ''' <summary>
        ''' 移除自身与自身的边
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub RemoveSelfLoop()
            Dim LQuery As T_Edge() =
 _
                LinqAPI.Exec(Of T_Edge) <= From x As T_Edge
                                           In Edges
                                           Where Not x.SelfLoop
                                           Select x
            Edges = LQuery
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="outDIR">The data directory for the data export, if the value of this directory is null then the data
        ''' will be exported at the current work directory.
        ''' (进行数据导出的文件夹，假若为空则会保存数据至当前的工作文件夹之中)</param>
        ''' <param name="encoding">The file encoding of the exported node and edge csv file.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Save(Optional outDIR As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
            If String.IsNullOrEmpty(outDIR) Then
                outDIR = App.CurrentDirectory
            End If

            Call Nodes.SaveTo(String.Format("{0}/nodes.csv", outDIR), False, encoding)
            Call Edges.SaveTo(String.Format("{0}/network-edges.csv", outDIR), False, encoding)

            Return True
        End Function

        Public Shared Function Load(DIR As String) As Network(Of T_Node, T_Edge)
            Return New Network(Of T_Node, T_Edge) With {
                .Edges = $"{DIR}/network-edges.csv".LoadCsv(Of T_Edge),
                .Nodes = $"{DIR}/nodes.csv".LoadCsv(Of T_Node)
            }
        End Function

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
            Return net ^ node.Identifier
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
