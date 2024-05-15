#Region "Microsoft.VisualBasic::224514d95b523dffd800775d954a4864, Data_science\Graph\PQTree.vb"

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

    '   Total Lines: 966
    '    Code Lines: 809
    ' Comment Lines: 90
    '   Blank Lines: 67
    '     File Size: 43.83 KB


    ' Class PQTree
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: allNodes, applyTemplate, contains, fromEmptyToFullP, fromFullToEmptyP
    '               indentString, isDescendant, isFullLeaf, isPertinentTree, recursivelyRemoveLeaf
    '               recursivelyRemoveLeafNotThroughQNodes, (+2 Overloads) reduce, sanityTest, (+2 Overloads) toString, ToString
    ' 
    '     Sub: allNodes, clearLabels, getLeaves, reverse
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.IEnumerations
Imports Microsoft.VisualBasic.ListExtensions

' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 

''' 
''' <summary>
''' @author santi
''' </summary>
Public Class PQTree

    Public Shared P_NODE As Integer = 0
    Public Shared Q_NODE As Integer = 1
    Public Shared LEAF_NODE As Integer = 2
    Public Shared DIRECTION_INDICATOR As Integer = 3

    Public Shared DIRECTION_INDICATOR_LEFT As Integer = 1
    Public Shared DIRECTION_INDICATOR_RIGHT As Integer = 2

    Public Const LABEL_NONE As Integer = -1
    Public Const LABEL_EMPTY As Integer = 0
    Public Const LABEL_PARTIAL As Integer = 1
    Public Const LABEL_FULL As Integer = 2

    Public Const NO_INDEX As Integer = -1

    Public nodeIndex As Integer ' if the nodes int he PQ-tree represent nodes in another graph,
    ' this is used to store their indexes
    Public nodeType As Integer
    Public direction As Integer ' left/right
    Public label As Integer ' empty, partial, full
    Public children As List(Of PQTree)
    Public parent As PQTree

    Public Sub New(a_index As Integer, a_type As Integer, a_parent As PQTree)
        nodeIndex = a_index
        nodeType = a_type
        If nodeType = PQTree.LEAF_NODE Then
            children = Nothing
        Else
            children = New List(Of PQTree)()
        End If
        parent = a_parent
        If parent IsNot Nothing Then
            parent.children.Add(Me)
        End If
        label = PQTree.LABEL_NONE
    End Sub


    Public Overridable Sub clearLabels()
        label = PQTree.LABEL_NONE
        If children IsNot Nothing Then
            For Each node As PQTree In children
                node.clearLabels()
            Next
        End If
    End Sub

    Public Overridable Function allNodes() As IList(Of PQTree)
        Dim l As IList(Of PQTree) = New List(Of PQTree)()
        allNodes(l)
        Return l
    End Function

    Private Sub allNodes(l As IList(Of PQTree))
        l.Add(Me)
        If children IsNot Nothing Then
            For Each child As PQTree In children
                child.allNodes(l)
            Next
        End If
    End Sub


    Public Overridable Sub getLeaves(l As IList(Of PQTree))
        If children Is Nothing OrElse children.Count = 0 Then
            l.Add(Me)
        Else
            For Each child As PQTree In children
                child.getLeaves(l)
            Next
        End If
    End Sub


    Public Overridable Function isFullLeaf(S As IList(Of PQTree)) As Boolean
        If S.Contains(Me) Then
            Return True
        End If
        Return False
    End Function


    Public Overridable Function isDescendant(ancestor As PQTree) As Boolean
        If ancestor Is Me Then
            Return True
        End If
        If parent Is Nothing Then
            Return False
        End If
        Return parent.isDescendant(ancestor)
    End Function


    Public Overridable Function isPertinentTree(S As IList(Of PQTree)) As Boolean
        For Each node As PQTree In S
            If Not node.isDescendant(Me) Then
                Return False
            End If
        Next
        Return True
    End Function

    Public Overridable Function applyTemplate(S As IList(Of PQTree)) As Boolean
        ' Node is full if: "all of its descendants are in S"
        ' Node is empty if: "none of its descendants are in S"
        ' Node is partial if: "some but not all of its descendants are in S"

        If nodeType = PQTree.DIRECTION_INDICATOR Then
            label = PQTree.LABEL_FULL
            Return True
        ElseIf nodeType = PQTree.LEAF_NODE Then
            ' Trivial templates:
            If isFullLeaf(S) Then
                label = PQTree.LABEL_FULL
                Return True
            Else
                label = PQTree.LABEL_EMPTY
                Return True
            End If
        ElseIf nodeType = PQTree.P_NODE Then
            Dim counts = New Integer() {0, 0, 0}
            For Each child As PQTree In children
                If child.nodeType <> PQTree.DIRECTION_INDICATOR Then
                    counts(child.label) += 1
                End If
            Next

            ' Template P0: (Figure 7, left)
            If counts(PQTree.LABEL_EMPTY) > 0 AndAlso counts(PQTree.LABEL_PARTIAL) = 0 AndAlso counts(PQTree.LABEL_FULL) = 0 Then
                label = PQTree.LABEL_EMPTY
                Return True
            End If
            ' Template P1: (Figure 7, right)
            If counts(PQTree.LABEL_EMPTY) = 0 AndAlso counts(PQTree.LABEL_PARTIAL) = 0 AndAlso counts(PQTree.LABEL_FULL) > 0 Then
                label = PQTree.LABEL_FULL
                Return True
            End If

            If isPertinentTree(S) Then
                ' Template P2: (Figure 8)
                If counts(PQTree.LABEL_EMPTY) > 0 AndAlso counts(PQTree.LABEL_PARTIAL) = 0 AndAlso counts(PQTree.LABEL_FULL) > 0 Then
                    Dim P1 As PQTree = New PQTree(PQTree.NO_INDEX, PQTree.P_NODE, Nothing)
                    P1.label = PQTree.LABEL_FULL
                    For Each child As PQTree In children
                        If child.label = PQTree.LABEL_FULL Then
                            P1.children.Add(child)
                            child.parent = P1
                        End If
                    Next

                    children.RemoveAll(P1.children)
                    children.Add(P1)
                    P1.parent = Me
                    Return True
                End If

                ' Template P4: (Figure 11)
                If counts(PQTree.LABEL_EMPTY) >= 0 AndAlso counts(PQTree.LABEL_PARTIAL) = 1 AndAlso counts(PQTree.LABEL_FULL) >= 0 Then
                    Dim partialChild As PQTree = Nothing
                    For Each child As PQTree In children
                        If child.label = PQTree.LABEL_PARTIAL Then
                            partialChild = child
                            Exit For
                        End If
                    Next
                    Dim P1 As PQTree = New PQTree(PQTree.NO_INDEX, PQTree.P_NODE, Nothing)
                    P1.label = PQTree.LABEL_FULL
                    For Each child As PQTree In children
                        If child.label = PQTree.LABEL_FULL Then
                            child.parent = P1
                            P1.children.Add(child)
                            child.parent = P1
                        End If
                    Next

                    children.RemoveAll(P1.children)
                    If P1.children.Count = 1 Then
                        P1.children(0).parent = partialChild
                        If partialChild.fromEmptyToFullP() Then
                            '                        if (partialChild.firstChildEmptyP()) {
                            '                        if (partialChild.children.get(0).label==LABEL_EMPTY) {
                            partialChild.children.Add(P1.children(0))
                            P1.children(0).parent = partialChild
                        ElseIf partialChild.fromFullToEmptyP() Then
                            partialChild.children.Insert(0, P1.children(0))
                            P1.children(0).parent = partialChild
                        Else
                            Return False
                        End If
                    Else
                        P1.parent = partialChild
                        If partialChild.fromEmptyToFullP() Then
                            '                        if (partialChild.firstChildEmptyP()) {
                            '                        if (partialChild.children.get(0).label==LABEL_EMPTY) {
                            partialChild.children.Add(P1)
                            P1.parent = partialChild
                        ElseIf partialChild.fromFullToEmptyP() Then
                            partialChild.children.Insert(0, P1)
                            P1.parent = partialChild
                        Else
                            Return False
                        End If
                    End If
                    Return True
                End If

                ' Template P6: (Figure 13)
                If counts(PQTree.LABEL_EMPTY) >= 0 AndAlso counts(PQTree.LABEL_PARTIAL) = 2 AndAlso counts(PQTree.LABEL_FULL) >= 0 Then
                    Dim partial1 As PQTree = Nothing
                    Dim partial2 As PQTree = Nothing
                    Dim P1 As PQTree = New PQTree(PQTree.NO_INDEX, PQTree.P_NODE, Nothing)
                    Dim Q1 As PQTree = New PQTree(PQTree.NO_INDEX, PQTree.Q_NODE, Nothing)
                    P1.label = PQTree.LABEL_FULL
                    Q1.label = PQTree.LABEL_PARTIAL
                    For Each child As PQTree In children
                        If child.label = PQTree.LABEL_PARTIAL Then
                            If partial1 Is Nothing Then
                                partial1 = child
                            Else
                                partial2 = child
                            End If
                        End If
                        If child.label = PQTree.LABEL_FULL Then
                            P1.children.Add(child)
                            child.parent = P1
                        End If
                    Next
                    If partial1.fromEmptyToFullP() Then
                        '                    if (partial1.firstChildEmptyP()) {                    
                        '                    if (partial1.children.get(0).label==LABEL_EMPTY) {
                        ' they are in the right order:
                        For Each child2 As PQTree In partial1.children
                            Q1.children.Add(child2)
                            child2.parent = Q1
                        Next
                    ElseIf partial1.fromFullToEmptyP() Then
                        ' we need to reverse them:
                        For i = partial1.children.Count - 1 To 0 Step -1
                            partial1.children(i).reverse()
                            Q1.children.Add(partial1.children(i))
                            partial1.children(i).parent = Q1
                        Next
                    Else
                        Return False
                    End If
                    If P1.children.Count > 0 Then
                        If P1.children.Count = 1 Then
                            Q1.children.Add(P1.children(0))
                            P1.children(0).parent = Q1
                        Else
                            Q1.children.Add(P1)
                            P1.parent = Q1
                        End If
                    End If
                    If partial2.fromFullToEmptyP() Then
                        '                    if (partial2.firstChildFullP()) {
                        '                    if (partial2.children.get(0).label==LABEL_FULL) {
                        ' they are in the right order:
                        For Each child2 As PQTree In partial2.children
                            Q1.children.Add(child2)
                            child2.parent = Q1
                        Next
                    ElseIf partial2.fromEmptyToFullP() Then
                        ' we need to reverse them:
                        For i = partial2.children.Count - 1 To 0 Step -1
                            partial2.children(i).reverse()
                            Q1.children.Add(partial2.children(i))
                            partial2.children(i).parent = Q1
                        Next
                    Else
                        Return False
                    End If

                    children.RemoveAll(P1.children)
                    children.Remove(partial1)
                    children.Remove(partial2)
                    children.Add(Q1)
                    Q1.parent = Me
                    Return True
                End If
            Else
                ' Template P3:  (Figures 9, 10)
                If counts(PQTree.LABEL_EMPTY) > 0 AndAlso counts(PQTree.LABEL_PARTIAL) = 0 AndAlso counts(PQTree.LABEL_FULL) > 0 Then
                    If counts(PQTree.LABEL_EMPTY) > 1 Then
                        If counts(PQTree.LABEL_FULL) > 1 Then
                            Dim Pnode1 As PQTree = New PQTree(PQTree.NO_INDEX, PQTree.P_NODE, Nothing)
                            Dim Pnode2 As PQTree = New PQTree(PQTree.NO_INDEX, PQTree.P_NODE, Nothing)
                            Pnode1.label = PQTree.LABEL_EMPTY
                            Pnode2.label = PQTree.LABEL_FULL
                            For Each child As PQTree In children
                                If child.label = PQTree.LABEL_EMPTY Then
                                    Pnode1.children.Add(child)
                                    child.parent = Pnode1
                                Else
                                    Pnode2.children.Add(child)
                                    child.parent = Pnode1
                                End If
                            Next
                            children.Clear()
                            children.Add(Pnode1)
                            children.Add(Pnode2)
                            Pnode1.parent = Me
                            Pnode2.parent = Me
                            nodeType = PQTree.Q_NODE
                            label = PQTree.LABEL_PARTIAL
                        Else
                            Dim Pnode1 As PQTree = New PQTree(PQTree.NO_INDEX, PQTree.P_NODE, Nothing)
                            Pnode1.label = PQTree.LABEL_EMPTY
                            For Each child As PQTree In children
                                If child.label = PQTree.LABEL_EMPTY Then
                                    Pnode1.children.Add(child)
                                    child.parent = Pnode1
                                End If
                            Next

                            children.RemoveAll(Pnode1.children)
                            children.Add(Pnode1)
                            Pnode1.parent = Me
                            nodeType = PQTree.Q_NODE
                            label = PQTree.LABEL_PARTIAL
                        End If
                    Else
                        If counts(PQTree.LABEL_FULL) > 1 Then
                            Dim Pnode2 As PQTree = New PQTree(PQTree.NO_INDEX, PQTree.P_NODE, Nothing)
                            Pnode2.label = PQTree.LABEL_FULL
                            For Each child As PQTree In children
                                If child.label = PQTree.LABEL_FULL Then
                                    Pnode2.children.Add(child)
                                    child.parent = Pnode2
                                End If
                            Next

                            children.RemoveAll(Pnode2.children)
                            children.Add(Pnode2)
                            Pnode2.parent = Me
                            nodeType = PQTree.Q_NODE
                            label = PQTree.LABEL_PARTIAL
                        Else
                            nodeType = PQTree.Q_NODE
                            label = PQTree.LABEL_PARTIAL
                        End If
                    End If
                    Return True
                End If

                ' Template P5: (Figure 12)
                If counts(PQTree.LABEL_EMPTY) >= 0 AndAlso counts(PQTree.LABEL_PARTIAL) = 1 AndAlso counts(PQTree.LABEL_FULL) >= 0 Then
                    Dim P1 As PQTree = New PQTree(PQTree.NO_INDEX, PQTree.P_NODE, Nothing)
                    Dim P2 As PQTree = New PQTree(PQTree.NO_INDEX, PQTree.P_NODE, Nothing)
                    P1.label = PQTree.LABEL_EMPTY
                    P2.label = PQTree.LABEL_FULL
                    Dim Q As PQTree = Nothing
                    For Each child As PQTree In children
                        If child.label = PQTree.LABEL_EMPTY Then
                            P1.children.Add(child)
                            child.parent = P1
                        End If
                        If child.label = PQTree.LABEL_FULL Then
                            P2.children.Add(child)
                            child.parent = P2
                        End If
                        If child.label = PQTree.LABEL_PARTIAL Then
                            Q = child
                        End If
                    Next
                    children.Clear()
                    If P1.children.Count > 0 Then
                        If P1.children.Count = 1 Then
                            children.Add(P1.children(0))
                            P1.children(0).parent = Me
                        Else
                            children.Add(P1)
                            P1.parent = Me
                        End If
                    End If
                    If Q.fromEmptyToFullP() Then
                        '                    if (Q.firstChildEmptyP()) {
                        '                    if (Q.children.get(0).label==LABEL_EMPTY) {
                        ' they are in the right order:
                        For Each child2 As PQTree In Q.children
                            children.Add(child2)
                            child2.parent = Me
                        Next
                    ElseIf Q.fromEmptyToFullP() Then
                        ' we need to reverse them:
                        For i = Q.children.Count - 1 To 0 Step -1
                            Dim tmp As PQTree = Q.children(i)
                            children.Add(tmp)
                            tmp.parent = Me
                            tmp.reverse()
                        Next
                    Else
                        Return False
                    End If
                    If P2.children.Count > 0 Then
                        If P2.children.Count = 1 Then
                            children.Add(P2.children(0))
                            P2.children(0).parent = Me
                        Else
                            children.Add(P2)
                            P2.parent = Me
                        End If
                    End If
                    nodeType = PQTree.Q_NODE
                    label = PQTree.LABEL_PARTIAL
                    Return True
                End If
            End If
        ElseIf nodeType = PQTree.Q_NODE Then
            Dim counts = New Integer() {0, 0, 0}
            For Each child As PQTree In children
                If child.nodeType <> PQTree.DIRECTION_INDICATOR Then
                    counts(child.label) += 1
                End If
            Next

            ' Template Q0: 
            If counts(PQTree.LABEL_EMPTY) > 0 AndAlso counts(PQTree.LABEL_PARTIAL) = 0 AndAlso counts(PQTree.LABEL_FULL) = 0 Then
                label = PQTree.LABEL_EMPTY
                Return True
            End If
            ' Template Q1: 
            If counts(PQTree.LABEL_EMPTY) = 0 AndAlso counts(PQTree.LABEL_PARTIAL) = 0 AndAlso counts(PQTree.LABEL_FULL) > 0 Then
                label = PQTree.LABEL_FULL
                Return True
            End If

            ' (Santi: Actually pattern Q2 is a special case of Q3, so I'll implement them together)            
            ' Template Q2: (Figure 14)
            ' If all the full leaves are either on the right or on the left
            ' Template Q3: (Figure 15):
            ' If all the full leaves ate groupped together in the middle, and empty ones are on the right AND left
            If counts(PQTree.LABEL_PARTIAL) <= 2 Then
                If counts(PQTree.LABEL_PARTIAL) = 0 AndAlso (counts(PQTree.LABEL_EMPTY) > 0 OrElse counts(PQTree.LABEL_FULL) > 0) Then
                    ' check whether the chilren are in the right order
                    Dim goodEF = True ' Empty -> Full order
                    Dim goodFE = True ' Full -> Empty order
                    Dim previous = -1
                    For i = 0 To children.Count - 1
                        If children(i).nodeType = PQTree.DIRECTION_INDICATOR Then
                            Continue For
                        End If
                        If previous <> -1 Then
                            If children(previous).label = PQTree.LABEL_FULL AndAlso children(i).label = PQTree.LABEL_EMPTY Then
                                goodEF = False
                            End If
                            If children(previous).label = PQTree.LABEL_EMPTY AndAlso children(i).label = PQTree.LABEL_FULL Then
                                goodFE = False
                            End If
                        End If
                        previous = i
                    Next
                    If goodEF OrElse goodFE Then
                        label = PQTree.LABEL_PARTIAL
                        Return True
                    End If
                    Return False
                ElseIf counts(PQTree.LABEL_PARTIAL) = 1 AndAlso (counts(PQTree.LABEL_EMPTY) > 0 OrElse counts(PQTree.LABEL_FULL) > 0) Then
                    ' 1 partial Q-node:
                    Dim newChildrenEF As IList(Of PQTree) = New List(Of PQTree)()
                    Dim newChildrenFE As IList(Of PQTree) = New List(Of PQTree)()
                    Dim reverseIfEF As IList(Of PQTree) = New List(Of PQTree)()
                    Dim reverseIfFE As IList(Of PQTree) = New List(Of PQTree)()
                    Dim EFstate = 0 ' 0 : empty, 1: full
                    Dim FEstate = 0 ' 0 : full, 1: empty
                    For Each child As PQTree In children
                        If child.nodeType = PQTree.DIRECTION_INDICATOR Then
                            newChildrenEF.Add(child)
                            newChildrenFE.Add(child)
                            Continue For
                        End If
                        If EFstate = 0 Then
                            If child.label = PQTree.LABEL_EMPTY Then
                                newChildrenEF.Add(child)
                            ElseIf child.label = PQTree.LABEL_FULL Then
                                newChildrenEF.Add(child)
                                EFstate = 1
                            ElseIf child.label = PQTree.LABEL_PARTIAL Then
                                If child.fromEmptyToFullP() Then
                                    '                                if (child.firstChildEmptyP()) {
                                    '                                if (child.children.get(0).label==LABEL_EMPTY) {
                                    For Each child2 As PQTree In child.children
                                        newChildrenEF.Add(child2)
                                    Next
                                ElseIf child.fromFullToEmptyP() Then
                                    For i = child.children.Count - 1 To 0 Step -1
                                        newChildrenEF.Add(child.children(i))
                                        reverseIfEF.Add(child.children(i))
                                    Next
                                Else
                                    Return False
                                End If
                                EFstate = 1
                            End If
                        ElseIf EFstate = 1 Then
                            If child.label = PQTree.LABEL_EMPTY Then
                                EFstate = 2
                            ElseIf child.label = PQTree.LABEL_FULL Then
                                newChildrenEF.Add(child)
                            ElseIf child.label = PQTree.LABEL_PARTIAL Then
                                EFstate = 2
                            End If
                        End If
                        If FEstate = 0 Then
                            If child.label = PQTree.LABEL_FULL Then
                                newChildrenFE.Add(child)
                            ElseIf child.label = PQTree.LABEL_EMPTY Then
                                newChildrenFE.Add(child)
                                FEstate = 1
                            ElseIf child.label = PQTree.LABEL_PARTIAL Then
                                If child.fromFullToEmptyP() Then
                                    '                                if (child.firstChildFullP()) {
                                    '                                if (child.children.get(0).label==LABEL_FULL) {
                                    For Each child2 As PQTree In child.children
                                        newChildrenFE.Add(child2)
                                    Next
                                ElseIf child.fromEmptyToFullP() Then
                                    For i = child.children.Count - 1 To 0 Step -1
                                        newChildrenFE.Add(child.children(i))
                                        reverseIfFE.Add(child.children(i))
                                    Next
                                Else
                                    Return False
                                End If
                                FEstate = 1
                            End If
                        ElseIf FEstate = 1 Then
                            If child.label = PQTree.LABEL_FULL Then
                                FEstate = 2
                            ElseIf child.label = PQTree.LABEL_EMPTY Then
                                newChildrenFE.Add(child)
                            ElseIf child.label = PQTree.LABEL_PARTIAL Then
                                FEstate = 2
                            End If
                        End If
                    Next
                    If EFstate = 1 Then
                        children.Clear()
                        label = PQTree.LABEL_PARTIAL
                        For Each child As PQTree In newChildrenEF
                            children.Add(child)
                            child.parent = Me
                        Next
                        For Each child As PQTree In reverseIfEF
                            child.reverse()
                        Next
                        Return True
                    ElseIf FEstate = 1 Then
                        children.Clear()
                        label = PQTree.LABEL_PARTIAL
                        For Each child As PQTree In newChildrenFE
                            children.Add(child)
                            child.parent = Me
                        Next
                        For Each child As PQTree In reverseIfFE
                            child.reverse()
                        Next
                        Return True
                    End If
                    Return False
                Else
                    ' 2 partial Q-nodes:
                    Dim newChildren As IList(Of PQTree) = New List(Of PQTree)()
                    Dim status = 0 ' 0 = empty, 1 = full, 2 = empty again, 3: error
                    For Each child As PQTree In children
                        If child.nodeType = PQTree.DIRECTION_INDICATOR Then
                            newChildren.Add(child)
                            Continue For
                        End If
                        Select Case status
                            Case 0
                                Select Case child.label
                                    Case PQTree.LABEL_EMPTY
                                        newChildren.Add(child)
                                    Case PQTree.LABEL_PARTIAL
                                        If child.fromEmptyToFullP() Then
                                            '                                if (child.firstChildEmptyP()) {                                
                                            '                                if (child.children.get(0).label==LABEL_EMPTY) {
                                            ' thye are in the right order:
                                            For Each child2 As PQTree In child.children
                                                newChildren.Add(child2)
                                            Next
                                        ElseIf child.fromFullToEmptyP() Then
                                            ' we need to reverse them:
                                            For i = child.children.Count - 1 To 0 Step -1
                                                newChildren.Add(child.children(i))
                                                child.children(i).reverse()
                                            Next
                                        Else
                                            Return False
                                        End If
                                        status = 1
                                    Case PQTree.LABEL_FULL
                                        newChildren.Add(child)
                                        status = 1
                                End Select
                            Case 1
                                Select Case child.label
                                    Case PQTree.LABEL_EMPTY
                                        newChildren.Add(child)
                                        status = 2
                                    Case PQTree.LABEL_PARTIAL
                                        If child.fromFullToEmptyP() Then
                                            '                                if (child.firstChildFullP()) {
                                            '                                if (child.children.get(0).label==LABEL_FULL) {
                                            ' thye are in the right order:
                                            For Each child2 As PQTree In child.children
                                                newChildren.Add(child2)
                                            Next
                                        ElseIf child.fromFullToEmptyP() Then
                                            ' we need to reverse them:
                                            For i = child.children.Count - 1 To 0 Step -1
                                                newChildren.Add(child.children(i))
                                                child.children(i).reverse()
                                            Next
                                        Else
                                            Return False
                                        End If
                                        status = 2
                                    Case PQTree.LABEL_FULL
                                        newChildren.Add(child)
                                End Select
                            Case 2
                                Select Case child.label
                                    Case PQTree.LABEL_EMPTY
                                        newChildren.Add(child)
                                    Case PQTree.LABEL_PARTIAL
                                        Return False
                                    Case PQTree.LABEL_FULL
                                        Return False
                                End Select
                        End Select
                    Next
                    children.Clear()
                    For Each child As PQTree In newChildren
                        children.Add(child)
                        child.parent = Me
                    Next
                    label = PQTree.LABEL_PARTIAL
                    Return True
                End If
            End If

        End If

        Return False
    End Function

    Public Overridable Function reduce(index As Integer) As Boolean
        Dim tmp As IList(Of PQTree) = New List(Of PQTree)()
        Dim S As IList(Of PQTree) = New List(Of PQTree)()
        getLeaves(tmp)
        For Each leaf As PQTree In tmp
            If leaf.nodeIndex = index Then
                S.Add(leaf)
            End If
        Next

        Return reduce(S)
    End Function

    Public Overridable Function reduce(S As IList(Of PQTree)) As Boolean
        Dim processed As ISet(Of PQTree) = New HashSet(Of PQTree)()
        Dim queue As List(Of PQTree) = New List(Of PQTree)()

        ' add all the leaves of the tree to the queue
        getLeaves(queue)
        clearLabels()

        While queue.Count > 0
            Dim X As PQTree = queue.PopAt(0)
            If Not X.applyTemplate(S) Then
                Return False
            End If
            processed.Add(X)

            ' If all the nodes in "S" are in node descendants of X, then we are done:
            If X.isPertinentTree(S) Then
                Return True
            End If

            ' If all the siblings of X have been matched, then place the parent in the queue:
            If X.parent IsNot Nothing Then
                If processed.ContainsAll(X.parent.children) Then
                    queue.Add(X.parent)
                End If
            End If
        End While

        Return True
    End Function


    Public Overridable Function fromEmptyToFullP() As Boolean
        Dim state = 0
        For Each child As PQTree In children
            If child.nodeType = PQTree.DIRECTION_INDICATOR Then
                Continue For
            End If
            Select Case state
                Case 0
                    If child.label = PQTree.LABEL_FULL Then
                        state = 1
                    ElseIf child.label = PQTree.LABEL_PARTIAL Then
                        If Not child.fromEmptyToFullP() Then
                            Return False
                        End If
                    End If
                Case 1
                    If child.label <> PQTree.LABEL_FULL Then
                        Return False
                    End If
            End Select
        Next
        Return True
    End Function


    Public Overridable Function fromFullToEmptyP() As Boolean
        Dim state = 0
        For Each child As PQTree In children
            If child.nodeType = PQTree.DIRECTION_INDICATOR Then
                Continue For
            End If
            Select Case state
                Case 0
                    If child.label = PQTree.LABEL_EMPTY Then
                        state = 1
                    ElseIf child.label = PQTree.LABEL_PARTIAL Then
                        If Not child.fromFullToEmptyP() Then
                            Return False
                        End If
                    End If
                Case 1
                    If child.label <> PQTree.LABEL_EMPTY Then
                        Return False
                    End If
            End Select
        Next
        Return True
    End Function

    Public Overridable Sub reverse()
        If nodeType = PQTree.Q_NODE Then
            Dim tmp As IList(Of PQTree) = New List(Of PQTree)()
            CType(tmp, List(Of PQTree)).AddRange(children)
            children.Clear()

            For Each child As PQTree In tmp
                child.reverse()
                children.Insert(0, child)
            Next
        ElseIf nodeType = PQTree.DIRECTION_INDICATOR Then
            If nodeType = PQTree.DIRECTION_INDICATOR Then
                If direction = PQTree.DIRECTION_INDICATOR_LEFT Then
                    direction = PQTree.DIRECTION_INDICATOR_RIGHT
                Else
                    direction = PQTree.DIRECTION_INDICATOR_LEFT

                End If
            End If
        End If
    End Sub


    ' 
    ' 		
    ' 		public boolean firstChildEmptyP() {
    ' 		    for(PQTree child:children) {
    ' 		        if (child.nodeType==DIRECTION_INDICATOR) continue;
    ' 		        if (child.label == LABEL_EMPTY) {
    ' 		            return true;
    ' 		        } else {
    ' 		            return false;
    ' 		        }
    ' 		    }
    ' 		    return false;
    ' 		}
    ' 		
    ' 		
    ' 		public boolean firstChildFullP() {
    ' 		    for(PQTree child:children) {
    ' 		        if (child.nodeType==DIRECTION_INDICATOR) continue;
    ' 		        if (child.label == LABEL_FULL) {
    ' 		            return true;
    ' 		        } else {
    ' 		            return false;
    ' 		        }
    ' 		    }
    ' 		    return false;
    ' 		}
    ' 	
    ' 		

    Public Overridable Function recursivelyRemoveLeaf(leaf As PQTree) As Boolean
        If children IsNot Nothing Then
            If children.Remove(leaf) Then
                Return True
            End If
            For Each child As PQTree In children
                If child.recursivelyRemoveLeaf(leaf) Then
                    Return True
                End If
            Next
        End If
        Return False
    End Function


    Public Overridable Function recursivelyRemoveLeafNotThroughQNodes(leaf As PQTree) As Boolean
        If children IsNot Nothing Then
            If children.Remove(leaf) Then
                Return True
            End If
            For Each child As PQTree In children
                If child.nodeType <> PQTree.Q_NODE AndAlso child.recursivelyRemoveLeafNotThroughQNodes(leaf) Then
                    Return True
                End If
            Next
        End If
        Return False
    End Function

    Public Overridable Function contains(node As PQTree) As Boolean
        If Me Is node Then
            Return True
        End If
        If children IsNot Nothing Then
            For Each child As PQTree In children
                If child.contains(node) Then
                    Return True
                End If
            Next
        End If
        Return False
    End Function



    Public Overridable Function sanityTest() As Boolean
        Dim nodes As IList(Of PQTree) = allNodes()
        For Each node As PQTree In nodes
            If node.parent IsNot Nothing Then
                If Not node.parent.children.Contains(node) Then
                    Console.WriteLine("sanityTest: Inconsistent parent in node: " & node.ToString())
                    Return False
                End If
                If node.children IsNot Nothing Then
                    For Each node2 As PQTree In node.children
                        If node2.parent IsNot node Then
                            Console.WriteLine("sanityTest: Inconsistent parent in node: " & node2.ToString())
                            Return False
                        End If
                    Next
                End If
            End If
        Next
        Console.WriteLine("sanityTest: passed")
        Return True
    End Function


    Public Overrides Function ToString() As String
        Return toString(0, Nothing)
    End Function

    Public Overloads Function toString(nodeParent As Dictionary(Of PQTree, Integer)) As String
        Return toString(0, nodeParent)
    End Function

    Friend Overridable Function indentString(indents As Integer) As String
        Dim out = ""
        For i = 0 To indents - 1
            out += "  "
        Next
        Return out
    End Function


    Public Overloads Function toString(indents As Integer, nodeParent As Dictionary(Of PQTree, Integer)) As String
        If nodeType = PQTree.DIRECTION_INDICATOR Then
            If direction = PQTree.DIRECTION_INDICATOR_LEFT Then
                Return indentString(indents) & "direction-indicator-left(" & nodeIndex.ToString() & ")"
            End If
            If direction = PQTree.DIRECTION_INDICATOR_RIGHT Then
                Return indentString(indents) & "direction-indicator-right(" & nodeIndex.ToString() & ")"
            End If
            Return indentString(indents) & "direction-indicator-???(" & nodeIndex.ToString() & ")"
        ElseIf nodeType = PQTree.LEAF_NODE Then
            If nodeParent IsNot Nothing Then
                If label = PQTree.LABEL_FULL Then
                    Return indentString(indents) & "full-leaf(" & nodeIndex.ToString() & " from " & nodeParent(Me).ToString() & ")"
                End If
                If label = PQTree.LABEL_EMPTY Then
                    Return indentString(indents) & "empty-leaf(" & nodeIndex.ToString() & " from " & nodeParent(Me).ToString() & ")"
                End If
                Return indentString(indents) & "leaf(" & nodeIndex.ToString() & " from " & nodeParent(Me).ToString() & ")"
            Else
                If label = PQTree.LABEL_FULL Then
                    Return indentString(indents) & "full-leaf(" & nodeIndex.ToString() & ")"
                End If
                If label = PQTree.LABEL_EMPTY Then
                    Return indentString(indents) & "empty-leaf(" & nodeIndex.ToString() & ")"
                End If
                Return indentString(indents) & "leaf(" & nodeIndex.ToString() & ")"
            End If
        ElseIf nodeType = PQTree.P_NODE Then
            Dim tmp As String = indentString(indents) & "P-node(" & nodeIndex.ToString() & ")(" & vbLf
            If label = PQTree.LABEL_FULL Then
                tmp = indentString(indents) & "full-P-node(" & nodeIndex.ToString() & ")(" & vbLf
            End If
            If label = PQTree.LABEL_PARTIAL Then
                tmp = indentString(indents) & "partial-P-node(" & nodeIndex.ToString() & ")(" & vbLf
            End If
            If label = PQTree.LABEL_EMPTY Then
                tmp = indentString(indents) & "empty-P-node(" & nodeIndex.ToString() & ")(" & vbLf
            End If
            Dim first = True
            For Each child As PQTree In children
                If Not first Then
                    tmp += "," & vbLf
                End If
                tmp += child.toString(indents + 1, nodeParent)
                first = False
            Next
            Return tmp & ")"
        Else
            Dim tmp = indentString(indents) & "Q-Node(" & vbLf
            If label = PQTree.LABEL_FULL Then
                tmp = indentString(indents) & "full-Q-node(" & nodeIndex.ToString() & ")(" & vbLf
            End If
            If label = PQTree.LABEL_PARTIAL Then
                tmp = indentString(indents) & "partial-Q-node(" & nodeIndex.ToString() & ")(" & vbLf
            End If
            If label = PQTree.LABEL_EMPTY Then
                tmp = indentString(indents) & "empty-Q-node(" & nodeIndex.ToString() & ")(" & vbLf
            End If
            Dim first = True
            For Each child As PQTree In children
                If Not first Then
                    tmp += "," & vbLf
                End If
                tmp += child.toString(indents + 1, nodeParent)
                first = False
            Next
            Return tmp & ")"
        End If
    End Function

End Class
