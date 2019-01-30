Namespace Layouts.Cola

    Module powergraphExtensions

        Public Sub toGroups(modules As ModuleSet, group As Group, groups As List(Of Group))
            Call modules.forAll(Sub(m)
                                    If m.isLeaf Then
                                        If group.leaves Is Nothing Then
                                            group.leaves = New List(Of Node)
                                        End If

                                        group.leaves.Add(m.id)
                                    Else
                                        Dim g = group
                                        m.gid = groups.Count

                                        If (Not m.isIsland OrElse m.isPredefined) Then
                                            g = New [Group] With {.id = m.gid}

                                            If m.isPredefined Then
                                                For Each prop As String In m.definition.Keys
                                                    g(prop) = m.definition(prop)
                                                Next
                                            End If
                                            If group.groups.IsNullOrEmpty Then
                                                group.groups = New List(Of Group)
                                            End If

                                            group.groups.Add(m.gid)
                                            groups.Add(g)
                                        End If

                                        toGroups(m.children, g, groups)
                                    End If
                                End Sub)
        End Sub

        Public Function intersection(Of T)(m As Dictionary(Of String, T), n As Dictionary(Of String, T)) As Dictionary(Of String, T)
            Dim i As New Dictionary(Of String, T)

            For Each v As String In m.Keys
                If n.ContainsKey(v) Then
                    i(v) = m(v)
                End If
            Next

            Return i
        End Function

        Public Function getGroups(Of Link)(nodes As Node(), links As Link(), la As LinkTypeAccessor(Of Link), rootGroup As Group) As PowerGraph
            Dim n = nodes.Length
            Dim c = New Configuration(Of Link)(n, links, la, rootGroup)

            While c.greedyMerge()
            End While

            Dim powerEdges As New List(Of PowerEdge)
            Dim g = c.getGroupHierarchy(powerEdges)

            powerEdges.DoEach(Sub(e)
                                  Dim f = Sub([end] As String) e([end]) = nodes(e([end]))

                                  Call f("source")
                                  Call f("target")
                              End Sub)

            Return New PowerGraph With {
                .groups = g,
                .powerEdges = powerEdges
            }
        End Function

    End Module

    Public Class PowerGraph
        Public Property groups As List(Of Group)
        Public powerEdges As List(Of PowerEdge)
    End Class

End Namespace