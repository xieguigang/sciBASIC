Imports System.Drawing
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Linq

Public Class Network : Implements IReadOnlyCollection(Of Node)
    Implements IList(Of Node)

    Protected Friend _nodesInnerList As List(Of Node) = New List(Of Node)
    Public Property Edges As Edge()

    Public Property FrameSize As Size

    Public Property BackgroundImage As Image

    Public ReadOnly Property Nodes As Node()
        Get
            Return _nodesInnerList.ToArray
        End Get
    End Property

    ''' <summary>
    ''' Returns the set of all Nodes that have emanating Edges.
    ''' This therefore returns all Nodes that will be visible in the drawing.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property connectedNodes() As Node()
        Get
            Dim _connectedNodes As New List(Of Node)
            For Each Edge In Edges
                Call _connectedNodes.AddRange({Edge.V, Edge.U})
            Next
            Return _connectedNodes.Distinct.ToArray
        End Get
    End Property

    Sub New(Nodes As FileStream.NetworkEdge(), Optional FrameSize As Size = Nothing)
        Me.FrameSize = If(FrameSize = Nothing, New Size(480 + Nodes.Count * 30, 320 + Nodes.Count * 24), FrameSize)
        Call CreateNetwork(NetworkModel:=Nodes)
    End Sub

    Private Shared Function __randColor() As Color
        Return Color.FromArgb(230, 255 * RandomDouble(), 255 * RandomDouble(), 255 * RandomDouble())
    End Function

    Private Sub CreateNetwork(NetworkModel As FileStream.NetworkEdge())
        Dim NodeNames As List(Of String) = New List(Of String)
        Call NodeNames.AddRange((From item In NetworkModel Select item.FromNode).ToArray)
        Call NodeNames.AddRange((From item In NetworkModel Select item.ToNode).ToArray)
        NodeNames = NodeNames.Distinct.ToList

        Dim Nodes = (From strt As SeqValue(Of String) In NodeNames.SeqIterator
                     Select New Node With {
                         ._DispName = strt.obj,
                         ._Id = strt.i,
                         .Color = __randColor()}).ToArray

        For i As Integer = 0 To Nodes.Count - 1  '创建连接
            Dim Name As String = Nodes(i)._DispName
            Dim ConnectionNodes = (From item In NetworkModel Let cnnId As String = item.GetConnectedNode(Name) Where Not String.IsNullOrEmpty(cnnId) Select cnnId Distinct).ToList
            Call ConnectionNodes.Remove(Name)

            Nodes(i).Neighbours = (From node In Nodes Where ConnectionNodes.IndexOf(node._DispName) > -1 Select node.Id).ToArray
            Nodes(i).Weights = (From node In Nodes(i).Neighbours Select FileStream.NetworkEdge.GetNode(Name, Nodes(node)._DispName, NetworkModel).Confidence).ToArray
        Next

        Me._nodesInnerList = Nodes.ToList
        Me.Edges = (From item In NetworkModel Select New Edge(v:=NodeItem(item.FromNode), u:=NodeItem(item.ToNode)) With {._weight = item.Confidence * 20}).ToArray
    End Sub

    Public ReadOnly Property NodeItem(Name As String) As Node
        Get
            Dim LQuery = (From item In Nodes Where String.Equals(Name, item.DispName) Select item).ToArray
            If LQuery.IsNullOrEmpty Then
                Return Nothing
            Else
                Return LQuery.First
            End If
        End Get
    End Property

    Public Iterator Function GetEnumerator() As IEnumerator(Of Node) Implements IEnumerable(Of Node).GetEnumerator
        For i As Integer = 0 To Nodes.Count - 1
            Yield Nodes(i)
        Next
    End Function

    Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of Node).Count, ICollection(Of Node).Count
        Get
            Return Nodes.Count
        End Get
    End Property

    Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function

    Public Sub Add(item As Node) Implements ICollection(Of Node).Add
        Call _nodesInnerList.Add(item)
    End Sub

    Public Sub Clear() Implements ICollection(Of Node).Clear
        Call _nodesInnerList.Clear()
    End Sub

    Public Function Contains(item As Node) As Boolean Implements ICollection(Of Node).Contains
        Return _nodesInnerList.Contains(item)
    End Function

    Public Sub CopyTo(array() As Node, arrayIndex As Integer) Implements ICollection(Of Node).CopyTo
        Call _nodesInnerList.CopyTo(array, arrayIndex)
    End Sub

    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of Node).IsReadOnly
        Get
            Return False
        End Get
    End Property

    Public Function Remove(item As Node) As Boolean Implements ICollection(Of Node).Remove
        Return _nodesInnerList.Remove(item)
    End Function

    Public Function IndexOf(item As Node) As Integer Implements IList(Of Node).IndexOf
        Return _nodesInnerList.IndexOf(item)
    End Function

    Public Sub Insert(index As Integer, item As Node) Implements IList(Of Node).Insert
        Call _nodesInnerList.Insert(index, item)
    End Sub

    Default Public Overloads Property Node(index As Integer) As Node Implements IList(Of Node).Item
        Get
            Return _nodesInnerList(index)
        End Get
        Set(value As Node)
            _nodesInnerList(index) = value
        End Set
    End Property

    Public Sub RemoveAt(index As Integer) Implements IList(Of Node).RemoveAt
        Call _nodesInnerList.RemoveAt(index)
    End Sub
End Class
