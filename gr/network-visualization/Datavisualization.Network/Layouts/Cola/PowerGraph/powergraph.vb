Imports number = System.Double
Imports any = System.Object

Namespace Layouts.Cola

    Module powergraphExtensions

        Private Sub toGroups(modules As ModuleSet, group As Integer, groups As List(Of Integer))
            modules.forAll(Sub(m)
                               If m.isLeaf() Then
                                   If Not group.leaves Then
                                       group.leaves = New Object() {}
                                   End If
                                   group.leaves.push(m.id)
                               Else
                                   Dim g = group
                                   m.gid = groups.Count
                                   If Not m.isIsland() OrElse m.isPredefined() Then
                                       g = New With {
                    Key .id = m.gid
                }
                                       If m.isPredefined() Then
                                           ' Apply original group properties
                                           For Each prop As var In m.definition.keys
                                               g(prop) = m.definition(prop)
                                           Next
                                       End If
                                       If Not group.groups Then
                                           group.groups = New any() {}
                                       End If
                                       group.groups.push(m.gid)
                                       groups.Add(g)
                                   End If
                                   toGroups(m.children, g, groups)
                               End If
                           End Sub)
        End Sub


        Public Function intersection(m As any, n As any) As any
            Dim i = New Object() {}
            For Each v As var In m.keys
                If n.Have(v) Then
                    i(v) = m(v)
                End If
            Next
            Return i
        End Function

        Private Function intersectionCount(m As any, n As any) As Double
            Return [Object].keys(intersection(m, n)).length
        End Function

        Private Function getGroups(Of Link)(nodes As any(), links As Link(), la As LinkTypeAccessor(Of Link), rootGroup As any()) As any
            Dim n = nodes.Length
            Dim c = New Configuration(n, links, la, rootGroup)
            While c.greedyMerge()


            End While
            Dim powerEdges As PowerEdge() = {}
            Dim g = c.getGroupHierarchy(powerEdges)
            powerEdges.ForEach(Function(e)
                                   Dim f = Function([end])
                                               Dim g = e([end])
                                               If GetType(g) Is Double Then
                                                   e([end]) = nodes(g)
                                               End If

                                           End Function
                                   f("source")
                                   f("target")

                               End Function)
            Return New With {
            Key .groups = g,
            Key .powerEdges = powerEdges
        }
        End Function

    End Module

End Namespace