Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.util
Imports Microsoft.VisualBasic.ListExtensions
Imports std = System.Math
' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 

Namespace Orthogonal

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class Blocks
        Public Shared DEBUG As Integer = 0

        Private Shared Function single_node(graph As Integer()()) As Pair(Of Dictionary(Of Integer, IList(Of Integer)), Dictionary(Of Integer, IList(Of Integer)))
            If DEBUG >= 2 Then
                Console.WriteLine("Blocks: special case of a graph with a single node.")
            End If
            ' special case:
            Dim blocks As Dictionary(Of Integer, IList(Of Integer)) = New Dictionary(Of Integer, IList(Of Integer))()
            Dim cutNodes As Dictionary(Of Integer, IList(Of Integer)) = New Dictionary(Of Integer, IList(Of Integer))()
            Dim l As IList(Of Integer) = New List(Of Integer)()
            l.Add(1)
            blocks(0) = l
            Return New Pair(Of Dictionary(Of Integer, IList(Of Integer)), Dictionary(Of Integer, IList(Of Integer)))(blocks, cutNodes)
        End Function

        Public Shared Function blocks(graph As Integer()()) As Pair(Of Dictionary(Of Integer, IList(Of Integer)), Dictionary(Of Integer, IList(Of Integer)))
            If graph.Length = 1 Then
                Return single_node(graph)
            End If

            ' the following code assumes that nodes are numbered 1,2,... (i.e. they start at 1 not at 0!)
            ' this is because the algorithms uses node numebers and their negative to determine
            ' certain properties of the graph, and 0 and its negative are the same, so it wouldn't work
            Dim n = graph.Length
            Dim b = New Integer(n + 1 - 1) {} ' one label per edge in the spanning tree
            Dim p = New Integer(n + 1 - 1) {} ' this will store the spanning tree
            Dim d = New Integer(n + 1 - 1) {} ' the distance of each node to the root of the tree
            Dim A = New Boolean(n + 1 - 1) {} ' one per node in the graph
            Dim X As ISet(Of Integer) = New HashSet(Of Integer)()
            Dim R As ISet(Of Integer) = New HashSet(Of Integer)()
            Dim U As List(Of Integer) = New List(Of Integer)()
            Dim treeroot = 1

            For i = 1 To n
                b(i) = 0
                A(i) = False
                If i <> treeroot Then
                    R.Add(i)
                End If
                p(i) = -1
            Next
            '        T.add(treeroot);
            U.Add(treeroot)
            d(treeroot) = 0

            While U.Count > 0
                '            for(int i = 1;i<=n;i++) A[i] = false;

                Dim toAddInU As IList(Of Integer) = New List(Of Integer)()

                Dim v = U.PopAt(U.Count - 1) ' get the last element of U
                If DEBUG >= 2 Then
                    Console.WriteLine("  v = " & v.ToString())
                End If
                Dim lL = 0
                For z = 1 To n
                    If graph(v - 1)(z - 1) = 1 Then ' we have to subtract 1, since in 'graph' nodes start from 0
                        If R.Contains(z) Then
                            ' add (v,z) to T???
                            R.Remove(z)
                            toAddInU.Add(z)
                            p(z) = v
                            d(z) = d(v) + 1
                            b(z) = -z
                            If DEBUG >= 2 Then
                                Console.WriteLine("  p[" & z.ToString() & "] = " & v.ToString())
                            End If
                            If DEBUG >= 2 Then
                                Console.WriteLine("  (" & z.ToString() & "," & p(z).ToString() & ") = " & b(z).ToString())
                            End If
                        End If
                    End If
                Next
                For z = 1 To n
                    If graph(v - 1)(z - 1) = 1 Then ' we have to subtract 1, since in 'graph' nodes start from 0
                        If U.Contains(z) Then
                            ' fundamental cycle detected:
                            If DEBUG >= 2 Then
                                Console.WriteLine("  Cycle: " & v.ToString() & " -> " & z.ToString() & " -> " & p(z).ToString() & " + Q")
                            End If
                            Dim q = b(z)
                            b(z) = v
                            If DEBUG >= 2 Then
                                Console.WriteLine("  (" & z.ToString() & "," & p(z).ToString() & ") = " & b(z).ToString())
                            End If
                            If q > 0 Then
                                A(q) = True
                            End If
                            lL = std.Max(lL, d(v) - d(p(z)))
                            If DEBUG >= 2 Then
                                Console.WriteLine("  L = " & lL.ToString())
                            End If
                        End If
                    End If
                Next
                If lL > 0 Then
                    If DEBUG >= 2 Then
                        Console.WriteLine("  Consolidating...")
                    End If
                    Dim k = v
                    Dim l = 0
                    While k <> treeroot AndAlso l < lL
                        If DEBUG >= 2 Then
                            Console.WriteLine("  edge: (" & k.ToString() & "," & p(k).ToString() & ") with b[" & k.ToString() & "]=" & b(k).ToString())
                        End If
                        If b(k) > 0 Then
                            A(b(k)) = True
                        End If
                        b(k) = v
                        If DEBUG >= 2 Then
                            Console.WriteLine("  (" & k.ToString() & "," & p(k).ToString() & ") = " & b(k).ToString())
                        End If
                        k = p(k)
                        l += 1
                    End While
                    For t = 1 To n
                        If b(t) > 0 AndAlso A(b(t)) Then
                            b(t) = v
                            If DEBUG >= 2 Then
                                Console.WriteLine("  (" & t.ToString() & "," & p(t).ToString() & ") = " & b(t).ToString())
                            End If
                        End If
                    Next
                End If
                CType(U, List(Of Integer)).AddRange(toAddInU)
                X.Add(v)
            End While

            ' Rename the blocks:
            Dim blockRenaming As Dictionary(Of Integer, Integer) = New Dictionary(Of Integer, Integer)()
            Dim trivialBlocks As IList(Of Integer) = New List(Of Integer)()
            Dim nextBlock = 1
            For i = 1 To n
                ' ignore
                If b(i) = 0 Then
                ElseIf b(i) = -i Then
                    trivialBlocks.Add(i)
                Else
                    If blockRenaming.ContainsKey(b(i)) Then
                        b(i) = blockRenaming(b(i))
                    Else
                        blockRenaming(b(i)) = nextBlock
                        b(i) = nextBlock
                        nextBlock += 1
                    End If
                End If
            Next
            For Each i In trivialBlocks
                b(i) = std.Min(Threading.Interlocked.Increment(nextBlock), nextBlock - 1)
                ' System.out.println("trivial block!!!");
            Next

            ' translate the blocks and cutedges:
            Dim lBlocks As Dictionary(Of Integer, IList(Of Integer)) = New Dictionary(Of Integer, IList(Of Integer))()
            Dim cutNodes As Dictionary(Of Integer, IList(Of Integer)) = New Dictionary(Of Integer, IList(Of Integer))()
            For i = 1 To n
                Dim nodeBlocks As IList(Of Integer) = New List(Of Integer)()
                'nodeBlocks.add(b[i]);
                For j = 1 To n
                    If p(j) = i Then
                        If Not nodeBlocks.Contains(b(j)) Then
                            nodeBlocks.Add(b(j))
                        End If
                    End If
                Next
                If p(i) > 0 Then
                    If Not nodeBlocks.Contains(b(i)) Then
                        nodeBlocks.Add(b(i))
                    End If
                End If

                For Each blockID In nodeBlocks
                    Dim block = lBlocks(blockID)
                    If block Is Nothing Then
                        block = New List(Of Integer)()
                        lBlocks(blockID) = block
                    End If
                    block.Add(i - 1)
                Next
                If nodeBlocks.Count > 1 Then
                    cutNodes(i - 1) = nodeBlocks
                End If
            Next


            Return New Pair(Of Dictionary(Of Integer, IList(Of Integer)), Dictionary(Of Integer, IList(Of Integer)))(lBlocks, cutNodes)
        End Function
    End Class

End Namespace
