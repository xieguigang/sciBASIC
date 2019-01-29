Imports any = System.Object
Imports number = System.Double

Class batch

	'*
'     * @property nudgeGap spacing between parallel edge segments
'     * @property margin space around nodes
'     * @property groupMargin space around groups
'     

	Private Function gridify(pgLayout As Object, nudgeGap As number, margin As number, groupMargin As number) As Object
		pgLayout.cola.start(0, 0, 0, 10, False)
		Dim gridrouter = route(pgLayout.cola.nodes(), pgLayout.cola.groups(), margin, groupMargin)
		Return gridrouter.routeEdges(Of Object)(pgLayout.powerGraph.powerEdges, nudgeGap, Function(e) e.source.routerNode.id, Function(e) e.target.routerNode.id)
	End Function

	Private Function route(nodes As any, groups As any, margin As number, groupMargin As number) As GridRouter(Of Object)
		nodes.forEach(Function(d) 
		d.routerNode = New With { _
			Key .name = d.name, _
			Key .bounds = d.bounds.inflate(-margin) _
		}

End Function)
		groups.forEach(Function(d) 
		d.routerNode = New With { _
			Key .bounds = d.bounds.inflate(-groupMargin), _
			Key .children = (If(d.groups IsNot Nothing, d.groups.map(Function(c) nodes.length + c.id), New Object() {})).concat(If(d.leaves IsNot Nothing, d.leaves.map(Function(c) c.index), New Object() {})) _
		}

End Function)
		Dim gridRouterNodes = nodes.concat(groups).map(Function(d, i) 
		d.routerNode.id = i
		Return d.routerNode

End Function)
		Return New GridRouter(gridRouterNodes, New With { _
			Key .getChildren = Function(v As Object) v.children, _
			Key .getBounds = Function(v) v.bounds _
		}, margin - groupMargin)
	End Function

	Private Interface network
		Property nodes() As Node()
		Property links() As Link(Of Node)()

	End Interface

	Private Function powerGraphGridLayout(graph As network, size As number(), grouppadding As number) As Object
		' compute power graph
		Dim powerGraph = Nothing
		graph.nodes.forEach(Function(v, i) InlineAssignHelper(v.index, i))

		New Layout().avoidOverlaps(False).nodes(graph.nodes).links(graph.links).powerGraphGroups(Function(d) 
		powerGraph = d
		powerGraph.groups.forEach(Function(v) InlineAssignHelper(v.padding, grouppadding))

End Function)

		' construct a flat graph with dummy nodes for the groups and edges connecting group dummy nodes to their children
		' power edges attached to groups are replaced with edges connected to the corresponding group dummy node
		Dim n = graph.nodes.length
		Dim edges = New Object() {}
		Dim vs = graph.nodes.slice(0)
		vs.forEach(Function(v, i) InlineAssignHelper(v.index, i))
		powerGraph.groups.forEach(Function(g) 
		Dim sourceInd = InlineAssignHelper(g.index, g.id + n)
		vs.push(g)
		If g.leaves IsNot Nothing Then
			g.leaves.forEach(Function(v) edges.push(New With { _
				Key .source = sourceInd, _
				Key .target = v.index _
			}))
		End If
		If g.groups IsNot Nothing Then
			g.groups.forEach(Function(gg) edges.push(New With { _
				Key .source = sourceInd, _
				Key .target = gg.id + n _
			}))
		End If

End Function)
		powerGraph.powerEdges.forEach(Function(e) 
		edges.push(New With { _
			Key .source = e.source.index, _
			Key .target = e.target.index _
		})

End Function)

		' layout the flat graph with dummy nodes and edges
		New Layout().size(size).nodes(vs).links(edges).avoidOverlaps(False).linkDistance(30).symmetricDiffLinkLengths(5).convergenceThreshold(0.0001).start(100, 0, 0, 0, False)

		' final layout taking node positions from above as starting positions
		' subject to group containment constraints
		' and then gridifying the layout
			'.flowLayout('y', 30)


		Return New With { _
			Key .cola = New Layout().convergenceThreshold(0.001).size(size).avoidOverlaps(True).nodes(graph.nodes).links(graph.links).groupCompactness(0.0001).linkDistance(30).symmetricDiffLinkLengths(5).powerGraphGroups(Function(d) 
			powerGraph = d
			powerGraph.groups.forEach(Function(v) InlineAssignHelper(v.padding, grouppadding))

End Function).start(50, 0, 100, 0, False), _
			Key .powerGraph = powerGraph _
		}
	End Function
	Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
		target = value
		Return value
	End Function
End Class
