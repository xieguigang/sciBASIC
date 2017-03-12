#Region "Microsoft.VisualBasic::db301fb5e9e153da1e98dcfeb8718dc3, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\FindPath\Dijkstra\LDM.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Dijkstra

    Public Class Connection : Implements IKeyValuePairObject(Of FileStream.Node, FileStream.Node)

        Dim _source As FileStream.NetworkEdge

        Public Property Selected As Boolean = False
        Public Property B As FileStream.Node Implements IKeyValuePairObject(Of FileStream.Node, FileStream.Node).Value
        Public Property A As FileStream.Node Implements IKeyValuePairObject(Of FileStream.Node, FileStream.Node).Key
        Public Property Weight As Integer

        Public Sub New(a As FileStream.Node, b As FileStream.Node, weight As Integer)
            _A = a
            _B = b
            _Weight = weight
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Function CreateObject(DataModel As FileStream.NetworkEdge) As Connection
            Return CreateObject(DataModel, DataModel.Confidence)
        End Function

        Public Shared Function CreateObject(edge As FileStream.NetworkEdge, weight As Integer) As Connection
            Dim ndA As New FileStream.Node(edge.FromNode)
            Dim ndB As New FileStream.Node(edge.ToNode)

            Return New Connection(ndA, ndB, weight:=weight) With {
                ._source = edge
            }
        End Function
    End Class

    Public Class Route : Implements Generic.IList(Of Connection)

        Dim _Connections As List(Of Connection)
        Dim _identifier As String

        Public Function ContainsNode(Id As String) As Boolean
            Dim LQuery = (From conn In _Connections.AsParallel
                          Where String.Equals(conn.A.ID, Id, StringComparison.OrdinalIgnoreCase) OrElse
                              String.Equals(conn.B.ID, Id, StringComparison.OrdinalIgnoreCase)
                          Select conn).ToArray
            Return Not LQuery.IsNullOrEmpty
        End Function

        Public Sub New(identifier As String)
            _Cost = Integer.MaxValue
            _Connections = New List(Of Connection)()
            _identifier = identifier
        End Sub

        Public ReadOnly Property Connections() As Connection()
            Get
                Return _Connections.ToArray
            End Get
        End Property

        Public Property Cost As Integer

        Public Overrides Function ToString() As String
            Return "Id:" & _identifier & " Cost:" & Cost
        End Function

        Public Sub SetValue(Connections As Generic.IEnumerable(Of Connection))
            Call _Connections.Clear()
            Call _Connections.AddRange(Connections)
        End Sub

#Region "Implements Generic.IList(Of Connection)"

        Public Sub Add(item As Connection) Implements ICollection(Of Connection).Add
            Call _Connections.Add(item)
        End Sub

        Public Sub Clear() Implements ICollection(Of Connection).Clear
            Call _Connections.Clear()
        End Sub

        Public Function Contains(item As Connection) As Boolean Implements ICollection(Of Connection).Contains
            Return _Connections.Contains(item)
        End Function

        Public Sub CopyTo(array() As Connection, arrayIndex As Integer) Implements ICollection(Of Connection).CopyTo
            Call _Connections.CopyTo(array, arrayIndex)
        End Sub

        Public ReadOnly Property Count As Integer Implements ICollection(Of Connection).Count
            Get
                Return _Connections.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of Connection).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As Connection) As Boolean Implements ICollection(Of Connection).Remove
            Return _Connections.Remove(item)
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Connection) Implements IEnumerable(Of Connection).GetEnumerator
            For Each cnnItem As Connection In _Connections
                Yield cnnItem
            Next
        End Function

        Public Function IndexOf(item As Connection) As Integer Implements IList(Of Connection).IndexOf
            Return _Connections.IndexOf(item)
        End Function

        Public Sub Insert(index As Integer, item As Connection) Implements IList(Of Connection).Insert
            Call _Connections.Insert(index, item)
        End Sub

        Default Public Property Item(index As Integer) As Connection Implements IList(Of Connection).Item
            Get
                Return _Connections(index)
            End Get
            Set(value As Connection)
                _Connections(index) = value
            End Set
        End Property

        Public Sub RemoveAt(index As Integer) Implements IList(Of Connection).RemoveAt
            Call _Connections.RemoveAt(index)
        End Sub

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
#End Region
    End Class
End Namespace
