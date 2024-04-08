Imports std = System.Math

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 

Namespace Orthogonal

    ''' <summary>
    ''' # OGE
    ''' 
    ''' Orthographic Graph Embedder (OGE) v1.1 by Santiago Ontañón (2016-2017)  
    '''
    ''' This tool computes an orthographic embedding of a plannar input graph. Although the tool was originally designed to be part of a procedural-content generation (PCG) module for a game, it is designed to be usable to find orthographic embeddings for any planar input graphs.
    '''
    ''' OGE uses an algorithm that uses st-numberings to construct a weak-visibility representation, and then PQ-trees to generate the orthographic embeddings. If the input graph is plannar, this procedure guarantees that an orthographic embedding without any edges crossing over any other vertices will be found. The algorihtms implemented in this package were adapted from the following papers:  
    ''' - S. Even and R. E. Tarjan, "Computing an st-numbering", Theoret. Comput. Sci. 2, (1976), 339-344.  
    ''' - Tamassia, Roberto, and Ioannis G. Tollis. "Planar Grid Embedding in Linear Time"  
    ''' - Tamassia, Roberto, and Ioannis G. Tollis. "A unified approach to visibility representations of planar graphs." Discrete &amp; Computational Geometry 1.4 (1986): 321-341.  
    '''
    ''' Example usage: java -classpath OGE.jar Main data/graph1 oe1.txt -png:oe1.png  
    '''
    ''' parameters: input-file output-file options   
    ''' - input-file: a file containing the adjacency matrix of a graph  
    ''' - output-file: the desired output filename  
    ''' - Options:  
    '''   - -output:[type] : the type of output desired, which can be:  
    '''     - txt (default): a text file with the connectivity matrix, and then a list of vertices, with their mapping to the original vertices, and their coordinates in the orthographic embedding. 
    '''     (more output types might be added in the future)  
    '''   - -png:filename : saves a graphical version of the output as a .png file  
    '''   - -simplify:true/false : defaults to true, applies a filter to try to reduce unnecessary auxiliary vertices.  
    '''   - -optimize:true/false : defaults to true, postprocesses the output to try to make it more compact.  
    '''   - -rs:XXX : specifies the random seed for the random number generator.
    '''
    ''' For example, providing this input graph (included as an example in the "examples" folder as "graph2.txt"), where a graph is represented as the adjacency matrix between each pair of vertices ("0" is no edge and "1" is edge):  
    '''
    ''' ```
    ''' 0,1,0,0,0,0,0,0,0,1,1  
    ''' 1,0,1,1,0,0,0,0,0,0,0  
    ''' 0,1,0,0,0,0,0,0,0,0,0  
    ''' 0,1,0,0,1,1,1,0,0,0,0  
    ''' 0,0,0,1,0,1,0,0,0,0,0  
    ''' 0,0,0,1,1,0,0,0,0,0,0  
    ''' 0,0,0,1,0,0,0,1,1,0,0  
    ''' 0,0,0,0,0,0,1,0,1,0,0  
    ''' 0,0,0,0,0,0,1,1,0,1,0  
    ''' 1,0,0,0,0,0,0,0,1,0,1  
    ''' 1,0,0,0,0,0,0,0,0,1,0  
    ''' ```
    ''' 
    ''' The program generates an orthographic embedding that looks like this (where the large vertices with numbers in them represent the original vertices of the input graph, and the smaller vertices are auxiliary vertices that had to be added in order to generate the projection, assuming that edges cannot have "elbows"):  
    '''
    ''' ![graph2 png output](examples/oe2.png)
    '''
    ''' The output embedding is saved in a text file that contains:
    ''' - First the adjacency matrix of the graph (including any auxiliary vertices that might have had to be added)
    ''' - Second a list of vertices of the new (projected) graph, with the index of the original node in the input graph they correspond to (or -1 if the vertex is a new auxiliary vertex), and the x, y coordinates of the vertex in the orthographic embedding.
    ''' 
    ''' @author santi
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/santiontanon/OGE
    ''' </remarks>
    Public Class OrthographicEmbedding
        Public Shared DEBUG As Integer = 0


        Public Shared Function orthographicEmbedding(graph As Integer()(), simplify As Boolean, fixNonOrthogonal As Boolean) As OrthographicEmbeddingResult
            Dim n = graph.Length
            Dim embedding = New OEVertex(n - 1) {}

            ' Algorithm from: "Planar Grid Embedding in Linear Time" Tamasia and Tollis
            ' Step 1: Construct a visibility representation Gamma for the graph
            Dim Gamma As New Visibility(graph)
            If Not Gamma.WVisibility() Then
                Return Nothing
            End If
            Gamma.reorganize()

            ' from this point on, we assume that the result is aligned with a grid size od 1.0
            ' Step 2: Transform Gamma into an orthogonal embedding G' by substituting each vertex segment
            '         with one of the structures shown in Fig. 5
            For v = 0 To n - 1
                embedding(v) = vertexOrtographicEmbedding(v, graph, Gamma, embedding)
            Next

            '        if (simplify) attemptCompleteSimplification(embedding);
            If simplify Then
                Return cautiousSimplification(embedding, Gamma, fixNonOrthogonal)
            Else
                ' Step 4: Let H' be the orthogonal representation so obtained. 
                '         Construct from H' a grid embedding for G using the compaction algorithm of Lemma 1        
                ' (I ignore that, and just provide my own algorihtm for it)
                Return New OrthographicEmbeddingResult(embedding, Gamma, fixNonOrthogonal)
            End If
        End Function


        ' Makes simplifications one by one, trying to generate an actual 2d representation, and only consider those for which my simple
        ' 2d algorithm generates graphs that are correct:

        Public Shared Function cautiousSimplification(embedding As OEVertex(), Gamma As Visibility, fixNonOrthogonal As Boolean) As OrthographicEmbeddingResult
            Dim n = embedding.Length
            Dim best As New OrthographicEmbeddingResult(embedding, Gamma, fixNonOrthogonal)
            Dim current As OrthographicEmbeddingResult = Nothing

            ' Step 3: Let H be the orthogonal representation of G'. 
            '         Simplify H by means of the bend-stretching transformations
            For v = 0 To n - 1
                For Each oev As OEElement In embedding(v).embedding
                    Dim w As Integer = oev.dest
                    Dim oew As OEElement = sym(oev, embedding)
                    ' Apply T1:
                    If oev.bends >= 1 AndAlso oew.bends >= 1 Then
                        If DEBUG >= 1 Then
                            Console.WriteLine("T1: " & v.ToString() & "->" & w.ToString())
                        End If
                        Dim x As Integer = oev.bends
                        Dim y As Integer = oew.bends
                        Dim buffer1 As Integer = oev.bends
                        Dim buffer2 As Integer = oew.bends
                        oev.bends = std.Max(0, x - y)
                        oew.bends = std.Max(0, y - x)
                        current = New OrthographicEmbeddingResult(embedding, Gamma, fixNonOrthogonal)
                        If current.sanityCheck(True) Then
                            best = current
                        Else
                            ' undo!
                            oev.bends = buffer1
                            oew.bends = buffer2
                        End If
                    End If
                Next
            Next

            For v = 0 To n - 1
                ' Apply T2: (case 1)
                Dim min = -1
                For Each oev As OEElement In embedding(v).embedding
                    If min = -1 OrElse oev.bends < min Then
                        min = oev.bends
                    End If
                Next
                If min > 0 Then
                    If DEBUG >= 1 Then
                        Console.WriteLine("T2(1): " & v.ToString())
                    End If
                    For Each oev As OEElement In embedding(v).embedding
                        oev.bends -= min
                        Dim oew As OEElement = sym(oev, embedding)
                        ' update the angle: (this was not in the original algorithm, 
                        ' but it's necessary, since I store absolute angles, instead of relative ones
                        oew.angle = (oew.angle + min) Mod 4
                    Next
                    current = New OrthographicEmbeddingResult(embedding, Gamma, fixNonOrthogonal)
                    If current.sanityCheck(True) Then
                        best = current
                    Else
                        ' undo!
                        For Each oev As OEElement In embedding(v).embedding
                            oev.bends += min
                            Dim oew As OEElement = sym(oev, embedding)
                            oew.angle = (oew.angle + min) Mod 4
                        Next
                    End If
                End If

                ' Apply T2: (case 2)
                min = -1
                For Each oev As OEElement In embedding(v).embedding
                    Dim oew As OEElement = sym(oev, embedding)
                    If min = -1 OrElse oew.bends < min Then
                        min = oew.bends
                    End If
                Next
                If min > 0 Then
                    If DEBUG >= 1 Then
                        Console.WriteLine("T2(2): " & v.ToString())
                    End If
                    For Each oev As OEElement In embedding(v).embedding
                        Dim oew As OEElement = sym(oev, embedding)
                        oew.bends -= min
                        ' update the angle: (this was not in the original algorithm, 
                        ' but it's necessary, since I store absolute angles, instead of relative ones
                        oev.angle = (oev.angle + min) Mod 4
                    Next
                    current = New OrthographicEmbeddingResult(embedding, Gamma, fixNonOrthogonal)
                    If current.sanityCheck(True) Then
                        best = current
                    Else
                        ' undo!
                        For Each oev As OEElement In embedding(v).embedding
                            Dim oew As OEElement = sym(oev, embedding)
                            oew.bends += min
                            oev.angle = (oev.angle + min) Mod 4
                        Next
                    End If
                End If
            Next

            For v = 0 To n - 1
                If embedding(v).embedding.Count > 1 AndAlso embedding(v).embedding.Count <= 3 Then
                    For e As Integer = 0 To embedding(v).embedding.Count - 1
                        Dim tryagain = True
                        While tryagain
                            tryagain = False
                            ' Apply T3: (case 1)
                            Dim e2 As Integer = (e + 1) Mod embedding(v).embedding.Count
                            Dim e3 As Integer = (e + 2) Mod embedding(v).embedding.Count
                            Dim oev As OEElement = embedding(v).embedding(e)
                            Dim oev2 As OEElement = embedding(v).embedding(e2)
                            Dim oev3 As OEElement = embedding(v).embedding(e3)
                            Dim e_angle As Integer = oev2.angle - oev.angle
                            Dim e2_angle As Integer = oev3.angle - oev2.angle
                            If e_angle < 0 Then
                                e_angle += 4
                            End If
                            If e2_angle < 0 Then
                                e2_angle += 4
                            End If
                            If e_angle >= 2 AndAlso oev2.bends >= 1 Then
                                If DEBUG >= 1 Then
                                    Console.WriteLine("T3(1): " & v.ToString() & "->" & oev2.dest.ToString())
                                End If
                                If DEBUG >= 1 Then
                                    Console.WriteLine("e: " & v.ToString() & "->" & oev.dest.ToString() & ", e': " & oev2.v.ToString() & "->" & oev2.dest.ToString() & ", angle(e) = " & e_angle.ToString() & ", bends(e') = " & oev2.bends.ToString())
                                End If
                                Dim m As Integer = std.Min(e_angle - 1, oev2.bends)

                                Dim buffer1 As Integer = oev2.angle
                                Dim buffer2 As Integer = oev2.bends
                                oev2.angle -= m
                                If oev2.angle < 0 Then
                                    oev2.angle += 4
                                End If
                                oev2.bends = oev2.bends - m
                                If DEBUG >= 1 Then
                                    Console.WriteLine("  result (e'): " & oev2.ToString())
                                End If
                                If DEBUG >= 1 Then
                                    Console.WriteLine("  result (sym(e')): " & sym(oev2, embedding).ToString())
                                End If
                                current = New OrthographicEmbeddingResult(embedding, Gamma, fixNonOrthogonal)
                                If current.sanityCheck(True) Then
                                    tryagain = True
                                    best = current
                                Else
                                    ' undo!
                                    oev2.angle = buffer1
                                    oev2.bends = buffer2
                                End If
                            End If
                            If Not tryagain Then
                                ' Apply T3: (case 2)
                                Dim oew2 As OEElement = sym(oev2, embedding)
                                If e2_angle >= 2 AndAlso oew2.bends >= 1 Then
                                    If DEBUG >= 1 Then
                                        Console.WriteLine("T3(2): " & v.ToString() & "->" & oev2.dest.ToString())
                                    End If
                                    If DEBUG >= 1 Then
                                        Console.WriteLine("e: " & v.ToString() & "->" & oev.dest.ToString() & ", e': " & oev2.v.ToString() & "->" & oev2.dest.ToString() & ", angle(e') = " & e2_angle.ToString() & ", bends(e') = " & oev2.bends.ToString())
                                    End If
                                    Dim m As Integer = std.Min(e2_angle - 1, oew2.bends)

                                    Dim buffer1 As Integer = oev2.angle
                                    Dim buffer2 As Integer = oew2.bends
                                    oev2.angle += m
                                    If oev2.angle >= 4 Then
                                        oev2.angle -= 4
                                    End If
                                    oew2.bends = oew2.bends - m
                                    If DEBUG >= 1 Then
                                        Console.WriteLine("  result (e'): " & oev2.ToString())
                                    End If
                                    If DEBUG >= 1 Then
                                        Console.WriteLine("  result (sym(e')): " & oew2.ToString())
                                    End If
                                    current = New OrthographicEmbeddingResult(embedding, Gamma, fixNonOrthogonal)
                                    If current.sanityCheck(True) Then
                                        tryagain = True
                                        best = current
                                    Else
                                        ' undo!
                                        oev2.angle = buffer1
                                        oew2.bends = buffer2
                                    End If
                                End If
                            End If
                        End While
                    Next
                End If
            Next

            Return best
        End Function


        Friend Shared Function sym(oev As OEElement, embedding As OEVertex()) As OEElement
            Dim w As Integer = oev.dest
            Dim ew As OEVertex = embedding(w)
            For Each tmp As OEElement In ew.embedding
                If tmp.dest = oev.v Then
                    Return tmp
                End If
            Next
            Return Nothing
        End Function


        Friend Shared Function vertexOrtographicEmbedding(v As Integer, graph As Integer()(), Gamma As Visibility, embedding As OEVertex()) As OEVertex
            Dim vertexEmbedding As IList(Of OEElement) = New List(Of OEElement)()
            Dim x As Double = -1, y As Double = -1
            Dim n = graph.Length
            Dim tolerance = 0.1

            If DEBUG >= 1 Then
                Console.WriteLine("Generating ortographic embedding for node " & v.ToString() & ":")
            End If
            Dim edgesOnTop As IList(Of Integer) = New List(Of Integer)()
            Dim edgesBelow As IList(Of Integer) = New List(Of Integer)()
            Dim vertexY As Double = Gamma.horizontal_y(v)
            For w = 0 To n - 1
                If graph(v)(w) <> 0 Then
                    Dim vIndex As Integer = Gamma.edgeIndexes(v)(w)
                    If Gamma.vertical_y1(vIndex) < vertexY - 0.5 OrElse Gamma.vertical_y2(vIndex) < vertexY - 0.5 Then
                        ' insert in the right spot:
                        Dim inserted = False
                        For i = 0 To edgesOnTop.Count - 1
                            If Gamma.vertical_x(vIndex) < Gamma.vertical_x(Gamma.edgeIndexes(v)(edgesOnTop(i))) Then
                                edgesOnTop.Insert(i, w)
                                inserted = True
                                Exit For
                            End If
                        Next
                        If Not inserted Then
                            edgesOnTop.Add(w)
                        End If
                    Else
                        ' insert in the right spot:
                        Dim inserted = False
                        For i = 0 To edgesBelow.Count - 1
                            If Gamma.vertical_x(vIndex) < Gamma.vertical_x(Gamma.edgeIndexes(v)(edgesBelow(i))) Then
                                edgesBelow.Insert(i, w)
                                inserted = True
                                Exit For
                            End If
                        Next
                        If Not inserted Then
                            edgesBelow.Add(w)
                        End If
                    End If
                End If
            Next

            ' possible cases:
            Dim ntop = edgesOnTop.Count
            Dim nbelow = edgesBelow.Count
            If ntop = 1 AndAlso nbelow = 0 Then ' (a)
                Dim w = edgesOnTop(0)
                Dim e As OEElement = New OEElement(v, w, OEElement.UP, 0)
                y = Gamma.horizontal_y(v)
                x = Gamma.vertical_x(Gamma.edgeIndexes(v)(w))
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (a).1: " & x.ToString() & "," & y.ToString())
                End If
            ElseIf ntop = 0 AndAlso nbelow = 1 Then ' (a)
                Dim w = edgesBelow(0)
                Dim e As OEElement = New OEElement(v, w, OEElement.DOWN, 0)
                y = Gamma.horizontal_y(v)
                x = Gamma.vertical_x(Gamma.edgeIndexes(v)(w))
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (a).2: " & x.ToString() & "," & y.ToString())
                End If
            ElseIf ntop = 2 AndAlso nbelow = 0 Then ' (b)
                Dim w = edgesOnTop(0)
                Dim e As OEElement = New OEElement(v, w, OEElement.UP, 0)
                y = Gamma.horizontal_y(v)
                x = Gamma.vertical_x(Gamma.edgeIndexes(v)(w))
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesOnTop(1)
                e = New OEElement(v, w, OEElement.RIGHT, 1)
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (b).1: " & x.ToString() & "," & y.ToString())
                End If
            ElseIf ntop = 1 AndAlso nbelow = 1 Then ' (c)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (c).1")
                End If
                Dim xtop As Double = Gamma.vertical_x(Gamma.edgeIndexes(v)(edgesOnTop(0)))
                Dim xbot As Double = Gamma.vertical_x(Gamma.edgeIndexes(v)(edgesBelow(0)))
                Dim w = edgesOnTop(0)
                y = Gamma.horizontal_y(v)
                x = (xtop + xbot) / 2
                If std.Abs(xtop - xbot) < tolerance Then
                    Dim e As OEElement = New OEElement(v, w, OEElement.UP, 0)
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)

                    w = edgesBelow(0)
                    e = New OEElement(v, w, OEElement.DOWN, 0)
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)
                ElseIf xtop < xbot Then
                    Dim e As OEElement = New OEElement(v, w, OEElement.LEFT, 0)
                    e.bendsToAddToSymmetric = 1
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)

                    w = edgesBelow(0)
                    e = New OEElement(v, w, OEElement.RIGHT, 0)
                    e.bendsToAddToSymmetric = 1
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)
                Else
                    Dim e As OEElement = New OEElement(v, w, OEElement.RIGHT, 1)
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)

                    w = edgesBelow(0)
                    e = New OEElement(v, w, OEElement.LEFT, 1)
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)
                End If
            ElseIf ntop = 0 AndAlso nbelow = 2 Then ' (b)
                Dim w = edgesBelow(1)
                Dim e As OEElement = New OEElement(v, w, OEElement.DOWN, 0)
                y = Gamma.horizontal_y(v)
                x = Gamma.vertical_x(Gamma.edgeIndexes(v)(w))
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesBelow(0)
                e = New OEElement(v, w, OEElement.LEFT, 1)
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (b).2: " & x.ToString() & "," & y.ToString())
                End If
            ElseIf ntop = 3 AndAlso nbelow = 0 Then ' (d)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (d).1")
                End If
                Dim w = edgesOnTop(0)
                Dim e As OEElement = New OEElement(v, w, OEElement.LEFT, 0)
                e.bendsToAddToSymmetric = 1
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesOnTop(1)
                e = New OEElement(v, w, OEElement.UP, 0)
                y = Gamma.horizontal_y(v)
                x = Gamma.vertical_x(Gamma.edgeIndexes(v)(w))
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesOnTop(2)
                e = New OEElement(v, w, OEElement.RIGHT, 1)
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
            ElseIf ntop = 2 AndAlso nbelow = 1 Then ' (e)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (e).1")
                End If
                Dim w0 = edgesOnTop(0)
                Dim w1 = edgesOnTop(1)
                Dim w2 = edgesBelow(0)
                Dim e As OEElement = New OEElement(v, w0, OEElement.UP, 0)
                y = Gamma.horizontal_y(v)
                x = Gamma.vertical_x(Gamma.edgeIndexes(v)(w0))
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                e = New OEElement(v, w1, OEElement.RIGHT, 1)
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                If Gamma.vertical_x(Gamma.edgeIndexes(v)(w2)) = Gamma.vertical_x(Gamma.edgeIndexes(v)(w0)) Then
                    e = New OEElement(v, w2, OEElement.DOWN, 0)
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)
                Else
                    e = New OEElement(v, w2, OEElement.DOWN, 1)
                    e.bendsToAddToSymmetric = 1
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)
                End If
            ElseIf ntop = 1 AndAlso nbelow = 2 Then ' (e)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (e).2")
                End If
                Dim w0 = edgesBelow(0)
                Dim w1 = edgesBelow(1)
                Dim w2 = edgesOnTop(0)
                Dim e As OEElement

                If Gamma.vertical_x(Gamma.edgeIndexes(v)(w2)) = Gamma.vertical_x(Gamma.edgeIndexes(v)(w1)) Then
                    e = New OEElement(v, w2, OEElement.UP, 0)
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)
                Else
                    e = New OEElement(v, w2, OEElement.UP, 1)
                    e.bendsToAddToSymmetric = 1
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)
                End If

                e = New OEElement(v, w1, OEElement.DOWN, 0)
                y = Gamma.horizontal_y(v)
                x = Gamma.vertical_x(Gamma.edgeIndexes(v)(w1))
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                e = New OEElement(v, w0, OEElement.LEFT, 1)
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
            ElseIf ntop = 0 AndAlso nbelow = 3 Then ' (d)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (d).2")
                End If
                Dim w = edgesBelow(2)
                Dim e As OEElement = New OEElement(v, w, OEElement.RIGHT, 0)
                e.bendsToAddToSymmetric = 1
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesBelow(1)
                e = New OEElement(v, w, OEElement.DOWN, 0)
                y = Gamma.horizontal_y(v)
                x = Gamma.vertical_x(Gamma.edgeIndexes(v)(w))
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesBelow(0)
                e = New OEElement(v, w, OEElement.LEFT, 1)
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
            ElseIf ntop = 4 AndAlso nbelow = 0 Then ' (f)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (f).1")
                End If
                Dim w = edgesOnTop(0)
                Dim e As OEElement = New OEElement(v, w, OEElement.LEFT, 0)
                e.bendsToAddToSymmetric = 1
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
                If DEBUG >= 1 Then
                    Console.WriteLine("   left:" & w.ToString())
                End If

                w = edgesOnTop(1)
                e = New OEElement(v, w, OEElement.UP, 0)
                y = Gamma.horizontal_y(v)
                x = Gamma.vertical_x(Gamma.edgeIndexes(v)(w))
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
                If DEBUG >= 1 Then
                    Console.WriteLine("   up:" & w.ToString())
                End If

                w = edgesOnTop(2)
                e = New OEElement(v, w, OEElement.RIGHT, 1)
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
                If DEBUG >= 1 Then
                    Console.WriteLine("   right:" & w.ToString())
                End If

                w = edgesOnTop(3)
                e = New OEElement(v, w, OEElement.DOWN, 2)
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
                If DEBUG >= 1 Then
                    Console.WriteLine("   down:" & w.ToString())
                End If
            ElseIf ntop = 3 AndAlso nbelow = 1 Then ' (g)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (g).1")
                End If
                Dim w = edgesOnTop(0)
                Dim e As OEElement = New OEElement(v, w, OEElement.LEFT, 0)
                e.bendsToAddToSymmetric = 1
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesOnTop(1)
                e = New OEElement(v, w, OEElement.UP, 0)
                y = Gamma.horizontal_y(v)
                x = Gamma.vertical_x(Gamma.edgeIndexes(v)(w))
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesOnTop(2)
                e = New OEElement(v, w, OEElement.RIGHT, 1)
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesBelow(0)
                e = New OEElement(v, w, OEElement.DOWN, 1)
                e.bendsToAddToSymmetric = 1
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
            ElseIf ntop = 2 AndAlso nbelow = 2 Then ' (h) or (i)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (h/i).1")
                End If

                If Gamma.vertical_x(Gamma.edgeIndexes(v)(edgesOnTop(1))) > Gamma.vertical_x(Gamma.edgeIndexes(v)(edgesBelow(0))) Then
                    Dim w = edgesOnTop(0)
                    Dim e As OEElement = New OEElement(v, w, OEElement.UP, 1)
                    e.bendsToAddToSymmetric = 1
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)

                    w = edgesOnTop(1)
                    e = New OEElement(v, w, OEElement.RIGHT, 1)
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)

                    w = edgesBelow(1)
                    e = New OEElement(v, w, OEElement.DOWN, 1)
                    e.bendsToAddToSymmetric = 1
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)

                    w = edgesBelow(0)
                    e = New OEElement(v, w, OEElement.LEFT, 1)
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)

                    y = Gamma.horizontal_y(v)
                    x = (Gamma.vertical_x(Gamma.edgeIndexes(v)(edgesOnTop(1))) + Gamma.vertical_x(Gamma.edgeIndexes(v)(edgesBelow(0)))) / 2
                Else
                    Dim w = edgesOnTop(0)
                    Dim e As OEElement = New OEElement(v, w, OEElement.LEFT, 0)
                    e.bendsToAddToSymmetric = 1
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)

                    w = edgesOnTop(1)
                    e = New OEElement(v, w, OEElement.UP, 1)
                    e.bendsToAddToSymmetric = 1
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)

                    w = edgesBelow(1)
                    e = New OEElement(v, w, OEElement.RIGHT, 0)
                    e.bendsToAddToSymmetric = 1
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)

                    w = edgesBelow(0)
                    e = New OEElement(v, w, OEElement.DOWN, 1)
                    e.bendsToAddToSymmetric = 1
                    findSymmetric(e, embedding)
                    vertexEmbedding.Add(e)

                    y = Gamma.horizontal_y(v)
                    x = (Gamma.vertical_x(Gamma.edgeIndexes(v)(edgesOnTop(0))) + Gamma.vertical_x(Gamma.edgeIndexes(v)(edgesBelow(1)))) / 2

                End If
            ElseIf ntop = 1 AndAlso nbelow = 3 Then ' (g)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (g).2")
                End If
                Dim w = edgesBelow(2)
                Dim e As OEElement = New OEElement(v, w, OEElement.RIGHT, 0)
                e.bendsToAddToSymmetric = 1
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesBelow(1)
                e = New OEElement(v, w, OEElement.DOWN, 0)
                y = Gamma.horizontal_y(v)
                x = Gamma.vertical_x(Gamma.edgeIndexes(v)(w))
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesBelow(0)
                e = New OEElement(v, w, OEElement.LEFT, 1)
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesOnTop(0)
                e = New OEElement(v, w, OEElement.UP, 1)
                e.bendsToAddToSymmetric = 1
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
            ElseIf ntop = 0 AndAlso nbelow = 4 Then ' (f)
                If DEBUG >= 1 Then
                    Console.WriteLine("Node " & v.ToString() & " processed with pattern (f).2")
                End If
                Dim w = edgesBelow(3)
                Dim e As OEElement = New OEElement(v, w, OEElement.RIGHT, 0)
                e.bendsToAddToSymmetric = 1
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesBelow(2)
                e = New OEElement(v, w, OEElement.DOWN, 0)
                y = Gamma.horizontal_y(v)
                x = Gamma.vertical_x(Gamma.edgeIndexes(v)(w))
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesBelow(1)
                e = New OEElement(v, w, OEElement.LEFT, 1)
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)

                w = edgesBelow(0)
                e = New OEElement(v, w, OEElement.UP, 2)
                findSymmetric(e, embedding)
                vertexEmbedding.Add(e)
            End If

            Return New OEVertex(vertexEmbedding, v, x, y)
        End Function

        Public Shared Sub findSymmetric(e As OEElement, embedding As OEVertex())
            If embedding(e.dest) IsNot Nothing Then
                For Each e_sym As OEElement In embedding(CInt(e.dest)).embedding
                    If e_sym.dest = e.v Then
                        e_sym.sym = e
                        e.sym = e_sym
                        e.bends += e_sym.bendsToAddToSymmetric
                        e_sym.bendsToAddToSymmetric = 0
                        e_sym.bends += e.bendsToAddToSymmetric
                        e.bendsToAddToSymmetric = 0
                        Exit For
                    End If
                Next
            End If
        End Sub
    End Class

End Namespace
