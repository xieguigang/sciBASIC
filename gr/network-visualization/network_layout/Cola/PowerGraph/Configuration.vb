#Region "Microsoft.VisualBasic::03eba2581c6a33baa60080026943ebe0, gr\network-visualization\network_layout\Cola\PowerGraph\Configuration.vb"

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

    '   Total Lines: 222
    '    Code Lines: 166 (74.77%)
    ' Comment Lines: 24 (10.81%)
    '    - Xml Docs: 79.17%
    ' 
    '   Blank Lines: 32 (14.41%)
    '     File Size: 9.40 KB


    '     Class Configuration
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: allEdges, getGroupHierarchy, greedyMerge, initModulesFromGroup, merge
    '                   nEdges, rootMerges, updateLambda
    ' 
    '         Sub: getEdges
    '         Class ModuleMerge
    ' 
    '             Properties: a, b, id, nEdges
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Cola

    Public Class Configuration(Of Link)

        ''' <summary>
        ''' canonical list of modules.
        ''' Initialized to a module for each leaf node, such that the ids and indexes 
        ''' of the module in the array match the indexes of the nodes in links Modules 
        ''' created through merges are appended to the end of this.
        ''' </summary>
        Private modules As List(Of [Module])

        ''' <summary>
        ''' top level modules and candidates for merges
        ''' </summary>
        Private roots As List(Of ModuleSet)

        ''' <summary>
        ''' remaining edge count 
        ''' </summary>
        Private R As Double

        Public Sub New(n As Double, edges As Link(), linkAccessor As LinkTypeAccessor(Of Link), rootGroup As Node)
            Me.modules = New List(Of [Module])
            Me.roots = New List(Of ModuleSet)

            If Not rootGroup Is Nothing Then
                Me.initModulesFromGroup(rootGroup)
            Else
                Me.roots.Add(New ModuleSet())
                For i As Integer = 0 To n - 1
                    Me.modules.Add(New [Module](i))
                    Me.roots(0).add(Me.modules(i))
                Next
            End If
            Me.R = edges.Length
            edges.DoEach(Sub(e)
                             Dim s = Me.modules(linkAccessor.getSourceIndex(e))
                             Dim t = Me.modules(linkAccessor.getTargetIndex(e))
                             Dim type = linkAccessor.[GetLinkType](e)

                             s.outgoing.add(type, t)
                             t.incoming.add(type, s)
                         End Sub)
        End Sub

        Private Function initModulesFromGroup(group As Node) As ModuleSet
            Dim moduleSet = New ModuleSet()
            Me.roots.Add(moduleSet)
            For i As Integer = 0 To group.leaves.Count - 1
                Dim node As Node = group.leaves(i)
                Dim [module] = New [Module](node.id)
                Me.modules(node.id) = [module]
                moduleSet.add([module])
            Next
            If Not group.groups.IsNullOrEmpty Then
                For j As Integer = 0 To group.groups.Count - 1
                    Dim child = group.groups(j)
                    ' Propagate group properties (like padding, stiffness, ...) as module definition so that the generated power graph group will inherit it
                    Dim definition = New Dictionary(Of String, Object)

                    For Each prop As String In child.VB
                        If prop <> "leaves" AndAlso prop <> "groups" Then
                            definition(prop) = child.VB(prop)
                        End If
                    Next

                    ' Use negative module id to avoid clashes between predefined and generated modules
                    moduleSet.add(New [Module](-1 - j, New LinkSets(), New LinkSets(), Me.initModulesFromGroup(child), definition))
                Next
            End If
            Return moduleSet
        End Function

        Private Function updateLambda(a As [Module], b As [Module], m As [Module]) As Action(Of LinkSets, String, String)
            Return Sub(s As LinkSets, i As String, o As String)
                       Call s.forAll(Sub(ms, linktype)
                                         Call ms.forAll(Sub(n)
                                                            Dim nls = n(i)
                                                            nls.add(linktype, m)
                                                            nls.remove(linktype, a)
                                                            nls.remove(linktype, b)
                                                            Call a(o).remove(linktype, n)
                                                            Call b(o).remove(linktype, n)
                                                        End Sub)
                                     End Sub)
                   End Sub
        End Function

        ''' <summary>
        ''' merge modules a and b keeping track of their power edges and removing the from roots
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="k"></param>
        ''' <returns></returns>
        Public Function merge(a As [Module], b As [Module], Optional k As Integer = 0) As [Module]
            Dim inInt = a.incoming.intersection(b.incoming)
            Dim outInt = a.outgoing.intersection(b.outgoing)
            Dim children As New ModuleSet()

            Call children.add(a)
            Call children.add(b)

            Dim m = New [Module](Me.modules.Count, outInt, inInt, children)
            Me.modules.Add(m)
            Dim update = updateLambda(a, b, m)
            update(outInt, "incoming", "outgoing")
            update(inInt, "outgoing", "incoming")
            Me.R -= inInt.count() + outInt.count()
            Me.roots(k).remove(a)
            Me.roots(k).remove(b)
            Me.roots(k).add(m)
            Return m
        End Function

        Private Function rootMerges(Optional k As Integer = 0) As ModuleMerge()
            Dim rs = Me.roots(index:=k).modules()
            Dim n = rs.Length
            Dim merges = New ModuleMerge(n * (n - 1)) {}
            Dim ctr = 0
            Dim i As Integer = 0, ends As Integer = n - 1

            While i < ends
                For j As Integer = i + 1 To n - 1
                    Dim a = rs(i)
                    Dim b = rs(j)

                    merges(ctr) = New ModuleMerge With {
                        .id = ctr,
                        .nEdges = Me.nEdges(a, b),
                        .a = a,
                        .b = b
                    }
                    ctr += 1
                Next

                i += 1
            End While

            Return merges.Where(Function(m) Not m Is Nothing).ToArray
        End Function

        Public Class ModuleMerge
            Public Property id As Integer
            Public Property nEdges As Integer
            Public Property a As [Module]
            Public Property b As [Module]
        End Class

        Public Function greedyMerge() As Boolean
            For i As Integer = 0 To Me.roots.Count - 1
                ' Handle single nested module case
                If Me.roots(i).modules().Length < 2 Then
                    Continue For
                End If

                ' find the merge that allows for the most edges to be removed.  
                ' secondary ordering based on arbitrary id (for predictability)
                Dim ms = Me.rootMerges(i).Sort(Function(a, b) If(a.nEdges = b.nEdges, a.id - b.id, a.nEdges - b.nEdges))
                Dim m = ms(0)

                If m.nEdges >= Me.R Then
                    Continue For
                Else
                    Call Me.merge(m.a, m.b, i)
                    Return True
                End If
            Next

            Return False
        End Function

        Private Function nEdges(a As [Module], b As [Module]) As Integer
            Dim inInt = a.incoming.intersection(b.incoming)
            Dim outInt = a.outgoing.intersection(b.outgoing)
            Return Me.R - inInt.count() - outInt.count()
        End Function

        Public Function getGroupHierarchy(retargetedEdges As List(Of PowerEdge(Of [Variant](Of Integer, Node)))) As List(Of Node)
            Dim groups As New List(Of Node)
            Dim root As New Node

            Call toGroups(Me.roots(0), root, groups)
            Call Me.allEdges() _
                .DoEach(Sub(e)
                            Dim a = Me.modules(e.source)
                            Dim b = Me.modules(e.target)
                            Dim pop = Function(x As [Module]) As [Variant](Of Integer, Node)
                                          If x.gid Is Nothing Then
                                              Return e.source
                                          Else
                                              Return groups(x.gid)
                                          End If
                                      End Function
                            Dim from = pop(a)
                            Dim [to] = pop(b)
                            Dim pe As New PowerEdge(Of [Variant](Of Integer, Node))(from, [to], e.type)

                            retargetedEdges.Add(pe)
                        End Sub)

            Return groups.AsList
        End Function

        Private Function allEdges() As List(Of PowerEdge(Of Integer))
            Dim es As New List(Of PowerEdge(Of Integer))
            Configuration(Of Link).getEdges(Me.roots(0), es)
            Return es
        End Function

        Shared Sub getEdges(modules As ModuleSet, es As List(Of PowerEdge(Of Integer)))
            modules.forAll(Sub(m)
                               Call m.getEdges(es)
                               Call Configuration(Of Link).getEdges(m.children, es)
                           End Sub)
        End Sub
    End Class
End Namespace
