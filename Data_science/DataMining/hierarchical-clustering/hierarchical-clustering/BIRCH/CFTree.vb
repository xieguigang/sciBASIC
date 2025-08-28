#Region "Microsoft.VisualBasic::80f6676ac55316b1e167c274ae77fd0d, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\BIRCH\CFTree.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 615
    '    Code Lines: 276 (44.88%)
    ' Comment Lines: 243 (39.51%)
    '    - Xml Docs: 74.90%
    ' 
    '   Blank Lines: 96 (15.61%)
    '     File Size: 26.02 KB


    '     Class CFTree
    ' 
    '         Properties: LeafListStart, MemoryLimit, SubclusterMembers
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: computeMemorySize, computeNewThreshold, computeSumLambdaSquared, countEntries, countLeafEntries
    '                   countNodes, hasReachedMemoryLimit, (+3 Overloads) insertEntry, mapToClosestSubcluster, rebuildIfAboveMemLimit
    '                   rebuildTree
    ' 
    '         Sub: AutomaticRebuild, copyTree, finishedInsertingData, MemoryLimitMB, PeriodicMemLimitCheck
    '              printCFTree, printLeafEntries, printLeafIndexes, splitRoot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

' 
'   This file is part of JBIRCH.
' 
'   JBIRCH is free software: you can redistribute it and/or modify
'   it under the terms of the GNU General Public License as published by
'   the Free Software Foundation, either version 3 of the License, or
'   (at your option) any later version.
' 
'   JBIRCH is distributed in the hope that it will be useful,
'   but WITHOUT ANY WARRANTY; without even the implied warranty of
'   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'   GNU General Public License for more details.
' 
'   You should have received a copy of the GNU General Public License
'   along with JBIRCH.  If not, see <http://www.gnu.org/licenses/>.
' 
' 

' 
'   CFTree.java
'   Copyright (C) 2009 Roberto Perdisci (roberto.perdisci@gmail.com)
' 


