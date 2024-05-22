#Region "Microsoft.VisualBasic::3a378216e913cee22071a79ceb0dddb7, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\Nudge\CloudOfTextRectangle.vb"

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

    '   Total Lines: 260
    '    Code Lines: 154 (59.23%)
    ' Comment Lines: 71 (27.31%)
    '    - Xml Docs: 77.46%
    ' 
    '   Blank Lines: 35 (13.46%)
    '     File Size: 10.79 KB


    '     Class CloudOfTextRectangle
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: arrange_text, conflicts_with, get_conflicts, get_tree_leaves, new_config_cloud
    '                   ToString, treat_conflicts
    ' 
    '         Sub: add_label, moveArrows, remove_label
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace Drawing2D.Text.Nudge

    ''' <summary>
    ''' class to represent an esemble of TextRectangle object. I.e in the
    ''' pyplot context, a cloud Of text annotation To attached point. Goal Is To 
    ''' arrange the cloud Of point In order To have the least conflicts between 
    ''' ext (i.e Rectangles). 
    '''
    ''' So we will define a method To Get And count the conflicts, an other To
    ''' arrange the cloud Of text rectangle In all possible ways With the "error"
    ''' Function (number Of conflicts) To minimize
    ''' </summary>
    Public Class CloudOfTextRectangle

        ''' <summary>
        ''' a collection of the text labels
        ''' </summary>
        Friend ReadOnly list_tr As New List(Of TextRectangle)
        ''' <summary>
        ''' <see cref="list_tr"/> element index which there 
        ''' text rectangle is conflicts with each other
        ''' </summary>
        Friend ReadOnly conflicts As New List(Of ConflictIndexTuple)

        Sub New(list_of_tr As IEnumerable(Of TextRectangle))
            Me.list_tr = New List(Of TextRectangle)(list_of_tr)
            Me.conflicts = Nothing

            Call get_conflicts()
        End Sub

        Sub New()
        End Sub

        Public Function conflicts_with(text As TextRectangle) As TextRectangle
            For Each tuple As ConflictIndexTuple In conflicts
                If list_tr(tuple.i) Is text Then
                    Return list_tr(tuple.j)
                End If
                If list_tr(tuple.j) Is text Then
                    Return list_tr(tuple.i)
                End If
            Next

            Return Nothing
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub add_label(label As TextRectangle)
            Call list_tr.Add(label)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub remove_label(label As TextRectangle)
            Call list_tr.Remove(label)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{list_tr.Count} text labels with {conflicts.Count} overlap conflicts!"
        End Function

        ''' <summary>
        ''' function that compute the conflicts associated to a cloud of
		''' text rectangles.
		''' output: list of tuple. tuple Or text rectangle's index involved
		''' in conflicts. So conflicts are modelised as pairs.
        ''' </summary>
        ''' <returns></returns>
        Public Function get_conflicts() As Integer
            Dim conflicts As New List(Of ConflictIndexTuple)

            For i As Integer = 0 To list_tr.Count - 2
                For j As Integer = i + 1 To list_tr.Count - 1
                    If list_tr(i).r.covers_rectangle(list_tr(j).r) Then
                        Call conflicts.Add(New ConflictIndexTuple With {.i = i, .j = j})
                    End If
                Next
            Next

            Me.conflicts.Clear()
            Me.conflicts.AddRange(conflicts)

            Return conflicts.Count
        End Function

        ''' <summary>
        ''' function to change the state of i-ème text rectangle in the
		''' cloud of text rectangle.
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="state"></param>
        ''' <returns></returns>
        Public Function new_config_cloud(index As Integer, state As States) As CloudOfTextRectangle
            Dim new_config As TextRectangle() = list_tr _
                .Select(Function(txt) txt.deepCopy) _
                .ToArray

            Call new_config(index).change_state(state)

            Return New CloudOfTextRectangle(new_config)
        End Function

        ''' <summary>
        ''' resolution of the conflicts is recursive. First, we try
        ''' to resolve the first conflict of the cloud of text rectangle.
        '''
        ''' first resolution's try gives two list : a list of cloud with
        ''' new configuration (the textRectangle has a different state's
        ''' configuration) and without the first conflict (i.e the confluct 
        ''' resolved), and a list of cloud with new new configuration and new 
        ''' conflict. The we recursely do this resolution on the two different
        ''' kind of clouds we get. The stop criteria is no conflict, or if we 
        ''' have previously explore all the conflict/configuration situation
        ''' before.
        ''' </summary>
        ''' <param name="parent_nodes_conflicts">
        ''' empty list. Needed for recursion.
        ''' </param>
        ''' <param name="cpt"></param>
        ''' <returns>
        ''' a tree of resolved conflicts in the form of
        ''' recursively nested dictionary.
        ''' </returns>
        Public Function treat_conflicts(parent_nodes_conflicts As List(Of CloudOfTextRectangle),
                                        cpt As Integer,
                                        moveAll As Boolean) As ResolvedTree
            ' check input type	
            Dim n_conflict = conflicts.Count

            ' stop condition to recursion --> no conflict in the cloud
            If n_conflict = 0 OrElse cpt > 3 Then
                Return New ResolvedTree With {.parent = Me, .childrens = Nothing}
            Else
                Call parent_nodes_conflicts.Add(Me)
            End If

            Dim n_min As Integer = (From c In parent_nodes_conflicts Select c.conflicts.Count).Min
            'print("parent_nodes_conflicts:")
            'print(parent_nodes_conflicts)
            Dim configs As New List(Of CloudOfTextRectangle)

            Static stateList As States() = {
                States.top_right,
                States.right_bottom,
                States.left_bottom,
                States.top_left
            }

            ' update the label position of each conflicts 
            If moveAll Then
                For Each first_conflict As ConflictIndexTuple In conflicts
                    For Each s As States In stateList
                        Call configs.Add(new_config_cloud(first_conflict.i, s))
                        Call configs.Add(new_config_cloud(first_conflict.j, s))
                    Next
                Next
            Else
                ' or just move first
                Dim first_conflict As ConflictIndexTuple = conflicts(Scan0)

                For Each s As States In stateList
                    Call configs.Add(new_config_cloud(first_conflict.i, s))
                    Call configs.Add(new_config_cloud(first_conflict.j, s))
                Next
            End If

            Dim new_configs_better As CloudOfTextRectangle() =
                (From c In configs Where parent_nodes_conflicts.IndexOf(c) = -1 AndAlso c.conflicts.Count < n_min).ToArray
            Dim new_configs_even As CloudOfTextRectangle() =
                (From c In configs Where parent_nodes_conflicts.IndexOf(c) = -1 AndAlso c.conflicts.Count = n_min).ToArray
            ' size limitation to four childrens
            Dim nsize As Integer = stdNum.Max(0, 4 - new_configs_better.Length)
            Dim new_configs = new_configs_better.JoinIterates(new_configs_even.Take(nsize)).ToArray
            'print("new_config")
            'print([c.conflicts for c in new_configs])
            ' second stop condition : no more config not explored and
            ' no config found with new conflict to treat
            If new_configs.Length = 0 Then
                Return New ResolvedTree With {.parent = Me, .childrens = Nothing}
            End If
            ' compteur qui compte les tentatives "infructeurses completes de trouver
            ' un sous chemin meilleurs. L'objectif est d'éviter les boucles trop goourmandes 
            ' en calcul
            If (From c In new_configs
                Let t = n_conflict = c.conflicts.Count
                Select If(t, 1, 0)).Sum = new_configs.Length Then

                cpt += 1
            Else
                cpt = 0
            End If

            Dim childrens = From c As CloudOfTextRectangle
                            In new_configs
                            Select c.treat_conflicts(parent_nodes_conflicts, cpt, moveAll)

            Return New ResolvedTree With {.parent = Me, .childrens = childrens.ToArray}
        End Function

        Private Sub moveArrows()
            For Each j In list_tr.Select(Function(tr, i) (i, tr))
                Dim i = j.i
                Dim tr = j.tr

                If tr.r.x1(0) >= 0 And tr.r.x1(1) >= 0 Then
                    Continue For
                End If
                If tr.r.x1(0) < 0 And tr.r.x1(1) >= 0 Then
                    list_tr(i).change_state(2)
                End If
                If tr.r.x1(0) >= 0 And tr.r.x1(1) < 0 Then
                    list_tr(i).change_state(3)
                End If
                If tr.r.x1(0) < 0 And tr.r.x1(1) < 0 Then
                    list_tr(i).change_state(4)
                End If
            Next
        End Sub

        ''' <summary>
        ''' main Function To arrange texts Using treat_conflicts result	
        ''' </summary>
        ''' <param name="arrows"></param>
        ''' <returns></returns>
        Public Function arrange_text(Optional arrows As Boolean = False,
                                     Optional moveAll As Boolean = False) As Integer
            If arrows Then
                Call moveArrows()
            End If

            Dim parent_asserts As New List(Of CloudOfTextRectangle)
            Dim resolve_conflicts_tree As ResolvedTree = treat_conflicts(parent_asserts, cpt:=0, moveAll:=moveAll)
            Dim tree_leaves = get_tree_leaves(resolve_conflicts_tree)
            Dim sorted_leaves = tree_leaves _
                .OrderBy(Function(x) x.conflicts.Count) _
                .ToArray

            Call list_tr.Clear()
            Call list_tr.AddRange(sorted_leaves(0).list_tr)

            Return get_conflicts()
        End Function

        Private Function get_tree_leaves(tree As ResolvedTree) As CloudOfTextRectangle()
            If tree.childrens Is Nothing Then
                Return {tree.parent}
            Else
                Return (From c As ResolvedTree
                        In tree.childrens
                        Select get_tree_leaves(c)) _
                    .IteratesALL _
                    .ToArray
            End If
        End Function
    End Class
End Namespace
