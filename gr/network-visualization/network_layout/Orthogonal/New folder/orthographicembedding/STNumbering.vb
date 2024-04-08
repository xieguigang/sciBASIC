﻿Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ListExtensions

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 

Namespace Orthogonal.orthographicembedding

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class STNumbering
        Public Shared DEBUG As Integer = 0

        ' Implementation of the algorithm in:
        ' S. Even and R. E. Tarjan, Computing an st-numbering, Theoret. Comput. Sci. 2, (1976), 339-344.

        Public Shared Function stNumbering(graph As Integer()(), r As Random) As Integer()
            '        int n = graph.length;
            Return OrthographicEmbedding.STNumbering.stNumbering(graph, r.Next(graph.Length), r)
            ' 
            ' 	        if (DEBUG>=1) {
            ' 	            System.out.println("Computing stNumebring of graph:");
            ' 	            for(int i = 0;i<graph.length;i++) {            
            ' 	                for(int j = 0;j<graph.length;j++) {
            ' 	                    System.out.print(graph[i][j] + ",");
            ' 	                }
            ' 	                System.out.println("");
            ' 	            }
            ' 	        }
            ' 	        
            ' 	        // Part 1: Create a minimum spanning tree, preorder and L:
            ' 	        int [][]treeGraph = new int[n][n];
            ' 	        int [][]ancestors = new int[n][n];
            ' 	        int T[] = new int[n];
            ' 	        int preorder[] = new int[n];
            ' 	        int L[] = new int[n];
            ' 	        int t = 0;
            ' 	        int s = -1;
            ' 	        depthFirstSpanningTree(graph, treeGraph, ancestors, T, preorder, L, s, t);
            ' 	        for(int i = 0;i<n;i++) {
            ' 	            if (T[i]==t && i!=t) {
            ' 	                s = i;
            ' 	                break;
            ' 	            }
            ' 	        }
            ' 	        if (s==-1) {
            ' 	            System.err.println("WVisibility2Connected: spanning tree is malformed!");
            ' 	            return null;
            ' 	        }
            ' 	        if (DEBUG>=1) System.out.println("(s,t) = (" + s + "," + t + ")");
            ' 	
            ' 	        // Part 3: run the STNUMBER algorithm (part 2 is inside of STNUMBER)
            ' 	        int stnumbers[] = STNUMBER(s,t,graph,treeGraph, ancestors, preorder, L);
            ' 	        if (DEBUG>=1) System.out.println("st-numbers: " + Arrays.toString(stnumbers));
            ' 	        
            ' 	        if (DEBUG>=1) {
            ' 	            if (verifySTNumbering(graph, stnumbers)) {
            ' 	                System.out.println("STNumbering: st-numbering is correct!");
            ' 	            } else {
            ' 	                System.out.println("STNumbering: st-numbering has errors!");
            ' 	            }
            ' 	        }
            ' 	        
            ' 	        return stnumbers;
            ' 	        
        End Function

        Public Shared Function allSTNumberings(graph As Integer()()) As IList(Of Integer())
            Dim l As IList(Of Integer()) = New List(Of Integer())()
            For s = 0 To graph.Length - 1
                CType(l, List(Of Integer())).AddRange(OrthographicEmbedding.STNumbering.allSTNumberings(graph, s))
            Next
            Return l
        End Function

        Public Shared Function stNumbering(graph As Integer()(), s As Integer, r As Random) As Integer()
            Dim t = -1
            Dim candidates As IList(Of Integer) = New List(Of Integer)()
            For i = 0 To graph.Length - 1
                If graph(s)(i) = 1 Then
                    candidates.Add(i)
                End If
            Next
            If candidates.Count = 0 Then
                Return Nothing
            End If
            t = candidates(r.Next(candidates.Count))
            Return OrthographicEmbedding.STNumbering.stNumbering(graph, s, t)
        End Function

        Public Shared Function allSTNumberings(graph As Integer()(), s As Integer) As IList(Of Integer())
            Dim candidates As IList(Of Integer) = New List(Of Integer)()
            For i = 0 To graph.Length - 1
                If graph(s)(i) = 1 Then
                    candidates.Add(i)
                End If
            Next
            Dim l As IList(Of Integer()) = New List(Of Integer())()
            For Each t In candidates
                l.Add(OrthographicEmbedding.STNumbering.stNumbering(graph, s, t))
            Next
            Return l
        End Function

        Public Shared Function stNumbering(graph As Integer()(), s As Integer, t As Integer) As Integer()
            Dim n = graph.Length

            If OrthographicEmbedding.STNumbering.DEBUG >= 1 Then
                Console.WriteLine("Computing stNumebring of graph: (s,t) = (" & s.ToString() & "," & t.ToString() & ")")
                For i = 0 To graph.Length - 1
                    For j = 0 To graph.Length - 1
                        Console.Write(graph(i)(j).ToString() & ",")
                    Next
                    Console.WriteLine("")
                Next
            End If

            ' Part 1: Create a minimum spanning tree, preorder and L:
            Dim treeGraph = RectangularArray.Matrix(Of Integer)(n, n)

            Dim ancestors = RectangularArray.Matrix(Of Integer)(n, n)
            Dim lT = New Integer(n - 1) {}
            Dim preorder = New Integer(n - 1) {}
            Dim L = New Integer(n - 1) {}
            OrthographicEmbedding.STNumbering.depthFirstSpanningTree(graph, treeGraph, ancestors, lT, preorder, L, s, t)
            If OrthographicEmbedding.STNumbering.DEBUG >= 1 Then
                Console.WriteLine("forced (s,t) = (" & s.ToString() & "," & t.ToString() & ")")
            End If

            ' Part 3: run the STNUMBER algorithm (part 2 is inside of STNUMBER)
            Dim stnumbers As Integer() = OrthographicEmbedding.STNumbering.STNUMBER(s, t, graph, treeGraph, ancestors, preorder, L)

            If OrthographicEmbedding.STNumbering.DEBUG >= 1 Then
                If OrthographicEmbedding.STNumbering.verifySTNumbering(graph, stnumbers) Then
                    Console.WriteLine("STNumbering: st-numbering is correct!")
                Else
                    Console.WriteLine("STNumbering: st-numbering has errors!")
                End If
            End If

            Return stnumbers
        End Function


        Public Shared Function verifySTNumbering(graph As Integer()(), stNumbering As Integer()) As Boolean
            Dim n = graph.Length

            ' find s and t:
            Dim s = -1, t = -1
            For i = 0 To n - 1
                If stNumbering(i) = 1 Then
                    If s <> -1 Then
                        Return False
                    End If
                    s = i
                End If
                If stNumbering(i) = n Then
                    If t <> -1 Then
                        Return False
                    End If
                    t = i
                End If
            Next

            ' verify that each other node has a lower and a higher neighbor:
            For v = 0 To n - 1
                If v <> s AndAlso v <> t Then
                    Dim hasHigher = False
                    Dim hasLower = False
                    For w = 0 To n - 1
                        If graph(v)(w) = 1 AndAlso stNumbering(w) < stNumbering(v) Then
                            hasLower = True
                        End If
                        If graph(v)(w) = 1 AndAlso stNumbering(w) > stNumbering(v) Then
                            hasHigher = True
                        End If
                    Next
                    If Not hasLower OrElse Not hasHigher Then
                        Return False
                    End If
                End If
            Next

            Return True
        End Function

        Friend Shared Function STNUMBER(s As Integer, t As Integer, graph As Integer()(), treeGraph As Integer()(), ancestors As Integer()(), preorder As Integer(), L As Integer()) As Integer()
            Dim n = graph.Length
            Dim lStnumber = New Integer(n - 1) {}
            Dim oldEdges = RectangularArray.Matrix(Of Boolean)(n, n)
            Dim oldNodes = New Boolean(n - 1) {}
            ' initially all edges and nodes are "new"
            For v = 0 To n - 1
                lStnumber(v) = -1
                oldNodes(v) = False
                For w = 0 To n - 1
                    oldEdges(v)(w) = False
                Next
            Next

            oldNodes(s) = True
            oldNodes(t) = True
            oldEdges(s)(t) = True
            oldEdges(t)(s) = True
            Dim stack As List(Of Integer) = New List(Of Integer)()
            stack.Add(t)
            stack.Add(s)
            Dim i = 0
            While stack.Count > 0

                Dim v = stack.PopAt(stack.Count - 1)
                If OrthographicEmbedding.STNumbering.DEBUG >= 2 Then
                    Console.WriteLine("STNUMBER: popped " & v.ToString())
                End If

                Dim path As IList(Of Integer) = OrthographicEmbedding.STNumbering.PATHFINDER(v, graph, treeGraph, oldEdges, oldNodes, ancestors, preorder, L)

                If OrthographicEmbedding.STNumbering.DEBUG >= 1 AndAlso path IsNot Nothing Then
                    ' verify this is a "simple path" (no repeated vertices):
                    For j = 0 To path.Count - 1
                        For k = j + 1 To path.Count - 1
                            If path(j).Equals(path(k)) Then
                                Throw New Exception("PATHFINDER did not return a simple path!")
                            End If
                        Next
                    Next
                End If
                If path Is Nothing Then
                    i += 1
                    lStnumber(v) = i
                    If OrthographicEmbedding.STNumbering.DEBUG >= 2 Then
                        Console.WriteLine("STNUMBER: stNumber(" & v.ToString() & ") = " & i.ToString())
                    End If
                Else
                    ' add all the elements to the stack except the last:
                    For j = path.Count - 2 To 0 Step -1
                        stack.Add(path(j))
                    Next
                End If
                If OrthographicEmbedding.STNumbering.DEBUG >= 2 Then
                    Console.WriteLine()
                End If
            End While

            Return lStnumber
        End Function

        Friend Shared Function PATHFINDER(v As Integer, graph As Integer()(), treeGraph As Integer()(), oldEdges As Boolean()(), oldNodes As Boolean(), ancestors As Integer()(), preorder As Integer(), L As Integer()) As IList(Of Integer)
            Dim path As IList(Of Integer) = New List(Of Integer)()
            Dim n = graph.Length
            ' if there is a new cycle edge {v,w} with w -*-> v:
            For w = 0 To n - 1
                ' 
                ' 				if (graph[v][w]==1)
                ' 				    System.out.println("a) {" + v + "," + w + "} : " + 
                ' 				                       (oldEdges[v][w] ? "old":"new") + " " + 
                ' 				                       (ancestors[w][v]==1 ? (w + "-*->" + "v"):""));
                ' 				
                If Not oldEdges(v)(w) AndAlso graph(v)(w) = 1 AndAlso ancestors(w)(v) = 1 Then
                    If OrthographicEmbedding.STNumbering.DEBUG >= 2 Then
                        Console.WriteLine("* PATHFINDER a)")
                    End If
                    oldEdges(v)(w) = True
                    oldEdges(w)(v) = True
                    path.Add(v)
                    path.Add(w)
                    Return path
                End If
            Next
            ' if there is a new tree edge v->w:
            For w = 0 To n - 1
                If Not oldEdges(v)(w) AndAlso treeGraph(v)(w) = 1 Then
                    If OrthographicEmbedding.STNumbering.DEBUG >= 2 Then
                        Console.WriteLine("* PATHFINDER b)")
                    End If
                    oldEdges(v)(w) = True
                    oldEdges(w)(v) = True
                    path.Add(v)
                    path.Add(w)
                    While Not oldNodes(w)
                        Dim found = False
                        For x = 0 To n - 1
                            ' the condition "!path.contains(x)" was not in the original
                            ' Even & Tarjan paper, but I had to add it, otherwise, the algorithm
                            ' sometimes returned paths that were not simple!
                            If Not path.Contains(x) Then
                                If Not oldEdges(w)(x) AndAlso graph(w)(x) = 1 AndAlso (preorder(x) = L(w) OrElse L(x) = L(w)) Then
                                    oldNodes(w) = True
                                    oldEdges(w)(x) = True
                                    oldEdges(x)(w) = True
                                    path.Add(x)
                                    w = x
                                    found = True
                                    Exit For
                                End If
                            End If
                        Next
                        If Not found Then
                            For i = 0 To graph.Length - 1
                                Console.Error.Write("{")
                                For j = 0 To graph.Length - 1
                                    Console.Error.Write(graph(i)(j).ToString() & ",")
                                Next
                                Console.Error.WriteLine("},")
                            Next
                            Throw New Exception("PAHFINDER: inside case (b), couldn't find next node in path.")
                        End If
                    End While
                    Return path
                End If
            Next
            ' if there is a new cycle edge {v,w} with v -*-> w:
            For w = 0 To n - 1
                If Not oldEdges(v)(w) AndAlso graph(v)(w) = 1 AndAlso treeGraph(v)(w) = 0 AndAlso ancestors(v)(w) = 1 Then
                    If OrthographicEmbedding.STNumbering.DEBUG >= 2 Then
                        Console.WriteLine("* PATHFINDER c)")
                    End If
                    oldEdges(v)(w) = True
                    oldEdges(w)(v) = True
                    path.Add(v)
                    path.Add(w)
                    While Not oldNodes(w)
                        Dim found = False
                        For x = 0 To n - 1
                            If Not oldEdges(w)(x) AndAlso treeGraph(x)(w) = 1 Then
                                oldNodes(w) = True
                                oldEdges(w)(x) = True
                                oldEdges(x)(w) = True
                                path.Add(x)
                                w = x
                                found = True
                                Exit For
                            End If
                        Next
                        If Not found Then
                            If Not found Then
                                Throw New Exception("PAHFINDER: inside case (c), couldn't find next node in path.")
                            End If
                        End If
                    End While
                    Return path
                End If
            Next
            Return Nothing
        End Function


        ' Computes a depth-first spanning tree of the graph, and outputs:
        '  treeGraph: the tree represented as a graph
        '  ancestors: ancestors[i][j]==1 if i is an ancestor or j
        '  T: the parent of each node in the spanning tree 
        '  preorder: the nodes sorted in preorder according to the spanning tree
        '  L: the L values for each node (see paper referred above)
        '  s, and t are the s and t nodes: s can be set to -1 if you want it to be determined automatically
        Public Shared Sub depthFirstSpanningTree(graph As Integer()(), treeGraph As Integer()(), ancestors As Integer()(), T As Integer(), preorder As Integer(), L As Integer(), s As Integer, pT As Integer)
            Dim n = graph.Length
            Dim stack As List(Of Integer) = New List(Of Integer)()
            Dim parents As List(Of Integer) = New List(Of Integer)()
            For i = 0 To graph.Length - 1
                T(i) = -1
            Next
            stack.Add(pT)
            parents.Add(pT)
            ' if (s!=-1) System.out.println("s->t: " + graph[t][s]);
            While stack.Count > 0
                Dim current = stack.PopAt(stack.Count - 1)
                Dim parent = parents.PopAt(parents.Count - 1)
                ' System.out.println("DFST: " + current + " -> " + parent);
                If T(current) = -1 Then
                    T(current) = parent
                    For [Next] = graph.Length - 1 To 0 Step -1
                        If graph(current)([Next]) = 1 Then
                            ' wait, to add it at the end (and make sure it's taken the first)
                            If [Next]() = s AndAlso current = pT Then
                            Else
                                If T([Next]) = -1 Then
                                    stack.Add([Next])
                                    parents.Add(current)
                                End If
                            End If
                        End If
                    Next
                    If s <> -1 AndAlso current = pT AndAlso graph(current)(s) = 1 Then
                        stack.Add(s)
                        parents.Add(current)
                    End If
                End If
            End While


            ' Compute the preorder:
            Dim counter = 1
            stack = New List(Of Integer)()
            stack.Add(pT)
            While stack.Count > 0
                Dim current = stack.PopAt(0)
                If OrthographicEmbedding.STNumbering.DEBUG >= 1 Then
                    Console.WriteLine("  computing preorder: " & current.ToString())
                End If
                preorder(current) = Math.Min(Threading.Interlocked.Increment(counter), counter - 1)
                Dim tmp = 0
                For i = 0 To n - 1
                    If T(i) = current AndAlso i <> current Then
                        stack.Insert(tmp, i)
                        tmp += 1
                    End If
                Next
            End While

            ' Compute the L numbers (not the most efficient way to do it):
            For v = 0 To n - 1
                For w = 0 To n - 1
                    treeGraph(v)(w) = 0
                    ancestors(v)(w) = 0
                Next
            Next
            For v = 0 To n - 1
                If T(v) <> v Then
                    treeGraph(T(v))(v) = 1
                End If
                ancestors(v)(v) = 1
                Dim tmp = v
                While T(tmp) <> tmp
                    ancestors(T(tmp))(v) = 1
                    tmp = T(tmp)
                End While
            Next
            For v = 0 To n - 1
                L(v) = preorder(v)
                For w = 0 To n - 1
                    ' if (w is a descendant of v)
                    ' and w is connected to u through an edge that is NOT in the spanning tree
                    If ancestors(v)(w) = 1 Then
                        '                    System.out.println(w + " is a descendant of " + v);
                        For u = 0 To n - 1
                            If graph(w)(u) = 1 AndAlso treeGraph(w)(u) = 0 Then
                                If preorder(u) < L(v) Then
                                    L(v) = preorder(u)
                                End If
                            End If
                        Next
                    End If
                Next
            Next
        End Sub
    End Class

End Namespace