Namespace BIRCH

    ''' <summary>
    ''' This is an implementation of the BIRCH clustering algorithm described in:
    ''' 
    ''' T. Zhang, R. Ramakrishnan, and M. Livny.
    ''' "BIRCH: A New Data Clustering Algorithm and Its Applications"
    ''' Data Mining and Knowledge Discovery, 1997.
    ''' 
    ''' @author Roberto Perdisci (roberto.perdisci@gmail.com)
    ''' @version 0.1
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/sehee-lee/JBIRCH
    ''' </remarks>
    Public Class CFTree

        ''' <summary>
        ''' Used when computing if the tree is reaching memory limit
        ''' </summary>
        Private Const MEM_LIM_FRAC As Double = 10

        ''' <summary>
        ''' Centroid Distance D0
        ''' </summary>
        Public Const D0_DIST As Integer = 0

        ''' <summary>
        ''' Centroid distance D1
        ''' </summary>
        Public Const D1_DIST As Integer = 1

        ''' <summary>
        ''' Cluster Distance D2
        ''' </summary>
        Public Const D2_DIST As Integer = 2

        ''' <summary>
        ''' Cluster Distance D3
        ''' </summary>
        Public Const D3_DIST As Integer = 3

        ''' <summary>
        ''' Cluster Distance D4
        ''' </summary>
        Public Const D4_DIST As Integer = 4

        ''' <summary>
        ''' The root node of the CFTree
        ''' </summary>
        Private root As CFNode

        ''' <summary>
        ''' dummy node that points to the list of leaves. used for fast retrieval of final subclusters
        ''' </summary>
        Private leafListStartField As CFNode = Nothing

        ''' <summary>
        ''' keeps count of the instances inserted into the tree
        ''' </summary>
        Private instanceIndex As Integer = 0

        ''' <summary>
        ''' if true, the tree is automatically rebuilt every time the memory limit is reached
        ''' </summary>
        Private automaticRebuildField As Boolean = True

        ''' <summary>
        ''' the memory limit used when automatic rebuilding is active
        ''' </summary>
        Private memLimit As Long = std.Pow(1024, 3) ' default = 1GB

        ''' <summary>
        ''' used when automatic rebuilding is active
        ''' </summary>
        Private periodicMemLimitCheckField As Long = 100000 ' checks if memeory limit is exceeded every 100,000 insertions

        ''' 
        ''' <param name="maxNodeEntries"> parameter B </param>
        ''' <param name="distThreshold"> parameter T </param>
        ''' <param name="distFunction"> must be one of CFTree.D0_DIST,...,CFTree.D4_DIST, otherwise it will default to D0_DIST </param>
        ''' <param name="applyMergingRefinement"> if true, activates merging refinement after each node split </param>
        Public Sub New(maxNodeEntries As Integer, distThreshold As Double, Optional distFunction As Integer = D0_DIST, Optional applyMergingRefinement As Boolean = False)
            If distFunction < D0_DIST OrElse distFunction > D4_DIST Then
                distFunction = D0_DIST
            End If

            root = New CFNode(maxNodeEntries, distThreshold, distFunction, applyMergingRefinement, True)
            leafListStartField = New CFNode(0, 0, distFunction, applyMergingRefinement, True) ' this is a dummy node that points to the fist leaf
            leafListStartField.NextLeaf = root ' at this point root is the only node and therefore also the only leaf
        End Sub

        ''' 
        ''' <returns> the current memory limit used to trigger automatic rebuilding </returns>
        Public Overridable Property MemoryLimit As Long
            Get
                Return memLimit
            End Get
            Set(value As Long)
                memLimit = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the start of the list of leaf nodes (remember: the first node is a dummy node)
        ''' 
        ''' @return
        ''' </summary>
        Public Overridable ReadOnly Property LeafListStart As CFNode
            Get
                Return leafListStartField
            End Get
        End Property

        ''' 
        ''' <param name="limit"> memory limit in Mbytes </param>
        Public Sub MemoryLimitMB(limit As Long)
            memLimit = limit * ByteSize.MB
        End Sub

        ''' 
        ''' <param name="auto"> if true, and memory limit is reached, the tree is automatically rebuilt with larger threshold </param>
        Public Sub AutomaticRebuild(auto As Boolean)
            automaticRebuildField = auto
        End Sub

        ''' 
        ''' <param name="period"> the number of insert operations after which we check whether the tree has reached the memory limit </param>
        Public Sub PeriodicMemLimitCheck(period As Long)
            periodicMemLimitCheckField = period
        End Sub

        ''' <summary>
        ''' Inserts a single pattern vector into the CFTree
        ''' </summary>
        ''' <param name="x"> the pattern vector to be inserted in the tree </param>
        ''' <returns> true if insertion was successful </returns>
        Public Overridable Function insertEntry(x As Double()) As Boolean
            instanceIndex += 1

            If automaticRebuildField AndAlso instanceIndex Mod periodicMemLimitCheckField = 0 Then
                ' rebuilds the tree if we reached or exceeded memory limits
                rebuildIfAboveMemLimit()
            End If

            Return insertEntry(x, instanceIndex)
        End Function

        ''' <summary>
        ''' Insert a pattern vector with a specific associated pattern vector index.
        ''' This method does not use periodic memory limit checks.
        ''' </summary>
        ''' <param name="x"> the pattern vector to be inserted in the tree </param>
        ''' <param name="index"> a specific index associated to the pattern vector x </param>
        ''' <returns> true if insertion was successful </returns>
        Public Overridable Function insertEntry(x As Double(), index As Integer) As Boolean
            Dim e As CFEntry = New CFEntry(x, index)

            ' System.out.println("Inserting " + e);

            Return insertEntry(e)
        End Function

        ''' <summary>
        ''' Inserts an entire CFEntry into the tree. Used for tree rebuilding.
        ''' </summary>
        ''' <param name="e"> the CFEntry to insert </param>
        ''' <returns> true if insertion happened without problems </returns>
        Private Function insertEntry(e As CFEntry) As Boolean

            Dim dontSplit = root.insertEntry(e)
            If Not dontSplit Then
                ' if dontSplit is false, it means there was not enough space to insert the new entry in the tree, 
                ' therefore wee need to split the root to make more room
                splitRoot()

                If automaticRebuildField Then
                    ' rebuilds the tree if we reached or exceeded memory limits
                    rebuildIfAboveMemLimit()
                End If
            End If

            Return True ' after root is split, we are sure x was inserted correctly in the tree, and we return true
        End Function

        ''' <summary>
        ''' Every time we split the root, we check whether the memory limit imposed on the tree
        ''' has been reached. In this case, we automatically increase the distance threshold and
        ''' rebuild the tree. 
        ''' 
        ''' It is worth noting that since we only check memory consumption only during root split,
        ''' and not for all node splits (for performance reasons), we cannot guarantee that
        ''' the memory limit will not be exceeded. The tree may grow significantly between a 
        ''' root split and the next.
        ''' Furthermore, the computation of memory consumption using the SizeOf class is only approximate.
        ''' 
        ''' Notice also that if the threshold grows to the point that all the entries fall into one entry
        ''' of the root (i.e., the root is the only node in the tree, and has only one sub-cluster)
        ''' the automatic rebuild cannot decrease the memory consumption (because increasing the threshold
        ''' has not effect on reducing the size of the tree), and if Java runs out of memory
        ''' the program will terminate.
        ''' </summary>
        ''' <returns> true if rebuilt </returns>
        Private Function rebuildIfAboveMemLimit() As Boolean
            If hasReachedMemoryLimit(Me, memLimit) Then
                Console.WriteLine("############## Size of Tree is reaching or has exceeded the memory limit")
                Console.WriteLine("############## Rebuilding the Tree...")

                Console.WriteLine("############## Current Threshold = " & root.DistThreshold.ToString())
                Dim newThreshold = computeNewThreshold(leafListStartField, root.DistFunction, root.DistThreshold)
                ' System.out.println("############## New Threshold = " + newThreshold);

                Dim newTree As CFTree = rebuildTree(root.MaxNodeEntries, newThreshold, root.DistFunction, root.applyMergingRefinement(), False)
                copyTree(newTree)

                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' Splits the root to accommodate a new entry. The height of the tree grows by one.
        ''' </summary>
        Private Sub splitRoot()
            ' the split happens by finding the two entries in this node that are the most far apart
            ' we then use these two entries as a "pivot" to redistribute the old entries into two new nodes

            Dim p = root.findFarthestEntryPair(root.Entries)

            Dim newEntry1 As CFEntry = New CFEntry()
            Dim newNode1 As CFNode = New CFNode(root.MaxNodeEntries, root.DistThreshold, root.DistFunction, root.applyMergingRefinement(), root.Leaf)
            newEntry1.Child = newNode1

            Dim newEntry2 As CFEntry = New CFEntry()
            Dim newNode2 As CFNode = New CFNode(root.MaxNodeEntries, root.DistThreshold, root.DistFunction, root.applyMergingRefinement(), root.Leaf)
            newEntry2.Child = newNode2

            ' the new root that hosts the new entries
            Dim newRoot As CFNode = New CFNode(root.MaxNodeEntries, root.DistThreshold, root.DistFunction, root.applyMergingRefinement(), False)
            newRoot.addToEntryList(newEntry1)
            newRoot.addToEntryList(newEntry2)

            ' this updates the pointers to the list of leaves
            If root.Leaf Then ' if root was a leaf
                leafListStartField.NextLeaf = newNode1
                newNode1.PreviousLeaf = leafListStartField
                newNode1.NextLeaf = newNode2
                newNode2.PreviousLeaf = newNode1
            End If

            ' redistributes the entries in the root between newEntry1 and newEntry2
            ' according to the distance to p.e1 and p.e2
            root.redistributeEntries(root.Entries, p, newEntry1, newEntry2)

            ' updates the root
            root = newRoot

            ' frees some memory by deleting the nodes in the tree that had to be split
            GC.Collect()

        End Sub

        ''' <summary>
        ''' Overwrites the structure of this tree (all nodes, entreis, and leaf list) with the structure of newTree.
        ''' </summary>
        ''' <param name="newTree"> the tree to be copied </param>
        Private Sub copyTree(newTree As CFTree)
            root = newTree.root
            leafListStartField = newTree.leafListStartField
        End Sub

        ''' <summary>
        ''' Computes a new threshold based on the average distance of the closest subclusters in each leaf node
        ''' </summary>
        ''' <param name="leafListStart"> the pointer to the start of the list (the first node is assumed to be a place-holder dummy node) </param>
        ''' <param name="distFunction"> </param>
        ''' <param name="currentThreshold"> </param>
        ''' <returns> the new threshold </returns>
        Public Overridable Function computeNewThreshold(leafListStart As CFNode, distFunction As Integer, currentThreshold As Double) As Double
            Dim avgDist As Double = 0
            Dim n = 0

            Dim l = leafListStart.NextLeaf
            While l IsNot Nothing
                If Not l.Dummy Then
                    Dim p = l.findClosestEntryPair(l.Entries)
                    If p IsNot Nothing Then
                        avgDist += p.e1.distance(p.e2, distFunction)
                        n += 1

                        ' This is a possible alternative: Overall avg distance between leaf entries
                        ' 						CFEntry[] v = l.getEntries().toArray(new CFEntry[0]);
                        ' 						for(int i=0; i < v.length-1; i++) {
                        ' 							for(int j=i+1; j < v.length; j++) {
                        ' 								avgDist += v[i].distance(v[j], distFunction);
                        ' 								n++;
                        ' 							}
                        ' 						}
                    End If
                End If
                l = l.NextLeaf
            End While

            Dim newThreshold As Double = 0
            If n > 0 Then
                newThreshold = avgDist / n
            End If

            If newThreshold <= currentThreshold Then ' this guarantees that newThreshold always increases compared to currentThreshold
                newThreshold = 2 * currentThreshold
            End If

            Return newThreshold
        End Function

        ''' <summary>
        ''' True if CFTree's memory occupation exceeds or is almost equal to the memory limit
        ''' </summary>
        ''' <param name="tree"> the tree to be tested </param>
        ''' <param name="limit"> the memory limit </param>
        ''' <returns> true if memory limit has been reached </returns>
        Private Function hasReachedMemoryLimit(tree As CFTree, limit As Long) As Boolean
            Dim memory = computeMemorySize(tree)

            ' System.out.println("#################### Tree Size = " + SizeOf.humanReadable(memory));
            If memory >= limit - limit / MEM_LIM_FRAC Then
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' Computes the memory usage of a CFTree
        ''' </summary>
        ''' <param name="t"> a CFTree </param>
        ''' <returns> memory usage in bytes </returns>
        Private Function computeMemorySize(t As CFTree) As Long

            Dim memSize As Long = 0
            Try
                memSize = HeapSizeOf.MeasureSize(t)
            Catch e As Exception
                Console.Error.WriteLine("#################### ERROR WHEN COMPUTING MEMORY SIZE: " & e.ToString())
            End Try
            Return memSize
        End Function

        ''' <summary>
        ''' This implementation of the rebuilding algorithm is different from
        ''' the one described in Section 4.5 of the paper. However the effect
        ''' is practically the same. Namely, given a tree t_i build using
        ''' threshold T_i, if we set a new threshold T_(i+1) and call
        ''' rebuildTree (assuming maxEntries stays the same) we will obtain
        ''' a more compact tree. 
        ''' 
        ''' Since the CFTree is sensitive to the order of the data, there
        ''' may be cases in which, if we set the T_(i+1) so that non of the
        ''' sub-clusters (i.e., the leaf entries) can be merged (e.g., T_(i+1)=-1)
        ''' we might actually obtain a new tree t_(i+1) containing more nodes
        ''' than t_i. However, the obtained sub-clusters in t_(i+1) will be 
        ''' identical to the sub-clusters in t_i.
        ''' 
        ''' In practice, though, if T_(i+1) > T_(i), the tree t_(i+1) will
        ''' usually be smaller than t_i.
        ''' Although the Reducibility Theorem in Section 4.5 may not hold
        ''' anymore, in practice this will not be a big problem, since 
        ''' even in those cases in which t_(i+1)>t_i, the growth should
        ''' be very small.
        ''' 
        ''' The advantage is that relaxing the constraint that the size
        ''' of t_(i+1) must be less than t_i makes the implementation
        ''' of the rebuilding algorithm much easier.
        ''' </summary>
        ''' <param name="newMaxEntries"> the new number of entries per node </param>
        ''' <param name="newThreshold"> the new threshold </param>
        ''' <param name="applyMergingRefinement"> if true, merging refinement will be applied after every split </param>
        ''' <param name="discardOldTree"> if true, the old tree will be discarded (to free memory)
        ''' </param>
        ''' <returns> the new (usually more compact) CFTree </returns>
        Public Overridable Function rebuildTree(newMaxEntries As Integer, newThreshold As Double, distFunction As Integer, applyMergingRefinement As Boolean, discardOldTree As Boolean) As CFTree
            Dim newTree As CFTree = New CFTree(newMaxEntries, newThreshold, distFunction, applyMergingRefinement)
            newTree.instanceIndex = instanceIndex
            newTree.memLimit = memLimit

            Dim oldLeavesList = leafListStartField.NextLeaf ' remember: the node this.leafListStart is a dummy node (place holder for beginning of leaf list)

            If discardOldTree Then
                root = Nothing
                GC.Collect() ' removes the old tree. Only the old leaves will be kept
            End If

            Dim leaf = oldLeavesList
            While leaf IsNot Nothing
                If Not leaf.Dummy Then
                    For Each e In leaf.Entries
                        Dim newE = e
                        If Not discardOldTree Then ' we need to make a deep copy of e
                            newE = New CFEntry(e)
                        End If

                        newTree.insertEntry(newE)
                    Next
                End If

                leaf = leaf.NextLeaf
            End While

            If discardOldTree Then
                leafListStartField = Nothing
                GC.Collect() ' removes the old list of leaves
            End If

            Return newTree
        End Function


        ''' 
        ''' <returns> a list of subcluster, and for each subcluster a list of pattern vector indexes that belong to it </returns>
        Public Overridable ReadOnly Property SubclusterMembers As List(Of List(Of Integer))
            Get
                Dim membersList As List(Of List(Of Integer)) = New List(Of List(Of Integer))()

                Dim l = leafListStartField.NextLeaf ' the first leaf is dummy!
                While l IsNot Nothing
                    If Not l.Dummy Then
                        ' System.out.println(l);
                        For Each e In l.Entries
                            membersList.Add(e.IndexList)
                        Next
                    End If
                    l = l.NextLeaf
                End While

                Return membersList
            End Get
        End Property

        ''' <summary>
        ''' Signals the fact that we finished inserting data.
        ''' The obtained subclusters will be assigned a positive, unique ID number
        ''' </summary>
        Public Overridable Sub finishedInsertingData()
            Dim l = leafListStartField.NextLeaf ' the first leaf is dummy!

            Dim id = 0
            While l IsNot Nothing
                If Not l.Dummy Then
                    ' System.out.println(l);
                    For Each e In l.Entries
                        id += 1
                        e.SubclusterID = id
                    Next
                End If
                l = l.NextLeaf
            End While
        End Sub

        ''' <summary>
        ''' Retrieves the subcluster id of the closest leaf entry to e
        ''' </summary>
        ''' <param name="x"> the entry to be mapped </param>
        ''' <returns> a positive integer, if the leaf entries were enumerated using finishedInsertingData(), otherwise -1 </returns>
        Public Overridable Function mapToClosestSubcluster(x As Double()) As Integer
            Dim e As CFEntry = New CFEntry(x)

            Return root.mapToClosestSubcluster(e)

        End Function

        ''' <summary>
        ''' Computes an estimate of the cost of running an O(n^2) algorithm to split each subcluster in more fine-grained clusters
        ''' </summary>
        ''' <returns> sqrt(sum_i[(n_i)^2]), where n_i is the number of members of the i-th subcluster </returns>
        Public Overridable Function computeSumLambdaSquared() As Double
            Dim lambdaSS As Double = 0

            Dim l = leafListStartField.NextLeaf
            While l IsNot Nothing
                If Not l.Dummy Then
                    For Each e In l.Entries
                        lambdaSS += std.Pow(e.IndexList.Count, 2)
                    Next
                End If
                l = l.NextLeaf
            End While

            Return std.Sqrt(lambdaSS)
        End Function

        ''' <summary>
        ''' prints the CFTree
        ''' </summary>
        Public Overridable Sub printCFTree()
            Console.WriteLine(root)
        End Sub

        ''' <summary>
        ''' Counts the nodes of the tree (including leaves)
        ''' </summary>
        ''' <returns> the number of nodes in the tree </returns>
        Public Overridable Function countNodes() As Integer
            Dim n = 1 ' at least root has to be present
            n += root.countChildrenNodes()

            Return n
        End Function

        ''' <summary>
        ''' Counts the number of CFEntries in the tree
        ''' </summary>
        ''' <returns> the number of entries in the tree </returns>
        Public Overridable Function countEntries() As Integer
            Dim n As Integer = root.size() ' at least root has to be present
            n += root.countEntriesInChildrenNodes()

            Return n
        End Function

        ''' <summary>
        ''' Counts the number of leaf entries (i.e., the number of sub-clusters in the tree)
        ''' </summary>
        ''' <returns> the number of leaf entries (i.e., the number of sub-clusters) </returns>
        Public Overridable Function countLeafEntries() As Integer
            Dim i = 0
            Dim l = leafListStartField.NextLeaf
            While l IsNot Nothing
                If Not l.Dummy Then
                    i += l.size()
                End If

                l = l.NextLeaf
            End While

            Return i
        End Function

        ''' <summary>
        ''' Prints the index of all the pattern vectors that fall into the leaf nodes.
        ''' This is only useful for debugging purposes.
        ''' </summary>
        Public Overridable Sub printLeafIndexes()
            Dim indexes As List(Of Integer) = New List(Of Integer)()

            Dim l = leafListStartField.NextLeaf
            While l IsNot Nothing
                If Not l.Dummy Then
                    Console.WriteLine(l)
                    For Each e In l.Entries
                        indexes.AddRange(e.IndexList)
                    Next
                End If
                l = l.NextLeaf
            End While

            Dim v As Integer() = indexes.ToArray()
            Array.Sort(v)
            Console.WriteLine("Num of Indexes = " & v.Length.ToString())
            Console.WriteLine(v.GetJson())
        End Sub

        ''' <summary>
        ''' Prints the index of the pattern vectors in each leaf entry (i.e., each subcluster)
        ''' </summary>
        Public Overridable Sub printLeafEntries()
            Dim i = 0
            Dim l = leafListStartField.NextLeaf
            While l IsNot Nothing
                If Not l.Dummy Then
                    For Each e In l.Entries
                        Console.WriteLine("[[" & Threading.Interlocked.Increment(i).ToString() & "]]")
                        Dim v As Integer() = e.IndexList.ToArray()
                        Array.Sort(v)
                        Console.WriteLine(v.GetJson())
                    Next
                End If

                l = l.NextLeaf
            End While
        End Sub

    End Class

End Namespace
