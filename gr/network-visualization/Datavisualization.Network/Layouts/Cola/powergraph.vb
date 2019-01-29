Imports number = System.Double
Imports any = System.Object

Interface LinkTypeAccessor(Of Link)
	Inherits LinkAccessor(Of Link)
	' return a unique identifier for the type of the link
	Function [getType](Link As any) As number
End Interface

Class PowerEdge

	Public source As any
	Public target As any
	Public type As number

	Private Sub New(source As any, target As any, type As number)
		Me.source = source
		Me.target = target
		Me.type = type
	End Sub
End Class

Class Configuration(Of Link)
	' canonical list of modules.
	' Initialized to a module for each leaf node, such that the ids and indexes of the module in the array match the indexes of the nodes in links
	' Modules created through merges are appended to the end of this.
	Private modules As [Module]()
	' top level modules and candidates for merges
	Private roots As ModuleSet()
	' remaining edge count
	Private R As number

	Public Sub New(n As number, edges As Link(), linkAccessor As LinkTypeAccessor(Of Link), rootGroup As any())
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
		Me.R = edges.length
		edges.forEach(Function(e) 
		Dim s = Me.modules(linkAccessor.getSourceIndex(e)), t = Me.modules(linkAccessor.getTargetIndex(e)), type = linkAccessor.[getType](e)
		s.outgoing.add(type, t)
		t.incoming.add(type, s)

End Function)
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
	Private Function merge(a As [Module], b As [Module], Optional k As number = 0) As [Module]
		Dim inInt = a.incoming.intersection(b.incoming)
		Dim outInt = a.outgoing.intersection(b.outgoing)
		Dim children = New ModuleSet()
		children.add(a)
		children.add(b)
		Dim m = New [Module](Me.modules.length, outInt, inInt, children)
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
		Me.R -= inInt.count() + outInt.count()
		Me.roots(k).remove(a)
		Me.roots(k).remove(b)
		Me.roots(k).add(m)
		Return m
	End Function

	Private Function rootMerges(Optional k As number = 0) As any()
		Dim rs = Me.roots(k).modules()
		Dim n = rs.length
		Dim merges = New Array(n * (n - 1))
		Dim ctr = 0
		Dim i As Integer = 0, i_ As Integer = n - 1
		While i < i_
			For j As var = i + 1 To n - 1
				Dim a = rs(i)
				Dim b = rs(j)
				merges(ctr) = New With { _
					Key .id = ctr, _
					Key .nEdges = Me.nEdges(a, b), _
					Key .a = a, _
					Key .b = b _
				}
				ctr += 1
			Next
			i += 1
		End While
		Return merges
	End Function

	Private Function greedyMerge() As Boolean
		For i As var = 0 To Me.roots.length - 1
			' Handle single nested module case
			If Me.roots(i).modules().length < 2 Then
				Continue For
			End If

			' find the merge that allows for the most edges to be removed.  secondary ordering based on arbitrary id (for predictability)
			Dim ms = Me.rootMerges(i).sort(Function(a, b) If(a.nEdges = b.nEdges, a.id - b.id, a.nEdges - b.nEdges))
			Dim m = ms(0)
			If m.nEdges >= Me.R Then
				Continue For
			End If
			Me.merge(m.a, m.b, i)
			Return True
		Next
	End Function

	Private Function nEdges(a As [Module], b As [Module]) As number
		Dim inInt = a.incoming.intersection(b.incoming)
		Dim outInt = a.outgoing.intersection(b.outgoing)
		Return Me.R - inInt.count() - outInt.count()
	End Function

	Private Function getGroupHierarchy(retargetedEdges As PowerEdge()) As any()
		Dim groups = New Object() {}
		Dim root = New Object() {}
		toGroups(Me.roots(0), root, groups)
		Dim es = Me.allEdges()
		es.forEach(Function(e) 
		Dim a = Me.modules(e.source)
		Dim b = Me.modules(e.target)
		retargetedEdges.push(New PowerEdge(If(a.gid Is Nothing, e.source, groups(a.gid)), If(b.gid Is Nothing, e.target, groups(b.gid)), e.type))

End Function)
		Return groups
	End Function

	Private Function allEdges() As PowerEdge()
		Dim es = New Object() {}
		Configuration.getEdges(Me.roots(0), es)
		Return es
	End Function

	Shared Sub New(modules As ModuleSet, es As PowerEdge())
		modules.forAll(Function(m) 
		m.getEdges(es)
		Configuration.getEdges(m.children, es)

End Function)
	End Sub
	Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
		target = value
		Return value
	End Function
End Class

