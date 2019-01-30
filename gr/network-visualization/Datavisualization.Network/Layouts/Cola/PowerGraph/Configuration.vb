Namespace Layouts.Cola
    Public Class Configuration(Of Link)
        ' canonical list of modules.
        ' Initialized to a module for each leaf node, such that the ids and indexes of the module in the array match the indexes of the nodes in links
        ' Modules created through merges are appended to the end of this.
        Private modules As [Module]()
        ' top level modules and candidates for merges
        Private roots As ModuleSet()
        ' remaining edge count
        Private R As Double

        Public Sub New(n As Double, edges As Link(), linkAccessor As LinkTypeAccessor(Of Link), rootGroup As any())
            Me.modules = New Array(n)
            Me.roots = New Object() {}
            If rootGroup Then
                Me.initModulesFromGroup(rootGroup)
            Else
                Me.roots.push(New ModuleSet())
                For i As var = 0 To n - 1
                    Me.roots(0).add(InlineAssignHelper(Me.modules(i), New [Module](i)))
                Next
            End If
            Me.R = edges.Length
            edges.DoEach(Sub(e)
                             Dim s = Me.modules(linkAccessor.getSourceIndex(e))
                             Dim t = Me.modules(linkAccessor.getTargetIndex(e))
                             Dim type = linkAccessor.[GetLinkType](e)

                             s.outgoing.Add(type, t)
                             t.incoming.Add(type, s)
                         End Sub)
        End Sub

        Private Function initModulesFromGroup(group As any) As ModuleSet
            Dim moduleSet = New ModuleSet()
            Me.roots.push(moduleSet)
            For i As var = 0 To group.leaves.length - 1
                Dim node = group.leaves(i)
                Dim [module] = New [Module](node.id)
                Me.modules(node.id) = [module]
                moduleSet.add([module])
            Next
            If group.groups Then
                For j As var = 0 To group.groups.length - 1
                    Dim child = group.groups(j)
                    ' Propagate group properties (like padding, stiffness, ...) as module definition so that the generated power graph group will inherit it
                    Dim definition = {}
                    For Each prop As var In child.keys
                        If prop <> "leaves" AndAlso prop <> "groups" AndAlso child.hasOwnProperty(prop) Then
                            definition(prop) = child(prop)
                        End If
                    Next

                    ' Use negative module id to avoid clashes between predefined and generated modules
                    moduleSet.add(New [Module](-1 - j, New LinkSets(), New LinkSets(), Me.initModulesFromGroup(child), definition))
                Next
            End If
            Return moduleSet
        End Function

        ' merge modules a and b keeping track of their power edges and removing the from roots
        Private Function merge(a As [Module], b As [Module], Optional k As Double = 0) As [Module]
            Dim inInt = a.incoming.Intersection(b.incoming)
            Dim outInt = a.outgoing.Intersection(b.outgoing)
            Dim children = New ModuleSet()
            children.add(a)
            children.add(b)
            Dim m = New [Module](Me.modules.Length, outInt, inInt, children)
            Me.modules.push(m)
            Dim update = Function(s As LinkSets, i As String, o As String)
                             s.forAll(Function(ms, linktype)
                                          ms.forAll(Function(n)
                                                        Dim nls = DirectCast(n(i), LinkSets)
                                                        nls.add(linktype, m)
                                                        nls.remove(linktype, a)
                                                        nls.remove(linktype, b)
                                                        DirectCast(a(o), LinkSets).remove(linktype, n)
                                                        DirectCast(b(o), LinkSets).remove(linktype, n)

                                                    End Function)

                                      End Function)

                         End Function
            update(outInt, "incoming", "outgoing")
            update(inInt, "outgoing", "incoming")
            Me.R -= inInt.Count() + outInt.Count()
            Me.roots(k).remove(a)
            Me.roots(k).remove(b)
            Me.roots(k).add(m)
            Return m
        End Function

        Private Function rootMerges(Optional k As Double = 0) As any()
            Dim rs = Me.roots(k).modules()
            Dim n = rs.Length
            Dim merges = New Array(n * (n - 1))
            Dim ctr = 0
            Dim i As Integer = 0, i_ As Integer = n - 1
            While i < i_
                For j As var = i + 1 To n - 1
                    Dim a = rs(i)
                    Dim b = rs(j)
                    merges(ctr) = New With {
                    Key .id = ctr,
                    Key .nEdges = Me.nEdges(a, b),
                    Key .a = a,
                    Key .b = b
                }
                    ctr += 1
                Next
                i += 1
            End While
            Return merges
        End Function

        Private Function greedyMerge() As Boolean
            For i As var = 0 To Me.roots.Length - 1
                ' Handle single nested module case
                If Me.roots(i).modules().Length < 2 Then
                    Continue For
                End If

                ' find the merge that allows for the most edges to be removed.  secondary ordering based on arbitrary id (for predictability)
                Dim ms = Me.rootMerges(i).Sort(Function(a, b) If(a.nEdges = b.nEdges, a.id - b.id, a.nEdges - b.nEdges))
                Dim m = ms(0)
                If m.nEdges >= Me.R Then
                    Continue For
                End If
                Me.merge(m.a, m.b, i)
                Return True
            Next
        End Function

        Private Function nEdges(a As [Module], b As [Module]) As Double
            Dim inInt = a.incoming.Intersection(b.incoming)
            Dim outInt = a.outgoing.Intersection(b.outgoing)
            Return Me.R - inInt.Count() - outInt.Count()
        End Function

        Private Function getGroupHierarchy(retargetedEdges As PowerEdge()) As any()
            Dim groups = New Object() {}
            Dim root = New Object() {}
            toGroups(Me.roots(0), root, groups)
            Dim es = Me.allEdges()
            es.ForEach(Function(e)
                           Dim a = Me.modules(e.source)
                           Dim b = Me.modules(e.target)
                           retargetedEdges.push(New PowerEdge(If(a.gid Is Nothing, e.source, groups(a.gid)), If(b.gid Is Nothing, e.target, groups(b.gid)), e.type))

                       End Function)
            Return groups
        End Function

        Private Function allEdges() As PowerEdge()
            Dim es = New Object() {}
            System.Configuration.getEdges(Me.roots(0), es)
            Return es
        End Function

        Shared Sub New(modules As ModuleSet, es As PowerEdge())
            modules.forAll(Function(m)
                               m.getEdges(es)
                               System.Configuration.getEdges(m.children, es)

                           End Function)
        End Sub
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class
End Namespace