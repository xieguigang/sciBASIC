#Region "Microsoft.VisualBasic::4c18b9ce8f18730eb01804ee15fb80a6, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\HierarchyBuilder\DistanceMap.vb"

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

    '   Total Lines: 142
    '    Code Lines: 101 (71.13%)
    ' Comment Lines: 14 (9.86%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 27 (19.01%)
    '     File Size: 4.54 KB


    '     Class DistanceMap
    ' 
    '         Properties: MinimalDistance
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Add, Dequeue, FindByCodePair, Remove, RemoveFirst
    '                   ToList, ToString
    ' 
    '         Sub: Enqueue, Sort
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language

Namespace Hierarchy

    ''' <summary>
    ''' Container for linkages
    ''' with the minimal methods needed in the package
    ''' Created by Alexandre Masselot on 7/18/14.
    ''' </summary>
    Public Class DistanceMap

        Dim linkTable As New Dictionary(Of String, HierarchyLink)
        Dim data As New List(Of HierarchyLink)

        ''' <summary>
        ''' Peak into the minimum distance
        ''' @return
        ''' </summary>
        Public ReadOnly Property MinimalDistance() As Double
            Get
                ' data.Peek()
                Dim peek As HierarchyLink = data(Scan0)

                If peek IsNot Nothing Then
                    Return peek.Tree.LinkageDistance
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(links As IEnumerable(Of HierarchyTreeNode))
            For Each x As HierarchyTreeNode In links
                Dim link As New HierarchyLink(x)

                If Not linkTable.ContainsKey(link.HashKey) Then
                    Call linkTable.Add(link.HashKey, link)
                End If
            Next

            data = New List(Of HierarchyLink)(linkTable.Values)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub Enqueue(queueItem As HierarchyLink)
            Call data.Add(queueItem)
            Call data.Sort()
        End Sub

        ''' <summary>
        ''' Poll
        ''' </summary>
        ''' <returns></returns>
        Private Function Dequeue() As HierarchyLink
            Dim frontItem As HierarchyLink = data(Scan0)
            data.RemoveAt(index:=Scan0)
            Return frontItem
        End Function

        Public Function ToList() As IList(Of HierarchyTreeNode)
            Dim l As New List(Of HierarchyTreeNode)

            For Each clusterPair As HierarchyLink In data
                l.Add(clusterPair.Tree)
            Next

            Return l
        End Function

        ''' <summary>
        ''' dictionary hash search for the link
        ''' </summary>
        ''' <param name="c1"></param>
        ''' <param name="c2"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindByCodePair(c1 As Cluster, c2 As Cluster) As HierarchyTreeNode
            Return linkTable(hashCodePair(c1, c2)).Tree
        End Function

        Public Function RemoveFirst() As HierarchyTreeNode
            Dim poll As HierarchyLink = Dequeue()

            Do While poll IsNot Nothing AndAlso poll.removed
                poll = Dequeue()
            Loop

            If poll Is Nothing Then
                Return Nothing
            Else
                With poll.Tree
                    Call linkTable.Remove(poll.HashKey)
                    Return .ByRef
                End With
            End If
        End Function

        Public Function Remove(pending As IEnumerable(Of HierarchyTreeNode)) As Boolean
            For Each i As HierarchyTreeNode In pending
                Call Remove(i)
            Next

            Return True
        End Function

        Public Function Remove(link As HierarchyTreeNode) As Boolean
            Dim removed As HierarchyLink = linkTable.RemoveAndGet(hashCodePair(link))

            If removed Is Nothing Then
                Return False
            Else
                removed.removed = True
                data.Remove(removed)
            End If

            Return True
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Sort()
            Call data.Sort()
        End Sub

        Public Function Add(link As HierarchyTreeNode, Optional direct As Boolean = False) As Boolean
            Dim hlink As New HierarchyLink(link)

            If linkTable.ContainsKey(hlink.HashKey) Then
#If DEBUG Then
                Dim existingItem As HierarchyLink = linkTable(hlink.HashKey)

                Call Console _
                    .Error _
                    .WriteLine("hashCode = " & existingItem.HashKey & " adding redundant link:" & link.ToString & " (exist:" & existingItem.ToString & ")")
#End If
                Return False
            Else
                Call linkTable.Add(hlink.HashKey, hlink)

                If Not direct Then
                    Call Enqueue(hlink)
                Else
                    Call data.Add(hlink)
                End If

                Return True
            End If
        End Function

        Public Overloads Function ToString() As String
            Return $"Have {linkTable.Count} linkage with minimal distance {MinimalDistance}"
        End Function
    End Class
End Namespace