Class powergraphExtensions

	Private Sub toGroups(modules As ModuleSet, group As any, groups As any)
		modules.forAll(Function(m) 
		If m.isLeaf() Then
			If Not group.leaves Then
				group.leaves = New Object() {}
			End If
			group.leaves.push(m.id)
		Else
			Dim g = group
			m.gid = groups.length
			If Not m.isIsland() OrElse m.isPredefined() Then
				g = New With { _
					Key .id = m.gid _
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
				groups.push(g)
			End If
			toGroups(m.children, g, groups)
		End If

End Function)
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

	Private Function intersectionCount(m As any, n As any) As number
		Return [Object].keys(intersection(m, n)).length
	End Function

	Private Function getGroups(Of Link)(nodes As any(), links As Link(), la As LinkTypeAccessor(Of Link), rootGroup As any()) As any
		Dim n = nodes.length
		Dim c = New Configuration(n, links, la, rootGroup)
		While c.greedyMerge()
			

		End While
		Dim powerEdges As PowerEdge() = {}
		Dim g = c.getGroupHierarchy(powerEdges)
		powerEdges.forEach(Function(e) 
		Dim f = Function([end]) 
		Dim g = e([end])
		If GetType(g) Is number Then
			e([end]) = nodes(g)
		End If

End Function
		f("source")
		f("target")

End Function)
		Return New With { _
			Key .groups = g, _
			Key .powerEdges = powerEdges _
		}
	End Function

End Class



Class [Module]
	Private gid As number

	Private id As number
	Public outgoing As New LinkSets()
	Public incoming As New LinkSets()
	Public children As New ModuleSet()
	Public definition As any

	Public Sub New(id As number, Optional outgoing As LinkSets = Nothing, Optional incoming As LinkSets = Nothing, Optional children As ModuleSet = Nothing, Optional definition As any = Nothing)
		Me.id = id
		Me.outgoing = outgoing
		Me.incoming = incoming
		Me.children = children
		Me.definition = definition
	End Sub

	Private Sub getEdges(es As PowerEdge())
		Me.outgoing.forAll(Function(ms, edgetype) 
		ms.forAll(Function(target) 
		es.push(New PowerEdge(Me.id, target.id, edgetype))

End Function)

End Function)
	End Sub

	Private Function isLeaf() As Boolean
		Return Me.children.count() = 0
	End Function

	Private Function isIsland() As Boolean
		Return Me.outgoing.count() = 0 AndAlso Me.incoming.count() = 0
	End Function

	Private Function isPredefined() As Boolean
		Return Me.definition IsNot Nothing
	End Function
End Class


Class ModuleSet
	Private table As any = New Object() {}
	Private Function count() As number
		Return [Object].keys(Me.table).length
	End Function
	Private Function intersection(other As ModuleSet) As ModuleSet
		Dim result = New ModuleSet()
		result.table = intersection(Me.table, other.table)
		Return result
	End Function
	Private Function intersectionCount(other As ModuleSet) As number
		Return Me.intersection(other).count()
	End Function
	Private Function contains(id As number) As Boolean
		Return Me.table.Have(id)
	End Function
	Private Sub add(m As [Module])
		Me.table(m.id) = m
	End Sub
	Private Sub remove(m As [Module])
		delete(Me.table, m.id)
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

Class LinkSets
	Private sets As any = New Object() {}
	Private n As number = 0
	Public Function count() As number
		Return Me.n
	End Function
	Private Function contains(id As number) As Boolean
		Dim result = False
		Me.forAllModules(Function(m) 
		If Not result AndAlso m.id = id Then
			result = True
		End If

End Function)
		Return result
	End Function
	Private Sub add(linktype As number, m As [Module])
		Dim s As ModuleSet = If(Me.sets.Have(linktype), Me.sets(linktype), InlineAssignHelper(Me.sets(linktype), New ModuleSet()))
		s.add(m)
		Me.n += 1
	End Sub
	Private Sub remove(linktype As number, m As [Module])
		Dim ms = DirectCast(Me.sets(linktype), ModuleSet)
		ms.remove(m)
		If ms.count() = 0 Then
			delete(Me.sets, linktype)
		End If
		Me.n -= 1
	End Sub
	Private Sub forAll(f As Action(Of ModuleSet, number))
		For Each linktype As var In Me.sets.keys
			f(DirectCast(Me.sets(linktype), ModuleSet), linktype)
		Next
	End Sub
	Private Sub forAllModules(f As Action(Of [Module]))
		Me.forAll(Function(ms, lt) ms.forAll(f))
	End Sub
	Private Function intersection(other As LinkSets) As LinkSets
		Dim result As New LinkSets()
		Me.forAll(Function(ms, lt) 
		If other.sets.Have(lt) Then
			Dim i = ms.intersection(other.sets(lt))
			Dim n = i.count()
			If n > 0 Then
				result.sets(lt) = i
				result.n += n
			End If
		End If

End Function)
		Return result
	End Function
	Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
		target = value
		Return value
	End Function
End Class

