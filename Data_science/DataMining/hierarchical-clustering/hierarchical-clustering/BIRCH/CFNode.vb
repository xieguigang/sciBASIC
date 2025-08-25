#Region "Microsoft.VisualBasic::c61119da25805db77f760bbdd56ab298, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\BIRCH\CFNode.vb"

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

    '   Total Lines: 608
    '    Code Lines: 368 (60.53%)
    ' Comment Lines: 140 (23.03%)
    '    - Xml Docs: 45.00%
    ' 
    '   Blank Lines: 100 (16.45%)
    '     File Size: 25.13 KB


    '     Class CFNode
    ' 
    '         Properties: DistFunction, DistThreshold, Dummy, Entries, Leaf
    '                     LeafStatus, MaxNodeEntries, NextLeaf, PreviousLeaf
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: applyMergingRefinement, countChildrenNodes, countEntriesInChildrenNodes, findClosestEntry, findClosestEntryPair
    '                   findFarthestEntryPair, insertEntry, mapToClosestSubcluster, size, splitEntry
    '                   ToString
    ' 
    '         Sub: addToEntryList, mergingRefinement, (+3 Overloads) redistributeEntries, replaceClosestPairWithNewEntries, replaceClosestPairWithNewMergedEntry
    '              replaceEntries, resetEntries
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

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
'   CFNode.java
'   Copyright (C) 2009 Roberto Perdisci (roberto.perdisci@gmail.com)
' 

