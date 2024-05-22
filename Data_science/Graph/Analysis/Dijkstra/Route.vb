#Region "Microsoft.VisualBasic::e594b8b64c5b14f252096dea7cf45b15, Data_science\Graph\Analysis\Dijkstra\Route.vb"

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

    '   Total Lines: 67
    '    Code Lines: 50 (74.63%)
    ' Comment Lines: 3 (4.48%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (20.90%)
    '     File Size: 1.99 KB


    '     Class Route
    ' 
    '         Properties: Connections, Cost, Count, id
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ContainsNode, ToString
    ' 
    '         Sub: Add, Clear, SetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Connection = Microsoft.VisualBasic.Data.GraphTheory.VertexEdge

Namespace Analysis.Dijkstra

    ''' <summary>
    ''' 从出发点到终点所经过的路径
    ''' </summary>
    Public Class Route : Implements IReadOnlyId

        Public ReadOnly Property id As String Implements IReadOnlyId.Identity

        ReadOnly route As List(Of Connection)
        ReadOnly vertex As New HashList(Of Vertex)

        Public ReadOnly Property Connections() As IEnumerable(Of Connection)
            Get
                Return route.AsEnumerable
            End Get
        End Property

        Default Public Property Item(index As Integer) As Connection
            Get
                Return route(index)
            End Get
            Set(value As Connection)
                route(index) = value
            End Set
        End Property

        Public Property Cost As Double

        Public ReadOnly Property Count As Integer
            Get
                Return route.Count
            End Get
        End Property

        Public Sub New(name As String)
            Cost = Integer.MaxValue
            route = New List(Of Connection)()
            id = name
        End Sub

        Public Function ContainsNode(index As Integer) As Boolean
            Return Not vertex(index) Is Nothing
        End Function

        Public Sub SetValue(Connections As IEnumerable(Of Connection))
            Call route.Clear()
            Call route.AddRange(Connections)
        End Sub

        Public Sub Add(item As Connection)
            Call route.Add(item)
        End Sub

        Public Sub Clear()
            Call route.Clear()
        End Sub

        Public Overrides Function ToString() As String
            Return "Id: " & id & ", cost: " & Cost
        End Function
    End Class
End Namespace
