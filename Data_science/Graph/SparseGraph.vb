Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
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

    Public NotInheritable Class Edge : Implements IInteraction

        <XmlAttribute> Public Property u As String Implements IInteraction.source
        <XmlAttribute> Public Property v As String Implements IInteraction.target

        Sub New()
        End Sub

        Sub New(u As String, v As String)
            _u = u
            _v = v
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
            Dim vlist As Edge() = index_u.TryGetValue(u)

            For Each v As String In keys
                Yield Aggregate e As Edge
                      In vlist
                      Where e.v = v
                      Into Count
            Next
        End If
    End Function

    Public Iterator Function GetGraph() As IEnumerable(Of IInteraction) Implements ISparseGraph.GetGraph
        For Each edge As Edge In graph
            Yield edge
        Next
    End Function
End Class
