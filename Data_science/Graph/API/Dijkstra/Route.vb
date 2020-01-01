#Region "Microsoft.VisualBasic::cc2acf1caad697d15a7328d45f07bce4, Data_science\Graph\API\Dijkstra\Route.vb"

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

    '     Class Route
    ' 
    '         Properties: Connections, Cost, Count, Identity, IsReadOnly
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Contains, ContainsNode, GetEnumerator, GetEnumerator1, IndexOf
    '                   Remove, ToString
    ' 
    '         Sub: Add, Clear, CopyTo, Insert, RemoveAt
    '              SetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Connection = Microsoft.VisualBasic.Data.GraphTheory.VertexEdge

Namespace Dijkstra

    ''' <summary>
    ''' 从出发点到终点所经过的路径
    ''' </summary>
    Public Class Route : Implements IList(Of Connection)
        Implements IReadOnlyId

        Public ReadOnly Property id As String Implements IReadOnlyId.Identity

        ReadOnly route As List(Of Connection)
        ReadOnly vertex As New HashList(Of Vertex)

        Public Sub New(name As String)
            _Cost = Integer.MaxValue
            route = New List(Of Connection)()
            id = name
        End Sub

        Public ReadOnly Property Connections() As Connection()
            Get
                Return route.ToArray
            End Get
        End Property

        Public Property Cost As Double

        Public Overrides Function ToString() As String
            Return "Id:" & id & " Cost:" & Cost
        End Function

        Public Function ContainsNode(ID%) As Boolean
            Return Not vertex(ID) Is Nothing
        End Function

        Public Sub SetValue(Connections As IEnumerable(Of Connection))
            Call route.Clear()
            Call route.AddRange(Connections)
        End Sub

#Region "Implements IList(Of Connection)"

        Public Sub Add(item As Connection) Implements ICollection(Of Connection).Add
            Call route.Add(item)
        End Sub

        Public Sub Clear() Implements ICollection(Of Connection).Clear
            Call route.Clear()
        End Sub

        Public Function Contains(item As Connection) As Boolean Implements ICollection(Of Connection).Contains
            Return route.Contains(item)
        End Function

        Public Sub CopyTo(array() As Connection, arrayIndex As Integer) Implements ICollection(Of Connection).CopyTo
            Call route.CopyTo(array, arrayIndex)
        End Sub

        Public ReadOnly Property Count As Integer Implements ICollection(Of Connection).Count
            Get
                Return route.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of Connection).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As Connection) As Boolean Implements ICollection(Of Connection).Remove
            Return route.Remove(item)
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Connection) Implements IEnumerable(Of Connection).GetEnumerator
            For Each cnnItem As Connection In route
                Yield cnnItem
            Next
        End Function

        Public Function IndexOf(item As Connection) As Integer Implements IList(Of Connection).IndexOf
            Return route.IndexOf(item)
        End Function

        Public Sub Insert(index As Integer, item As Connection) Implements IList(Of Connection).Insert
            Call route.Insert(index, item)
        End Sub

        Default Public Property Item(index As Integer) As Connection Implements IList(Of Connection).Item
            Get
                Return route(index)
            End Get
            Set(value As Connection)
                route(index) = value
            End Set
        End Property

        Public Sub RemoveAt(index As Integer) Implements IList(Of Connection).RemoveAt
            Call route.RemoveAt(index)
        End Sub

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
#End Region
    End Class
End Namespace
