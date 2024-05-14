#Region "Microsoft.VisualBasic::0451fe65c63d780a6ababd6deaccc4ab, gr\network-visualization\Network.IO.Extensions\IO\FileStream\csv\Edge.vb"

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

    '   Total Lines: 152
    '    Code Lines: 106
    ' Comment Lines: 27
    '   Blank Lines: 19
    '     File Size: 6.13 KB


    '     Class NetworkEdge
    ' 
    '         Properties: fromNode, interaction, Key, selfLoop, toNode
    '                     value
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Contains, Equals, GetConnectedNode, GetNode, IsEqual
    '                   Nodes, ToString
    '         Operators: -, +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract

Namespace FileStream

    ''' <summary>
    ''' The edge between the two nodes in the network.(节点与节点之间的相互关系)
    ''' </summary>
    ''' <remarks></remarks>
    <XmlType("VisualizeNode")>
    Public Class NetworkEdge : Inherits IDynamicsTable
        Implements IInteraction, INetworkEdge
        Implements INamedValue

        <Column(Name:="fromNode")> <XmlAttribute("source")>
        Public Overridable Property fromNode As String Implements IInteraction.source
        <Column(Name:="toNode")> <XmlAttribute("target")>
        Public Overridable Property toNode As String Implements IInteraction.target
        ''' <summary>
        ''' 与当前的这个边对象所相关联的一个数值对象，可以为置信度，相关度，强度之类的
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("value")>
        Public Overridable Property value As Double Implements INetworkEdge.value
        <Column(Name:=NamesOf.REFLECTION_ID_MAPPING_INTERACTION_TYPE), XmlText>
        Public Overridable Property interaction As String Implements INetworkEdge.Interaction

        ''' <summary>
        ''' 起始节点是否是终止节点
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property selfLoop As Boolean
            Get
                Return String.Equals(fromNode, toNode, StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

        Private Property Key As String Implements IKeyedEntity(Of String).Key
            Get
                Return GetDirectedGuid()
            End Get
            Set(value As String)
                ' Do Nothing
            End Set
        End Property

        Public Sub New()
        End Sub

        Sub New(from As String, target As String, confi As Double)
            Me.fromNode = from
            Me.toNode = target
            Me.value = confi
        End Sub

        ''' <summary>
        ''' Copy value
        ''' </summary>
        ''' <param name="clone"></param>
        Sub New(clone As NetworkEdge)
            With Me
                .value = clone.value
                .fromNode = clone.fromNode
                .interaction = clone.interaction
                .Properties = New Dictionary(Of String, String)(clone.Properties)
                .toNode = clone.toNode
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Contains(Interactor As String) As Boolean
            Return String.Equals(Interactor, fromNode, CaseInsensitive) OrElse String.Equals(Interactor, toNode, CaseInsensitive)
        End Function

        ''' <summary>
        ''' Yield all node ids
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function Nodes() As IEnumerable(Of String)
            Yield fromNode
            Yield toNode
        End Function

        ''' <summary>
        ''' 假若存在连接则返回相对的节点，否则返回空字符串
        ''' </summary>
        ''' <param name="Node"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetConnectedNode(Node As String) As String
            Return Graph.Abstract.GetConnectedNode(Me, Node)
        End Function

        Public Overloads Function Equals(Id1 As String, Id2 As String) As Boolean
            Return (String.Equals(fromNode, Id1) AndAlso
                String.Equals(toNode, Id2)) OrElse
                (String.Equals(fromNode, Id2) AndAlso
                String.Equals(toNode, Id1))
        End Function

        Public Function IsEqual(OtherNode As NetworkEdge) As Boolean
            Return String.Equals(fromNode, OtherNode.fromNode) AndAlso
                String.Equals(toNode, OtherNode.toNode) AndAlso
                String.Equals(interaction, OtherNode.interaction) AndAlso
                value = OtherNode.value
        End Function

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(toNode) Then
                Return fromNode
            Else
                If String.IsNullOrEmpty(interaction) Then
                    Return String.Format("{0} --> {1}", fromNode, toNode)
                Else
                    Return String.Format("{0} {1} {2}", fromNode, interaction, toNode)
                End If
            End If
        End Function

        Public Shared Function GetNode(Node1 As String, Node2 As String, Network As NetworkEdge()) As NetworkEdge
            Dim LQuery = (From Node As NetworkEdge
                          In Network
                          Where String.Equals(Node1, Node.fromNode) AndAlso
                              String.Equals(Node2, Node.toNode)
                          Select Node).ToArray

            If LQuery.Length > 0 Then Return LQuery(Scan0)

            Dim Found = (From Node As NetworkEdge
                         In Network
                         Where String.Equals(Node1, Node.toNode) AndAlso
                              String.Equals(Node2, Node.fromNode)
                         Select Node).FirstOrDefault
            Return Found
        End Function

        Public Shared Operator +(list As List(Of NetworkEdge), x As NetworkEdge) As List(Of NetworkEdge)
            Call list.Add(x)
            Return list
        End Operator

        Public Shared Operator -(list As List(Of NetworkEdge), x As NetworkEdge) As List(Of NetworkEdge)
            Call list.Remove(x)
            Return list
        End Operator
    End Class
End Namespace
