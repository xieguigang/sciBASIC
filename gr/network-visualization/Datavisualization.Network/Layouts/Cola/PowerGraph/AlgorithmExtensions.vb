#Region "Microsoft.VisualBasic::643703879ca86daccdaac357a5b51734, gr\network-visualization\Datavisualization.Network\Layouts\Cola\PowerGraph\AlgorithmExtensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module powergraphExtensions
    ' 
    '         Function: getGroups, intersection
    ' 
    '         Sub: (+2 Overloads) toGroups
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Layouts.Cola

    Module powergraphExtensions

        <Extension>
        Private Sub toGroups(m As [Module], group As Node, groups As List(Of Node))
            If m.isLeaf Then
                If group.leaves Is Nothing Then
                    group.leaves = New List(Of [Variant](Of Integer, Node))
                End If

                group.leaves.Add(m.id)
            Else
                Dim g = group
                m.gid = groups.Count

                If (Not m.isIsland OrElse m.isPredefined) Then
                    g = New Node With {.id = m.gid}

                    If m.isPredefined Then
                        For Each prop As String In m.definition.Keys
                            g(prop) = m.definition(prop)
                        Next
                    End If
                    If group.groups Is Nothing Then
                        group.groups = New List(Of [Variant](Of Integer, Node))
                    End If

                    group.groups.Add(m.gid)
                    groups.Add(g)
                End If

                toGroups(m.children, g, groups)
            End If
        End Sub

        Public Sub toGroups(modules As ModuleSet, group As Node, groups As List(Of Node))
            Call modules.forAll(Sub(m As [Module]) m.toGroups(group, groups))
        End Sub

        Public Function intersection(Of T)(m As Dictionary(Of String, T), n As Dictionary(Of String, T)) As Dictionary(Of String, T)
            Dim i As New Dictionary(Of String, T)

            For Each v As String In m.Keys
                If n.ContainsKey(v) Then
                    i(v) = m(v)
                End If
            Next

            Return i
        End Function

        Public Function getGroups(Of Link)(nodes As Node(), links As Link(), la As LinkTypeAccessor(Of Link), rootGroup As Node) As PowerGraph
            Dim n As Integer = nodes.Length
            Dim c As New Configuration(Of Link)(n, links, la, rootGroup)

            While c.greedyMerge()
            End While

            Dim powerEdgeIndices As New List(Of PowerEdge(Of [Variant](Of Integer, Node)))
            Dim powerEdges As New List(Of PowerEdge(Of Node))
            Dim g As List(Of Node) = c.getGroupHierarchy(powerEdgeIndices)

            powerEdgeIndices.DoEach(Sub(e)
                                        ' javascript之中，对象类型在这里发生了转换
                                        ' 将index转换为具体的node对象
                                        Dim f = Sub([end] As String)
                                                    Dim eg As [Variant](Of Integer, Node) = e([end])

                                                    If eg Like GetType(Integer) Then
                                                        e([end]) = nodes.ElementAtOrDefault(CType(eg, Integer))
                                                    End If
                                                End Sub

                                        Call f("source")
                                        Call f("target")
                                    End Sub)

            Return New PowerGraph With {
                .groups = g,
                .powerEdges = powerEdges
            }
        End Function

    End Module
End Namespace