Namespace BIRCH

    ''' 
    ''' <summary>
    ''' @author Roberto Perdisci (roberto.perdisci@gmail.com)
    ''' @version 0.1
    ''' 
    ''' </summary>
    Public Class CFNode

        Private Shared ReadOnly LINE_SEP As String = vbLf


        Private entriesField As List(Of CFEntry) = Nothing ' stores the CFEntries for this node
        Private maxNodeEntriesField As Integer = 0 ' max number of entries per node (parameter B)
        Private distThresholdField As Double = 0 ' the distance threshold (parameter T), a.k.a. "radius"
        Private distFunctionField As Integer = CFTree.D0_DIST ' the distance function to use
        Private leafStatusField As Boolean = False ' if true, this is a leaf
        Private nextLeafField As CFNode = Nothing ' pointer to the next leaf (if not a leaf, pointer will be null)
        Private previousLeafField As CFNode = Nothing ' pointer to the previous leaf (if not a leaf, pointer will be null)
        Private applyMergingRefinement_Conflict As Boolean = False ' if true, merging refinement will be applied after every split

        Public Sub New(maxNodeEntries As Integer, distThreshold As Double, distFunction As Integer, applyMergingRefinement As Boolean, leafStatus As Boolean)
            maxNodeEntriesField = maxNodeEntries
            distThresholdField = distThreshold
            distFunctionField = distFunction

            entriesField = New List(Of CFEntry)(maxNodeEntries)
            leafStatusField = leafStatus
            applyMergingRefinement_Conflict = applyMergingRefinement
        End Sub

        ''' 
        ''' <returns> the number of CFEntries in the node </returns>
        Public Overridable Function size() As Integer
            Return entriesField.Count
        End Function

        ''' 
        ''' <returns> true if this is only a place-holder node for maintaining correct pointers in the list of leaves </returns>
        Public Overridable ReadOnly Property Dummy As Boolean
            Get
                Return (maxNodeEntriesField = 0 AndAlso distThresholdField = 0 AndAlso size() = 0 AndAlso (previousLeafField IsNot Nothing OrElse nextLeafField IsNot Nothing))
            End Get
        End Property

        ''' 
        ''' <returns> the max number of entries the node can host (parameter B) </returns>
        Public Overridable ReadOnly Property MaxNodeEntries As Integer
            Get
                Return maxNodeEntriesField
            End Get
        End Property

        ''' 
        ''' <returns> the distance threshold used to decide whether a CFEntry can absorb a new entry </returns>
        Public Overridable ReadOnly Property DistThreshold As Double
            Get
                Return distThresholdField
            End Get
        End Property

        Public Overridable ReadOnly Property DistFunction As Integer
            Get
                Return distFunctionField
            End Get
        End Property

        Protected Friend Overridable Property NextLeaf As CFNode
            Get
                Return nextLeafField
            End Get
            Set(value As CFNode)
                nextLeafField = value
            End Set
        End Property

        Protected Friend Overridable Property PreviousLeaf As CFNode
            Get
                Return previousLeafField
            End Get
            Set(value As CFNode)
                previousLeafField = value
            End Set
        End Property

        Protected Friend Overridable Sub addToEntryList(e As CFEntry)
            entriesField.Add(e)
        End Sub

        Protected Friend Overridable ReadOnly Property Entries As List(Of CFEntry)
            Get
                Return entriesField
            End Get
        End Property

        ''' <summary>
        ''' Retrieves the subcluster id of the closest leaf entry to e
        ''' </summary>
        ''' <param name="e"> the entry to be mapped </param>
        ''' <returns> a positive integer, if the leaf entries were enumerated after data insertion is finished, otherwise -1 </returns>
        Public Overridable Function mapToClosestSubcluster(e As CFEntry) As Integer
            Dim closest = findClosestEntry(e)
            If Not closest.hasChild() Then
                Return closest.SubclusterID
            End If

            Return closest.Child.mapToClosestSubcluster(e)
        End Function

        ''' <summary>
        ''' Inserts a new entry to the CFTree
        ''' </summary>
        ''' <param name="e"> the entry to be inserted </param>
        ''' <returns> TRUE if the new entry could be inserted without problems, otherwise we need to split the node </returns>
        Public Overridable Function insertEntry(e As CFEntry) As Boolean
            If entriesField.Count = 0 Then ' if the node is empty we can insert the entry directly here
                entriesField.Add(e)
                Return True ' insert was successful. no split necessary
            End If

            Dim closest = findClosestEntry(e)
            ' System.out.println("Closest Entry = " + closest);

            Dim dontSplit = False
            If closest.hasChild() Then ' if closest has a child we go down with a recursive call
                dontSplit = closest.Child.insertEntry(e)
                If dontSplit Then
                    closest.update(e) ' this updates the CF to reflect the additional entry
                    Return True
                Else
                    ' if the node below /closest/ didn't have enough room to host the new entry
                    ' we need to split it
                    Dim splitPair = splitEntry(closest)

                    ' after adding the new entries derived from splitting /closest/ to this node,
                    ' if we have more than maxEntries we return false, 
                    ' so that the parent node will be split as well to redistribute the "load"
                    If entriesField.Count > maxNodeEntriesField Then
                        Return False ' splitting stops at this node
                    Else

                        If applyMergingRefinement_Conflict Then ' performs step 4 of insert process (see BIRCH paper, Section 4.3)
                            mergingRefinement(splitPair)
                        End If

                        Return True
                    End If
                End If
            ElseIf closest.isWithinThreshold(e, distThresholdField, distFunctionField) Then
                ' if  dist(closest,e) <= T, /e/ will be "absorbed" by /closest/
                closest.update(e)
                Return True ' no split necessary at the parent level
            ElseIf entriesField.Count < maxNodeEntriesField Then
                ' if /closest/ does not have children, and dist(closest,e) > T
                ' if there is enough room in this node, we simply add e to it
                entriesField.Add(e)
                Return True ' no split necessary at the parent level
                ' not enough space on this node
            Else
                entriesField.Add(e) ' adds it momentarily to this node
                Return False ' returns false so that the parent entry will be split
            End If

        End Function

        ''' 
        ''' <param name="closest"> the entry to be split </param>
        ''' <returns> the new entries derived from splitting </returns>
        Public Overridable Function splitEntry(closest As CFEntry) As CFEntryPair
            ' IF there was a child, but we could not insert the new entry without problems THAN
            ' split the child of closest entry

            Dim oldNode = closest.Child
            Dim oldEntries = closest.Child.Entries
            Dim p = findFarthestEntryPair(oldEntries)

            Dim newEntry1 As CFEntry = New CFEntry()
            Dim newNode1 As CFNode = New CFNode(maxNodeEntriesField, distThresholdField, distFunctionField, applyMergingRefinement_Conflict, oldNode.Leaf)
            newEntry1.Child = newNode1

            Dim newEntry2 As CFEntry = New CFEntry()
            Dim newNode2 As CFNode = New CFNode(maxNodeEntriesField, distThresholdField, distFunctionField, applyMergingRefinement_Conflict, oldNode.Leaf)
            newEntry2.Child = newNode2


            If oldNode.Leaf Then ' we do this to preserve the pointers in the leafList

                Dim prevL = oldNode.PreviousLeaf
                Dim nextL = oldNode.NextLeaf

                ' DEBUGGING STUFF...
                ' 				System.out.println(">>>>>>>>>>>>>>>>>> SPLIT <<<<<<<<<<<<<<<<<<<<");
                ' 				System.out.println("PREVL : " + prevL);
                ' 				System.out.println("NEXTL : " + nextL);
                ' 				

                If prevL IsNot Nothing Then
                    prevL.NextLeaf = newNode1
                End If

                If nextL IsNot Nothing Then
                    nextL.PreviousLeaf = newNode2
                End If

                newNode1.PreviousLeaf = prevL
                newNode1.NextLeaf = newNode2
                newNode2.PreviousLeaf = newNode1
                newNode2.NextLeaf = nextL
            End If


            redistributeEntries(oldEntries, p, newEntry1, newEntry2)
            ' redistributes the entries in n between newEntry1 and newEntry2
            ' according to the distance to p.e1 and p.e2

            entriesField.Remove(closest) ' this will be substitute by two new entries
            entriesField.Add(newEntry1)
            entriesField.Add(newEntry2)

            Dim newPair As CFEntryPair = New CFEntryPair(newEntry1, newEntry2)

            ' DEBUGGING STUFF...
            ' 			if(oldNode.isLeaf()) { 
            ' 				System.out.println(">>>>>>>>>>>>>>>>>> ---- <<<<<<<<<<<<<<<<<<<<");
            ' 				System.out.println("PREVL : " + newNode1.getPreviousLeaf());
            ' 				System.out.println("N1 : " + newNode1);
            ' 				System.out.println("N1.NEXT : " + newNode1.getNextLeaf());
            ' 				System.out.println("N2 : " + newNode2);
            ' 				System.out.println("N2.PREV : " + newNode2.getPreviousLeaf());
            ' 				System.out.println("NEXTL : " + newNode2.getNextLeaf());
            ' 				System.out.println(">>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<");
            ' 			}
            ' 			

            Return newPair
        End Function

        ''' <summary>
        ''' Called when splitting is necessary
        ''' </summary>
        ''' <param name="oldEntries"> </param>
        ''' <param name="farEntries"> </param>
        ''' <param name="newE1"> </param>
        ''' <param name="newE2"> </param>
        Protected Friend Overridable Sub redistributeEntries(oldEntries As List(Of CFEntry), farEntries As CFEntryPair, newE1 As CFEntry, newE2 As CFEntry)
            For Each e In oldEntries
                Dim dist1 = farEntries.e1.distance(e, distFunctionField)
                Dim dist2 = farEntries.e2.distance(e, distFunctionField)

                If dist1 <= dist2 Then
                    newE1.addToChild(e)
                    newE1.update(e)
                Else
                    newE2.addToChild(e)
                    newE2.update(e)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Called when "merging refinement" is attempted but no actual merging can be applied
        ''' </summary>
        ''' <param name="oldEntries1"> </param>
        ''' <param name="oldEntries2"> </param>
        Protected Friend Overridable Sub redistributeEntries(oldEntries1 As List(Of CFEntry), oldEntries2 As List(Of CFEntry), closeEntries As CFEntryPair, newE1 As CFEntry, newE2 As CFEntry)
            Dim v As List(Of CFEntry) = New List(Of CFEntry)()
            v.AddRange(oldEntries1)
            v.AddRange(oldEntries2)

            For Each e In v
                Dim dist1 = closeEntries.e1.distance(e, distFunctionField)
                Dim dist2 = closeEntries.e2.distance(e, distFunctionField)

                If dist1 <= dist2 Then
                    If newE1.ChildSize < maxNodeEntriesField Then
                        newE1.addToChild(e)
                        newE1.update(e)
                    Else
                        newE2.addToChild(e)
                        newE2.update(e)
                    End If
                ElseIf dist2 < dist1 Then
                    If newE2.ChildSize < maxNodeEntriesField Then
                        newE2.addToChild(e)
                        newE2.update(e)
                    Else
                        newE1.addToChild(e)
                        newE1.update(e)
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' Called when "merging refinement" is attempted and two entries are actually merged
        ''' </summary>
        ''' <param name="oldEntries1"> </param>
        ''' <param name="oldEntries2"> </param>
        Protected Friend Overridable Sub redistributeEntries(oldEntries1 As List(Of CFEntry), oldEntries2 As List(Of CFEntry), newE As CFEntry)
            Dim v As List(Of CFEntry) = New List(Of CFEntry)()
            v.AddRange(oldEntries1)
            v.AddRange(oldEntries2)

            For Each e In v
                newE.addToChild(e)
                newE.update(e)
            Next
        End Sub

        ''' 
        ''' <param name="e"> a CFEntry </param>
        ''' <returns> the entry in this node that is closest to e </returns>
        Protected Friend Overridable Function findClosestEntry(e As CFEntry) As CFEntry
            Dim minDist = Double.MaxValue
            Dim closest As CFEntry = Nothing
            For Each c In entriesField
                Dim d = c.distance(e, distFunctionField)
                If d < minDist Then
                    minDist = d
                    closest = c
                End If
            Next

            Return closest
        End Function

        Protected Friend Overridable Function findFarthestEntryPair(entries As List(Of CFEntry)) As CFEntryPair
            If entries.Count < 2 Then
                Return Nothing
            End If

            Dim maxDist As Double = -1
            Dim p As CFEntryPair = New CFEntryPair()

            For i = 0 To entries.Count - 1 - 1
                For j = i + 1 To entries.Count - 1
                    Dim e1 = entries(i)
                    Dim e2 = entries(j)

                    Dim dist = e1.distance(e2, distFunctionField)
                    If dist > maxDist Then
                        p.e1 = e1
                        p.e2 = e2
                        maxDist = dist
                    End If
                Next
            Next

            Return p
        End Function

        Protected Friend Overridable Function findClosestEntryPair(entries As List(Of CFEntry)) As CFEntryPair
            If entries.Count < 2 Then
                Return Nothing ' not possible to find a valid pair
            End If

            Dim minDist = Double.MaxValue
            Dim p As CFEntryPair = New CFEntryPair()

            For i = 0 To entries.Count - 1 - 1
                For j = i + 1 To entries.Count - 1
                    Dim e1 = entries(i)
                    Dim e2 = entries(j)

                    Dim dist = e1.distance(e2, distFunctionField)
                    If dist < minDist Then
                        p.e1 = e1
                        p.e2 = e2
                        minDist = dist
                    End If
                Next
            Next

            Return p
        End Function

        ''' <summary>
        ''' Used during merging refinement
        ''' </summary>
        ''' <param name="p"> </param>
        ''' <param name="newE1"> </param>
        ''' <param name="newE2"> </param>
        Private Sub replaceClosestPairWithNewEntries(p As CFEntryPair, newE1 As CFEntry, newE2 As CFEntry)
            For i = 0 To entriesField.Count - 1
                If entriesField(i).Equals(p.e1) Then
                    entriesField(i) = newE1

                ElseIf entriesField(i).Equals(p.e2) Then
                    entriesField(i) = newE2
                End If
            Next
        End Sub

        ''' <summary>
        ''' Used during merging refinement
        ''' </summary>
        ''' <param name="p"> </param>
        ''' <param name="newE"> </param>
        Private Sub replaceClosestPairWithNewMergedEntry(p As CFEntryPair, newE As CFEntry)
            For i = 0 To entriesField.Count - 1
                If entriesField(i).Equals(p.e1) Then
                    entriesField(i) = newE

                ElseIf entriesField(i).Equals(p.e2) Then
                    entriesField.RemoveAt(i)
                End If
            Next
        End Sub

        ''' 
        ''' <param name="splitEntries"> the entry that got split
        '''  </param>
        Public Overridable Sub mergingRefinement(splitEntries As CFEntryPair)

            ' System.out.println(">>>>>>>>>>>>>>> Merging Refinement <<<<<<<<<<<<");
            ' System.out.println(splitEntries.e1);
            ' System.out.println(splitEntries.e2);

            Dim nodeEntries = entriesField
            Dim p = findClosestEntryPair(nodeEntries)

            If p Is Nothing Then ' not possible to find a valid pair
                Return
            End If

            If p.Equals(splitEntries) Then
                Return ' if the closet pair is the one that was just split, we terminate
            End If

            Dim oldNode1 = p.e1.Child
            Dim oldNode2 = p.e2.Child

            Dim oldNode1Entries = oldNode1.Entries
            Dim oldNode2Entries = oldNode2.Entries

            If oldNode1.Leaf <> oldNode2.Leaf Then ' just to make sure everything is going ok
                Console.Error.WriteLine("ERROR: Nodes at the same level must have same leaf status")
                Environment.Exit(2)
            End If

            If oldNode1Entries.Count + oldNode2Entries.Count > maxNodeEntriesField Then
                ' the two nodes cannot be merged into one (they will not fit)
                ' in this case we simply redistribute them between p.e1 and p.e2

                Dim newEntry1 As CFEntry = New CFEntry()
                ' note: in the CFNode construction below the last parameter is false 
                ' because a split cannot happen at the leaf level 
                ' (the only exception is when the root is first split, but that's treated separately)
                Dim newNode1 = oldNode1
                newNode1.resetEntries()
                newEntry1.Child = newNode1

                Dim newEntry2 As CFEntry = New CFEntry()
                Dim newNode2 = oldNode2
                newNode2.resetEntries()
                newEntry2.Child = newNode2

                redistributeEntries(oldNode1Entries, oldNode2Entries, p, newEntry1, newEntry2)
                replaceClosestPairWithNewEntries(p, newEntry1, newEntry2)
            Else
                ' if the the two closest entries can actually be merged into one single entry

                Dim newEntry As CFEntry = New CFEntry()
                ' note: in the CFNode construction below the last parameter is false 
                ' because a split cannot happen at the leaf level 
                ' (the only exception is when the root is first split, but that's treated separately)
                Dim newNode As CFNode = New CFNode(maxNodeEntriesField, distThresholdField, distFunctionField, applyMergingRefinement_Conflict, oldNode1.Leaf)
                newEntry.Child = newNode

                redistributeEntries(oldNode1Entries, oldNode2Entries, newEntry)

                If oldNode1.Leaf AndAlso oldNode2.Leaf Then ' this is done to maintain proper links in the leafList
                    If oldNode1.PreviousLeaf IsNot Nothing Then
                        oldNode1.PreviousLeaf.NextLeaf = newNode
                    End If
                    If oldNode1.NextLeaf IsNot Nothing Then
                        oldNode1.NextLeaf.PreviousLeaf = newNode
                    End If
                    newNode.PreviousLeaf = oldNode1.PreviousLeaf
                    newNode.NextLeaf = oldNode1.NextLeaf

                    ' this is a dummy node that is only used to maintain proper links in the leafList
                    ' no CFEntry will ever point to this leaf
                    Dim dummy As CFNode = New CFNode(0, 0, 0, False, True)
                    If oldNode2.PreviousLeaf IsNot Nothing Then
                        oldNode2.PreviousLeaf.NextLeaf = dummy
                    End If
                    If oldNode2.NextLeaf IsNot Nothing Then
                        oldNode2.NextLeaf.PreviousLeaf = dummy
                    End If
                    dummy.PreviousLeaf = oldNode2.PreviousLeaf
                    dummy.NextLeaf = oldNode2.NextLeaf
                End If

                replaceClosestPairWithNewMergedEntry(p, newEntry)
            End If

            ' merging refinement is done
        End Sub

        ''' <summary>
        ''' Substitutes the entries in this node with the entries of the parameter node
        ''' </summary>
        ''' <param name="n"> the node from which entries are copied </param>
        Private Sub replaceEntries(n As CFNode)
            entriesField = n.entriesField
        End Sub

        Private Sub resetEntries()
            entriesField = New List(Of CFEntry)()
        End Sub

        Public Overridable ReadOnly Property Leaf As Boolean
            Get
                Return leafStatusField
            End Get
        End Property

        ''' 
        ''' <returns> true if merging refinement is enabled </returns>
        Public Overridable Function applyMergingRefinement() As Boolean
            Return applyMergingRefinement_Conflict
        End Function

        Protected Friend Overridable WriteOnly Property LeafStatus As Boolean
            Set(value As Boolean)
                leafStatusField = value
            End Set
        End Property



        Protected Friend Overridable Function countChildrenNodes() As Integer
            Dim n = 0
            For Each e In entriesField
                If e.hasChild() Then
                    n += 1
                    n += e.Child.countChildrenNodes()
                End If
            Next

            Return n
        End Function

        Protected Friend Overridable Function countEntriesInChildrenNodes() As Integer
            Dim n = 0
            For Each e In entriesField
                If e.hasChild() Then
                    n += e.Child.size()
                    n += e.Child.countChildrenNodes()
                End If
            Next

            Return n
        End Function

        Public Overrides Function ToString() As String
            Dim buff As StringBuilder = New StringBuilder()

            buff.Append("==============================================" & LINE_SEP)
            If Leaf Then
                buff.Append(">>> THIS IS A LEAF " & LINE_SEP)
            End If
            buff.Append("Num of Entries = " & entriesField.Count.ToString() & LINE_SEP)
            buff.Append("{")
            For Each e In entriesField
                buff.Append("[" & e.ToString() & "]")
            Next
            buff.Append("}" & LINE_SEP)
            buff.Append("==============================================" & LINE_SEP)

            Return buff.ToString()
        End Function
    End Class

End Namespace
