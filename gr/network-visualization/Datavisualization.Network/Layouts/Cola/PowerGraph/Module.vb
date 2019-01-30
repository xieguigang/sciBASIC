Imports System.Runtime.CompilerServices

Namespace Layouts.Cola


    Public Class [Module]

        Public gid As Integer?
        Public id As Integer

        Public outgoing As New LinkSets
        Public incoming As New LinkSets
        Public children As New ModuleSet
        Public definition As any

        Default Public Property LinkSetItem(name As String) As LinkSets
            Get
                If name = NameOf(outgoing) Then
                    Return outgoing
                ElseIf name = NameOf(incoming) Then
                    Return incoming
                Else
                    Throw New NotImplementedException(name)
                End If
            End Get
            Set(value As LinkSets)
                If name = NameOf(outgoing) Then
                    outgoing = value
                ElseIf name = NameOf(incoming) Then
                    incoming = value
                Else
                    Throw New NotImplementedException(name)
                End If
            End Set
        End Property

        Public ReadOnly Property isLeaf() As Boolean
            Get
                Return Me.children.Count() = 0
            End Get
        End Property

        Public ReadOnly Property isIsland() As Boolean
            Get
                Return Me.outgoing.Count() = 0 AndAlso Me.incoming.Count() = 0
            End Get
        End Property

        Public ReadOnly Property isPredefined() As Boolean
            Get
                Return Me.definition IsNot Nothing
            End Get
        End Property

        Public Sub New(id%,
                       Optional outgoing As LinkSets = Nothing,
                       Optional incoming As LinkSets = Nothing,
                       Optional children As ModuleSet = Nothing,
                       Optional definition As any = Nothing)

            Me.id = id
            Me.outgoing = outgoing
            Me.incoming = incoming
            Me.children = children
            Me.definition = definition
        End Sub

        Private Sub getEdges(es As List(Of PowerEdge))
            Me.outgoing.forAll(Sub(ms, edgetype)
                                   ms.forAll(Sub(target)
                                                 es.Add(New PowerEdge(Me.id, target.id, edgetype))
                                             End Sub)
                               End Sub)
        End Sub
    End Class

    Public Class ModuleSet

        Dim table As New Dictionary(Of String, [Module])

        Public ReadOnly Property count() As Integer
            Get
                Return table.Count
            End Get
        End Property

        Public Function intersection(other As ModuleSet) As ModuleSet
            Dim result = New ModuleSet()
            result.table = intersection(Me.table, other.table)
            Return result
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function intersectionCount(other As ModuleSet) As Integer
            Return Me.intersection(other).count()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function contains(id As Double) As Boolean
            Return Me.table.ContainsKey(id)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub add(m As [Module])
            Me.table(m.id.ToString) = m
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub remove(m As [Module])
            Call Me.table.Remove(m.id.ToString)
        End Sub

        Public Sub forAll(f As Action(Of [Module]))
            For Each mid As String In Me.table.Keys
                f(Me.table(mid))
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function modules() As [Module]()
            Return table.Values.Where(Function(m) m.isPredefined).ToArray
        End Function
    End Class
End Namespace