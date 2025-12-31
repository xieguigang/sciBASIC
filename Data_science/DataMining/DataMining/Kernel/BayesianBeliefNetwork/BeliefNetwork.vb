#Region "Microsoft.VisualBasic::34a9b88409ae852d6e16c4fbbbb1489f, Data_science\DataMining\DataMining\Kernel\BayesianBeliefNetwork\BeliefNetwork.vb"

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

    '   Total Lines: 298
    '    Code Lines: 218 (73.15%)
    ' Comment Lines: 32 (10.74%)
    '    - Xml Docs: 78.12%
    ' 
    '   Blank Lines: 48 (16.11%)
    '     File Size: 12.48 KB


    '     Class BeliefNode
    ' 
    '         Properties: Id, Name, Range
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: Clear
    ' 
    '     Class BeliefNetwork
    ' 
    '         Properties: Nodes
    ' 
    '         Function: (+2 Overloads) CreateFrom
    ' 
    '         Sub: ResetNodes, SetNodes
    '         Class NetworkInitializer
    ' 
    '             Function: (+2 Overloads) Build
    ' 
    '             Sub: CreateNode
    ' 
    '         Class NetworkLayout
    ' 
    '             Properties: Nodes
    ' 
    '             Function: Load, TestData, ToString
    '             Class BeliefNode
    ' 
    '                 Properties: CPTable, Name, Parents, Range
    ' 
    '                 Function: ToString
    '                 Class ParentList
    ' 
    '                     Properties: Num, ParentNodes
    ' 
    '                     Function: ToString
    ' 
    '                 Class CPTableF
    ' 
    '                     Properties: CPColumns
    '                     Class CPColumn
    ' 
    '                         Properties: Count, Data
    ' 
    '                         Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Xml.Serialization

Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 贝叶斯信念网络中的一个节点
    ''' </summary>
    ''' <remarks></remarks>
    Public Class BeliefNode

        ''' <summary>
        ''' 本节点的父节点
        ''' </summary>
        ''' <remarks></remarks>
        Public Parents As ArrayList
        ''' <summary>
        ''' CP Table，用于把各节点和它的直接父节点相关联起来的一个概率表
        ''' </summary>
        ''' <remarks></remarks>
        Public CP_Table As Double(,)
        Public Evidence As Integer

        Dim _name As String
        Dim _id As Integer, _range As Integer

        Public Sub New(name As String, id As Integer, range As Integer)
            _name = name
            _id = id
            _range = range
            Parents = New ArrayList()
            Call Clear()
        End Sub

        Public ReadOnly Property Name() As String
            Get
                Return _name
            End Get
        End Property

        Public ReadOnly Property Id() As Integer
            Get
                Return _id
            End Get
        End Property

        Public ReadOnly Property Range() As Integer
            Get
                Return _range
            End Get
        End Property

        Public Sub Clear()
            Evidence = -1
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("Name:={0}; Id:={1}; Range:={2}", Name, Id, Range)
        End Function
    End Class

    ''' <summary>
    ''' 贝叶斯信念网络
    ''' </summary>
    ''' <remarks></remarks>
    Public Class BeliefNetwork

        Protected Friend _bnNodes As List(Of BeliefNode) = New List(Of BeliefNode)

        Public ReadOnly Property Nodes() As BeliefNode()
            Get
                Return _bnNodes.ToArray
            End Get
        End Property

        Public Sub ResetNodes()
            For Each Node As BeliefNode In _bnNodes
                Call Node.Clear()
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Array">Array的元素个数必须与节点的数目相等</param>
        ''' <remarks></remarks>
        Public Sub SetNodes(Array As Integer())
            For i As Integer = 0 To _bnNodes.Count - 1
                _bnNodes(i).Evidence = Array(i)
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="File">网络数据的文件路径</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateFrom(File As String) As BeliefNetwork
            Dim Network As BeliefNetwork = New BeliefNetwork
            Return NetworkInitializer.Build(File, Network)
        End Function

        Public Shared Function CreateFrom(NetworkLayout As NetworkLayout) As BeliefNetwork
            Dim Network As BeliefNetwork = New BeliefNetwork
            Return NetworkInitializer.Build(NetworkLayout, Network)
        End Function

        Protected Friend Class NetworkInitializer

            Public Shared Function Build(xmlfile As String, network As BeliefNetwork) As BeliefNetwork
                Dim NetworkLayout = xmlfile.LoadXml(Of BeliefNetwork.NetworkLayout)()
                Return Build(NetworkLayout, network)
            End Function

            Public Shared Function Build(networkLayout As BeliefNetwork.NetworkLayout, network As BeliefNetwork) As BeliefNetwork
                For Each Node In networkLayout.Nodes
                    Call NetworkInitializer.CreateNode(Node, network)
                Next
                Return network
            End Function

            Private Shared Sub CreateNode(theNode As NetworkLayout.BeliefNode, network As BeliefNetwork)
                Dim range As Integer = theNode.Range

                ' Creat new node and add it to the list later
                Dim nId As Integer = network._bnNodes.Count
                Dim newNode As New BeliefNode(theNode.Name, nId, range)

                ' Connect to all its parents
                For Each node As String In theNode.Parents.ParentNodes
                    For Each bn_node As BeliefNode In network._bnNodes
                        If String.Equals(node, bn_node.Name) Then
                            newNode.Parents.Add(bn_node)
                            Exit For
                        End If
                    Next
                Next

                ' Prepare CP Table
                Dim table_rows As Integer = 1
                If range <> theNode.CPTable.CPColumns.Count + 1 Then
                    Throw New Exception("CPT cols mismatch")
                End If
                For Each bn_node As BeliefNode In newNode.Parents
                    table_rows *= bn_node.Range
                Next

                newNode.CP_Table = New Double(table_rows - 1, range - 1) {}

                ' Assign value to CP Table
                For i As Integer = 0 To theNode.CPTable.CPColumns.Count - 1
                    Dim cpColumn = theNode.CPTable.CPColumns(i)

                    If cpColumn.Count <> table_rows Then
                        Throw New Exception("CPT Rows mismatch")
                    End If

                    For j As Integer = 0 To table_rows - 1
                        newNode.CP_Table(j, i) = Convert.ToDouble(cpColumn(j))
                    Next
                Next

                ' Assign value to the last col of the table by rule of Sum Pcol=1.0
                For i As Integer = 0 To table_rows - 1
                    Dim pr As Double = 1.0
                    For j As Integer = 0 To range - 2
                        pr -= newNode.CP_Table(i, j)
                    Next
                    If pr < EValue Then 'If pr < 0 Then   ’VB在这里存在计算误差
                        Throw New Exception("Probability does not normalize")
                    End If
                    newNode.CP_Table(i, range - 1) = pr
                Next

                network._bnNodes.Add(newNode)
            End Sub

            Const EValue As Double = -10 ^ -10
        End Class

        <XmlRoot("Microsoft.VisualBasic.DataVisualization.DataMining.Framework.BeliefNetwork.NetworkLayout")>
        Public Class NetworkLayout
            <XmlElement> Public Property Nodes As BeliefNode()

            Public Class BeliefNode
                <XmlAttribute> Public Property Name As String
                <XmlAttribute> Public Property Range As Integer
                Public Property Parents As ParentList
                Public Property CPTable As CPTableF

                Public Class ParentList
                    <XmlIgnore> Public ReadOnly Property Num As Integer
                        Get
                            If ParentNodes.IsNullOrEmpty Then
                                Return 0
                            Else
                                Return ParentNodes.Length
                            End If
                        End Get
                    End Property

                    <XmlElement> Public Property ParentNodes As String()

                    Public Overrides Function ToString() As String
                        Return String.Format("{0} parent nodes", Num)
                    End Function
                End Class

                Public Class CPTableF
                    <XmlElement> Public Property CPColumns As CPColumn()

                    Public Class CPColumn
                        <XmlAttribute> Public Property Data As Double()

                        Default Public ReadOnly Property Item(Idx As Integer) As Double
                            Get
                                Return Data(Idx)
                            End Get
                        End Property

                        Public ReadOnly Property Count As Integer
                            Get
                                Return Data.Count
                            End Get
                        End Property

                        Public Overrides Function ToString() As String
                            Dim sBuilder As StringBuilder = New StringBuilder(1024)
                            For Each e In Data
                                Call sBuilder.Append(e & ", ")
                            Next
                            Call sBuilder.Remove(sBuilder.Length - 2, 2)

                            Return sBuilder.ToString
                        End Function
                    End Class
                End Class

                Public Overrides Function ToString() As String
                    Return String.Format("Named:={0}; Range:={1}", Name, Range)
                End Function
            End Class

            Public Overrides Function ToString() As String
                Return String.Format("{0} nodes in the belief network", Nodes.Length)
            End Function

            Public Shared Function Load(FilePath As String) As NetworkLayout
                Return FilePath.LoadXml(Of NetworkLayout)()
            End Function

            Public Shared Widening Operator CType(FilePath As String) As NetworkLayout
                Return FilePath.LoadXml(Of NetworkLayout)()
            End Operator

            Public Shared Function TestData() As NetworkLayout
                Dim NodeList As List(Of BeliefNode) = New List(Of BeliefNode)
                Call NodeList.Add(New BeliefNode With
                                  {
                                      .Name = "Cloudy",
                                      .Range = 2,
                                      .Parents = New BeliefNode.ParentList With {.ParentNodes = New String() {}},
                                      .CPTable = New BeliefNode.CPTableF With {.CPColumns =
                                          New NetworkLayout.BeliefNode.CPTableF.CPColumn() {New NetworkLayout.BeliefNode.CPTableF.CPColumn() With {
                                                  .Data = {0.3}}}}
                                  })
                Call NodeList.Add(New BeliefNode With
                                 {
                                     .Name = "Sprinkler",
                                     .Range = 2,
                                     .Parents = New BeliefNode.ParentList With {.ParentNodes = New String() {"Cloudy"}},
                                     .CPTable = New BeliefNode.CPTableF With {.CPColumns = New NetworkLayout.BeliefNode.CPTableF.CPColumn() {New NetworkLayout.BeliefNode.CPTableF.CPColumn() With {
                                                 .Data = {0.5, 0.9}}}}
                                 })
                Call NodeList.Add(New BeliefNode With
                                 {
                                     .Name = "Rain",
                                     .Range = 2,
                                     .Parents = New BeliefNode.ParentList With {.ParentNodes = New String() {"Cloudy"}},
                                     .CPTable = New BeliefNode.CPTableF With {.CPColumns = New NetworkLayout.BeliefNode.CPTableF.CPColumn() {New NetworkLayout.BeliefNode.CPTableF.CPColumn() With {
                                                 .Data = {0.8, 0.2}}}}
                                 })
                Call NodeList.Add(New BeliefNode With
                                 {
                                     .Name = "WetGrass",
                                     .Range = 2,
                                     .Parents = New BeliefNode.ParentList With {.ParentNodes = New String() {"Sprinkler", "Rain"}},
                                     .CPTable = New BeliefNode.CPTableF With {.CPColumns = New NetworkLayout.BeliefNode.CPTableF.CPColumn() {New NetworkLayout.BeliefNode.CPTableF.CPColumn() With {
                                                 .Data = {1.0, 0.1, 0.1, 0.01}}}}
                                 })

                Return New NetworkLayout With {.Nodes = NodeList.ToArray}
            End Function
        End Class
    End Class
End Namespace
