Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.ListExtensions
Imports Microsoft.VisualBasic.IEnumerations

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
    Public Class PQTree
        Public Shared DEBUG As Integer = 0

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
        Public children As List(Of OrthographicEmbedding.PQTree)
        Public parent As OrthographicEmbedding.PQTree

        Public Sub New(a_index As Integer, a_type As Integer, a_parent As OrthographicEmbedding.PQTree)
            nodeIndex = a_index
            nodeType = a_type
            If nodeType = OrthographicEmbedding.PQTree.LEAF_NODE Then
                children = Nothing
            Else
                children = New List(Of OrthographicEmbedding.PQTree)()
            End If
            parent = a_parent
            If parent IsNot Nothing Then
                parent.children.Add(Me)
            End If
            label = OrthographicEmbedding.PQTree.LABEL_NONE
        End Sub


        Public Overridable Sub clearLabels()
            label = OrthographicEmbedding.PQTree.LABEL_NONE
            If children IsNot Nothing Then
                For Each node As OrthographicEmbedding.PQTree In children
                    node.clearLabels()
                Next
            End If
        End Sub

        Public Overridable Function allNodes() As IList(Of OrthographicEmbedding.PQTree)
            Dim l As IList(Of OrthographicEmbedding.PQTree) = New List(Of OrthographicEmbedding.PQTree)()
            allNodes(l)
            Return l
        End Function

        Private Sub allNodes(l As IList(Of OrthographicEmbedding.PQTree))
            l.Add(Me)
            If children IsNot Nothing Then
                For Each child As OrthographicEmbedding.PQTree In children
                    child.allNodes(l)
                Next
            End If
        End Sub


        Public Overridable Sub getLeaves(l As IList(Of OrthographicEmbedding.PQTree))
            If children Is Nothing OrElse children.Count = 0 Then
                l.Add(Me)
            Else
                For Each child As OrthographicEmbedding.PQTree In children
                    child.getLeaves(l)
                Next
            End If
        End Sub


        Public Overridable Function isFullLeaf(S As IList(Of OrthographicEmbedding.PQTree)) As Boolean
            If S.Contains(Me) Then
                Return True
            End If
            Return False
        End Function


        Public Overridable Function isDescendant(ancestor As OrthographicEmbedding.PQTree) As Boolean
            If ancestor Is Me Then
                Return True
            End If
            If parent Is Nothing Then
                Return False
            End If
            Return parent.isDescendant(ancestor)
        End Function


        Public Overridable Function isPertinentTree(S As IList(Of OrthographicEmbedding.PQTree)) As Boolean
            For Each node As OrthographicEmbedding.PQTree In S
                If Not node.isDescendant(Me) Then
                    Return False
                End If
            Next
            Return True
        End Function

        Public Overridable Function applyTemplate(S As IList(Of OrthographicEmbedding.PQTree)) As Boolean
            ' Node is full if: "all of its descendants are in S"
            ' Node is empty if: "none of its descendants are in S"
            ' Node is partial if: "some but not all of its descendants are in S"

            If nodeType = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR Then
                label = OrthographicEmbedding.PQTree.LABEL_FULL
                Return True
            ElseIf nodeType = OrthographicEmbedding.PQTree.LEAF_NODE Then
                ' Trivial templates:
                If isFullLeaf(S) Then
                    label = OrthographicEmbedding.PQTree.LABEL_FULL
                    Return True
                Else
                    label = OrthographicEmbedding.PQTree.LABEL_EMPTY
                    Return True
                End If
            ElseIf nodeType = OrthographicEmbedding.PQTree.P_NODE Then
                Dim counts = New Integer() {0, 0, 0}
                For Each child As OrthographicEmbedding.PQTree In children
                    If child.nodeType <> OrthographicEmbedding.PQTree.DIRECTION_INDICATOR Then
                        counts(child.label) += 1
                    End If
                Next

                ' Template P0: (Figure 7, left)
                If counts(OrthographicEmbedding.PQTree.LABEL_EMPTY) > 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_PARTIAL) = 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_FULL) = 0 Then
                    label = OrthographicEmbedding.PQTree.LABEL_EMPTY
                    If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                        Console.WriteLine("P0")
                    End If
                    Return True
                End If
                ' Template P1: (Figure 7, right)
                If counts(OrthographicEmbedding.PQTree.LABEL_EMPTY) = 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_PARTIAL) = 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_FULL) > 0 Then
                    label = OrthographicEmbedding.PQTree.LABEL_FULL
                    If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                        Console.WriteLine("P1")
                    End If
                    Return True
                End If

                If isPertinentTree(S) Then
                    ' Template P2: (Figure 8)
                    If counts(OrthographicEmbedding.PQTree.LABEL_EMPTY) > 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_PARTIAL) = 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_FULL) > 0 Then
                        If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                            Console.WriteLine("P2")
                        End If
                        Dim P1 As OrthographicEmbedding.PQTree = New OrthographicEmbedding.PQTree(OrthographicEmbedding.PQTree.NO_INDEX, OrthographicEmbedding.PQTree.P_NODE, Nothing)
                        P1.label = OrthographicEmbedding.PQTree.LABEL_FULL
                        For Each child As OrthographicEmbedding.PQTree In children
                            If child.label = OrthographicEmbedding.PQTree.LABEL_FULL Then
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
                    If counts(OrthographicEmbedding.PQTree.LABEL_EMPTY) >= 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_PARTIAL) = 1 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_FULL) >= 0 Then
                        If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                            Console.WriteLine("P4")
                        End If
                        Dim partialChild As OrthographicEmbedding.PQTree = Nothing
                        For Each child As OrthographicEmbedding.PQTree In children
                            If child.label = OrthographicEmbedding.PQTree.LABEL_PARTIAL Then
                                partialChild = child
                                Exit For
                            End If
                        Next
                        Dim P1 As OrthographicEmbedding.PQTree = New OrthographicEmbedding.PQTree(OrthographicEmbedding.PQTree.NO_INDEX, OrthographicEmbedding.PQTree.P_NODE, Nothing)
                        P1.label = OrthographicEmbedding.PQTree.LABEL_FULL
                        For Each child As OrthographicEmbedding.PQTree In children
                            If child.label = OrthographicEmbedding.PQTree.LABEL_FULL Then
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
                    If counts(OrthographicEmbedding.PQTree.LABEL_EMPTY) >= 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_PARTIAL) = 2 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_FULL) >= 0 Then
                        If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                            Console.WriteLine("P6")
                        End If
                        Dim partial1 As OrthographicEmbedding.PQTree = Nothing
                        Dim partial2 As OrthographicEmbedding.PQTree = Nothing
                        Dim P1 As OrthographicEmbedding.PQTree = New OrthographicEmbedding.PQTree(OrthographicEmbedding.PQTree.NO_INDEX, OrthographicEmbedding.PQTree.P_NODE, Nothing)
                        Dim Q1 As OrthographicEmbedding.PQTree = New OrthographicEmbedding.PQTree(OrthographicEmbedding.PQTree.NO_INDEX, OrthographicEmbedding.PQTree.Q_NODE, Nothing)
                        P1.label = OrthographicEmbedding.PQTree.LABEL_FULL
                        Q1.label = OrthographicEmbedding.PQTree.LABEL_PARTIAL
                        For Each child As OrthographicEmbedding.PQTree In children
                            If child.label = OrthographicEmbedding.PQTree.LABEL_PARTIAL Then
                                If partial1 Is Nothing Then
                                    partial1 = child
                                Else
                                    partial2 = child
                                End If
                            End If
                            If child.label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                                P1.children.Add(child)
                                child.parent = P1
                            End If
                        Next
                        If partial1.fromEmptyToFullP() Then
                            '                    if (partial1.firstChildEmptyP()) {                    
                            '                    if (partial1.children.get(0).label==LABEL_EMPTY) {
                            ' they are in the right order:
                            For Each child2 As OrthographicEmbedding.PQTree In partial1.children
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
                            For Each child2 As OrthographicEmbedding.PQTree In partial2.children
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
                    If counts(OrthographicEmbedding.PQTree.LABEL_EMPTY) > 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_PARTIAL) = 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_FULL) > 0 Then
                        If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                            Console.WriteLine("P3")
                        End If
                        If counts(OrthographicEmbedding.PQTree.LABEL_EMPTY) > 1 Then
                            If counts(OrthographicEmbedding.PQTree.LABEL_FULL) > 1 Then
                                Dim Pnode1 As OrthographicEmbedding.PQTree = New OrthographicEmbedding.PQTree(OrthographicEmbedding.PQTree.NO_INDEX, OrthographicEmbedding.PQTree.P_NODE, Nothing)
                                Dim Pnode2 As OrthographicEmbedding.PQTree = New OrthographicEmbedding.PQTree(OrthographicEmbedding.PQTree.NO_INDEX, OrthographicEmbedding.PQTree.P_NODE, Nothing)
                                Pnode1.label = OrthographicEmbedding.PQTree.LABEL_EMPTY
                                Pnode2.label = OrthographicEmbedding.PQTree.LABEL_FULL
                                For Each child As OrthographicEmbedding.PQTree In children
                                    If child.label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
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
                                nodeType = OrthographicEmbedding.PQTree.Q_NODE
                                label = OrthographicEmbedding.PQTree.LABEL_PARTIAL
                            Else
                                Dim Pnode1 As OrthographicEmbedding.PQTree = New OrthographicEmbedding.PQTree(OrthographicEmbedding.PQTree.NO_INDEX, OrthographicEmbedding.PQTree.P_NODE, Nothing)
                                Pnode1.label = OrthographicEmbedding.PQTree.LABEL_EMPTY
                                For Each child As OrthographicEmbedding.PQTree In children
                                    If child.label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                                        Pnode1.children.Add(child)
                                        child.parent = Pnode1
                                    End If
                                Next

                                children.RemoveAll(Pnode1.children)
                                children.Add(Pnode1)
                                Pnode1.parent = Me
                                nodeType = OrthographicEmbedding.PQTree.Q_NODE
                                label = OrthographicEmbedding.PQTree.LABEL_PARTIAL
                            End If
                        Else
                            If counts(OrthographicEmbedding.PQTree.LABEL_FULL) > 1 Then
                                Dim Pnode2 As OrthographicEmbedding.PQTree = New OrthographicEmbedding.PQTree(OrthographicEmbedding.PQTree.NO_INDEX, OrthographicEmbedding.PQTree.P_NODE, Nothing)
                                Pnode2.label = OrthographicEmbedding.PQTree.LABEL_FULL
                                For Each child As OrthographicEmbedding.PQTree In children
                                    If child.label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                                        Pnode2.children.Add(child)
                                        child.parent = Pnode2
                                    End If
                                Next

                                children.RemoveAll(Pnode2.children)
                                children.Add(Pnode2)
                                Pnode2.parent = Me
                                nodeType = OrthographicEmbedding.PQTree.Q_NODE
                                label = OrthographicEmbedding.PQTree.LABEL_PARTIAL
                            Else
                                nodeType = OrthographicEmbedding.PQTree.Q_NODE
                                label = OrthographicEmbedding.PQTree.LABEL_PARTIAL
                            End If
                        End If
                        Return True
                    End If

                    ' Template P5: (Figure 12)
                    If counts(OrthographicEmbedding.PQTree.LABEL_EMPTY) >= 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_PARTIAL) = 1 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_FULL) >= 0 Then
                        If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                            Console.WriteLine("P5")
                        End If
                        Dim P1 As OrthographicEmbedding.PQTree = New OrthographicEmbedding.PQTree(OrthographicEmbedding.PQTree.NO_INDEX, OrthographicEmbedding.PQTree.P_NODE, Nothing)
                        Dim P2 As OrthographicEmbedding.PQTree = New OrthographicEmbedding.PQTree(OrthographicEmbedding.PQTree.NO_INDEX, OrthographicEmbedding.PQTree.P_NODE, Nothing)
                        P1.label = OrthographicEmbedding.PQTree.LABEL_EMPTY
                        P2.label = OrthographicEmbedding.PQTree.LABEL_FULL
                        Dim Q As OrthographicEmbedding.PQTree = Nothing
                        For Each child As OrthographicEmbedding.PQTree In children
                            If child.label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                                P1.children.Add(child)
                                child.parent = P1
                            End If
                            If child.label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                                P2.children.Add(child)
                                child.parent = P2
                            End If
                            If child.label = OrthographicEmbedding.PQTree.LABEL_PARTIAL Then
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
                            For Each child2 As OrthographicEmbedding.PQTree In Q.children
                                children.Add(child2)
                                child2.parent = Me
                            Next
                        ElseIf Q.fromEmptyToFullP() Then
                            ' we need to reverse them:
                            For i = Q.children.Count - 1 To 0 Step -1
                                Dim tmp As OrthographicEmbedding.PQTree = Q.children(i)
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
                        nodeType = OrthographicEmbedding.PQTree.Q_NODE
                        label = OrthographicEmbedding.PQTree.LABEL_PARTIAL
                        Return True
                    End If
                End If
            ElseIf nodeType = OrthographicEmbedding.PQTree.Q_NODE Then
                Dim counts = New Integer() {0, 0, 0}
                For Each child As OrthographicEmbedding.PQTree In children
                    If child.nodeType <> OrthographicEmbedding.PQTree.DIRECTION_INDICATOR Then
                        counts(child.label) += 1
                    End If
                Next

                ' Template Q0: 
                If counts(OrthographicEmbedding.PQTree.LABEL_EMPTY) > 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_PARTIAL) = 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_FULL) = 0 Then
                    If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                        Console.WriteLine("Q0")
                    End If
                    label = OrthographicEmbedding.PQTree.LABEL_EMPTY
                    Return True
                End If
                ' Template Q1: 
                If counts(OrthographicEmbedding.PQTree.LABEL_EMPTY) = 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_PARTIAL) = 0 AndAlso counts(OrthographicEmbedding.PQTree.LABEL_FULL) > 0 Then
                    If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                        Console.WriteLine("Q1")
                    End If
                    label = OrthographicEmbedding.PQTree.LABEL_FULL
                    Return True
                End If

                ' (Santi: Actually pattern Q2 is a special case of Q3, so I'll implement them together)            
                ' Template Q2: (Figure 14)
                ' If all the full leaves are either on the right or on the left
                ' Template Q3: (Figure 15):
                ' If all the full leaves ate groupped together in the middle, and empty ones are on the right AND left
                If counts(OrthographicEmbedding.PQTree.LABEL_PARTIAL) <= 2 Then
                    If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                        Console.WriteLine("Q2/Q3")
                    End If
                    If counts(OrthographicEmbedding.PQTree.LABEL_PARTIAL) = 0 AndAlso (counts(OrthographicEmbedding.PQTree.LABEL_EMPTY) > 0 OrElse counts(OrthographicEmbedding.PQTree.LABEL_FULL) > 0) Then
                        If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                            Console.WriteLine("Q2/Q3: 0 partials")
                        End If
                        ' check whether the chilren are in the right order
                        Dim goodEF = True ' Empty -> Full order
                        Dim goodFE = True ' Full -> Empty order
                        Dim previous = -1
                        For i = 0 To children.Count - 1
                            If children(i).nodeType = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR Then
                                Continue For
                            End If
                            If previous <> -1 Then
                                If children(previous).label = OrthographicEmbedding.PQTree.LABEL_FULL AndAlso children(i).label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                                    goodEF = False
                                End If
                                If children(previous).label = OrthographicEmbedding.PQTree.LABEL_EMPTY AndAlso children(i).label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                                    goodFE = False
                                End If
                            End If
                            previous = i
                        Next
                        If goodEF OrElse goodFE Then
                            label = OrthographicEmbedding.PQTree.LABEL_PARTIAL
                            Return True
                        End If
                        Return False
                    ElseIf counts(OrthographicEmbedding.PQTree.LABEL_PARTIAL) = 1 AndAlso (counts(OrthographicEmbedding.PQTree.LABEL_EMPTY) > 0 OrElse counts(OrthographicEmbedding.PQTree.LABEL_FULL) > 0) Then
                        If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                            Console.WriteLine("Q2/Q3: 1 partials")
                        End If
                        ' 1 partial Q-node:
                        Dim newChildrenEF As IList(Of OrthographicEmbedding.PQTree) = New List(Of OrthographicEmbedding.PQTree)()
                        Dim newChildrenFE As IList(Of OrthographicEmbedding.PQTree) = New List(Of OrthographicEmbedding.PQTree)()
                        Dim reverseIfEF As IList(Of OrthographicEmbedding.PQTree) = New List(Of OrthographicEmbedding.PQTree)()
                        Dim reverseIfFE As IList(Of OrthographicEmbedding.PQTree) = New List(Of OrthographicEmbedding.PQTree)()
                        Dim EFstate = 0 ' 0 : empty, 1: full
                        Dim FEstate = 0 ' 0 : full, 1: empty
                        For Each child As OrthographicEmbedding.PQTree In children
                            If child.nodeType = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR Then
                                newChildrenEF.Add(child)
                                newChildrenFE.Add(child)
                                Continue For
                            End If
                            If EFstate = 0 Then
                                If child.label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                                    newChildrenEF.Add(child)
                                ElseIf child.label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                                    newChildrenEF.Add(child)
                                    EFstate = 1
                                ElseIf child.label = OrthographicEmbedding.PQTree.LABEL_PARTIAL Then
                                    If child.fromEmptyToFullP() Then
                                        '                                if (child.firstChildEmptyP()) {
                                        '                                if (child.children.get(0).label==LABEL_EMPTY) {
                                        For Each child2 As OrthographicEmbedding.PQTree In child.children
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
                                If child.label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                                    EFstate = 2
                                ElseIf child.label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                                    newChildrenEF.Add(child)
                                ElseIf child.label = OrthographicEmbedding.PQTree.LABEL_PARTIAL Then
                                    EFstate = 2
                                End If
                            End If
                            If FEstate = 0 Then
                                If child.label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                                    newChildrenFE.Add(child)
                                ElseIf child.label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                                    newChildrenFE.Add(child)
                                    FEstate = 1
                                ElseIf child.label = OrthographicEmbedding.PQTree.LABEL_PARTIAL Then
                                    If child.fromFullToEmptyP() Then
                                        '                                if (child.firstChildFullP()) {
                                        '                                if (child.children.get(0).label==LABEL_FULL) {
                                        For Each child2 As OrthographicEmbedding.PQTree In child.children
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
                                If child.label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                                    FEstate = 2
                                ElseIf child.label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                                    newChildrenFE.Add(child)
                                ElseIf child.label = OrthographicEmbedding.PQTree.LABEL_PARTIAL Then
                                    FEstate = 2
                                End If
                            End If
                        Next
                        If EFstate = 1 Then
                            children.Clear()
                            label = OrthographicEmbedding.PQTree.LABEL_PARTIAL
                            For Each child As OrthographicEmbedding.PQTree In newChildrenEF
                                children.Add(child)
                                child.parent = Me
                            Next
                            For Each child As OrthographicEmbedding.PQTree In reverseIfEF
                                child.reverse()
                            Next
                            Return True
                        ElseIf FEstate = 1 Then
                            children.Clear()
                            label = OrthographicEmbedding.PQTree.LABEL_PARTIAL
                            For Each child As OrthographicEmbedding.PQTree In newChildrenFE
                                children.Add(child)
                                child.parent = Me
                            Next
                            For Each child As OrthographicEmbedding.PQTree In reverseIfFE
                                child.reverse()
                            Next
                            Return True
                        End If
                        Return False
                    Else
                        ' 2 partial Q-nodes:
                        If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                            Console.WriteLine("Q2/Q3: 2 partials")
                        End If
                        Dim newChildren As IList(Of OrthographicEmbedding.PQTree) = New List(Of OrthographicEmbedding.PQTree)()
                        Dim status = 0 ' 0 = empty, 1 = full, 2 = empty again, 3: error
                        For Each child As OrthographicEmbedding.PQTree In children
                            If child.nodeType = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR Then
                                newChildren.Add(child)
                                Continue For
                            End If
                            Select Case status
                                Case 0
                                    Select Case child.label
                                        Case OrthographicEmbedding.PQTree.LABEL_EMPTY
                                            newChildren.Add(child)
                                        Case OrthographicEmbedding.PQTree.LABEL_PARTIAL
                                            If child.fromEmptyToFullP() Then
                                                '                                if (child.firstChildEmptyP()) {                                
                                                '                                if (child.children.get(0).label==LABEL_EMPTY) {
                                                ' thye are in the right order:
                                                For Each child2 As OrthographicEmbedding.PQTree In child.children
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
                                        Case OrthographicEmbedding.PQTree.LABEL_FULL
                                            newChildren.Add(child)
                                            status = 1
                                    End Select
                                Case 1
                                    Select Case child.label
                                        Case OrthographicEmbedding.PQTree.LABEL_EMPTY
                                            newChildren.Add(child)
                                            status = 2
                                        Case OrthographicEmbedding.PQTree.LABEL_PARTIAL
                                            If child.fromFullToEmptyP() Then
                                                '                                if (child.firstChildFullP()) {
                                                '                                if (child.children.get(0).label==LABEL_FULL) {
                                                ' thye are in the right order:
                                                For Each child2 As OrthographicEmbedding.PQTree In child.children
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
                                        Case OrthographicEmbedding.PQTree.LABEL_FULL
                                            newChildren.Add(child)
                                    End Select
                                Case 2
                                    Select Case child.label
                                        Case OrthographicEmbedding.PQTree.LABEL_EMPTY
                                            newChildren.Add(child)
                                        Case OrthographicEmbedding.PQTree.LABEL_PARTIAL
                                            Return False
                                        Case OrthographicEmbedding.PQTree.LABEL_FULL
                                            Return False
                                    End Select
                            End Select
                        Next
                        children.Clear()
                        For Each child As OrthographicEmbedding.PQTree In newChildren
                            children.Add(child)
                            child.parent = Me
                        Next
                        label = OrthographicEmbedding.PQTree.LABEL_PARTIAL
                        Return True
                    End If
                End If

            End If

            Return False
        End Function

        Public Overridable Function reduce(index As Integer) As Boolean
            Dim tmp As IList(Of OrthographicEmbedding.PQTree) = New List(Of OrthographicEmbedding.PQTree)()
            Dim S As IList(Of OrthographicEmbedding.PQTree) = New List(Of OrthographicEmbedding.PQTree)()
            getLeaves(tmp)
            For Each leaf As OrthographicEmbedding.PQTree In tmp
                If leaf.nodeIndex = index Then
                    S.Add(leaf)
                End If
            Next

            Return reduce(S)
        End Function

        Public Overridable Function reduce(S As IList(Of OrthographicEmbedding.PQTree)) As Boolean
            Dim processed As ISet(Of OrthographicEmbedding.PQTree) = New HashSet(Of OrthographicEmbedding.PQTree)()
            Dim queue As List(Of OrthographicEmbedding.PQTree) = New List(Of OrthographicEmbedding.PQTree)()

            ' add all the leaves of the tree to the queue
            getLeaves(queue)

            If OrthographicEmbedding.PQTree.DEBUG >= 2 Then
                Console.WriteLine("PQTree.reduce: T = " & ToString())
            End If
            If OrthographicEmbedding.PQTree.DEBUG >= 2 Then
                Console.WriteLine("PQTree.reduce: S = " & S.ToString())
            End If

            clearLabels()

            While queue.Count > 0
                Dim X As OrthographicEmbedding.PQTree = queue.PopAt(0)
                If OrthographicEmbedding.PQTree.DEBUG >= 2 Then
                    Console.WriteLine("PQTree.reduce: processing " & X.ToString())
                End If
                If Not X.applyTemplate(S) Then
                    If OrthographicEmbedding.PQTree.DEBUG >= 2 Then
                        Console.WriteLine("PQTree.reduce: no pattern mattched! FAILURE!")
                    End If
                    Return False
                End If
                processed.Add(X)
                If OrthographicEmbedding.PQTree.DEBUG >= 2 Then
                    Console.WriteLine("PQTree.reduce: processed " & X.ToString())
                End If
                If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                    sanityTest()
                End If

                ' If all the nodes in "S" are in node descendants of X, then we are done:
                If X.isPertinentTree(S) Then
                    If OrthographicEmbedding.PQTree.DEBUG >= 2 Then
                        Console.WriteLine("PQTree.reduce: the last node contained all of S, SUCCESS!")
                    End If
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
            For Each child As OrthographicEmbedding.PQTree In children
                If child.nodeType = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR Then
                    Continue For
                End If
                Select Case state
                    Case 0
                        If child.label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                            state = 1
                        ElseIf child.label = OrthographicEmbedding.PQTree.LABEL_PARTIAL Then
                            If Not child.fromEmptyToFullP() Then
                                Return False
                            End If
                        End If
                    Case 1
                        If child.label <> OrthographicEmbedding.PQTree.LABEL_FULL Then
                            Return False
                        End If
                End Select
            Next
            Return True
        End Function


        Public Overridable Function fromFullToEmptyP() As Boolean
            Dim state = 0
            For Each child As OrthographicEmbedding.PQTree In children
                If child.nodeType = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR Then
                    Continue For
                End If
                Select Case state
                    Case 0
                        If child.label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                            state = 1
                        ElseIf child.label = OrthographicEmbedding.PQTree.LABEL_PARTIAL Then
                            If Not child.fromFullToEmptyP() Then
                                Return False
                            End If
                        End If
                    Case 1
                        If child.label <> OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                            Return False
                        End If
                End Select
            Next
            Return True
        End Function

        Public Overridable Sub reverse()
            If nodeType = OrthographicEmbedding.PQTree.Q_NODE Then
                Dim tmp As IList(Of OrthographicEmbedding.PQTree) = New List(Of OrthographicEmbedding.PQTree)()
                CType(tmp, List(Of OrthographicEmbedding.PQTree)).AddRange(children)
                children.Clear()

                For Each child As OrthographicEmbedding.PQTree In tmp
                    child.reverse()
                    children.Insert(0, child)
                Next
            ElseIf nodeType = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR Then
                If nodeType = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR Then
                    If OrthographicEmbedding.PQTree.DEBUG >= 1 Then
                        Console.WriteLine("Reversing direction indicator: " & ToString())
                    End If
                    If direction = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR_LEFT Then
                        direction = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR_RIGHT
                    Else
                        direction = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR_LEFT

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

        Public Overridable Function recursivelyRemoveLeaf(leaf As OrthographicEmbedding.PQTree) As Boolean
            If children IsNot Nothing Then
                If children.Remove(leaf) Then
                    Return True
                End If
                For Each child As OrthographicEmbedding.PQTree In children
                    If child.recursivelyRemoveLeaf(leaf) Then
                        Return True
                    End If
                Next
            End If
            Return False
        End Function


        Public Overridable Function recursivelyRemoveLeafNotThroughQNodes(leaf As OrthographicEmbedding.PQTree) As Boolean
            If children IsNot Nothing Then
                If children.Remove(leaf) Then
                    Return True
                End If
                For Each child As OrthographicEmbedding.PQTree In children
                    If child.nodeType <> OrthographicEmbedding.PQTree.Q_NODE AndAlso child.recursivelyRemoveLeafNotThroughQNodes(leaf) Then
                        Return True
                    End If
                Next
            End If
            Return False
        End Function

        Public Overridable Function contains(node As OrthographicEmbedding.PQTree) As Boolean
            If Me Is node Then
                Return True
            End If
            If children IsNot Nothing Then
                For Each child As OrthographicEmbedding.PQTree In children
                    If child.contains(node) Then
                        Return True
                    End If
                Next
            End If
            Return False
        End Function



        Public Overridable Function sanityTest() As Boolean
            Dim nodes As IList(Of OrthographicEmbedding.PQTree) = allNodes()
            For Each node As OrthographicEmbedding.PQTree In nodes
                If node.parent IsNot Nothing Then
                    If Not node.parent.children.Contains(node) Then
                        Console.WriteLine("sanityTest: Inconsistent parent in node: " & node.ToString())
                        Return False
                    End If
                    If node.children IsNot Nothing Then
                        For Each node2 As OrthographicEmbedding.PQTree In node.children
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

        Public Overridable Function toString(nodeParent As Dictionary(Of OrthographicEmbedding.PQTree, Integer)) As String
            Return toString(0, nodeParent)
        End Function

        Friend Overridable Function indentString(indents As Integer) As String
            Dim out = ""
            For i = 0 To indents - 1
                out += "  "
            Next
            Return out
        End Function


        Public Overridable Function toString(indents As Integer, nodeParent As Dictionary(Of OrthographicEmbedding.PQTree, Integer)) As String
            If nodeType = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR Then
                If direction = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR_LEFT Then
                    Return indentString(indents) & "direction-indicator-left(" & nodeIndex.ToString() & ")"
                End If
                If direction = OrthographicEmbedding.PQTree.DIRECTION_INDICATOR_RIGHT Then
                    Return indentString(indents) & "direction-indicator-right(" & nodeIndex.ToString() & ")"
                End If
                Return indentString(indents) & "direction-indicator-???(" & nodeIndex.ToString() & ")"
            ElseIf nodeType = OrthographicEmbedding.PQTree.LEAF_NODE Then
                If nodeParent IsNot Nothing Then
                    If label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                        Return indentString(indents) & "full-leaf(" & nodeIndex.ToString() & " from " & nodeParent(Me).ToString() & ")"
                    End If
                    If label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                        Return indentString(indents) & "empty-leaf(" & nodeIndex.ToString() & " from " & nodeParent(Me).ToString() & ")"
                    End If
                    Return indentString(indents) & "leaf(" & nodeIndex.ToString() & " from " & nodeParent(Me).ToString() & ")"
                Else
                    If label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                        Return indentString(indents) & "full-leaf(" & nodeIndex.ToString() & ")"
                    End If
                    If label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                        Return indentString(indents) & "empty-leaf(" & nodeIndex.ToString() & ")"
                    End If
                    Return indentString(indents) & "leaf(" & nodeIndex.ToString() & ")"
                End If
            ElseIf nodeType = OrthographicEmbedding.PQTree.P_NODE Then
                Dim tmp As String = indentString(indents) & "P-node(" & nodeIndex.ToString() & ")(" & vbLf
                If label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                    tmp = indentString(indents) & "full-P-node(" & nodeIndex.ToString() & ")(" & vbLf
                End If
                If label = OrthographicEmbedding.PQTree.LABEL_PARTIAL Then
                    tmp = indentString(indents) & "partial-P-node(" & nodeIndex.ToString() & ")(" & vbLf
                End If
                If label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                    tmp = indentString(indents) & "empty-P-node(" & nodeIndex.ToString() & ")(" & vbLf
                End If
                Dim first = True
                For Each child As OrthographicEmbedding.PQTree In children
                    If Not first Then
                        tmp += "," & vbLf
                    End If
                    tmp += child.toString(indents + 1, nodeParent)
                    first = False
                Next
                Return tmp & ")"
            Else
                Dim tmp = indentString(indents) & "Q-Node(" & vbLf
                If label = OrthographicEmbedding.PQTree.LABEL_FULL Then
                    tmp = indentString(indents) & "full-Q-node(" & nodeIndex.ToString() & ")(" & vbLf
                End If
                If label = OrthographicEmbedding.PQTree.LABEL_PARTIAL Then
                    tmp = indentString(indents) & "partial-Q-node(" & nodeIndex.ToString() & ")(" & vbLf
                End If
                If label = OrthographicEmbedding.PQTree.LABEL_EMPTY Then
                    tmp = indentString(indents) & "empty-Q-node(" & nodeIndex.ToString() & ")(" & vbLf
                End If
                Dim first = True
                For Each child As OrthographicEmbedding.PQTree In children
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

End Namespace
