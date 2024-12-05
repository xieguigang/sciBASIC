#Region "Microsoft.VisualBasic::12cfb6b501e595dfa78400a8c4c3e472, Data_science\Graph\SparseGraph.vb"

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

    '   Total Lines: 144
    '    Code Lines: 106 (73.61%)
    ' Comment Lines: 8 (5.56%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 30 (20.83%)
    '     File Size: 4.50 KB


    ' Class SparseGraph
    ' 
    '     Properties: Edges, Vertex
    ' 
    '     Function: Copy, (+2 Overloads) CreateMatrix, GetGraph, Links, MakeIndex
    '               ToString
    '     Class Edge
    ' 
    '         Properties: u, v
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Interface IInteraction
    ' 
    '         Properties: source, target
    ' 
    '     Interface ISparseGraph
    ' 
    '         Function: GetGraph
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Public Class SparseGraph : Implements ISparseGraph

    <XmlElement("edges")>
    Public Property Edges As Edge()
        Get
            Return graph.ToArray
        End Get
        Set(value As Edge())
            graph = value.ToArray
            index_u = MakeIndex(value)
        End Set
    End Property

    Public ReadOnly Property Vertex As String()
        Get
            Return graph _
                .Select(Iterator Function(e) As IEnumerable(Of String)
                            Yield e.u
                            Yield e.v
                        End Function) _
                .IteratesALL _
                .Distinct _
                .ToArray
        End Get
    End Property

    Public NotInheritable Class Edge : Implements IInteraction

        <XmlAttribute> Public Property u As String Implements IInteraction.source
        <XmlAttribute> Public Property v As String Implements IInteraction.target

        Sub New()
        End Sub

        Sub New(u As String, v As String)
            _u = u
            _v = v
        End Sub

        Sub New(line As IInteraction)
            _u = line.source
            _v = line.target
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{u}, {v}]"
        End Function

    End Class

    Public Interface IInteraction

        ''' <summary>
        ''' U
        ''' </summary>
        ''' <returns></returns>
        Property source As String
        ''' <summary>
        ''' V
        ''' </summary>
        ''' <returns></returns>
        Property target As String

    End Interface

    Public Interface ISparseGraph

        Function GetGraph() As IEnumerable(Of IInteraction)

    End Interface

    Dim index_u As Dictionary(Of String, IInteraction())
    Dim graph As Edge()

    Sub New()
    End Sub

    Sub New(edges As IEnumerable(Of Edge))
        Me.Edges = edges.ToArray
    End Sub

    Public Overrides Function ToString() As String
        Dim vlist = Vertex
        Return $"sparse graph of {vlist.Length}x{vlist.Length} vertex and {graph.Length} edges."
    End Function

    Public Function CreateMatrix(keys As String()) As NumericMatrix
        Dim rows As New List(Of Double())

        For Each u As String In keys
            Call rows.Add(Links(u, keys, index_u).ToArray)
        Next

        Return New NumericMatrix(rows)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Shared Function MakeIndex(Of T As IInteraction)(graph As IEnumerable(Of T)) As Dictionary(Of String, IInteraction())
        Return graph.GroupBy(Function(a) a.source) _
            .ToDictionary(Function(a) a.Key,
                            Function(a)
                                Return a.Select(Function(e) DirectCast(e, IInteraction)).ToArray
                            End Function)
    End Function

    Public Shared Function CreateMatrix(Of T As IInteraction)(graph As IEnumerable(Of T), keys As String()) As NumericMatrix
        Dim index_u = MakeIndex(graph)
        Dim rows As New List(Of Double())

        For Each u As String In keys
            Call rows.Add(Links(u, keys, index_u).ToArray)
        Next

        Return New NumericMatrix(rows)
    End Function

    Private Shared Iterator Function Links(u As String, keys As String(), index_u As Dictionary(Of String, IInteraction())) As IEnumerable(Of Double)
        If Not index_u.ContainsKey(u) Then
            For i As Integer = 0 To keys.Length - 1
                Yield 0.0
            Next
        Else
            Dim vlist As IInteraction() = index_u.TryGetValue(u)

            For Each v As String In keys
                Yield Aggregate e As IInteraction
                      In vlist
                      Where e.target = v
                      Into Count
            Next
        End If
    End Function

    Public Iterator Function GetGraph() As IEnumerable(Of IInteraction) Implements ISparseGraph.GetGraph
        For Each edge As Edge In graph
            Yield edge
        Next
    End Function

    ''' <summary>
    ''' make graph structure data copy
    ''' </summary>
    ''' <param name="g"></param>
    ''' <returns></returns>
    Public Shared Function Copy(g As ISparseGraph) As SparseGraph
        Dim edges As Edge() = g.GetGraph.Select(Function(e) New Edge(e)).ToArray
        Dim graph As New SparseGraph With {.Edges = edges}

        Return graph
    End Function
End Class
