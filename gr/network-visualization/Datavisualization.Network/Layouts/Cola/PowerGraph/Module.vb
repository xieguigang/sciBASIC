Namespace Layouts.Cola


    Public Class [Module]

        Public gid As Double
        Public id As Double

        Public outgoing As New List(Of LinkSets)
        Public incoming As New List(Of LinkSets)
        Public children As New List(Of ModuleSet)
        Public definition As any

        Public Sub New(id As Double, Optional outgoing As LinkSets = Nothing, Optional incoming As LinkSets = Nothing, Optional children As ModuleSet = Nothing, Optional definition As any = Nothing)
            Me.id = id
            Me.outgoing = outgoing
            Me.incoming = incoming
            Me.children = children
            Me.definition = definition
        End Sub

        Private Sub getEdges(es As PowerEdge())
            Me.outgoing.forAll(Sub(ms, edgetype)
                                   ms.forAll(Sub(target)
                                                 es.push(New PowerEdge(Me.id, target.id, edgetype))
                                             End Sub)
                               End Sub)
        End Sub

        Private Function isLeaf() As Boolean
            Return Me.children.Count() = 0
        End Function

        Private Function isIsland() As Boolean
            Return Me.outgoing.Count() = 0 AndAlso Me.incoming.Count() = 0
        End Function

        Private Function isPredefined() As Boolean
            Return Me.definition IsNot Nothing
        End Function
    End Class


    Class ModuleSet
        Private table As any = New Object() {}
        Private Function count() As Double
            Return [Object].keys(Me.table).length
        End Function
        Private Function intersection(other As ModuleSet) As ModuleSet
            Dim result = New ModuleSet()
            result.table = intersection(Me.table, other.table)
            Return result
        End Function
        Private Function intersectionCount(other As ModuleSet) As Double
            Return Me.intersection(other).count()
        End Function
        Private Function contains(id As Double) As Boolean
            Return Me.table.Have(id)
        End Function
        Private Sub add(m As [Module])
            Me.table(m.id) = m
        End Sub
        Private Sub remove(m As [Module])
            Delete(Me.table, m.id)
        End Sub
        Private Sub forAll(f As Action(Of [Module]))
            For Each mid As var In Me.table.keys
                f(Me.table(mid))
            Next
        End Sub
        Private Function modules() As [Module]()
            Dim vs = New Object() {}
            Me.forAll(Function(m)
                          If Not m.isPredefined() Then
                              vs.push(m)
                          End If

                      End Function)
            Return vs
        End Function
    End Class
End Namespace